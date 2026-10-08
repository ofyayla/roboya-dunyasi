using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Roboya.Core
{
    /// <summary>
    /// Text for adult screens (parent area, gate), looked up by key from content/localization/tr.json (CLAUDE.md
    /// rule 7: no Turkish text in code). Child screens have no text; their narration is in content/voice.
    /// A missing key shows "[key]" and logs once, so a gap is visible in testing instead of an empty label.
    /// </summary>
    public sealed class LocalizedStrings
    {
        public const string File = "localization/tr.json";

        private readonly Dictionary<string, string> _table;
        private readonly HashSet<string> _reported = new HashSet<string>();

        public LocalizedStrings(Dictionary<string, string> table)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public static LocalizedStrings Parse(string json)
        {
            var table = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? throw new FormatException("Empty string table.");
            return new LocalizedStrings(table);
        }

        public bool Has(string key) => key != null && _table.ContainsKey(key);

        public string Get(string key)
        {
            if (key != null && _table.TryGetValue(key, out var text))
            {
                return text;
            }

            if (_reported.Add(key ?? string.Empty))
            {
                Debug.LogWarning("Missing string key: " + key);
            }

            return "[" + key + "]";
        }

        public string Format(string key, params object[] args) => string.Format(Get(key), args);
    }

    /// <summary>The keys adult screens use; a test checks that every one exists in the shipped table.</summary>
    public static class StringKeys
    {
        public const string GateQuestion = "gate.question";
        public const string GateWrong = "gate.wrong";
        public const string GateLocked = "gate.locked";
        public const string GateConfirm = "gate.confirm";
        public const string GateCancel = "gate.cancel";
        public const string ParentTitle = "parent.title";
        public const string ParentEmpty = "parent.empty";
        public const string ParentBack = "parent.back";

        public static readonly string[] All =
        {
            GateQuestion, GateWrong, GateLocked, GateConfirm, GateCancel, ParentTitle, ParentEmpty, ParentBack,
        };
    }
}
