using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.Services
{
    public class SharedService
    {
        private readonly ILogger<SharedService> logger;
        public IExamination? ExamData { get; private set; }

        public IExamMetaData? ExamMetaData { get; private set; }


        public event EventHandler? ExamDataLoaded;
        public event EventHandler? ExamMetaDataLoaded;

        public SharedService(ILogger<SharedService> logger)
        {
            this.logger = logger;
        }

        public void SetExamData(IExamination data)
        {
            logger.LogInformation("New ExamData is registered");

            this.ExamData = data;
            ExamDataLoaded?.Invoke(this, EventArgs.Empty);
        }

        public void SetExamMetaData(IExamMetaData data)
        {
            this.ExamMetaData = data;
            ExamMetaDataLoaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
