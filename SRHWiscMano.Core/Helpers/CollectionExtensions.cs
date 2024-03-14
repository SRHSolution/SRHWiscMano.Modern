namespace SRHWiscMano.Core.Helpers
{
    public static class CollectionExtensions
    {
        public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (T obj in items)
                collection.Add(obj);
            return collection;
        }

        public static double[][] ToJaggedArray(this double[,] twoDimensionalArray)
        {
            int rows = twoDimensionalArray.GetLength(0); // 첫 번째 차원(행)의 길이를 얻습니다.
            int cols = twoDimensionalArray.GetLength(1); // 두 번째 차원(열)의 길이를 얻습니다.

            double[][] jaggedArray = new double[rows][];

            for (int i = 0; i < rows; i++)
            {
                jaggedArray[i] = new double[cols];
                for (int j = 0; j < cols; j++)
                {
                    jaggedArray[i][j] = twoDimensionalArray[i, j]; // 2D 배열의 요소를 jagged array에 복사합니다.
                }
            }

            return jaggedArray;
        }

    }
}
