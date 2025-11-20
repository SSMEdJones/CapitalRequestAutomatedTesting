namespace CapitalRequest.API.Models
{
    public class CapitalPool
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public bool? IsWbs { get; set; }

        public int? SortOrder { get; set; }

        public int? ItemOrder { get; set; }
    }
}
