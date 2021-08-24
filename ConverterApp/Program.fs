module ConverterApp

open Elmish.WPF
open Elmish
open System.IO

type ErrorString = string
type OutFiles = string list

type MaybeFiles =
    { MaybeInputFile: string option
      MaybeDictFile: string option }

type Files = { InputFile: string; DictFile: string }

type State =
    | GatheringData of MaybeFiles
    | GotAllData of Files
    | Converting
    | Done of Result<OutFiles, ErrorString>

type Model =
    { AppState: State
      StatusMessage: string }


let init () =
    { AppState =
          GatheringData
              { MaybeInputFile = None
                MaybeDictFile = None }
      StatusMessage = "Wybierz pliki do konwersji" },
    []

type Msg =
    | RequestSelectInputFile
    | RequestSelectDictFile
    | SetInputFileCanceled
    | SetDictFileCanceled
    | SetInputFile of string
    | SetDictFile of string
    | SetStatusMessage of string
    | RequestConvert
    | ConvertSuccess of string list
    | ConvertFailed of exn
    | SetInputFileFailed of exn
    | SetDictFileFailed of exn


let resultDoubleMap ifOk ifError =
    function
    | Ok files -> files |> ifOk
    | Error errors -> errors |> ifError

let unPair f (x, y) = f x y

let convert inputFile dictFile =
    async {
        return
            ExcelConverter.convert inputFile dictFile
            |> resultDoubleMap ConvertSuccess (ConvertFailed << failwith)
    }


let loadOutputDirectory () =
    async {
        let dlg = Microsoft.Win32.OpenFileDialog()
        dlg.Filter <- "Dokument Excel (*.xls)|*.xls"
        dlg.DefaultExt <- "xls"
        let result = dlg.ShowDialog()

        if result.HasValue && result.Value then
            return SetInputFile dlg.FileName
        else
            return SetInputFileCanceled
    }


let loadInputFile () =
    async {
        let dlg = Microsoft.Win32.OpenFileDialog()
        dlg.Filter <- "Dokument Excel (*.xls)|*.xls"
        dlg.DefaultExt <- "xls"
        let result = dlg.ShowDialog()

        if result.HasValue && result.Value then
            return SetInputFile dlg.FileName
        else
            return SetInputFileCanceled
    }

let loadDictFile () =
    async {
        let dlg = Microsoft.Win32.OpenFileDialog()
        dlg.Filter <- "Słownik w formacie tekstowym (*.txt)|*.txt"
        dlg.DefaultExt <- "txt"
        let result = dlg.ShowDialog()

        if result.HasValue && result.Value then
            return SetDictFile dlg.FileName
        else
            return SetDictFileCanceled
    }

let convertToGotData model =
    match model.AppState with
    | GatheringData { MaybeInputFile = Some input
                      MaybeDictFile = Some dict } ->
        { model with
              AppState = GotAllData { InputFile = input; DictFile = dict } }
    | _ -> model


let updateGatheringDataDict model dictFile =
    match model.AppState with
    | GatheringData maybeFiles ->
        { model with
              StatusMessage = "Wybierz plik Excel"
              AppState =
                  GatheringData
                      { maybeFiles with
                            MaybeDictFile = Some dictFile } }
    | _ -> model

let updateGatheringDataInput model inputFile =
    match model.AppState with
    | GatheringData maybeFiles ->
        { model with
              StatusMessage = "Wybierz słownik"
              AppState =
                  GatheringData
                      { maybeFiles with
                            MaybeInputFile = Some inputFile } }
    | _ -> model


let update msg model =
    match msg with
    | RequestConvert ->
        match model.AppState with
        | GotAllData files ->
            { model with
                  StatusMessage = "Konwertowanie..."
                  AppState = Converting },
            Cmd.OfAsync.either (unPair convert) (files.InputFile, files.DictFile) id ConvertFailed
        // TODO: give some useful error message
        | _ -> model, Cmd.none
    | RequestSelectInputFile ->
        { model with
              StatusMessage = "Ładowanie pliku excel" },
        Cmd.OfAsync.either loadInputFile () id SetInputFileFailed
    | RequestSelectDictFile ->
        { model with
              StatusMessage = "Ładowanie słownika" },
        Cmd.OfAsync.either loadDictFile () id SetDictFileFailed
    | SetInputFileCanceled ->
        { model with
              //tutaj był error
              StatusMessage = "Wybierz ponownie dokument Excel" },
        Cmd.none

    | SetInputFile inputFile ->
        updateGatheringDataInput model inputFile
        |> convertToGotData,
        Cmd.none

    | SetDictFile dictFile ->
        updateGatheringDataDict model dictFile
        |> convertToGotData,
        Cmd.none

    | SetDictFileCanceled ->
        { model with
              //tutaj był error
              StatusMessage = "Wybierz plik słownika ponownie" },
        Cmd.none
    | ConvertSuccess createdFiles ->
        { model with
              StatusMessage = "Udało się przekonwertować"
              AppState = Done <| Ok createdFiles },
        Cmd.none
    | ConvertFailed errorList ->
        { model with
              //tutaj był error
              StatusMessage =
                  "Nie udało się przekonwertowć. Upewnij się, że pliki nie są otwarte w innym programie (np. w Excelu)"
              AppState = Done <| Error errorList.Message },
        Cmd.none

    | SetInputFileFailed (_) ->
        { model with
              //tutaj był error
              StatusMessage = "Coś poszło nie tak, spróbuj wybrać dokument Excel ponownie" },
        Cmd.none

    | SetDictFileFailed (_) ->
        { model with
              //tutaj był error
              StatusMessage = "Coś poszło nie tak, spróbuj wybrać słownik ponownie" },
        Cmd.none
    | SetStatusMessage text -> { model with StatusMessage = text }, Cmd.none

let intersperse sep ls =
    List.foldBack
        (fun x ->
            function
            | [] -> [ x ]
            | xs -> x :: sep :: xs)
        ls
        []

let displayOutFiles model =
    match model.AppState with
    | Done (Ok files) -> files |> intersperse "\n" |> List.reduceBack (+)
    | _ -> "Jeszcze Nie przekonwertowano"

let displayInputFile model =
    match model.AppState with
    | GatheringData ({ MaybeInputFile = (Some file) }) -> file
    | GotAllData ({ InputFile = file }) -> file
    | Done (Ok _) -> ""
    | _ -> "Wybierz plik excel"

let displayDictFile model =
    match model.AppState with
    | GatheringData ({ MaybeDictFile = (Some file) }) -> file
    | GotAllData ({ DictFile = file }) -> file
    | Done (Ok _) -> ""
    | _ -> "Wybierz słownik"

let bindings () : Binding<Model, Msg> list =
    [ "StatusMessage"
      |> Binding.oneWay (fun m -> m.StatusMessage)
      "OutputFiles" |> Binding.oneWay displayOutFiles
      "Convert" |> Binding.cmd RequestConvert
      "InputFile" |> Binding.oneWay displayInputFile
      "DictFile" |> Binding.oneWay displayDictFile
      "SelectInputFile"
      |> Binding.cmd RequestSelectInputFile
      "IsButtonEnabled"
      |> Binding.oneWay
          (fun m ->
              match m.AppState with
              | GotAllData _ -> true
              | _ -> false)
      "ConvertButtonText"
      |> Binding.oneWay (fun _ -> "Konwertuj")
      "SelectDictFile"
      |> Binding.cmd RequestSelectDictFile ]


let designVm =
    ViewModel.designInstance (init () |> fst) (bindings ())


let main window =

    WpfProgram.mkProgram init update bindings
    |> WpfProgram.startElmishLoop window
