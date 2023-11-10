using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.ViewModels
{
    using OxyPlot;
    using System.ComponentModel;
    using System.Windows.Media;

    public partial class ColorRangeSliderViewModel : ViewModelBase
    {
        private OxyColor _lowColor = OxyColors.Red;
        private OxyColor _highColor = OxyColors.Blue;
        private double _minimum = 0;
        private double _maximum = 100;
        private double _value = 50;
        private double _tickFrequency = 10;

        // Convert OxyColor to System.Windows.Media.Color
        public Color LowColor => OxyColorToMediaColor(_lowColor);
    
        public Color HighColor => OxyColorToMediaColor(_highColor);

        [ObservableProperty] private double minimum;

        [ObservableProperty] private double maximum;

        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    SetProperty(ref _value, value);
                    UpdateColors();
                }
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
            _lowColor = palette.Colors[0];
            _highColor = palette.Colors[(int)(_value / _maximum * (palette.Colors.Count - 1))];
            OnPropertyChanged(nameof(LowColor));
            OnPropertyChanged(nameof(HighColor));
        }

        private static Color OxyColorToMediaColor(OxyColor oxyColor)
        {
            return Color.FromArgb(oxyColor.A, oxyColor.R, oxyColor.G, oxyColor.B);
        }
    }
}
