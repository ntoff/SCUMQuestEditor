@echo off
set DATE_STR=%date:~-4,4%-%date:~-10,2%-%date:~-7,2% %time:~0,2%:%time:~3,2%:%time:~6,2%
set DATE_STR=!DATE_STR: =0!
for /f "delims=" %%V in (Version.txt) do set VERSION=%%V
echo namespace SCUMQuestEditor > BuildInfo.cs
echo { >> BuildInfo.cs
echo     public static class BuildInfo >> BuildInfo.cs
echo     { >> BuildInfo.cs
echo         public const string BuildDate = "!!DATE_STR!!"; >> BuildInfo.cs
echo         public const string Version = "!VERSION!"; >> BuildInfo.cs
echo     } >> BuildInfo.cs
echo } >> BuildInfo.cs
