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
        }

        private void StartElmish(object sender, EventArgs e)
        {
            Activated -= StartElmish;
            ConverterApp.main(MainWindow);
        }

    }
}
