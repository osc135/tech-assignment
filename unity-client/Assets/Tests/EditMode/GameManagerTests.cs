using System;
using System.Threading.Tasks;
using NUnit.Framework;
using HijackPoker.Api;
using HijackPoker.Managers;
using HijackPoker.Models;
using System.Collections.Generic;

namespace HijackPoker.Tests
{
    [TestFixture]
    public class GameManagerTests
    {
        private MockPokerApi _mockApi;
        private TableStateManager _stateManager;
        private GameManager _gameManager;

        [SetUp]
        public void SetUp()
        {
            _mockApi = new MockPokerApi();
            _stateManager = new TableStateManager();
            _gameManager = new GameManager(_mockApi, _stateManager, tableId: 1);
        }

        // ── CheckConnection ────────────────────────────────────────

        [Test]
        public async Task CheckConnection_SetsConnected_WhenHealthOk()
        {
            _mockApi.HealthResponse = new HealthResponse
            {
                Service = "holdem-processor",
                Status = "ok",
                Timestamp = "2026-01-01T00:00:00Z"
            };

            await _gameManager.CheckConnection();

            Assert.IsTrue(_gameManager.IsConnected);
        }

        [Test]
        public async Task CheckConnection_SetsDisconnected_WhenHealthFails()
        {
            _mockApi.ShouldThrow = true;

            await _gameManager.CheckConnection();

            Assert.IsFalse(_gameManager.IsConnected);
        }

        [Test]
        public async Task CheckConnection_SetsDisconnected_WhenStatusNotOk()
        {
            _mockApi.HealthResponse = new HealthResponse
            {
                Service = "holdem-processor",
                Status = "error",
                Timestamp = "2026-01-01T00:00:00Z"
            };

            await _gameManager.CheckConnection();

            Assert.IsFalse(_gameManager.IsConnected);
        }

        [Test]
        public async Task CheckConnection_FiresConnectionChangedEvent()
        {
            bool? eventValue = null;
            _gameManager.OnConnectionChanged += (connected) => eventValue = connected;

            _mockApi.HealthResponse = new HealthResponse { Status = "ok" };
            await _gameManager.CheckConnection();

            Assert.IsTrue(eventValue);
        }

        // ── LoadCurrentState ───────────────────────────────────────

        [Test]
        public async Task LoadCurrentState_UpdatesStateManager()
        {
            _mockApi.TableResponse = CreateTableResponse(gameNo: 1, handStep: 5);

            await _gameManager.LoadCurrentState();

            Assert.IsNotNull(_stateManager.CurrentState);
            Assert.AreEqual(5, _stateManager.CurrentState.Game.HandStep);
        }

        [Test]
        public async Task LoadCurrentState_FiresOnError_WhenApiFails()
        {
            _mockApi.ShouldThrow = true;
            string errorMsg = null;
            _gameManager.OnError += (msg) => errorMsg = msg;

            await _gameManager.LoadCurrentState();

            Assert.IsNotNull(errorMsg);
        }

        [Test]
        public async Task LoadCurrentState_DoesNotUpdateState_WhenNull()
        {
            _mockApi.TableResponse = null;

            await _gameManager.LoadCurrentState();

            Assert.IsFalse(_stateManager.HasState);
        }

        // ── AdvanceStep ────────────────────────────────────────────

        [Test]
        public async Task AdvanceStep_CallsProcessThenGetTable()
        {
            _mockApi.ProcessResponse = new ProcessResponse
            {
                Success = true,
                Result = new ProcessResult { Status = "processed", TableId = 1, Step = 6, StepName = "DEAL_FLOP" }
            };
            _mockApi.TableResponse = CreateTableResponse(gameNo: 1, handStep: 6);

            await _gameManager.AdvanceStep();

            Assert.AreEqual(1, _mockApi.ProcessCallCount);
            Assert.AreEqual(1, _mockApi.GetTableCallCount);
            Assert.AreEqual(6, _stateManager.CurrentState.Game.HandStep);
        }

        [Test]
        public async Task AdvanceStep_SetsProcessingFlag_DuringExecution()
        {
            var processingStates = new List<bool>();
            _gameManager.OnProcessingChanged += (p) => processingStates.Add(p);

            _mockApi.ProcessResponse = new ProcessResponse
            {
                Success = true,
                Result = new ProcessResult { Status = "processed", TableId = 1, Step = 6 }
            };
            _mockApi.TableResponse = CreateTableResponse(gameNo: 1, handStep: 6);

            await _gameManager.AdvanceStep();

            Assert.AreEqual(2, processingStates.Count);
            Assert.IsTrue(processingStates[0]);   // started processing
            Assert.IsFalse(processingStates[1]);   // finished processing
            Assert.IsFalse(_gameManager.IsProcessing);
        }

        [Test]
        public async Task AdvanceStep_PreventsDoubleClick()
        {
            _mockApi.ProcessResponse = new ProcessResponse
            {
                Success = true,
                Result = new ProcessResult { Status = "processed", TableId = 1, Step = 6 }
            };
            _mockApi.TableResponse = CreateTableResponse(gameNo: 1, handStep: 6);

            // Simulate rapid double-click
            var task1 = _gameManager.AdvanceStep();
            var task2 = _gameManager.AdvanceStep(); // should be ignored
            await Task.WhenAll(task1, task2);

            Assert.AreEqual(1, _mockApi.ProcessCallCount);
        }

        [Test]
        public async Task AdvanceStep_FiresError_WhenProcessFails()
        {
            _mockApi.ProcessResponse = new ProcessResponse
            {
                Success = false,
                Error = "table not found"
            };

            string errorMsg = null;
            _gameManager.OnError += (msg) => errorMsg = msg;

            await _gameManager.AdvanceStep();

            Assert.AreEqual("table not found", errorMsg);
            Assert.AreEqual(0, _mockApi.GetTableCallCount); // should not fetch table
        }

        [Test]
        public async Task AdvanceStep_FiresError_WhenExceptionThrown()
        {
            _mockApi.ShouldThrow = true;
            string errorMsg = null;
            _gameManager.OnError += (msg) => errorMsg = msg;

            await _gameManager.AdvanceStep();

            Assert.IsNotNull(errorMsg);
            Assert.IsFalse(_gameManager.IsProcessing); // still resets
        }

        [Test]
        public async Task AdvanceStep_FiresStateChanged()
        {
            _mockApi.ProcessResponse = new ProcessResponse
            {
                Success = true,
                Result = new ProcessResult { Status = "processed", TableId = 1, Step = 6 }
            };
            _mockApi.TableResponse = CreateTableResponse(gameNo: 1, handStep: 6);

            TableResponse receivedState = null;
            _stateManager.OnStateChanged += (state) => receivedState = state;

            await _gameManager.AdvanceStep();

            Assert.IsNotNull(receivedState);
            Assert.AreEqual(6, receivedState.Game.HandStep);
        }

        // ── Helpers ────────────────────────────────────────────────

        private static TableResponse CreateTableResponse(int gameNo, int handStep, string stepName = "")
        {
            return new TableResponse
            {
                Game = new GameState
                {
                    Id = 1,
                    TableId = 1,
                    TableName = "Starter Table",
                    GameNo = gameNo,
                    HandStep = handStep,
                    StepName = stepName,
                    DealerSeat = 1,
                    SmallBlindSeat = 2,
                    BigBlindSeat = 3,
                    CommunityCards = new List<string>(),
                    Pot = 0,
                    SidePots = new List<SidePot>(),
                    Move = 0,
                    Status = "in_progress",
                    SmallBlind = 1f,
                    BigBlind = 2f,
                    MaxSeats = 6,
                    CurrentBet = 0,
                    Winners = new List<Winner>()
                },
                Players = new List<PlayerState>
                {
                    new PlayerState { PlayerId = 1, Username = "Alice", Seat = 1, Stack = 150f, Status = "1", Cards = new List<string>() },
                    new PlayerState { PlayerId = 2, Username = "Bob", Seat = 2, Stack = 150f, Status = "1", Cards = new List<string>() },
                    new PlayerState { PlayerId = 3, Username = "Charlie", Seat = 3, Stack = 150f, Status = "1", Cards = new List<string>() },
                    new PlayerState { PlayerId = 4, Username = "Diana", Seat = 4, Stack = 150f, Status = "1", Cards = new List<string>() },
                    new PlayerState { PlayerId = 5, Username = "Eve", Seat = 5, Stack = 150f, Status = "1", Cards = new List<string>() },
                    new PlayerState { PlayerId = 6, Username = "Frank", Seat = 6, Stack = 150f, Status = "1", Cards = new List<string>() }
                }
            };
        }
    }

    // ── Mock API ───────────────────────────────────────────────────

    public class MockPokerApi : IPokerApi
    {
        public HealthResponse HealthResponse;
        public ProcessResponse ProcessResponse;
        public TableResponse TableResponse;
        public bool ShouldThrow;

        public int ProcessCallCount { get; private set; }
        public int GetTableCallCount { get; private set; }
        public int HealthCallCount { get; private set; }

        public Task<HealthResponse> GetHealthAsync()
        {
            HealthCallCount++;
            if (ShouldThrow) throw new Exception("Mock API error");
            return Task.FromResult(HealthResponse);
        }

        public Task<ProcessResponse> ProcessStepAsync(int tableId)
        {
            ProcessCallCount++;
            if (ShouldThrow) throw new Exception("Mock API error");
            return Task.FromResult(ProcessResponse);
        }

        public Task<TableResponse> GetTableStateAsync(int tableId)
        {
            GetTableCallCount++;
            if (ShouldThrow) throw new Exception("Mock API error");
            return Task.FromResult(TableResponse);
        }
    }
}
