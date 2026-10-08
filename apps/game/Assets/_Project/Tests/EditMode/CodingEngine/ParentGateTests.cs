using System;
using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Parents;

namespace Roboya.Tests.CodingEngine
{
    public class ParentGateTests
    {
        private double _clock;

        private ParentGate Gate(int seed = 7) => new ParentGate(new Random(seed), () => _clock);

        private static void Type(ParentGate gate, string digits)
        {
            foreach (char c in digits)
            {
                gate.Press(c - '0');
            }
        }

        [Test]
        public void NewChallenge_FourDigitsWithoutRepeatedNeighbours()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var d = Gate(seed).Digits;

                Assert.AreEqual(ParentGate.DigitCount, d.Count);
                for (int i = 1; i < d.Count; i++)
                {
                    Assert.AreNotEqual(d[i - 1], d[i], "seed " + seed);
                }
            }
        }

        [Test]
        public void Submit_ReversedDigits_Passes()
        {
            var gate = Gate();
            Type(gate, ParentGate.ExpectedFor(gate.Digits));

            Assert.AreEqual(GateResult.Passed, gate.Submit());
        }

        [Test]
        public void Submit_SameOrderAsShown_IsWrong()
        {
            var gate = Gate();
            Type(gate, string.Concat(gate.Digits.Select(d => d.ToString())));

            Assert.AreEqual(GateResult.Wrong, gate.Submit());
            Assert.AreEqual(string.Empty, gate.Entered, "a fresh challenge starts empty");
        }

        [Test]
        public void Submit_TooShort_StaysPending()
        {
            var gate = Gate();
            gate.Press(1);

            Assert.AreEqual(GateResult.Pending, gate.Submit());
            Assert.AreEqual("1", gate.Entered);
        }

        [Test]
        public void Backspace_RemovesLastDigit_AndPressIgnoresBadInput()
        {
            var gate = Gate();
            gate.Press(3);
            gate.Press(4);
            gate.Press(12);
            gate.Press(-1);
            gate.Backspace();

            Assert.AreEqual("3", gate.Entered);
            gate.Backspace();
            gate.Backspace();
            Assert.AreEqual(string.Empty, gate.Entered);
        }

        [Test]
        public void Press_MoreThanFourDigits_IsIgnored()
        {
            var gate = Gate();
            Type(gate, "123456");

            Assert.AreEqual("1234", gate.Entered);
        }

        [Test]
        public void Submit_ThreeWrongAnswers_LocksForThirtySeconds()
        {
            var gate = Gate();
            for (int i = 0; i < 2; i++)
            {
                Type(gate, "0000");
                Assert.AreEqual(GateResult.Wrong, gate.Submit());
            }

            Type(gate, "0000");
            Assert.AreEqual(GateResult.LockedOut, gate.Submit());
            Assert.IsTrue(gate.IsLocked);
            Assert.AreEqual(ParentGate.LockSeconds, gate.SecondsLeft, 1e-9);

            Type(gate, ParentGate.ExpectedFor(gate.Digits));
            Assert.AreEqual(string.Empty, gate.Entered, "typing is ignored while locked");
            Assert.AreEqual(GateResult.LockedOut, gate.Submit());
        }

        [Test]
        public void Lock_ExpiresAfterTheWait_AndFailuresStartOver()
        {
            var gate = Gate();
            for (int i = 0; i < 3; i++)
            {
                Type(gate, "0000");
                gate.Submit();
            }

            _clock += ParentGate.LockSeconds + 0.1;

            Assert.IsFalse(gate.IsLocked);
            Assert.AreEqual(0.0, gate.SecondsLeft);
            Type(gate, "0000");
            Assert.AreEqual(GateResult.Wrong, gate.Submit(), "one miss after the lock is not another lock");
        }

        [Test]
        public void Submit_CorrectAnswerResetsTheFailureCount()
        {
            var gate = Gate();
            Type(gate, "0000");
            gate.Submit();
            Type(gate, "0000");
            gate.Submit();
            Type(gate, ParentGate.ExpectedFor(gate.Digits));
            Assert.AreEqual(GateResult.Passed, gate.Submit());

            Type(gate, "0000");
            Assert.AreEqual(GateResult.Wrong, gate.Submit());
            Type(gate, "0000");
            Assert.AreEqual(GateResult.Wrong, gate.Submit());
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new ParentGate(null, () => 0));
            Assert.Throws<ArgumentNullException>(() => new ParentGate(new Random(1), null));
        }
    }
}
