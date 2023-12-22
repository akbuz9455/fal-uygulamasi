using System;
using System.Collections.Generic;
using UnityEngine;

namespace LeastSquares.Spark
{

    public class DiffLine
    {
        public enum LineType
        {
            Unchanged,
            Added,
            Deleted
        }

        public LineType Type { get; set; }
        public string Content { get; set; }
    }

    public static class DiffGenerator
    {
        public static List<DiffLine> GenerateDiff(string a, string b)
        {
            string[] lines1 = a.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string[] lines2 = b.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            int[,] lengths = new int[lines1.Length + 1, lines2.Length + 1];

            // Calculate the lengths of the longest common subsequence
            for (int i = 0; i < lines1.Length; i++)
            {
                for (int j = 0; j < lines2.Length; j++)
                {
                    if (lines1[i] == lines2[j])
                    {
                        lengths[i + 1, j + 1] = lengths[i, j] + 1;
                    }
                    else
                    {
                        lengths[i + 1, j + 1] = Math.Max(lengths[i + 1, j], lengths[i, j + 1]);
                    }
                }
            }

            // Generate the diff lines by backtracking through the lengths matrix
            List<DiffLine> result = new List<DiffLine>();
            int x = lines1.Length, y = lines2.Length;
            while (x > 0 || y > 0)
            {
                if (x > 0 && y > 0 && lines1[x - 1] == lines2[y - 1])
                {
                    result.Add(new DiffLine { Type = DiffLine.LineType.Unchanged, Content = lines1[x - 1] });
                    x--;
                    y--;
                }
                else if (y > 0 && (x == 0 || lengths[x, y - 1] >= lengths[x - 1, y]))
                {
                    result.Add(new DiffLine { Type = DiffLine.LineType.Added, Content = lines2[y - 1] });
                    y--;
                }
                else if (x > 0 && (y == 0 || lengths[x, y - 1] < lengths[x - 1, y]))
                {
                    result.Add(new DiffLine { Type = DiffLine.LineType.Deleted, Content = lines1[x - 1] });
                    x--;
                }
                else
                {
                    Debug.LogError($"error: {x}, {y}, {lines1.Length}, {lines2.Length}");
                    break;
                }
            }

            result.Reverse();

            return result;
        }

    }
}