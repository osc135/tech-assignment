using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using HijackPoker.Models;
using HijackPoker.Utils;

namespace HijackPoker.UI
{
    public class TableView
    {
        private readonly VisualElement _tableArea;
        private readonly Label _potLabel;
        private readonly CommunityCardsView _communityCards;
        private readonly HudView _hud;
        private readonly Dictionary<int, SeatView> _seats = new();

        public TableView(VisualElement root)
        {
            _tableArea = root.Q<VisualElement>("table-area");
            _potLabel = root.Q<Label>("pot-label");

            var communityContainer = root.Q<VisualElement>("community-cards");
            _communityCards = new CommunityCardsView(communityContainer);

            var phaseLabel = root.Q<Label>("phase-label");
            _hud = new HudView(phaseLabel);

            // Create 6 seats
            for (int i = 1; i <= 6; i++)
            {
                var seat = new SeatView(i);
                _seats[i] = seat;
                _tableArea.Add(seat.Root);
            }

            // Initialize community cards as empty
            _communityCards.Render(null);
        }

        public HudView Hud => _hud;

        public void Render(TableResponse state)
        {
            if (state == null) return;

            var game = state.Game;
            var players = state.Players;

            // HUD
            _hud.Render(game);

            // Pot
            _potLabel.text = game.Pot > 0 ? $"Pot: {MoneyFormatter.Format(game.Pot)}" : "";

            // Community cards
            _communityCards.Render(game.CommunityCards);

            // Seats — map players by seat number
            var playerBySeat = players?.ToDictionary(p => p.Seat) ?? new Dictionary<int, PlayerState>();

            for (int i = 1; i <= 6; i++)
            {
                playerBySeat.TryGetValue(i, out var player);
                _seats[i].Render(player, game);
            }
        }
    }
}
