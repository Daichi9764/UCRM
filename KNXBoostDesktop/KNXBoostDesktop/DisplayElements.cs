namespace KNXBoostDesktop
{
    public class DisplayElements
    {
        public MainWindow MainWindow { get; private set; }
        public ConsoleWindow ConsoleWindow { get; private set; }

        public DisplayElements()
        {
            MainWindow = new MainWindow();
            ConsoleWindow = new ConsoleWindow();
        }

        public void ShowMainWindow()
        {
            MainWindow.Show();
        }

        public void ShowConsoleWindow()
        {
            ConsoleWindow.Show();
        }
    }
}
