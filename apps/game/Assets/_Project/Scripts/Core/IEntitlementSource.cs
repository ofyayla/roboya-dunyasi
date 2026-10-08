namespace Roboya.Core
{
    /// <summary>
    /// Premium access as decided by the server (CLAUDE.md golden rule 3). The client only caches and reads it;
    /// it never computes or grants premium on its own.
    /// </summary>
    public interface IEntitlementSource
    {
        bool HasPremium { get; }
    }

    /// <summary>Until the entitlement API exists (F1-17) every profile is on the free tier.</summary>
    public sealed class FreeTierEntitlements : IEntitlementSource
    {
        public bool HasPremium => false;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only switch for testing paid levels (ROBOYA_DEV_PREMIUM=1). Compiled out of every player build,
    /// so it can never grant premium on a device (ADR 0009).
    /// </summary>
    public sealed class DevEntitlements : IEntitlementSource
    {
        public const string Variable = "ROBOYA_DEV_PREMIUM";

        public static bool Requested => System.Environment.GetEnvironmentVariable(Variable) == "1";

        public bool HasPremium => true;
    }
#endif
}
