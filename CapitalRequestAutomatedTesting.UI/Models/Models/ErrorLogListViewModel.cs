namespace CapitalRequestAutomatedTesting.UI.Models.Models
{
    public class ErrorLogListViewModel
    {
        public List<ErrorLogViewModel> Logs { get; set; } = new List<ErrorLogViewModel>();
        
        // Pagination properties
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        
        // Search properties
        public string SearchQuery { get; set; }
        public bool IsSearchResult => !string.IsNullOrWhiteSpace(SearchQuery);
        
        // Navigation helpers
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}