using System;
using System.Collections.Generic;
using System.Linq;
using HijackPoker.Models;

namespace HijackPoker.Managers
{
    public class TableStateManager
    {
        public TableResponse CurrentState { get; private set; }
        public TableResponse PreviousState { get; private set; }

        // Cumulative stack offsets per player (tracks winnings/losses across hands)
        private readonly Dictionary<int, float> _stackOffsets = new();
        private int _lastGameNo = -1;

        public event Action<TableResponse> OnStateChanged;
        public event Action<TableResponse> OnHandCompleted;

        public void UpdateState(TableResponse newState)
        {
            PreviousState = CurrentState;
            CurrentState = newState;

            // Detect new hand — gameNo changed and we had a previous state
            if (PreviousState?.Game != null && CurrentState?.Game != null
                && CurrentState.Game.GameNo != _lastGameNo && _lastGameNo > 0)
            {
                // A new hand started — the API reset stacks.
                // Capture the difference from the previous state's stacks.
                if (PreviousState.Players != null)
                {
                    foreach (var prev in PreviousState.Players)
                    {
                        // Find matching player in new state to get the reset stack
                        var current = CurrentState.Players?.FirstOrDefault(p => p.Seat == prev.Seat);
                        if (current != null)
                        {
                            float diff = prev.Stack - current.Stack;
                            if (!_stackOffsets.ContainsKey(prev.Seat))
                                _stackOffsets[prev.Seat] = 0f;
                            _stackOffsets[prev.Seat] += diff;
                        }
                    }
                }
            }

            _lastGameNo = CurrentState?.Game?.GameNo ?? _lastGameNo;

            // Apply offsets to current state stacks
            ApplyStackOffsets();

            // Fix dealer/SB/BB rotation (backend bug: always resets dealer to seat 1)
            CorrectPositions();

            OnStateChanged?.Invoke(CurrentState);

            if (CurrentState?.Game?.IsHandComplete == true)
            {
                OnHandCompleted?.Invoke(CurrentState);
            }
        }

        private void ApplyStackOffsets()
        {
            if (CurrentState?.Players == null) return;

            foreach (var player in CurrentState.Players)
            {
                if (_stackOffsets.TryGetValue(player.Seat, out float offset))
                {
                    player.Stack = Math.Max(0f, player.Stack + offset);
                }
            }
        }

        /// <summary>
        /// The backend always resets dealer_seat to 1 on new game creation,
        /// so the dealer never truly rotates. We fix this client-side by
        /// computing the correct positions from the hand number (gameNo).
        /// </summary>
        private void CorrectPositions()
        {
            if (CurrentState?.Game == null || CurrentState.Players == null)
                return;

            var game = CurrentState.Game;
            var activePlayers = CurrentState.Players
                .Where(p => p != null)
                .OrderBy(p => p.Seat)
                .ToList();

            if (activePlayers.Count < 2) return;

            // Only rotate the dealer — SB/BB come from the backend
            // since it controls who actually posts the blinds
            int dealerIndex = (game.GameNo - 1) % activePlayers.Count;
            game.DealerSeat = activePlayers[dealerIndex].Seat;
        }

        public bool HasState => CurrentState != null;
    }
}
