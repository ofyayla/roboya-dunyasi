using System;
using System.Collections.Generic;
using System.Text;

namespace Roboya.CodingEngine.Parents
{
    public enum GateResult
    {
        /// <summary>Not submitted yet (still typing).</summary>
        Pending,

        /// <summary>Correct: the adult area opens.</summary>
        Passed,

        /// <summary>Wrong: a new challenge is ready.</summary>
        Wrong,

        /// <summary>Too many wrong answers in a row: wait before trying again.</summary>
        LockedOut,
    }

    /// <summary>
    /// The parental gate (VEL-01, UYM-07): "type these numbers in reverse order". Random digits, entered on a
    /// keypad. Young children cannot do it; adults can in seconds. Purchases, settings, external links and the
    /// whole parent area sit behind it. Pure logic: time and randomness are injected so it is easy to test.
    /// After <see cref="MaxFailures"/> wrong answers in a row the gate locks for <see cref="LockSeconds"/>.
    /// </summary>
    public sealed class ParentGate
    {
        public const int DigitCount = 4;
        public const int MaxFailures = 3;
        public const double LockSeconds = 30.0;

        private readonly Random _random;
        private readonly Func<double> _now;
        private readonly StringBuilder _entered = new StringBuilder();
        private readonly List<int> _digits = new List<int>();
        private int _failures;
        private double _lockedUntil = double.NegativeInfinity;

        public ParentGate(Random random, Func<double> secondsClock)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _now = secondsClock ?? throw new ArgumentNullException(nameof(secondsClock));
            NewChallenge();
        }

        /// <summary>The numbers shown on screen, in the order shown.</summary>
        public IReadOnlyList<int> Digits => _digits;

        /// <summary>What the adult has typed so far.</summary>
        public string Entered => _entered.ToString();

        public bool IsLocked => _now() < _lockedUntil;

        public double SecondsLeft => Math.Max(0.0, _lockedUntil - _now());

        public static string ExpectedFor(IReadOnlyList<int> digits)
        {
            var sb = new StringBuilder();
            for (int i = digits.Count - 1; i >= 0; i--)
            {
                sb.Append(digits[i]);
            }

            return sb.ToString();
        }

        public void NewChallenge()
        {
            _digits.Clear();
            int previous = -1;
            while (_digits.Count < DigitCount)
            {
                // Avoid repeated neighbours so reversing is a real task and a typo is easy to spot.
                int d = _random.Next(0, 10);
                if (d != previous)
                {
                    _digits.Add(d);
                    previous = d;
                }
            }

            _entered.Clear();
        }

        public void Press(int digit)
        {
            if (IsLocked || digit < 0 || digit > 9 || _entered.Length >= DigitCount)
            {
                return;
            }

            _entered.Append(digit);
        }

        public void Backspace()
        {
            if (!IsLocked && _entered.Length > 0)
            {
                _entered.Length--;
            }
        }

        public GateResult Submit()
        {
            if (IsLocked)
            {
                return GateResult.LockedOut;
            }

            if (_entered.Length < DigitCount)
            {
                return GateResult.Pending;
            }

            if (Entered == ExpectedFor(_digits))
            {
                _failures = 0;
                NewChallenge();
                return GateResult.Passed;
            }

            _failures++;
            NewChallenge();
            if (_failures >= MaxFailures)
            {
                _failures = 0;
                _lockedUntil = _now() + LockSeconds;
                return GateResult.LockedOut;
            }

            return GateResult.Wrong;
        }
    }
}
