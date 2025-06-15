#!/bin/bash

echo "SiliconeTrader Key Encryption Script"
echo "This script will encrypt your API keys for use with SiliconeTrader."
echo "The encrypted file is typically only valid for the current user on this computer (if using Windows DPAPI via .NET)."
echo ""

read -p "Enter the path for the output encrypted file (e.g., keys.bin): " OUTPUT_PATH
read -p "Enter your public API key: " PUBLIC_KEY
read -s -p "Enter your private API key: " PRIVATE_KEY
echo "" # for newline after secret input

# Assuming the script is run from the 'SiliconeTrader.Keys/scripts' directory
KEY_TOOL_PATH="../bin/Release/net8.0/SiliconeTrader.Keys.dll"

if [ ! -f "$KEY_TOOL_PATH" ]; then
    echo "ERROR: SiliconeTrader.Keys.dll not found at $KEY_TOOL_PATH"
    echo "Please ensure the SiliconeTrader.Keys project has been built in Release mode."
    exit 1
fi

echo "Encrypting keys..."
dotnet "$KEY_TOOL_PATH" --encrypt --path="$OUTPUT_PATH" --publickey="$PUBLIC_KEY" --privatekey="$PRIVATE_KEY"

echo ""
echo "Script finished. Check the output above for success or errors."