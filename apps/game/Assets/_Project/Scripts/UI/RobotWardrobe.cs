using System;
using System.Collections.Generic;
using Roboya.Core;

namespace Roboya.UI
{
    /// <summary>
    /// What Roboya can wear and what is worn right now (ILR-03): the part catalog, the per-pose anchors, the part
    /// sprites and a live view of the equipped parts. Every Roboya on screen (board, story scenes, island, path,
    /// garage) draws itself through this, so a hat taken on in the garage shows everywhere.
    /// </summary>
    public sealed class RobotWardrobe
    {
        private static readonly IReadOnlyDictionary<string, string> Nothing = new Dictionary<string, string>();

        private readonly Func<IReadOnlyDictionary<string, string>> _equipped;

        public RobotWardrobe(RobotPartCatalog catalog, RobotAnchors anchors, PartArt art, Func<IReadOnlyDictionary<string, string>> equipped)
        {
            Catalog = catalog;
            Anchors = anchors ?? RobotAnchors.Empty;
            Art = art;
            _equipped = equipped;
        }

        public RobotPartCatalog Catalog { get; }

        public RobotAnchors Anchors { get; }

        public PartArt Art { get; }

        public IReadOnlyDictionary<string, string> Equipped => _equipped != null ? _equipped() ?? Nothing : Nothing;
    }
}
