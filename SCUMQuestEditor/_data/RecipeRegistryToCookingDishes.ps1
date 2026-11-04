# Extract-PrimaryAssetNames.ps1
# Extracts PrimaryAssetName from CookingRecipeRegistry.json,
# cleans up names (removes "Cook_" prefix, adds "Dish_" to drinks),
# sorts alphabetically, and writes unique names to a text file.

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$jsonFile = Join-Path $scriptDir "CookingRecipeRegistry.json"
$outputFile = Join-Path $scriptDir "cookingdishes.txt"

if (-not (Test-Path $jsonFile)) {
    Write-Error "JSON file not found: $jsonFile"
    exit 1
}

# Extract all PrimaryAssetName values (unique)
$json = Get-Content $jsonFile -Raw | ConvertFrom-Json
$entries = $json[0].Properties._entries
$names = $entries |
         Where-Object { $_ -and $_.AssetId -and $_.AssetId.PrimaryAssetName } |
         ForEach-Object { $_.AssetId.PrimaryAssetName } |
         Sort-Object -Unique

# Clean up names:
# 1) Remove "Cook_" prefix
# 2) If it then starts with "Drink", add "Dish_" prefix
$cleaned = @()
foreach ($name in $names) {
    # Remove "Cook_" prefix (case-insensitive)
    if ($name -ilike "Cook_*") {
        $name = $name.Substring("Cook_".Length)
    }
    # If it now starts with "Drink_", prepend "Dish_"
    if ($name -ilike "Drink_*") {
        $name = "Dish_" + $name
    }
    if ($name -ne "Empty" -and $name -ne "Pizza") {
        $cleaned += $name
    }
}

# Sort alphabetically and write output (no trailing newline or blank line)
$sorted = $cleaned | Sort-Object
$lines = $sorted -join "`r`n"
[System.IO.File]::WriteAllText($outputFile, $lines, [System.Text.Encoding]::UTF8)

Write-Host "Done! Wrote $($cleaned.Count) unique entries to:"
Write-Host "  $outputFile"
