using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.Core.Models
{
    /// <summary>
    /// Exmination 데이터에 대한 Meta 정보 클래스
    /// </summary>
    public partial class ExamMetaData : IExamMetaData
    {
        public IPatient PatientData { get; }
    }
}
