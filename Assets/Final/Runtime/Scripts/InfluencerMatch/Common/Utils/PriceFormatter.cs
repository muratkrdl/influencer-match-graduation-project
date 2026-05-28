using System.Globalization;
using System.Text;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Static helper that centralizes price and large-number formatting across the presentation layer.
    /// </summary>
    public static class PriceFormatter
    {
        private const string k_Prefix = "$";
        private const string k_NumericFormat = "N0";

        private static readonly NumberFormatInfo s_FormatInfo = CultureInfo.InvariantCulture.NumberFormat;

        public static string Format(int value)
        {
            return k_Prefix + value.ToString(k_NumericFormat, s_FormatInfo);
        }

        public static string FormatNumeric(decimal value)
        {
            return value.ToString(k_NumericFormat, s_FormatInfo);
        }

        public static bool TryParse(string text, out decimal value)
        {
            value = 0m;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            StringBuilder digits = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c >= '0' && c <= '9')
                {
                    digits.Append(c);
                }
            }

            if (digits.Length == 0)
            {
                return false;
            }

            return decimal.TryParse(digits.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out value);
        }
    }
}
