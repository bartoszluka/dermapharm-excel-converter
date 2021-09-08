open ExcelConverter

[<EntryPoint>]
let main args = 
    convert 
        "C:/Users/Bartek/source/repos/FSharp/ConverterApp/ExcelConverter/excel/Kopia document.xls"
        "C:/Users/Bartek/source/repos/FSharp/ConverterApp/ExcelConverter/excel/CENNIK.xlsx"
        |> printfn "%A"
    0