dotnet publish -c Release -f net5.0-windows -r win10-x64 --self-contained true -o Published -p:PublishSingleFile=true
cp Publish.wxs Publish
cd Publish
candle Publish.wxs
light Publish.wixobj