open ExcelConverter
open System.Diagnostics

[<EntryPoint>]
let main args = 
    // convert 
    //     "C:/Users/Bartek/source/repos/FSharp/ConverterApp/ExcelConverter/excel/Kopia document.xls"
    //     "C:/Users/Bartek/source/repos/FSharp/ConverterApp/ExcelConverter/excel/CENNIK.xlsx"
    //     |> printfn "%A"
    let stopwatch = Stopwatch()
    stopwatch.Start()
    convertKakadu "C:/Users/Bartek/source/repos/FSharp/ConverterApp/ExcelConverter/excel/Dermapharm_Zam_2021-09-02_skl.xlsx" "C:/Users/Bartek/source/repos/FSharp/ConverterApp/ExcelConverter/excel/CENNIK.xlsx" 
    |> ignore
    stopwatch.Stop()
    printfn "%i" stopwatch.ElapsedMilliseconds
    0