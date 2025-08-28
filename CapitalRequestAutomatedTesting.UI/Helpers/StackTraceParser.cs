using System.Text.RegularExpressions;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{
    public static class StackTraceParser
    {
        private static readonly Regex _regex = new(@"in (.*\.cs):line (\d+)", RegexOptions.Compiled);

        public static List<StackTraceLink> Parse(string stackTrace)
        {
            var links = new List<StackTraceLink>();
            var matches = _regex.Matches(stackTrace ?? "");

            foreach (Match match in matches)
            {
                links.Add(new StackTraceLink
                {
                    FilePath = match.Groups[1].Value,
                    LineNumber = int.Parse(match.Groups[2].Value)
                });
            }

            return links;
        }
    }
}
