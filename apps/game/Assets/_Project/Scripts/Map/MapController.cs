using System;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Profiles;
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
        public const string OnboardingVoice = "onboarding.welcome";

        private readonly GameServices _services;
        private readonly IslandView _island;
        private readonly PathView _path;
        private readonly WorkshopView _workshop;
        private readonly ParentView _parent;
        private readonly ParentGateView _gate;
        private readonly IconButton _grownup;
        private readonly WelcomeView _welcome;
        private readonly NoticeView _notice;
        private readonly ProfileEditorView _editor;

        public MapController(VisualElement root, GameServices services, RegionArt art, PartArt partArt)
        {
            _services = services;
            var host = root.Q("map-root") ?? root;
            _island = new IslandView(services, art, partArt, OnRegion, () => services.Voice.Play(RegionLockedVoice));
            _path = new PathView(services, art, partArt, ShowIsland, ShowWorkshop);
            _workshop = new WorkshopView(services, art, partArt, ShowPath);
            _parent = new ParentView(services.Strings, ShowIsland);
            _gate = new ParentGateView(services.Strings);
            _welcome = new WelcomeView(art, () => _gate.Open(AfterGate));
            _notice = new NoticeView(services, OpenEditorForNew, ShowWelcome);
            _editor = new ProfileEditorView(services.Strings, SaveProfile, OnEditorCancelled);
            _parent.AddSection(new ProfilesSection(services, profile => _editor.Open(profile), OpenEditorForNew));
            _grownup = new IconButton(IconKind.Grownup, () => _gate.Open(ShowParent)) { name = "to-parent" };
            _grownup.AddToClassList("map__grownup");
            host.Add(_island);
            host.Add(_path);
            host.Add(_workshop);
            host.Add(_parent);
            host.Add(_grownup);
            host.Add(_welcome);
            host.Add(_gate);
            host.Add(_notice);
            host.Add(_editor);
        }

        public void Open()
        {
            // First run, or the notice changed, or every profile was removed: the parent comes first (A1).
            if (NeedsOnboarding)
            {
                ShowWelcome();
                return;
            }

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

        private bool NeedsOnboarding =>
            !_services.Profiles.Registry.HasConsentFor(_services.Notice.Version) || !_services.Profiles.HasActive;

        private void ShowWelcome()
        {
            Show(_welcome);
            _services.Voice.Play(OnboardingVoice);
        }

        /// <summary>After the parental gate: the notice first when consent is missing, then the profile.</summary>
        private void AfterGate()
        {
            if (!_services.Profiles.Registry.HasConsentFor(_services.Notice.Version))
            {
                _notice.Open();
            }
            else
            {
                OpenEditorForNew();
            }
        }

        private void OpenEditorForNew()
        {
            var limit = _services.ProgressRules.ProfileLimit(_services.Entitlements.HasPremium);
            if (_services.Profiles.Registry.Profiles.Count < limit)
            {
                _editor.Open();
            }
        }

        private void SaveProfile(string nickname, string avatarId, AgeBand band)
        {
            if (_editor.Editing == null)
            {
                var profile = _services.Profiles.Add(nickname, avatarId, band);
                _services.Profiles.SetActive(profile.Id);
            }
            else
            {
                _services.Profiles.Update(_editor.Editing, nickname, avatarId, band);
            }

            if (_welcome.style.display == DisplayStyle.Flex || NeedsOnboarding)
            {
                ShowIsland();
            }
        }

        private void OnEditorCancelled()
        {
            if (NeedsOnboarding)
            {
                ShowWelcome();
            }
        }

        private void ShowIsland() => Show(_island);

        private void ShowParent() => Show(_parent);

        private void ShowPath() => Show(_path);

        private void ShowWorkshop()
        {
            Show(_workshop);
            _services.Voice.Play(WorkshopVoice);
        }

        private void Show(VisualElement view)
        {
            foreach (var v in new VisualElement[] { _island, _path, _workshop, _parent, _welcome })
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
