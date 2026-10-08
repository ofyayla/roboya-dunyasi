using System.Collections.Generic;
using System.Threading;
using Roboya.CodingEngine.Levels;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.UI
{
    /// <summary>
    /// Wide story scene before and after a level (PRD principle 2): the region background, Roboya and the region
    /// friend at full size, a few props and foreground bushes, all staged in code from <see cref="RegionArt"/>.
    /// No text on screen; the narration line carries the story. Animations run on one frame loop that only
    /// writes style values (no per-frame allocation).
    /// </summary>
    public sealed class StoryStage : VisualElement
    {
        public const float EnterSeconds = 0.6f;
        public const float ExitSeconds = 0.45f;

        /// <summary>Pause after the narration ends before the intro moves on by itself.</summary>
        public const float AutoContinueDelay = 1.2f;

        /// <summary>Safety net when a line never reports its end (e.g. audio disabled).</summary>
        public const float MaxWaitSeconds = 30f;

        private const int MaxProps = 3;
        private const float FootY = 0.86f;

        private sealed class Actor
        {
            public VisualElement View;
            public VisualElement Shadow;
            public float X;
            public float Height;
            public float Aspect = 1f;
            public float Enter;
            public float Hop;
            public float Squash;
            public float Tilt;
        }

        private readonly RegionArt _art;
        private readonly IVoicePlayer _voice;
        private readonly VisualElement _backdrop = new VisualElement { name = "story-backdrop" };
        private readonly VisualElement _ground = new VisualElement();
        private readonly VisualElement _cast = new VisualElement();
        private readonly Actor _robot;
        private readonly Actor _friend;
        private readonly Actor[] _props = new Actor[MaxProps];
        private readonly LogShape[] _logs = new LogShape[MaxProps];
        private readonly List<Actor> _front = new List<Actor>();
        private readonly IconButton _continue;
        private readonly VisualElement _cardHost = new VisualElement { name = "story-card" };

        private int _session;
        private float _time;
        private float _enterT;
        private bool _celebrating;
        private bool _continueRequested;
        private int _propCount;
        private CardElement _card;
        private VisualElement _reward;
        private string _cardVoice;
        private float _cardAge = -1f;

        public StoryStage(RegionArt art, IVoicePlayer voice)
        {
            _art = art;
            _voice = voice;
            name = "story";
            AddToClassList("story");
            _backdrop.AddToClassList("story__backdrop");
            _backdrop.pickingMode = PickingMode.Ignore;
            _ground.AddToClassList("story__layer");
            _ground.pickingMode = PickingMode.Ignore;
            _cast.AddToClassList("story__layer");
            _cast.pickingMode = PickingMode.Ignore;
            Add(_backdrop);
            Add(_ground);
            Add(_cast);

            if (art != null && art.Background != null)
            {
                _backdrop.style.backgroundImage = new StyleBackground(art.Background);
            }

            for (int i = 0; i < MaxProps; i++)
            {
                _props[i] = NewActor("story-prop-" + i);
                _logs[i] = new LogShape();
                _props[i].View.Add(_logs[i]);
            }

            _robot = NewActor("story-robot");
            _friend = NewActor("story-friend");
            _robot.X = 0.3f;
            _robot.Height = 0.52f;
            _friend.X = 0.7f;
            _friend.Height = 0.44f;

            // Foreground bushes frame the scene and give it depth, as in the concept art.
            var bush = art != null ? art.PropSprite(StoryProp.Bush) : null;
            if (bush != null)
            {
                foreach (float x in new[] { 0.02f, 0.98f })
                {
                    var front = NewActor(null);
                    SetSprite(front, bush);
                    front.X = x;
                    front.Height = 0.36f;
                    front.Shadow.style.display = DisplayStyle.None;
                    _front.Add(front);
                }
            }

            // YON-01: a level's new card pops up large between the friends; tapping it repeats its narration.
            _cardHost.AddToClassList("story__card");
            _cardHost.RegisterCallback<ClickEvent>(_ => _voice.Play(_cardVoice));
            Add(_cardHost);

            _continue = new IconButton(IconKind.Next, () => _continueRequested = true) { name = "story-continue" };
            _continue.AddToClassList("story__continue");
            Add(_continue);

            style.display = DisplayStyle.None;
        }

        public bool IsOpen => style.display == DisplayStyle.Flex;

        /// <summary>
        /// Intro: plays the scene and closes it when the child taps continue, or shortly after the narration ends.
        /// </summary>
        public async Awaitable PlayIntroAsync(StoryBeat beat, CancellationToken token)
        {
            int session = Open(beat, celebrate: false, showContinue: true, token);
            await Tween.Delay(EnterSeconds, token);
            _voice.Play(beat.VoiceKey);

            if (beat.NewCard.HasValue)
            {
                await WaitForQuietAsync(session, 0.3f, token);
                if (session == _session && !_continueRequested)
                {
                    _cardAge = 0f;
                    _cardHost.style.display = DisplayStyle.Flex;
                    _voice.Play(beat.NewCardVoiceKey);
                }
            }

            await WaitForQuietAsync(session, AutoContinueDelay, token);
            if (session == _session)
            {
                await CloseAsync(token);
            }
        }

        /// <summary>Waits until the child taps continue or the narration has been quiet for <paramref name="quietSeconds"/>.</summary>
        private async Awaitable WaitForQuietAsync(int session, float quietSeconds, CancellationToken token)
        {
            float quiet = 0f;
            float waited = 0f;
            while (session == _session && !_continueRequested && quiet < quietSeconds && waited < MaxWaitSeconds)
            {
                await Awaitable.NextFrameAsync(token);
                waited += Time.deltaTime;
                quiet = _voice.IsPlaying ? 0f : quiet + Time.deltaTime;
            }
        }

        /// <summary>
        /// Outro: fades in over the board, plays the success line and keeps celebrating until <see cref="Hide"/>;
        /// returns once the characters are in place so the result panel can appear on top.
        /// </summary>
        /// <param name="reward">A robot part earned with this level (ILR-03), shown after the success line.</param>
        public async Awaitable PlayOutroAsync(StoryBeat beat, CancellationToken token, Sprite reward = null, string rewardVoice = null)
        {
            int session = Open(beat, celebrate: true, showContinue: false, token);
            style.opacity = 0f;
            await Tween.Run(0.35f, t => style.opacity = t, token);
            _voice.Play(beat.VoiceKey);
            if (reward != null)
            {
                _ = RevealRewardAsync(session, reward, rewardVoice, token);
            }

            await Tween.Delay(EnterSeconds * 0.6f, token);
        }

        private async Awaitable RevealRewardAsync(int session, Sprite reward, string voiceKey, CancellationToken token)
        {
            try
            {
                await WaitForQuietAsync(session, 0.3f, token);
                if (session != _session)
                {
                    return;
                }

                _reward = new VisualElement { name = "story-reward", pickingMode = PickingMode.Ignore };
                _reward.style.backgroundImage = new StyleBackground(reward);
                _reward.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                _cardHost.Add(_reward);
                _cardVoice = voiceKey;
                _cardAge = 0f;
                _cardHost.style.display = DisplayStyle.Flex;
                _voice.Play(voiceKey);
            }
            catch (System.OperationCanceledException)
            {
                // Screen closed before the reveal.
            }
        }

        public void Hide()
        {
            _session++;
            style.display = DisplayStyle.None;
        }

        private int Open(StoryBeat beat, bool celebrate, bool showContinue, CancellationToken token)
        {
            int session = ++_session;
            _time = 0f;
            _enterT = 0f;
            _celebrating = celebrate;
            _continueRequested = false;
            style.display = DisplayStyle.Flex;
            style.opacity = 1f;
            style.scale = new Scale(Vector3.one);
            _continue.style.display = showContinue ? DisplayStyle.Flex : DisplayStyle.None;

            if (_art != null)
            {
                SetSprite(_robot, _art.RobotPoseSprite(beat.Robot));
                SetSprite(_friend, _art.FriendPoseSprite(beat.Friend));
            }

            StageProps(beat.Props);
            StageCard(beat);
            _ = RunAsync(session, token);
            return session;
        }

        private void StageCard(StoryBeat beat)
        {
            _cardAge = -1f;
            _cardHost.style.display = DisplayStyle.None;
            _cardVoice = beat.NewCardVoiceKey;
            if (_card != null)
            {
                _card.RemoveFromHierarchy();
                _card = null;
            }

            if (_reward != null)
            {
                _reward.RemoveFromHierarchy();
                _reward = null;
            }

            if (beat.NewCard.HasValue)
            {
                _card = new CardElement(beat.NewCard.Value) { pickingMode = PickingMode.Ignore };
                _card.Icon.pickingMode = PickingMode.Ignore;
                _card.AddToClassList("story__card-face");
                _cardHost.Add(_card);
            }
        }

        private async Awaitable CloseAsync(CancellationToken token)
        {
            int session = _session;
            await Tween.Run(ExitSeconds, t =>
            {
                style.opacity = 1f - t;
                style.scale = new Scale(Vector3.one * (1f - (0.06f * t)));
            }, token);
            if (session == _session)
            {
                Hide();
            }
        }

        private void StageProps(IReadOnlyList<StoryProp> props)
        {
            _propCount = 0;
            for (int i = 0; i < MaxProps; i++)
            {
                var actor = _props[i];
                bool used = props != null && i < props.Count;
                actor.View.style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                actor.Shadow.style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (!used)
                {
                    continue;
                }

                _propCount++;
                var prop = props[i];
                var sprite = _art != null ? _art.PropSprite(prop) : null;
                bool drawLog = prop == StoryProp.Log || sprite == null;
                _logs[i].style.display = drawLog ? DisplayStyle.Flex : DisplayStyle.None;
                actor.View.style.backgroundImage = drawLog ? new StyleBackground(StyleKeyword.None) : new StyleBackground(sprite);
                actor.Aspect = drawLog ? LogShape.Aspect : Aspect(sprite);
                actor.Height = PropHeight(prop);
            }

            // Props stand between the two friends, spread evenly around the middle.
            float spacing = 0.11f;
            float start = 0.5f - (spacing * (_propCount - 1) * 0.5f);
            for (int i = 0; i < _propCount; i++)
            {
                _props[i].X = start + (spacing * i);
            }
        }

        private async Awaitable RunAsync(int session, CancellationToken token)
        {
            try
            {
                while (session == _session)
                {
                    float dt = Time.deltaTime;
                    _time += dt;
                    _enterT = Mathf.Min(1f, _enterT + (dt / EnterSeconds));
                    if (_cardAge >= 0f)
                    {
                        _cardAge += dt;
                    }

                    Animate();
                    Layout();
                    await Awaitable.NextFrameAsync(token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Scene torn down with the screen.
            }
        }

        private void Animate()
        {
            float t = _time;
            float e = Tween.EaseInOut(_enterT);
            bool talking = !_celebrating && _voice.IsPlaying;

            // Slow push-in on the backdrop: a gentle camera move.
            float push = Mathf.Min(1f, t / 14f);
            _backdrop.style.scale = new Scale(Vector3.one * (1.04f + (0.05f * push)));
            _backdrop.style.translate = new Translate(Length.Percent(-1.2f * push), Length.Percent(-0.6f * push));

            // Friends hop in from the sides, then breathe; Roboya bounces while the line plays.
            float entryHop = _enterT < 1f ? Mathf.Abs(Mathf.Sin(_enterT * Mathf.PI * 2f)) * 0.04f : 0f;
            _robot.Enter = (1f - e) * -0.4f;
            _friend.Enter = (1f - e) * 0.4f;
            _robot.Hop = entryHop + (talking ? Mathf.Abs(Mathf.Sin(t * 9f)) * 0.018f : 0f);
            _friend.Hop = entryHop;
            _robot.Squash = Mathf.Sin(t * 2.4f) * 0.015f;
            _friend.Squash = Mathf.Sin((t * 2.1f) + 1.3f) * 0.015f;
            _friend.Tilt = Mathf.Sin(t * 1.7f) * 2f;
            _robot.Tilt = 0f;

            if (_celebrating && _enterT >= 1f)
            {
                _robot.Hop = Mathf.Abs(Mathf.Sin(t * 5f)) * 0.05f;
                _friend.Hop = Mathf.Abs(Mathf.Sin((t * 5f) + 1.2f)) * 0.035f;
            }

            for (int i = 0; i < _propCount; i++)
            {
                _props[i].Hop = Mathf.Abs(Mathf.Sin((t * 2f) + i)) * 0.006f;
                _props[i].Enter = 0f;
            }

            if (_cardAge >= 0f)
            {
                float pop = EaseOutBack(Mathf.Min(1f, _cardAge / 0.45f));
                _cardHost.style.scale = new Scale(new Vector3(pop, pop, 1f));
                _cardHost.style.rotate = new Rotate(new Angle(Mathf.Sin(t * 1.6f) * 3f, AngleUnit.Degree));
            }

            if (_continue.style.display == DisplayStyle.Flex)
            {
                float pulse = 1f + (Mathf.Max(0f, Mathf.Sin(t * 4f)) * 0.08f);
                _continue.style.scale = new Scale(new Vector3(pulse, pulse, 1f));
            }
        }

        private void Layout()
        {
            float w = resolvedStyle.width;
            float h = resolvedStyle.height;
            if (float.IsNaN(w) || float.IsNaN(h) || w <= 0f || h <= 0f)
            {
                return;
            }

            for (int i = 0; i < _propCount; i++)
            {
                Place(_props[i], w, h, FootY + 0.01f);
            }

            if (_card != null || _reward != null)
            {
                // The reward sits lower and smaller so the stars above stay clear.
                float size = h * (_reward != null ? 0.34f : 0.42f);
                float face = h * 0.26f;
                _cardHost.style.width = size;
                _cardHost.style.height = size;
                _cardHost.style.borderTopLeftRadius = size * 0.5f;
                _cardHost.style.borderTopRightRadius = size * 0.5f;
                _cardHost.style.borderBottomLeftRadius = size * 0.5f;
                _cardHost.style.borderBottomRightRadius = size * 0.5f;
                float bob = Mathf.Sin(_time * 2.2f) * h * 0.012f;
                float centerY = h * (_reward != null ? 0.52f : 0.36f);
                _cardHost.style.translate = new Translate((w * 0.5f) - (size * 0.5f), centerY - (size * 0.5f) + bob);
                if (_card != null)
                {
                    _card.style.width = face;
                    _card.style.height = face;
                    SetPadding(_card, face * 0.18f);
                    SetRadius(_card, face * 0.2f);
                    _card.style.borderBottomWidth = face * 0.07f;
                }

                if (_reward != null)
                {
                    _reward.style.width = face * 1.15f;
                    _reward.style.height = face * 1.15f;
                }
            }

            Place(_friend, w, h, FootY - 0.02f);
            Place(_robot, w, h, FootY);
            foreach (var front in _front)
            {
                Place(front, w, h, 1.08f);
            }
        }

        private static void Place(Actor a, float w, float h, float footY)
        {
            float ah = a.Height * h;
            float aw = ah * a.Aspect;
            float x = (a.X + a.Enter) * w;
            float y = footY * h;
            a.View.style.width = aw;
            a.View.style.height = ah;
            a.View.style.translate = new Translate(x - (aw * 0.5f), y - (ah * 0.965f) - (a.Hop * h));
            a.View.style.scale = new Scale(new Vector3(1f - a.Squash, 1f + a.Squash, 1f));
            a.View.style.rotate = new Rotate(new Angle(a.Tilt, AngleUnit.Degree));

            float sw = Mathf.Min(aw * 0.8f, h * 0.3f) * (1f - (a.Hop * 4f));
            a.Shadow.style.width = sw;
            a.Shadow.style.height = sw * 0.22f;
            a.Shadow.style.translate = new Translate(x - (sw * 0.5f), y - (sw * 0.11f));
        }

        private Actor NewActor(string actorName)
        {
            var view = new VisualElement { name = actorName, pickingMode = PickingMode.Ignore };
            view.AddToClassList("story__actor");
            var shadow = new VisualElement { pickingMode = PickingMode.Ignore };
            shadow.AddToClassList("story__shadow");
            _ground.Add(shadow);
            _cast.Add(view);
            return new Actor { View = view, Shadow = shadow };
        }

        private static void SetSprite(Actor a, Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            a.View.style.backgroundImage = new StyleBackground(sprite);
            a.Aspect = Aspect(sprite);
        }

        private static void SetPadding(VisualElement e, float v)
        {
            e.style.paddingLeft = v;
            e.style.paddingRight = v;
            e.style.paddingTop = v;
            e.style.paddingBottom = v;
        }

        private static void SetRadius(VisualElement e, float v)
        {
            e.style.borderTopLeftRadius = v;
            e.style.borderTopRightRadius = v;
            e.style.borderBottomLeftRadius = v;
            e.style.borderBottomRightRadius = v;
        }

        private static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + (c3 * Mathf.Pow(x - 1f, 3f)) + (c1 * Mathf.Pow(x - 1f, 2f));
        }

        private static float Aspect(Sprite s) => s != null && s.rect.height > 0f ? s.rect.width / s.rect.height : 1f;

        private static float PropHeight(StoryProp prop)
        {
            switch (prop)
            {
                case StoryProp.Tree: return 0.4f;
                case StoryProp.Bush: return 0.2f;
                case StoryProp.Rock: return 0.16f;
                case StoryProp.Log: return 0.1f;
                case StoryProp.Gear: return 0.14f;
                default: return 0.12f;
            }
        }
    }
}
