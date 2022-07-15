@REM cd ./HIP
@REM dotnet publish -r win10-x64
@REM zip -r ../HIP-win10-x64.zip ./bin
tar -a -c -f ./HIP-win10-x64.zip ./HIP
cd ./Language
dotnet publish -r win10-x64
7z a -tzip ../Language-win10-x64.zip ./bin/Debug/net6.0/win10-x64/publish/*
cd ..