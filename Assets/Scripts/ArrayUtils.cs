using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayUtils
{
    public static int[] GetRow(int[,] mat, int row)
    {
        int[] ret = new int[mat.GetLength(1)];
        for (int i = 0; i < mat.GetLength(1); i++)
        {
            ret[i] = mat[row, i];
        }

        return ret;
    }
}
