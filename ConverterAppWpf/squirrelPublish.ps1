$version = $args[0]
$squirrelDir = "squirrel"
dotnet publish `
    --configuration Release `
    --framework net5.0-windows `
    --runtime win10-x64 `
    --self-contained true `
    --output published-app `
    -p:PublishSingleFile=true &&
rmd $squirrelDir &&
mkdir $squirrelDir/lib/net45 -Force &&
Copy-Item published-app/* $squirrelDir/lib/net45 -Exclude *.pdb, *.nupkg, *.vshost &&
octo pack `
    --id ExcelConverter `
    --version $version `
    --basePath $squirrelDir `
    --title "Konwerter Excela" `
    --author "Bartek Luka" `
    --description "Konwerter plików excelowych" `
    --overwrite &&
~\.nuget\packages\clowd.squirrel\*\tools\Squirrel.com --releasify .\ExcelConverter.$version.nupkg --selfContained