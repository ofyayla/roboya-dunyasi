using System;
using System.Collections.Generic;

namespace Roboya.CodingEngine.Commands
{
    /// <summary>A child's program: the main card strip plus optional named procedures (function cards).</summary>
    public sealed class Program
    {
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<Command>> NoProcedures =
            new Dictionary<string, IReadOnlyList<Command>>();

        public Program(IReadOnlyList<Command> main, IReadOnlyDictionary<string, IReadOnlyList<Command>> procedures = null)
        {
            Main = main ?? throw new ArgumentNullException(nameof(main));
            Procedures = procedures ?? NoProcedures;
        }

        public static Program Of(params Command[] commands) => new Program(commands);

        public IReadOnlyList<Command> Main { get; }

        public IReadOnlyDictionary<string, IReadOnlyList<Command>> Procedures { get; }

        /// <summary>Total cards placed by the child, procedures counted once.</summary>
        public int CardCount
        {
            get
            {
                int n = 0;
                foreach (var c in Main)
                {
                    n += c.CardCount;
                }

                foreach (var body in Procedures.Values)
                {
                    foreach (var c in body)
                    {
                        n += c.CardCount;
                    }
                }

                return n;
            }
        }
    }
}
