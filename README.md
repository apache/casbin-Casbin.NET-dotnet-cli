# casbin-dotnet-cli

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/) 
[![NuGet](https://img.shields.io/nuget/v/casbin-dotnet-cli.svg)](https://www.nuget.org/packages/casbin-dotnet-cli/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

casbin-dotnet-cli is a command-line tool based on Casbin.NET, enabling you to use all of Casbin APIs in the shell. 

## Project Structure

```
casbin-dotnet-cli/  
├── .github/  
│   └── workflows/  
│       └── build.yml                    # GitHub Actions workflow configuration  
├── src/  
│   └── CasbinCli/  
│       ├── Commands/  
│       │   ├── EnforceCommand.cs        # Enhanced enforce command implementation  
│       │   └── EnforceExCommand.cs      # Enhanced enforceEx command implementation  
│       ├── Models/  
│       │   └── ResponseBody.cs          # JSON response model  
│       ├── Services/  
│       │   ├── EnforcementService.cs    # Enhanced core enforcement service  
│       │   └── ConfigValidationService.cs # Configuration file validation service  
│       ├── Program.cs                   # Dynamic version management entry point  
│       └── CasbinCli.csproj            # Project file  
├── test/  
│   ├── CasbinCli.Tests/                # Unit test project  
│   │   ├── Services/  
│   │   │   ├── EnforcementServiceTests.cs    # Enhanced core service unit tests  
│   │   │   └── ParameterProcessingTests.cs   # Parameter processing tests  
│   │   ├── Models/  
│   │   │   └── ResponseBodyTests.cs          # Response model tests  
│   │   └── CasbinCli.Tests.csproj           # Test project file  
│   ├── basic_model.conf                # Basic access control model  
│   ├── basic_policy.csv                # Basic policy file  
│   ├── rbac_with_domains_model.conf    # RBAC with domains model  
│   ├── rbac_with_domains_policy.csv    # RBAC with domains policy  
│   ├── abac_rule_model.conf            # ABAC rule model file  
│   ├── abac_rule_policy.csv            # ABAC rule policy file  
│   ├── abac_model.conf                 # ABAC model file  
│   └── abac_policy.csv                 # ABAC policy file   
├── build.ps1                           # Local multi-platform build script  
├── .releaserc.json                     # Semantic release configuration  
├── README.md                           # Project documentation  
└── casbin-dotnet-cli.sln              # Visual Studio solution file  
```

## Installation

### Building from Source

1. Clone the repository

```
git clone https://github.com/casbin/casbin-dotnet-cli.git
```

1. Build the project

```
cd casbin-dotnet-cli  
dotnet build
```

1. Run the CLI

```
dotnet run --project src/CasbinCli -- [command] [options]
```

### Install as Global Tool

```
dotnet tool install -g casbin-dotnet-cli
```

After installation, you can use the `casbin` command directly.

## Command Options

| Option         | Description                                    | Required |
| -------------- | ---------------------------------------------- | -------- |
| `-m, --model`  | Path to the model file or model text           | Yes      |
| `-p, --policy` | Path to the policy file or policy text         | Yes      |
| `enforce`      | Check permissions                              | No       |
| `enforceEx`    | Check permissions and get matching policy rule | No       |

## Quick Start

### Basic Permission Check

Check if Alice has read permission on data1: README.md:37-42

```
casbin enforce -m "test/basic_model.conf" -p "test/basic_policy.csv" "alice" "data1" "read"
```

Output:

```
{"allow":true,"explain":[]}
```

### Permission Check with Explanation

Check if Alice has write permission on data1 (with explanation): README.md:44-49

```
casbin enforceEx -m "test/basic_model.conf" -p "test/basic_policy.csv" "alice" "data1" "write"
```

Output:

```
{"allow":false,"explain":[]}
```

### Domain-Based Permission Check

Check if Alice has read permission on data1 in domain1: README.md:51-56

```
casbin enforceEx -m "test/rbac_with_domains_model.conf" -p "test/rbac_with_domains_policy.csv" "alice" "domain1" "data1" "read"
```

Output:

```
{"allow":true,"explain":["admin","domain1","data1","read"]}
```

## Advanced Usage

### Attribute-Based Access Control (ABAC)

Support for structured parameters in ABAC scenarios: enforce_test.go:47-49

```
casbin enforceEx -m "test/abac_model.conf" -p "test/abac_policy.csv" "{\"Age\":30}" "/data1" "read"
```

Output:

```
{"allow":true,"explain":["r.sub.Age > 18","/data1","read"]}
```

### Supported Parameter Formats

- **Simple strings**: `"alice"`, `"data1"`, `"read"`
- **Structured objects**: `"{\"field\":\"value\"}"`, `"{\"Age\":25}"`

## Core Components

### EnforcementService

The core enforcement service responsible for:

- Initializing Casbin enforcer
- Processing parameter parsing (including JSON object parameters)
- Executing permission checks
- Formatting output results

### Commands

- **EnforceCommand**: Implements basic permission checking functionality
- **EnforceExCommand**: Implements permission checking with explanation functionality

### ResponseBody

Standardized JSON response format: enforce.go:29-30

```
{  
  "allow": boolean,    // whether access is allowed  
  "explain": string[]  // matching policy rules (enforceEx command only)  
}
```

## System Requirements

- .NET 8.0 or later
- Casbin.NET 1.3.9 or later

## Dependencies

- `Casbin.NET`: Core access control library
- `System.CommandLine`: Command-line parsing framework
- `Newtonsoft.Json`: JSON serialization library

## License

This project is licensed under the Apache 2.0 License - see the [LICENSE](https://deepwiki.com/search/LICENSE) file for details.