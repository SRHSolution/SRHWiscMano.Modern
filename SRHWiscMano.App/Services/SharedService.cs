using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App.Services
{
    public class SharedService
    {
        public ITimeSeriesData? ExamData { get; private set; }

        public IExamMetaData? ExamMetaData { get; private set; }


        public event EventHandler? ExamDataLoaded;
        public event EventHandler? ExamMetaDataLoaded;

        public void SetExamData(ITimeSeriesData data)
        {
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
