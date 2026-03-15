namespace HijackPoker.Utils
{
    public static class MoneyFormatter
    {
        public static string Format(float amount)
        {
            return $"${amount:F2}";
        }

        public static string FormatWithSign(float amount)
        {
            return amount >= 0 ? $"+${amount:F2}" : $"-${(-amount):F2}";
        }
    }
}
