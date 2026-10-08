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
        public const string OnboardingNoticeTitle = "onboarding.notice_title";
        public const string OnboardingAccept = "onboarding.accept";
        public const string OnboardingDecline = "onboarding.decline";
        public const string ProfileTitle = "profile.title";
        public const string ProfileNickname = "profile.nickname";
        public const string ProfileAvatar = "profile.avatar";
        public const string ProfileAge = "profile.age";
        public const string ProfileAgeMinik = "profile.age.minik";
        public const string ProfileAgeKasif = "profile.age.kasif";
        public const string ProfileAgeMucit = "profile.age.mucit";
        public const string ProfileSave = "profile.save";
        public const string ProfileCancel = "profile.cancel";
        public const string ProfileDefaultNickname = "profile.default_nickname";
        public const string ProfileListTitle = "profile.list_title";
        public const string ProfileAdd = "profile.add";
        public const string ProfileRemove = "profile.remove";
        public const string ProfileRemoveConfirm = "profile.remove_confirm";
        public const string ProfileLimit = "profile.limit";
        public const string ProfileActive = "profile.active";
        public const string TimeTitle = "time.title";
        public const string TimeMinutes = "time.minutes";
        public const string TimeUnlimited = "time.unlimited";
        public const string TimeToday = "time.today";
        public const string TimeNote = "time.note";
        public const string RestParent = "rest.parent";
        public const string AccountTitle = "account.title";
        public const string AccountHint = "account.hint";
        public const string AccountEmail = "account.email";
        public const string AccountSendCode = "account.send_code";
        public const string AccountCode = "account.code";
        public const string AccountSignIn = "account.sign_in";
        public const string AccountSignOut = "account.sign_out";
        public const string AccountSignedIn = "account.signed_in";
        public const string AccountCodeSent = "account.code_sent";
        public const string AccountErrorCode = "account.error.invalid_code";
        public const string AccountErrorMany = "account.error.too_many_requests";
        public const string AccountErrorDevice = "account.error.device_limit";
        public const string AccountErrorNetwork = "account.error.network";
        public const string AccountErrorGeneric = "account.error.generic";
        public const string AccountSync = "account.sync";
        public const string SyncDone = "account.sync.done";
        public const string SyncOffline = "account.sync.offline";
        public const string SyncConsent = "account.sync.consent";
        public const string SyncOutdated = "account.sync.outdated";
        public const string SyncPartly = "account.sync.partly";
        public const string SyncFailed = "account.sync.failed";
        public const string PrivacyTitle = "privacy.title";
        public const string PrivacyRead = "privacy.read";
        public const string PrivacyClose = "privacy.close";
        public const string PrivacyView = "privacy.view";
        public const string PrivacyViewSummary = "privacy.view.summary";
        public const string PrivacyViewProfiles = "privacy.view.profiles";
        public const string PrivacyExport = "privacy.export";
        public const string PrivacyExportDone = "privacy.export.done";
        public const string PrivacyWithdraw = "privacy.withdraw";
        public const string PrivacyWithdrawConfirm = "privacy.withdraw.confirm";
        public const string PrivacyWipe = "privacy.wipe";
        public const string PrivacyWipeConfirm = "privacy.wipe.confirm";
        public const string PrivacyDelete = "privacy.delete";
        public const string PrivacyDeleteConfirm = "privacy.delete.confirm";
        public const string PrivacyDeletePending = "privacy.delete.pending";
        public const string PrivacyDeleteCancel = "privacy.delete.cancel";
        public const string PrivacyDeleteCancelled = "privacy.delete.cancelled";
        public const string PrivacyNeedAccount = "privacy.need_account";
        public const string PrivacyError = "privacy.error";
        public const string SubscriptionTitle = "subscription.title";
        public const string SubscriptionStatusFree = "subscription.status.free";
        public const string SubscriptionStatusActive = "subscription.status.active";
        public const string SubscriptionStatusTrial = "subscription.status.trial";
        public const string SubscriptionStatusGrace = "subscription.status.grace";
        public const string SubscriptionMonthly = "subscription.monthly";
        public const string SubscriptionYearly = "subscription.yearly";
        public const string SubscriptionTrialNote = "subscription.trial_note";
        public const string SubscriptionRestore = "subscription.restore";
        public const string SubscriptionNeedAccount = "subscription.need_account";
        public const string SubscriptionStoreMissing = "subscription.store_missing";
        public const string SubscriptionTerms = "subscription.terms";
        public const string SubscriptionDone = "subscription.done";
        public const string SubscriptionCancelled = "subscription.cancelled";
        public const string SubscriptionPending = "subscription.pending";
        public const string SubscriptionLinked = "subscription.linked";
        public const string SubscriptionOffline = "subscription.offline";
        public const string SubscriptionFailed = "subscription.failed";
        public const string ReportTitle = "report.title";
        public const string ReportSummary = "report.summary";
        public const string ReportNote = "report.note";
        public const string ReportEmpty = "report.empty";
        public const string ReportConcept = "report.concept";
        public const string ReportNotStarted = "report.level.notstarted";
        public const string ReportExploring = "report.level.exploring";
        public const string ReportGrowing = "report.level.growing";
        public const string ReportConfident = "report.level.confident";
        public const string ReportConceptLine = "report.line";

        public static readonly string[] All =
        {
            GateQuestion, GateWrong, GateLocked, GateConfirm, GateCancel, ParentTitle, ParentEmpty, ParentBack,
            OnboardingNoticeTitle, OnboardingAccept, OnboardingDecline, ProfileTitle, ProfileNickname, ProfileAvatar,
            ProfileAge, ProfileAgeMinik, ProfileAgeKasif, ProfileAgeMucit, ProfileSave, ProfileCancel,
            ProfileDefaultNickname, ProfileListTitle, ProfileAdd, ProfileRemove, ProfileRemoveConfirm, ProfileLimit,
            ProfileActive, TimeTitle, TimeMinutes, TimeUnlimited, TimeToday, TimeNote, RestParent,
            ReportTitle, ReportSummary, ReportNote, ReportEmpty, ReportConcept, ReportNotStarted, ReportExploring, ReportGrowing, ReportConfident, ReportConceptLine,
            AccountTitle, AccountHint, AccountEmail, AccountSendCode, AccountCode, AccountSignIn, AccountSignOut, AccountSignedIn, AccountCodeSent, AccountErrorCode, AccountErrorMany, AccountErrorDevice, AccountErrorNetwork, AccountErrorGeneric,
            AccountSync, SyncDone, SyncOffline, SyncConsent, SyncOutdated, SyncPartly, SyncFailed,
            PrivacyTitle, PrivacyRead, PrivacyClose, PrivacyView, PrivacyViewSummary, PrivacyViewProfiles, PrivacyExport, PrivacyExportDone, PrivacyWithdraw, PrivacyWithdrawConfirm, PrivacyWipe, PrivacyWipeConfirm, PrivacyDelete, PrivacyDeleteConfirm, PrivacyDeletePending, PrivacyDeleteCancel, PrivacyDeleteCancelled, PrivacyNeedAccount, PrivacyError,
            SubscriptionTitle, SubscriptionStatusFree, SubscriptionStatusActive, SubscriptionStatusTrial, SubscriptionStatusGrace, SubscriptionMonthly, SubscriptionYearly, SubscriptionTrialNote, SubscriptionRestore, SubscriptionNeedAccount, SubscriptionStoreMissing, SubscriptionTerms, SubscriptionDone, SubscriptionCancelled, SubscriptionPending, SubscriptionLinked, SubscriptionOffline, SubscriptionFailed,
        };
    }
}
