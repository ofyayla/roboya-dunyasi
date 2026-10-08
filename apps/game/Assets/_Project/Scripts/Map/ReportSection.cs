using System.Collections.Generic;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Skills;
using Roboya.Core;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>Progress summary and skill report for the active child (F1-12): plain sentences, no scores, no diagnosis.</summary>
    public sealed class ReportSection : VisualElement, IRefreshable
    {
        private readonly GameServices _services;
        private readonly Label _summary = new Label { name = "report-summary" };
        private readonly VisualElement _lines = new VisualElement { name = "report-lines" };
        private readonly Label _note;

        public ReportSection(GameServices services)
        {
            _services = services;
            name = "report-section";
            AddToClassList("section");
            var title = new Label(services.Strings.Get(StringKeys.ReportTitle));
            title.AddToClassList("section__title");
            Add(title);
            _summary.AddToClassList("profile-row__band");
            Add(_summary);
            _lines.AddToClassList("report__lines");
            Add(_lines);
            _note = new Label(services.Strings.Get(StringKeys.ReportNote)) { name = "report-note" };
            _note.style.color = new UnityEngine.Color(0.29f, 0.2f, 0.14f);
            _note.AddToClassList("section__note");
            Add(_note);
            services.Profiles.Changed += Refresh;
            Refresh();
        }

        public void Refresh()
        {
            bool active = _services.Profiles.HasActive;
            style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
            _lines.Clear();
            if (!active)
            {
                return;
            }

            var facts = new List<LevelFacts>();
            foreach (var entry in _services.Catalog.All)
            {
                facts.Add(LevelFacts.From(entry.Dto));
            }

            var report = ProgressReport.Build(facts, _services.Progress.Book);
            var s = _services.Strings;
            if (report.CompletedLevels == 0)
            {
                _summary.text = s.Get(StringKeys.ReportEmpty);
                return;
            }

            _summary.text = s.Format(
                StringKeys.ReportSummary, _services.Profiles.Active.Nickname, report.TotalLevels, report.CompletedLevels, report.Stars);
            foreach (var concept in report.Concepts)
            {
                string name = s.Get("report.concept." + concept.Concept.ToString().ToLowerInvariant());
                var line = new Label(s.Format(StringKeys.ReportConceptLine, name, s.Get(LevelKey(concept.Level))))
                {
                    name = "report-" + concept.Concept.ToString().ToLowerInvariant(),
                };
                line.AddToClassList("report__line");
                _lines.Add(line);
            }
        }

        private static string LevelKey(SkillLevel level)
        {
            switch (level)
            {
                case SkillLevel.Confident: return StringKeys.ReportConfident;
                case SkillLevel.Growing: return StringKeys.ReportGrowing;
                case SkillLevel.Exploring: return StringKeys.ReportExploring;
                default: return StringKeys.ReportNotStarted;
            }
        }
    }
}
