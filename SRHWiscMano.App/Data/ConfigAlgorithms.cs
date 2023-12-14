using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.App.Data
{
    public class ConfigAlgorithms
    {
        public int VPSampleRange { get; set; } = 1; // 클릭 센서 + 위아래 각 1개 추가
        public int VPDuration { get; set; } = 1000; // 클릭 time 앞뒤로 1초
    }
}
