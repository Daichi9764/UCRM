using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace KNXBoostDesktop
{
    public partial class LoadingWindow
    {
        private ObservableCollection<Activity> Activities { get; }
        private TaskCompletionSource<bool>  _closeCompletionSource;

        public LoadingWindow()
        {
            InitializeComponent();

            progressBar.Style = (Style)FindResource("NormalProgressBar");
            progressBar.IsIndeterminate = true;

            Activities = new ObservableCollection<Activity>();
            activityLog.ItemsSource = Activities;
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
            Dispatcher.Invoke(() => { taskNameText.Text = taskName; });
        }

        public void LogActivity(string activity)
        {
            Dispatcher.Invoke(() =>
            {
                Activities.Add(new Activity { Text = activity, IsCompleted = false });
                activityLog.ScrollIntoView(Activities.Last());
            });
        }

        public void CompleteActivity()
        {
            Dispatcher.Invoke(() =>
            {
                progressBar.Style = (Style)FindResource("GreenProgressBar");
                progressBar.IsIndeterminate = false;
                progressBar.Value = 100;
            });
        }

        public void MarkActivityComplete()
        {
            Dispatcher.Invoke(() =>
            {
                if (Activities.Count > 0)
                {
                    Activities.Last().IsCompleted = true;
                    activityLog.Items.Refresh();
                }
            });
        }
    }

    public class Activity
    {
        public string Text { get; set; }
        public bool IsCompleted { get; set; }
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
