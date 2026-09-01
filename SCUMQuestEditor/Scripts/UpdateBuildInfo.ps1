$date = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$projectDir = Split-Path $PSScriptRoot -Parent
$version = [System.IO.File]::ReadAllText((Join-Path $projectDir "Version.txt")).Trim()
$quote = [char]34
$cs = "namespace SCUMQuestEditor`r`n{`r`n    public static class BuildInfo`r`n    {`r`n        public const string BuildDate = $quote$date$quote;`r`n        public const string Version = $quote$version$quote;`r`n    }`r`n}"
[System.IO.File]::WriteAllText((Join-Path $projectDir "BuildInfo.cs"), $cs, [System.Text.Encoding]::UTF8)
