using System;
using UnityEngine.UIElements;

namespace HijackPoker.UI
{
    public class ControlsView
    {
        private readonly Button _stepButton;
        private readonly Button _autoButton;
        private readonly Button _speedButton;

        private bool _isAutoPlaying;
        private int _speedIndex;
        private readonly float[] _speeds = { 1f, 0.5f, 0.25f, 2f };
        private readonly string[] _speedLabels = { "1s", "0.5s", "0.25s", "2s" };

        public event Action OnNextStep;
        public event Action<bool> OnAutoPlayToggled;
        public float CurrentSpeed => _speeds[_speedIndex];

        public ControlsView(Button stepButton, Button autoButton, Button speedButton)
        {
            _stepButton = stepButton;
            _autoButton = autoButton;
            _speedButton = speedButton;

            _stepButton.clicked += () => OnNextStep?.Invoke();
            _autoButton.clicked += ToggleAutoPlay;
            _speedButton.clicked += CycleSpeed;
        }

        public void SetProcessing(bool isProcessing)
        {
            _stepButton.SetEnabled(!isProcessing && !_isAutoPlaying);
        }

        private void ToggleAutoPlay()
        {
            _isAutoPlaying = !_isAutoPlaying;

            _autoButton.text = _isAutoPlaying ? "Stop" : "Auto Play";
            _autoButton.EnableInClassList("running", _isAutoPlaying);
            _stepButton.SetEnabled(!_isAutoPlaying);

            OnAutoPlayToggled?.Invoke(_isAutoPlaying);
        }

        public void StopAutoPlay()
        {
            if (!_isAutoPlaying) return;

            _isAutoPlaying = false;
            _autoButton.text = "Auto Play";
            _autoButton.EnableInClassList("running", false);
            _stepButton.SetEnabled(true);
        }

        private void CycleSpeed()
        {
            _speedIndex = (_speedIndex + 1) % _speeds.Length;
            _speedButton.text = _speedLabels[_speedIndex];
        }
    }
}
