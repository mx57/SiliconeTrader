@ECHO OFF
ECHO SiliconeTrader Key Encryption Script
ECHO This script will encrypt your API keys for use with SiliconeTrader.
ECHO The encrypted file is typically only valid for the current user on this computer.
ECHO.

SET /P OUTPUT_PATH="Enter the path for the output encrypted file (e.g., keys.bin): "
SET /P PUBLIC_KEY="Enter your public API key: "
SET /P PRIVATE_KEY="Enter your private API key: "
ECHO.

REM Assuming the script is run from the 'SiliconeTrader.Keys/scripts' directory
SET KEY_TOOL_PATH="..\bin\Release\net8.0\SiliconeTrader.Keys.dll"

IF NOT EXIST %KEY_TOOL_PATH% (
    ECHO ERROR: SiliconeTrader.Keys.dll not found at %KEY_TOOL_PATH%
    ECHO Please ensure the SiliconeTrader.Keys project has been built in Release mode.
    GOTO :EOF
)

ECHO Encrypting keys...
dotnet %KEY_TOOL_PATH% --encrypt --path="%OUTPUT_PATH%" --publickey="%PUBLIC_KEY%" --privatekey="%PRIVATE_KEY%"

ECHO.
ECHO Script finished. Check the output above for success or errors.
PAUSE
:EOF