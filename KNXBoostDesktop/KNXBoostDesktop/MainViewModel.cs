using System.ComponentModel;

namespace KNXBoostDesktop
{
    /// <summary>
    /// ViewModel class that implements INotifyPropertyChanged to notify listeners of property changes.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isProjectImported;

        /// <summary>
        /// Gets or sets a value indicating whether a project has been imported.
        /// </summary>
        public bool IsProjectImported
        {
            get => _isProjectImported;
            set 
            { 
                _isProjectImported = value; 
                OnPropertyChanged(nameof(IsProjectImported));
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for a given property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}