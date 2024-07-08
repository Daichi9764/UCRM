using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace KNXBoostDesktop
{
    public partial class LoadingWindow
    {
        private ObservableCollection<Activity> Activities { get; }

        public LoadingWindow()
        {
            InitializeComponent();

            ProgressBar.Style = (Style)FindResource("NormalProgressBar");
            ProgressBar.IsIndeterminate = true;

            Activities = new ObservableCollection<Activity>();
            ActivityLog.ItemsSource = Activities;
        }

        public void UpdatePosition(double mainWindowLeft, double mainWindowTop)
        {
            Left = mainWindowLeft + (Owner.Width - Width) / 2;
            Top = mainWindowTop + (Owner.Height - Height) / 2;
        }
        
        public async Task CloseAfterDelay(int delay)
        {
            await Task.Delay(delay).ConfigureAwait(false);
            Dispatcher.Invoke(Close);
            //_closeCompletionSource.SetResult(true);
        }

        public void UpdateTaskName(string taskName)
        {
            Dispatcher.Invoke(() => { TaskNameText.Text = taskName; });
        }

        public void LogActivity(string activity)
        {
            Dispatcher.Invoke(() =>
            {
                var activityToAdd = new Activity
                {
                    Text = activity,
                    Background = "Transparent",
                    IsCompleted = false
                };
                
                activityToAdd.Foreground = 
                    App.DisplayElements!.SettingsWindow!.EnableLightTheme
                        ? "#000000" : "#FFFFFF";
                
                Activities.Add(activityToAdd);
                //ApplyActivityStyle();
                ActivityLog.ScrollIntoView(Activities.Last());
            });
        }

        public void CompleteActivity()
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Style = (Style)FindResource("GreenProgressBar");
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = 100;
            });
        }

        public void MarkActivityComplete()
        {
            Dispatcher.Invoke(() =>
            {
                if (Activities.Count > 0)
                {
                    Activities.Last().IsCompleted = true;
                    ActivityLog.Items.Refresh();
                }
            });
        }

        public void SetLightMode()
        {
            MainGrid.Background = MainWindow.ConvertStringColor("#FFFFFF");
            TaskNameText.Foreground = MainWindow.ConvertStringColor("#000000");
            ActivityLog.Background = MainWindow.ConvertStringColor("#F5F5F5");
            ProgressBar.Background = MainWindow.ConvertStringColor("#FFFFFF");
            ActivityLog.Foreground = MainWindow.ConvertStringColor("#000000");
            ActivityLog.BorderBrush = MainWindow.ConvertStringColor("#D7D7D7");
            ActivityLog.ItemContainerStyle = (Style)FindResource("LightActivityStyle");
        }
        
        public void SetDarKMode()
        {
            MainGrid.Background = MainWindow.ConvertStringColor("#313131");
            TaskNameText.Foreground = MainWindow.ConvertStringColor("#FFFFFF");
            ActivityLog.Background = MainWindow.ConvertStringColor("#262626");
            ProgressBar.Background = MainWindow.ConvertStringColor("#262626");
            ActivityLog.Foreground = MainWindow.ConvertStringColor("#FFFFFF");
            ActivityLog.BorderBrush = MainWindow.ConvertStringColor("#434343");
            ActivityLog.ItemContainerStyle = (Style)FindResource("DarkActivityStyle");
        }
    }

    public class Activity
    {
        public string Text { get; set; }
        public bool IsCompleted { get; set; }
        public string Background { get; set; }
        public string Foreground { get; set; }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
