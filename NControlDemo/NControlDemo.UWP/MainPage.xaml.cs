using NControlDemo.FormsApp.Mvvm;
using NControlDemo.UWP.Platform.IoC;
using NControlDemo.UWP.Platform.Mvvm;

namespace NControlDemo.UWP
{
    /// <summary>
    /// Main page of the UWP demo.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new NControlDemo.FormsApp.App(new ContainerProvider(), (container) =>
            {
                // Register providers
                container.Register<IImageProvider, ImageProvider>();
            }));
        }
    }
}
