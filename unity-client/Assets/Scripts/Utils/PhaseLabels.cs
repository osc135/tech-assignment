using System.Collections.Generic;

namespace HijackPoker.Utils
{
    public static class PhaseLabels
    {
        private static readonly Dictionary<string, string> Labels = new()
        {
            { "GAME_PREP", "Preparing Hand..." },
            { "SETUP_DEALER", "Setting Up Dealer" },
            { "SETUP_SMALL_BLIND", "Posting Small Blind" },
            { "SETUP_BIG_BLIND", "Posting Big Blind" },
            { "DEAL_CARDS", "Dealing Hole Cards" },
            { "PRE_FLOP_BETTING_ROUND", "Pre-Flop Betting" },
            { "DEAL_FLOP", "Dealing Flop" },
            { "FLOP_BETTING_ROUND", "Flop Betting" },
            { "DEAL_TURN", "Dealing Turn" },
            { "TURN_BETTING_ROUND", "Turn Betting" },
            { "DEAL_RIVER", "Dealing River" },
            { "RIVER_BETTING_ROUND", "River Betting" },
            { "AFTER_RIVER_BETTING_ROUND", "Showdown" },
            { "FIND_WINNERS", "Evaluating Hands" },
            { "PAY_WINNERS", "Paying Winners" },
            { "RECORD_STATS_AND_NEW_HAND", "Hand Complete" },
        };

        public static string GetLabel(string stepName)
        {
            if (string.IsNullOrEmpty(stepName))
                return "";

            return Labels.TryGetValue(stepName, out var label) ? label : stepName;
        }
    }
}
