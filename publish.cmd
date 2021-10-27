cd ./ImageCompress

dotnet publish -c Release /p:PublishSingleFile=true -f net6.0 --self-contained -r win-x64
dotnet publish -c Release /p:PublishSingleFile=true -f net6.0 --self-contained -r linux-x64
dotnet publish -c Release /p:PublishSingleFile=true -f net6.0 --self-contained -r osx-x64