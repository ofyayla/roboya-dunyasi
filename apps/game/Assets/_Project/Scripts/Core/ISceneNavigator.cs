namespace Roboya.Core
{
    /// <summary>Scene changes requested by screens; implemented by the composition root.</summary>
    public interface ISceneNavigator
    {
        /// <summary>The level the Game scene should open (null: first playable).</summary>
        string SelectedLevelId { get; }

        /// <summary>A short voice line the map plays on arrival (e.g. GLR-01 "ask a grown-up"), consumed once.</summary>
        string PendingMapLine { get; }

        void ConsumeMapLine();

        void PlayLevel(string levelId);

        /// <summary>True when the map should open on the ship workshop (the child tapped a freshly earned part), consumed once.</summary>
        bool PendingWorkshop { get; }

        void ConsumeWorkshop();

        void GoToMap(string lineOnArrival = null);

        /// <summary>Opens the map on the ship workshop so the earned part is fitted on the ship (ILR-03).</summary>
        void GoToWorkshop();
    }
}
