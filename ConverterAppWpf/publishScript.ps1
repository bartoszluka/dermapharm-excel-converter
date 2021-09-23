dotnet publish -c Release -f net5.0-windows -r win10-x64 --self-contained true -o Published -p:PublishSingleFile=true
cp Publish.wxs Published
cd Published
candle Publish.wxs
light Publish.wixobj