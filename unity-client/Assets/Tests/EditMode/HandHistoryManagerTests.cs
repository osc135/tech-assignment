using System.Collections.Generic;
using NUnit.Framework;
using HijackPoker.Managers;
using HijackPoker.Models;

namespace HijackPoker.Tests
{
    [TestFixture]
    public class HandHistoryManagerTests
    {
        private HandHistoryManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new HandHistoryManager();
        }

        // ── Basic entry creation ─────────────────────────────────────

        [Test]
        public void ProcessState_AddsEntry_ForValidState()
        {
            var state = CreateState(gameNo: 1, handStep: 0, stepName: "GAME_PREP");

            _manager.ProcessState(state);

            Assert.AreEqual(1, _manager.Entries.Count);
            StringAssert.Contains("Hand #1", _manager.Entries[0]);
            StringAssert.Contains("Preparing Hand", _manager.Entries[0]);
        }

        [Test]
        public void ProcessState_DoesNothing_WhenNull()
        {
            _manager.ProcessState(null);

            Assert.AreEqual(0, _manager.Entries.Count);
        }

        [Test]
        public void ProcessState_DoesNothing_WhenGameNull()
        {
            _manager.ProcessState(new TableResponse { Game = null });

            Assert.AreEqual(0, _manager.Entries.Count);
        }

        // ── Duplicate detection ──────────────────────────────────────

        [Test]
        public void ProcessState_SkipsDuplicate_SameStepAndHand()
        {
            var state = CreateState(gameNo: 1, handStep: 5, stepName: "PRE_FLOP_BETTING_ROUND");

            _manager.ProcessState(state);
            _manager.ProcessState(state);

            Assert.AreEqual(1, _manager.Entries.Count);
        }

        [Test]
        public void ProcessState_DoesNotSkip_DifferentStep()
        {
            _manager.ProcessState(CreateState(gameNo: 1, handStep: 5, stepName: "PRE_FLOP_BETTING_ROUND"));
            _manager.ProcessState(CreateState(gameNo: 1, handStep: 6, stepName: "DEAL_FLOP"));

            Assert.AreEqual(2, _manager.Entries.Count);
        }

        // ── Hand separator ───────────────────────────────────────────

        [Test]
        public void ProcessState_AddsBlankSeparator_WhenNewHandStarts()
        {
            _manager.ProcessState(CreateState(gameNo: 1, handStep: 15, stepName: "RECORD_STATS_AND_NEW_HAND"));
            _manager.ProcessState(CreateState(gameNo: 2, handStep: 0, stepName: "GAME_PREP"));

            // Entry for hand 1 + blank separator + entry for hand 2
            Assert.AreEqual(3, _manager.Entries.Count);
            Assert.AreEqual("", _manager.Entries[1]);
        }

        [Test]
        public void ProcessState_NoSeparator_ForFirstHand()
        {
            _manager.ProcessState(CreateState(gameNo: 1, handStep: 0, stepName: "GAME_PREP"));

            Assert.AreEqual(1, _manager.Entries.Count);
            Assert.AreNotEqual("", _manager.Entries[0]);
        }

        // ── Step-specific formatting ─────────────────────────────────

        [Test]
        public void ProcessState_FormatsSmallBlind_WithPlayerName()
        {
            var state = CreateState(gameNo: 1, handStep: 2, stepName: "SETUP_SMALL_BLIND");
            state.Game.SmallBlindSeat = 2;
            state.Game.SmallBlind = 1f;

            _manager.ProcessState(state);

            StringAssert.Contains("Bob posts SB $1.00", _manager.Entries[0]);
        }

        [Test]
        public void ProcessState_FormatsBigBlind_WithPlayerName()
        {
            var state = CreateState(gameNo: 1, handStep: 3, stepName: "SETUP_BIG_BLIND");
            state.Game.BigBlindSeat = 3;
            state.Game.BigBlind = 2f;

            _manager.ProcessState(state);

            StringAssert.Contains("Charlie posts BB $2.00", _manager.Entries[0]);
        }

        [Test]
        public void ProcessState_FormatsFlopCards()
        {
            var state = CreateState(gameNo: 1, handStep: 6, stepName: "DEAL_FLOP");
            state.Game.CommunityCards = new List<string> { "AH", "KD", "7C" };

            _manager.ProcessState(state);

            StringAssert.Contains("Flop:", _manager.Entries[0]);
            StringAssert.Contains("A\u2665", _manager.Entries[0]);
            StringAssert.Contains("K\u2666", _manager.Entries[0]);
            StringAssert.Contains("7\u2663", _manager.Entries[0]);
        }

        [Test]
        public void ProcessState_FormatsTurnCard()
        {
            var state = CreateState(gameNo: 1, handStep: 8, stepName: "DEAL_TURN");
            state.Game.CommunityCards = new List<string> { "AH", "KD", "7C", "QS" };

            _manager.ProcessState(state);

            StringAssert.Contains("Turn: Q\u2660", _manager.Entries[0]);
        }

        [Test]
        public void ProcessState_FormatsRiverCard()
        {
            var state = CreateState(gameNo: 1, handStep: 10, stepName: "DEAL_RIVER");
            state.Game.CommunityCards = new List<string> { "AH", "KD", "7C", "QS", "2H" };

            _manager.ProcessState(state);

            StringAssert.Contains("River: 2\u2665", _manager.Entries[0]);
        }

        [Test]
        public void ProcessState_FormatsHandComplete()
        {
            var state = CreateState(gameNo: 1, handStep: 15, stepName: "RECORD_STATS_AND_NEW_HAND");

            _manager.ProcessState(state);

            StringAssert.Contains("Hand Complete", _manager.Entries[0]);
        }

        // ── Betting round entries ────────────────────────────────────

        [Test]
        public void ProcessState_FormatsBettingRound_WithPlayerActions()
        {
            var state = CreateState(gameNo: 1, handStep: 5, stepName: "PRE_FLOP_BETTING_ROUND");
            state.Players[0].Action = "call";
            state.Players[1].Action = "fold";
            state.Players[2].Action = "check";

            _manager.ProcessState(state);

            StringAssert.Contains("Alice calls", _manager.Entries[0]);
            StringAssert.Contains("Bob folds", _manager.Entries[0]);
            StringAssert.Contains("Charlie checks", _manager.Entries[0]);
        }

        [Test]
        public void ProcessState_FormatsBet_WithAmount()
        {
            var state = CreateState(gameNo: 1, handStep: 7, stepName: "FLOP_BETTING_ROUND");
            state.Players[0].Action = "bet";
            state.Players[0].Bet = 10f;

            _manager.ProcessState(state);

            StringAssert.Contains("Alice bets $10.00", _manager.Entries[0]);
        }

        [Test]
        public void ProcessState_FormatsRaise_WithAmount()
        {
            var state = CreateState(gameNo: 1, handStep: 7, stepName: "FLOP_BETTING_ROUND");
            state.Players[0].Action = "raise";
            state.Players[0].Bet = 20f;

            _manager.ProcessState(state);

            StringAssert.Contains("Alice raises to $20.00", _manager.Entries[0]);
        }

        [Test]
        public void ProcessState_FormatsAllIn()
        {
            var state = CreateState(gameNo: 1, handStep: 9, stepName: "TURN_BETTING_ROUND");
            state.Players[0].Action = "allin";

            _manager.ProcessState(state);

            StringAssert.Contains("Alice all-in", _manager.Entries[0]);
        }

        // ── Winner formatting ────────────────────────────────────────

        [Test]
        public void ProcessState_FormatsWinner_WithHandRankAndWinnings()
        {
            var state = CreateState(gameNo: 1, handStep: 14, stepName: "PAY_WINNERS");
            state.Players[0].Winnings = 12f;
            state.Players[0].HandRank = "Full House";

            _manager.ProcessState(state);

            StringAssert.Contains("Alice wins +$12.00 (Full House)", _manager.Entries[0]);
        }

        [Test]
        public void ProcessState_FormatsMultipleWinners()
        {
            var state = CreateState(gameNo: 1, handStep: 13, stepName: "FIND_WINNERS");
            state.Players[0].Winnings = 6f;
            state.Players[0].HandRank = "Two Pair";
            state.Players[1].Winnings = 6f;
            state.Players[1].HandRank = "Two Pair";

            _manager.ProcessState(state);

            StringAssert.Contains("Alice wins +$6.00 (Two Pair)", _manager.Entries[0]);
            StringAssert.Contains("Bob wins +$6.00 (Two Pair)", _manager.Entries[0]);
        }

        // ── Max entries cap ──────────────────────────────────────────

        [Test]
        public void ProcessState_CapsEntriesAt100()
        {
            for (int i = 0; i < 110; i++)
            {
                _manager.ProcessState(CreateState(gameNo: 1, handStep: i, stepName: "GAME_PREP"));
            }

            Assert.LessOrEqual(_manager.Entries.Count, 100);
        }

        // ── Events ───────────────────────────────────────────────────

        [Test]
        public void ProcessState_FiresOnEntriesChanged()
        {
            int fireCount = 0;
            _manager.OnEntriesChanged += () => fireCount++;

            _manager.ProcessState(CreateState(gameNo: 1, handStep: 0, stepName: "GAME_PREP"));

            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void ProcessState_DoesNotFireEvent_OnDuplicate()
        {
            int fireCount = 0;
            var state = CreateState(gameNo: 1, handStep: 0, stepName: "GAME_PREP");

            _manager.ProcessState(state);
            _manager.OnEntriesChanged += () => fireCount++;
            _manager.ProcessState(state); // duplicate

            Assert.AreEqual(0, fireCount);
        }

        // ── Clear ────────────────────────────────────────────────────

        [Test]
        public void Clear_RemovesAllEntries()
        {
            _manager.ProcessState(CreateState(gameNo: 1, handStep: 0, stepName: "GAME_PREP"));
            _manager.ProcessState(CreateState(gameNo: 1, handStep: 1, stepName: "SETUP_DEALER"));

            _manager.Clear();

            Assert.AreEqual(0, _manager.Entries.Count);
        }

        [Test]
        public void Clear_ResetsTracking_SoSameStateCanBeAddedAgain()
        {
            var state = CreateState(gameNo: 1, handStep: 0, stepName: "GAME_PREP");
            _manager.ProcessState(state);
            _manager.Clear();
            _manager.ProcessState(state);

            Assert.AreEqual(1, _manager.Entries.Count);
        }

        // ── Helpers ──────────────────────────────────────────────────

        private static TableResponse CreateState(int gameNo, int handStep, string stepName)
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
                    new PlayerState { PlayerId = 1, Username = "Alice", Seat = 1, Stack = 150f, Status = "1", Cards = new List<string>(), Action = "" },
                    new PlayerState { PlayerId = 2, Username = "Bob", Seat = 2, Stack = 150f, Status = "1", Cards = new List<string>(), Action = "" },
                    new PlayerState { PlayerId = 3, Username = "Charlie", Seat = 3, Stack = 150f, Status = "1", Cards = new List<string>(), Action = "" },
                    new PlayerState { PlayerId = 4, Username = "Diana", Seat = 4, Stack = 150f, Status = "1", Cards = new List<string>(), Action = "" },
                    new PlayerState { PlayerId = 5, Username = "Eve", Seat = 5, Stack = 150f, Status = "1", Cards = new List<string>(), Action = "" },
                    new PlayerState { PlayerId = 6, Username = "Frank", Seat = 6, Stack = 150f, Status = "1", Cards = new List<string>(), Action = "" }
                }
            };
        }
    }
}
