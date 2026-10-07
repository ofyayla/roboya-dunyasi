using System;

namespace Roboya.CodingEngine.World
{
    /// <summary>Thrown when level data is structurally valid JSON but describes an impossible level.</summary>
    public sealed class LevelValidationException : Exception
    {
        public LevelValidationException(string message)
            : base(message)
        {
        }
    }
}
