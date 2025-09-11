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
  
using Casbin;  
using CasbinCli.Models;  
using Newtonsoft.Json;  
  
namespace CasbinCli.Services  
{
    public class EnforcementService  
    {  
  
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
                
                // 使用CommandExecutor进行反射调用
                var methodName = isEnforceEx ? "EnforceEx" : "Enforce";
                var executor = new CommandExecutor(enforcer, methodName, args);
                var jsonResult = await Task.Run(() => executor.OutputResult());
                
                return JsonConvert.DeserializeObject<ResponseBody>(jsonResult) ?? 
                       new ResponseBody { Allow = false, Explain = Array.Empty<string>() };
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