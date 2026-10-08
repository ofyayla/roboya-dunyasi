using Roboya.CodingEngine.Progress;

namespace Roboya.Core
{
    /// <summary>On-device progress for the active child profile (ILR-01; ADR 0009).</summary>
    public interface IProgressStore
    {
        /// <summary>The loaded book; mutate it, then call <see cref="Save"/>.</summary>
        ProgressBook Book { get; }

        void Save();
    }
}
