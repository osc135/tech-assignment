using UnityEngine.UIElements;
using HijackPoker.Utils;

namespace HijackPoker.UI
{
    public static class CardView
    {
        public static VisualElement CreateFaceUp(string cardStr, bool small = false)
        {
            var card = new VisualElement();
            card.AddToClassList("card");
            if (small) card.AddToClassList("card-small");

            if (!CardUtils.TryParse(cardStr, out var rank, out var suit))
            {
                card.AddToClassList("empty");
                return card;
            }

            card.AddToClassList(CardUtils.GetColorClass(suit));

            var label = new Label(CardUtils.GetDisplayString(cardStr));
            label.AddToClassList("card-label");
            card.Add(label);

            var back = new VisualElement();
            back.AddToClassList("card-back");
            card.Add(back);

            return card;
        }

        public static VisualElement CreateFaceDown(bool small = false)
        {
            var card = new VisualElement();
            card.AddToClassList("card");
            card.AddToClassList("face-down");
            if (small) card.AddToClassList("card-small");

            var label = new Label();
            label.AddToClassList("card-label");
            card.Add(label);

            var back = new VisualElement();
            back.AddToClassList("card-back");
            card.Add(back);

            return card;
        }

        public static VisualElement CreateEmpty(bool small = false)
        {
            var card = new VisualElement();
            card.AddToClassList("card");
            card.AddToClassList("empty");
            if (small) card.AddToClassList("card-small");

            var label = new Label();
            label.AddToClassList("card-label");
            card.Add(label);

            return card;
        }
    }
}
