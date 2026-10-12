using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Sharing;
using static Roboya.Tests.CodingEngine.SharingLevels;

namespace Roboya.Tests.CodingEngine
{
    public class SharingSolverTests
    {
        [Test]
        public void Solve_OneEach_GivesToBothHivesInEitherOrder()
        {
            var solution = SharingSolver.Solve(OneEach());

            Assert.AreEqual(2, solution.Length);
            Assert.IsTrue(solution.IsSolvable);
            Assert.AreEqual(2, solution.Programs.Count);
            CollectionAssert.AreEquivalent(
                new[] { "give(0,1) give(1,1)", "give(1,1) give(0,1)" },
                solution.Programs.Select(p => string.Join(" ", p.Tail)));
        }

        [Test]
        public void Solve_ThreeHoneyForOneHive_NeedsThreeGivesWithoutAmounts()
        {
            var solution = SharingSolver.Solve(Make(new[] { 3 }, basket: 3));

            Assert.AreEqual(3, solution.Length);
            Assert.AreEqual(1, solution.Programs.Count);
        }

        [Test]
        public void Solve_WithAmounts_OneCardGivesThree()
        {
            var solution = SharingSolver.Solve(Make(new[] { 3 }, basket: 4, amounts: true));

            Assert.AreEqual(1, solution.Length);
            Assert.AreEqual(SharingCard.Give(0, 3), solution.Programs[0].Tail[0]);
        }

        [Test]
        public void Solve_BasketOfTwo_OrderMatters()
        {
            var level = BasketOfTwo();

            var solution = SharingSolver.Solve(level);
            var withoutCap = SharingSolver.Solve(Make(new[] { 3 }, flowers: new[] { 2, 1 }, amounts: true));

            Assert.AreEqual(4, solution.Length, "take, give, take, give");
            Assert.AreEqual(3, withoutCap.Length, "take both, give three");
            Assert.IsTrue(solution.Programs.All(p => SharingInterpreter.Run(level, p).IsSuccess));
            Assert.IsFalse(SharingInterpreter.Run(level, Plan(T0, T1, SharingCard.Give(0, 3))).IsSuccess, "the basket cannot hold both");
        }

        [Test]
        public void Solve_Subtraction_LeavesThreeInTheBasket()
        {
            var level = Subtraction();

            var solution = SharingSolver.Solve(level);

            Assert.AreEqual(1, solution.Length);
            Assert.AreEqual(3, SharingPrediction.LeftAfter(level, solution.Programs[0]));
        }

        [Test]
        public void Solve_ShareFourBetweenTwo_AllArrangementsOfTwoEach()
        {
            var level = Make(new[] { 0, 0 }, basket: 4, goal: SharingGoalKind.Share);

            var solution = SharingSolver.Solve(level);

            Assert.AreEqual(4, solution.Length);
            Assert.AreEqual(6, solution.Programs.Count);
        }

        [Test]
        public void Solve_ManyShortestPlans_AreCapped()
        {
            var level = Make(new[] { 0, 0, 0 }, basket: 7, table: true, goal: SharingGoalKind.Share);

            var solution = SharingSolver.Solve(level);

            Assert.AreEqual(7, solution.Length);
            Assert.AreEqual(SharingSolver.MaxPrograms, solution.Programs.Count);
        }

        [Test]
        public void Solve_NotEnoughHoney_IsUnsolvable()
        {
            var solution = SharingSolver.Solve(Make(new[] { 3 }, basket: 2));

            Assert.AreEqual(-1, solution.Length);
            Assert.IsFalse(solution.IsSolvable);
            Assert.AreEqual(0, solution.Programs.Count);
        }

        [Test]
        public void Solve_PlanLongerThanTheStrip_IsUnsolvable()
        {
            var solution = SharingSolver.Solve(Make(new[] { 3 }, basket: 3, maxCards: 2));

            Assert.IsFalse(solution.IsSolvable);
        }

        [Test]
        public void Solve_AlreadyWon_IsLengthZero()
        {
            var level = Make(new[] { 1 }, basket: 0, has: new[] { 1 });

            var solution = SharingSolver.Solve(level);

            Assert.AreEqual(0, solution.Length);
            Assert.AreEqual(1, solution.Programs.Count);
            Assert.AreEqual(0, solution.Programs[0].CardCount);
        }

        [Test]
        public void Solve_HivesThatAlreadyHoldHoney_AskLess()
        {
            var level = Make(new[] { 3 }, basket: 1, has: new[] { 2 });

            Assert.AreEqual(1, SharingSolver.Solve(level).Length);
        }

        [Test]
        public void Solve_RepeatBox_FindsTheBodyAndTheTurns()
        {
            var level = ShareThreeWithBox();

            var solution = SharingSolver.Solve(level);

            Assert.AreEqual(3, solution.Length);
            Assert.AreEqual(6, solution.Programs.Count, "the three hives in any order");
            Assert.IsTrue(solution.Programs.All(p => p.Turns == 2 && p.Box.Count == 3 && p.Tail.Count == 0));
            Assert.IsTrue(solution.Programs.All(p => SharingInterpreter.Run(level, p).IsSuccess));
        }

        [Test]
        public void Solve_RepeatBoxIsNeeded_WhenTheStripIsTooShortWithoutIt()
        {
            var level = ShareThreeWithBox();

            Assert.IsFalse(SharingSolver.Solve(level, allowRepeat: false).IsSolvable);
            Assert.IsTrue(SharingSolver.Solve(level).IsSolvable);
        }

        [Test]
        public void Solve_RepeatBoxWithATable_EndsWithTheTableCard()
        {
            var level = ShareThreeWithTable();

            var solution = SharingSolver.Solve(level);

            Assert.AreEqual(4, solution.Length);
            Assert.IsTrue(solution.Programs.All(p => p.Tail.Count == 1 && p.Tail[0].IsTable));
        }

        [Test]
        public void Solve_RepeatBoxThatCannotHelp_IsUnsolvable()
        {
            var level = Make(new[] { 0, 0 }, basket: 4, goal: SharingGoalKind.Share, maxCards: 1, box: new RepeatBoxSpec(1, 3));

            Assert.IsFalse(SharingSolver.Solve(level).IsSolvable);
        }

        [Test]
        public void Alphabet_ListsTakesThenGivesThenTheTable()
        {
            var level = Make(new[] { 1, 1 }, flowers: new[] { 1 }, table: true, amounts: true);

            var cards = SharingSolver.Alphabet(level);

            Assert.AreEqual(1 + (2 * 3) + 3, cards.Count);
            Assert.AreEqual(SharingCard.Take(0), cards[0]);
            Assert.AreEqual(SharingCard.Give(0, 1), cards[1]);
            Assert.IsTrue(cards.Last().IsTable);
        }

        [Test]
        public void ShortestFrom_WithoutAPlanList_StillReportsTheDepth()
        {
            var level = OneEach();

            Assert.AreEqual(2, SharingSolver.ShortestFrom(level, level.Start(), 5, SharingSolver.Alphabet(level), null));
            Assert.AreEqual(-1, SharingSolver.ShortestFrom(level, level.Start(), 1, SharingSolver.Alphabet(level), null));
            Assert.AreEqual(-1, SharingSolver.ShortestFrom(level, level.Start(), 5, new SharingCard[0], null));
        }
    }

    public class SharingHintTests
    {
        [Test]
        public void Next_EmptyPlan_AddsTheFirstCardOnAShortestWay()
        {
            var hint = SharingHint.Next(OneEach(), Plan());

            Assert.AreEqual(SharingHintKind.Add, hint.Kind);
            Assert.AreEqual(G0, hint.Card);
            Assert.IsFalse(hint.InBox);
        }

        [Test]
        public void Next_AnyOrderIsFine()
        {
            var hint = SharingHint.Next(OneEach(), Plan(G1));

            Assert.AreEqual(SharingHintKind.Add, hint.Kind);
            Assert.AreEqual(G0, hint.Card, "the second hive was done first: only the first is left");
        }

        [Test]
        public void Next_CardThatSpills_IsRemoved()
        {
            var hint = SharingHint.Next(OneEach(), Plan(G0, G0));

            Assert.AreEqual(SharingHintKind.Remove, hint.Kind);
            Assert.AreEqual(1, hint.Index);
            Assert.AreEqual(G0, hint.Card);
        }

        [Test]
        public void Next_RightPlan_IsPressPlay() => Assert.AreEqual(SharingHintKind.PressPlay, SharingHint.Next(OneEach(), Plan(G1, G0)).Kind);

        [Test]
        public void Next_RightCardWrongCount_AsksToChangeTheCount()
        {
            var level = Make(new[] { 3 }, basket: 4, amounts: true);

            var hint = SharingHint.Next(level, Plan(SharingCard.Give(0, 1)));

            Assert.AreEqual(SharingHintKind.SetCount, hint.Kind);
            Assert.AreEqual(0, hint.Index);
            Assert.AreEqual(3, hint.Count);
        }

        [Test]
        public void Next_EmptyPlanWithAmounts_AddsTheCardThatHandsOverEnough()
        {
            var level = Make(new[] { 3 }, basket: 4, amounts: true);

            var hint = SharingHint.Next(level, Plan());

            Assert.AreEqual(SharingCard.Give(0, 3), hint.Card);
        }

        [Test]
        public void Next_TakeThatDoesNotFit_IsRemoved()
        {
            var hint = SharingHint.Next(BasketOfTwo(), Plan(T0, T1));

            Assert.AreEqual(SharingHintKind.Remove, hint.Kind);
            Assert.AreEqual(1, hint.Index);
        }

        [Test]
        public void Next_BasketOfTwo_FollowsTheChildsOwnOrder()
        {
            var hint = SharingHint.Next(BasketOfTwo(), Plan(T0, SharingCard.Give(0, 2)));

            Assert.AreEqual(SharingHintKind.Add, hint.Kind);
            Assert.AreEqual(T1, hint.Card);
        }

        [Test]
        public void Next_UnsolvableLevel_HasNoHint()
        {
            Assert.AreEqual(SharingHintKind.None, SharingHint.Next(Make(new[] { 3 }, basket: 1), Plan()).Kind);
        }

        [Test]
        public void Next_RepeatLevel_StartsWithTheFirstBoxCard()
        {
            var hint = SharingHint.Next(ShareThreeWithBox(), Repeat(1, new SharingCard[0]));

            Assert.AreEqual(SharingHintKind.Add, hint.Kind);
            Assert.IsTrue(hint.InBox);
            Assert.AreEqual(G0, hint.Card);
        }

        [Test]
        public void Next_RepeatLevel_FullBoxWithOneTurn_AsksForTwoTurns()
        {
            var hint = SharingHint.Next(ShareThreeWithBox(), Repeat(1, new[] { G0, G1, G2 }));

            Assert.AreEqual(SharingHintKind.SetTurns, hint.Kind);
            Assert.AreEqual(2, hint.Count);
        }

        [Test]
        public void Next_RepeatLevel_WrongBoxCard_IsRemoved()
        {
            var hint = SharingHint.Next(ShareThreeWithBox(), Repeat(2, new[] { G0, G0 }));

            Assert.AreEqual(SharingHintKind.Remove, hint.Kind);
            Assert.IsTrue(hint.InBox);
            Assert.AreEqual(1, hint.Index);
        }

        [Test]
        public void Next_RepeatLevel_BoxDoneButTheTableIsMissing_AddsItToTheTail()
        {
            var hint = SharingHint.Next(ShareThreeWithTable(), Repeat(2, new[] { G0, G1, G2 }));

            Assert.AreEqual(SharingHintKind.Add, hint.Kind);
            Assert.IsFalse(hint.InBox);
            Assert.IsTrue(hint.Card.IsTable);
        }

        [Test]
        public void Next_RepeatLevel_GoalMetDespiteAStoppingTailCard_IsPressPlay()
        {
            var hint = SharingHint.Next(ShareThreeWithBox(), Repeat(2, new[] { G0, G1, G2 }, GT, GT));

            Assert.AreEqual(SharingHintKind.PressPlay, hint.Kind, "the goal is met, the extra table cards only stop");
        }

        [Test]
        public void Next_RepeatLevel_RightPlan_IsPressPlay()
        {
            Assert.AreEqual(SharingHintKind.PressPlay, SharingHint.Next(ShareThreeWithTable(), Repeat(2, new[] { G2, G1, G0 }, GT)).Kind);
        }

        [Test]
        public void Next_RepeatLevelThatCannotBeWon_HasNoHint()
        {
            var level = Make(new[] { 0, 0 }, basket: 4, goal: SharingGoalKind.Share, maxCards: 1, box: new RepeatBoxSpec(1, 3));

            Assert.AreEqual(SharingHintKind.None, SharingHint.Next(level, Repeat(1, new SharingCard[0])).Kind);
        }
    }

    public class SharingPredictionTests
    {
        [Test]
        public void Choices_AreThreeDifferentCountsWithTheAnswerAmongThem()
        {
            var choices = SharingPrediction.Choices("paylasim-koyu.koy-meydani.07", 5, 3);

            Assert.AreEqual(3, choices.Length);
            CollectionAssert.AreEquivalent(new[] { 3, 2, 5 }, choices);
        }

        [Test]
        public void Choices_WhenTheCountsCollide_AreFilledWithNeighbours()
        {
            var choices = SharingPrediction.Choices("x", 4, 2);

            Assert.AreEqual(3, choices.Distinct().Count());
            Assert.Contains(2, choices);
        }

        [Test]
        public void Choices_NothingGiven_StillThreeDistinctCounts()
        {
            var choices = SharingPrediction.Choices(null, 3, 3);

            Assert.AreEqual(3, choices.Distinct().Count());
            Assert.Contains(3, choices);
        }

        [Test]
        public void Choices_OrderFollowsTheLevelIdButIsStable()
        {
            CollectionAssert.AreEqual(SharingPrediction.Choices("a", 5, 3), SharingPrediction.Choices("a", 5, 3));
            var orders = new[] { "a", "b", "c", "d", "e", "f" }.Select(id => string.Join(",", SharingPrediction.Choices(id, 5, 3))).Distinct().Count();
            Assert.Greater(orders, 1, "the right card is not always in the same place");
        }
    }
}
