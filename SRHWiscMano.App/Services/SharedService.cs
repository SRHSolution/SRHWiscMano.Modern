using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Converters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRHWiscMano.App.Data;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.Services
{
    public class SharedService
    {
        private readonly ILogger<SharedService> logger;
        private readonly AppSettings settings;
        public IExamination? ExamData { get; private set; }

        public IExamMetaData? ExamMetaData { get; private set; }


        public event EventHandler? ExamDataLoaded;
        public event EventHandler? ExamMetaDataLoaded;

        public SharedService(ILogger<SharedService> logger, IOptions<AppSettings> settings)
        {
            this.logger = logger;
            this.settings = settings.Value;
        }

        public async Task SetExamData(IExamination data)
        {
            logger.LogInformation("New ExamData is registered");

            this.ExamData = data;
            await ExamData.UpdatePlotData(settings.InterpolateSensorScale);

            ExamDataLoaded?.Invoke(this, EventArgs.Empty);
        }

        public void SetExamMetaData(IExamMetaData data)
        {
            this.ExamMetaData = data;
            ExamMetaDataLoaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
