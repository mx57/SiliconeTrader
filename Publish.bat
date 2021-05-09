RMDIR /s /q "Publish/bin"
dotnet publish -f netcoreapp2.1 -c Release /p:PublishProfile="SiliconeTrader/Properties/PublishProfiles/FolderProfile.pubxml" -o "../Publish/bin"
dotnet publish -f netcoreapp2.1 -c Release /p:PublishProfile="SiliconeTrader.Web/Properties/PublishProfiles/FolderProfile.pubxml" -o "../Publish/bin"
ECHO "All done!"
PAUSE