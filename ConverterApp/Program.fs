module ConverterApp

open Elmish.WPF
open Elmish
open System.IO
open Squirrel

type ErrorString = string
type OutFiles = string list

type InputFileType =
    | Basic
    | Kakadu

type MaybeFiles =
    { MaybeInputFile: string option
      MaybeDictFile: string option }

type Files = { InputFile: string; DictFile: string }

type State =
    | GatheringData of MaybeFiles
    | GotAllData of Files
    | Converting
    | Done of Result<OutFiles, ErrorString>

type Inputs =
    | Files of Files
    | MaybeFiles of MaybeFiles

type Model =
    { StatusMessage: string
      Files: Inputs
      InputFileType: InputFileType
      Results: string list }


type Msg =
    // user interaction
    | RequestSelectInputFile
    | RequestSelectDictFile
    | ChangeInputFileType of InputFileType
    | RequestConvert
    | SetInputFileCanceled
    | SetDictFileCanceled
    // internal
    | SquirrelUpdate
    | SetInputFile of string
    | SetDictFile of string
    | SetStatusMessage of string
    | ConvertSuccess of string list
    | LoadLastDict
    // errors
    | ConvertFailed of exn
    | SetInputFileFailed of exn
    | SetDictFileFailed of exn
    | LoadLastDictFailed of exn
    | SquirrelUpdateFailed of exn

let init () =
    { Files =
          MaybeFiles
              { MaybeInputFile = None
                MaybeDictFile = None }
      InputFileType = Basic
      Results = []
      StatusMessage = "Wybierz pliki do konwersji" },
    Cmd.ofMsg LoadLastDict


let resultDoubleMap ifOk ifError =
    function
    | Ok files -> files |> ifOk
    | Error errors -> errors |> ifError

let unTriple f (x, y, z) = f x y z

let convert inputFileType inputFile dictFile =
    async {
        let convertingFunction =
            match inputFileType with
            | Basic -> ExcelConverter.convert
            | Kakadu -> ExcelConverter.convertKakadu

        return
            convertingFunction inputFile dictFile
            |> resultDoubleMap ConvertSuccess (ConvertFailed << failwith)
    }

let private lastDictFilePath =
    sprintf "%s\\%s" (System.Environment.GetEnvironmentVariable "HOMEPATH") ".last-dict-file"

let squirrelUpdate () =
    async {
        let m: UpdateManager =
            UpdateManager.GitHubUpdateManager("https://github.com/bartoszluka/dermapharm-excel-converter")
            |> Async.AwaitTask
            |> Async.RunSynchronously

        let! _ = m.UpdateApp() |> Async.AwaitTask

        let! _ =
            m.CreateUninstallerRegistryEntry()
            |> Async.AwaitTask

        m.CreateShortcutForThisExe(
            ShortcutLocation.Desktop
            ||| ShortcutLocation.StartMenu
        )

        return SetStatusMessage "updated"
    }

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
    match model.Files with
    | MaybeFiles { MaybeInputFile = Some i
                   MaybeDictFile = Some d } ->
        { model with
              Files = Files { InputFile = i; DictFile = d } }
    | _ -> model

let updateGatheringDataDict model dictFile =
    match model.Files with
    | MaybeFiles maybeFiles ->
        { model with
              StatusMessage = "Wybierz plik Excel"
              Files =
                  MaybeFiles
                      { maybeFiles with
                            MaybeDictFile = Some dictFile } }
    | Files files ->
        { model with
              StatusMessage = "Zaktualizowano słownik"
              Files = Files { files with DictFile = dictFile } }

let updateGatheringDataInput model inputFile =
    match model.Files with
    | MaybeFiles maybeFiles ->
        { model with
              StatusMessage = "Wybierz słownik"
              Files =
                  MaybeFiles
                      { maybeFiles with
                            MaybeInputFile = Some inputFile } }
    | Files files ->
        { model with
              StatusMessage = "Zaktualizowano plik Excel"
              Files = Files { files with InputFile = inputFile } }

let none model = model, Cmd.none

let updateInputFileType model inputFileType =
    { model with
          InputFileType = inputFileType }

let update msg model =
    match msg with
    | RequestConvert ->
        match model.Files with
        | Files files ->
            { model with
                  StatusMessage = "Konwertowanie..." },
            Cmd.OfAsync.either
                (unTriple convert)
                (model.InputFileType, files.InputFile, files.DictFile)
                id
                ConvertFailed
        | _ ->
            { model with
                  StatusMessage = "Najpierw wybierz plik excelowy i słownik" },
            Cmd.none
    | RequestSelectInputFile ->
        { model with
              StatusMessage = "Ładowanie pliku excel" },
        Cmd.OfAsync.either loadInputFile () id SetInputFileFailed
    | RequestSelectDictFile ->
        { model with
              StatusMessage = "Ładowanie słownika" },
        Cmd.OfAsync.either loadDictFile () id SetDictFileFailed
    | ChangeInputFileType inputFileType ->
        { model with
              InputFileType = inputFileType }
        |> none
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
              Results = createdFiles },
        Cmd.none

    | ConvertFailed errorList ->
        { model with
              StatusMessage =
                  "Nie udało się przekonwertowć. Upewnij się, że pliki nie są otwarte w innym programie (np. w Excelu)"
                  + "\n"
                  + errorList.Message
              Results = [] },
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
    | SquirrelUpdateFailed e -> { model with StatusMessage = e.Message }, Cmd.none
#else
    | LoadLastDictFailed _ -> model, Cmd.none
    | SquirrelUpdateFailed _ -> model, Cmd.none
#endif
    | LoadLastDict -> model, Cmd.OfAsync.either loadLastDict () id LoadLastDictFailed
    | SquirrelUpdate -> model, Cmd.OfAsync.either squirrelUpdate () id SquirrelUpdateFailed

let displayInputFile model =
    match model.Files with
    | MaybeFiles ({ MaybeInputFile = (Some file) }) -> file
    | Files ({ InputFile = file }) -> file
    | _ -> "Wybierz plik excel"

let displayDictFile model =
    match model.Files with
    | MaybeFiles ({ MaybeDictFile = (Some file) }) -> file
    | Files ({ DictFile = file }) -> file
    | _ -> "Wybierz słownik"

let radioButtonState state model = model.InputFileType = state

let bindings () : Binding<Model, Msg> list =
    [ "Title"
      |> Binding.oneWay (fun _ -> "Konwerter Excela dla Dermapharm")
      "Update" |> Binding.cmd SquirrelUpdate
      "StatusMessage"
      |> Binding.oneWay (fun m -> m.StatusMessage)
      "OutputFiles"
      |> Binding.oneWay (fun m -> m.Results)
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
      |> Binding.oneWay (radioButtonState Basic)
      "IsKakadu"
      |> Binding.oneWay (radioButtonState Kakadu)
      "IsButtonEnabled"
      |> Binding.oneWay (fun _ -> true)
      //   (fun m ->
      //       match m.Files with
      //       | Files _ -> true
      //       | MaybeFiles _ -> false)
      "ConvertButtonText"
      |> Binding.oneWay (fun _ -> "Konwertuj")
      "SelectDictFile"
      |> Binding.cmd RequestSelectDictFile ]


let designVm =
    ViewModel.designInstance (init () |> fst) (bindings ())


let main window =

    WpfProgram.mkProgram init update bindings
    |> WpfProgram.startElmishLoop window
