
namespace ScenarioFramework
{
    using System;
    using System.Collections.Generic;

    public class TestScenario
    {
        public string ScenarioId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ScenarioStep> Steps { get; set; } = new();
        public ScenarioStatus Status { get; set; }
    }
}
