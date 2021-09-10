open ExcelConverter

[<EntryPoint>]
let main args = 
    // convert 
    //     "C:/Users/Bartek/source/repos/FSharp/ConverterApp/ExcelConverter/excel/Kopia document.xls"
    //     "C:/Users/Bartek/source/repos/FSharp/ConverterApp/ExcelConverter/excel/CENNIK.xlsx"
    //     |> printfn "%A"
    convertKakadu "C:/Users/Bartek/source/repos/FSharp/ConverterApp/ExcelConverter/excel/Dermapharm_Zam_2021-09-02_skl.xlsx" "C:/Users/Bartek/source/repos/FSharp/ConverterApp/ExcelConverter/excel/CENNIK.xlsx" 
    |> printfn "%A"
    0