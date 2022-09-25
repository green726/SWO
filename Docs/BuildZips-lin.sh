cd ./Language
dotnet publish -r win10-x64
zip -r ../Builds/Language-win10-x64.zip ./bin/Debug/net6.0/win10-x64/publish
dotnet publish -r linux-x64
zip -r ../Builds/Language-linux-x64.zip ./bin/Debug/net6.0/linux-x64/publish
cd ..
zip -r ./Builds/Resources.zip ./Resources
