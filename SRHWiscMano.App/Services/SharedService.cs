using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using MahApps.Metro.Converters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Shouldly;
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

        public SourceList<TimeFrame> TimeFrames { get; }= new();

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

            TimeFrames.Clear();

            //ExamData 에서 로드한 Note를 정보를 FrameNote sourcelist에 입력한다.
            foreach (var note in ExamData.Notes)
            {
                TimeFrames.Add(CreateTimeFrame(note.Text, note.Time));
            }

            ExamDataLoaded?.Invoke(this, EventArgs.Empty);
        }

        public TimeFrame CreateTimeFrame(string text, Instant time)
        {
            ExamData.ShouldNotBeNull("Examdata should be loaded first");
            return new TimeFrame(text, time, settings.TimeFrameDurationInMillisecond, ExamData.PlotData);
        }


        public void SetExamMetaData(IExamMetaData data)
        {
            this.ExamMetaData = data;
            ExamMetaDataLoaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
