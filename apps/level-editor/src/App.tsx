import { validateLevel, type LevelDto } from "@roboya/level-schema";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { api, type LevelListItem } from "./api";
import { GridView } from "./components/GridView";
import { LevelList } from "./components/LevelList";
import { CardsPanel, MetaPanel, SizePanel, VoicePanel } from "./components/SettingsPanels";
import { StatusPanel } from "./components/StatusPanel";
import { ToolBar } from "./components/ToolBar";
import { applyTool, levelPath, newLevel, tracePath, type ItemBrush, type Tool } from "./model/levelOps";
import { useSolver } from "./useSolver";
import "./App.css";

export function App() {
  const { t } = useTranslation();
  const [levels, setLevels] = useState<LevelListItem[]>([]);
  const [path, setPath] = useState<string | null>(null);
  const [level, setLevel] = useState<LevelDto>(() => newLevel("sabir-ormani", "yon-avcisi", 1));
  const [tool, setTool] = useState<Tool>("wall");
  const [brush, setBrush] = useState<ItemBrush>({ kind: "fruit", color: "red" });
  const [voiceKeys, setVoiceKeys] = useState<ReadonlySet<string>>(new Set());
  const [saveMessage, setSaveMessage] = useState<string | null>(null);
  const solver = useSolver(level);

  const refresh = useCallback(() => {
    api.listLevels().then(setLevels).catch((e: unknown) => console.error(e));
  }, []);

  useEffect(() => {
    refresh();
    api.voiceKeys().then((keys) => setVoiceKeys(new Set(keys))).catch((e: unknown) => console.error(e));
  }, [refresh]);

  const edit = (next: LevelDto) => {
    setLevel(next);
    setSaveMessage(null);
  };

  const open = (p: string) => {
    api
      .readLevel(p)
      .then((l) => {
        setLevel(l);
        setPath(p);
        setSaveMessage(null);
      })
      .catch((e: unknown) => console.error(e));
  };

  const createNew = () => {
    const sameGame = levels.filter((l) => l.game === "yon-avcisi");
    const order = Math.max(0, ...sameGame.map((l) => l.order)) + 1;
    const fresh = newLevel("sabir-ormani", "yon-avcisi", order);
    setLevel(fresh);
    setPath(levelPath(fresh));
    setSaveMessage(null);
  };

  const shortest = solver.status === "done" ? solver.result.shortestLength : null;
  const toSave = useMemo<LevelDto>(() => (shortest !== null ? { ...level, solution: { shortestLength: shortest } } : level), [level, shortest]);
  const schema = validateLevel(toSave);
  const schemaErrors = schema.valid ? [] : schema.errors;
  const solverOk = solver.status === "done" && solver.result.valid && shortest !== null;
  const routeCards = solver.status === "done" ? (solver.result.solution ?? []) : [];

  const save = () => {
    const target = levelPath(toSave);
    api
      .saveLevel(target, toSave)
      .then(({ saved }) => {
        setPath(saved);
        setSaveMessage(t("status.saved", { path: saved }));
        refresh();
      })
      .catch((e: unknown) => setSaveMessage(String(e)));
  };

  return (
    <div className="layout">
      <header className="top">
        <h1>{t("app.title")}</h1>
        <p className="muted">{t("app.subtitle")}</p>
      </header>
      <LevelList levels={levels} selected={path} onOpen={open} onNew={createNew} />
      <main className="workspace">
        <ToolBar tool={tool} brush={brush} onTool={setTool} onBrush={setBrush} />
        <GridView level={level} path={tracePath(level, routeCards)} onCell={(x, y) => edit(applyTool(level, x, y, tool, brush))} />
        <SizePanel level={level} onChange={edit} />
      </main>
      <aside className="side">
        <StatusPanel solver={solver} schemaErrors={schemaErrors} canSave={solverOk && schemaErrors.length === 0} saveMessage={saveMessage} onSave={save} />
        <CardsPanel level={level} onChange={edit} />
        <MetaPanel level={level} onChange={edit} />
        <VoicePanel level={level} voiceKeys={voiceKeys} />
      </aside>
    </div>
  );
}
