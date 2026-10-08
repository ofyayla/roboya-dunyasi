import type { AgeLevel, CardId, Concept, LevelDto, ValueId } from "@roboya/level-schema";
import { useTranslation } from "react-i18next";
import { resize, size, togglePaletteCard } from "../model/levelOps";

const CARDS: CardId[] = ["forward", "backward", "turn_left", "turn_right", "repeat", "if", "call", "action"];
const CONCEPTS: Concept[] = ["direction", "sequencing", "debugging", "loops", "conditionals", "functions", "events", "counting"];
const VALUES: ValueId[] = ["patience", "sharing", "helping", "kindness", "responsibility", "creativity"];
const AGES: AgeLevel[] = ["minik", "kasif", "mucit"];

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
  const setOption = (key: "ghostPath" | "guided", on: boolean) => {
    // Only `true` flags are stored, so saved files stay minimal.
    const flags = { ghostPath: level.options?.ghostPath ?? false, guided: level.options?.guided ?? false, [key]: on };
    const options: NonNullable<LevelDto["options"]> = {};
    if (flags.ghostPath) options.ghostPath = true;
    if (flags.guided) options.guided = true;
    const { options: _previous, ...rest } = level;
    void _previous;
    onChange(Object.keys(options).length > 0 ? { ...rest, options } : rest);
  };
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
      <div className="checks">
        <label>
          <input type="checkbox" checked={level.options?.ghostPath ?? false} onChange={(e) => setOption("ghostPath", e.target.checked)} />
          {t("meta.ghostPath")}
        </label>
        <label>
          <input type="checkbox" checked={level.options?.guided ?? false} onChange={(e) => setOption("guided", e.target.checked)} />
          {t("meta.guided")}
        </label>
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
