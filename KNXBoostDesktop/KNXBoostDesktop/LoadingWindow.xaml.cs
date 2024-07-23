using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls;


namespace KNXBoostDesktop
{
    public partial class LoadingWindow
    {
        /// <summary>
        /// Gets the collection of activities displayed in the loading window.
        /// </summary>
        private ObservableCollection<Activity> Activities { get; }

        private CancellationTokenSource _cancellationTokenSource;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingWindow"/> class.
        /// Sets up the progress bar and initializes the activity's collection.
        /// </summary>
        public LoadingWindow(CancellationTokenSource cancellationTokenSource)
        {
            InitializeComponent();

            ProgressBar.Style = (Style)FindResource("NormalProgressBar");
            ProgressBar.IsIndeterminate = true;

            Activities = new ObservableCollection<Activity>();
            ActivityLog.ItemsSource = Activities;
            
            ApplyScaling(App.DisplayElements!.SettingsWindow!.AppScaleFactor/100f);
            _cancellationTokenSource = cancellationTokenSource;
            this.Closing += LoadingWindow_Closing;
        }
        
        private void CloseLoading(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        
        

        private void LoadingWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _cancellationTokenSource.Cancel();
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
                        ? "#000000" : "#E3DED4",
                    StartTime = DateTime.Now,
                    IsInProgress = true
                };

                newActivity.StartTrackingDuration(); // Commence à suivre la durée

                Activities.Add(newActivity);
                ActivityLog.ScrollIntoView(newActivity);
            });
        }

        public void UpdateLogActivity(int index, string activity)
        {
            Dispatcher.Invoke(() =>
            {
                if (index < 0 || index >= Activities.Count)
                    return;

                var existingActivity = Activities[index];
                existingActivity.Text = activity;
                ActivityLog.ScrollIntoView(existingActivity);
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
            ProgressBar.BorderBrush = MainWindow.ConvertStringColor("#D7D7D7");
            TaskNameText.Foreground = MainWindow.ConvertStringColor("#000000");
            ActivityLog.Background = MainWindow.ConvertStringColor("#F5F5F5");
            ProgressBar.Background = MainWindow.ConvertStringColor("#FFFFFF");
            ActivityLog.Foreground = MainWindow.ConvertStringColor("#000000");
            ActivityLog.BorderBrush = MainWindow.ConvertStringColor("#D7D7D7");
            ActivityLog.ItemContainerStyle = (Style)FindResource("LightActivityStyle");
            TotalTime.Foreground = MainWindow.ConvertStringColor("#000000");
        }
        
        
        /// <summary>
        /// Applies dark mode color settings to the loading window.
        /// </summary>
        public void SetDarKMode()
        {
            MainGrid.Background = MainWindow.ConvertStringColor("#313131");
            ProgressBar.BorderBrush = MainWindow.ConvertStringColor("#434343");
            TaskNameText.Foreground = MainWindow.ConvertStringColor("#E3DED4");
            ActivityLog.Background = MainWindow.ConvertStringColor("#262626");
            ProgressBar.Background = MainWindow.ConvertStringColor("#262626");
            ActivityLog.Foreground = MainWindow.ConvertStringColor("#E3DED4");
            ActivityLog.BorderBrush = MainWindow.ConvertStringColor("#434343");
            ActivityLog.ItemContainerStyle = (Style)FindResource("DarkActivityStyle");
            TotalTime.Foreground = MainWindow.ConvertStringColor("#E3DED4");
        }

        private void ApplyScaling(float scale)
        {
            LoadingWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
            
            Height = Height * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : Height * scale;
            Width = Width * scale > 0.9*SystemParameters.PrimaryScreenWidth ? 0.9*SystemParameters.PrimaryScreenWidth : Width * scale;
            
        }
    }

    public class Activity : INotifyPropertyChanged
{
    private string _text = null!;
    private bool _isCompleted;
    private string _background = null!;
    private string _foreground = null!;
    private string _duration = null!;
    private DateTime _startTime;
    private DispatcherTimer _timer = null!;
    private bool _isInProgress;
    
    public bool IsInProgress
    {
        get => _isInProgress;
        init
        {
            _isInProgress = value;
            OnPropertyChanged(nameof(IsInProgress));
            OnPropertyChanged(nameof(IsInProgressVisibility)); // Met à jour la visibilité en fonction de IsInProgress
        }
    }

    public Visibility IsInProgressVisibility => IsInProgress && !IsCompleted ? Visibility.Visible : Visibility.Hidden;


    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            OnPropertyChanged(nameof(Text));
        }
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            _isCompleted = value;
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(IsCompletedVisibility)); // Met à jour la visibilité en fonction de IsCompleted
            OnPropertyChanged(nameof(IsNotCompletedVisibility)); // Met à jour la visibilité en fonction de IsCompleted
            
        }

    }

    public string Background
    {
        get => _background;
        set
        {
            _background = value;
            OnPropertyChanged(nameof(Background));
        }
    }

    public string Foreground
    {
        get => _foreground;
        set
        {
            _foreground = value;
            OnPropertyChanged(nameof(Foreground));
        }
    }

    public string Duration
    {
        get => _duration;
        set
        {
            _duration = value;
            OnPropertyChanged(nameof(Duration));
        }
    }

    public DateTime StartTime
    {
        get => _startTime;
        init
        {
            _startTime = value;
            OnPropertyChanged(nameof(StartTime));
        }
    }

    public DispatcherTimer Timer
    {
        get => _timer;
        set
        {
            _timer = value;
            OnPropertyChanged(nameof(Timer));
        }
    }

    public Visibility IsCompletedVisibility => IsCompleted ? Visibility.Visible : Visibility.Hidden;
    public Visibility IsNotCompletedVisibility => IsCompleted ? Visibility.Hidden : Visibility.Visible;

        public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Met à jour la durée en fonction du temps écoulé
    private void UpdateDuration()
    {
        var elapsed = DateTime.Now - StartTime;
        
        // Affiche "00:00" si le temps écoulé est inférieur à une seconde
        Duration = elapsed < TimeSpan.FromMilliseconds(100) ? "00:00" : $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";
    }

    // Méthode publique pour démarrer le suivi de la durée
    public void StartTrackingDuration()
    {
        Timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1) // Mise à jour toutes les secondes
        };
        Timer.Tick += (_, _) =>
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
        Timer.Stop();
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
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
