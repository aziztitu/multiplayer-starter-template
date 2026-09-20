$ErrorActionPreference = "Stop"

$toolsDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Split-Path -Parent $toolsDir
$example = Join-Path $toolsDir ".gitattributes.example"
$dest = Join-Path $root ".gitattributes"

if (-not (Test-Path $example)) {
    Write-Error "Missing $example"
}

Set-Location $root

git lfs version | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Git LFS is not installed. Install it from https://git-lfs.com then re-run this script."
}

git lfs install
if ($LASTEXITCODE -ne 0) {
    Write-Error "git lfs install failed."
}

Copy-Item -Path $example -Destination $dest -Force

Write-Host "Git LFS is installed for this repo."
Write-Host "Copied tools/.gitattributes.example to .gitattributes"
Write-Host "Commit .gitattributes. New matching files (fbx, png, wav, …) go to LFS."
Write-Host "Files already in Git stay as normal objects unless you run git lfs migrate."
