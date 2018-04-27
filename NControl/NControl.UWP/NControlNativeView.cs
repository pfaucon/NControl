using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NControl.UWP
{
    public class NControlNativeView : Grid
    {
        private Rectangle _rect;

        public NControlNativeView()
        {
            _rect = new Rectangle();
            Children.Add(_rect);
        }

        public Brush Fill
        {
            set
            {
                _rect.Fill = value;
            }
        }
        
    }
}
