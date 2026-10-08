using System.IO;
using Roboya.Core;
using Roboya.Games.YonAvcisi;
using Roboya.Map;
using Roboya.UI;
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
        private const string MapUxml = UiDir + "/Map/Map.uxml";
        private const string PartArtPath = "Assets/_Project/Art/Island/PartArt.asset";
        private const string ArtDir = "Assets/_Project/Art";
        private const string SabirOrmaniArtPath = ArtDir + "/SabirOrmani/SabirOrmaniArt.asset";

        public static void Build(string scenesDir)
        {
            EnsurePanelSettings();
            AssetDatabase.SaveAssets();
            BuildBoot(scenesDir + "/Boot.unity");
            BuildGame(scenesDir + "/Game.unity");
            BuildMap(scenesDir + "/Map.unity");
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
            var boot = go.GetComponent<Bootstrap>() ?? go.AddComponent<Bootstrap>();
            var bootSo = new SerializedObject(boot);
            bootSo.FindProperty("firstScene").stringValue = Bootstrap.MapScene;
            bootSo.ApplyModifiedPropertiesWithoutUndo();

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
            so.FindProperty("art").objectReferenceValue = EnsureSabirOrmaniArt();
            so.FindProperty("partArt").objectReferenceValue = EnsurePartArt();
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildMap(string path)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            var cam = Object.FindAnyObjectByType<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.56f, 0.83f, 0.85f);
            }

            var go = FindOrCreate("Map");
            var doc = go.GetComponent<UIDocument>() ?? go.AddComponent<UIDocument>();
            var docSo = new SerializedObject(doc);
            docSo.FindProperty("m_PanelSettings").objectReferenceValue = panel;
            docSo.FindProperty("sourceAsset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MapUxml);
            docSo.ApplyModifiedPropertiesWithoutUndo();

            var screen = go.GetComponent<MapScreen>() ?? go.AddComponent<MapScreen>();
            var so = new SerializedObject(screen);
            so.FindProperty("document").objectReferenceValue = doc;
            so.FindProperty("art").objectReferenceValue = EnsureSabirOrmaniArt();
            so.FindProperty("partArt").objectReferenceValue = EnsurePartArt();
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>Ship body and repair parts, clouds and the island illustration, looked up by file name.</summary>
        private static PartArt EnsurePartArt()
        {
            var art = AssetDatabase.LoadAssetAtPath<PartArt>(PartArtPath);
            if (art == null)
            {
                art = ScriptableObject.CreateInstance<PartArt>();
                AssetDatabase.CreateAsset(art, PartArtPath);
            }

            var sprites = new System.Collections.Generic.List<Sprite>();
            foreach (var dir in new[] { ArtDir + "/Ship", ArtDir + "/Island" })
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Sprite", new[] { dir }))
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
                    if (sprite != null)
                    {
                        sprites.Add(sprite);
                    }
                }
            }

            sprites.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            var so = new SerializedObject(art);
            var array = so.FindProperty("sprites");
            array.arraySize = sprites.Count;
            for (int i = 0; i < sprites.Count; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(art);
            AssetDatabase.SaveAssets();
            return art;
        }

        /// <summary>Creates or refreshes the region sprite set from the files in Art/ (F0-17, docs/art/style-guide.md).</summary>
        private static RegionArt EnsureSabirOrmaniArt()
        {
            var art = AssetDatabase.LoadAssetAtPath<RegionArt>(SabirOrmaniArtPath);
            if (art == null)
            {
                art = ScriptableObject.CreateInstance<RegionArt>();
                AssetDatabase.CreateAsset(art, SabirOrmaniArtPath);
            }

            Sprite S(string rel) => AssetDatabase.LoadAssetAtPath<Sprite>(ArtDir + "/" + rel + ".png");
            var so = new SerializedObject(art);
            so.FindProperty("robotFront").objectReferenceValue = S("Characters/Roboya/roboya_front");
            so.FindProperty("robotBack").objectReferenceValue = S("Characters/Roboya/roboya_back");
            so.FindProperty("robotSide").objectReferenceValue = S("Characters/Roboya/roboya_side");
            so.FindProperty("robotHappy").objectReferenceValue = S("Characters/Roboya/roboya_happy");
            so.FindProperty("robotLaughing").objectReferenceValue = S("Characters/Roboya/roboya_laughing");
            so.FindProperty("robotCurious").objectReferenceValue = S("Characters/Roboya/roboya_curious");
            so.FindProperty("robotSurprised").objectReferenceValue = S("Characters/Roboya/roboya_surprised");
            so.FindProperty("robotProud").objectReferenceValue = S("Characters/Roboya/roboya_proud");
            so.FindProperty("goalIdle").objectReferenceValue = S("Characters/BilgeKaplumbaga/turtle_front");
            so.FindProperty("goalHappy").objectReferenceValue = S("Characters/BilgeKaplumbaga/turtle_happy");
            so.FindProperty("goalExplaining").objectReferenceValue = S("Characters/BilgeKaplumbaga/turtle_explaining");
            so.FindProperty("goalThanks").objectReferenceValue = S("Characters/BilgeKaplumbaga/turtle_thanks");
            so.FindProperty("tileFloor").objectReferenceValue = S("SabirOrmani/tile_grass");
            so.FindProperty("tilePath").objectReferenceValue = S("SabirOrmani/tile_path");
            so.FindProperty("background").objectReferenceValue = S("SabirOrmani/bg_sabir_ormani");
            so.FindProperty("fruitRed").objectReferenceValue = S("SabirOrmani/item_apple");
            so.FindProperty("fruitYellow").objectReferenceValue = S("SabirOrmani/item_pear");
            so.FindProperty("shipPart").objectReferenceValue = S("SabirOrmani/item_gear");
            so.FindProperty("propTree").objectReferenceValue = S("SabirOrmani/prop_tree");
            so.FindProperty("propRock").objectReferenceValue = S("SabirOrmani/prop_rock");
            so.FindProperty("propBush").objectReferenceValue = S("SabirOrmani/prop_bush");
            SetSprites(so.FindProperty("obstacles"), S, "SabirOrmani/prop_tree", "SabirOrmani/prop_rock", "SabirOrmani/prop_bush");
            SetSprites(so.FindProperty("decor"), S, "SabirOrmani/prop_bush", "SabirOrmani/prop_tree", "SabirOrmani/prop_bush");

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(art);
            AssetDatabase.SaveAssets();
            return art;
        }

        private static void SetSprites(SerializedProperty array, System.Func<string, Sprite> load, params string[] names)
        {
            array.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = load(names[i]);
            }
        }

        private static GameObject FindOrCreate(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go : new GameObject(name);
        }

        public static string ScenePath(string dir, string name) => Path.Combine(dir, name + ".unity").Replace('\\', '/');
    }
}
