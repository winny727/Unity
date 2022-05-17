using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class SudokuSolve : MonoBehaviour
{
    public static int Len = 9;
    public static List<int[,]> Results = new List<int[,]>();
    public static int MaxResult = 1;
    public static int SolveTime;
    public static bool IsSolve;

    private void Start()
    {
        //int[,] arr = new int[,]
        //{
        //    {0,8,0,0,0,0,0,6,0},
        //    {0,0,0,7,0,9,0,0,0},
        //    {4,0,0,0,0,2,0,0,0},
        //    {7,0,0,0,0,0,1,0,9},
        //    {0,0,0,0,5,0,0,3,0},
        //    {0,0,0,0,0,0,0,0,7},
        //    {0,3,6,0,8,0,0,0,0},
        //    {0,0,5,0,0,0,0,0,0},
        //    {0,0,0,1,0,0,0,0,2}
        //};

        //Stopwatch sw = new Stopwatch();
        //sw.Start();
        //Solve(arr);
        //sw.Stop();

        //Debug.Log(sw.ElapsedMilliseconds);
        //for (int i = 0; i < Results.Count; i++)
        //{
        //    Print(Results[i]);
        //}
    }

    public static void SolveSuduku(int[,] arr, int maxResult = 1)
    {
        MaxResult = maxResult;

        Stopwatch sw = new Stopwatch();
        sw.Start();
        Solve(arr);
        sw.Stop();

        IsSolve = true;
        SolveTime = (int)sw.ElapsedMilliseconds;
    }

    public static void Clear()
    {
        Results.Clear();
        SolveTime = 0;
        IsSolve = false;
    }

    private void Print(int[,] arr)
    {
        string str = "";
        for (int x = 0; x < Len; x++)
        {
            for (int y = 0; y < Len; y++)
            {
                str += arr[x, y] + ",";
            }
            str += "\n";
        }
        Debug.Log(str);
    }

    private static void Solve(int[,] arr)
    {
        if (Results.Count >= MaxResult)
        {
            return;
        }

        bool isFull = true;
        int minValid = int.MaxValue;
        int x = 0, y = 0;
        for (int i = 0; i < Len; i++)
        {
            for (int j = 0; j < Len; j++)
            {
                if (arr[i, j] == 0)
                {
                    isFull = false;
                    int validCount = 0;
                    for (int n = 0; n < Len; n++)
                    {
                        arr[i, j] = n;
                        if (CheckValid(arr, i, j))
                        {
                            validCount++;
                        }
                    }
                    if (validCount < minValid)
                    {
                        minValid = validCount;
                        x = i;
                        y = j;
                    }
                    arr[i, j] = 0;
                }
            }
        }

        if (isFull)
        {
            Results.Add(arr);
            return;
        }

        List<int> validNums = GetValidNums(arr, x, y);
        for (int n = 0; n < validNums.Count; n++)
        {
            int[,] newArr = new int[Len, Len];
            for (int ii = 0; ii < Len; ii++)
            {
                for (int jj = 0; jj < Len; jj++)
                {
                    if (ii == x && jj == y)
                    {
                        newArr[ii, jj] = validNums[n];
                    }
                    else
                    {
                        newArr[ii, jj] = arr[ii, jj];
                    }
                }
            }
            Solve(newArr);
        }
        //for (int i = 0; i < Len; i++)
        //{
        //    for (int j = 0; j < Len; j++)
        //    {
        //        if (arr[i, j] == 0)
        //        {
        //            List<int> validNums = GetValidNums(arr, i, j);
        //            for (int n = 0; n < validNums.Count; n++)
        //            {
        //                int[,] newArr = new int[Len, Len];
        //                for (int ii = 0; ii < Len; ii++)
        //                {
        //                    for (int jj = 0; jj < Len; jj++)
        //                    {
        //                        if (ii == i && jj == j)
        //                        {
        //                            newArr[ii, jj] = validNums[n];
        //                        }
        //                        else
        //                        {
        //                            newArr[ii, jj] = arr[ii, jj];
        //                        }
        //                    }
        //                }
        //                Solve(newArr);
        //            }
        //            return;
        //        }
        //    }
        //}
        //Results.Add(arr);
    }

    private static List<int> GetValidNums(int[,] arr, int row, int col)
    {
        List<int> result = new List<int>();
        for (int i = 0; i < Len; i++)
        {
            result.Add(i + 1);
        }

        for (int i = 0; i < Len; i++)
        {
            int val = arr[i, col];
            if (val != 0 && result.Contains(val))
            {
                result.Remove(val);
            }
        }

        for (int i = 0; i < Len; i++)
        {
            int val = arr[row, i];
            if (val != 0 && result.Contains(val))
            {
                result.Remove(val);
            }
        }

        int x = row - row % 3;
        int y = col - col % 3;
        for (int i = x; i < x + 3; i++)
        {
            for (int j = y; j < y + 3; j++)
            {
                int val = arr[i, j];
                if (val != 0 && result.Contains(val))
                {
                    result.Remove(val);
                }
            }
        }

        return result;
    }

    public static bool CheckValid(int[,] arr, int row, int col)
    {
        int value = arr[row, col];
        for (int i = 0; i < Len; i++)
        {
            if (i == row)
            {
                continue;
            }

            int val = arr[i, col];
            if (val != 0 && val == value)
            {
                return false;
            }
        }

        for (int i = 0; i < Len; i++)
        {
            if (i == col)
            {
                continue;
            }

            int val = arr[row, i];
            if (val != 0 && val == value)
            {
                return false;
            }
        }

        int x = row - row % 3;
        int y = col - col % 3;
        for (int i = x; i < x + 3; i++)
        {
            for (int j = y; j < y + 3; j++)
            {
                if (i == row && j == col)
                {
                    continue;
                }

                int val = arr[i, j];
                if (val != 0 && val == value)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
