using NUnit.Framework;
using Roboya.UI;
using UnityEngine;

namespace Roboya.Tests.UI
{
    public class ObliqueProjectionTests
    {
        private static readonly Vector2 Area = new Vector2(1200f, 560f);

        [Test]
        public void ScaleAt_BackAndFrontEdges_MatchTopScaleAndOne()
        {
            var p = new ObliqueProjection(5, 5, Area, topScale: 0.8f);

            Assert.AreEqual(0.8f, p.ScaleAt(0f), 1e-4f);
            Assert.AreEqual(1f, p.ScaleAt(5f), 1e-4f);
        }

        [Test]
        public void Project_ColumnLine_StaysStraight()
        {
            var p = new ObliqueProjection(6, 4, Area);
            var a = p.Project(2f, 0f);
            var b = p.Project(2f, 4f);

            for (float gy = 0.5f; gy < 4f; gy += 0.5f)
            {
                var m = p.Project(2f, gy);
                float cross = ((b.x - a.x) * (m.y - a.y)) - ((b.y - a.y) * (m.x - a.x));
                Assert.AreEqual(0f, cross, 0.5f, "point at gy=" + gy + " is off the column line");
            }
        }

        [Test]
        public void Project_RowsFartherAway_AreShorterOnScreen()
        {
            var p = new ObliqueProjection(5, 5, Area);

            float back = p.Project(0f, 1f).y - p.Project(0f, 0f).y;
            float front = p.Project(0f, 5f).y - p.Project(0f, 4f).y;

            Assert.Less(back, front);
            Assert.Greater(back, 0f);
        }

        [Test]
        public void Project_NorthOnGrid_IsUpOnScreen()
        {
            var p = new ObliqueProjection(5, 5, Area);

            Assert.Less(p.CellCenter(2, 1).y, p.CellCenter(2, 2).y);
            Assert.AreEqual(p.CellCenter(2, 1).x, p.CellCenter(2, 2).x, 1e-3f, "the middle column must not drift sideways");
        }

        [Test]
        public void Constructor_AnyGrid_FitsInsideArea()
        {
            foreach (var size in new[] { new Vector2Int(3, 3), new Vector2Int(5, 5), new Vector2Int(8, 4), new Vector2Int(4, 8) })
            {
                var p = new ObliqueProjection(size.x, size.y, Area);
                var frontLeft = p.Project(0f, size.y);
                var frontRight = p.Project(size.x, size.y);
                var backLeft = p.Project(0f, 0f);

                Assert.GreaterOrEqual(frontLeft.x, 0f, size.ToString());
                Assert.LessOrEqual(frontRight.x, Area.x, size.ToString());
                Assert.GreaterOrEqual(backLeft.y - (p.Cell * p.TopScale * 0.75f), -0.5f, size + " headroom");
                Assert.LessOrEqual(frontLeft.y + p.SlabHeight, Area.y + 0.5f, size + " slab");
            }
        }

        [Test]
        public void Constructor_ZeroArea_IsNotValid()
        {
            var p = new ObliqueProjection(5, 5, Vector2.zero);

            Assert.IsFalse(p.IsValid);
        }
    }
}
