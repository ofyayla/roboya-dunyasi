using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Levels;
using Roboya.CodingEngine.Play;
using Roboya.Games.Common;
using Roboya.UI;
using UnityEngine.UIElements;

namespace Roboya.Tests.Core
{
    public class DropCardTests
    {
        [Test]
        public void CardElement_DropCard_HasItsOwnIconAndStyle()
        {
            var card = new CardElement(CardType.Drop);

            Assert.AreEqual(IconKind.Drop, CardElement.IconFor(CardType.Drop));
            Assert.AreEqual("drop", CardElement.ClassSuffix(CardType.Drop));
            Assert.IsTrue(card.ClassListContains("card--drop"));
            Assert.AreEqual(IconKind.Drop, card.Icon.Kind);
        }

        [Test]
        public void Bind_PaletteWithDrop_ShowsDropLastAfterTheMoves()
        {
            var palette = new VisualElement();
            var tray = new CardTray(palette, new VisualElement(), new VisualElement());
            var cards = new[] { CardType.Drop, CardType.TurnRight, CardType.Forward };
            tray.Bind(new PlanStrip(4, cards), cards);

            var shown = palette.Children().OfType<CardElement>().Select(c => c.Card).ToList();

            CollectionAssert.AreEqual(new[] { CardType.Forward, CardType.TurnRight, CardType.Drop }, shown);
        }

        [Test]
        public void Bind_PaletteWithoutDrop_HasNoDropCard()
        {
            var palette = new VisualElement();
            var tray = new CardTray(palette, new VisualElement(), new VisualElement());
            var cards = new[] { CardType.Forward, CardType.TurnRight };
            tray.Bind(new PlanStrip(4, cards), cards);

            Assert.IsFalse(palette.Children().OfType<CardElement>().Any(c => c.Card == CardType.Drop));
        }

        [Test]
        public void CardIntroVoice_Drop_FollowsTheCardNamePattern()
        {
            Assert.AreEqual("card.drop.intro", LevelStory.CardIntroVoice(CardId.Drop));
        }
    }
}
