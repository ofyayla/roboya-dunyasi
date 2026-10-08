using System;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// "Roboya's charge is empty" (VEL-02): the daily play time is used up, so no new level starts. A sleepy Roboya and
    /// an empty battery tell the child, with no text and no scolding; a parent can change the limit behind the gate.
    /// </summary>
    public sealed class RestView : VisualElement
    {
        public RestView(RegionArt art, Action onGrownUp)
        {
            name = "rest";
            AddToClassList("map-view");
            AddToClassList("rest");
            style.display = DisplayStyle.None;
            if (art != null && art.Background != null)
            {
                style.backgroundImage = new StyleBackground(art.Background);
            }

            var robot = new VisualElement { name = "rest-robot", pickingMode = PickingMode.Ignore };
            robot.AddToClassList("rest__robot");
            if (art != null && art.RobotFront != null)
            {
                robot.style.backgroundImage = new StyleBackground(art.RobotFront);
            }

            Add(robot);
            var battery = new Icon(IconKind.Battery) { Color = new Color(0.35f, 0.27f, 0.22f), Accent = Color.white, name = "rest-battery" };
            battery.AddToClassList("rest__battery");
            Add(battery);
            var button = new IconButton(IconKind.Grownup, onGrownUp) { name = "rest-parent" };
            button.AddToClassList("welcome__button");
            Add(button);
        }
    }
}
