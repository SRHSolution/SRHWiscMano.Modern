using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.Helpers
{
    public static class PlotDataUtils
    {
        /// <summary>
        /// 입력받은 originalArray 에서 Row, Column range 영역의 데이터만을 갖는 array 로 반환한다.
        /// </summary>
        /// <param name="originalArray"></param>
        /// <param name="startRow">X축 sample start index</param>
        /// <param name="endRow">X축 sample end index</param>
        /// <param name="startColumn">Y축 sensor start index</param>
        /// <param name="endColumn">Y축 sensor end index</param>
        /// <returns></returns>
        public static double[,] CreateSubRange(double[,] originalArray, int startRow, int endRow, int startColumn, int endColumn)
        {
            int numRows = endRow - startRow + 1;
            int numCols = endColumn - startColumn + 1;
            double[,] subArray = new double[numRows, numCols];

            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startColumn; j <= endColumn; j++)
                {
                    subArray[i - startRow, j - startColumn] = originalArray[i, j];
                }
            }

            return subArray;
        }

    }
}
