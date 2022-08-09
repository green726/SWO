7z a -tzip ./SAP-win10-x64.zip ./SAP/*
7z a -tzip ./SAP-linux-x64.zip ./SAP/*
cd ./Language
dotnet publish -r win10-x64
7z a -tzip ../Language-win10-x64.zip ./bin/Debug/net6.0/win10-x64/publish/*
dotnet publish -r linux-x64
7z a -tzip ../Language-linux-x64.zip ./bin/Debug/net6.0/linux-x64/publish/*
cd ..
7z a -tzip ./Resources.zip ./Resources/*
