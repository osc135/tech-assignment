using System.Collections.Generic;
using NUnit.Framework;
using HijackPoker.Managers;
using HijackPoker.Models;

namespace HijackPoker.Tests
{
    [TestFixture]
    public class TableStateManagerTests
    {
        private TableStateManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new TableStateManager();
        }

        // ── Basic state updates ────────────────────────────────────

        [Test]
        public void UpdateState_SetsCurrentState()
        {
            var state = CreateState(gameNo: 1, handStep: 5);

            _manager.UpdateState(state);

            Assert.IsNotNull(_manager.CurrentState);
            Assert.AreEqual(5, _manager.CurrentState.Game.HandStep);
        }

        [Test]
        public void UpdateState_SetsPreviousState()
        {
            var state1 = CreateState(gameNo: 1, handStep: 5);
            var state2 = CreateState(gameNo: 1, handStep: 6);

            _manager.UpdateState(state1);
            _manager.UpdateState(state2);

            Assert.IsNotNull(_manager.PreviousState);
            Assert.AreEqual(5, _manager.PreviousState.Game.HandStep);
            Assert.AreEqual(6, _manager.CurrentState.Game.HandStep);
        }

        [Test]
        public void UpdateState_FiresOnStateChanged()
        {
            TableResponse received = null;
            _manager.OnStateChanged += (s) => received = s;

            var state = CreateState(gameNo: 1, handStep: 5);
            _manager.UpdateState(state);

            Assert.IsNotNull(received);
            Assert.AreEqual(5, received.Game.HandStep);
        }

        [Test]
        public void UpdateState_FiresOnHandCompleted_WhenHandComplete()
        {
            TableResponse completed = null;
            _manager.OnHandCompleted += (s) => completed = s;

            var state = CreateState(gameNo: 1, handStep: 15, stepName: "RECORD_STATS_AND_NEW_HAND");
            _manager.UpdateState(state);

            Assert.IsNotNull(completed);
        }

        [Test]
        public void UpdateState_DoesNotFireOnHandCompleted_WhenNotComplete()
        {
            TableResponse completed = null;
            _manager.OnHandCompleted += (s) => completed = s;

            var state = CreateState(gameNo: 1, handStep: 6, stepName: "DEAL_FLOP");
            _manager.UpdateState(state);

            Assert.IsNull(completed);
        }

        // ── Dealer rotation ────────────────────────────────────────

        [Test]
        public void CorrectPositions_RotatesDealerByGameNo()
        {
            // gameNo 1 => dealerIndex 0 => seat 1
            var state1 = CreateState(gameNo: 1, handStep: 5);
            _manager.UpdateState(state1);
            Assert.AreEqual(1, _manager.CurrentState.Game.DealerSeat);

            // gameNo 2 => dealerIndex 1 => seat 2
            var state2 = CreateState(gameNo: 2, handStep: 5);
            _manager.UpdateState(state2);
            Assert.AreEqual(2, _manager.CurrentState.Game.DealerSeat);

            // gameNo 3 => dealerIndex 2 => seat 3
            var state3 = CreateState(gameNo: 3, handStep: 5);
            _manager.UpdateState(state3);
            Assert.AreEqual(3, _manager.CurrentState.Game.DealerSeat);
        }

        [Test]
        public void CorrectPositions_DealerWrapsAround()
        {
            // gameNo 7 => dealerIndex 0 (wraps with 6 players) => seat 1
            var state = CreateState(gameNo: 7, handStep: 5);
            _manager.UpdateState(state);
            Assert.AreEqual(1, _manager.CurrentState.Game.DealerSeat);
        }

        [Test]
        public void CorrectPositions_DoesNotChangeSbBb()
        {
            // SB/BB come from the backend (seats 2 and 3 in our fixture)
            var state = CreateState(gameNo: 4, handStep: 5);
            _manager.UpdateState(state);

            // Dealer should rotate to seat 4, but SB/BB stay as backend set them
            Assert.AreEqual(4, _manager.CurrentState.Game.DealerSeat);
            Assert.AreEqual(2, _manager.CurrentState.Game.SmallBlindSeat);
            Assert.AreEqual(3, _manager.CurrentState.Game.BigBlindSeat);
        }

        // ── Stack offsets ──────────────────────────────────────────

        [Test]
        public void StackOffsets_ClampsToZero()
        {
            // First hand — Alice at $150
            var state1 = CreateState(gameNo: 1, handStep: 15, stepName: "RECORD_STATS_AND_NEW_HAND");
            state1.Players[0].Stack = 10f; // Alice lost a lot
            _manager.UpdateState(state1);

            // New hand — backend resets Alice to $150
            // Offset = 10 - 150 = -140, so displayed = 150 + (-140) = 10
            var state2 = CreateState(gameNo: 2, handStep: 5);
            state2.Players[0].Stack = 150f;
            _manager.UpdateState(state2);

            Assert.AreEqual(10f, _manager.CurrentState.Players[0].Stack);
        }

        [Test]
        public void StackOffsets_NeverGoesNegative()
        {
            // First hand — Alice ends with $5
            var state1 = CreateState(gameNo: 1, handStep: 15, stepName: "RECORD_STATS_AND_NEW_HAND");
            state1.Players[0].Stack = 5f;
            _manager.UpdateState(state1);

            // New hand — backend resets to $150, offset = 5 - 150 = -145
            // Then during the hand Alice bets and has $2 left
            // Displayed = 2 + (-145) = -143 => clamped to 0
            var state2 = CreateState(gameNo: 2, handStep: 9);
            state2.Players[0].Stack = 2f;
            _manager.UpdateState(state2);

            Assert.GreaterOrEqual(_manager.CurrentState.Players[0].Stack, 0f);
        }

        // ── HasState ───────────────────────────────────────────────

        [Test]
        public void HasState_FalseBeforeFirstUpdate()
        {
            Assert.IsFalse(_manager.HasState);
        }

        [Test]
        public void HasState_TrueAfterUpdate()
        {
            _manager.UpdateState(CreateState(gameNo: 1, handStep: 0));
            Assert.IsTrue(_manager.HasState);
        }

        // ── Helpers ────────────────────────────────────────────────

        private static TableResponse CreateState(int gameNo, int handStep, string stepName = "")
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
}
