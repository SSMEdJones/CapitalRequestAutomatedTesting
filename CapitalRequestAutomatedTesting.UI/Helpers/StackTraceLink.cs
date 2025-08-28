namespace CapitalRequestAutomatedTesting.UI.Helpers
{
    public class StackTraceLink
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string DisplayText => $"{Path.GetFileName(FilePath)}:line {LineNumber}";
        public string EditorLink => $"vscode://file/{FilePath.Replace("\\", "/")}:{LineNumber}";
    }
}
