using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class JerseyColorBatchTool : EditorWindow
{
    private const string targetMaterialName = "Jersey_Cel_Mat";
    private const string colorProperty = "_Color";
    private const string SAVE_KEY = "JerseyColorBatchTool_SaveData";

    private Color currentColor = Color.white;
    private Color previousColor = Color.white;

    private bool livePreview = true;
    private bool createInstanceBeforeEdit = false;

    private List<ColorPreset> presets = new List<ColorPreset>();
    private List<Color> presetHistory = new List<Color>();
    private List<Color> customSwatches = new List<Color>();

    private Vector2 scrollPos;
    private int selectedTab = 0;
    private readonly string[] tabs = { "Presets", "Custom Color", "Team History" };

    [System.Serializable]
    public class ColorPreset
    {
        public string name;
        public Color color;
    }

    [System.Serializable]
    private class SaveData
    {
        public List<ColorPreset> presets;
        public List<Color> presetHistory;
        public List<Color> customSwatches;
        public Color currentColor;
        public bool livePreview;
        public bool createInstanceBeforeEdit;
    }

    [MenuItem("Tools/Jersey Color Batch Tool")]
    public static void ShowWindow()
    {
        GetWindow<JerseyColorBatchTool>("Jersey Tool");
    }

    private void OnEnable()
    {
        LoadData();

        if (presets == null || presets.Count == 0)
        {
            presets = new List<ColorPreset>
            {
                new ColorPreset { name = "Home", color = new Color(0.1f, 0.3f, 0.8f) },
                new ColorPreset { name = "Away", color = Color.white },
                new ColorPreset { name = "Alternate", color = new Color(0.8f, 0.1f, 0.1f) }
            };
        }
    }

    private void OnDisable()
    {
        SaveDataToPrefs();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Space(6);

        DrawBoxSection(DrawSelectionInfo);
        DrawBoxSection(DrawUtilitySection);

        GUILayout.Space(10);
        selectedTab = GUILayout.Toolbar(selectedTab, tabs, GUILayout.Height(30));
        GUILayout.Space(10);

        EditorGUILayout.BeginVertical("box");
        GUILayout.Space(6);

        switch (selectedTab)
        {
            case 0:
                DrawPresetSection();
                break;
            case 1:
                DrawCustomColorSection();
                break;
            case 2:
                DrawHistorySection();
                break;
        }

        GUILayout.Space(8);
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    private void DrawBoxSection(System.Action drawMethod)
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Space(4);
        drawMethod.Invoke();
        GUILayout.Space(6);
        EditorGUILayout.EndVertical();
        GUILayout.Space(6);
    }

    // -------------------- SECTIONS --------------------

    private void DrawSelectionInfo()
    {
        EditorGUILayout.LabelField("SELECTION INFO", EditorStyles.boldLabel);
        GUILayout.Space(4);

        var materials = GetTargetMaterials();

        EditorGUILayout.HelpBox(
            $"Selected Objects: {Selection.gameObjects.Length}\nDetected Jersey Materials: {materials.Count}",
            MessageType.Info);
    }

    private void DrawUtilitySection()
    {
        EditorGUILayout.LabelField("UTILITIES", EditorStyles.boldLabel);
        GUILayout.Space(4);

        livePreview = EditorGUILayout.ToggleLeft("Live Preview", livePreview);
        createInstanceBeforeEdit = EditorGUILayout.ToggleLeft("Create Instance Before Edit", createInstanceBeforeEdit);

        GUILayout.Space(6);

        if (GUILayout.Button("Generate Random Team Color"))
        {
            Color random = GenerateRandomColor();
            currentColor = random;
            ApplyColor(random);
            SaveToHistory(random);
        }

        if (GUILayout.Button("Reset To Previous Color"))
        {
            ApplyColor(previousColor);
        }
    }

    private void DrawPresetSection()
    {
        EditorGUILayout.LabelField("PRESETS", EditorStyles.boldLabel);
        GUILayout.Space(6);

        for (int i = 0; i < presets.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            presets[i].name = EditorGUILayout.TextField(presets[i].name, GUILayout.Width(120));
            presets[i].color = EditorGUILayout.ColorField(presets[i].color);

            if (GUILayout.Button("Apply", GUILayout.Width(60)))
            {
                ApplyColor(presets[i].color);
                SaveToHistory(presets[i].color);
            }

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                presets.RemoveAt(i);
                SaveDataToPrefs();
                break;
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
        }

        GUILayout.Space(6);

        if (GUILayout.Button("Add New Preset", GUILayout.Height(28)))
        {
            presets.Add(new ColorPreset { name = "NewPreset", color = Color.white });
            SaveDataToPrefs();
        }
    }

    private void DrawCustomColorSection()
    {
        EditorGUILayout.LabelField("CUSTOM COLOR", EditorStyles.boldLabel);
        GUILayout.Space(6);

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Color Picker", GUILayout.Width(90));
        currentColor = EditorGUILayout.ColorField(currentColor);
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck() && livePreview)
        {
            ApplyColor(currentColor);
        }

        GUILayout.Space(6);

        if (!livePreview && GUILayout.Button("Apply Custom Color"))
        {
            ApplyColor(currentColor);
            SaveToHistory(currentColor);
        }

        if (GUILayout.Button("Save To Swatch"))
        {
            if (!customSwatches.Contains(currentColor))
            {
                customSwatches.Add(currentColor);
                SaveDataToPrefs();
            }
        }

        if (customSwatches.Count > 0)
        {
            GUILayout.Space(6);
            EditorGUILayout.LabelField("Swatches:");

            EditorGUILayout.BeginHorizontal();
            foreach (var swatch in customSwatches.ToList())
            {
                GUI.backgroundColor = swatch;

                if (GUILayout.Button("", GUILayout.Width(28), GUILayout.Height(22)))
                {
                    ApplyColor(swatch);
                    SaveToHistory(swatch);
                }

                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawHistorySection()
    {
        EditorGUILayout.LabelField("TEAM COLOR HISTORY", EditorStyles.boldLabel);
        GUILayout.Space(6);

        var lastFive = presetHistory.TakeLast(5).ToList();

        foreach (var color in lastFive)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.ColorField(color);

            if (GUILayout.Button("Reapply", GUILayout.Width(70)))
            {
                ApplyColor(color);
                SaveToHistory(color);
            }

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                presetHistory.Remove(color);
                SaveDataToPrefs();
                break;
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
        }

        if (presetHistory.Count > 0)
        {
            GUILayout.Space(6);

            if (GUILayout.Button("Clear Entire History"))
            {
                presetHistory.Clear();
                SaveDataToPrefs();
            }
        }
    }

    // -------------------- CORE LOGIC --------------------

    private List<Material> GetTargetMaterials()
    {
        List<Material> found = new List<Material>();

        foreach (var obj in Selection.gameObjects)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>(true);

            foreach (var r in renderers)
            {
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat != null && mat.name.Contains(targetMaterialName))
                        found.Add(mat);
                }
            }
        }

        return found.Distinct().ToList();
    }

    private void ApplyColor(Color color)
    {
        var materials = GetTargetMaterials();
        if (materials.Count == 0)
            return;

        previousColor = materials[0].GetColor(colorProperty);

        foreach (var mat in materials)
        {
            Material targetMat = mat;

            if (createInstanceBeforeEdit)
            {
                targetMat = new Material(mat);
                ReplaceMaterialInstance(mat, targetMat);
            }

            Undo.RecordObject(targetMat, "Change Jersey Color");
            targetMat.SetColor(colorProperty, color);
            EditorUtility.SetDirty(targetMat);
        }
    }

    private void SaveToHistory(Color color)
    {
        if (presetHistory.Count == 0 || presetHistory.Last() != color)
        {
            presetHistory.Add(color);
            SaveDataToPrefs();
        }
    }

    private void ReplaceMaterialInstance(Material original, Material instance)
    {
        foreach (var obj in Selection.gameObjects)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>(true);

            foreach (var r in renderers)
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == original)
                        mats[i] = instance;
                }
                r.sharedMaterials = mats;
            }
        }
    }

    private Color GenerateRandomColor()
    {
        float hue = Random.value;
        float saturation = Random.Range(0.6f, 1f);
        float value = Random.Range(0.7f, 1f);

        return Color.HSVToRGB(hue, saturation, value);
    }

    // -------------------- SAVE / LOAD --------------------

    private void SaveDataToPrefs()
    {
        SaveData data = new SaveData
        {
            presets = presets,
            presetHistory = presetHistory,
            customSwatches = customSwatches,
            currentColor = currentColor,
            livePreview = livePreview,
            createInstanceBeforeEdit = createInstanceBeforeEdit
        };

        string json = JsonUtility.ToJson(data);
        EditorPrefs.SetString(SAVE_KEY, json);
    }

    private void LoadData()
    {
        if (!EditorPrefs.HasKey(SAVE_KEY))
            return;

        string json = EditorPrefs.GetString(SAVE_KEY);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        presets = data.presets ?? new List<ColorPreset>();
        presetHistory = data.presetHistory ?? new List<Color>();
        customSwatches = data.customSwatches ?? new List<Color>();

        currentColor = data.currentColor;
        livePreview = data.livePreview;
        createInstanceBeforeEdit = data.createInstanceBeforeEdit;
    }
}
