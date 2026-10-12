using System;
using NUnit.Framework;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.Play;
using static Roboya.Tests.CodingEngine.TestLevels;

namespace Roboya.Tests.CodingEngine
{
    public class PlanStripTests
    {
        private static PlanStrip Strip(int capacity = 3) => new PlanStrip(capacity, Moves);

        [Test]
        public void Append_UntilFull_RejectsOverflow()
        {
            var plan = Strip(2);

            Assert.IsTrue(plan.Append(F));
            Assert.IsTrue(plan.Append(L));
            Assert.IsTrue(plan.IsFull);
            Assert.IsFalse(plan.Append(F));
            Assert.AreEqual(2, plan.Count);
        }

        [Test]
        public void Insert_DisallowedCard_Rejected()
        {
            var plan = Strip();

            Assert.IsFalse(plan.Insert(0, CardType.Backward));
            Assert.IsTrue(plan.IsEmpty);
        }

        [Test]
        public void Insert_IndexOutOfRange_IsClamped()
        {
            var plan = Strip();
            plan.Append(F);

            plan.Insert(-5, L);
            plan.Insert(99, Rt);

            CollectionAssert.AreEqual(new[] { L, F, Rt }, plan.Cards);
        }

        [Test]
        public void RemoveAt_ValidAndInvalid_BehavesSafely()
        {
            var plan = Strip();
            plan.Append(F);

            Assert.IsFalse(plan.RemoveAt(3));
            Assert.IsTrue(plan.RemoveAt(0));
            Assert.IsTrue(plan.IsEmpty);
        }

        [Test]
        public void Move_ReordersCards()
        {
            var plan = Strip();
            plan.Load(new[] { F, L, Rt });

            Assert.IsTrue(plan.Move(0, 2));
            Assert.IsFalse(plan.Move(5, 0));

            CollectionAssert.AreEqual(new[] { L, Rt, F }, plan.Cards);
        }

        [Test]
        public void Replace_ChangesCardOrRejects()
        {
            var plan = Strip();
            plan.Append(F);

            Assert.IsTrue(plan.Replace(0, L));
            Assert.IsFalse(plan.Replace(1, L));
            Assert.IsFalse(plan.Replace(0, CardType.Repeat));
            CollectionAssert.AreEqual(new[] { L }, plan.Cards);
        }

        [Test]
        public void Load_DropsDisallowedAndOverflow()
        {
            var plan = Strip(2);

            plan.Load(new[] { F, CardType.Backward, L, Rt });

            CollectionAssert.AreEqual(new[] { F, L }, plan.Cards);
        }

        [Test]
        public void Version_IncrementsOnlyOnChange()
        {
            var plan = Strip();
            int v0 = plan.Version;

            plan.Clear();
            Assert.AreEqual(v0, plan.Version);
            plan.Append(F);
            plan.Clear();

            Assert.AreEqual(v0 + 2, plan.Version);
        }

        [Test]
        public void ToProgram_RunsLikeCards()
        {
            var level = Build(new[] { "G.", "R." });
            var plan = new PlanStrip(3, Moves);
            plan.Append(F);

            Assert.AreEqual(ExecutionOutcome.Success, Interpreter.Execute(level, plan.ToProgram()).Outcome);
        }

        [Test]
        public void Constructor_InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlanStrip(0, Moves));
            Assert.Throws<ArgumentNullException>(() => new PlanStrip(2, null));
        }
    }
}
