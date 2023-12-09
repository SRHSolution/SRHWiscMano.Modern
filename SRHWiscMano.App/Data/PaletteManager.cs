using OxyPlot;
using SRHWiscMano.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SRHWiscMano.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRHWiscMano.App.Services;
using SRHWiscMano.App.ViewModels;

namespace SRHWiscMano.App.Data
{
    [Singleton]
    public class PaletteManager
    {
        private readonly AppSettings settings;
        public Dictionary<string, OxyPalette> Palettes { get; } = new();


        private string selectedPaletteKey;
        public string SelectedPaletteKey
        {
            get => selectedPaletteKey;
            set
            {
                selectedPaletteKey = value;
                settings.SelectedPaletteKey = value;
            }
        }

        public OxyPalette SelectedPalette { get; set; }

        public PaletteManager(ILogger<ExplorerViewModel> logger, IOptions<AppSettings> settings)
        {
            this.settings = settings.Value;

            ExtendPalettes();
            Palettes.AddRange(PaletteUtils.GetPredefinedPalettes());

            SelectedPalette = Palettes["Mid"];
        }

        public void ExtendPalettes()
        {
            var midColors = new[]
            {
                OxyColor.FromArgb(255, 24, 3, 95),
                OxyColor.FromArgb(255, 30, 237, 215),
                OxyColor.FromArgb(255, 47, 243, 38),
                OxyColor.FromArgb(255, 248, 248, 1),
                OxyColor.FromArgb(255, 253, 5, 0),
                OxyColor.FromArgb(255, 95, 0, 69)
            };
            var palette = OxyPalette.Interpolate(200, midColors);
            Palettes.Add("Mid", palette);
        }
    }
}
