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

using CasbinCli.Commands;  
using System.CommandLine;  
using System.Reflection;  
  
namespace CasbinCli  
{  
    class Program  
    {  
        private static string GetVersion()  
        {  
            var assembly = Assembly.GetExecutingAssembly();  
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion  
                         ?? assembly.GetName().Version?.ToString()  
                         ?? "dev";  
            return version;  
        }  
  
        private static string GetCasbinVersion()  
        {  
            try  
            {  
                var casbinAssembly = Assembly.GetAssembly(typeof(NetCasbin.Enforcer));  
                return casbinAssembly?.GetName().Version?.ToString() ?? "unknown";  
            }  
            catch  
            {  
                return "unknown";  
            }  
        }  
  
        static async Task<int> Main(string[] args)  
        {  
            var rootCommand = new RootCommand("Casbin is a powerful and efficient open-source access control library for .NET projects. It provides support for enforcing authorization based on various access control models.");  
  
            // Add Command
            rootCommand.AddCommand(EnforceCommand.Create());  
            rootCommand.AddCommand(EnforceExCommand.Create());  
  
            // Check the version parameters  
            if (args.Length > 0 && (args[0] == "--version" || args[0] == "-v"))  
            {  
                Console.WriteLine($"casbin-dotnet-cli {GetVersion()}");  
                Console.WriteLine($"Casbin.NET {GetCasbinVersion()}");  
                return 0;  
            }  
  
            // If no parameters are provided, display help information
            if (args.Length == 0)  
            {  
                Console.WriteLine(rootCommand.Description);  
                Console.WriteLine("\nUse 'casbin --help' for more information.");  
                return 0;  
            }  
  
            return await rootCommand.InvokeAsync(args);  
        }  
    }  
}