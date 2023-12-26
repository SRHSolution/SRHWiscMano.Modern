using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal.Execution;

namespace SRHWiscMano.Test
{
    [TestFixture]
    internal class TestRx
    {
        [OneTimeSetUp]
        public void SetupOneTime()
        {
            Console.WriteLine($"{this.GetType().Namespace}");
        }

        public IEnumerable<int> GetNumbers()
        {
            for (int i = 1; i <= 5; i++)
            {
                Thread.Sleep(1000); // Simulating delay
                yield return i;
            }
        }

        [Test]
        public void TestYieldReturn()
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name}");
            var watch = Stopwatch.StartNew();
            GetNumbers();
            Console.WriteLine($"call enumerable {watch.ElapsedMilliseconds}");

            GetNumbers().ElementAt(3);
            Console.WriteLine($"call element {watch.ElapsedMilliseconds}");
            GetNumbers().ToList();
            Console.WriteLine($"call tolist {watch.ElapsedMilliseconds}");

            foreach (var number in GetNumbers())
            {
                Console.WriteLine($"Received: {number}, {watch.ElapsedMilliseconds}");
            }
            watch.Stop();
        }


        public IObservable<int> GetNumberSequence()
        {
            return Observable.Create<int>(observer =>
            {
                for (int i = 1; i <= 5; i++)
                {
                    Thread.Sleep(1000); // Simulating delay
                    observer.OnNext(i);
                }

                observer.OnCompleted();
                return Disposable.Empty;
            });
        }

        [Test]
        public void TestRxSequence()
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name}");
            var watch = Stopwatch.StartNew();
            var subscription = GetNumberSequence().Subscribe(
                onNext: number => Console.WriteLine($"Received: {number}, {watch.ElapsedMilliseconds}"),
                onError: error => Console.WriteLine($"Error: {error.Message}, {watch.ElapsedMilliseconds}"),
                onCompleted: () => Console.WriteLine($"Sequence completed, {watch.ElapsedMilliseconds}")
            );

            // 위의 subscription 동작이 완료 된 후 진행된다.
            // Console.ReadLine(); // Wait for user input to end the program
            subscription.Dispose();
        }


        [Test]
        public void TestRxTimer()
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name}");
            IObservable<long> ticks = Observable.Timer(
                dueTime: TimeSpan.Zero,
                period: TimeSpan.FromSeconds(1));

            ticks.Subscribe(
                tick => Console.WriteLine($"Tick {tick}"));

            Thread.Sleep(5000);
            
        }
    }
}
