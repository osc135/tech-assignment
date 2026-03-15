using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HijackPoker.UI
{
    public class HandHistoryView
    {
        private readonly VisualElement _panel;
        private readonly ScrollView _scrollView;
        private readonly Button _toggleButton;
        private bool _isOpen;

        public HandHistoryView(VisualElement parent)
        {
            // Toggle button — sits with the other controls
            _toggleButton = new Button(Toggle);
            _toggleButton.text = "History";
            _toggleButton.AddToClassList("btn-history");
            parent.Add(_toggleButton);

            // Drop-up panel — expands above the controls
            _panel = new VisualElement();
            _panel.AddToClassList("history-panel");
            _panel.style.display = DisplayStyle.None;
            parent.Add(_panel);

            var header = new VisualElement();
            header.AddToClassList("history-header");

            var title = new Label("Hand History");
            title.AddToClassList("history-title");
            header.Add(title);

            _panel.Add(header);

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.AddToClassList("history-scroll");
            _panel.Add(_scrollView);
        }

        public void UpdateEntries(IReadOnlyList<string> entries)
        {
            _scrollView.Clear();

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry))
                {
                    var spacer = new VisualElement();
                    spacer.AddToClassList("history-spacer");
                    _scrollView.Add(spacer);
                }
                else
                {
                    var label = new Label(entry);
                    label.AddToClassList("history-entry");
                    _scrollView.Add(label);
                }
            }

            // Auto-scroll to bottom
            _scrollView.schedule.Execute(() =>
            {
                _scrollView.scrollOffset = new UnityEngine.Vector2(0, float.MaxValue);
            });
        }

        private void Toggle()
        {
            _isOpen = !_isOpen;
            _panel.style.display = _isOpen ? DisplayStyle.Flex : DisplayStyle.None;
            _toggleButton.EnableInClassList("active", _isOpen);
        }
    }
}
