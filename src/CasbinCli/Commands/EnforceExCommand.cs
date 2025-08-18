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

using CasbinCli.Services;  
using System.CommandLine;  
  
namespace CasbinCli.Commands  
{  
    public static class EnforceExCommand  
    {  
        public static Command Create()  
        {  
            var command = new Command("enforceEx", "Test if a 'subject' can access a 'object' with a given 'action' based on the policy with explanation.")  
            {  
                Description = @"  
                    The enforceEx command evaluates access control policies and provides detailed explanation of the decision.  
  
                    Examples:  
                        casbin enforceEx -m model.conf -p policy.csv alice data1 read  
                        casbin enforceEx -m rbac_model.conf -p rbac_policy.csv alice domain1 data1 read  
                        casbin enforceEx -m abac_model.conf -p abac_policy.csv '{""Age"":30}' /data1 read  
  
                    The command supports the same access control models as enforce but returns additional explanation:  
                        - When access is allowed: returns the matching policy rule details  
                        - When access is denied: returns empty explanation array  
  
                    Output format: {""allow"": boolean, ""explain"": string[]}  
            "  
            };  
  
            var modelOption = new Option<string>(  
                aliases: new[] { "--model", "-m" },  
                description: "Path to the model configuration file. The model defines the access control model (ACL, RBAC, ABAC, etc.)")  
            {  
                IsRequired = true  
            };  
  
            var policyOption = new Option<string>(  
                aliases: new[] { "--policy", "-p" },  
                description: "Path to the policy data file. Contains the actual policy rules in CSV format.")  
            {  
                IsRequired = true  
            };  
  
            var argumentsArgument = new Argument<string[]>(  
                name: "arguments",  
                description: @"The enforcement arguments (subject, object, action, etc.)  
                    For basic ACL: subject object action  
                    For RBAC: subject object action  
                    For RBAC with domains: subject domain object action  
                    For ABAC: {""attribute"":""value""} object action")  
            {  
                Arity = ArgumentArity.ZeroOrMore  
            };  
  
            command.AddOption(modelOption);  
            command.AddOption(policyOption);  
            command.AddArgument(argumentsArgument);  
  
            command.SetHandler(async (string modelPath, string policyPath, string[] arguments) =>  
            {  
                var service = new EnforcementService();  
                var result = await service.ExecuteEnforceAsync(modelPath, policyPath, arguments, true);  
                service.OutputResult(result);  
            }, modelOption, policyOption, argumentsArgument);  
  
            return command;  
        }  
    }  
}