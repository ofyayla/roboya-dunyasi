using System;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.Core;
using Roboya.UI;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// Switches between the island, the Patience Forest path and the ship workshop. No text on screen: every view speaks
    /// through short narration lines from content/voice/script.csv.
    /// </summary>
    public sealed class MapController : IDisposable
    {
        public const string WelcomeVoice = "map.welcome";
        public const string RegionLockedVoice = "map.region_locked";
        public const string LockedWaitVoice = "roboya.locked_wait";
        public const string AskGrownUpVoice = "roboya.ask_grownup";
        public const string WorkshopVoice = "workshop.welcome";

        private readonly GameServices _services;
        private readonly IslandView _island;
        private readonly PathView _path;
        private readonly WorkshopView _workshop;

        public MapController(VisualElement root, GameServices services, RegionArt art, PartArt partArt)
        {
            _services = services;
            var host = root.Q("map-root") ?? root;
            _island = new IslandView(services, art, partArt, OnRegion, () => services.Voice.Play(RegionLockedVoice));
            _path = new PathView(services, art, partArt, ShowIsland, ShowWorkshop);
            _workshop = new WorkshopView(services, art, partArt, ShowPath);
            host.Add(_island);
            host.Add(_path);
            host.Add(_workshop);
        }

        public void Open()
        {
            var nav = _services.Navigator;
            // A repair part earned but not yet seen: open on the island so it drops onto the ship (ILR-03).
            bool newPart = ProgressQueries.EarnedParts(_services) > _services.Progress.Book.ShipPartsSeen;
            if (!newPart && (nav.SelectedLevelId != null || nav.PendingMapLine != null))
            {
                // Back from a level: the child continues on the path, not the island.
                ShowPath();
                if (nav.PendingMapLine != null)
                {
                    _services.Voice.Play(nav.PendingMapLine);
                    nav.ConsumeMapLine();
                }

                return;
            }

            ShowIsland();
            if (nav.PendingMapLine != null)
            {
                _services.Voice.Play(nav.PendingMapLine);
                nav.ConsumeMapLine();
            }
        }

        public void Dispose()
        {
            _services.Voice.Stop();
        }

        /// <summary>Wire id of a region, as used in content files.</summary>
        public static string WireId(RegionId region)
        {
            switch (region)
            {
                case RegionId.SabirOrmani: return "sabir-ormani";
                case RegionId.PaylasimKoyu: return "paylasim-koyu";
                case RegionId.YardimlasmaLimani: return "yardimlasma-limani";
                case RegionId.NezaketBahcesi: return "nezaket-bahcesi";
                case RegionId.SorumlulukKulesi: return "sorumluluk-kulesi";
                default: return "mucit-atolyesi";
            }
        }

        private void OnRegion(string regionId)
        {
            // Only the Patience Forest has levels in the MVP; other regions open with their content (v1.0).
            ShowPath();
            _services.Voice.Play(WelcomeVoice);
        }

        private void ShowIsland() => Show(_island);

        private void ShowPath() => Show(_path);

        private void ShowWorkshop()
        {
            Show(_workshop);
            _services.Voice.Play(WorkshopVoice);
        }

        private void Show(VisualElement view)
        {
            foreach (var v in new VisualElement[] { _island, _path, _workshop })
            {
                v.style.display = v == view ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (view is IRefreshable r)
            {
                r.Refresh();
            }
        }
    }

    internal interface IRefreshable
    {
        void Refresh();
    }
}
