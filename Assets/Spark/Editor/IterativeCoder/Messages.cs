using System.Text.RegularExpressions;

namespace LeastSquares.Spark
{
    public class Messages
    {
        public static string EditScriptMessage(string Code, string Prompt)
        {
            return $"|EDIT_SCRIPT|`{Code}`|`{Prompt}`";
        }

        public static string NewScriptMessage(string Prompt)
        {
            return $"|NEW_SCRIPT|`{Prompt}`";
        }
        
          
        public static string ParseNewScriptMessage(string messageContent)
        {
            var prompt = messageContent.Substring("|NEW_SCRIPT|".Length);
            return prompt.Substring(1, prompt.Length-2);
        }

        public static string ParseEditScriptMessage(string messageContent, out string changes)
        {
            changes = string.Empty;
            if (string.IsNullOrEmpty(messageContent)) return string.Empty;
            var match = Regex.Match(messageContent, @"\|EDIT_SCRIPT\|`(.+)`\|`(.+)`$", RegexOptions.Multiline | RegexOptions.Singleline);
            if (!match.Success || match.Groups.Count != 3) return string.Empty;
            changes = match.Groups[2].Value;
            return match.Groups[1].Value;
        }
        
        public static string ParseCodeScriptMessage(string messageContent)
        {
            if (string.IsNullOrEmpty(messageContent)) return string.Empty;
            
            const string startTag = "|CODE|`";
            if (!messageContent.StartsWith(startTag))
                return string.Empty;
            var trimmed = messageContent.Substring(startTag.Length);
            return trimmed.EndsWith("`") ? trimmed.Substring(0, trimmed.Length-1) : trimmed;
        }
    }
}