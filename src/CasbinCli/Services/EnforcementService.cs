// Copyright 2025 The casbin Authors. All Rights Reserved.  
//  
// Licensed under the Apache License, Version 2.0 (the "License");  
// you may not use this file except in compliance with the License.  
// You may obtain a copy of the License at  
//  
//      http://www.apache.org/licenses/LICENSE-2.0  
//  
// Unless required by applicable law or agreed to in writing, software  
// distributed under the License is distributed on an "AS IS" BASIS,  
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  
// See the License for the specific language governing permissions and  
// limitations under the License.  
  
using NetCasbin;  
using CasbinCli.Models;  
using Newtonsoft.Json;  
using System.Text.RegularExpressions;  
using System.Reflection;  
using System.Reflection.Emit;  
  
namespace CasbinCli.Services  
{  
    public class EnforcementService  
    {  
        private static readonly Regex ParamRegex = new(@"{\s*""?(\w+)""?\s*:\s*(?:""?([^""{}]+)""?)\s*}", RegexOptions.Compiled);  
  
        public async Task<ResponseBody> ExecuteEnforceAsync(string modelPath, string policyPath, string[] args, bool isEnforceEx)  
        {  
            try  
            {  
                // Verify the existence of the file  
                if (!File.Exists(modelPath))  
                {  
                    var error = $"Model file not found: {modelPath}";  
                    Console.Error.WriteLine($"Error during enforcement: {error}");  
                    return new ResponseBody { Allow = false, Explain = Array.Empty<string>() };  
                }  
  
                if (!File.Exists(policyPath))  
                {  
                    var error = $"Policy file not found: {policyPath}";  
                    Console.Error.WriteLine($"Error during enforcement: {error}");  
                    return new ResponseBody { Allow = false, Explain = Array.Empty<string>() };  
                }  
  
                // Verify the format of the configuration file  
                var modelValidation = ConfigValidationService.ValidateModelFile(modelPath);  
                if (!modelValidation.IsValid)  
                {  
                    Console.Error.WriteLine($"Error during enforcement: {modelValidation.Message}");  
                    return new ResponseBody { Allow = false, Explain = Array.Empty<string>() };  
                }  
  
                var policyValidation = ConfigValidationService.ValidatePolicyFile(policyPath);  
                if (!policyValidation.IsValid)  
                {  
                    Console.Error.WriteLine($"Error during enforcement: {policyValidation.Message}");  
                    return new ResponseBody { Allow = false, Explain = Array.Empty<string>() };  
                }  
  
                // Verify parameters  
                if (args == null || args.Length == 0)  
                {  
                    var error = "No enforcement parameters provided";  
                    Console.Error.WriteLine($"Error during enforcement: {error}");  
                    return new ResponseBody { Allow = false, Explain = Array.Empty<string>() };  
                }  
  
                var enforcer = new Enforcer(modelPath, policyPath);  
                var parameters = ProcessParameters(args);  
  
                if (isEnforceEx)  
                {  
                    bool result = await Task.Run(() => enforcer.Enforce(parameters));  
                      
                    // Enhanced generation of explanatory information  
                    string[] explain = GenerateExplanation(result, parameters, enforcer);  
                      
                    return new ResponseBody  
                    {  
                        Allow = result,  
                        Explain = explain  
                    };  
                }  
                else  
                {  
                    bool result = await Task.Run(() => enforcer.Enforce(parameters));  
                    return new ResponseBody  
                    {  
                        Allow = result,  
                        Explain = Array.Empty<string>()  
                    };  
                }  
            }  
            catch (FileNotFoundException ex)  
            {  
                Console.Error.WriteLine($"Error during enforcement: File not found - {ex.Message}");  
                return new ResponseBody { Allow = false, Explain = Array.Empty<string>() };  
            }  
            catch (UnauthorizedAccessException ex)  
            {  
                Console.Error.WriteLine($"Error during enforcement: Access denied - {ex.Message}");  
                return new ResponseBody { Allow = false, Explain = Array.Empty<string>() };  
            }  
            catch (Exception ex)  
            {  
                Console.Error.WriteLine($"Error during enforcement: {ex.Message}");  
                return new ResponseBody { Allow = false, Explain = Array.Empty<string>() };  
            }  
        }  
  
        private string[] GenerateExplanation(bool result, object[] parameters, Enforcer enforcer)  
        {  
            if (!result)  
                return Array.Empty<string>();  
  
            try  
            {  
                // Try to obtain the matching policy rules  
                var policies = enforcer.GetPolicy();  
                var roles = enforcer.GetRolesForUser(parameters[0]?.ToString() ?? "");  
                  
                if (roles.Any())  
                {  
                    // If there is a role, return the role information  
                    return new[] { roles.First(), parameters.Skip(1).FirstOrDefault()?.ToString() ?? "", parameters.Skip(2).FirstOrDefault()?.ToString() ?? "" };  
                }  
                else  
                {  
                    // Return the parameters that directly match  
                    return parameters.Take(3).Select(p => p?.ToString() ?? "").ToArray();  
                }  
            }  
            catch  
            {  
                // If the explanation fails to be obtained, return a simple success message  
                return new[] { "Policy matched" };  
            }  
        }  
  
        private object[] ProcessParameters(string[] args)  
        {  
            var parameters = new object[args.Length];  
  
            for (int i = 0; i < args.Length; i++)  
            {  
                var match = ParamRegex.Match(args[i]);  
                if (match.Success)  
                {  
                    var fieldName = match.Groups[1].Value;  
                    var valueStr = match.Groups[2].Value;  
  
                    if (int.TryParse(valueStr, out int intValue))  
                    {  
                        parameters[i] = CreateStructWithValue(fieldName, intValue);  
                    }  
                    else  
                    {  
                        parameters[i] = CreateStructWithValue(fieldName, valueStr);  
                    }  
                }  
                else  
                {  
                    parameters[i] = args[i];  
                }  
            }  
  
            return parameters;  
        }  
  
        private object CreateStructWithValue(string fieldName, object value)  
        {  
            
            var capitalizedFieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);  
              
            
            var assemblyName = new AssemblyName($"DynamicAssembly_{Guid.NewGuid():N}");  
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);  
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");  
            var typeBuilder = moduleBuilder.DefineType($"DynamicType_{Guid.NewGuid():N}", TypeAttributes.Public);  
  
             
            var fieldBuilder = typeBuilder.DefineField(capitalizedFieldName, value.GetType(), FieldAttributes.Public);  
  
            
            var dynamicType = typeBuilder.CreateType();  
              
            
            var instance = Activator.CreateInstance(dynamicType);  
            var field = dynamicType.GetField(capitalizedFieldName);  
            field?.SetValue(instance, value);  
  
            return instance;  
        }  
  
        public void OutputResult(ResponseBody response)  
        {  
            var json = JsonConvert.SerializeObject(response, new JsonSerializerSettings  
            {  
                Formatting = Formatting.None,  
                NullValueHandling = NullValueHandling.Include  
            });  
            Console.WriteLine(json);  
        }  
    }  
}