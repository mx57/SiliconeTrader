using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks; // Added for Task

namespace SiliconeTrader.Machine.Misc
{
    public static class Utils
    {
        private static Regex fixJsonPattern = new Regex(@"\w[^,{]+[\w""]", RegexOptions.Compiled);

        public static string FixInvalidJson(string json)
        {
            string fixedJson = fixJsonPattern.Replace(json, match =>
            {
                string matchString = match.ToString();
                if (!matchString.EndsWith("\""))
                {
                    string[] split = matchString.Split(": ");

                    if (split.Length == 1)
                    {
                        return $"\"{matchString.Trim('"', ' ')}\"";
                    }
                    else
                    {
                        string left = split[0].Trim('"', ' ');
                        string right = split[1].Trim('"', ' ');

                        if (right[0] == '[')
                        {
                            return $"\"{left}\": {right.Replace("[", "[\"")}\"";
                        }
                        else if (right == "True" || right == "False")
                        {
                            return $"\"{left}\": \"{right.ToLowerInvariant()}\"";
                        }
                        else if (right == "null")
                        {
                            return $"\"{left}\": {right}";
                        }
                        else
                        {
                            return $"\"{left}\": \"{right}\"";
                        }
                    }
                }
                else
                {
                    return matchString;
                }
            });
            return fixedJson;
        }

        public static async Task<string[]> ReadAllLinesWriteSafeAsync(string path)
        {
            var lines = new List<string>();
            // FileShare.ReadWrite allows reading even if the file is open for writing by another process.
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {
                string line;
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines.ToArray();
        }
    }
}
