using MathNet.Numerics.Interpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.Helpers
{
    public static class Interpolators
    {

        /// <summary>
        /// 입력받은 Array 데이터를 newLength의 크기로 다시 분할하여 interpolation한다
        /// </summary>
        /// <param name="originalArray"></param>
        /// <param name="newLength"></param>
        /// <returns></returns>
        public static double[] LinearInterpolate(double[] originalArray, int newLength)
        {
            double[] interpolatedArray = new double[newLength];
            double scale = (double)(originalArray.Length - 1) / (newLength - 1);

            for (int i = 0; i < newLength; i++)
            {
                double pos = i * scale;
                int leftIndex = (int)pos;
                int rightIndex = leftIndex + 1;

                if (rightIndex >= originalArray.Length)
                {
                    interpolatedArray[i] = originalArray[leftIndex];
                }
                else
                {
                    double leftValue = originalArray[leftIndex];
                    double rightValue = originalArray[rightIndex];
                    double fraction = pos - leftIndex;
                    interpolatedArray[i] = leftValue + (rightValue - leftValue) * fraction;
                }
            }

            return interpolatedArray;
        }

        public static IEnumerable<double> InterpolateTo(this double[] source, int targetSize)
        {
            if (targetSize == source.Length)
                return source;
            if (source.Length == 1)
                return Enumerable.Repeat(source[0], targetSize);
            LinearSpline spline = LinearSpline.InterpolateSorted(XValues(source.Length).ToArray(), source);
            return XValues(targetSize).Select(x => spline.Interpolate(x));
        }

        private static IEnumerable<double> XValues(int count)
        {
            double sourceStep = 1.0 / (count - 1.0);
            return Enumerable.Range(0, count).Select(i => i * sourceStep);
        }
    }
}
