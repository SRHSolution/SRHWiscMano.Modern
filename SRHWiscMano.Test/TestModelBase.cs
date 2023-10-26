using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Test
{
    internal class TestModelBase
    {
        public static string DirTestData = VisualStudioProvider.GetPathInSolution("TestData");
        public static string DirOutput = VisualStudioProvider.GetPathInSolution("TestData/Output");

        public string LoadTestData(string fileName)
        {
            return Path.Combine(DirTestData, fileName);
        }

        public string SaveOutput(string fileName)
        {
            return Path.Combine(DirOutput, fileName);
        }
    }
}
