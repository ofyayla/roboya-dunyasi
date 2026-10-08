using System;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// First screen on a new device (A1): Roboya asks for "a grown-up's help" by voice, with one big grown-up button.
    /// The notice, the consent and the child profile come behind the parental gate. No text, so a child sees only Roboya.
    /// </summary>
    public sealed class WelcomeView : VisualElement
    {
        public WelcomeView(RegionArt art, Action onGrownUp)
        {
            name = "welcome";
            AddToClassList("map-view");
            AddToClassList("welcome");
            style.display = DisplayStyle.None;
            if (art != null && art.Background != null)
            {
                style.backgroundImage = new StyleBackground(art.Background);
            }

            var robot = new VisualElement { name = "welcome-robot", pickingMode = PickingMode.Ignore };
            robot.AddToClassList("welcome__robot");
            if (art != null && art.RobotFront != null)
            {
                robot.style.backgroundImage = new StyleBackground(art.RobotFront);
            }

            Add(robot);
            var button = new IconButton(IconKind.Grownup, onGrownUp) { name = "welcome-start" };
            button.AddToClassList("welcome__button");
            Add(button);
        }
    }
}
