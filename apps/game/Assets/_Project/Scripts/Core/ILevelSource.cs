using System.Collections.Generic;
using UnityEngine;

namespace Roboya.Core
{
    /// <summary>Supplies raw level JSON. Region packs move to Addressables + CDN in Faz 1 (ADR 0006).</summary>
    public interface ILevelSource
    {
        Awaitable<IReadOnlyList<string>> LoadAllAsync();
    }
}
