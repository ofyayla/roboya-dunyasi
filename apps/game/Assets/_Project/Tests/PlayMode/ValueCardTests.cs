using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.Core;
using Roboya.Games.Common;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Roboya.Tests.PlayMode
{
    /// <summary>The value moment of a corner (plan C4): the film's value picks its lines, the calm card shows the region's friend.</summary>
    public class ValueCardTests
    {
        private sealed class RecordingVoice : IVoicePlayer
        {
            private readonly HashSet<string> _known;

            public RecordingVoice(params string[] known)
            {
                _known = new HashSet<string>(known);
            }

            public List<string> Played { get; } = new List<string>();

            public bool IsPlaying => false;

            public bool Has(string key) => _known.Contains(key);

            public void Play(string key) => Played.Add(key);

            public void Stop()
            {
            }

            public Awaitable PreloadAsync(IEnumerable<string> keys) => Awaitable.NextFrameAsync();
        }

        private static async Awaitable RunAsync(ValueCard card, ValueMoment moment, Sprite friend, CancellationToken token)
        {
            try
            {
                await card.ShowAsync(moment, friend, token);
            }
            catch (OperationCanceledException)
            {
                // The test ends the card by cancelling; the card cleans up on the way out.
            }
        }

        private static IEnumerator WaitFrames(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator ShowAsync_SharingWithoutFilm_SpeaksTheOneLineAndShowsTheRegionFriend()
        {
            var voice = new RecordingVoice("value.sharing");
            var card = new ValueCard(voice);
            var texture = new Texture2D(4, 4);
            var friend = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
            var moment = ValueMoments.Of(ValueId.Sharing, voice.Has);
            using (var cts = new CancellationTokenSource())
            {
                var run = RunAsync(card, moment, friend, cts.Token);
                yield return WaitFrames(5);

                Assert.IsFalse(card.ClassListContains("hidden"), "the card is shown");
                Assert.IsFalse(card.PlayingFilm, "no village film yet: the calm card");
                CollectionAssert.AreEqual(new[] { "value.sharing" }, voice.Played);
                var friendView = card.Q(className: "value-card__friend");
                Assert.IsNotNull(friendView, "the old village bee stands in for the turtle");
                Assert.AreSame(friend, friendView.resolvedStyle.backgroundImage.sprite);

                cts.Cancel();
                yield return WaitFrames(5);
                Assert.IsTrue(run.GetAwaiter().IsCompleted);
            }

            Assert.IsTrue(card.ClassListContains("hidden"), "the card closes itself");
            UnityEngine.Object.Destroy(friend);
            UnityEngine.Object.Destroy(texture);
        }

        [UnityTest]
        public IEnumerator ShowAsync_ValueWithoutAnyLine_StaysSilent()
        {
            var voice = new RecordingVoice();
            var card = new ValueCard(voice);
            var moment = ValueMoments.Of(ValueId.Sharing, voice.Has);
            using (var cts = new CancellationTokenSource())
            {
                var run = RunAsync(card, moment, null, cts.Token);
                yield return WaitFrames(5);
                cts.Cancel();
                yield return WaitFrames(5);
                Assert.IsTrue(run.GetAwaiter().IsCompleted);
            }

            // The line is not in the script yet: the voice player ignores unknown keys, the card just stays calm and short.
            CollectionAssert.AreEqual(new[] { "value.sharing" }, voice.Played);
        }

        [UnityTest]
        public IEnumerator ShowAsync_NoFriendPicture_KeepsTheCodeDrawnTurtle()
        {
            var voice = new RecordingVoice("value.patience");
            var card = new ValueCard(voice);
            var moment = ValueMoments.Of(ValueId.Patience, voice.Has);
            using (var cts = new CancellationTokenSource())
            {
                var run = RunAsync(card, moment, null, cts.Token);
                yield return WaitFrames(3);
                Assert.IsNull(card.Q(className: "value-card__friend"));
                Assert.Greater(card.Q(className: "value-card__turtle").childCount, 0, "the forest's turtle icon stays");
                cts.Cancel();
                yield return WaitFrames(5);
                Assert.IsTrue(run.GetAwaiter().IsCompleted);
            }
        }
    }
}
