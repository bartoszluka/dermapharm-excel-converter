using System;
using System.Threading.Tasks;
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
            using var manager = UpdateManager.GitHubUpdateManager("https://github.com/bartoszluka/dermapharm-excel-converter");

            await manager.Result.UpdateApp();
        }

        private static void OnInstall(Version obj)
        {
            using var manager = new UpdateManager("https://the.place/you-host/updates");
            // using var manager = UpdateManager.GitHubUpdateManager("https://github.com/bartoszluka/dermapharm-excel-converter");
            manager.CreateUninstallerRegistryEntry();
            manager.CreateShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
        }

        private void OnFirstRun()
        {
            throw new NotImplementedException();
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
