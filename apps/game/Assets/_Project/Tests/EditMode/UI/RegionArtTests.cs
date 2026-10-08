using NUnit.Framework;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.UI;
using UnityEditor;
using UnityEngine;

namespace Roboya.Tests.UI
{
    public class RegionArtTests
    {
        private RegionArt _art;
        private Sprite _front;
        private Sprite _curious;
        private Sprite _idle;
        private Sprite _thanks;
        private Sprite _apple;

        [SetUp]
        public void SetUp()
        {
            _art = ScriptableObject.CreateInstance<RegionArt>();
            _front = NewSprite();
            _curious = NewSprite();
            _idle = NewSprite();
            _thanks = NewSprite();
            _apple = NewSprite();
            var so = new SerializedObject(_art);
            so.FindProperty("robotFront").objectReferenceValue = _front;
            so.FindProperty("robotCurious").objectReferenceValue = _curious;
            so.FindProperty("goalIdle").objectReferenceValue = _idle;
            so.FindProperty("goalThanks").objectReferenceValue = _thanks;
            so.FindProperty("fruitRed").objectReferenceValue = _apple;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_art);
        }

        [Test]
        public void RobotPoseSprite_AssignedPose_ReturnsIt()
        {
            Assert.AreSame(_curious, _art.RobotPoseSprite(RobotPose.Curious));
        }

        [Test]
        public void RobotPoseSprite_UnassignedPose_FallsBackToFront()
        {
            Assert.AreSame(_front, _art.RobotPoseSprite(RobotPose.Proud));
        }

        [Test]
        public void FriendPoseSprite_UnassignedPose_FallsBackToIdle()
        {
            Assert.AreSame(_thanks, _art.FriendPoseSprite(FriendPose.Thanks));
            Assert.AreSame(_idle, _art.FriendPoseSprite(FriendPose.Explaining));
        }

        [Test]
        public void PropSprite_LogOrMissing_ReturnsNullSoStageDrawsOrSkips()
        {
            Assert.AreSame(_apple, _art.PropSprite(StoryProp.Apple));
            Assert.IsNull(_art.PropSprite(StoryProp.Log));
            Assert.IsNull(_art.PropSprite(StoryProp.Tree));
        }

        private static Sprite NewSprite()
        {
            var tex = new Texture2D(4, 4);
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);
        }
    }
}
