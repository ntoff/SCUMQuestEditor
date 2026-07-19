$date = Get-Date -Format "yyyyMMdd_HHmm"
$zipName = "SCUMQuestEditor_$date.zip"
$packagesDir = "..\packages"

if (!(Test-Path $packagesDir)) {
    New-Item -ItemType Directory -Path $packagesDir -Force | Out-Null
}

$publishDir = Join-Path (Get-Location) $args[0]
$zipPath = Join-Path (Get-Location) (Join-Path $packagesDir $zipName)

Compress-Archive -Path "$publishDir*" -DestinationPath $zipPath -Force
Write-Host "Created: $zipPath"
