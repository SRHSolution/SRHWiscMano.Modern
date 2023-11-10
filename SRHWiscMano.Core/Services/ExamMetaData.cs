using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.Core.Services
{
    public partial class ExamMetaData : ViewModelBase, IExamMetaData
    {
        public IPatient PatientData { get; }
    }
}
