using System;
using System.Collections.Generic;
using System.Linq;
using HijackPoker.Models;
using HijackPoker.Utils;

namespace HijackPoker.Managers
{
    public class HandHistoryManager
    {
        private readonly List<string> _entries = new();
        private const int MaxEntries = 100;

        private int _lastHandStep = -1;
        private int _lastGameNo = -1;

        public IReadOnlyList<string> Entries => _entries;
        public event Action OnEntriesChanged;

        public void ProcessState(TableResponse state)
        {
            if (state?.Game == null) return;

            var game = state.Game;
            var players = state.Players ?? new List<PlayerState>();

            // Skip duplicate updates for the same step
            if (game.GameNo == _lastGameNo && game.HandStep == _lastHandStep)
                return;

            // New hand started
            if (game.GameNo != _lastGameNo && _lastGameNo > 0)
            {
                AddEntry("");  // blank line separator
            }

            string entry = BuildEntry(game, players);
            if (!string.IsNullOrEmpty(entry))
            {
                AddEntry(entry);
            }

            _lastGameNo = game.GameNo;
            _lastHandStep = game.HandStep;
        }

        private string BuildEntry(GameState game, List<PlayerState> players)
        {
            string prefix = $"Hand #{game.GameNo}";
            string phase = PhaseLabels.GetLabel(game.StepName);

            switch (game.StepName)
            {
                case "SETUP_SMALL_BLIND":
                    var sbPlayer = players.FirstOrDefault(p => p.Seat == game.SmallBlindSeat);
                    if (sbPlayer != null)
                        return $"{prefix} — {sbPlayer.Username} posts SB {MoneyFormatter.Format(game.SmallBlind)}";
                    return $"{prefix} — {phase}";

                case "SETUP_BIG_BLIND":
                    var bbPlayer = players.FirstOrDefault(p => p.Seat == game.BigBlindSeat);
                    if (bbPlayer != null)
                        return $"{prefix} — {bbPlayer.Username} posts BB {MoneyFormatter.Format(game.BigBlind)}";
                    return $"{prefix} — {phase}";

                case "PRE_FLOP_BETTING_ROUND":
                case "FLOP_BETTING_ROUND":
                case "TURN_BETTING_ROUND":
                case "RIVER_BETTING_ROUND":
                    return $"{prefix} — {BuildBettingEntry(players, phase)}";

                case "DEAL_FLOP":
                    var flopCards = game.CommunityCards?.Take(3).Select(CardUtils.GetDisplayString);
                    string flop = flopCards != null ? string.Join(" ", flopCards) : "";
                    return $"{prefix} — Flop: {flop}";

                case "DEAL_TURN":
                    if (game.CommunityCards?.Count >= 4)
                        return $"{prefix} — Turn: {CardUtils.GetDisplayString(game.CommunityCards[3])}";
                    return $"{prefix} — {phase}";

                case "DEAL_RIVER":
                    if (game.CommunityCards?.Count >= 5)
                        return $"{prefix} — River: {CardUtils.GetDisplayString(game.CommunityCards[4])}";
                    return $"{prefix} — {phase}";

                case "PAY_WINNERS":
                case "FIND_WINNERS":
                    var winners = players.Where(p => p.IsWinner).ToList();
                    if (winners.Count > 0)
                    {
                        var parts = winners.Select(w =>
                        {
                            string rank = !string.IsNullOrEmpty(w.HandRank) ? $" ({w.HandRank})" : "";
                            return $"{w.Username} wins {MoneyFormatter.FormatWithSign(w.Winnings)}{rank}";
                        });
                        return $"{prefix} — {string.Join(", ", parts)}";
                    }
                    return $"{prefix} — {phase}";

                case "RECORD_STATS_AND_NEW_HAND":
                    return $"{prefix} — Hand Complete";

                default:
                    return $"{prefix} — {phase}";
            }
        }

        private string BuildBettingEntry(List<PlayerState> players, string phase)
        {
            var actions = new List<string>();

            foreach (var p in players)
            {
                if (string.IsNullOrEmpty(p.Action)) continue;

                switch (p.Action.ToLower())
                {
                    case "fold":
                        actions.Add($"{p.Username} folds");
                        break;
                    case "check":
                        actions.Add($"{p.Username} checks");
                        break;
                    case "call":
                        actions.Add($"{p.Username} calls");
                        break;
                    case "bet":
                        actions.Add($"{p.Username} bets {MoneyFormatter.Format(p.Bet)}");
                        break;
                    case "raise":
                        actions.Add($"{p.Username} raises to {MoneyFormatter.Format(p.Bet)}");
                        break;
                    case "allin":
                        actions.Add($"{p.Username} all-in");
                        break;
                }
            }

            if (actions.Count > 0)
                return string.Join(", ", actions);

            return phase;
        }

        private void AddEntry(string entry)
        {
            _entries.Add(entry);

            while (_entries.Count > MaxEntries)
            {
                _entries.RemoveAt(0);
            }

            OnEntriesChanged?.Invoke();
        }

        public void Clear()
        {
            _entries.Clear();
            _lastHandStep = -1;
            _lastGameNo = -1;
            OnEntriesChanged?.Invoke();
        }
    }
}
