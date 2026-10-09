using System;
using System.IO;
using Roboya.CodingEngine.Parents;
using UnityEngine;

namespace Roboya.Core
{
    /// <summary>
    /// Today's play time and the daily limit of the active child (F1-11, VEL-02). Time counts only while a level is on
    /// screen. When the limit is used up a level in progress may be finished; a new one cannot start (the Roboya
    /// "charge is empty" moment). The file holds parent settings and minutes, no personal data.
    /// </summary>
    public sealed class ScreenTimeService
    {
        public const string File = "screentime.json";
        private const float SaveEverySeconds = 15f;
        private const float MaxStep = 1f;

        private readonly string _path;
        private readonly ProfileManager _profiles;
        private readonly Func<DateTime> _now;
        private float _unsaved;

        public ScreenTimeService(string folder, ProfileManager profiles, Func<DateTime> now = null)
        {
            _path = Path.Combine(folder, File);
            _profiles = profiles;
            _now = now ?? (() => DateTime.Now);
            Book = Load(_path);
            Book.Retain(IdsOf(profiles));
            profiles.Changed += () =>
            {
                Book.Retain(IdsOf(profiles));
                Flush();
            };
        }

        public ScreenTimeBook Book { get; }

        public bool IsExhausted => _profiles.HasActive && Book.IsExhausted(_profiles.Active.Id, _profiles.Active.AgeBand, _now().Date);

        public int LimitMinutes => _profiles.HasActive ? Book.LimitMinutes(_profiles.Active.Id, _profiles.Active.AgeBand) : 0;

        public int UsedMinutesToday => _profiles.HasActive ? (int)(Book.UsedSeconds(_profiles.Active.Id, _now().Date) / 60.0) : 0;

        /// <summary>The plan shadow on the board (a parent setting, per child).</summary>
        public bool PlanTraceEnabled => _profiles.HasActive && Book.PlanTraceEnabled(_profiles.Active.Id, _profiles.Active.AgeBand);

        public void SetPlanTrace(bool enabled)
        {
            if (!_profiles.HasActive)
            {
                return;
            }

            Book.SetPlanTrace(_profiles.Active.Id, enabled);
            Flush();
        }

        public void SetLimit(int minutes)
        {
            if (!_profiles.HasActive)
            {
                return;
            }

            Book.SetLimit(_profiles.Active.Id, minutes);
            Flush();
        }

        /// <summary>Counts <paramref name="deltaSeconds"/> of play for the active child; call every frame a level is shown.</summary>
        public void Tick(float deltaSeconds)
        {
            if (!_profiles.HasActive)
            {
                return;
            }

            // A long frame (app resumed from the background) must not count as play time.
            float step = Mathf.Clamp(deltaSeconds, 0f, MaxStep);
            Book.AddUsage(_profiles.Active.Id, _now().Date, step);
            _unsaved += step;
            if (_unsaved >= SaveEverySeconds)
            {
                Flush();
            }
        }

        public void Flush()
        {
            _unsaved = 0f;
            string temp = _path + ".tmp";
            System.IO.File.WriteAllText(temp, Book.ToJson());
            if (System.IO.File.Exists(_path))
            {
                System.IO.File.Replace(temp, _path, null);
            }
            else
            {
                System.IO.File.Move(temp, _path);
            }
        }

        private static ScreenTimeBook Load(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return new ScreenTimeBook();
            }

            try
            {
                return ScreenTimeBook.FromJson(System.IO.File.ReadAllText(path));
            }
            catch (FormatException e)
            {
                string aside = path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                System.IO.File.Move(path, aside);
                Debug.LogWarning("Screen time file unreadable, starting fresh: " + e.Message);
                return new ScreenTimeBook();
            }
        }

        private static System.Collections.Generic.IEnumerable<string> IdsOf(ProfileManager profiles)
        {
            foreach (var p in profiles.Registry.Profiles)
            {
                yield return p.Id;
            }
        }
    }
}
