// Copyright 2025 The casbin Authors. All Rights Reserved.  
//  
// Licensed under the Apache License, Version 2.0 (the "License");  
// you may not use this file except in compliance with the License.  
// You may obtain a copy of the License at  
//  
//      http://www.apache.org/licenses/LICENSE/2.0  
//  
// Unless required by applicable law or agreed to in writing, software  
// distributed under the License is distributed on an "AS IS" BASIS,  
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  
// See the License for the specific language governing permissions and  
// limitations under the License.

using Casbin;
using CasbinCli.Models;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Dynamic;

namespace CasbinCli.Services
{
    // 自定义 DynamicObject 类，模拟 Casbin.NET 的 JsonValue
    public class CustomJsonValue : DynamicObject
    {
        private readonly JsonElement _element;

        public CustomJsonValue(string json) :
            this(JsonDocument.Parse(CommandExecutor.ConvertToJson(json)).RootElement)
        {
        }

        private CustomJsonValue(JsonElement element)
        {
            _element = element;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_element.ValueKind != JsonValueKind.Object)
            {
                result = null!;
                return false;
            }

            // 尝试直接匹配
            if (_element.TryGetProperty(binder.Name, out var value))
            {
                result = GetValue(value);
                return true;
            }

            // 尝试小写匹配（处理 Age -> age 的情况）
            var lowerName = binder.Name.ToLower();
            if (_element.TryGetProperty(lowerName, out var lowerValue))
            {
                result = GetValue(lowerValue);
                return true;
            }

            // 尝试首字母小写匹配（处理 Age -> age 的情况）
            var firstLowerName = char.ToLower(binder.Name[0]) + binder.Name.Substring(1);
            if (_element.TryGetProperty(firstLowerName, out var firstLowerValue))
            {
                result = GetValue(firstLowerValue);
                return true;
            }

            result = null!;
            return false;
        }

        public object this[int index] => GetValue(_element[index]);

        public override string ToString()
        {
            return _element.ToString();
        }

        private static object GetValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => new CustomJsonValue(element),
                JsonValueKind.Array => new CustomJsonValue(element),
                JsonValueKind.String => element.GetString()!,
                JsonValueKind.Number => element.GetInt32(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null!,
                JsonValueKind.Undefined => null!,
                _ => throw new InvalidOperationException(),
            };
        }
    }

    public class CommandExecutor
    {
        private readonly Enforcer _enforcer;
        private readonly string _inputMethodName;
        private readonly string[] _inputVal;

        public CommandExecutor(Enforcer enforcer, string inputMethodName, string[] inputVal)
        {
            _enforcer = enforcer;
            _inputMethodName = inputMethodName;
            _inputVal = inputVal;
        }

        /// <summary>
        /// 将字符串输入转换为JSON格式的字符串
        /// </summary>
        /// <param name="input">要转换为JSON格式的输入字符串，应该用花括号{}包围</param>
        /// <returns>表示输入字符串中键值对的JSON格式字符串</returns>
        public static string ConvertToJson(string input)
        {
            input = input.Trim();
            
            // 处理简单格式 {key: value}
            if (!input.Contains("\""))
            {
                // 移除花括号
                if (input.StartsWith("{") && input.EndsWith("}"))
                {
                    input = input.Substring(1, input.Length - 2).Trim();
                }
                
                var jsonBuilder = new System.Text.StringBuilder("{");
                var pairs = input.Split(',');
                
                foreach (var pair in pairs)
                {
                    var trimmedPair = pair.Trim();
                    var keyValue = trimmedPair.Split(':');
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim();
                        var value = keyValue[1].Trim();
                        
                        // 如果值是数字，不加引号；如果是字符串，加引号
                        if (int.TryParse(value, out _) || double.TryParse(value, out _))
                        {
                            jsonBuilder.Append($"\"{key}\":{value},");
                        }
                        else
                        {
                            jsonBuilder.Append($"\"{key}\":\"{value}\",");
                        }
                    }
                }
                
                if (jsonBuilder.Length > 1)
                {
                    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                }
                jsonBuilder.Append("}");
                return jsonBuilder.ToString();
            }

            return input;
        }

        public string OutputResult()
        {
            var clazz = _enforcer.GetType();
            var methods = clazz.GetMethods();

            var responseBody = new ResponseBody { Allow = false, Explain = Array.Empty<string>() };
            
            // 尝试直接调用Enforce方法，而不是使用反射
            if (_inputMethodName.Equals("Enforce", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // 处理JSON参数
                    var parameters = new object[_inputVal.Length];
                    for (int i = 0; i < _inputVal.Length; i++)
                    {
                        if (_inputVal[i].Trim().StartsWith("{"))
                        {
                            parameters[i] = new CustomJsonValue(_inputVal[i]);
                        }
                        else
                        {
                            parameters[i] = _inputVal[i];
                        }
                    }
                    
                    // 尝试不同的调用方式
                    bool result;
                    if (parameters.Length == 3 && parameters[0] is CustomJsonValue)
                    {
                        result = _enforcer.Enforce((CustomJsonValue)parameters[0], (string)parameters[1], (string)parameters[2]);
                    }
                    else if (parameters.Length == 3)
                    {
                        result = _enforcer.Enforce((string)parameters[0], (string)parameters[1], (string)parameters[2]);
                    }
                    else if (parameters.Length == 4)
                    {
                        result = _enforcer.Enforce((string)parameters[0], (string)parameters[1], (string)parameters[2], (string)parameters[3]);
                    }
                    else
                    {
                        result = _enforcer.Enforce(parameters);
                    }
                    responseBody.Allow = result;
                    responseBody.Explain = Array.Empty<string>();
                    _enforcer.SavePolicy();
                    return JsonConvert.SerializeObject(responseBody);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Direct Enforce call failed: {ex.Message}");
                }
            }
            else if (_inputMethodName.Equals("EnforceEx", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // 处理JSON参数
                    var parameters = new object[_inputVal.Length];
                    for (int i = 0; i < _inputVal.Length; i++)
                    {
                        if (_inputVal[i].Trim().StartsWith("{"))
                        {
                            parameters[i] = new CustomJsonValue(_inputVal[i]);
                        }
                        else
                        {
                            parameters[i] = _inputVal[i];
                        }
                    }
                    
                    //TODO: 需要优化
                    // 尝试不同的调用方式
                    var result = parameters.Length == 3 && parameters[0] is CustomJsonValue
                        ? _enforcer.EnforceEx((CustomJsonValue)parameters[0], (string)parameters[1], (string)parameters[2])
                        : parameters.Length == 3
                        ? _enforcer.EnforceEx((string)parameters[0], (string)parameters[1], (string)parameters[2])
                        : parameters.Length == 4
                        ? _enforcer.EnforceEx((string)parameters[0], (string)parameters[1], (string)parameters[2], (string)parameters[3])
                        : _enforcer.EnforceEx(parameters);
                        
                    responseBody.Allow = result.Result;
                    responseBody.Explain = result.Explains?.FirstOrDefault()?.ToArray() ?? Array.Empty<string>();
                    _enforcer.SavePolicy();
                    return JsonConvert.SerializeObject(responseBody);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Direct EnforceEx call failed: {ex.Message}");
                }
            }
            
            foreach (var method in methods)
            {
                var methodName = method.Name;
                if (methodName.Equals(_inputMethodName, StringComparison.OrdinalIgnoreCase) && 
                    !method.ContainsGenericParameters)
                {
                    var genericParameterTypes = method.GetParameters();
                    var convertedParams = new object[genericParameterTypes.Length];
                    var returnType = method.ReturnType;

                    // 处理特殊的方法签名
                    if (genericParameterTypes.Length == 3 && 
                        genericParameterTypes[0].ParameterType == typeof(string) &&
                        genericParameterTypes[1].ParameterType == typeof(List<string>) &&
                        genericParameterTypes[2].ParameterType == typeof(List<string>))
                    {
                        convertedParams[0] = _inputVal[0];
                        convertedParams[1] = _inputVal[1].Split(',').ToList();
                        convertedParams[2] = _inputVal[2].Split(',').ToList();
                    }
                    else if (genericParameterTypes.Length == 2 &&
                             genericParameterTypes[0].ParameterType == typeof(List<string>) &&
                             genericParameterTypes[1].ParameterType == typeof(List<string>))
                    {
                        convertedParams[0] = _inputVal[0].Split(',').ToList();
                        convertedParams[1] = _inputVal[1].Split(',').ToList();
                    }
                    else
                    {
                        // 通用参数转换
                        for (int i = 0; i < genericParameterTypes.Length; i++)
                        {
                            var paramType = genericParameterTypes[i].ParameterType;
                            
                            if (paramType == typeof(int))
                            {
                                convertedParams[i] = int.Parse(_inputVal[i]);
                            }
                            else if (paramType == typeof(string))
                            {
                                convertedParams[i] = _inputVal[i];
                            }
                            else if (paramType == typeof(object[]))
                            {
                                convertedParams[i] = SmartConvertValue(_inputVal.Skip(i).ToArray());
                            }
                            else if (paramType == typeof(string[]))
                            {
                                convertedParams[i] = _inputVal.Skip(i).ToArray();
                            }
                            else if (paramType == typeof(string[][]))
                            {
                                var arr = _inputVal.Skip(i).ToArray();
                                var res = new string[arr.Length][];
                                for (int j = 0; j < res.Length; j++)
                                {
                                    res[j] = arr[j].Split(',');
                                }
                                convertedParams[i] = res;
                            }
                            else if (paramType == typeof(List<string>))
                            {
                                var arr = _inputVal.Skip(i).ToArray();
                                convertedParams[i] = arr.ToList();
                            }
                            else if (paramType == typeof(List<List<string>>))
                            {
                                var res = new List<List<string>>();
                                var arr = _inputVal.Skip(i).ToArray();
                                foreach (var s in arr)
                                {
                                    var ans = s.Split(',').ToList();
                                    res.Add(ans);
                                }
                                convertedParams[i] = res;
                            }
                        }
                    }

                    // 处理JSON参数
                    var extraConvertedParams = new object[_inputVal.Length];
                    bool hasJson = false;
                    
                    try
                    {
                        for (int i = 0; i < _inputVal.Length; i++)
                        {
                            if (_inputVal[i].Trim().StartsWith("{"))
                            {
                                var jsonString = ConvertToJson(_inputVal[i]);
                                var objectMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                                extraConvertedParams[i] = objectMap ?? new Dictionary<string, object>();
                                hasJson = true;
                            }
                            else
                            {
                                extraConvertedParams[i] = _inputVal[i];
                            }
                        }
                    }
                    catch (Exception)
                    {
                        hasJson = false;
                    }

                    object? invoke;
                    if (hasJson)
                    {
                        invoke = method.Invoke(_enforcer, extraConvertedParams);
                    }
                    else
                    {
                        invoke = method.Invoke(_enforcer, convertedParams);
                    }

                    // 处理返回值
                    if (returnType == typeof(bool))
                    {
                        responseBody.Allow = (bool)invoke!;
                    }
                    else if (returnType == typeof(List<string>))
                    {
                        responseBody.Explain = ((List<string>)invoke!).ToArray();
                    }
                    else if (returnType.Name == "EnforceResult")
                    {
                        // 使用反射获取EnforceResult的属性
                        var allowProperty = returnType.GetProperty("Allow");
                        var explainProperty = returnType.GetProperty("Explain");
                        
                        if (allowProperty != null)
                        {
                            responseBody.Allow = (bool)allowProperty.GetValue(invoke)!;
                        }
                        
                        if (explainProperty != null)
                        {
                            var explainValue = explainProperty.GetValue(invoke);
                            if (explainValue is List<string> explainList)
                            {
                                responseBody.Explain = explainList.ToArray();
                            }
                        }
                    }

                    _enforcer.SavePolicy();
                    break;
                }
            }

            return JsonConvert.SerializeObject(responseBody);
        }

        private object SmartConvertValue(object value)
        {
            if (value is string[] values)
            {
                var convertedArray = new object[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    convertedArray[i] = SmartConvertValue(values[i]);
                }
                return convertedArray;
            }

            var strValue = ((string)value!).Trim();

            // 去掉引号
            if (strValue.StartsWith("\"") && strValue.EndsWith("\""))
            {
                return strValue.Substring(1, strValue.Length - 2);
            }

            // 整数
            if (Regex.IsMatch(strValue, @"^-?\d+$"))
            {
                return int.Parse(strValue);
            }

            // 浮点数
            if (Regex.IsMatch(strValue, @"^-?\d*\.\d+$"))
            {
                return double.Parse(strValue);
            }

            // 布尔值
            if (strValue.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if (strValue.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return strValue;
        }
    }
}
