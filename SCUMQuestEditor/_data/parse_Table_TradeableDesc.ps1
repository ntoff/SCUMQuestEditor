$inputFile = 'Table_TradeableDesc.json'
$outputFile = 'TradeItems.json'

$data = Get-Content $inputFile -Raw | ConvertFrom-Json
$rows = $data[0].Rows.PSObject.Properties

$traderMap = @{}
foreach ($row in $rows) {
    $cleanName = if ($row.Name -match '_C$') { $row.Name -replace '_C$', '' } else { $row.Name }
    foreach ($traderType in $row.Value.TraderTypes) {
        $cleanTrader = $traderType -replace '^ETraderType::', ''
        if (-not $traderMap.ContainsKey($cleanTrader)) {
            $traderMap[$cleanTrader] = [System.Collections.Generic.List[string]]::new()
        }
        $traderMap[$cleanTrader].Add($cleanName)
    }
}

function Get-NaturalKey {
    param([string]$s)
    $parts = [regex]::Split($s, '(\d+)')
    $result = @()
    foreach ($part in $parts) {
        if ($part -match '^\d+$') { $result += $part.PadLeft(20, '0') }
        else { $result += $part.ToLower() }
    }
    return $result
}

$nl = [System.Environment]::NewLine
$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add('[')
$traderNames = $traderMap.Keys | Sort-Object

for ($ti = 0; $ti -lt $traderNames.Count; $ti++) {
    $traderName = $traderNames[$ti]
    $children = $traderMap[$traderName] | Sort-Object { Get-NaturalKey $_ }
    $isLastTrader = ($ti -eq ($traderNames.Count - 1))
    $comma = if ($isLastTrader) { '' } else { ',' }
    
    $lines.Add('    ' + [char]123)
    $lines.Add('        ' + [char]34 + 'TraderName' + [char]34 + ': ' + [char]34 + $traderName + [char]34 + ',')
    $lines.Add('        ' + [char]34 + 'Children' + [char]34 + ': [')
    
    for ($ci = 0; $ci -lt $children.Count; $ci++) {
        $isLastChild = ($ci -eq ($children.Count - 1))
        $c = if ($isLastChild) { '' } else { ',' }
        $lines.Add('            ' + [char]34 + $children[$ci] + [char]34 + $c)
    }
    
    $lines.Add('        ]')
    $lines.Add('    }' + $comma)
}
$lines.Add(']')

$output = $lines -join $nl
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($outputFile, $output, $utf8NoBom)
