# build.ps1 - Local build script for casbin-dotnet-cli  
param(  
    [string]$Configuration = "Release",  
    [switch]$Clean = $false  
)  
  
$platforms = @(  
    "win-x64",  
    "win-arm64",   
    "linux-x64",  
    "linux-arm64",  
    "osx-x64",  
    "osx-arm64"  
)  
  
Write-Host "Starting build process for casbin-dotnet-cli..." -ForegroundColor Cyan  
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow  
  
if ($Clean) {  
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow  
    if (Test-Path "dist") {  
        Remove-Item -Recurse -Force "dist"  
    }  
}  
  
New-Item -ItemType Directory -Force -Path "dist" | Out-Null  
  
foreach ($platform in $platforms) {  
    Write-Host "Building for $platform..." -ForegroundColor Green  
      
    $outputPath = "dist/$platform"  
      
    $publishArgs = @(  
        "publish"  
        "src/CasbinCli/CasbinCli.csproj"  
        "-c", $Configuration  
        "-r", $platform  
        "--self-contained", "true"  
        "-p:PublishSingleFile=true"  
        "-o", $outputPath  
        "--verbosity", "quiet"  
    )  
      
    & dotnet @publishArgs  
      
    if ($LASTEXITCODE -eq 0) {  
        Write-Host "Successfully built for $platform" -ForegroundColor Green  
          
        $exeName = if ($platform.StartsWith("win")) { "casbin.exe" } else { "casbin" }  
        $exePath = Join-Path $outputPath $exeName  
        if (Test-Path $exePath) {  
            $fileSize = [math]::Round((Get-Item $exePath).Length / 1MB, 2)  
            Write-Host "  Binary: $exePath ($fileSize MB)" -ForegroundColor Gray  
        }  
    } else {  
        Write-Host "Failed to build for $platform" -ForegroundColor Red  
    }  
}  
  
Write-Host "Build completed!" -ForegroundColor Cyan  
Write-Host "Binaries are available in the dist folder" -ForegroundColor Yellow  
  
Get-ChildItem -Path "dist" -Directory | ForEach-Object {  
    $platform = $_.Name  
    $exeName = if ($platform.StartsWith("win")) { "casbin.exe" } else { "casbin" }  
    $exePath = Join-Path $_.FullName $exeName  
    if (Test-Path $exePath) {  
        Write-Host "  $platform -> $exePath" -ForegroundColor Gray  
    }  
}  
  
Write-Host "Usage examples:" -ForegroundColor Yellow  
Write-Host "  Windows: .\dist\win-x64\casbin.exe --version" -ForegroundColor Gray  
Write-Host "  Linux:   ./dist/linux-x64/casbin --version" -ForegroundColor Gray  
Write-Host "  macOS:   ./dist/osx-x64/casbin --version" -ForegroundColor Gray