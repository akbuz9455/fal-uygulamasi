using System.Text.RegularExpressions;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class CodeCleaner
    {
        public static string CleanExtraCommentsAndQuotes(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            input = Regex.Replace(input, "^```[a-zA-Z]*", "", RegexOptions.Multiline);
            input = Regex.Replace(input, "^`+", "", RegexOptions.Multiline);
            input = Regex.Replace(input, "`+$", "", RegexOptions.Multiline);
            return input;
        }
    }
}