@REM cd ./HIP
@REM dotnet publish -r win10-x64
@REM zip -r ../HIP-win10-x64.zip ./bin
tar -a -c -f ./HIP-win10-x64.zip ./HIP
cd ./Language
dotnet publish -r win10-x64
tar -a -c -f ../Language-win10-x64.zip ./bin