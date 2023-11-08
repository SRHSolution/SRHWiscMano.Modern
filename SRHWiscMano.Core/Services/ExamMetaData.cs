using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.Core.Services
{
    public partial class ExamMetaData : ViewModelBase, IExamMetaData
    {
        public IPatient PatientData { get; }
    }
}
