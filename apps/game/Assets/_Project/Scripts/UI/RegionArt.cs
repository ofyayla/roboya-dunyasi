using Roboya.CodingEngine.Levels.Generated;
using UnityEngine;

namespace Roboya.UI
{
    /// <summary>
    /// Sprites for one region and its characters. One asset per region (e.g. SabirOrmaniArt.asset), wired by the
    /// editor SceneBuilder; views fall back to code-drawn icons when a slot is empty.
    /// </summary>
    [CreateAssetMenu(menuName = "Roboya/Region Art")]
    public sealed class RegionArt : ScriptableObject
    {
        [Header("Roboya")]
        [SerializeField] private Sprite robotFront;
        [SerializeField] private Sprite robotBack;
        [SerializeField] private Sprite robotSide;
        [SerializeField] private Sprite robotHappy;
        [SerializeField] private Sprite robotLaughing;
        [SerializeField] private Sprite robotCurious;
        [SerializeField] private Sprite robotSurprised;
        [SerializeField] private Sprite robotProud;

        [Header("Ari the bee (Bal Peşinde)")]
        [SerializeField] private Sprite ariFront;
        [SerializeField] private Sprite ariBack;
        [SerializeField] private Sprite ariSide;
        [SerializeField] private Sprite ariHappy;

        [Header("Goal character")]
        [SerializeField] private Sprite goalIdle;
        [SerializeField] private Sprite goalHappy;
        [SerializeField] private Sprite goalExplaining;
        [SerializeField] private Sprite goalThanks;

        [Header("Board")]
        [SerializeField] private Sprite tileFloor;
        [SerializeField] private Sprite tilePath;
        [SerializeField] private Sprite[] obstacles;
        [SerializeField] private Sprite background;

        [Tooltip("Props placed beside the board (front-left, right, back-left).")]
        [SerializeField] private Sprite[] decor;

        [Header("Items")]
        [SerializeField] private Sprite fruitRed;
        [SerializeField] private Sprite fruitYellow;
        [SerializeField] private Sprite shipPart;
        [SerializeField] private Sprite flowerYellow;
        [SerializeField] private Sprite flowerRed;
        [SerializeField] private Sprite flowerBlue;
        [SerializeField] private Sprite flowerPurple;
        [SerializeField] private Sprite flowerOrange;
        [SerializeField] private Sprite flowerGreen;
        [SerializeField] private Sprite honeyDrop;
        [SerializeField] private Sprite honeycomb;

        [Header("Story scene props")]
        [SerializeField] private Sprite propTree;
        [SerializeField] private Sprite propRock;
        [SerializeField] private Sprite propBush;

        /// <summary>
        /// The character the child is programming in a game: Ari the bee in Bal Peşinde, Roboya elsewhere. Ari's happy
        /// pose also stands in for "laughing". Falls back to Roboya when Ari's sprites are missing.
        /// </summary>
        public ActorSprites ActorFor(GameId game)
        {
            if (game == GameId.BalPesinde && ariFront != null)
            {
                return new ActorSprites(ariFront, ariBack, ariSide, ariHappy, ariHappy);
            }

            return new ActorSprites(robotFront, robotBack, robotSide, robotHappy, robotLaughing);
        }

        public Sprite RobotFront => robotFront;

        public Sprite RobotBack => robotBack;

        /// <summary>Faces left (west); mirrored for east.</summary>
        public Sprite RobotSide => robotSide;

        public Sprite RobotHappy => robotHappy;

        public Sprite RobotLaughing => robotLaughing;

        public Sprite GoalIdle => goalIdle;

        public Sprite GoalHappy => goalHappy;

        public Sprite TileFloor => tileFloor;

        public Sprite TilePath => tilePath;

        public Sprite Background => background;

        public Sprite DecorAt(int index) =>
            decor != null && index >= 0 && index < decor.Length ? decor[index] : null;

        /// <summary>Stable obstacle variety per cell so a level always looks the same.</summary>
        public Sprite ObstacleFor(int x, int y)
        {
            if (obstacles == null || obstacles.Length == 0)
            {
                return null;
            }

            int i = ((x * 7) + (y * 13)) % obstacles.Length;
            return obstacles[i < 0 ? -i : i];
        }

        /// <summary>Story pose for Roboya; an unassigned pose falls back to the front view.</summary>
        public Sprite RobotPoseSprite(RobotPose pose) => OrFallback(RobotPoseOrNull(pose), robotFront);

        /// <summary>Story pose for the region friend; an unassigned pose falls back to the idle sprite.</summary>
        public Sprite FriendPoseSprite(FriendPose pose) => OrFallback(FriendPoseOrNull(pose), goalIdle);

        /// <summary>Sprite for a story prop, or null when the region draws it in code (log) or lacks it.</summary>
        public Sprite PropSprite(StoryProp prop)
        {
            switch (prop)
            {
                case StoryProp.Apple: return fruitRed;
                case StoryProp.Pear: return fruitYellow;
                case StoryProp.Gear: return shipPart;
                case StoryProp.Tree: return propTree;
                case StoryProp.Rock: return propRock;
                case StoryProp.Bush: return propBush;
                default: return null;
            }
        }

        /// <summary>Board obstacle chosen by the level (grid.looks / scenery); null for the code-drawn log.</summary>
        public Sprite LookSprite(ObstacleLook look)
        {
            switch (look)
            {
                case ObstacleLook.Tree: return propTree;
                case ObstacleLook.Rock: return propRock;
                case ObstacleLook.Bush: return propBush;
                default: return null;
            }
        }

        public Sprite ItemFor(string kind, string color)
        {
            if (kind == "ship-part")
            {
                return shipPart;
            }

            if (kind == "honey")
            {
                return honeyDrop != null ? honeyDrop : fruitYellow;
            }

            if (kind == "flower")
            {
                var flower = FlowerOf(color);
                return flower != null ? flower : fruitYellow;
            }

            // Fruit: every colour gets a shape too (apple round, pear drop); green reads as the pear.
            return color == "yellow" || color == "green" ? fruitYellow : fruitRed;
        }

        /// <summary>Each flower colour has its own petal shape, so colour is never the only signal.</summary>
        private Sprite FlowerOf(string color)
        {
            switch (color)
            {
                case "red": return flowerRed;
                case "blue": return flowerBlue;
                case "purple": return flowerPurple;
                case "orange": return flowerOrange;
                case "green": return flowerGreen;
                default: return flowerYellow;
            }
        }

        private Sprite RobotPoseOrNull(RobotPose pose)
        {
            switch (pose)
            {
                case RobotPose.Happy: return robotHappy;
                case RobotPose.Curious: return robotCurious;
                case RobotPose.Surprised: return robotSurprised;
                case RobotPose.Proud: return robotProud;
                case RobotPose.Laughing: return robotLaughing;
                default: return robotFront;
            }
        }

        private Sprite FriendPoseOrNull(FriendPose pose)
        {
            switch (pose)
            {
                case FriendPose.Happy: return goalHappy;
                case FriendPose.Explaining: return goalExplaining;
                case FriendPose.Thanks: return goalThanks;
                default: return goalIdle;
            }
        }

        // Unity objects: explicit null checks, not ??, so unassigned slots fall back correctly.
        private static Sprite OrFallback(Sprite sprite, Sprite fallback) => sprite != null ? sprite : fallback;
    }

    /// <summary>The views of the character a game programs: front (south), back (north), side (west, mirrored for east).</summary>
    public readonly struct ActorSprites
    {
        public ActorSprites(Sprite front, Sprite back, Sprite side, Sprite happy, Sprite laughing)
        {
            Front = front;
            Back = back;
            Side = side;
            Happy = happy;
            Laughing = laughing;
        }

        public Sprite Front { get; }

        public Sprite Back { get; }

        public Sprite Side { get; }

        public Sprite Happy { get; }

        public Sprite Laughing { get; }
    }
}
