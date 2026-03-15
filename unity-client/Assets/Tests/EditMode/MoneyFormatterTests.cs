using NUnit.Framework;
using HijackPoker.Utils;

namespace HijackPoker.Tests
{
    [TestFixture]
    public class MoneyFormatterTests
    {
        [TestCase(150f, "$150.00")]
        [TestCase(0f, "$0.00")]
        [TestCase(1.5f, "$1.50")]
        [TestCase(1000.1f, "$1000.10")]
        [TestCase(0.01f, "$0.01")]
        public void Format_ReturnsCorrectString(float amount, string expected)
        {
            Assert.AreEqual(expected, MoneyFormatter.Format(amount));
        }

        [TestCase(12f, "+$12.00")]
        [TestCase(0f, "+$0.00")]
        [TestCase(-5f, "-$5.00")]
        public void FormatWithSign_ReturnsCorrectString(float amount, string expected)
        {
            Assert.AreEqual(expected, MoneyFormatter.FormatWithSign(amount));
        }
    }
}
