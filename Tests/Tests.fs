module Tests

open System
open ExcelConverter
open Xunit

let test convertFun dictFile excelFile correctList =
    let result = convertFun excelFile dictFile

    match result with
    | Ok list ->
        list
        |> List.map (fun (s: string) -> s.Split("\\") |> Array.last)
        |> List.sort
        |> (=) (List.sort correctList)
        |> Assert.True
    | Error _ -> Assert.True false

let testKakadu = test convertKakadu

let testBasic = test convert


[<Fact>]
let ``kakadu file with additional columns and empty columns at the end`` () =
    testKakadu
        "C:/Users/Bartek/source/repos/FSharp/ConverterApp/Tests/excel/CENNIK1.xlsx"
        "C:/Users/Bartek/source/repos/FSharp/ConverterApp/Tests/excel/kakadu1.xlsx"
        [ "AGO237.xlsx"
          "BBI258.xlsx"
          "BLU216.xlsx"
          "BON234.xlsx"
          "BOR203.xlsx"
          "DAB265.xlsx"
          "ECH245.xlsx"
          "ELK275.xlsx"
          "FUT213.xlsx"
          "GKR257.xlsx"
          "GLI246.xlsx"
          "HEL227.xlsx"
          "HET244.xlsx"
          "IKE223.xlsx"
          "JAN214.xlsx"
          "JED278.xlsx"
          "JUR232.xlsx"
          "KAS268.xlsx"
          "KOR260.xlsx"
          "KRM242.xlsx"
          "KRZ273.xlsx"
          "KUT281.xlsx"
          "LAZ255.xlsx"
          "LEA251.xlsx"
          "LPU264.xlsx"
          "LUD277.xlsx"
          "MAN212.xlsx"
          "MAR221.xlsx"
          "MOK215.xlsx"
          "NOS279.xlsx"
          "NPK261.xlsx"
          "OLA274.xlsx"
          "OLB280.xlsx"
          "OPS247.xlsx"
          "PCC248.xlsx"
          "PIA218.xlsx"
          "POR236.xlsx"
          "PPM256.xlsx"
          "PRO208.xlsx"
          "PUL269.xlsx"
          "RED220.xlsx"
          "REM226.xlsx"
          "SDL272.xlsx"
          "SKO217.xlsx"
          "STA235.xlsx"
          "SWI262.xlsx"
          "TEL239.xlsx"
          "TRW276.xlsx"
          "TUR263.xlsx"
          "ZAB254.xlsx"
          "ZAM271.xlsx"
          "ZPT210.xlsx" ]

[<Fact>]
let ``kakadu file with no noise`` () =
    testKakadu
        "C:/Users/Bartek/source/repos/FSharp/ConverterApp/Tests/excel/CENNIK2.xlsx"
        "C:/Users/Bartek/source/repos/FSharp/ConverterApp/Tests/excel/kakadu2.xlsx"
        [ "AGO237.xlsx"
          "BBI258.xlsx"
          "BLU216.xlsx"
          "BON234.xlsx"
          "BOR203.xlsx"
          "DAB265.xlsx"
          "ECH245.xlsx"
          "ELK275.xlsx"
          "FUT213.xlsx"
          "GKR257.xlsx"
          "GLI246.xlsx"
          "HEL227.xlsx"
          "HET244.xlsx"
          "IKE223.xlsx"
          "JAN214.xlsx"
          "JED278.xlsx"
          "JUR232.xlsx"
          "KAS268.xlsx"
          "KOR260.xlsx"
          "KRM242.xlsx"
          "KRZ273.xlsx"
          "LAZ255.xlsx"
          "LEA251.xlsx"
          "LPU264.xlsx"
          "LUD277.xlsx"
          "MAN212.xlsx"
          "MAR221.xlsx"
          "MOK215.xlsx"
          "NOS279.xlsx"
          "OLA274.xlsx"
          "OLB280.xlsx"
          "OLI231.xlsx"
          "OPS247.xlsx"
          "PCC248.xlsx"
          "PIA218.xlsx"
          "POR236.xlsx"
          "PPM256.xlsx"
          "PRO208.xlsx"
          "PUL269.xlsx"
          "RED220.xlsx"
          "REM226.xlsx"
          "SDL272.xlsx"
          "SKO217.xlsx"
          "STA235.xlsx"
          "SWI262.xlsx"
          "TRW276.xlsx"
          "TUR263.xlsx"
          "ZAB254.xlsx"
          "ZAM271.xlsx"
          "ZPT210.xlsx" ]

[<Fact>]
let ``kakadu file with additional columns and rows`` () =
    testKakadu
        "C:/Users/Bartek/source/repos/FSharp/ConverterApp/Tests/excel/CENNIK3.xlsx"
        "C:/Users/Bartek/source/repos/FSharp/ConverterApp/Tests/excel/kakadu3.xlsx"
        [ "AGO237.xlsx"
          "BBI258.xlsx"
          "BLU216.xlsx"
          "BON234.xlsx"
          "BOR203.xlsx"
          "DAB265.xlsx"
          "ECH245.xlsx"
          "ELK275.xlsx"
          "FUT213.xlsx"
          "GKR257.xlsx"
          "GLI246.xlsx"
          "HEL227.xlsx"
          "HET244.xlsx"
          "IKE223.xlsx"
          "IWI266.xlsx"
          "JAN214.xlsx"
          "JED278.xlsx"
          "JUR232.xlsx"
          "KAS268.xlsx"
          "KOR260.xlsx"
          "KRM242.xlsx"
          "KRZ273.xlsx"
          "LAZ255.xlsx"
          "LEA251.xlsx"
          "LPU264.xlsx"
          "LUD277.xlsx"
          "MAN212.xlsx"
          "MAR221.xlsx"
          "MOK215.xlsx"
          "NOS279.xlsx"
          "NPK261.xlsx"
          "OLA274.xlsx"
          "OLI231.xlsx"
          "OPS247.xlsx"
          "PCC248.xlsx"
          "PIA218.xlsx"
          "POR236.xlsx"
          "PPM256.xlsx"
          "PRO208.xlsx"
          "PUL269.xlsx"
          "RED220.xlsx"
          "REM226.xlsx"
          "SDL272.xlsx"
          "SKO217.xlsx"
          "STA235.xlsx"
          "SWI262.xlsx"
          "TRW276.xlsx"
          "TUR263.xlsx"
          "ZAB254.xlsx"
          "ZAM271.xlsx"
          "ZPT210.xlsx" ]

[<Fact>]
let ``basic test 1 table`` () =
    testBasic
        "C:/Users/Bartek/source/repos/FSharp/ConverterApp/Tests/excel/CENNIK4.xlsx"
        "C:/Users/Bartek/source/repos/FSharp/ConverterApp/Tests/excel/zwykly1.xls"
        [ "Sklep w CH Tesco Suwałki1 Kościuszki 10.xlsx" ]

[<Fact>]
let ``basic test many tables`` () =
    test
        convert
        "C:/Users/Bartek/source/repos/FSharp/ConverterApp/Tests/excel/CENNIK5.xlsx"
        "C:/Users/Bartek/source/repos/FSharp/ConverterApp/Tests/excel/zwykly2.xls"
        [ "Gdańsk.xlsx"
          "Katowice.xlsx"
          "Łomianki ul.Burakowa.xlsx"
          "Sklep CH Ursynów Warszawa WAW07.xlsx"
          "Sklep GDA02 ZOO FORUM w Gdańsku.xlsx"
          "Sklep Kalisz 01 Galeria Amber lokal AL-0652.xlsx"
          "Sklep Łódź 02 CHU GALERIA Łódzka.xlsx"
          "Sklep Radom 01 Chrobrego 1.xlsx"
          "Sklep w CH Pasaż Grunwaldzki Wrocła Sklep w CH Pasaż Grunwaldzki Wrocł.xlsx"
          "Sklep w CH Tesco Suwałki1 Kościuszki 10.xlsx"
          "Sklep w CH-U Arkadia WAW04 w Warszawie.xlsx"
          "Sklep w GAL. Północna WAW06 w Warszawie.xlsx"
          "Sklep w Galeria H-U ATUT w Węgrzcac w Węgrzcach.xlsx"
          "Sklep w Galeria Sfera BIE01 w Bielko Białej.xlsx"
          "Sklep w Galeria Sudecka JEL01 w Jeleniej Górze.xlsx"
          "Stare Miasto k Konina Centrum Handlowe Ferio.xlsx"
          "Tarnowo Podgórne k Poznania Tarnowo Podgórne k Poznania.xlsx" ]
