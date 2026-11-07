using System.Windows.Media;
using Caliburn.Micro;

namespace Allusion.ViewModels
{
    public class GroupViewModel : PropertyChangedBase
    {
        public string Title { get; set; } = "no name";
        public int BorderColorIndex { get; set; } = 15;
        public Brush GroupBrush { get; set; } = Brushes.Gray;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; } = 320;
        public double Height { get; set; } = 220;
        public BindableCollection<ImageViewModel> Images { get; set; } = new();

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                NotifyOfPropertyChange(nameof(IsSelected));
            }
        }
    }
}
