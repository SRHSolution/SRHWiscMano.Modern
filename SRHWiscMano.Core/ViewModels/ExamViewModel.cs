using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.ViewModels
{
    /// <summary>
    /// 센서 데이터를 View에 표시하기 위한 ViewModel
    /// </summary>
    public partial class ExamViewModel : ViewModelBase, IExamViewModel
    {
        public IExamination ExamData { get; }
        public IExamMetaData ExamMetaData { get; }
    }
}
