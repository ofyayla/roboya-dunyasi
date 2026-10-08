using System;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.Core;
using Roboya.UI;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// Switches between the island, the Patience Forest path and the garage. No text on screen: every view speaks
    /// through short narration lines from content/voice/script.csv.
    /// </summary>
    public sealed class MapController : IDisposable
    {
        public const string WelcomeVoice = "map.welcome";
        public const string RegionLockedVoice = "map.region_locked";
        public const string LockedWaitVoice = "roboya.locked_wait";
        public const string AskGrownUpVoice = "roboya.ask_grownup";
        public const string GarageVoice = "garage.welcome";

        private readonly GameServices _services;
        private readonly IslandView _island;
        private readonly PathView _path;
        private readonly GarageView _garage;
        private readonly ParentView _parent;
        private readonly ParentGateView _gate;
        private readonly IconButton _grownup;

        public MapController(VisualElement root, GameServices services, RegionArt art, PartArt partArt)
        {
            _services = services;
            var host = root.Q("map-root") ?? root;
            var wardrobe = new RobotWardrobe(services.Parts, services.Anchors, partArt, () => services.Progress.Book.AllEquipped);
            _island = new IslandView(services, art, partArt, wardrobe, OnRegion, () => services.Voice.Play(RegionLockedVoice));
            _path = new PathView(services, art, wardrobe, ShowIsland, ShowGarage);
            _garage = new GarageView(services, art, partArt, wardrobe, ShowPath);
            _parent = new ParentView(services.Strings, ShowIsland);
            _gate = new ParentGateView(services.Strings);
            _grownup = new IconButton(IconKind.Grownup, () => _gate.Open(ShowParent)) { name = "to-parent" };
            _grownup.AddToClassList("map__grownup");
            host.Add(_island);
            host.Add(_path);
            host.Add(_garage);
            host.Add(_parent);
            host.Add(_grownup);
            host.Add(_gate);
        }

        public void Open()
        {
            var nav = _services.Navigator;
            if (nav.SelectedLevelId != null || nav.PendingMapLine != null)
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

        private void ShowParent() => Show(_parent);

        private void ShowPath() => Show(_path);

        private void ShowGarage()
        {
            Show(_garage);
            _services.Voice.Play(GarageVoice);
        }

        private void Show(VisualElement view)
        {
            foreach (var v in new VisualElement[] { _island, _path, _garage, _parent })
            {
                v.style.display = v == view ? DisplayStyle.Flex : DisplayStyle.None;
            }

            // The grown-up entrance lives on the island only; it opens the gate, never the parent area directly.
            _grownup.style.display = view == _island ? DisplayStyle.Flex : DisplayStyle.None;

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
