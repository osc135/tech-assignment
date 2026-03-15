using UnityEngine.UIElements;
using HijackPoker.Models;
using HijackPoker.Utils;

namespace HijackPoker.UI
{
    public class SeatView
    {
        private readonly VisualElement _root;
        private readonly VisualElement _panel;
        private readonly Label _nameLabel;
        private readonly Label _stackLabel;
        private readonly VisualElement _holeCards;
        private readonly Label _betLabel;
        private readonly Label _actionLabel;
        private readonly Label _handRankLabel;
        private readonly Label _winningsLabel;
        private readonly Label _positionChip;
        private readonly int _seatNumber;

        public SeatView(int seatNumber)
        {
            _seatNumber = seatNumber;

            _root = new VisualElement();
            _root.AddToClassList("seat");
            _root.AddToClassList($"seat-{seatNumber}");
            _root.AddToClassList("empty");

            // Position chip (SB / BB) — floats near the seat
            _positionChip = new Label();
            _positionChip.AddToClassList("position-chip");
            _root.Add(_positionChip);

            // Single panel that contains EVERYTHING
            _panel = new VisualElement();
            _panel.AddToClassList("seat-panel");

            _nameLabel = new Label();
            _nameLabel.AddToClassList("player-name");
            _panel.Add(_nameLabel);

            _stackLabel = new Label();
            _stackLabel.AddToClassList("player-stack");
            _panel.Add(_stackLabel);

            _holeCards = new VisualElement();
            _holeCards.AddToClassList("hole-cards");
            _panel.Add(_holeCards);

            _betLabel = new Label();
            _betLabel.AddToClassList("player-bet");
            _panel.Add(_betLabel);

            _actionLabel = new Label();
            _actionLabel.AddToClassList("action-badge");
            _panel.Add(_actionLabel);

            _handRankLabel = new Label();
            _handRankLabel.AddToClassList("hand-rank");
            _panel.Add(_handRankLabel);

            _winningsLabel = new Label();
            _winningsLabel.AddToClassList("player-winnings");
            _panel.Add(_winningsLabel);

            _root.Add(_panel);
        }

        public VisualElement Root => _root;

        public void Render(PlayerState player, GameState game)
        {
            if (player == null)
            {
                _root.EnableInClassList("empty", true);
                return;
            }

            _root.EnableInClassList("empty", false);
            _root.EnableInClassList("folded", player.IsFolded);
            _root.EnableInClassList("all-in", player.IsAllIn);
            _root.EnableInClassList("winner", player.IsWinner);

            // Name
            _nameLabel.text = player.Username;

            // Position chip (SB / BB near the seat)
            RenderPositionChip(player.Seat, game);

            // Stack
            _stackLabel.text = MoneyFormatter.Format(player.Stack);

            // Hole cards
            RenderHoleCards(player, game);

            // Bet
            _betLabel.text = player.Bet > 0 ? MoneyFormatter.Format(player.Bet) : "";

            // Action badge
            RenderAction(player.Action);

            // Hand rank (showdown only)
            _handRankLabel.text = !string.IsNullOrEmpty(player.HandRank) ? player.HandRank : "";

            // Winnings
            _winningsLabel.text = player.IsWinner ? MoneyFormatter.FormatWithSign(player.Winnings) : "";
        }

        private void RenderHoleCards(PlayerState player, GameState game)
        {
            _holeCards.Clear();

            if (!player.HasCards) return;

            bool showCards = game.IsShowdown || player.IsWinner;

            foreach (var cardStr in player.Cards)
            {
                if (showCards && !player.IsFolded)
                {
                    _holeCards.Add(CardView.CreateFaceUp(cardStr, small: true));
                }
                else
                {
                    _holeCards.Add(CardView.CreateFaceDown(small: true));
                }
            }
        }

        private void RenderAction(string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                _actionLabel.style.display = DisplayStyle.None;
                return;
            }

            _actionLabel.style.display = DisplayStyle.Flex;
            _actionLabel.text = action.ToUpper();

            _actionLabel.RemoveFromClassList("action-check");
            _actionLabel.RemoveFromClassList("action-call");
            _actionLabel.RemoveFromClassList("action-bet");
            _actionLabel.RemoveFromClassList("action-raise");
            _actionLabel.RemoveFromClassList("action-fold");
            _actionLabel.RemoveFromClassList("action-allin");

            _actionLabel.AddToClassList($"action-{action.ToLower()}");
        }

        private void RenderPositionChip(int seat, GameState game)
        {
            _positionChip.RemoveFromClassList("chip-dealer");
            _positionChip.RemoveFromClassList("chip-sb");
            _positionChip.RemoveFromClassList("chip-bb");

            if (seat == game.DealerSeat)
            {
                _positionChip.text = "D";
                _positionChip.AddToClassList("chip-dealer");
                _positionChip.style.display = DisplayStyle.Flex;
            }
            else if (seat == game.SmallBlindSeat)
            {
                _positionChip.text = "SB";
                _positionChip.AddToClassList("chip-sb");
                _positionChip.style.display = DisplayStyle.Flex;
            }
            else if (seat == game.BigBlindSeat)
            {
                _positionChip.text = "BB";
                _positionChip.AddToClassList("chip-bb");
                _positionChip.style.display = DisplayStyle.Flex;
            }
            else
            {
                _positionChip.style.display = DisplayStyle.None;
            }
        }
    }
}
