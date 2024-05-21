using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MahApps.Metro.Converters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Shouldly;
using SRHWiscMano.App.Data;
using SRHWiscMano.Core.Data;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.Services
{
    public class SharedService
    {
        private readonly ILogger<SharedService> logger;
        private readonly AppSettings settings;

        /// <summary>
        /// 분석할 TimeSample 및 TimeNote 데이터를 갖고 있는 객체, 모든 viewmodel에 연동됨.
        /// </summary>
        public IExamination? ExamData { get; private set; }

        public IReadOnlyList<TimeSample> InterpolatedSamples => ExamData.InterpolatedSamples;

        public IExamMetaData? ExamMetaData { get; private set; }

        /// <summary>
        /// Notes 정보를 기준으로 일정 영역을 분리하여 볼 수 있도록 하는 데이터
        /// </summary>
        public SourceCache<ITimeFrame, int> TimeFrames { get; } = new(item => item.Id);

        public event EventHandler? ExamDataLoaded;
        public event EventHandler? ExamMetaDataLoaded;

        public SharedService(ILogger<SharedService> logger, IOptions<AppSettings> settings)
        {
            this.logger = logger;
            this.settings = settings.Value;

            WeakReferenceMessenger.Default.Register<SensorBoundsChangedMessage>(this, SensorBoundsChanged);
        }

        

        public async Task SetExamData(IExamination data, double interpolateScale = 1)
        {
            logger.LogInformation("New ExamData is registered");

            this.ExamData = data;
            await Task.Run(() => ExamData.UpdateInterpolation(settings.InterpolateSensorScale));

            TimeFrames.Clear();

            //ExamData 에서 로드한 Note를 정보를 FrameNote sourcelist에 입력한다.
            foreach (var note in ExamData.Notes)
            {
                // TimeFrames.AddOrUpdate(CreateTimeFrame(note.Text, note.Time));
                TimeFrames.AddOrUpdate(CreateTimeFrame(note));
            }
            
            ExamDataLoaded?.Invoke(this, EventArgs.Empty);
        }

        public TimeFrame CreateTimeFrame(string text, Instant time)
        {
            ExamData.ShouldNotBeNull("Examdata should be loaded first");
            return new TimeFrame(text, time, settings.TimeFrameDurationInMillisecond, ExamData);
        }

        public TimeFrame CreateTimeFrame(FrameNote note)
        {
            ExamData.ShouldNotBeNull("Examdata should be loaded first");
            return new TimeFrame(note, settings.TimeFrameDurationInMillisecond, ExamData);
        }

        /// <summary>
        /// Sensor Bounds를 변경되었을 때 TimeFrames 에도 이를 반영한다.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="message"></param>
        private void SensorBoundsChanged(object recipient, SensorBoundsChangedMessage message)
        {
            foreach (var item in TimeFrames.Items)
            {
                TimeFrames.Edit(updater =>
                {
                    item.UpdateSensorBounds(message.Value.MinBound, message.Value.MaxBound);
                    updater.AddOrUpdate(item);
                });
            }
        }

        public void SetExamMetaData(IExamMetaData data)
        {
            this.ExamMetaData = data;
            ExamMetaDataLoaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
