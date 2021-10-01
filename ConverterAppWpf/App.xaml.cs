using System;
using System.Windows;
using Squirrel;

namespace ConverterAppWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Activated += StartElmish;
            SquirrelAwareApp.HandleEvents(
                onInitialInstall: OnInstall,
                onAppUpdate: OnUpdate,
                onAppUninstall: OnUninstall,
                onFirstRun: OnFirstRun);

            Update();
        }

        private static async void Update()
        {
            using var manager = await UpdateManager.GitHubUpdateManager("https://github.com/bartoszluka/dermapharm-excel-converter");

            // await manager.CreateUninstallerRegistryEntry();
            // manager.CreateShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
            await manager.UpdateApp();
        }

        private static void OnInstall(Version obj)
        {
            using var manager = new UpdateManager("");
            // using var manager = UpdateManager.GitHubUpdateManager("https://github.com/bartoszluka/dermapharm-excel-converter");
            manager.CreateUninstallerRegistryEntry();
            manager.CreateShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
        }

        private void OnFirstRun()
        {
            // var result = MessageBox.Show("Witaj!");
        }

        private void OnUninstall(Version obj)
        {

            // throw new NotImplementedException();
        }

        private void OnUpdate(Version obj)
        {
            // throw new NotImplementedException();
        }

        private void StartElmish(object sender, EventArgs e)
        {
            Activated -= StartElmish;
            ConverterApp.main(MainWindow);
        }

    }
}
