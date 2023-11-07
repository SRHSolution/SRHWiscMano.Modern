using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.SkiaSharp;
using SRHWiscMano.Core.Helpers;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using SkiaSharp;
using System.Windows.Media.Media3D;

namespace SRHWiscMano.Test
{
    [TestFixture, Apartment(ApartmentState.STA)]
    internal class TestOxyPalette : TestModelBase
    {

        [OneTimeSetUp]
        public void SetupViewModel()
        {
            Ioc.Default.ConfigureServices(ConfigureServices());
        }
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            // services.AddSingleton<IViewerPage, ViewerPage>();    
            var provider = services.BuildServiceProvider();
            return provider;
        }

        /// <summary>
        /// Palette의 색정보를 image로 rendering 한뒤에 이를 bitmap 이미지로 변환하여 파일로 저장한다.
        /// </summary>
        [Test]
        public void DrawOxyImage()
        {
            var palette = OxyPalettes.BlackWhiteRed(200);
            var paletteImage = palette.GenerateColorAxisImage(false, false);

            var resizedImg = paletteImage.GetResizedImage(20, 300);


            // Create a frame from the BitmapImage
            BitmapFrame frame = BitmapFrame.Create(resizedImg);

            // Create a JpegBitmapEncoder and add the frame to the encoder
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(frame);

            // Save the image to a file
            using (FileStream stream = new FileStream("output3.jpg", FileMode.Create))
            {
                encoder.Save(stream);
            }

        }

    }
}
