using System;
using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Sharing;
using Roboya.CodingEngine.World;
using static Roboya.Tests.CodingEngine.SharingLevels;

namespace Roboya.Tests.CodingEngine
{
    public class SharingLevelTests
    {
        [Test]
        public void New_ValidLevel_KeepsWhatItWasGiven()
        {
            var level = Make(new[] { 2, 1 }, basket: 3, flowers: new[] { 1 }, cap: 4, table: true, amounts: true, has: new[] { 1, 0 });

            Assert.AreEqual(2, level.Hives.Count);
            Assert.AreEqual(2, level.Hives[0].Need);
            Assert.AreEqual(1, level.Hives[0].Has);
            Assert.AreEqual(4, level.TotalHoney);
            Assert.AreEqual(4, level.BasketCap);
            Assert.IsTrue(level.HasTable);
            Assert.IsTrue(level.AllowsAmounts);
        }

        [Test]
        public void Start_BuildsTheStateFromTheLevel()
        {
            var state = Make(new[] { 2, 1 }, basket: 3, flowers: new[] { 1, 2 }, has: new[] { 1, 0 }).Start();

            Assert.AreEqual(3, state.Basket);
            Assert.AreEqual(1, state.FlowerAt(0));
            Assert.AreEqual(2, state.FlowerAt(1));
            Assert.AreEqual(1, state.HiveAt(0));
            Assert.AreEqual(0, state.Table);
        }

        [Test]
        public void RoomIn_WithoutCap_IsUnlimited_WithCap_IsWhatIsLeft()
        {
            Assert.AreEqual(int.MaxValue, Make(new[] { 1 }, basket: 1).RoomIn(Make(new[] { 1 }, basket: 1).Start()));
            var capped = Make(new[] { 1 }, basket: 1, cap: 3);
            Assert.AreEqual(2, capped.RoomIn(capped.Start()));
        }

        [Test]
        public void New_TooManyHives_Throws() =>
            Assert.Throws<LevelValidationException>(() => Make(new[] { 1, 1, 1, 1, 1 }, basket: 5));

        [Test]
        public void New_NoHives_Throws() =>
            Assert.Throws<LevelValidationException>(() => Make(new int[0], basket: 1));

        [Test]
        public void New_DeliverHiveAsksForNothing_Throws() =>
            Assert.Throws<LevelValidationException>(() => Make(new[] { 0 }, basket: 1));

        [Test]
        public void New_NegativeHiveHoney_Throws() =>
            Assert.Throws<LevelValidationException>(() => Make(new[] { 1 }, basket: 1, has: new[] { -1 }));

        [Test]
        public void New_ShareWithOneHive_Throws() =>
            Assert.Throws<LevelValidationException>(() => Make(new[] { 0 }, basket: 2, goal: SharingGoalKind.Share));

        [Test]
        public void New_TooManyFlowers_Throws() =>
            Assert.Throws<LevelValidationException>(() => Make(new[] { 1 }, flowers: new[] { 1, 1, 1, 1 }));

        [Test]
        public void New_EmptyFlower_Throws() =>
            Assert.Throws<LevelValidationException>(() => Make(new[] { 1 }, flowers: new[] { 0 }));

        [Test]
        public void New_MoreThanSevenHoney_Throws() =>
            Assert.Throws<LevelValidationException>(() => Make(new[] { 1 }, basket: 5, flowers: new[] { 3 }));

        [Test]
        public void New_StartBasketBeyondCap_Throws() =>
            Assert.Throws<LevelValidationException>(() => Make(new[] { 1 }, basket: 3, cap: 2));

        [Test]
        public void New_NoCards_Throws() =>
            Assert.Throws<LevelValidationException>(() => Make(new[] { 1 }, basket: 1, maxCards: 0));

        [Test]
        public void New_NullId_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new SharingLevel(null, new[] { new HiveSpec(1) }, null, 1, 0, false, SharingGoalKind.Deliver, false, 3));

        [Test]
        public void RepeatBox_OutOfRange_Throws()
        {
            Assert.Throws<LevelValidationException>(() => new RepeatBoxSpec(0, 2));
            Assert.Throws<LevelValidationException>(() => new RepeatBoxSpec(5, 2));
            Assert.Throws<LevelValidationException>(() => new RepeatBoxSpec(2, 0));
            Assert.Throws<LevelValidationException>(() => new RepeatBoxSpec(2, 4));
        }
    }

    public class SharingStateTests
    {
        [Test]
        public void Took_And_Gave_ReturnNewStates()
        {
            var level = Make(new[] { 2 }, flowers: new[] { 3 });
            var start = level.Start();

            var took = start.Took(0, 2);
            var gave = took.Gave(0, 1);
            var table = gave.Gave(SharingLevel.Table, 1);

            Assert.AreEqual(3, start.FlowerAt(0));
            Assert.AreEqual(1, took.FlowerAt(0));
            Assert.AreEqual(0, start.Basket);
            Assert.AreEqual(2, took.Basket);
            Assert.AreEqual(1, gave.HiveAt(0));
            Assert.AreEqual(1, gave.Basket);
            Assert.AreEqual(1, table.Table);
            Assert.AreEqual(1, table.At(SharingLevel.Table));
            Assert.AreEqual(1, table.At(0));
            Assert.AreEqual(0, table.Basket);
        }

        [Test]
        public void Equals_SameHoney_IsEqualAndHasTheSameKey()
        {
            var level = Make(new[] { 1, 1 }, basket: 2);
            var a = level.Start().Gave(0, 1);
            var b = level.Start().Gave(0, 1);
            var c = level.Start().Gave(1, 1);

            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreNotEqual(a, c);
            Assert.IsFalse(a.Equals(null));
            Assert.IsFalse(a.Equals((object)"x"));
        }

        [Test]
        public void Counts_ReportSizes()
        {
            var state = Make(new[] { 1, 1 }, flowers: new[] { 1 }).Start();

            Assert.AreEqual(2, state.HiveCount);
            Assert.AreEqual(1, state.FlowerCount);
        }
    }

    public class SharingCardTests
    {
        [Test]
        public void Equals_ComparesKindTargetAndCount()
        {
            Assert.AreEqual(SharingCard.Give(1, 2), SharingCard.Give(1, 2));
            Assert.AreNotEqual(SharingCard.Give(1, 2), SharingCard.Give(1, 3));
            Assert.AreNotEqual(SharingCard.Give(1), SharingCard.Give(0));
            Assert.AreNotEqual(SharingCard.Take(0), SharingCard.Give(0));
            Assert.AreEqual(SharingCard.Take(1), SharingCard.Take(1));
            Assert.IsFalse(SharingCard.Take(1).Equals(null));
            Assert.IsFalse(SharingCard.Take(1).Equals((object)"x"));
            Assert.AreEqual(SharingCard.Take(1).GetHashCode(), SharingCard.Take(1).GetHashCode());
        }

        [Test]
        public void SameTarget_IgnoresTheCount()
        {
            Assert.IsTrue(SharingCard.Give(0, 1).SameTarget(SharingCard.Give(0, 3)));
            Assert.IsFalse(SharingCard.Give(0).SameTarget(SharingCard.Give(1)));
            Assert.IsFalse(SharingCard.Give(0).SameTarget(null));
        }

        [Test]
        public void IsTable_OnlyForTheTableTarget()
        {
            Assert.IsTrue(SharingCard.Give(SharingLevel.Table).IsTable);
            Assert.IsFalse(SharingCard.Give(0).IsTable);
            Assert.IsFalse(SharingCard.Take(0).IsTable);
        }

        [Test]
        public void WithCount_KeepsTheTarget() => Assert.AreEqual(SharingCard.Give(2, 3), SharingCard.Give(2).WithCount(3));

        [Test]
        public void ToString_NamesTheCard()
        {
            Assert.AreEqual("take(1)", SharingCard.Take(1).ToString());
            Assert.AreEqual("give(2,3)", SharingCard.Give(2, 3).ToString());
            Assert.AreEqual("give(table,1)", SharingCard.Give(SharingLevel.Table).ToString());
        }
    }

    public class SharingProgramTests
    {
        [Test]
        public void Expand_RepeatsTheBoxThenTheTail()
        {
            var program = Repeat(2, new[] { G0, G1 }, GT);

            CollectionAssert.AreEqual(new[] { G0, G1, G0, G1, GT }, program.Expand());
            Assert.AreEqual(3, program.CardCount);
        }

        [Test]
        public void Linear_HasNoBoxAndOneTurn()
        {
            var program = Plan(G0, G1);

            Assert.AreEqual(1, program.Turns);
            Assert.AreEqual(0, program.Box.Count);
            Assert.AreEqual(2, program.CardCount);
        }

        [Test]
        public void New_ZeroTurns_BecomesOne() => Assert.AreEqual(1, new SharingProgram(0, null, null).Turns);

        [Test]
        public void Expand_EmptyBoxIgnoresTurns() => Assert.AreEqual(1, new SharingProgram(3, null, new[] { G0 }).Expand().Count);
    }

    public class SharingInterpreterTests
    {
        [Test]
        public void Run_GiveToEachHive_Wins()
        {
            var result = SharingInterpreter.Run(OneEach(), Plan(G0, G1));

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Stopped);
            Assert.IsNull(result.StoppedCard);
            Assert.AreEqual(0, result.Final.Basket);
            CollectionAssert.AreEqual(
                new[] { SharingEventKind.Gave, SharingEventKind.Gave },
                result.Events.Select(e => e.Kind).ToArray());
            Assert.AreEqual(1, result.Events[0].BasketAfter);
            Assert.AreEqual(1, result.Events[0].TargetCountAfter);
        }

        [Test]
        public void Run_ForgottenHive_DoesNotWin()
        {
            var result = SharingInterpreter.Run(OneEach(), Plan(G0));

            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(result.Stopped);
            Assert.AreEqual(1, result.Final.Basket);
        }

        [Test]
        public void Run_TooMuchForADeliveryHive_SpillsAndKeepsRunning()
        {
            var result = SharingInterpreter.Run(OneEach(), Plan(G0, G0));

            Assert.IsTrue(result.Spilled);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(result.Stopped);
            Assert.AreEqual(2, result.Final.HiveAt(0));
            Assert.AreEqual(SharingEventKind.Spilled, result.Events.Last().Kind);
        }

        [Test]
        public void Run_ShareHivesNeverSpill()
        {
            var level = Make(new[] { 0, 0 }, basket: 4, goal: SharingGoalKind.Share);

            var result = SharingInterpreter.Run(level, Plan(G0, G0, G0));

            Assert.IsFalse(result.Spilled);
            Assert.IsFalse(result.Events.Any(e => e.Kind == SharingEventKind.Spilled));
        }

        [Test]
        public void Run_GiveWithAnEmptyBasket_StopsOnThatCard()
        {
            var result = SharingInterpreter.Run(OneEach(), Plan(G0, G1, G0));

            Assert.IsTrue(result.Stopped);
            Assert.AreEqual(SharingStopReason.BasketEmpty, result.StopReason);
            Assert.AreEqual(G0, result.StoppedCard);
            Assert.AreEqual(SharingEventKind.Stopped, result.Events.Last().Kind);
            Assert.IsTrue(result.IsSuccess, "the goal was already met when the extra card stopped");
        }

        [Test]
        public void Run_GiveCountLargerThanTheBasket_GivesWhatIsThereAndStops()
        {
            var level = Make(new[] { 3 }, basket: 2, amounts: true);

            var result = SharingInterpreter.Run(level, Plan(SharingCard.Give(0, 3)));

            Assert.AreEqual(SharingStopReason.BasketEmpty, result.StopReason);
            Assert.AreEqual(2, result.Final.HiveAt(0));
            Assert.AreEqual(2, result.Events.Count(e => e.Kind == SharingEventKind.Gave));
        }

        [Test]
        public void Run_TakeTakesAllThatFits()
        {
            var level = Make(new[] { 3 }, flowers: new[] { 3 }, cap: 2, amounts: true);

            var result = SharingInterpreter.Run(level, Plan(T0));

            Assert.AreEqual(2, result.Final.Basket);
            Assert.AreEqual(1, result.Final.FlowerAt(0));
            Assert.AreEqual(SharingEventKind.Took, result.Events[0].Kind);
            Assert.AreEqual(2, result.Events[0].Amount);
            Assert.AreEqual(2, result.Events[0].BasketAfter);
        }

        [Test]
        public void Run_TakeWithAFullBasket_StopsBasketFull()
        {
            var level = Make(new[] { 3 }, flowers: new[] { 2, 1 }, cap: 2, amounts: true);

            var result = SharingInterpreter.Run(level, Plan(T0, T1));

            Assert.AreEqual(SharingStopReason.BasketFull, result.StopReason);
            Assert.AreEqual(T1, result.StoppedCard);
            Assert.AreEqual(1, result.Final.FlowerAt(1), "the flower keeps its honey");
        }

        [Test]
        public void Run_TakeFromAnEmptyFlower_StopsFlowerEmpty()
        {
            var level = Make(new[] { 1 }, flowers: new[] { 1 });

            var result = SharingInterpreter.Run(level, Plan(T0, T0));

            Assert.AreEqual(SharingStopReason.FlowerEmpty, result.StopReason);
            Assert.AreEqual(1, result.Final.Basket);
        }

        [Test]
        public void Run_Repeat_RunsTheBoxPerTurn()
        {
            var level = ShareThreeWithBox();

            var result = SharingInterpreter.Run(level, Repeat(2, new[] { G0, G1, G2 }));

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(6, result.Events.Count(e => e.Kind == SharingEventKind.Gave));
        }

        [Test]
        public void Run_EventsCarryTheProgramCard()
        {
            var card = SharingCard.Give(1);

            var result = SharingInterpreter.Run(OneEach(), Plan(G0, card));

            Assert.AreSame(card, result.Events[1].Card);
        }

        [Test]
        public void TryDo_CardThatWouldStop_IsFalse()
        {
            var level = OneEach();

            Assert.IsTrue(SharingInterpreter.TryDo(level, level.Start(), G0, out var next));
            Assert.AreEqual(1, next.Basket);
            Assert.IsFalse(SharingInterpreter.TryDo(level, next.Gave(1, 1), G0, out _));
        }

        [Test]
        public void Step_WithoutEventList_StillMovesTheHoney()
        {
            var level = OneEach();
            bool spilled = false;

            var next = SharingInterpreter.Step(level, level.Start(), G0, null, out var stop, ref spilled);

            Assert.AreEqual(SharingStopReason.None, stop);
            Assert.AreEqual(1, next.HiveAt(0));
        }
    }

    public class SharingRulesTests
    {
        [Test]
        public void IsGoalMet_Deliver_NeedsExactlyWhatEachHiveAsks()
        {
            var level = Make(new[] { 2, 1 }, basket: 4);

            Assert.IsFalse(SharingRules.IsGoalMet(level, level.Start()));
            Assert.IsTrue(SharingRules.IsGoalMet(level, level.Start().Gave(0, 2).Gave(1, 1)));
            Assert.IsFalse(SharingRules.IsGoalMet(level, level.Start().Gave(0, 3).Gave(1, 1)));
        }

        [Test]
        public void IsGoalMet_Deliver_HoneyMayStayInTheBasket()
        {
            var level = Subtraction();

            Assert.IsTrue(SharingRules.IsGoalMet(level, level.Start().Gave(0, 2)));
        }

        [Test]
        public void IsGoalMet_Share_NeedsEqualHivesAndAnEmptyBasket()
        {
            var level = Make(new[] { 0, 0 }, basket: 4, goal: SharingGoalKind.Share);
            var start = level.Start();

            Assert.IsFalse(SharingRules.IsGoalMet(level, start));
            Assert.IsTrue(SharingRules.IsGoalMet(level, start.Gave(0, 2).Gave(1, 2)));
            Assert.IsFalse(SharingRules.IsGoalMet(level, start.Gave(0, 3).Gave(1, 1)), "unequal");
            Assert.IsFalse(SharingRules.IsGoalMet(level, start.Gave(0, 1).Gave(1, 1)), "honey left in the basket");
        }

        [Test]
        public void IsGoalMet_ShareWithoutATable_AllowsNothingLeftOver()
        {
            var level = Make(new[] { 0, 0 }, basket: 5, goal: SharingGoalKind.Share);
            var state = level.Start().Gave(0, 2).Gave(1, 2);

            Assert.IsFalse(SharingRules.IsGoalMet(level, state));
        }

        [Test]
        public void IsGoalMet_ShareWithATable_TheLeftOverIsFewerThanTheHives()
        {
            var level = Make(new[] { 0, 0, 0 }, basket: 7, table: true, goal: SharingGoalKind.Share);
            var start = level.Start();

            Assert.IsTrue(SharingRules.IsGoalMet(level, start.Gave(0, 2).Gave(1, 2).Gave(2, 2).Gave(SharingLevel.Table, 1)));
            Assert.IsFalse(SharingRules.IsGoalMet(level, start.Gave(0, 1).Gave(1, 1).Gave(2, 1).Gave(SharingLevel.Table, 4)), "the table keeps as much as the hives");
        }

        [Test]
        public void WaitingHives_DeliveryShortOfWhatItAsked()
        {
            var level = Make(new[] { 2, 1 }, basket: 4);

            var waiting = SharingRules.WaitingHives(level, level.Start().Gave(0, 2));

            CollectionAssert.AreEqual(new[] { false, true }, waiting);
        }

        [Test]
        public void WaitingHives_ShareBelowTheOthersOrEmpty()
        {
            var level = Make(new[] { 0, 0, 0 }, basket: 6, goal: SharingGoalKind.Share);

            var waiting = SharingRules.WaitingHives(level, level.Start().Gave(0, 2).Gave(1, 1));

            CollectionAssert.AreEqual(new[] { false, true, true }, waiting);
        }

        [Test]
        public void WaitingHives_WhenTheGoalIsMet_NobodyWaits()
        {
            var level = OneEach();

            CollectionAssert.AreEqual(new[] { false, false }, SharingRules.WaitingHives(level, level.Start().Gave(0, 1).Gave(1, 1)));
        }

        [Test]
        public void IsEqualButLeftOver_ShareThatCameOutEqualWithHoneyLeft()
        {
            var level = Make(new[] { 0, 0 }, basket: 5, goal: SharingGoalKind.Share);

            Assert.IsTrue(SharingRules.IsEqualButLeftOver(level, level.Start().Gave(0, 2).Gave(1, 2)));
            Assert.IsFalse(SharingRules.IsEqualButLeftOver(level, level.Start().Gave(0, 3).Gave(1, 1)));
            Assert.IsFalse(SharingRules.IsEqualButLeftOver(level, level.Start()), "nobody has any");
            Assert.IsFalse(SharingRules.IsEqualButLeftOver(OneEach(), OneEach().Start().Gave(0, 1).Gave(1, 1)), "not a share level");
        }

        [Test]
        public void IsEqualButLeftOver_ShareThatIsWon_IsFalse()
        {
            var level = Make(new[] { 0, 0 }, basket: 4, goal: SharingGoalKind.Share);

            Assert.IsFalse(SharingRules.IsEqualButLeftOver(level, level.Start().Gave(0, 2).Gave(1, 2)));
        }
    }
}
