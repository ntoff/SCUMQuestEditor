$inputFile = "EliminationWeapons.txt"
$outputFile = "EliminationWeapons.json"

# Read all lines, remove empty lines
$items = Get-Content -Path $inputFile | Where-Object { $_.Trim() -ne "" }

# JSON array builder
$jsonParts = @()
$currentLine = ""
$maxLength = 120
$separator = ", "

foreach ($item in $items) {
    # Escape quotes and backslashes for valid JSON string
    $escapedItem = $item -replace '\\', '\\' -replace '"', '\"'
    $potentialLine = if ($currentLine -eq "") { "`"$escapedItem`"" } else { "$currentLine$separator`"$escapedItem`"" }
    
    if ($potentialLine.Length -gt $maxLength) {
        if ($currentLine -ne "") {
            $jsonParts += $currentLine
        }
        $currentLine = "`"$escapedItem`""
    } else {
        $currentLine = $potentialLine
    }
}

# Add the last line
if ($currentLine -ne "") {
    $jsonParts += $currentLine
}

# Wrap in array brackets and format
$finalJson = "[" + [Environment]::NewLine + ($jsonParts -join "," + [Environment]::NewLine) + [Environment]::NewLine + "]"

Set-Content -Path $outputFile -Value $finalJson
