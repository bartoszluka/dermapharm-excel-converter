open ExcelConverter
open System.IO

let private usage = "expected one .xls file"

[<EntryPoint>]
let main argv =

    if isNull argv || Array.length argv <> 2 then
        printfn "%s" usage
        1
    else
        convert argv.[0] argv.[1] |> ignore

        0
