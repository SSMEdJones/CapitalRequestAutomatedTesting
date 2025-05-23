namespace CapitalRequestAutomatedTesting.UI.Models
{

    public class ApiResponse<T>
    {
        public T Result { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

}
