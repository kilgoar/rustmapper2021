using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using RustMapEditor.Variables;

public static class SettingsManager
{
    public const string SettingsPath = "EditorSettings.json";

    public const string BundlePathExt = @"\Bundles\Bundles";
	public static bool style { get; set; }
    public static string RustDirectory { get; set; }
    public static float PrefabRenderDistance { get; set; }
    public static float PathRenderDistance { get; set; }
    public static float WaterTransparency { get; set; }
    public static bool LoadBundleOnLaunch { get; set; }
    public static bool TerrainTextureSet { get; set; }
	public static CrazingPreset crazing { get; set; }
	public static PerlinSplatPreset perlinSplat { get; set; }
	public static RipplePreset ripple { get; set; }
	public static OceanPreset ocean { get; set; }
	public static TerracingPreset terracing { get; set; }
	public static PerlinPreset perlin { get; set; }
	public static GeologyPreset geology { get; set; }
	public static ReplacerPreset replacer { get; set; }
	public static string[] geologyPresets { get; set; }
    public static string[] PrefabPaths { get; private set; }
	
	public static GeologyPreset[] macro {get; set; }
 	

    [InitializeOnLoadMethod]
    private static void Init()
    {
        if (!File.Exists(SettingsPath))
            using (StreamWriter write = new StreamWriter(SettingsPath, false))
                write.Write(JsonUtility.ToJson(new EditorSettings(), true));

        LoadSettings();
    }

    /// <summary>Saves the current EditorSettings to a JSON file.</summary>
    public static void SaveSettings()
    {
        using (StreamWriter write = new StreamWriter(SettingsPath, false))
        {
            EditorSettings editorSettings = new EditorSettings
            (
                RustDirectory, PrefabRenderDistance, PathRenderDistance, WaterTransparency, LoadBundleOnLaunch, TerrainTextureSet, style, crazing, perlinSplat, ripple, ocean, terracing, perlin, geology, replacer
            );
            write.Write(JsonUtility.ToJson(editorSettings, true));
        }
    }
	
	public static void SaveGeologyPreset()
    {
        using (StreamWriter write = new StreamWriter($"Presets/Geology/{geology.title}.json", false))
        {
            write.Write(JsonUtility.ToJson(geology, true));
        }
    }
	
	public static void SaveReplacerPreset()
    {
        using (StreamWriter write = new StreamWriter($"Presets/Replacer/{replacer.title}.json", false))
        {
            write.Write(JsonUtility.ToJson(replacer, true));
        }
    }
	
	public static void LoadGeologyPreset(string filename)
	{
		using (StreamReader reader = new StreamReader($"Presets/Geology/{filename}.json"))
			{
				geology = JsonUtility.FromJson<GeologyPreset>(reader.ReadToEnd());
			}
	}
	
	public static void LoadReplacerPreset(string filename)
	{
		using (StreamReader reader = new StreamReader($"Presets/Replacer/{filename}.json"))
			{
				replacer = JsonUtility.FromJson<ReplacerPreset>(reader.ReadToEnd());
			}
	}
	
	public static void LoadGeologyMacro(string filename)
	{
			using (StreamReader reader = new StreamReader($"Presets/Geology/Macros/{filename}.macro"))
			{
				string macroFile = reader.ReadToEnd();
				
				
				char[] delimiters = { '*'};
				string[] parse = macroFile.Split(delimiters);
				int length = parse.Length-1;
				GeologyPreset[] newMacro = new GeologyPreset[length];
				
				for(int i = 0; i < length; i++)
					{
						newMacro[i] = JsonUtility.FromJson<GeologyPreset>(parse[i]);
					}
				macro = newMacro;
			}
	}
	
	
	public static void SaveGeologyMacro(string macroTitle)
    {
		string macroFile="";
		for (int i = 0; i < macro.Length; i++)
		{
			macroFile += JsonUtility.ToJson(macro[i], true) + "*";
		}
		
        using (StreamWriter write = new StreamWriter($"Presets/Geology/Macros/{macroTitle}.macro", false))
        {
            write.Write(macroFile);
        }
    }
	
	public static void AddToMacro(string macroTitle)
	{
		int append  = 0;
		if (File.Exists($"Presets/Geology/Macros/{macroTitle}.macro"))
			{
				LoadGeologyMacro(macroTitle);
			}
		else
			{
				macro = new GeologyPreset[0];
			}

		if (macro != null)
		{
		 append = macro.Length;
		}
		
		GeologyPreset[] newMacro = new GeologyPreset[append+1];
		for (int i = 0; i < append; i++)
			{	
					newMacro[i] = macro[i];
			}
		
		newMacro[append] = geology;
		macro = newMacro;
		SaveGeologyMacro(macroTitle);
	
	}
	

    /// <summary>Loads and sets the current EditorSettings from a JSON file.</summary>
    public static void LoadSettings()
    {
        using (StreamReader reader = new StreamReader(SettingsPath))
        {
            EditorSettings editorSettings = JsonUtility.FromJson<EditorSettings>(reader.ReadToEnd());
            RustDirectory = editorSettings.rustDirectory;
            PrefabRenderDistance = editorSettings.prefabRenderDistance;
            PathRenderDistance = editorSettings.pathRenderDistance;
            WaterTransparency = editorSettings.waterTransparency;
            LoadBundleOnLaunch = editorSettings.loadbundleonlaunch;
            PrefabPaths = editorSettings.prefabPaths;
			style = editorSettings.style;
			crazing = editorSettings.crazing;
			perlinSplat = editorSettings.perlinSplat;
			ripple = editorSettings.ripple;
			ocean = editorSettings.ocean;
			terracing = editorSettings.terracing;
			perlin = editorSettings.perlin;
			geology = editorSettings.geology;
			replacer = editorSettings.replacer;
        }

		LoadPresets();
		LoadMacros();
    }
	
	public static void LoadPresets()
	{
		string[] geologyPresets = Directory.GetFiles("Presets/Geology/");
	}
	
	public static void LoadMacros()
	{
		string[] geologyPresets = Directory.GetFiles("Presets/Geology/Macros/");
	}
	
	public static string[] GetPresetTitles(string path)
	{
		char[] delimiters = { '/', '.'};
		string[] geologyPresets = Directory.GetFiles(path);
		string[] parse;
		string[] filenames = new string [geologyPresets.Length];
		int filenameID;
		for(int i = 0; i < geologyPresets.Length; i++)
		{
			parse = geologyPresets[i].Split(delimiters);
			filenameID = parse.Length - 2;
			filenames[i] = parse[filenameID];
		}
		return filenames;
	}
	

    /// <summary> Sets the EditorSettings back to default values.</summary>
    public static void SetDefaultSettings()
    {
        RustDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Rust";
        ToolTips.rustDirectoryPath.text = RustDirectory;
        PrefabRenderDistance = 700f;
        PathRenderDistance = 250f;
        WaterTransparency = 0.2f;
        LoadBundleOnLaunch = false;
        Debug.Log("Default Settings set.");
    }
}

[Serializable]
public struct EditorSettings
{
    public string rustDirectory;
    public float prefabRenderDistance;
    public float pathRenderDistance;
    public float waterTransparency;
    public bool loadbundleonlaunch;
    public bool terrainTextureSet;
	public bool style;
	public CrazingPreset crazing;
	public PerlinSplatPreset perlinSplat;
	public RipplePreset ripple;
	public OceanPreset ocean;
	public TerracingPreset terracing;
	public PerlinPreset perlin;
	public GeologyPreset geology;
	public ReplacerPreset replacer;
	public string[] prefabPaths;

    public EditorSettings
    (
        string rustDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Rust", float prefabRenderDistance = 700f, float pathRenderDistance = 200f, 
        float waterTransparency = 0.2f, bool loadbundleonlaunch = false, bool terrainTextureSet = false, bool style = true, CrazingPreset crazing = new CrazingPreset(), PerlinSplatPreset perlinSplat = new PerlinSplatPreset(),
		RipplePreset ripple = new RipplePreset(), OceanPreset ocean = new OceanPreset(), TerracingPreset terracing = new TerracingPreset(), PerlinPreset perlin = new PerlinPreset(), GeologyPreset geology = new GeologyPreset(), 
		ReplacerPreset replacer = new ReplacerPreset())
        {
            this.rustDirectory = rustDirectory;
            this.prefabRenderDistance = prefabRenderDistance;
            this.pathRenderDistance = pathRenderDistance;
            this.waterTransparency = waterTransparency;
            this.loadbundleonlaunch = loadbundleonlaunch;
            this.terrainTextureSet = terrainTextureSet;
			this.style = style;
			this.crazing = crazing;
			this.perlinSplat = perlinSplat;
            this.prefabPaths = SettingsManager.PrefabPaths;
			this.ripple = ripple;
			this.ocean = ocean;
			this.terracing = terracing;
			this.perlin = perlin;
			this.geology = geology;
			this.replacer = replacer;
        }
}