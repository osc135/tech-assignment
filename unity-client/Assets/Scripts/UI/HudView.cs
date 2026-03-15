using UnityEngine.UIElements;
using HijackPoker.Models;
using HijackPoker.Utils;

namespace HijackPoker.UI
{
    public class HudView
    {
        private readonly Label _phaseLabel;

        public HudView(Label phaseLabel)
        {
            _phaseLabel = phaseLabel;
        }

        public void Render(GameState game)
        {
            if (game == null)
            {
                _phaseLabel.text = "Ready \u2014 click Next Step to begin";
                return;
            }

            var label = PhaseLabels.GetLabel(game.StepName);

            if (game.IsHandComplete)
            {
                _phaseLabel.text = $"Hand #{game.GameNo} Complete";
            }
            else
            {
                _phaseLabel.text = $"Hand #{game.GameNo} \u2014 {label}";
            }
        }

        public void SetConnectionStatus(string text)
        {
            _phaseLabel.text = text;
        }
    }
}
