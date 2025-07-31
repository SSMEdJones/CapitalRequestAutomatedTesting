namespace CapitalRequestAutomatedTesting.UI.Utilities
{
    public static class TextManipulation
    {
        /// <summary>
        /// This method replaces carrage return to html <br/> tag.  
        /// </summary>
        public static string ConvertToHtmlText(string text)
        {

            // If input is null, use an empty string; otherwise, use the input
            string safeText = text ?? string.Empty;

            string formattedMessage = safeText.Replace("\r\n", "<br/>").Replace("\n", "<br/>");
            return formattedMessage;
        }

        /// <summary>
        /// This method replaces <br/> to carrage return (\r\n).  
        /// </summary>
        public static string ConvertFromHtmlToText(string text)
        {

            // If input is null, use an empty string; otherwise, use the input
            string safeText = text ?? string.Empty;

            string unformattedMessage = safeText.Replace("<br/>", "\r\n");
            return unformattedMessage;
        }
    }
}


