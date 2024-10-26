using System.Windows.Media;
using Caliburn.Micro;
using static Allusion.WPFCore.Service.StaticLogger;

namespace Allusion.ViewModels
{
    public class NotificationViewModel : PropertyChangedBase
    {
        private string _message;
        private Brush _background;
        private Brush _borderBrush;

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        public Brush Background
        {
            get => _background;
            set
            {
                _background = value;
                NotifyOfPropertyChange(() => Background);
            }
        }

        public Brush BorderBrush
        {
            get => _borderBrush;
            set
            {
                _borderBrush = value;
                NotifyOfPropertyChange(() => BorderBrush);
            }
        }

        public NotificationViewModel(string message, LogLevel logLevel)
        {
            Message = message;
            SetColors(logLevel);
            AutoDismiss(); // Start the auto-dismiss logic when instantiated
        }

        private void SetColors(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    Background = Brushes.DarkOliveGreen;
                    BorderBrush = Brushes.DarkSeaGreen;
                    break;
                case LogLevel.Warning:
                    Background = Brushes.Orange;
                    BorderBrush = Brushes.Gold;
                    break;
                case LogLevel.Error:
                    Background = Brushes.IndianRed;
                    BorderBrush = Brushes.Red;
                    break;
                default:
                    Background = Brushes.LightGray;
                    BorderBrush = Brushes.Gray;
                    break;
            }
        }

        private async void AutoDismiss()
        {
            await Task.Delay(3000); // Show for 3 seconds
            
        }
    }
}
