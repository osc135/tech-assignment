using UnityEngine;
using UnityEngine.UIElements;
using HijackPoker.Api;
using HijackPoker.UI;

namespace HijackPoker.Managers
{
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(PokerApiClient))]
    public class GameBootstrap : MonoBehaviour
    {
        private GameManager _gameManager;
        private TableStateManager _stateManager;
        private HandHistoryManager _historyManager;
        private TableView _tableView;
        private ControlsView _controlsView;
        private HandHistoryView _historyView;

        private bool _autoPlaying;
        private float _autoPlayTimer;

        private async void Start()
        {
            // Get references
            var apiClient = GetComponent<PokerApiClient>();
            var uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;

            // Create managers
            _stateManager = new TableStateManager();
            _gameManager = new GameManager(apiClient, _stateManager);
            _historyManager = new HandHistoryManager();

            // Create views
            _tableView = new TableView(root);
            var controlsContainer = root.Q<VisualElement>(className: "controls");
            _controlsView = new ControlsView(
                root.Q<Button>("btn-step"),
                root.Q<Button>("btn-auto"),
                root.Q<Button>("btn-speed")
            );
            _historyView = new HandHistoryView(controlsContainer);

            // Wire events
            _stateManager.OnStateChanged += state => _tableView.Render(state);
            _stateManager.OnStateChanged += state => _historyManager.ProcessState(state);
            _historyManager.OnEntriesChanged += () => _historyView.UpdateEntries(_historyManager.Entries);

            _gameManager.OnProcessingChanged += processing =>
                _controlsView.SetProcessing(processing);

            _gameManager.OnError += error =>
            {
                var errorLabel = root.Q<Label>("error-message");
                if (errorLabel != null) errorLabel.text = error;
            };

            _gameManager.OnConnectionChanged += connected =>
            {
                var statusLabel = root.Q<Label>("connection-status");
                if (statusLabel != null)
                {
                    statusLabel.text = connected ? "Connected" : "API not available";
                    statusLabel.EnableInClassList("connected", connected);
                    statusLabel.EnableInClassList("disconnected", !connected);
                }
            };

            _controlsView.OnNextStep += async () => await _gameManager.AdvanceStep();
            _controlsView.OnAutoPlayToggled += playing => _autoPlaying = playing;

            // Check connection and load initial state
            await _gameManager.CheckConnection();

            if (_gameManager.IsConnected)
            {
                await _gameManager.LoadCurrentState();

                if (!_stateManager.HasState)
                {
                    _tableView.Hud.SetConnectionStatus("Ready \u2014 click Next Step to begin");
                }
            }
            else
            {
                _tableView.Hud.SetConnectionStatus("API not available at localhost:3030");
            }
        }

        private void Update()
        {
            if (!_autoPlaying || _gameManager.IsProcessing) return;

            _autoPlayTimer += Time.deltaTime;

            if (_autoPlayTimer >= _controlsView.CurrentSpeed)
            {
                _autoPlayTimer = 0f;
                _ = _gameManager.AdvanceStep();
            }
        }
    }
}
