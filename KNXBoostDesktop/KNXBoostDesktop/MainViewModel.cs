using System.ComponentModel;

namespace KNXBoostDesktop
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isProjectImported;

        public bool IsProjectImported
        {
            get => _isProjectImported;
            set 
            { 
                _isProjectImported = value; 
                OnPropertyChanged(nameof(IsProjectImported));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}