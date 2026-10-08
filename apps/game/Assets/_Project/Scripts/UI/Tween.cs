using System;
using System.Threading;
using UnityEngine;

namespace Roboya.UI
{
    /// <summary>Minimal frame-based tweening with Unity 6 Awaitable (no coroutines, no per-frame allocation).</summary>
    public static class Tween
    {
        public static async Awaitable Run(float duration, Action<float> apply, CancellationToken token = default)
        {
            float t = 0f;
            while (t < duration)
            {
                token.ThrowIfCancellationRequested();
                apply(EaseInOut(t / duration));
                await Awaitable.NextFrameAsync(token);
                t += Time.deltaTime;
            }

            apply(1f);
        }

        public static Awaitable Delay(float seconds, CancellationToken token = default) =>
            Awaitable.WaitForSecondsAsync(seconds, token);

        public static float EaseInOut(float x) => x < 0.5f ? 2f * x * x : 1f - Mathf.Pow(-2f * x + 2f, 2f) / 2f;

        /// <summary>Damped horizontal shake used for "bump" and the wrong card (OYN-03).</summary>
        public static float Shake(float x, float amplitude) => Mathf.Sin(x * Mathf.PI * 6f) * amplitude * (1f - x);
    }
}
