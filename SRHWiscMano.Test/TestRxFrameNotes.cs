using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using MathNet.Numerics;
using MoreLinq;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;

namespace SRHWiscMano.Test
{
    [TestFixture]
    internal class TestRxFrameNotes : TestModelBase
    {
        [OneTimeSetUp]
        public void SetupOneTime()
        {
            Console.WriteLine($"{this.GetType().Namespace}");
        }


        [Test]
        public void RxFrameNotes()
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name}");

            ExamDataTextReader reader = new ExamDataTextReader();
            var examData = reader.ReadFromFile(LoadTestData("100.txt"));

            IEnumerable<int> intEnums = Enumerable.Range(0, 10);
            var dd = intEnums.Skip(3).Take(5);

            // IObservable<FrameNote> frameNotes = examData.Notes.ToObservable();
            // frameNotes.Subscribe((fn) => { }, ex => { }, () => { });
            

            
        }
    }
}
