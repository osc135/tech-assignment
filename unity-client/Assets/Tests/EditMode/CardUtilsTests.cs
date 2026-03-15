using NUnit.Framework;
using HijackPoker.Utils;

namespace HijackPoker.Tests
{
    [TestFixture]
    public class CardUtilsTests
    {
        [TestCase("AH", "A", 'H')]
        [TestCase("KD", "K", 'D')]
        [TestCase("QS", "Q", 'S')]
        [TestCase("JC", "J", 'C')]
        [TestCase("10H", "10", 'H')]
        [TestCase("2C", "2", 'C')]
        [TestCase("9D", "9", 'D')]
        public void TryParse_ValidCard_ReturnsRankAndSuit(string input, string expectedRank, char expectedSuit)
        {
            bool result = CardUtils.TryParse(input, out var rank, out var suit);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedRank, rank);
            Assert.AreEqual(expectedSuit, suit);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("A")]
        [TestCase("X")]
        public void TryParse_InvalidCard_ReturnsFalse(string input)
        {
            bool result = CardUtils.TryParse(input, out _, out _);

            Assert.IsFalse(result);
        }

        [TestCase('H', "\u2665")]
        [TestCase('D', "\u2666")]
        [TestCase('C', "\u2663")]
        [TestCase('S', "\u2660")]
        public void GetSuitSymbol_ReturnsCorrectSymbol(char suit, string expected)
        {
            Assert.AreEqual(expected, CardUtils.GetSuitSymbol(suit));
        }

        [TestCase('H', true)]
        [TestCase('D', true)]
        [TestCase('C', false)]
        [TestCase('S', false)]
        public void IsRed_ReturnsCorrectColor(char suit, bool expectedRed)
        {
            Assert.AreEqual(expectedRed, CardUtils.IsRed(suit));
        }

        [TestCase("AH", "A\u2665")]
        [TestCase("10D", "10\u2666")]
        [TestCase("KS", "K\u2660")]
        [TestCase("2C", "2\u2663")]
        public void GetDisplayString_ReturnsFormattedCard(string input, string expected)
        {
            Assert.AreEqual(expected, CardUtils.GetDisplayString(input));
        }

        [Test]
        public void GetDisplayString_InvalidCard_ReturnsQuestionMarks()
        {
            Assert.AreEqual("??", CardUtils.GetDisplayString(null));
            Assert.AreEqual("??", CardUtils.GetDisplayString(""));
            Assert.AreEqual("??", CardUtils.GetDisplayString("X"));
        }

        [TestCase('H', "red")]
        [TestCase('D', "red")]
        [TestCase('C', "black")]
        [TestCase('S', "black")]
        public void GetColorClass_ReturnsCorrectClass(char suit, string expected)
        {
            Assert.AreEqual(expected, CardUtils.GetColorClass(suit));
        }
    }
}
