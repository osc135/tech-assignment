using System.Collections.Generic;

namespace HijackPoker.Utils
{
    public static class CardUtils
    {
        private static readonly Dictionary<char, string> SuitSymbols = new()
        {
            { 'H', "\u2665" },  // ♥
            { 'D', "\u2666" },  // ♦
            { 'C', "\u2663" },  // ♣
            { 'S', "\u2660" },  // ♠
        };

        private static readonly HashSet<char> RedSuits = new() { 'H', 'D' };

        public static bool TryParse(string cardStr, out string rank, out char suit)
        {
            rank = null;
            suit = default;

            if (string.IsNullOrEmpty(cardStr) || cardStr.Length < 2)
                return false;

            suit = char.ToUpper(cardStr[^1]);
            rank = cardStr[..^1];

            if (!SuitSymbols.ContainsKey(suit))
                return false;

            if (string.IsNullOrEmpty(rank))
                return false;

            return true;
        }

        public static string GetSuitSymbol(char suit)
        {
            return SuitSymbols.TryGetValue(char.ToUpper(suit), out var symbol) ? symbol : "?";
        }

        public static bool IsRed(char suit)
        {
            return RedSuits.Contains(char.ToUpper(suit));
        }

        public static string GetDisplayString(string cardStr)
        {
            if (!TryParse(cardStr, out var rank, out var suit))
                return "??";

            return $"{rank}{GetSuitSymbol(suit)}";
        }

        public static string GetColorClass(char suit)
        {
            return IsRed(suit) ? "red" : "black";
        }
    }
}
