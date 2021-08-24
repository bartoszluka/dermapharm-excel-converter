module ConverterApp

open Elmish.WPF
open Elmish
open System.IO

type ErrorList = string list
type OutFiles = string list

type MaybeFiles =
    { InputFile: string option
      DictFile: string option }

type Files = { InputFile: string; DictFile: string }

type State =
    | GatheringData of MaybeFiles
    | GotAllData of Files
    | Converting
    | Done of Result<OutFiles, ErrorList>

type Model =
    { InputFile: string option
      DictFile: string option
      // OutputDirectory: string
      AppState: State
      StatusMessage: string
      ErrorMessage: string
      CreatedFiles: string list }


let init () =
    { InputFile = Some ""
      DictFile = Some ""
      //   OutputDirectory = ""
      AppState = GatheringData { InputFile = None; DictFile = None }
      StatusMessage = "Wybierz pliki do konwersji"
      ErrorMessage = ""
      CreatedFiles = [] },
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


let resultDoubleMap ifOk ifError result =
    match result with
    | Ok files -> files |> ifOk
    | Error errors -> errors |> ifError

let convert (inputFile, dictFile) =
    async {
        return
            match inputFile, dictFile with
            | Some excel, Some dict ->
                ExcelConverter.convert excel dict
                |> resultDoubleMap ConvertSuccess (ConvertFailed << failwith)
            | None, Some _ -> SetStatusMessage "Nie wybrano pliku excel"
            | Some _, None -> SetStatusMessage "Nie wybrano pliku słownika"
            | None, None -> SetStatusMessage "Nie wybrano żadnego z plików wejściowych"
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

let update msg model =
    match msg with
    | RequestConvert ->
        { model with AppState = Converting },
        Cmd.OfAsync.either
            convert
            (model.InputFile, model.DictFile)
            id
            //  (fun mg mod -> { m with AppState = Done(Ok []) })
            ConvertFailed
    | RequestSelectInputFile -> model, Cmd.OfAsync.either loadInputFile () id SetInputFileFailed
    | RequestSelectDictFile -> model, Cmd.OfAsync.either loadDictFile () id SetDictFileFailed
    | SetInputFileCanceled ->
        { model with
              ErrorMessage = "Wybierz ponownie dokument Excel" },
        Cmd.none
    | SetInputFile inputFile ->
        { model with
              InputFile = Some inputFile },
        Cmd.none
    | SetDictFile dictFile -> { model with DictFile = Some dictFile }, Cmd.none
    | SetDictFileCanceled ->
        { model with
              ErrorMessage = "Wybierz plik słownika ponownie" },
        Cmd.none
    | ConvertSuccess createdFiles ->
        { model with
              CreatedFiles = createdFiles
              StatusMessage = "Udało się przekonwertować"
              AppState = Done(Ok []) },
        Cmd.none
    | ConvertFailed (_) ->
        { model with
              ErrorMessage =
                  "Nie udało się przekonwertowć. Upewnij się, że pliki nie są otwarte w innym programie (np Excelu)" },
        Cmd.none
    | SetInputFileFailed (_) ->
        { model with
              ErrorMessage = "Coś poszło nie tak, spróbuj wybrać dokument Excel ponownie" },
        Cmd.none
    | SetDictFileFailed (_) ->
        { model with
              ErrorMessage = "Coś poszło nie tak, spróbuj wybrać słownik ponownie" },
        Cmd.none
    | SetStatusMessage text -> { model with StatusMessage = text }, Cmd.none


let bindings () : Binding<Model, Msg> list =
    [ "StatusMessage"
      |> Binding.oneWay (fun m -> m.StatusMessage)
      "ErrorMessage"
      |> Binding.oneWay (fun m -> m.ErrorMessage)
      "OutputFiles"
      |> Binding.oneWay (fun m -> m.CreatedFiles)
      "Convert" |> Binding.cmd RequestConvert
      "InputFile"
      |> Binding.oneWay (fun m -> Option.defaultValue "Nie wybrano pliku" m.InputFile)
      "DictFile"
      |> Binding.oneWay (fun m -> Option.defaultValue "Nie wybrano pliku" m.DictFile)
      "SelectInputFile"
      |> Binding.cmd RequestSelectInputFile
      "IsButtonEnabled"
      |> Binding.oneWay
          (fun m ->
              match m.AppState with
              | GatheringData -> true
              | Converting -> false
              | Done _ -> false)
      "ConvertButtonText"
      |> Binding.oneWay
          (fun m ->
              match m.AppState with
              | GatheringData -> "Wybierz pliki"
              | Converting -> "Konwertowanie"
              | Done _ -> "Koniec")
      "SelectDictFile"
      |> Binding.cmd RequestSelectDictFile ]


let designVm =
    ViewModel.designInstance (init () |> fst) (bindings ())


let main window =

    WpfProgram.mkProgram init update bindings
    |> WpfProgram.startElmishLoop window
