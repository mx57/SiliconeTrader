@ECHO OFF
ECHO Cleaning old publish directory...
IF EXIST "Publish/bin" (
    RMDIR /s /q "Publish/bin"
)
MKDIR "Publish/bin"

ECHO Publishing SiliconeTrader.Machine...
dotnet publish "SiliconeTrader.Machine/SiliconeTrader.Machine.csproj" -c Release -o "Publish/bin/SiliconeTrader.Machine" --nologo

ECHO Publishing SiliconeTrader.UI...
dotnet publish "SiliconeTrader.UI/SiliconeTrader.UI.csproj" -c Release -o "Publish/bin/SiliconeTrader.UI" --nologo

ECHO All done!
PAUSE