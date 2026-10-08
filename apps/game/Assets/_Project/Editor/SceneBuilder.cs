using System.IO;
using Roboya.Core;
using Roboya.Games.YonAvcisi;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.EditorTools
{
    /// <summary>
    /// Wires the thin scenes from code (CLAUDE.md §7: no hand-edited scene YAML). Idempotent: finds or creates
    /// each object and component, then assigns serialized references.
    /// </summary>
    public static class SceneBuilder
    {
        private const string UiDir = "Assets/_Project/UI";
        private const string PanelSettingsPath = UiDir + "/ChildPanelSettings.asset";
        private const string ThemePath = UiDir + "/RoboyaTheme.tss";
        private const string YonAvcisiUxml = UiDir + "/YonAvcisi/YonAvcisi.uxml";

        public static void Build(string scenesDir)
        {
            EnsurePanelSettings();
            AssetDatabase.SaveAssets();
            BuildBoot(scenesDir + "/Boot.unity");
            BuildGame(scenesDir + "/Game.unity");
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenesDir + "/Boot.unity");
        }

        private static PanelSettings EnsurePanelSettings()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, PanelSettingsPath);
            }

            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1280, 800);
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panel.match = 0.5f;
            EditorUtility.SetDirty(panel);
            return panel;
        }

        private static void BuildBoot(string path)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var go = FindOrCreate("Bootstrap");
            if (go.GetComponent<Bootstrap>() == null)
            {
                go.AddComponent<Bootstrap>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildGame(string path)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            // Load after opening: OpenScene(Single) unloads unused assets, destroying earlier references.
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            var cam = Object.FindAnyObjectByType<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(1f, 0.956f, 0.886f);
            }

            var go = FindOrCreate("YonAvcisi");
            var doc = go.GetComponent<UIDocument>() ?? go.AddComponent<UIDocument>();
            // Assign through SerializedObject: the property setters do not persist into the saved scene in batch mode.
            var docSo = new SerializedObject(doc);
            docSo.FindProperty("m_PanelSettings").objectReferenceValue = panel;
            docSo.FindProperty("sourceAsset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(YonAvcisiUxml);
            docSo.ApplyModifiedPropertiesWithoutUndo();

            var screen = go.GetComponent<YonAvcisiScreen>() ?? go.AddComponent<YonAvcisiScreen>();
            var so = new SerializedObject(screen);
            so.FindProperty("document").objectReferenceValue = doc;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject FindOrCreate(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go : new GameObject(name);
        }

        public static string ScenePath(string dir, string name) => Path.Combine(dir, name + ".unity").Replace('\\', '/');
    }
}
