# QuickWheel 微软商店发布脚本
# 使用说明：
# 1. 确保已安装 Visual Studio 2022 或 MSBuild
# 2. 确保已配置 Windows SDK
# 3. 运行此脚本前，请更新 Package.appxmanifest 中的版本号和发布者信息

param(
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter()]
    [string]$Version = "1.0.0.0"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "QuickWheel 微软商店发布工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 检查 MSBuild
$msbuildPath = "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
if (-not (Test-Path $msbuildPath)) {
    $msbuildPath = "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
}
if (-not (Test-Path $msbuildPath)) {
    $msbuildPath = "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
}

if (-not (Test-Path $msbuildPath)) {
    Write-Error "未找到 MSBuild。请安装 Visual Studio 2022。"
    exit 1
}

Write-Host "找到 MSBuild: $msbuildPath" -ForegroundColor Green

# 设置路径
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionPath = Join-Path $scriptPath "QuickWheel.sln"
$packagePath = Join-Path $scriptPath "Package"
$outputPath = Join-Path $scriptPath "Publish"

# 清理旧输出
if (Test-Path $outputPath) {
    Write-Host "清理旧输出目录..." -ForegroundColor Yellow
    Remove-Item -Path $outputPath -Recurse -Force
}

New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

# 还原 NuGet 包
Write-Host ""
Write-Host "还原 NuGet 包..." -ForegroundColor Cyan
dotnet restore $solutionPath
if ($LASTEXITCODE -ne 0) {
    Write-Error "NuGet 包还原失败"
    exit 1
}

# 构建解决方案
Write-Host ""
Write-Host "构建解决方案 ($Configuration)..." -ForegroundColor Cyan
& $msbuildPath $solutionPath `
    /p:Configuration=$Configuration `
    /p:Platform=x64 `
    /p:UapAppxPackageBuildMode=StoreUpload `
    /p:AppxBundle=Always `
    /p:AppxBundlePlatforms=x64 `
    /p:AppxPackageDir=$outputPath `
    /t:Rebuild

if ($LASTEXITCODE -ne 0) {
    Write-Error "构建失败"
    exit 1
}

# 查找生成的 MSIX 包
$msixFiles = Get-ChildItem -Path $outputPath -Filter "*.msix" -Recurse
$appxUploadFiles = Get-ChildItem -Path $outputPath -Filter "*.appxupload" -Recurse

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "构建成功！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

if ($msixFiles) {
    Write-Host ""
    Write-Host "生成的 MSIX 包：" -ForegroundColor Cyan
    $msixFiles | ForEach-Object { 
        Write-Host "  - $($_.FullName)" -ForegroundColor White
    }
}

if ($appxUploadFiles) {
    Write-Host ""
    Write-Host "生成的商店上传包：" -ForegroundColor Cyan
    $appxUploadFiles | ForEach-Object { 
        Write-Host "  - $($_.FullName)" -ForegroundColor White
    }
}

Write-Host ""
Write-Host "发布步骤：" -ForegroundColor Yellow
Write-Host "1. 登录 Microsoft Partner Center" -ForegroundColor White
Write-Host "2. 选择你的应用" -ForegroundColor White
Write-Host "3. 导航到 '包' 部分" -ForegroundColor White
Write-Host "4. 上传 .appxupload 文件" -ForegroundColor White
Write-Host "5. 提交审核" -ForegroundColor White
Write-Host ""
Write-Host "注意：首次发布前请确保：" -ForegroundColor Yellow
Write-Host "- 已更新 Package.appxmanifest 中的发布者信息" -ForegroundColor White
Write-Host "- 已准备好商店截图（1366x768 或 1920x1080）" -ForegroundColor White
Write-Host "- 已填写应用描述和元数据" -ForegroundColor White
Write-Host "- 已配置隐私政策 URL" -ForegroundColor White
Write-Host ""
