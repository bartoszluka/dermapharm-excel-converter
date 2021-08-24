type Result'<'ok, 'error> =
    | Ok of 'ok
    | Error of 'error

type HttpResult = Result'<int, string list>

let okResult = Ok 21
let error = Error "some error message"

match okResult with
| Ok n -> printfn "this is ok %d" n // to się wykona
| Error err -> printfn "there was an error: %s" err // a to nie
