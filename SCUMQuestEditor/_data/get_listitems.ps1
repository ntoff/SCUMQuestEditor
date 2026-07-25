$logPath = "C:\Users\Norah\AppData\Local\SCUM\Saved\Logs\SCUM.log"
$outputPath = "C:\Users\Norah\source\repos\SCUMQuestEditor\SCUMQuestEditor\_data\listitems.txt"

$items = @()
$inList = $false

Get-Content $logPath | ForEach-Object {
    if ($_ -match "START OF 'Item' LIST") {
        $inList = $true
        return
    }
    if ($_ -match "END OF 'Item' LIST") {
        $inList = $false
        return
    }

    if ($inList) {
        # Match: Sender player state null for message <Item>!
        if ($_ -match "Sender player state null for message (.+)!") {
            $items += $matches[1]
        }
        # Match: LogSCUM:     <Item>
        elseif ($_ -match "LogSCUM:\s+(.+)") {
            $items += $matches[1].Trim()
        }
    }
}

$cleanItems = $items | Sort-Object -Unique

if ($outputPath) {
    $cleanItems | Set-Content $outputPath -Encoding UTF8
    Write-Host "`nSaved to: $outputPath" -ForegroundColor Green
}
