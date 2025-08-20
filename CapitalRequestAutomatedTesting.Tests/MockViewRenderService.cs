using CapitalRequestAutomatedTesting.UI.Services;
using Newtonsoft.Json;

namespace CapitalRequestAutomatedTesting.Tests
{
    /// <summary>
    /// Mock implementation of IViewRenderService for testing purposes.
    /// This avoids the need for a full Razor view engine during tests.
    /// </summary>
    public class MockViewRenderService : IViewRenderService
    {
        /// <summary>
        /// Returns a simple string representation of the view and model data instead of 
        /// actually rendering a Razor view.
        /// </summary>
        /// <param name="viewName">Name of the view that would be rendered</param>
        /// <param name="model">Model data that would be passed to the view</param>
        /// <returns>A string representation of the view name and model data</returns>
        public Task<string> RenderToStringAsync(string viewName, object model)
        {
            // For testing purposes, we just return a simple formatted string
            // that indicates the view and serializes the model to JSON
            var modelJson = model != null 
                ? JsonConvert.SerializeObject(model, Formatting.Indented, 
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                : "null";
                
            return Task.FromResult(
                $"[MOCK VIEW RENDERER] View: {viewName}\nModel: {modelJson}");
        }
    }
}