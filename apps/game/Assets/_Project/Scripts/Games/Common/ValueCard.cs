using System;
using System.Threading;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.Common
{
    /// <summary>
    /// The patience value moment (F1-24): at the end of each forest corner the wise turtle appears alone on a calm card and a short
    /// line is spoken. Placeholder art, drawn in code (the final animation is external art work); tapping moves on, nothing is scored.
    /// </summary>
    public sealed class ValueCard : VisualElement
    {
        private const float Seconds = 8f;
        private readonly IVoicePlayer _voice;
        private readonly VisualElement _turtle;

        public ValueCard(IVoicePlayer voice)
        {
            _voice = voice;
            name = "value-card";
            AddToClassList("value-card");
            AddToClassList("hidden");
            _turtle = new VisualElement();
            _turtle.AddToClassList("value-card__turtle");
            _turtle.Add(new Icon(IconKind.Turtle) { Color = new Color(0.36f, 0.62f, 0.3f), Accent = Color.white });
            Add(_turtle);
        }

        public async Awaitable ShowAsync(string voiceKey, CancellationToken token)
        {
            RemoveFromClassList("hidden");
            _voice.Play(voiceKey);
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                EventCallback<PointerDownEvent> tap = _ => cts.Cancel();
                RegisterCallback(tap);
                try
                {
                    // Slow breathing: the turtle never hurries.
                    await Tween.Run(Seconds, t =>
                    {
                        float s = 1f + (0.05f * Mathf.Sin(t * Mathf.PI * 2f * (Seconds / 3.2f)));
                        _turtle.style.scale = new Scale(new Vector3(s, s, 1f));
                    }, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    token.ThrowIfCancellationRequested();
                }
                finally
                {
                    UnregisterCallback(tap);
                    AddToClassList("hidden");
                }
            }
        }
    }
}
