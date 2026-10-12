using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Levels.Generated;

namespace Roboya.CodingEngine.Levels
{
    /// <summary>One story scene ready to stage: poses, props and the narration key.</summary>
    public sealed class StoryBeat
    {
        public StoryBeat(
            RobotPose robot,
            FriendPose friend,
            IReadOnlyList<StoryProp> props,
            string voiceKey,
            CardType? newCard = null,
            string newCardVoiceKey = null)
        {
            Robot = robot;
            Friend = friend;
            Props = props ?? Array.Empty<StoryProp>();
            VoiceKey = voiceKey;
            NewCard = newCard;
            NewCardVoiceKey = newCardVoiceKey;
        }

        public RobotPose Robot { get; }

        public FriendPose Friend { get; }

        public IReadOnlyList<StoryProp> Props { get; }

        /// <summary>Narration for the scene; null when the level has no such line (the scene stays silent).</summary>
        public string VoiceKey { get; }

        /// <summary>Card introduced by this level (YON-01), shown large in the intro scene; null otherwise.</summary>
        public CardType? NewCard { get; }

        /// <summary>Narration that explains <see cref="NewCard"/>, played after <see cref="VoiceKey"/>.</summary>
        public string NewCardVoiceKey { get; }
    }

    /// <summary>
    /// Resolves the level's story scenes (schema v2 <c>story</c>) with defaults, so levels without a story block
    /// still open with Roboya and the region friend. Narration always comes from <c>voice</c>, never duplicated.
    /// </summary>
    public static class LevelStory
    {
        public static StoryBeat Intro(LevelDto level)
        {
            var beat = Resolve(level?.Story?.Intro, level?.Voice?.Intro, RobotPose.Curious, FriendPose.Explaining);
            var introduces = level?.Cards?.Introduces;
            if (!introduces.HasValue)
            {
                return beat;
            }

            return new StoryBeat(
                beat.Robot,
                beat.Friend,
                beat.Props,
                beat.VoiceKey,
                LevelLoader.ToCard(introduces.Value),
                CardIntroVoice(introduces.Value));
        }

        /// <summary>Narration key that introduces a card, e.g. <c>card.turn_right.intro</c> (content/voice/script.csv).</summary>
        public static string CardIntroVoice(CardId card) => "card." + WireName(card) + ".intro";

        public static StoryBeat Outro(LevelDto level) =>
            Resolve(level?.Story?.Outro, level?.Voice?.Success, RobotPose.Happy, FriendPose.Happy);

        private static string WireName(CardId card)
        {
            switch (card)
            {
                case CardId.Forward: return "forward";
                case CardId.Backward: return "backward";
                case CardId.TurnLeft: return "turn_left";
                case CardId.TurnRight: return "turn_right";
                case CardId.Repeat: return "repeat";
                case CardId.If: return "if";
                case CardId.Call: return "call";
                default: return "action";
            }
        }

        private static StoryBeat Resolve(StorySceneDto scene, string voice, RobotPose robot, FriendPose friend) =>
            new StoryBeat(scene?.Roboya ?? robot, scene?.Friend ?? friend, scene?.Props, voice);
    }
}
