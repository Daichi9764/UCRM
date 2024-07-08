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
        /// <summary>
        /// Gets the collection of activities displayed in the loading window.
        /// </summary>
        private ObservableCollection<Activity> Activities { get; }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingWindow"/> class.
        /// Sets up the progress bar and initializes the activities collection.
        /// </summary>
        public LoadingWindow()
        {
            InitializeComponent();

            ProgressBar.Style = (Style)FindResource("NormalProgressBar");
            ProgressBar.IsIndeterminate = true;

            Activities = new ObservableCollection<Activity>();
            ActivityLog.ItemsSource = Activities;
        }

        
        /// <summary>
        /// Updates the position of the loading window relative to the main window's position.
        /// </summary>
        /// <param name="mainWindowLeft">The left position of the main window.</param>
        /// <param name="mainWindowTop">The top position of the main window.</param>
        public void UpdatePosition(double mainWindowLeft, double mainWindowTop)
        {
            Left = mainWindowLeft + (Owner.Width - Width) / 2;
            Top = mainWindowTop + (Owner.Height - Height) / 2;
        }
        
        
        /// <summary>
        /// Closes the loading window after a specified delay.
        /// </summary>
        /// <param name="delay">The delay in milliseconds before closing the window.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task CloseAfterDelay(int delay)
        {
            await Task.Delay(delay).ConfigureAwait(false);
            Dispatcher.Invoke(Close);
            //_closeCompletionSource.SetResult(true);
        }

        
        /// <summary>
        /// Updates the text of the task name displayed in the loading window.
        /// </summary>
        /// <param name="taskName">The name of the current task.</param>
        public void UpdateTaskName(string taskName)
        {
            Dispatcher.Invoke(() => { TaskNameText.Text = taskName; });
        }

        
        /// <summary>
        /// Logs a new activity to the loading window's activity log.
        /// </summary>
        /// <param name="activity">The activity description to log.</param>
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

        
        /// <summary>
        /// Completes the current activity by updating the progress bar to show completion.
        /// </summary>
        public void CompleteActivity()
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Style = (Style)FindResource("GreenProgressBar");
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = 100;
            });
        }

        
        /// <summary>
        /// Marks the last logged activity as completed.
        /// </summary>
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

        
        /// <summary>
        /// Applies light mode color settings to the loading window.
        /// </summary>
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
        
        
        /// <summary>
        /// Applies dark mode color settings to the loading window.
        /// </summary>
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
        /// <summary>
        /// Gets or sets the text of the activity.
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the activity is completed.
        /// </summary>
        public bool IsCompleted { get; set; }
        
        /// <summary>
        /// Gets or sets the background color of the activity.
        /// </summary>
        public string Background { get; set; }
        
        /// <summary>
        /// Gets or sets the foreground color of the activity.
        /// </summary>
        public string Foreground { get; set; }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a <see cref="Visibility"/> enumeration value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">Additional parameters for the conversion.</param>
        /// <param name="culture">Culture information for the conversion.</param>
        /// <returns>A <see cref="Visibility"/> value based on the boolean value.</returns>
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
