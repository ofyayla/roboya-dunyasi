using UnityEngine;

namespace Roboya.UI
{
    /// <summary>
    /// Sprites for robot parts and the island map, looked up by the names used in content/rewards/robot-parts.json
    /// and content/map/island.json. Filled by the editor SceneBuilder from Art/RobotParts and Art/Island.
    /// </summary>
    [CreateAssetMenu(menuName = "Roboya/Part Art")]
    public sealed class PartArt : ScriptableObject
    {
        [SerializeField] private Sprite[] sprites;

        public Sprite Find(string spriteName)
        {
            if (sprites == null || string.IsNullOrEmpty(spriteName))
            {
                return null;
            }

            foreach (var s in sprites)
            {
                if (s != null && s.name == spriteName)
                {
                    return s;
                }
            }

            return null;
        }
    }
}
