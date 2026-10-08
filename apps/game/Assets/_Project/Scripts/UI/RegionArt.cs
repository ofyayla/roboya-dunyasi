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

        [Header("Goal character")]
        [SerializeField] private Sprite goalIdle;
        [SerializeField] private Sprite goalHappy;

        [Header("Board")]
        [SerializeField] private Sprite tileFloor;
        [SerializeField] private Sprite tilePath;
        [SerializeField] private Sprite[] obstacles;
        [SerializeField] private Sprite background;

        [Header("Items")]
        [SerializeField] private Sprite fruitRed;
        [SerializeField] private Sprite fruitYellow;
        [SerializeField] private Sprite shipPart;

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

        public Sprite ItemFor(string kind, string color)
        {
            if (kind == "ship-part")
            {
                return shipPart;
            }

            return color == "yellow" ? fruitYellow : fruitRed;
        }
    }
}
