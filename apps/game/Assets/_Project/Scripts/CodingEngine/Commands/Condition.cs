using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Commands
{
    /// <summary>A sensor question used by <see cref="IfCommand"/>. Pure: reads the level and state only.</summary>
    public abstract class Condition
    {
        public abstract bool Evaluate(Level level, RobotState state);
    }

    /// <summary>"Önümde engel var mı?" — true when the cell ahead is blocked or outside the grid.</summary>
    public sealed class PathBlockedAhead : Condition
    {
        public static readonly PathBlockedAhead Instance = new PathBlockedAhead();

        private PathBlockedAhead()
        {
        }

        public override bool Evaluate(Level level, RobotState state) =>
            !level.Grid.IsWalkable(state.Position.Step(state.Facing));
    }

    /// <summary>"Bu hücrede {renk} bir nesne var mı?" — true when the current cell holds an item of the colour.</summary>
    public sealed class OnItemColor : Condition
    {
        public OnItemColor(string color)
        {
            Color = color;
        }

        public string Color { get; }

        public override bool Evaluate(Level level, RobotState state)
        {
            int index = level.ItemIndexAt(state.Position);
            return index >= 0 && level.Items[index].Color == Color;
        }
    }

    public sealed class Not : Condition
    {
        public Not(Condition inner)
        {
            Inner = inner;
        }

        public Condition Inner { get; }

        public override bool Evaluate(Level level, RobotState state) => !Inner.Evaluate(level, state);
    }
}
