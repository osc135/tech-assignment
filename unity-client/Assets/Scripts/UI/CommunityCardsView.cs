using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HijackPoker.UI
{
    public class CommunityCardsView
    {
        private readonly VisualElement _container;

        public CommunityCardsView(VisualElement container)
        {
            _container = container;
        }

        public void Render(List<string> communityCards)
        {
            _container.Clear();

            var cards = communityCards ?? new List<string>();

            for (int i = 0; i < 5; i++)
            {
                if (i < cards.Count && !string.IsNullOrEmpty(cards[i]))
                {
                    _container.Add(CardView.CreateFaceUp(cards[i]));
                }
                else
                {
                    _container.Add(CardView.CreateEmpty());
                }
            }
        }
    }
}
