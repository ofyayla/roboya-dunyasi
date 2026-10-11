using System.Threading;
using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.World;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.Common
{
    /// <summary>
    /// The village's answer when a run ends (plan D3, PK-0001, K5): the hive families side by side, each with its hive and its slots, so
    /// "the same" is seen and not read. A hive with less than its share has a sad bee; one that got too much spills; when the level is won
    /// every bee is glad. No numbers, no stars lost: it only shows who was left out. Hidden between uses.
    /// </summary>
    public sealed class HiveCompareView : VisualElement
    {
        public const float ShowSeconds = 2.6f;

        private readonly RegionArt _art;
        private readonly VisualElement _panel = new VisualElement { pickingMode = PickingMode.Ignore };

        public HiveCompareView(RegionArt art)
        {
            _art = art;
            name = "hive-compare";
            pickingMode = PickingMode.Ignore;
            AddToClassList("hive-compare");
            _panel.AddToClassList("hive-compare__panel");
            Add(_panel);
            style.display = DisplayStyle.None;
        }

        public bool IsOpen => style.display == DisplayStyle.Flex;

        /// <summary>Builds the row for the final state and shows it for a moment; the table joins when the left-over lies on it.</summary>
        public async Awaitable ShowAsync(Level level, RobotState final, HiveFeedback feedback, bool success, CancellationToken token)
        {
            if (feedback.Hives.Count == 0)
            {
                return;
            }

            _panel.Clear();
            var report = HiveReport.Of(level, final);
            for (int k = 0; k < feedback.Hives.Count; k++)
            {
                var status = report.Drops[feedback.Hives[k]];
                _panel.Add(Family(status, feedback.Moods[k], success));
            }

            foreach (var status in report.Drops)
            {
                if (status.Kind == DropPointKind.Table && status.Count > 0)
                {
                    _panel.Add(TableCell(status));
                }
            }

            style.display = DisplayStyle.Flex;
            try
            {
                await Tween.Delay(ShowSeconds, token);
            }
            finally
            {
                style.display = DisplayStyle.None;
            }
        }

        private VisualElement Family(DropStatus status, HiveMood mood, bool success)
        {
            string suffix = mood == HiveMood.Sad ? "sad" : mood == HiveMood.Spilling ? "spilling" : success ? "thanks" : "happy";
            var cell = new VisualElement { name = "family-" + status.Index, pickingMode = PickingMode.Ignore };
            cell.AddToClassList("hive-compare__family");

            var villager = new VisualElement { pickingMode = PickingMode.Ignore };
            villager.AddToClassList("hive-compare__villager");
            villager.AddToClassList("hive-compare__villager--" + suffix);
            var sprite = _art == null ? null
                : mood == HiveMood.Sad ? _art.VillagerSad
                : success ? _art.VillagerThanks
                : _art.VillagerHappy;
            if (sprite != null)
            {
                villager.style.backgroundImage = new StyleBackground(sprite);
            }
            else
            {
                villager.AddToClassList("hive-compare__villager--fallback");
            }

            cell.Add(villager);

            var hive = new VisualElement { pickingMode = PickingMode.Ignore };
            hive.AddToClassList("hive-compare__hive");
            var hiveSprite = _art != null ? _art.HiveFor(status.State) : null;
            if (hiveSprite != null)
            {
                hive.style.backgroundImage = new StyleBackground(hiveSprite);
            }
            else
            {
                hive.AddToClassList("hive-compare__hive--fallback");
            }

            cell.Add(hive);
            cell.Add(Slots(status.Need, status.Count));
            return cell;
        }

        private VisualElement TableCell(DropStatus status)
        {
            var cell = new VisualElement { name = "family-table", pickingMode = PickingMode.Ignore };
            cell.AddToClassList("hive-compare__family");
            var table = new VisualElement { pickingMode = PickingMode.Ignore };
            table.AddToClassList("hive-compare__hive");
            var sprite = _art != null ? _art.PropSprite(Roboya.CodingEngine.Levels.Generated.StoryProp.VillageTable) : null;
            if (sprite != null)
            {
                table.style.backgroundImage = new StyleBackground(sprite);
            }
            else
            {
                table.AddToClassList("hive-compare__hive--fallback");
            }

            cell.Add(table);
            cell.Add(Slots(status.Count, status.Count));
            return cell;
        }

        private static VisualElement Slots(int capacity, int filled)
        {
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.AddToClassList("hive-compare__slots");
            int total = Mathf.Max(capacity, filled);
            for (int i = 0; i < total; i++)
            {
                var slot = new VisualElement { pickingMode = PickingMode.Ignore };
                slot.AddToClassList("hive__slot");
                slot.AddToClassList("hive-compare__slot");
                slot.EnableInClassList("hive__slot--filled", i < filled);
                row.Add(slot);
            }

            return row;
        }
    }
}
