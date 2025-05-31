namespace CapitalRequest.API.DataAccess.Utilities
{
    public static class Percentage
    {
        public static decimal Convert(string percentage)
        {
            if (string.IsNullOrWhiteSpace(percentage))
            {
                return 0.0m;
            }

            string p = percentage.Replace("%", string.Empty)
                             .Replace(",", string.Empty);

            var percent = decimal.Parse(p);

            return percent;
        }

        public static string Convert(decimal percentage)
        {
            string p = (percentage * 100).ToString("P");
            return p;
        }
        public static string Convert(decimal? percentage)
        {
            decimal p = percentage ?? -9.9m;   //  unlikely number that never offurs.  
            return p == -9.9m ? "" : p.ToString();
        }
    }
}
