using System.Linq;
using NUnit.Framework;
using Roboya.Games.Common;

namespace Roboya.Tests.Core
{
    public class DiceSpotsTests
    {
        [Test]
        public void DiceSpots_OneToNine_HasExactlyThatManyDistinctSpotsInsideTheFace()
        {
            for (int n = 1; n <= 9; n++)
            {
                var spots = RemainingView.DiceSpots(n);

                Assert.AreEqual(n, spots.Count, "count " + n);
                Assert.AreEqual(n, spots.Distinct().Count(), "spots are all different for " + n);
                Assert.IsTrue(spots.All(s => s.x >= 0f && s.x <= 1f && s.y >= 0f && s.y <= 1f));
            }
        }

        [Test]
        public void DiceSpots_AboveNine_FallsBackToAGridWithoutOverlap()
        {
            var spots = RemainingView.DiceSpots(12);

            Assert.AreEqual(12, spots.Count);
            Assert.AreEqual(12, spots.Distinct().Count());
        }
    }
}
