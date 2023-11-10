using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;

namespace SRHWiscMano.Core.ViewModels
{
    /// <summary>
    /// 센서 데이터를 View에 표시하기 위한 ViewModel
    /// </summary>
    public interface IExamViewModel
    {
        IExamination ExamData { get; }
        IExamMetaData ExamMetaData { get; }
    }
}
