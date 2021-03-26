using UnityEditor;
using UnityEngine;
using RustMapEditor.UI;
using RustMapEditor.Variables;

public class MapManagerWindow : EditorWindow
{
    #region Values
    int mainMenuOptions = 0, mapToolsOptions = 0, heightMapOptions = 0, conditionalPaintOptions = 0, generatorOptions = 0;
    float offset = 0f, heightSet = 500f, heightLow = 450f, heightHigh = 750f;
    bool clampOffset = true, autoUpdate = false;
    float normaliseLow = 450f, normaliseHigh = 1000f;
    Conditions conditions = new Conditions() 
    { 
        GroundConditions = new GroundConditions(TerrainSplat.Enum.Grass), BiomeConditions = new BiomeConditions(TerrainBiome.Enum.Temperate), TopologyConditions = new TopologyConditions(TerrainTopology.Enum.Beach)
    };
    
	int texture = 0, smoothPasses = 0;
    Vector2 scrollPos = new Vector2(0, 0);
    Selections.Objects rotateSelection;
    float terraceErodeFeatureSize = 150f, terraceErodeInteriorCornerWeight = 1f, blurDirection = 0f, filterStrength = 1f;
	
	GeologyPreset activePreset = new GeologyPreset();
	
	int presetIndex = 0;
	int macroIndex = 0;
	string macroTitle = "";
	
	float tttWeight = .7f;
	
	string [] geologyList = SettingsManager.GetPresetTitles("Presets/Geology/");
	string [] macroList = SettingsManager.GetPresetTitles("Presets/Geology/Macros/");
	

	int layerIndex = (int)TerrainManager.LandLayer;
	int prefabIndex= 0, thicc = 3;
    bool aboveTerrain = false;
	
	Layers layers = new Layers() { Ground = TerrainSplat.Enum.Grass, Biome = TerrainBiome.Enum.Temperate, Topologies = TerrainTopology.Enum.Field };
	Layers sourceLayers = new Layers() { Ground = TerrainSplat.Enum.Grass, Biome = TerrainBiome.Enum.Temperate, Topologies = TerrainTopology.Enum.Field };
	
    SlopesInfo slopesInfo = new SlopesInfo() { SlopeLow = 40f, SlopeHigh = 60f, SlopeBlendLow = 25f, SlopeBlendHigh = 75f, BlendSlopes = false };
    HeightsInfo heightsInfo = new HeightsInfo() { HeightLow = 400f, HeightHigh = 600f, HeightBlendLow = 300f, HeightBlendHigh = 700f, BlendHeights = false };

	CrazingPreset crazing = new CrazingPreset();
	PerlinSplatPreset perlinSplat = new PerlinSplatPreset();
	OceanPreset ocean = new OceanPreset();
	RipplePreset ripple = new RipplePreset();
	TerracingPreset terracing = new TerracingPreset();
	PerlinPreset perlin = new PerlinPreset();
	
	string macroDisplay;
	
	int mapSize = 3000;
	float landHeight = .505f;
    #endregion

    public void OnGUI()
    {
		
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        GUIContent[] mainMenu = new GUIContent[6];
        mainMenu[0] = new GUIContent("File");
        mainMenu[1] = new GUIContent("Settings");
        mainMenu[2] = new GUIContent("Prefabs");
		mainMenu[3] = new GUIContent("Layers");
        mainMenu[4] = new GUIContent("Generator");
        mainMenu[5] = new GUIContent("Advanced");

        mainMenuOptions = GUILayout.Toolbar(mainMenuOptions, mainMenu, EditorStyles.toolbarButton);
		
		Functions.SaveSettings();
		
        #region Menu
        switch (mainMenuOptions)
        {
            #region File
            case 0:
                Functions.EditorIO();
				Functions.NewMapOptions(ref mapSize, ref landHeight, ref layers);
				Functions.MapInfo();
                break;
            #endregion
            #region Prefabs
			case 1:
				Functions.EditorSettings();
				Functions.EditorInfo();

                Functions.EditorLinks();
				break;
            case 2:
				GUIContent[] prefabsMenu = new GUIContent[2];
				prefabsMenu[0] = new GUIContent("List");
				prefabsMenu[1] = new GUIContent("Hierarchy");
				
				prefabIndex = GUILayout.Toolbar(prefabIndex, prefabsMenu, EditorStyles.toolbarButton);
					switch (prefabIndex)
					{
					case 0:
					break;
					case 1:
					break;
					}

				break;
            #endregion
            case 3:
            GUIContent[] layersMenu = new GUIContent[4];
            layersMenu[0] = new GUIContent("Ground");
            layersMenu[1] = new GUIContent("Biome");
            layersMenu[2] = new GUIContent("Alpha");
			layersMenu[3] = new GUIContent("Topology");

			EditorGUI.BeginChangeCheck();
			layerIndex = GUILayout.Toolbar(layerIndex, layersMenu, EditorStyles.toolbarButton);
			
			if (EditorGUI.EndChangeCheck())
			{
				Functions.SetLandLayer((LandLayers)layerIndex, TerrainTopology.TypeToIndex((int)layers.Topologies));
			}

				switch (TerrainManager.LandLayer)
				{
					case LandLayers.Ground:
						Functions.TextureSelect((LandLayers)layerIndex, ref layers);

						Functions.LayerTools(TerrainManager.LandLayer, TerrainSplat.TypeToIndex((int)layers.Ground));


						
						crazing = SettingsManager.crazing;
						crazing.splatLayer = TerrainTopology.TypeToIndex((int)layers.Ground);
						Functions.Crazing(ref crazing);
						
						perlinSplat = SettingsManager.perlinSplat;
						perlinSplat.splatLayer = TerrainTopology.TypeToIndex((int)layers.Ground);
						Functions.PerlinSplat(ref perlinSplat);
						
						Functions.HeightTools(TerrainManager.LandLayer, TerrainSplat.TypeToIndex((int)layers.Ground), ref heightsInfo);
						Functions.SlopeTools(TerrainManager.LandLayer, TerrainSplat.TypeToIndex((int)layers.Ground), ref slopesInfo);

						Functions.RiverTools(TerrainManager.LandLayer, TerrainSplat.TypeToIndex((int)layers.Ground), ref aboveTerrain);
						
						break;
					case LandLayers.Biome:
						Functions.TextureSelect((LandLayers)layerIndex, ref layers);

						Functions.LayerTools(TerrainManager.LandLayer, TerrainBiome.TypeToIndex((int)layers.Biome));

						
						Functions.HeightTools(TerrainManager.LandLayer, TerrainBiome.TypeToIndex((int)layers.Biome), ref heightsInfo);
						Functions.SlopeTools(TerrainManager.LandLayer, TerrainBiome.TypeToIndex((int)layers.Biome), ref slopesInfo);
						break;
					case LandLayers.Alpha:
					
						Functions.LayerTools(TerrainManager.LandLayer, 0, 1);

						break;
					case LandLayers.Topology:
					
						Functions.TopologyTools();
						
						Functions.TopologyLayerSelect(ref layers);

						Functions.LayerTools(TerrainManager.LandLayer, 0, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));


						Functions.HeightTools(TerrainManager.LandLayer, 0, ref heightsInfo, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
						Functions.SlopeTools(TerrainManager.LandLayer, 0, ref slopesInfo, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
						
						Functions.Combinator(ref layers, ref sourceLayers, ref tttWeight, ref thicc);
						
						
						Functions.RiverTools(TerrainManager.LandLayer, 0, ref aboveTerrain, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
						Functions.LakeOcean(ref layers);
						break;
				}
			
            switch (mapToolsOptions)
            {
				/*
                #region HeightMap
                case 0:
                    GUIContent[] heightMapMenu = new GUIContent[2];
                    heightMapMenu[0] = new GUIContent("Heights");
                    heightMapMenu[1] = new GUIContent("Filters");
                    heightMapOptions = GUILayout.Toolbar(heightMapOptions, heightMapMenu, EditorStyles.toolbarButton);
            
                    switch (heightMapOptions)
                    {
                        case 0:
                            Elements.BoldLabel(ToolTips.heightsLabel);
                            Functions.OffsetMap(ref offset, ref clampOffset);
                            Functions.SetHeight(ref heightSet);
                            Functions.ClampHeight(ref heightLow, ref heightHigh);
                            Elements.BoldLabel(ToolTips.miscLabel);
                            Functions.InvertMap();
                            break;
                        case 1:
                            Functions.NormaliseMap(ref normaliseLow, ref normaliseHigh, ref autoUpdate);
                            Functions.SmoothMap(ref filterStrength, ref blurDirection, ref smoothPasses);
                            Functions.TerraceMap(ref terraceErodeFeatureSize, ref terraceErodeInteriorCornerWeight);
                            break;
                    }
                    break;
                #endregion
				
                #region Textures
                case 0:
                    Functions.ConditionalPaint(ref conditionalPaintOptions, ref texture, ref conditions, ref layers);
                    break;
                #endregion

                #region Misc
                case 2:
                    Functions.RotateMap(ref rotateSelection);
                    break;
                    #endregion
					*/
            }
            break;
		case 4:
			GUIContent[] generatorMenu = new GUIContent[2];
				generatorMenu[0] = new GUIContent("Heightmap");
				generatorMenu[1] = new GUIContent("Geology");
				
				generatorOptions = GUILayout.Toolbar(generatorOptions, generatorMenu, EditorStyles.toolbarButton);
				switch (generatorOptions)
				{
						case 0:
							Functions.SetHeight(ref heightSet);
							Functions.OffsetMap(ref offset, ref clampOffset);
						
							perlin = SettingsManager.perlin;
							Functions.PerlinTerrain(ref perlin);
							
							Functions.NormaliseMap(ref normaliseLow, ref normaliseHigh, ref autoUpdate);
							
							ocean = SettingsManager.ocean;
							Functions.Ocean(ref ocean);
							
							ripple = SettingsManager.ripple;
							Functions.Ripple(ref ripple);
							
							terracing = SettingsManager.terracing;
							Functions.RandomTerracing(ref terracing);
							
						break;

						case 1:
								Functions.Geology(ref activePreset, ref presetIndex, ref geologyList, ref macroIndex, ref macroList, ref macroTitle, ref macroDisplay);
						break;

				
				}
		break;
		
		case 5:
			Functions.Merger();
		break;
		
        }
        #endregion
        EditorGUILayout.EndScrollView();
    }
}