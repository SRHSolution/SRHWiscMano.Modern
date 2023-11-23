using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.Helpers
{
    public static class Interpolators
    {
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
    }
}
