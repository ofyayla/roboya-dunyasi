using System.Collections.Generic;
using NUnit.Framework;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.Levels;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.World;

namespace Roboya.Tests.CodingEngine
{
    public class LevelLoaderTests
    {
        private const string Valid = @"{
  ""schemaVersion"": 2,
  ""id"": ""sabir-ormani.yon-avcisi.99"",
  ""region"": ""sabir-ormani"",
  ""game"": ""yon-avcisi"",
  ""order"": 99,
  ""meta"": { ""concepts"": [""direction""], ""value"": ""patience"", ""difficulty"": 1, ""ageLevels"": [""minik""] },
  ""grid"": { ""rows"": [""...."", "".#.."", ""....""] },
  ""robot"": { ""x"": 0, ""y"": 2, ""facing"": ""north"" },
  ""items"": [
    { ""id"": ""elma"", ""kind"": ""fruit"", ""color"": ""red"", ""x"": 0, ""y"": 0 },
    { ""id"": ""cark"", ""kind"": ""ship-part"", ""x"": 3, ""y"": 0 }
  ],
  ""goal"": { ""reach"": { ""x"": 3, ""y"": 0 }, ""collect"": [""elma"", ""cark""] },
  ""cards"": { ""palette"": [""forward"", ""turn_right"", ""repeat""], ""maxProgramLength"": 8, ""introduces"": ""repeat"" },
  ""starterProgram"": [
    { ""op"": ""repeat"", ""times"": 2, ""body"": [{ ""op"": ""forward"" }] },
    { ""op"": ""turn_right"" },
    { ""op"": ""if"", ""condition"": { ""type"": ""not"", ""inner"": { ""type"": ""path_blocked"" } }, ""then"": [{ ""op"": ""forward"" }], ""else"": [] },
    { ""op"": ""call"", ""name"": ""iki"" },
    { ""op"": ""action"", ""id"": ""zipla"" }
  ],
  ""voice"": { ""intro"": ""yon_avcisi.l99.intro"" },
  ""story"": { ""intro"": { ""roboya"": ""surprised"", ""friend"": ""explaining"", ""props"": [""log"", ""apple""] } },
  ""solution"": { ""shortestLength"": 5 }
}";

        [Test]
        public void Load_ValidJson_BuildsLevel()
        {
            var level = LevelLoader.Load(Valid);

            Assert.AreEqual("sabir-ormani.yon-avcisi.99", level.Id);
            Assert.AreEqual(4, level.Grid.Width);
            Assert.AreEqual(CellType.Blocked, level.Grid[new GridPosition(1, 1)]);
            Assert.AreEqual(new GridPosition(0, 2), level.Start.Position);
            Assert.AreEqual(Direction.North, level.Start.Facing);
            Assert.AreEqual(2, level.Items.Count);
            Assert.AreEqual("red", level.Items[0].Color);
            Assert.AreEqual("fruit", level.Items[0].Kind);
            Assert.AreEqual("ship-part", level.Items[1].Kind);
            Assert.IsNull(level.Items[1].Color);
            Assert.AreEqual(new GridPosition(3, 0), level.Goal.Reach.Value);
            CollectionAssert.AreEqual(new[] { 0, 1 }, level.Goal.MustCollect);
            Assert.IsTrue(level.AvailableCards.Contains(CardType.Repeat));
            Assert.AreEqual(8, level.MaxProgramLength);
        }

        [Test]
        public void ToProgram_StarterProgram_MapsEveryCommandKind()
        {
            var dto = LevelLoader.Parse(Valid);

            var program = LevelLoader.ToProgram(dto.StarterProgram);

            Assert.IsInstanceOf<RepeatCommand>(program.Main[0]);
            Assert.AreEqual(2, ((RepeatCommand)program.Main[0]).Times);
            Assert.AreSame(MoveCommand.TurnRight, program.Main[1]);
            var branch = (IfCommand)program.Main[2];
            Assert.IsInstanceOf<Not>(branch.Condition);
            Assert.IsInstanceOf<PathBlockedAhead>(((Not)branch.Condition).Inner);
            Assert.AreEqual("iki", ((CallCommand)program.Main[3]).Procedure);
            Assert.AreEqual("zipla", ((ActionCommand)program.Main[4]).ActionId);
        }

        [Test]
        public void Load_ThenSolveByHand_Succeeds()
        {
            var level = LevelLoader.Load(Valid);
            var program = new Program(new Command[]
            {
                MoveCommand.Forward, MoveCommand.Forward, MoveCommand.TurnRight,
                MoveCommand.Forward, MoveCommand.Forward, MoveCommand.Forward,
            });

            Assert.AreEqual(ExecutionOutcome.Success, Interpreter.Execute(level, program).Outcome);
        }

        [Test]
        public void Parse_MalformedJson_ThrowsValidation()
        {
            Assert.Throws<LevelValidationException>(() => LevelLoader.Parse("{ not json"));
        }

        [Test]
        public void Parse_NullLiteral_ThrowsValidation()
        {
            Assert.Throws<LevelValidationException>(() => LevelLoader.Parse("null"));
        }

        [Test]
        public void Parse_UnsupportedVersion_ThrowsValidation()
        {
            Assert.Throws<LevelValidationException>(() => LevelLoader.Parse(Valid.Replace("\"schemaVersion\": 2", "\"schemaVersion\": 3")));
        }

        [Test]
        public void Parse_Version1WithoutStory_StillLoads()
        {
            var json = Valid.Replace("\"schemaVersion\": 2", "\"schemaVersion\": 1");

            var dto = LevelLoader.Parse(json);

            Assert.AreEqual(1, dto.SchemaVersion);
        }

        [Test]
        public void Parse_StoryBlock_ReadsPosesAndProps()
        {
            var dto = LevelLoader.Parse(Valid);

            Assert.AreEqual(RobotPose.Surprised, dto.Story.Intro.Roboya);
            Assert.AreEqual(FriendPose.Explaining, dto.Story.Intro.Friend);
            CollectionAssert.AreEqual(new[] { StoryProp.Log, StoryProp.Apple }, dto.Story.Intro.Props);
            Assert.IsNull(dto.Story.Outro);
        }

        [Test]
        public void ToLevel_UnknownCollectItem_ThrowsValidation()
        {
            var json = Valid.Replace("[\"elma\", \"cark\"]", "[\"armut\"]");

            Assert.Throws<LevelValidationException>(() => LevelLoader.Load(json));
        }

        [Test]
        public void ToLevel_RaggedGrid_ThrowsValidation()
        {
            Assert.Throws<LevelValidationException>(() => LevelLoader.Load(Valid.Replace("\".#..\"", "\".#.\"")));
        }

        [Test]
        public void ToLevel_MissingSections_ThrowsValidation()
        {
            var dto = LevelLoader.Parse(Valid);
            dto.Robot = null;

            Assert.Throws<LevelValidationException>(() => LevelLoader.ToLevel(dto));
            Assert.Throws<System.ArgumentNullException>(() => LevelLoader.ToLevel(null));
        }

        [Test]
        public void ToProgram_NullBlockOrIfWithoutCondition_Handled()
        {
            Assert.AreEqual(0, LevelLoader.ToProgram(null).Main.Count);
            var bad = new List<CommandDto> { new CommandDto { Op = CardId.If, Then = new List<CommandDto>() } };

            Assert.Throws<LevelValidationException>(() => LevelLoader.ToProgram(bad));
        }

        [TestCase(Facing.North, Direction.North)]
        [TestCase(Facing.East, Direction.East)]
        [TestCase(Facing.South, Direction.South)]
        [TestCase(Facing.West, Direction.West)]
        public void ToDirection_AllFacings_Map(Facing facing, Direction expected)
        {
            Assert.AreEqual(expected, LevelLoader.ToDirection(facing));
        }

        [TestCase(CardId.Forward, CardType.Forward)]
        [TestCase(CardId.Backward, CardType.Backward)]
        [TestCase(CardId.TurnLeft, CardType.TurnLeft)]
        [TestCase(CardId.TurnRight, CardType.TurnRight)]
        [TestCase(CardId.Repeat, CardType.Repeat)]
        [TestCase(CardId.If, CardType.If)]
        [TestCase(CardId.Call, CardType.Call)]
        [TestCase(CardId.Action, CardType.Action)]
        public void ToCard_AllCards_Map(CardId card, CardType expected)
        {
            Assert.AreEqual(expected, LevelLoader.ToCard(card));
        }

        [TestCase("flower", "blue")]
        [TestCase("honey", "yellow")]
        [TestCase("gear", "green")]
        [TestCase("star", "purple")]
        [TestCase("fruit", "orange")]
        public void Load_ItemKindsAndColors_RoundTrip(string kind, string color)
        {
            var json = Valid.Replace("\"kind\": \"fruit\", \"color\": \"red\"", "\"kind\": \"" + kind + "\", \"color\": \"" + color + "\"");

            var item = LevelLoader.Load(json).Items[0];

            Assert.AreEqual(kind, item.Kind);
            Assert.AreEqual(color, item.Color);
        }

        [Test]
        public void Load_OnColorCondition_Maps()
        {
            var json = Valid.Replace("{ \"type\": \"not\", \"inner\": { \"type\": \"path_blocked\" } }", "{ \"type\": \"on_color\", \"color\": \"red\" }");

            var program = LevelLoader.ToProgram(LevelLoader.Parse(json).StarterProgram);

            Assert.AreEqual("red", ((OnItemColor)((IfCommand)program.Main[2]).Condition).Color);
        }

        [Test]
        public void Load_GoalWithoutReach_HasNoReach()
        {
            var json = Valid.Replace("\"reach\": { \"x\": 3, \"y\": 0 }, ", string.Empty);

            Assert.IsFalse(LevelLoader.Load(json).Goal.Reach.HasValue);
        }
    }
}
