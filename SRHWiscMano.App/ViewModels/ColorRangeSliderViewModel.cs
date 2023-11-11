using CommunityToolkit.Mvvm.ComponentModel;
using SRHWiscMano.Core.ViewModels;
using OxyPlot;
using System.Windows.Media;

namespace SRHWiscMano.App.ViewModels
{
    /// <summary>
    /// Heatmap 에 적용하는 Palette를 제어하는 Slider ViewModel
    /// </summary>
    public partial class ColorRangeSliderViewModel : ViewModelBase
    {
        [ObservableProperty] private Color lowColor = OxyColorToMediaColor(OxyColors.Red);
        [ObservableProperty] private Color highColor = OxyColorToMediaColor(OxyColors.Blue);

        /// <summary>
        /// 센서 Range Minimum
        /// </summary>
        [ObservableProperty] private double minimum;

        /// <summary>
        /// 센서 Range Maximum
        /// </summary>
        [ObservableProperty] private double maximum;

        private double _value = 50;
        public double Value
        {
            get => _value;
            set
            {
                SetProperty(ref _value, value);
                UpdateColors();
            }
        }

        [ObservableProperty] private double tickFrequency;

        public ColorRangeSliderViewModel()
        {
            // Initialize colors
            UpdateColors();
        }

        private void UpdateColors()
        {
            var palette = OxyPalettes.Jet(200); // or any other palette
            var lowColor = palette.Colors[0];
            var highColor = palette.Colors[(int)(_value / (Maximum- Minimum) * (palette.Colors.Count - 1))];

            LowColor = OxyColorToMediaColor(lowColor);
            HighColor = OxyColorToMediaColor(highColor);
        }

        /// <summary>
        /// Convert OxyColor to System.Windows.Media.Color
        /// </summary>
        /// <param name="oxyColor"></param>
        /// <returns></returns>
        private static Color OxyColorToMediaColor(OxyColor oxyColor)
        {
            return Color.FromArgb(oxyColor.A, oxyColor.R, oxyColor.G, oxyColor.B);
        }
    }
}
