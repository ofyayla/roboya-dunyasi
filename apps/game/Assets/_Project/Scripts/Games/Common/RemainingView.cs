using System.Collections.Generic;
using System.Threading;
using Roboya.CodingEngine.Play;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.Common
{
    /// <summary>
    /// "Kaç kaldı?" (PK-0002, plan D4): after a win Ari asks how much honey is left and the child taps one of three dot cards ("had in
    /// all", "gave", "left"). The dots sit like a die so a small number is known at a glance; the numeral under them is small and only
    /// a second help. A wrong card is never a failure: Ari opens the basket and counts the honey left together with the child. Nothing is
    /// scored, the stars are already kept, and the arrow always lets the child go on.
    /// </summary>
    public sealed class RemainingView : VisualElement
    {
        public const string AskVoice = "bal_pesinde.kac_kaldi.ask";
        public const string RightVoice = "bal_pesinde.kac_kaldi.right";
        public const string WrongVoice = "bal_pesinde.kac_kaldi.wrong";
        private const float CountSeconds = 0.7f;

        private readonly RegionArt _art;
        private readonly IVoicePlayer _voice;
        private readonly VisualElement _basket = new VisualElement { name = "remaining-basket", pickingMode = PickingMode.Ignore };
        private readonly VisualElement _cards = new VisualElement { name = "remaining-cards" };
        private readonly List<(RemainingChoice choice, VisualElement card)> _views = new List<(RemainingChoice, VisualElement)>();
        private RemainingKind? _picked;
        private bool _skipped;

        public RemainingView(RegionArt art, IVoicePlayer voice)
        {
            _art = art;
            _voice = voice;
            name = "remaining-view";
            AddToClassList("remaining");
            style.display = DisplayStyle.None;
            _basket.AddToClassList("remaining__basket");
            var sprite = art != null ? art.HoneyBasket : null;
            if (sprite != null)
            {
                _basket.style.backgroundImage = new StyleBackground(sprite);
            }
            else
            {
                _basket.AddToClassList("remaining__basket--fallback");
            }

            _cards.AddToClassList("remaining__cards");
            Add(_basket);
            Add(_cards);
            var next = new IconButton(IconKind.Next, () => _skipped = true) { name = "remaining-skip" };
            next.AddToClassList("remaining__skip");
            Add(next);
        }

        public bool IsOpen => style.display == DisplayStyle.Flex;

        /// <summary>Asks, waits for a card (or the arrow), answers gently and returns; never blocks longer than the child takes.</summary>
        public async Awaitable ShowAsync(IReadOnlyList<RemainingChoice> choices, CancellationToken token)
        {
            _picked = null;
            _skipped = false;
            _views.Clear();
            _cards.Clear();
            foreach (var choice in choices)
            {
                var card = Card(choice);
                _cards.Add(card);
                _views.Add((choice, card));
            }

            style.display = DisplayStyle.Flex;
            _voice.Play(AskVoice);
            try
            {
                while (!_picked.HasValue && !_skipped)
                {
                    await Awaitable.NextFrameAsync(token);
                }

                if (_picked.HasValue)
                {
                    await AnswerAsync(_picked.Value, token);
                }
            }
            finally
            {
                style.display = DisplayStyle.None;
            }
        }

        private async Awaitable AnswerAsync(RemainingKind picked, CancellationToken token)
        {
            var right = _views.Find(v => v.choice.IsCorrect);
            if (picked == RemainingKind.Left)
            {
                right.card.AddToClassList("remaining__card--right");
                _voice.Play(RightVoice);
                await Tween.Delay(1.4f, token);
                return;
            }

            // Not it: no red, no buzzer. Ari counts what is left, one dot after the other, with the child.
            _views.Find(v => v.choice.Kind == picked).card.AddToClassList("remaining__card--tried");
            _voice.Play(WrongVoice);
            await Tween.Delay(1.2f, token);
            int n = 0;
            foreach (var dot in right.card.Query(className: "remaining__dot").ToList())
            {
                n++;
                dot.AddToClassList("remaining__dot--counted");
                if (n <= 10)
                {
                    _voice.Play("count." + n.ToString("D2"));
                }

                await Tween.Delay(CountSeconds, token);
            }

            right.card.AddToClassList("remaining__card--right");
            await Tween.Delay(0.8f, token);
        }

        private VisualElement Card(RemainingChoice choice)
        {
            var card = new VisualElement { name = "remaining-" + choice.Kind.ToString().ToLowerInvariant() };
            card.AddToClassList("remaining__card");
            card.AddToClassList("remaining__card--" + choice.Kind.ToString().ToLowerInvariant());
            var face = new VisualElement { pickingMode = PickingMode.Ignore };
            face.AddToClassList("remaining__face");
            foreach (var spot in DiceSpots(choice.Count))
            {
                var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                dot.AddToClassList("remaining__dot");
                dot.style.left = new Length(spot.x * 100f, LengthUnit.Percent);
                dot.style.top = new Length(spot.y * 100f, LengthUnit.Percent);
                face.Add(dot);
            }

            card.Add(face);
            var numeral = new Label(choice.Count.ToString()) { pickingMode = PickingMode.Ignore };
            numeral.AddToClassList("remaining__numeral");
            card.Add(numeral);
            card.RegisterCallback<ClickEvent>(_ => _picked ??= choice.Kind);
            return card;
        }

        /// <summary>Where the dots of a card go, as fractions of its face: the pips of a die up to six, then a grid.</summary>
        public static IReadOnlyList<Vector2> DiceSpots(int count)
        {
            const float a = 0.2f;
            const float c = 0.5f;
            const float b = 0.8f;
            switch (count)
            {
                case 1: return new[] { new Vector2(c, c) };
                case 2: return new[] { new Vector2(a, a), new Vector2(b, b) };
                case 3: return new[] { new Vector2(a, a), new Vector2(c, c), new Vector2(b, b) };
                case 4: return new[] { new Vector2(a, a), new Vector2(b, a), new Vector2(a, b), new Vector2(b, b) };
                case 5: return new[] { new Vector2(a, a), new Vector2(b, a), new Vector2(c, c), new Vector2(a, b), new Vector2(b, b) };
                case 6: return new[] { new Vector2(a, a), new Vector2(b, a), new Vector2(a, c), new Vector2(b, c), new Vector2(a, b), new Vector2(b, b) };
                case 7: return new[] { new Vector2(a, a), new Vector2(b, a), new Vector2(a, c), new Vector2(c, c), new Vector2(b, c), new Vector2(a, b), new Vector2(b, b) };
                default:
                    var spots = new List<Vector2>();
                    int columns = count <= 9 ? 3 : 4;
                    for (int i = 0; i < count; i++)
                    {
                        float x = columns == 1 ? c : a + ((b - a) * (i % columns) / (columns - 1));
                        int rows = (count + columns - 1) / columns;
                        float y = rows == 1 ? c : a + ((b - a) * (i / columns) / (rows - 1));
                        spots.Add(new Vector2(x, y));
                    }

                    return spots;
            }
        }
    }
}
