namespace Roboya.Core
{
    /// <summary>Implemented by one component per scene; Bootstrap hands it the services after loading.</summary>
    public interface ISceneEntry
    {
        void Enter(GameServices services);
    }
}
