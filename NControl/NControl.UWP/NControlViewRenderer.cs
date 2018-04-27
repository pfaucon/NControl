using NControl.Abstractions;
using NControl.UWP;
using NGraphics.UWP;
using System;
using System.IO;
using System.Linq;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(NControlView), typeof(NControlViewRenderer))]
namespace NControl.UWP
{
    /// <summary>
    /// NControlView renderer.
    /// </summary>
    public class NControlViewRenderer : ViewRenderer<NControlView, NControlNativeView>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NControlViewRenderer() : base()
        {
            PointerPressed += OnPointerPressed;
            PointerMoved += OnPointerMoved;
            PointerReleased += OnPointerReleased;
        }

        /// <summary>
        /// Raises the element changed event.
        /// </summary>
        /// <param name="e">E.</param>
        protected override void OnElementChanged(ElementChangedEventArgs<NControlView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
                e.OldElement.OnInvalidate -= HandleInvalidate;

            if (e.NewElement != null)
            {
                e.NewElement.OnInvalidate += HandleInvalidate;
            }

            // handle the situation when OnElementChanged is called while the view is going to be removed
            if ((e.OldElement != null) && (e.NewElement == null) && (Control == null))
                return;

            if (Control == null)
            {
                var nativeControl = new NControlNativeView()
                {
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch
                };
                nativeControl.SizeChanged += delegate { RedrawControl(); };
                SetNativeControl(nativeControl);
                //UpdateClip();
                UpdateInputTransparent();
            }

            RedrawControl();
        }

        /// <summary>
        /// Redraw when background color changes
        /// </summary>
        protected override void UpdateBackgroundColor()
        {
            base.UpdateBackgroundColor();

            RedrawControl();
        }

        /// <summary>
        /// Raises the element property changed event.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Control == null)
                return;

            if (e.PropertyName == VisualElement.HeightProperty.PropertyName ||
                e.PropertyName == VisualElement.WidthProperty.PropertyName ||
                e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
                RedrawControl();
            else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
                UpdateInputTransparent();
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            // Get the primary touch point. We do not track multitouch at the moment.
            var primaryTouchPoint = pointerRoutedEventArgs.GetCurrentPoint(Windows.UI.Xaml.Window.Current.Content);

            var uiElements = VisualTreeHelper.FindElementsInHostCoordinates(primaryTouchPoint.Position,
                Windows.UI.Xaml.Window.Current.Content);
            foreach (var uiElement in uiElements)
            {
                // Are we interested?
                var renderer = uiElement as NControlViewRenderer;
                if (renderer == null)
                    continue;

                // Get NControlView element
                var element = renderer.Element;

                // Get this' position on screen
                var transform = Windows.UI.Xaml.Window.Current.Content.TransformToVisual(renderer.Control);

                // Transform touches
                var touchPoints = pointerRoutedEventArgs.GetIntermediatePoints(Windows.UI.Xaml.Window.Current.Content);
                var touches = touchPoints
                    .Select(t => transform.TransformPoint(new Windows.Foundation.Point(t.Position.X, t.Position.Y)))
                    .Select(t => new NGraphics.Point(t.X, t.Y)).ToList();

                if (element.TouchesBegan(touches))
                    break;
            }
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            // Get the primary touch point. We do not track multitouch at the moment.
            var primaryTouchPoint = pointerRoutedEventArgs.GetCurrentPoint(Windows.UI.Xaml.Window.Current.Content);

            var uiElements = VisualTreeHelper.FindElementsInHostCoordinates(primaryTouchPoint.Position,
                Windows.UI.Xaml.Window.Current.Content);
            foreach (var uiElement in uiElements)
            {
                // Are we interested?
                var renderer = uiElement as NControlViewRenderer;
                if (renderer == null)
                    continue;

                // Get NControlView element
                var element = renderer.Element;

                // Get this' position on screen
                var transform = Windows.UI.Xaml.Window.Current.Content.TransformToVisual(renderer.Control);

                // Transform touches
                var touchPoints = pointerRoutedEventArgs.GetIntermediatePoints(Windows.UI.Xaml.Window.Current.Content);
                var touches = touchPoints
                    .Select(t => transform.TransformPoint(new Windows.Foundation.Point(t.Position.X, t.Position.Y)))
                    .Select(t => new NGraphics.Point(t.X, t.Y)).ToList();

                if (element.TouchesMoved(touches))
                    break;
            }
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            // Get the primary touch point. We do not track multitouch at the moment.
            var primaryTouchPoint = pointerRoutedEventArgs.GetCurrentPoint(Windows.UI.Xaml.Window.Current.Content);

            var uiElements = VisualTreeHelper.FindElementsInHostCoordinates(primaryTouchPoint.Position,
                Windows.UI.Xaml.Window.Current.Content);
            foreach (var uiElement in uiElements)
            {
                // Are we interested?
                var renderer = uiElement as NControlViewRenderer;
                if (renderer == null)
                    continue;

                // Get NControlView element
                var element = renderer.Element;

                // Get this' position on screen
                var transform = Windows.UI.Xaml.Window.Current.Content.TransformToVisual(renderer.Control);

                // Transform touches
                var touchPoints = pointerRoutedEventArgs.GetIntermediatePoints(Windows.UI.Xaml.Window.Current.Content);
                var touches = touchPoints
                    .Select(t => transform.TransformPoint(new Windows.Foundation.Point(t.Position.X, t.Position.Y)))
                    .Select(t => new NGraphics.Point(t.X, t.Y)).ToList();

                if (element.TouchesEnded(touches))
                    break;
            }
        }

        #region Drawing

        /// <summary>
        /// Redraws the control by clearing the canvas element and adding new elements
        /// </summary>
        private void RedrawControl()
        {
            if (Element.Width.Equals(-1) || Element.Height.Equals(-1))
                return;


            if (Control == null)
                return;

            var width = Convert.ToInt32(Control.ActualWidth);
            var height = Convert.ToInt32(Control.ActualHeight);
            if (width == 0 || height == 0)
                return;

            var canvas = Platforms.Current.CreateImageCanvas(new NGraphics.Size(width, height));
            Element.Draw(canvas, new NGraphics.Rect(0, 0, width, height));
            var bitmapSource = new BitmapImage();
            var stream = new MemoryStream();
            canvas.GetImage().SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);
            bitmapSource.SetSource(stream.AsRandomAccessStream());
            Control.Fill = new ImageBrush
            {
                ImageSource = bitmapSource
            };
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Updates the IsHitTestVisible property on the native control
        /// </summary>
        private void UpdateInputTransparent()
        {
            Control.IsHitTestVisible = !Element.InputTransparent;
        }

        /// <summary>
        /// Handles the invalidate.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Arguments.</param>
        private void HandleInvalidate(object sender, System.EventArgs args)
        {
            // Invalidate control
            RedrawControl();
        }

        #endregion
    }
}
