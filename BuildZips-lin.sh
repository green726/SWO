zip -r -j ./HIP-win10-x64.zip ./HIP
zip -r -j ./HIP-linux-x64.zip ./HIP
cd ./Language
dotnet publish -r win10-x64
zip -r -j ../Language-win10-x64.zip ./bin/Debug/net6.0/win10-x64/publish
dotnet publish -r linux-x64
zip -r -j ../Language-linux-x64.zip ./bin/Debug/net6.0/linux-x64/publish
cd ..
zip -r -j ./Resources.zip ./Resources
