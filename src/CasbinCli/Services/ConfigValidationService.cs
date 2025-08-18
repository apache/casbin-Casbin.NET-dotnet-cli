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

using System.Text.RegularExpressions;  
  
namespace CasbinCli.Services  
{  
    public class ConfigValidationService  
    {  
        private static readonly Regex SectionRegex = new(@"^\[(\w+)\]$", RegexOptions.Compiled);  
        private static readonly Regex KeyValueRegex = new(@"^(\w+)\s*=\s*(.+)$", RegexOptions.Compiled);  
  
        public static ValidationResult ValidateModelFile(string filePath)  
        {  
            try  
            {  
                if (!File.Exists(filePath))  
                {  
                    return new ValidationResult(false, $"Model file not found: {filePath}");  
                }  
  
                var lines = File.ReadAllLines(filePath);  
                var requiredSections = new HashSet<string> { "request_definition", "policy_definition", "policy_effect", "matchers" };  
                var foundSections = new HashSet<string>();  
  
                foreach (var line in lines)  
                {  
                    var trimmedLine = line.Trim();  
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))  
                        continue;  
  
                    var sectionMatch = SectionRegex.Match(trimmedLine);  
                    if (sectionMatch.Success)  
                    {  
                        foundSections.Add(sectionMatch.Groups[1].Value);  
                    }  
                }  
  
                var missingSections = requiredSections.Except(foundSections);  
                if (missingSections.Any())  
                {  
                    return new ValidationResult(false, $"Missing required sections: {string.Join(", ", missingSections)}");  
                }  
  
                return new ValidationResult(true, "Model file is valid");  
            }  
            catch (Exception ex)  
            {  
                return new ValidationResult(false, $"Error validating model file: {ex.Message}");  
            }  
        }  
  
        public static ValidationResult ValidatePolicyFile(string filePath)  
        {  
            try  
            {  
                if (!File.Exists(filePath))  
                {  
                    return new ValidationResult(false, $"Policy file not found: {filePath}");  
                }  
  
                var lines = File.ReadAllLines(filePath);  
                var validLines = 0;  
  
                foreach (var line in lines)  
                {  
                    var trimmedLine = line.Trim();  
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))  
                        continue;  
  
                    var parts = trimmedLine.Split(',').Select(p => p.Trim()).ToArray();  
                    if (parts.Length < 2)  
                    {  
                        return new ValidationResult(false, $"Invalid policy line: {trimmedLine}");  
                    }  
  
                    validLines++;  
                }  
  
                if (validLines == 0)  
                {  
                    return new ValidationResult(false, "Policy file contains no valid policy rules");  
                }  
  
                return new ValidationResult(true, $"Policy file is valid ({validLines} rules found)");  
            }  
            catch (Exception ex)  
            {  
                return new ValidationResult(false, $"Error validating policy file: {ex.Message}");  
            }  
        }  
    }  
  
    public record ValidationResult(bool IsValid, string Message);  
}