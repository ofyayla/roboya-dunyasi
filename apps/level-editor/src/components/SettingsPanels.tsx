import type { AgeLevel, CardId, Concept, LevelDto, ValueId } from "@roboya/level-schema";
import { useTranslation } from "react-i18next";
import {
  MAX_CARRYING,
  MAX_NEED,
  applyVillagePreset,
  honeyGoal,
  resize,
  setCarryAtEnd,
  setCarrying,
  setDeliver,
  setOption,
  setShare,
  size,
  togglePaletteCard,
  type HoneyGoalKind,
  type OptionKey,
} from "../model/levelOps";

const CARDS: CardId[] = ["forward", "backward", "turn_left", "turn_right", "repeat", "if", "call", "action", "drop"];
const CONCEPTS: Concept[] = ["direction", "sequencing", "debugging", "loops", "conditionals", "functions", "events", "counting"];
const VALUES: ValueId[] = ["patience", "sharing", "helping", "kindness", "responsibility", "creativity"];
const AGES: AgeLevel[] = ["minik", "kasif", "mucit"];
const OPTIONS: OptionKey[] = ["ghostPath", "guided", "beeMemoryVisible", "intro", "autoDrop", "resetEachRun", "clearMemoryEachRun", "askRemaining"];

interface Props {
  level: LevelDto;
  onChange: (level: LevelDto) => void;
}

function toggle<T>(list: readonly T[], item: T): T[] {
  return list.includes(item) ? list.filter((x) => x !== item) : [...list, item];
}

export function SizePanel({ level, onChange }: Props) {
  const { t } = useTranslation();
  const { width, height } = size(level);
  return (
    <fieldset className="panel row">
      <legend>{t("grid.heading")}</legend>
      <label>
        {t("grid.width")}
        <input type="number" min={2} max={12} value={width} onChange={(e) => onChange(resize(level, Number(e.target.value), height))} />
      </label>
      <label>
        {t("grid.height")}
        <input type="number" min={2} max={12} value={height} onChange={(e) => onChange(resize(level, width, Number(e.target.value)))} />
      </label>
    </fieldset>
  );
}

export function CardsPanel({ level, onChange }: Props) {
  const { t } = useTranslation();
  return (
    <fieldset className="panel">
      <legend>{t("cards.heading")}</legend>
      <div className="checks">
        {CARDS.map((card) => (
          <label key={card}>
            <input type="checkbox" checked={level.cards.palette.includes(card)} onChange={() => onChange(togglePaletteCard(level, card))} />
            {t(`card.${card}`)}
          </label>
        ))}
      </div>
      <div className="row">
        <label>
          {t("cards.maxLength")}
          <input
            type="number"
            min={1}
            max={30}
            value={level.cards.maxProgramLength}
            onChange={(e) => onChange({ ...level, cards: { ...level.cards, maxProgramLength: Math.max(1, Math.min(30, Number(e.target.value))) } })}
          />
        </label>
        <label>
          {t("cards.introduces")}
          <select
            value={level.cards.introduces ?? ""}
            onChange={(e) => {
              const cards = { ...level.cards };
              if (e.target.value) cards.introduces = e.target.value as CardId;
              else delete cards.introduces;
              onChange({ ...level, cards });
            }}
          >
            <option value="">{t("cards.none")}</option>
            {level.cards.palette.map((c) => (
              <option key={c} value={c}>
                {t(`card.${c}`)}
              </option>
            ))}
          </select>
        </label>
      </div>
    </fieldset>
  );
}

export function MetaPanel({ level, onChange }: Props) {
  const { t } = useTranslation();
  const meta = level.meta;
  const set = (patch: Partial<LevelDto["meta"]>) => onChange({ ...level, meta: { ...meta, ...patch } });
  return (
    <fieldset className="panel">
      <legend>{t("meta.heading")}</legend>
      <p className="muted">
        {t("meta.id")}: <code>{level.id}</code> · {t("meta.order")}: {level.order}
      </p>
      <div className="row">
        <label>
          {t("meta.value")}
          <select value={meta.value} onChange={(e) => set({ value: e.target.value as ValueId })}>
            {VALUES.map((v) => (
              <option key={v} value={v}>
                {t(`value.${v}`)}
              </option>
            ))}
          </select>
        </label>
        <label>
          {t("meta.difficulty")}
          <input type="number" min={1} max={5} value={meta.difficulty} onChange={(e) => set({ difficulty: Math.max(1, Math.min(5, Number(e.target.value))) })} />
        </label>
      </div>
      <p className="label">{t("meta.concepts")}</p>
      <div className="checks">
        {CONCEPTS.map((c) => (
          <label key={c}>
            <input
              type="checkbox"
              checked={meta.concepts.includes(c)}
              onChange={() => {
                const next = toggle(meta.concepts, c);
                if (next.length > 0) set({ concepts: next });
              }}
            />
            {t(`concept.${c}`)}
          </label>
        ))}
      </div>
      <p className="label">{t("meta.ageLevels")}</p>
      <div className="checks">
        {AGES.map((a) => (
          <label key={a}>
            <input
              type="checkbox"
              checked={meta.ageLevels.includes(a)}
              onChange={() => {
                const next = toggle(meta.ageLevels, a);
                if (next.length > 0) set({ ageLevels: next });
              }}
            />
            {t(`age.${a}`)}
          </label>
        ))}
      </div>
      <p className="label">{t("meta.options")}</p>
      <div className="checks">
        {OPTIONS.map((key) => (
          <label key={key}>
            <input type="checkbox" checked={level.options?.[key] ?? false} onChange={(e) => onChange(setOption(level, key, e.target.checked))} />
            {t(`option.${key}`)}
          </label>
        ))}
      </div>
      <label className="block">
        {t("meta.notes")}
        <textarea rows={3} value={meta.notes ?? ""} onChange={(e) => set({ notes: e.target.value })} />
      </label>
    </fieldset>
  );
}

export function VoicePanel({ level, voiceKeys }: { level: LevelDto; voiceKeys: ReadonlySet<string> }) {
  const { t } = useTranslation();
  const rows: Array<[string, string | undefined]> = [
    [t("voice.intro"), level.voice.intro],
    [t("voice.success"), level.voice.success],
  ];
  return (
    <fieldset className="panel">
      <legend>{t("voice.heading")}</legend>
      <ul className="plain">
        {rows.map(([label, key]) => (
          <li key={label}>
            {label}: <code>{key ?? "—"}</code>
            {key && voiceKeys.size > 0 && !voiceKeys.has(key) && <strong className="warn"> ⚠ {t("voice.missing")}</strong>}
          </li>
        ))}
      </ul>
    </fieldset>
  );
}

/** Honey: the basket at the start and the honey goal (deliver to hives, share equally, keep some). ADR 0030. */
export function HoneyPanel({ level, onChange }: Props) {
  const { t } = useTranslation();
  const hives = (level.drops ?? []).filter((d) => d.kind === "hive");
  const kind = honeyGoal(level);
  const choose = (next: HoneyGoalKind) => {
    let updated = setShare(level, false);
    for (const h of hives) updated = setDeliver(updated, h.id, null);
    if (next === "share") updated = setShare(updated, true);
    if (next === "deliver" && hives[0]) updated = setDeliver(updated, hives[0].id, hives[0].need ?? 1);
    onChange(updated);
  };
  return (
    <fieldset className="panel">
      <legend>{t("honey.heading")}</legend>
      <label className="block">
        {t("honey.carrying")}
        <input type="number" min={0} max={MAX_CARRYING} value={level.robot.carrying ?? 0} onChange={(e) => onChange(setCarrying(level, Number(e.target.value)))} />
      </label>
      <p className="label">{t("honey.goal")}</p>
      <div className="tool-list">
        {(["none", "deliver", "share"] as const).map((k) => (
          <label key={k} className={`tool ${kind === k ? "tool--active" : ""}`}>
            <input type="radio" name="honey-goal" value={k} checked={kind === k} onChange={() => choose(k)} />
            {t(`honey.kind.${k}`)}
          </label>
        ))}
      </div>
      {kind === "share" && !level.goal.share && <p className="warn">{t("honey.shareNeedsHives")}</p>}
      {kind === "deliver" &&
        hives.map((h) => (
          <label key={h.id} className="block">
            {t("honey.deliverTo", { id: h.id })}
            <input
              type="number"
              min={0}
              max={MAX_NEED}
              value={level.goal.deliver?.find((d) => d.to === h.id)?.count ?? 0}
              onChange={(e) => onChange(setDeliver(level, h.id, Number(e.target.value) || null))}
            />
          </label>
        ))}
      {kind === "none" && hives.length > 0 && <p className="muted">{t("honey.noGoalHint")}</p>}
      <label className="block">
        {t("honey.carryAtEnd")}
        <input
          type="number"
          min={0}
          max={MAX_CARRYING}
          value={level.goal.carryAtEnd ?? ""}
          placeholder={t("honey.carryAtEndNone")}
          onChange={(e) => onChange(setCarryAtEnd(level, e.target.value === "" ? null : Number(e.target.value)))}
        />
      </label>
    </fieldset>
  );
}

/** Starting values for a Paylaşım Köyü level (PK-0004, PK-0005). */
export function VillagePresetPanel({ level, onChange }: Props) {
  const { t } = useTranslation();
  return (
    <fieldset className="panel">
      <legend>{t("village.heading")}</legend>
      <p className="muted">{t("village.hint")}</p>
      <div className="row">
        <button type="button" onClick={() => onChange(applyVillagePreset(level, "minik"))}>
          {t("village.minik")}
        </button>
        <button type="button" onClick={() => onChange(applyVillagePreset(level, "kasif"))}>
          {t("village.kasif")}
        </button>
      </div>
    </fieldset>
  );
}
