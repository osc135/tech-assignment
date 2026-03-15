using NUnit.Framework;
using Newtonsoft.Json;
using HijackPoker.Models;

namespace HijackPoker.Tests
{
    [TestFixture]
    public class ModelTests
    {
        private const string PreflopJson = @"{
            ""game"": {
                ""id"": 1, ""tableId"": 1, ""tableName"": ""Starter Table"",
                ""gameNo"": 1, ""handStep"": 5, ""stepName"": ""PRE_FLOP_BETTING_ROUND"",
                ""dealerSeat"": 1, ""smallBlindSeat"": 2, ""bigBlindSeat"": 3,
                ""communityCards"": [], ""pot"": 3.00, ""sidePots"": [],
                ""move"": 4, ""status"": ""in_progress"",
                ""smallBlind"": 1.00, ""bigBlind"": 2.00,
                ""maxSeats"": 6, ""currentBet"": 2.00, ""winners"": []
            },
            ""players"": [
                {
                    ""playerId"": 1, ""username"": ""Alice"", ""seat"": 1,
                    ""stack"": 150.00, ""bet"": 0, ""totalBet"": 0,
                    ""status"": ""1"", ""action"": """",
                    ""cards"": [""AH"", ""KD""], ""handRank"": """", ""winnings"": 0
                },
                {
                    ""playerId"": 4, ""username"": ""Diana"", ""seat"": 4,
                    ""stack"": 150.00, ""bet"": 0, ""totalBet"": 0,
                    ""status"": ""11"", ""action"": ""fold"",
                    ""cards"": [""2C"", ""7S""], ""handRank"": """", ""winnings"": 0
                },
                {
                    ""playerId"": 6, ""username"": ""Frank"", ""seat"": 6,
                    ""stack"": 150.00, ""bet"": 0, ""totalBet"": 0,
                    ""status"": ""12"", ""action"": ""allin"",
                    ""cards"": [""AS"", ""AD""], ""handRank"": """", ""winnings"": 0
                }
            ]
        }";

        private const string ShowdownJson = @"{
            ""game"": {
                ""id"": 1, ""tableId"": 1, ""tableName"": ""Starter Table"",
                ""gameNo"": 1, ""handStep"": 14, ""stepName"": ""PAY_WINNERS"",
                ""dealerSeat"": 1, ""smallBlindSeat"": 2, ""bigBlindSeat"": 3,
                ""communityCards"": [""JH"", ""7D"", ""2C"", ""KS"", ""4H""],
                ""pot"": 12.00, ""sidePots"": [],
                ""move"": 0, ""status"": ""in_progress"",
                ""smallBlind"": 1.00, ""bigBlind"": 2.00,
                ""maxSeats"": 6, ""currentBet"": 0,
                ""winners"": [{ ""seat"": 1, ""playerId"": 1 }]
            },
            ""players"": [
                {
                    ""playerId"": 1, ""username"": ""Alice"", ""seat"": 1,
                    ""stack"": 162.00, ""bet"": 0, ""totalBet"": 4.00,
                    ""status"": ""1"", ""action"": ""call"",
                    ""cards"": [""AH"", ""KD""], ""handRank"": ""Two Pair"", ""winnings"": 12.00
                }
            ]
        }";

        private const string ProcessJson = @"{
            ""success"": true,
            ""result"": {
                ""status"": ""processed"",
                ""tableId"": 1,
                ""step"": 6,
                ""stepName"": ""DEAL_FLOP""
            }
        }";

        private const string HealthJson = @"{
            ""service"": ""holdem-processor"",
            ""status"": ""ok"",
            ""timestamp"": ""2026-02-21T12:00:00.000Z""
        }";

        [Test]
        public void TableResponse_Deserializes_GameState()
        {
            var response = JsonConvert.DeserializeObject<TableResponse>(PreflopJson);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Game);
            Assert.AreEqual(1, response.Game.TableId);
            Assert.AreEqual("Starter Table", response.Game.TableName);
            Assert.AreEqual(5, response.Game.HandStep);
            Assert.AreEqual("PRE_FLOP_BETTING_ROUND", response.Game.StepName);
            Assert.AreEqual(1, response.Game.DealerSeat);
            Assert.AreEqual(2, response.Game.SmallBlindSeat);
            Assert.AreEqual(3, response.Game.BigBlindSeat);
            Assert.AreEqual(3.00f, response.Game.Pot);
            Assert.AreEqual(6, response.Game.MaxSeats);
        }

        [Test]
        public void TableResponse_Deserializes_Players()
        {
            var response = JsonConvert.DeserializeObject<TableResponse>(PreflopJson);

            Assert.AreEqual(3, response.Players.Count);
            Assert.AreEqual("Alice", response.Players[0].Username);
            Assert.AreEqual(1, response.Players[0].Seat);
            Assert.AreEqual(150.00f, response.Players[0].Stack);
            Assert.AreEqual(2, response.Players[0].Cards.Count);
            Assert.AreEqual("AH", response.Players[0].Cards[0]);
            Assert.AreEqual("KD", response.Players[0].Cards[1]);
        }

        [Test]
        public void PlayerState_StatusProperties_WorkCorrectly()
        {
            var response = JsonConvert.DeserializeObject<TableResponse>(PreflopJson);

            var alice = response.Players[0];
            Assert.IsTrue(alice.IsActive);
            Assert.IsFalse(alice.IsFolded);
            Assert.IsFalse(alice.IsAllIn);

            var diana = response.Players[1];
            Assert.IsFalse(diana.IsActive);
            Assert.IsTrue(diana.IsFolded);

            var frank = response.Players[2];
            Assert.IsFalse(frank.IsActive);
            Assert.IsTrue(frank.IsAllIn);
        }

        [Test]
        public void GameState_IsShowdown_FalseBeforeStep12()
        {
            var response = JsonConvert.DeserializeObject<TableResponse>(PreflopJson);

            Assert.IsFalse(response.Game.IsShowdown);
            Assert.IsFalse(response.Game.IsHandComplete);
        }

        [Test]
        public void GameState_IsShowdown_TrueAtStep12OrAbove()
        {
            var response = JsonConvert.DeserializeObject<TableResponse>(ShowdownJson);

            Assert.IsTrue(response.Game.IsShowdown);
            Assert.AreEqual(5, response.Game.CommunityCards.Count);
        }

        [Test]
        public void GameState_Winners_DeserializesCorrectly()
        {
            var response = JsonConvert.DeserializeObject<TableResponse>(ShowdownJson);

            Assert.AreEqual(1, response.Game.Winners.Count);
            Assert.AreEqual(1, response.Game.Winners[0].Seat);
            Assert.AreEqual(1, response.Game.Winners[0].PlayerId);
        }

        [Test]
        public void PlayerState_Winner_HasWinningsAndHandRank()
        {
            var response = JsonConvert.DeserializeObject<TableResponse>(ShowdownJson);

            var alice = response.Players[0];
            Assert.IsTrue(alice.IsWinner);
            Assert.AreEqual(12.00f, alice.Winnings);
            Assert.AreEqual("Two Pair", alice.HandRank);
        }

        [Test]
        public void ProcessResponse_Deserializes_Correctly()
        {
            var response = JsonConvert.DeserializeObject<ProcessResponse>(ProcessJson);

            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual("processed", response.Result.Status);
            Assert.AreEqual(1, response.Result.TableId);
            Assert.AreEqual(6, response.Result.Step);
            Assert.AreEqual("DEAL_FLOP", response.Result.StepName);
        }

        [Test]
        public void HealthResponse_Deserializes_Correctly()
        {
            var response = JsonConvert.DeserializeObject<HealthResponse>(HealthJson);

            Assert.AreEqual("holdem-processor", response.Service);
            Assert.AreEqual("ok", response.Status);
            Assert.IsNotNull(response.Timestamp);
        }

        [Test]
        public void GameState_CommunityCards_EmptyBeforeFlop()
        {
            var response = JsonConvert.DeserializeObject<TableResponse>(PreflopJson);

            Assert.IsNotNull(response.Game.CommunityCards);
            Assert.AreEqual(0, response.Game.CommunityCards.Count);
        }
    }
}
