$date = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$quote = [char]34
$cs = "namespace SCUMQuestEditor`r`n{`r`n    public static class BuildInfo`r`n    {`r`n        public const string BuildDate = $quote$date$quote;`r`n    }`r`n}"
[System.IO.File]::WriteAllText("BuildInfo.cs", $cs, [System.Text.Encoding]::UTF8)
