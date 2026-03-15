using System;
using System.Threading.Tasks;
using HijackPoker.Api;
using HijackPoker.Models;
using UnityEngine;

namespace HijackPoker.Managers
{
    public class GameManager
    {
        private readonly IPokerApi _api;
        private readonly TableStateManager _stateManager;
        private readonly int _tableId;

        public bool IsProcessing { get; private set; }
        public bool IsConnected { get; private set; }

        public event Action<bool> OnProcessingChanged;
        public event Action<string> OnError;
        public event Action<bool> OnConnectionChanged;

        public TableStateManager StateManager => _stateManager;

        public GameManager(IPokerApi api, TableStateManager stateManager, int tableId = 1)
        {
            _api = api;
            _stateManager = stateManager;
            _tableId = tableId;
        }

        public async Task CheckConnection()
        {
            try
            {
                var health = await _api.GetHealthAsync();
                IsConnected = health != null && health.Status == "ok";
                OnConnectionChanged?.Invoke(IsConnected);
            }
            catch (Exception e)
            {
                Debug.LogError($"Health check failed: {e.Message}");
                IsConnected = false;
                OnConnectionChanged?.Invoke(false);
            }
        }

        public async Task LoadCurrentState()
        {
            try
            {
                var state = await _api.GetTableStateAsync(_tableId);
                if (state != null)
                {
                    _stateManager.UpdateState(state);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load table state: {e.Message}");
                OnError?.Invoke("Failed to load table state");
            }
        }

        public async Task AdvanceStep()
        {
            if (IsProcessing) return;

            IsProcessing = true;
            OnProcessingChanged?.Invoke(true);

            try
            {
                var processResult = await _api.ProcessStepAsync(_tableId);

                if (processResult == null || !processResult.Success)
                {
                    var errorMsg = processResult?.Error ?? "Unknown error";
                    Debug.LogError($"Process step failed: {errorMsg}");
                    OnError?.Invoke(errorMsg);
                    return;
                }

                var state = await _api.GetTableStateAsync(_tableId);
                if (state != null)
                {
                    _stateManager.UpdateState(state);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"AdvanceStep failed: {e.Message}");
                OnError?.Invoke(e.Message);
            }
            finally
            {
                IsProcessing = false;
                OnProcessingChanged?.Invoke(false);
            }
        }
    }
}
