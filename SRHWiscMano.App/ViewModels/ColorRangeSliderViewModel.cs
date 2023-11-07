using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.App.ViewModels
{
    using OxyPlot;
    using System.ComponentModel;
    using System.Windows.Media;

    public class ColorRangeSliderViewModel : INotifyPropertyChanged
    {
        private OxyColor _lowColor = OxyColors.Red;
        private OxyColor _highColor = OxyColors.Blue;
        private double _minimum = 0;
        private double _maximum = 100;
        private double _value = 50;
        private double _tickFrequency = 10;

        public event PropertyChangedEventHandler PropertyChanged;

        // Convert OxyColor to System.Windows.Media.Color
        public Color LowColor => OxyColorToMediaColor(_lowColor);
        public Color HighColor => OxyColorToMediaColor(_highColor);

        public double Minimum
        {
            get => _minimum;
            set
            {
                if (_minimum != value)
                {
                    _minimum = value;
                    OnPropertyChanged(nameof(Minimum));
                }
            }
        }

        public double Maximum
        {
            get => _maximum;
            set
            {
                if (_maximum != value)
                {
                    _maximum = value;
                    OnPropertyChanged(nameof(Maximum));
                }
            }
        }

        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                    UpdateColors();
                }
            }
        }

        public double TickFrequency
        {
            get => _tickFrequency;
            set
            {
                if (_tickFrequency != value)
                {
                    _tickFrequency = value;
                    OnPropertyChanged(nameof(TickFrequency));
                }
            }
        }

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
