using System.Collections.ObjectModel;
using System.ComponentModel;
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
                var newActivity = new Activity
                {
                    Text = activity,
                    Background = "Transparent",
                    Foreground = App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.EnableLightTheme
                        ? "#000000" : "#FFFFFF",
                    StartTime = DateTime.Now,
                    IsInProgress = true
                };

                newActivity.StartTrackingDuration(); // Commence à suivre la durée

                Activities.Add(newActivity);
                ActivityLog.ScrollIntoView(newActivity);
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
                if (Activities.Count == 0) return;
        
                Activities.Last().MarkComplete(); // Marque l'activité comme complétée
        
                ActivityLog.Items.Refresh();
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

    public class Activity : INotifyPropertyChanged
{
    private string text;
    private bool isCompleted;
    private string background;
    private string foreground;
    private string duration;
    private DateTime startTime;
    private DispatcherTimer timer;
    private bool isInProgress;
    
    public bool IsInProgress
    {
        get => isInProgress;
        set
        {
            isInProgress = value;
            OnPropertyChanged(nameof(IsInProgress));
            OnPropertyChanged(nameof(IsInProgressVisibility)); // Met à jour la visibilité en fonction de IsInProgress
        }
    }

    public Visibility IsInProgressVisibility => IsInProgress && !IsCompleted ? Visibility.Visible : Visibility.Hidden;


    public string Text
    {
        get => text;
        set
        {
            text = value;
            OnPropertyChanged(nameof(Text));
        }
    }

    public bool IsCompleted
    {
        get => isCompleted;
        set
        {
            isCompleted = value;
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(IsCompletedVisibility)); // Met à jour la visibilité en fonction de IsCompleted
        }
    }

    public string Background
    {
        get => background;
        set
        {
            background = value;
            OnPropertyChanged(nameof(Background));
        }
    }

    public string Foreground
    {
        get => foreground;
        set
        {
            foreground = value;
            OnPropertyChanged(nameof(Foreground));
        }
    }

    public string Duration
    {
        get => duration;
        set
        {
            duration = value;
            OnPropertyChanged(nameof(Duration));
        }
    }

    public DateTime StartTime
    {
        get => startTime;
        set
        {
            startTime = value;
            OnPropertyChanged(nameof(StartTime));
        }
    }

    public DispatcherTimer Timer
    {
        get => timer;
        set
        {
            timer = value;
            OnPropertyChanged(nameof(Timer));
        }
    }

    public Visibility IsCompletedVisibility => IsCompleted ? Visibility.Visible : Visibility.Hidden;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Met à jour la durée en fonction du temps écoulé
    private void UpdateDuration()
    {
        var elapsed = DateTime.Now - StartTime;
        
        // Affiche "00:00" si le temps écoulé est inférieur à une seconde
        if (elapsed < TimeSpan.FromMilliseconds(100))
        {
            Duration = "00:00";
        }
        else
        {
            Duration = $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";
        }
    }

    // Méthode publique pour démarrer le suivi de la durée
    public void StartTrackingDuration()
    {
        Timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1) // Mise à jour toutes les secondes
        };
        Timer.Tick += (sender, args) =>
        {
            UpdateDuration(); // Met à jour la durée en temps réel
        };
        Timer.Start();

        UpdateDuration(); // Met à jour la durée initiale
    }

    // Méthode publique pour marquer l'activité comme complétée
    public void MarkComplete()
    {
        IsCompleted = true;
        Timer?.Stop();
        UpdateDuration(); // Met à jour la durée une dernière fois
    }
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
