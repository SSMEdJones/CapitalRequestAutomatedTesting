public class ErrorLogViewModel
{
    public int Id { get; set; }
    public DateTime Logged { get; set; }

    public string Level { get; set; }
    public string Message { get; set; }

    public string Logger { get; set; }
    public string Exception { get; set; }
    public string Url { get; set; }
    public string UserName { get; set; }

    public string StackTrace { get; set; }
    public string AdditionalInfo { get; set; }

    public string ScenarioId { get; set; }
    public string RollbackStatus { get; set; }
}