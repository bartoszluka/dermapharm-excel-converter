module ConverterApp

open Elmish.WPF
open Elmish
open System.IO

type ErrorString = string
type OutFiles = string list

type InputFileType =
    | Basic
    | Kakadu

type MaybeFiles =
    { MaybeInputFile: string option
      MaybeDictFile: string option
      InputFileType: InputFileType }

type Files =
    { InputFile: string
      DictFile: string
      InputFileType: InputFileType }

type State =
    | GatheringData of MaybeFiles
    | GotAllData of Files
    | Converting
    | Done of Result<OutFiles, ErrorString>

type Model =
    { AppState: State
      StatusMessage: string }


type Msg =
    | RequestSelectInputFile
    | RequestSelectDictFile
    | ChangeInputFileType of InputFileType
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
    | LoadLastDictFailed of exn
    | LoadLastDict

let init () =
    { AppState =
          GatheringData
              { MaybeInputFile = None
                MaybeDictFile = None
                InputFileType = Basic }
      StatusMessage = "Wybierz pliki do konwersji" },
    Cmd.ofMsg LoadLastDict


let resultDoubleMap ifOk ifError =
    function
    | Ok files -> files |> ifOk
    | Error errors -> errors |> ifError

let unPair f (x, y) = f x y

let unTriple f (x, y, z) = f x y z

let convert inputFileType inputFile dictFile =
    async {
        let convertingFunction =
            match inputFileType with
            | Basic _ -> ExcelConverter.convert
            | Kakadu -> ExcelConverter.convertKakadu

        return
            convertingFunction inputFile dictFile
            |> resultDoubleMap ConvertSuccess (ConvertFailed << failwith)
    }

let private lastDictFilePath = "last-dict-file"

let loadLastDict () =
    async {
        let! contents =
            File.ReadAllTextAsync lastDictFilePath
            |> Async.AwaitTask

        return SetDictFile contents
    }

let loadInputFile () =
    async {
        let dlg = Microsoft.Win32.OpenFileDialog()
        dlg.Filter <- "Dokument Excel (*.xls lub *.xlsx)|*.xls*"
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
        dlg.Filter <- "Słownik w formacie Excel (*.xlsx)|*.xlsx"
        dlg.DefaultExt <- "xlsx"
        let result = dlg.ShowDialog()

        if result.HasValue && result.Value then
            do!
                File.WriteAllTextAsync(lastDictFilePath, dlg.FileName)
                |> Async.AwaitTask

            return SetDictFile dlg.FileName
        else
            return SetDictFileCanceled
    }

let convertToGotData model =
    match model.AppState with
    | GatheringData { MaybeInputFile = Some input
                      MaybeDictFile = Some dict
                      InputFileType = inputFileType } ->
        { model with
              AppState =
                  GotAllData
                      { InputFile = input
                        DictFile = dict
                        InputFileType = inputFileType } }
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
    | Done _ ->
        { model with
              StatusMessage = "Wybierz plik Excel"
              AppState =
                  GatheringData
                      { MaybeInputFile = None
                        MaybeDictFile = Some dictFile
                        InputFileType = Basic } }
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
    | Done _ ->
        { model with
              StatusMessage = "Wybierz słownik"
              AppState =
                  GatheringData
                      { MaybeInputFile = Some inputFile
                        MaybeDictFile = None
                        InputFileType = Basic } }
    | _ -> model

let none model = model, Cmd.none

let updateInputFileType model inputFileType =
    match model.AppState with
    | GatheringData maybeFiles ->
        { model with
              AppState =
                  GatheringData
                  <| { maybeFiles with
                           InputFileType = inputFileType } }
    | GotAllData files ->
        { model with
              AppState =
                  GotAllData
                  <| { files with
                           InputFileType = inputFileType } }

    | _ -> model

let update msg model =
    match msg with
    | RequestConvert ->
        match model.AppState with
        | GotAllData files ->
            { model with
                  StatusMessage = "Konwertowanie..."
                  AppState = Converting },
            Cmd.OfAsync.either
                (unTriple convert)
                (files.InputFileType, files.InputFile, files.DictFile)
                id
                ConvertFailed
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
    | ChangeInputFileType fileType -> updateInputFileType model fileType |> none
    | SetInputFileCanceled ->
        { model with
              //tutaj był error
              StatusMessage = "Wybierz ponownie dokument Excel" },
        Cmd.none

    | SetInputFile inputFile ->
        updateGatheringDataInput model inputFile
        |> convertToGotData
        |> none

    | SetDictFile dictFile ->
        updateGatheringDataDict model dictFile
        |> convertToGotData
        |> none

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
#if DEBUG
    | LoadLastDictFailed e -> { model with StatusMessage = e.Message }, Cmd.none
#else
    | LoadPreviousDictFailed _ -> model, Cmd.none
#endif
    | LoadLastDict -> model, Cmd.OfAsync.either loadLastDict () id LoadLastDictFailed

let displayOutFiles model =
    match model.AppState with
    | Done (Ok files) -> files
    | _ -> []

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

let radioButtonState state fallback model =
    match model.AppState with
    | GatheringData maybeFiles -> maybeFiles.InputFileType = state
    | GotAllData files -> files.InputFileType = state
    | _ -> fallback

let bindings () : Binding<Model, Msg> list =
    [ "StatusMessage"
      |> Binding.oneWay (fun m -> m.StatusMessage)
      "OutputFiles" |> Binding.oneWay displayOutFiles
      "Convert" |> Binding.cmd RequestConvert
      "InputFile" |> Binding.oneWay displayInputFile
      "DictFile" |> Binding.oneWay displayDictFile
      "SelectInputFile"
      |> Binding.cmd RequestSelectInputFile
      "ChangeInputToBasic"
      |> Binding.cmd (ChangeInputFileType Basic)
      "ChangeInputToKakadu"
      |> Binding.cmd (ChangeInputFileType Kakadu)
      "IsBasic"
      |> Binding.oneWay (radioButtonState Basic true)
      "IsKakadu"
      |> Binding.oneWay (radioButtonState Kakadu false)
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
