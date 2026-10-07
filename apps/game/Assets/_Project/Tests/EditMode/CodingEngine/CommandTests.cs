using System;
using System.Collections.Generic;
using NUnit.Framework;
using Roboya.CodingEngine.Commands;
using static Roboya.Tests.CodingEngine.TestLevels;

namespace Roboya.Tests.CodingEngine
{
    public class CommandTests
    {
        [Test]
        public void CardCount_NestedCommands_CountsEveryCard()
        {
            var repeat = new RepeatCommand(3, new Command[] { MoveCommand.Forward, MoveCommand.TurnLeft });
            var branch = new IfCommand(PathBlockedAhead.Instance, new Command[] { MoveCommand.TurnRight }, new Command[] { repeat });

            Assert.AreEqual(3, repeat.CardCount);
            Assert.AreEqual(5, branch.CardCount);
            Assert.AreEqual(1, new CallCommand("dans").CardCount);
            Assert.AreEqual(1, new ActionCommand("zipla").CardCount);
        }

        [Test]
        public void ProgramCardCount_WithProcedures_CountsProceduresOnce()
        {
            var procedures = new Dictionary<string, IReadOnlyList<Command>>
            {
                ["kare"] = new Command[] { MoveCommand.Forward, MoveCommand.TurnRight },
            };
            var program = new Program(new Command[] { new CallCommand("kare"), new CallCommand("kare") }, procedures);

            Assert.AreEqual(4, program.CardCount);
            Assert.AreEqual(3, Prog(F, F, L).CardCount);
        }

        [Test]
        public void Repeat_TimesOutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RepeatCommand(0, new Command[0]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RepeatCommand(RepeatCommand.MaxTimes + 1, new Command[0]));
        }

        [Test]
        public void Repeat_NullBodyOrNullCommand_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new RepeatCommand(2, null));
            Assert.Throws<ArgumentException>(() => new RepeatCommand(2, new Command[] { null }));
        }

        [Test]
        public void If_NullCondition_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new IfCommand(null, new Command[0], null));
        }

        [Test]
        public void If_NullElse_IsEmpty()
        {
            Assert.AreEqual(0, new IfCommand(PathBlockedAhead.Instance, new Command[0], null).Else.Count);
        }

        [Test]
        public void CallAndAction_EmptyName_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CallCommand(""));
            Assert.Throws<ArgumentException>(() => new ActionCommand(null));
        }

        [Test]
        public void MoveFor_NonMoveCard_Throws()
        {
            Assert.Throws<ArgumentException>(() => MoveCommand.For(CardType.Repeat));
            Assert.AreSame(MoveCommand.Backward, MoveCommand.For(CardType.Backward));
        }

        [Test]
        public void IsPrimitiveMove_Cards_ClassifiesCorrectly()
        {
            Assert.IsTrue(CardType.Backward.IsPrimitiveMove());
            Assert.IsFalse(CardType.Repeat.IsPrimitiveMove());
            Assert.IsFalse(CardType.Action.IsPrimitiveMove());
        }

        [Test]
        public void Program_NullMain_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new Program(null));
        }
    }
}
