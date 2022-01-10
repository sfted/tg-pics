using Microsoft.UI.Xaml;

namespace TgPics
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        private Window mainWindow;

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            mainWindow = new MainWindow();
            mainWindow.Activate();
        }
    }
}
