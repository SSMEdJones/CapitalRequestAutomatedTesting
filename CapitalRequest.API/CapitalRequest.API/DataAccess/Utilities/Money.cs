using System.Globalization;

namespace CapitalRequest.API.DataAccess.Utilities
{
    public static class Money
    {
        public static string ConvertParanthesisToNegativeSign(string numStr)
        {
            string negativeMoney = numStr;
            if (numStr.Substring(0, 1) == "(" && numStr.Substring(numStr.Length - 1, 1) == ")")
            {
                //negativeMoney = $"-{numStr.Substring(1, numStr.Length - 2)}";
                negativeMoney = $"-{numStr[1..^1]}";
            }

            return negativeMoney;
        }

        public static decimal? Convert(string money)
        {
            if (money == null) return null;

            if (money.Trim() == "$")
            { // If UI passes $ after deleting number, then treat $ as nothing is entered.
                return null;
            }

            if (string.IsNullOrWhiteSpace(money))
            {
                return 0.0m;
            }

            if (money.Substring(0, 1) == "(" && money.Substring(money.Length - 1, 1) == ")")
            {
                var negativeMoney = $"-{money[1..^1]}";

                money = negativeMoney;

            }

            string m = money.Replace("$", string.Empty)
                             .Replace(",", string.Empty);
            m = ConvertParanthesisToNegativeSign(m);

            return decimal.Parse(m);
        }


        public static string Convert(decimal? money)
        {
            decimal m = money ?? -9.9m; //unlikely number that never happen
            return m == -9.9m ? "" : NumberFormat(m);
        }

        /// <summary>
        /// Customize currency format to use negative sign.  
        /// By default, negative Value of Money format of the en-US use parenthesis.
        /// </summary>
        /// <param name="money">money in two decimal points</param>
        /// <returns></returns>
        public static string NumberFormat(decimal money)
        {
            NumberFormatInfo customNfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();

            // Set the negative currency pattern to use a minus sign "-$12.3" instead of parentheses ($12.3)
            customNfi.CurrencyNegativePattern = 1; // "-$n"

            string formattedValue = money.ToString("C", customNfi);
            return formattedValue;

        }
    }
}
