@echo off
set DATE_STR=%date:~-4,4%-%date:~-10,2%-%date:~-7,2% %time:~0,2%:%time:~3,2%:%time:~6,2%
set DATE_STR=!DATE_STR: !0!
for /f "delims=" %%V in (%~dp0\..\Version.txt) do set VERSION=%%V
echo namespace SCUMQuestEditor > %~dp0..\BuildInfo.cs
echo { >> %~dp0..\BuildInfo.cs
echo     public static class BuildInfo >> %~dp0..\BuildInfo.cs
echo     { >> %~dp0..\BuildInfo.cs
echo         public const string BuildDate = "!!DATE_STR!!"; >> %~dp0..\BuildInfo.cs
echo         public const string Version = "!VERSION!"; >> %~dp0..\BuildInfo.cs
echo     } >> %~dp0..\BuildInfo.cs
echo } >> %~dp0..\BuildInfo.cs
