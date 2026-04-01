# CSS (Command-Line Spreadsheet) Build Script
# このスクリプトは自己完結型の実行ファイルをビルドします

Write-Host "Building CSS (Command-Line Spreadsheet)..." -ForegroundColor Green
Write-Host ""

# プロジェクトディレクトリに移動
$projectDir = Join-Path $PSScriptRoot "CssApp"
Set-Location $projectDir

# クリーンビルド
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path "bin") {
    Remove-Item -Path "bin" -Recurse -Force
}
if (Test-Path "obj") {
    Remove-Item -Path "obj" -Recurse -Force
}

# ビルド
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# 発行（自己完結型）
Write-Host "Publishing self-contained executable..." -ForegroundColor Yellow
$publishDir = Join-Path $PSScriptRoot "publish"

if (Test-Path $publishDir) {
    Remove-Item -Path $publishDir -Recurse -Force
}

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Executable location: $publishDir\css.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run the application:" -ForegroundColor Yellow
Write-Host "  cd $publishDir" -ForegroundColor White
Write-Host "  .\css.exe" -ForegroundColor White
Write-Host ""
Write-Host "To open a CSV file:" -ForegroundColor Yellow
Write-Host "  .\css.exe data.csv" -ForegroundColor White
Write-Host ""

# ファイルサイズを表示
$exePath = Join-Path $publishDir "css.exe"
if (Test-Path $exePath) {
    $fileSize = (Get-Item $exePath).Length / 1MB
    Write-Host "Executable size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Cyan
}

# Made with Bob
