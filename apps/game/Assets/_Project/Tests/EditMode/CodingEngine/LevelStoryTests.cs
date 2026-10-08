using System.Collections.Generic;
using NUnit.Framework;
using Roboya.CodingEngine.Levels;
using Roboya.CodingEngine.Levels.Generated;

namespace Roboya.Tests.CodingEngine
{
    public class LevelStoryTests
    {
        private static LevelDto Level(StoryDto story) => new LevelDto
        {
            Voice = new VoiceDto { Intro = "l.intro", Success = "l.success" },
            Story = story,
        };

        [Test]
        public void Intro_NoStoryBlock_UsesDefaultsAndIntroVoice()
        {
            var beat = LevelStory.Intro(Level(null));

            Assert.AreEqual(RobotPose.Curious, beat.Robot);
            Assert.AreEqual(FriendPose.Explaining, beat.Friend);
            Assert.AreEqual(0, beat.Props.Count);
            Assert.AreEqual("l.intro", beat.VoiceKey);
        }

        [Test]
        public void Outro_NoStoryBlock_BothHappyWithSuccessVoice()
        {
            var beat = LevelStory.Outro(Level(new StoryDto()));

            Assert.AreEqual(RobotPose.Happy, beat.Robot);
            Assert.AreEqual(FriendPose.Happy, beat.Friend);
            Assert.AreEqual("l.success", beat.VoiceKey);
        }

        [Test]
        public void Intro_PartialScene_FillsMissingPoseOnly()
        {
            var story = new StoryDto { Intro = new StorySceneDto { Roboya = RobotPose.Surprised, Props = new List<StoryProp> { StoryProp.Log } } };

            var beat = LevelStory.Intro(Level(story));

            Assert.AreEqual(RobotPose.Surprised, beat.Robot);
            Assert.AreEqual(FriendPose.Explaining, beat.Friend);
            CollectionAssert.AreEqual(new[] { StoryProp.Log }, beat.Props);
        }

        [Test]
        public void Outro_FullScene_UsesGivenPoses()
        {
            var story = new StoryDto { Outro = new StorySceneDto { Roboya = RobotPose.Proud, Friend = FriendPose.Thanks } };

            var beat = LevelStory.Outro(Level(story));

            Assert.AreEqual(RobotPose.Proud, beat.Robot);
            Assert.AreEqual(FriendPose.Thanks, beat.Friend);
        }

        [Test]
        public void Outro_NoSuccessVoice_VoiceKeyIsNull()
        {
            var level = new LevelDto { Voice = new VoiceDto { Intro = "x" } };

            Assert.IsNull(LevelStory.Outro(level).VoiceKey);
            Assert.IsNull(LevelStory.Intro(null).VoiceKey);
        }
    }
}
