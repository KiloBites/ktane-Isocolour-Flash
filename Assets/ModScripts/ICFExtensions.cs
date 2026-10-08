using System.Collections.Generic;
using System.Linq;

public static class ICFExtensions
{
    public static T[,] To2DArray<T>(this IEnumerable<IEnumerable<T>> source)
    {
        var data = source.Select(x => x.ToArray()).ToArray();
        var rows = data.Length;
        var cols = data.Max(x => x.Length);
        var result = new T[rows, cols];
        
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < data[i].Length; j++)
                result[i, j] = data[i][j];
        
        return result;
    }

    public static T[,] To2DArray<T>(this T[,] source)
    {
        var rows = source.GetLength(0);
        var cols = source.GetLength(1);
        var result = new T[rows, cols];
        
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i, j] = source[i, j];
        
        return result;
    }
}