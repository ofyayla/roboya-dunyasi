using System;
using System.Text;
using Roboya.CodingEngine.Parents;
using Roboya.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.UI
{
    /// <summary>
    /// The parental gate screen (VEL-01): the numbers to reverse, a keypad, and a cancel button. Adult text comes
    /// from the string table. Targets are 72 units (≥ 64). A wrong answer shows a short message and new numbers;
    /// too many wrong answers lock the keypad with a countdown.
    /// </summary>
    public sealed class ParentGateView : VisualElement
    {
        private readonly LocalizedStrings _strings;
        private readonly ParentGate _gate;
        private readonly Label _digits = new Label { name = "gate-digits" };
        private readonly Label _entered = new Label { name = "gate-entered" };
        private readonly Label _message = new Label { name = "gate-message" };
        private readonly VisualElement _pad = new VisualElement { name = "gate-pad" };
        private Action _onPassed;
        private bool _lockMessageShown;

        public ParentGateView(LocalizedStrings strings, Func<double> clock = null)
        {
            _strings = strings;
            _gate = new ParentGate(new System.Random(), clock ?? (() => Time.realtimeSinceStartupAsDouble));
            name = "parent-gate";
            AddToClassList("gate");
            style.display = DisplayStyle.None;

            var card = new VisualElement();
            card.AddToClassList("gate__card");
            var question = new Label(strings.Get(StringKeys.GateQuestion));
            question.AddToClassList("gate__question");
            _digits.AddToClassList("gate__digits");
            _entered.AddToClassList("gate__entered");
            _message.AddToClassList("gate__message");
            card.Add(question);
            card.Add(_digits);
            card.Add(_entered);
            card.Add(_message);

            _pad.AddToClassList("gate__pad");
            for (int d = 1; d <= 9; d++)
            {
                _pad.Add(Key(d.ToString(), d));
            }

            _pad.Add(Key("⌫", -1, "gate-back"));
            _pad.Add(Key("0", 0));
            _pad.Add(Key(strings.Get(StringKeys.GateConfirm), -2, "gate-confirm"));
            card.Add(_pad);

            var cancel = new Button(Close) { name = "gate-cancel", text = strings.Get(StringKeys.GateCancel) };
            cancel.AddToClassList("gate__cancel");
            card.Add(cancel);
            Add(card);
            schedule.Execute(Tick).Every(250);
        }

        public bool IsOpen => style.display == DisplayStyle.Flex;

        /// <summary>The numbers currently shown (tests and adults read the same text).</summary>
        public System.Collections.Generic.IReadOnlyList<int> Challenge => _gate.Digits;

        public void Open(Action onPassed)
        {
            _onPassed = onPassed;
            _message.text = string.Empty;
            style.display = DisplayStyle.Flex;
            Render();
        }

        public void Close()
        {
            style.display = DisplayStyle.None;
            _onPassed = null;
        }

        private Button Key(string label, int code, string keyName = null)
        {
            var button = new Button(() => OnKey(code)) { text = label, name = keyName ?? "gate-key-" + label };
            button.AddToClassList("gate__key");
            if (code == -2)
            {
                button.AddToClassList("gate__key--confirm");
            }

            return button;
        }

        private void OnKey(int code)
        {
            if (code >= 0)
            {
                _gate.Press(code);
            }
            else if (code == -1)
            {
                _gate.Backspace();
            }
            else
            {
                Submit();
                return;
            }

            Render();
        }

        private void Submit()
        {
            var result = _gate.Submit();
            switch (result)
            {
                case GateResult.Passed:
                    var done = _onPassed;
                    Close();
                    done?.Invoke();
                    return;
                case GateResult.Wrong:
                    _message.text = _strings.Get(StringKeys.GateWrong);
                    break;
                case GateResult.LockedOut:
                    break;
            }

            Render();
        }

        private void Tick()
        {
            if (IsOpen)
            {
                Render();
            }
        }

        private void Render()
        {
            var sb = new StringBuilder();
            foreach (int d in _gate.Digits)
            {
                sb.Append(d).Append("   ");
            }

            _digits.text = sb.ToString().TrimEnd();
            _entered.text = string.IsNullOrEmpty(_gate.Entered) ? "–" : _gate.Entered;
            bool locked = _gate.IsLocked;
            _pad.SetEnabled(!locked);
            if (locked)
            {
                _message.text = _strings.Format(StringKeys.GateLocked, Mathf.CeilToInt((float)_gate.SecondsLeft));
                _lockMessageShown = true;
            }
            else if (_lockMessageShown)
            {
                _message.text = string.Empty;
                _lockMessageShown = false;
            }
        }
    }
}
