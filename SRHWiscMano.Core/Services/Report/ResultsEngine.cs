using SRHWiscMano.Core.Models.Results;

namespace SRHWiscMano.Core.Services.Report
{
    internal class ResultsEngine
    {
        public static SwallowResults<TResult> Generate<TSource, TResult>(
            Func<Func<SwallowResults<TSource>, TSource>, TResult> handler)
        {
            return Fill(new SwallowResults<TResult>(), handler);
        }

        public static SwallowResults<TResult> Combine<TSource1, TSource2, TResult>(
            SwallowResults<TSource1> input1,
            SwallowResults<TSource2> input2,
            Func<TSource1, TSource2, TResult> combiner)
        {
            SwallowResults<Tuple<TSource1, TSource2>> boxed =
                new SwallowResults<Tuple<TSource1, TSource2>>();
            Fill(boxed,
                (Func<Func<SwallowResults<TSource1>, TSource1>, Tuple<TSource1, TSource2>>) (selector =>
                    Tuple.Create(selector(input1), default(TSource2))));
            Fill(boxed,
                (Func<Tuple<TSource1, TSource2>, Func<SwallowResults<TSource2>, TSource2>, Tuple<TSource1, TSource2>>)
                ((previous, selector) => Tuple.Create(previous.Item1, selector(input2))));
            return Generate(
                (Func<Func<SwallowResults<Tuple<TSource1, TSource2>>, Tuple<TSource1, TSource2>>, TResult>) (selector =>
                {
                    Tuple<TSource1, TSource2> tuple = selector(boxed);
                    return combiner(tuple.Item1, tuple.Item2);
                }));
        }

        private static SwallowResults<TResult> Fill<TSource, TResult>(
            SwallowResults<TResult> results,
            Func<Func<SwallowResults<TSource>, TSource>, TResult> handler)
        {
            return Fill(results,
                (Func<TResult, Func<SwallowResults<TSource>, TSource>, TResult>) ((previous, selector) =>
                    handler(selector)));
        }

        private static SwallowResults<TResult> Fill<TSource, TResult>(
            SwallowResults<TResult> results,
            Func<TResult, Func<SwallowResults<TSource>, TSource>, TResult> handler)
        {
            FillRegion(results.VP, i => i.VP, handler);
            FillRegion(results.TB, i => i.TB, handler);
            FillRegion(results.HP, i => i.HP, handler);
            FillRegion(results.PreUES, i => i.PreUES, handler);
            FillRegion(results.PostUES, i => i.PostUES, handler);
            FillUesRegion(results.UES, i => i.UES, handler);
            results.TotalSwallowDuration = handler(results.TotalSwallowDuration, r => r.TotalSwallowDuration);
            results.TotalPharyngealPressure = handler(results.TotalPharyngealPressure, r => r.TotalPharyngealPressure);
            return results;
        }

        private static ResultParameters<TResult> FillRegion<TSource, TResult>(
            ResultParameters<TResult> region,
            Func<SwallowResults<TSource>, ResultParameters<TSource>> regionSelector,
            Func<TResult, Func<SwallowResults<TSource>, TSource>, TResult> handler)
        {
            region.Duration = handler(region.Duration, r => regionSelector(r).Duration);
            region.MaximumPressure = handler(region.MaximumPressure, r => regionSelector(r).MaximumPressure);
            region.TotalPressureFromMaxSensor = handler(region.TotalPressureFromMaxSensor,
                r => regionSelector(r).TotalPressureFromMaxSensor);
            region.TotalVolumePressure =
                handler(region.TotalVolumePressure, r => regionSelector(r).TotalVolumePressure);
            return region;
        }

        private static UesResultParameters<TResult> FillUesRegion<TSource, TResult>(
            UesResultParameters<TResult> region,
            Func<SwallowResults<TSource>, UesResultParameters<TSource>> regionSelector,
            Func<TResult, Func<SwallowResults<TSource>, TSource>, TResult> handler)
        {
            FillRegion(region, regionSelector, handler);
            region.NadirDuration = handler(region.NadirDuration, r => regionSelector(r).NadirDuration);
            region.MinimumPressure = handler(region.MinimumPressure, r => regionSelector(r).MinimumPressure);
            return region;
        }
    }
}