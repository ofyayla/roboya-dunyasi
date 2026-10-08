using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Levels.Generated;

namespace Roboya.CodingEngine.Levels
{
    /// <summary>One story scene ready to stage: poses, props and the narration key.</summary>
    public sealed class StoryBeat
    {
        public StoryBeat(RobotPose robot, FriendPose friend, IReadOnlyList<StoryProp> props, string voiceKey)
        {
            Robot = robot;
            Friend = friend;
            Props = props ?? Array.Empty<StoryProp>();
            VoiceKey = voiceKey;
        }

        public RobotPose Robot { get; }

        public FriendPose Friend { get; }

        public IReadOnlyList<StoryProp> Props { get; }

        /// <summary>Narration for the scene; null when the level has no such line (the scene stays silent).</summary>
        public string VoiceKey { get; }
    }

    /// <summary>
    /// Resolves the level's story scenes (schema v2 <c>story</c>) with defaults, so levels without a story block
    /// still open with Roboya and the region friend. Narration always comes from <c>voice</c>, never duplicated.
    /// </summary>
    public static class LevelStory
    {
        public static StoryBeat Intro(LevelDto level) =>
            Resolve(level?.Story?.Intro, level?.Voice?.Intro, RobotPose.Curious, FriendPose.Explaining);

        public static StoryBeat Outro(LevelDto level) =>
            Resolve(level?.Story?.Outro, level?.Voice?.Success, RobotPose.Happy, FriendPose.Happy);

        private static StoryBeat Resolve(StorySceneDto scene, string voice, RobotPose robot, FriendPose friend) =>
            new StoryBeat(scene?.Roboya ?? robot, scene?.Friend ?? friend, scene?.Props, voice);
    }
}
