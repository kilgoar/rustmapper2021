using System;
using UnityEngine;
using UnityEditor;
using RustMapEditor.Variables;
using static TerrainManager;

namespace RustMapEditor.UI
{
	public enum BIOMES
	{
    TEMPERATE = 0,
    ARID = 1,
    TUNDRA = 2,
	ARCTIC = 3
	}
    public static class Functions
    {
        #region MainMenu
	
		public static void refabs(ref Vector3 prefabsOffset, ref string mark)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Monument Marker"))
			{
				PrefabManager.addMonumentMarker(mark);
			}
			mark = EditorGUILayout.TextField("Monument Name", mark);
			EditorGUILayout.EndHorizontal();
			if (GUILayout.Button("Remove All Electric Circuits"))
			{
				PrefabManager.removeElectrics(PrefabManager.CurrentMapPrefabs, PrefabManager.CurrentMapElectrics);
				
			}
			if (GUILayout.Button("Keep Only Electric Circuits"))
			{
				PrefabManager.keepElectrics(PrefabManager.CurrentMapPrefabs, PrefabManager.CurrentMapElectrics);
				
			}

			prefabsOffset = EditorGUILayout.Vector3Field("Offset:", prefabsOffset);
			
			
			if (GUILayout.Button("Offset Prefabs"))
			{
				PrefabManager.Offset(PrefabManager.CurrentMapPrefabs, PrefabManager.CurrentMapElectrics, prefabsOffset);
				
			}
			if (GUILayout.Button("Place .Prefab"))
			{
				MergeOffsetCustomPrefabPanel();
			}
			if (GUILayout.Button("Decompress lz4"))
					DecompressMapPanel();
				if (GUILayout.Button("Rip Custom Prefab into json"))
					LoadCustomPrefabJSONPanel();
		}
	
		public static void Combinator(ref Layers layers, ref Layers sourceLayers, ref float tttWeight, ref int thicc)
		{
			EditorGUILayout.Space();
			GUILayout.Label("Topology Combinator", EditorStyles.boldLabel);
			sourceTopologyLayerSelect(ref sourceLayers);
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();		
					if (GUILayout.Button("Copy"))
					{
						GenerativeManager.copyTopologyLayer(layers, sourceLayers);
					}
					
					if (GUILayout.Button("Erase Overlaps"))
					{
						GenerativeManager.notTopologyLayer(layers, sourceLayers);
					}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
						if (GUILayout.Button("Outline"))
						{
                            GenerativeManager.paintTopologyOutline(layers, sourceLayers, thicc);
						}
						thicc = EditorGUILayout.IntField("Thickness:", thicc);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			GUILayout.Label("Ground Combinator", EditorStyles.boldLabel);
			sourceTextureSelect(ref sourceLayers);
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
				
						if (GUILayout.Button("Fill"))
						{
                            GenerativeManager.terrainToTopology(layers, sourceLayers, tttWeight);
						}
						tttWeight = GUILayout.HorizontalSlider(tttWeight, 0.7f, .1f);
			EditorGUILayout.EndHorizontal();
		}
		
		public static void LakeOcean(ref Layers layers)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Fill Lakes"))
					{
						GenerativeManager.lakeTopologyFill(layers);
					}
			if (GUILayout.Button("Fill Oceans"))
					{
						GenerativeManager.oceanTopologyFill(layers);
					}
			EditorGUILayout.EndHorizontal();
		}
	
		public static void Crazing(ref CrazingPreset preset)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Space();
			GUILayout.Label("Mottling", EditorStyles.boldLabel);
			preset.zones = EditorGUILayout.IntField("Patches", preset.zones);
			
			EditorGUILayout.BeginHorizontal();
			preset.minSize = EditorGUILayout.IntField("Minimum size", preset.minSize);
			preset.maxSize = EditorGUILayout.IntField("Maximum size", preset.maxSize);
			EditorGUILayout.EndHorizontal();
			
			if (GUILayout.Button("Apply"))
					{
						GenerativeManager.splatCrazing(preset);
						SettingsManager.crazing = preset;
						SettingsManager.SaveSettings();
					}
			if (EditorGUI.EndChangeCheck())
			{
				SettingsManager.crazing = preset;

			}				
		}
		
		public static void Ocean(ref OceanPreset ocean)
		{
			

			
			EditorGUI.BeginChangeCheck();
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Ocean Maker", EditorStyles.boldLabel);
					
					ocean.radius = EditorGUILayout.IntField("Island size", ocean.radius);
					ocean.gradient = EditorGUILayout.IntField("Shore size", ocean.gradient);
					ocean.seafloor = EditorGUILayout.IntField("Seafloor", ocean.seafloor);
					
					EditorGUILayout.BeginHorizontal();
					ocean.xOffset = EditorGUILayout.IntField("Z center", ocean.xOffset);
					ocean.yOffset = EditorGUILayout.IntField("Y center", ocean.yOffset);
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					ocean.perlin = EditorGUILayout.ToggleLeft("Channels", ocean.perlin);
					if (ocean.perlin)
					{
						ocean.s = EditorGUILayout.IntField("Channel Size", ocean.s);
					}
					
					EditorGUILayout.EndHorizontal();
					
					if (GUILayout.Button("Apply"))
					{
							SettingsManager.ocean = ocean;
                            GenerativeManager.oceans(ocean);
							SettingsManager.SaveSettings();
					}
			if (EditorGUI.EndChangeCheck())
				{
					SettingsManager.ocean = ocean;
				}	
		}
		
		public static void Ripple(ref RipplePreset rippling)
		{
			EditorGUI.BeginChangeCheck();
			GUILayout.Label("Ripple", EditorStyles.boldLabel);
						
						rippling.size = EditorGUILayout.IntField("Scale", rippling.size);
						rippling.density = EditorGUILayout.IntField("Density", rippling.density);
						rippling.weight = EditorGUILayout.FloatField("Strength", rippling.weight);
						
						if (GUILayout.Button("Apply"))
							{
                                GenerativeManager.rippledFiguring(rippling);
								SettingsManager.ripple = rippling;
								SettingsManager.SaveSettings();
							}
				if (EditorGUI.EndChangeCheck())
				{
					SettingsManager.ripple = rippling;
				}		
		}
		
		public static void PerlinTerrain(ref PerlinPreset perlin)
		{
				EditorGUI.BeginChangeCheck();
					EditorGUILayout.LabelField("Generate Smooth Terrain", EditorStyles.boldLabel);
					perlin.layers = EditorGUILayout.IntField("Layers:", perlin.layers);
					perlin.period = EditorGUILayout.IntField("Period:", perlin.period);
					perlin.scale = EditorGUILayout.IntField("Scale:", perlin.scale);
					perlin.simple = EditorGUILayout.Toggle("Simple", perlin.simple);
							if (GUILayout.Button("Apply"))
							{
								SettingsManager.perlin = perlin;
								SettingsManager.SaveSettings();
								
								if (perlin.simple)
								{
									GenerativeManager.perlinSimple(perlin);
								}
								else
								{
									GenerativeManager.perlinRidiculous(perlin);
								}
							}
				if (EditorGUI.EndChangeCheck())
				{
					SettingsManager.perlin = perlin;
				}	
		}
		
		public static void RandomTerracing(ref TerracingPreset terracing)
		{
			EditorGUI.BeginChangeCheck();
					EditorGUILayout.LabelField("Random Terracing", EditorStyles.boldLabel);
					
					EditorGUILayout.BeginHorizontal();
					terracing.gates = EditorGUILayout.IntField("Terraces", terracing.gates);
					terracing.zStart = EditorGUILayout.IntField("Starting height", terracing.zStart);
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					terracing.gateBottom = EditorGUILayout.IntField("Shortest", terracing.gateBottom);
					terracing.gateTop = EditorGUILayout.IntField("Tallest", terracing.gateTop);
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Weight",GUILayout.MaxWidth(50));
					terracing.weight = GUILayout.HorizontalSlider(terracing.weight, 0.0f, 1f);
					EditorGUILayout.EndHorizontal();
					
					
					
					
					EditorGUILayout.BeginHorizontal();
					
					terracing.circular = EditorGUILayout.Toggle("Smooth", terracing.circular);
					
					terracing.perlinBanks = EditorGUILayout.Toggle("Density", terracing.perlinBanks);
					terracing.perlinDensity = EditorGUILayout.IntField("", terracing.perlinDensity);
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					terracing.flatten = EditorGUILayout.Toggle("Basins", terracing.flatten);
					if (terracing.flatten)
					{
					terracing.descaleFactor = EditorGUILayout.IntField("Basin Flatness", terracing.descaleFactor);
					}
					EditorGUILayout.EndHorizontal();
					
					if (GUILayout.Button("Apply"))
					{
							SettingsManager.terracing = terracing;
							GenerativeManager.randomTerracing(terracing);
							SettingsManager.SaveSettings();
					}
			if (EditorGUI.EndChangeCheck())
				{
					SettingsManager.terracing = terracing;
				}	
		}
		
		public static void PerlinSplat(ref PerlinSplatPreset perlinSplat)
		{

			
					EditorGUI.BeginChangeCheck();
					
							EditorGUILayout.Space();
							GUILayout.Label("Smooth mottling", EditorStyles.boldLabel);
							
							perlinSplat.scale = EditorGUILayout.IntField("Scale", perlinSplat.scale);
							perlinSplat.strength = EditorGUILayout.FloatField("Strength", perlinSplat.strength);
							
							EditorGUILayout.BeginHorizontal();
							perlinSplat.paintBiome = EditorGUILayout.Toggle("Paint on Biome", perlinSplat.paintBiome);
							perlinSplat.invert = EditorGUILayout.Toggle("Invert", perlinSplat.invert);
							EditorGUILayout.EndHorizontal();
							
							//cnds.BiomeConditions.CheckLayer[TerrainBiome.TypeToIndex((int)cnds.BiomeConditions.Layer)] = Elements.ToolbarToggle(ToolTips.checkTexture, cnds.BiomeConditions.CheckLayer[TerrainBiome.TypeToIndex((int)cnds.BiomeConditions.Layer)]);

							
							perlinSplat.biomeLayer = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Target Biome:", perlinSplat.biomeLayer);
							if (GUILayout.Button("Apply"))
							{
                                GenerativeManager.perlinSplat(perlinSplat);
								SettingsManager.perlinSplat = perlinSplat;
								SettingsManager.SaveSettings();
							}
				if (EditorGUI.EndChangeCheck())
				{
					SettingsManager.perlinSplat = perlinSplat;
				}		
		}
	
		public static void Replacer(ref ReplacerPreset replacer, ref int replacerPresetIndex, ref string [] replacerList)
		{
			GUILayout.Label("Batch Prefab Replacer:", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID0 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID0);
			replacer.replaceID0 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID0);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID1 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID1);
			replacer.replaceID1 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID1);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID2 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID2);
			replacer.replaceID2 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID2);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID3 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID3);
			replacer.replaceID3 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID3);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID4 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID4);
			replacer.replaceID4 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID4);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID5 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID5);
			replacer.replaceID5 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID5);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID6 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID6);
			replacer.replaceID6 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID6);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID7 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID7);
			replacer.replaceID7 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID7);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID8 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID8);
			replacer.replaceID8 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID8);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID9 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID9);
			replacer.replaceID9 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID9);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID10 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID10);
			replacer.replaceID10 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID10);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID11 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID11);
			replacer.replaceID11 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID11);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID12 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID12);
			replacer.replaceID12 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID12);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID13 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID13);
			replacer.replaceID13 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID13);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID14 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID14);
			replacer.replaceID14 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID14);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID15 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID15);
			replacer.replaceID15 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID15);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			replacer.prefabID16 = (uint)EditorGUILayout.LongField("Search ID:", replacer.prefabID16);
			replacer.replaceID16 = (uint)EditorGUILayout.LongField("Replace ID:", replacer.replaceID16);
			EditorGUILayout.EndHorizontal();
			
			if (GUILayout.Button("Delete prefabs"))
						{
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID0);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID1);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID2);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID3);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID4);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID5);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID6);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID7);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID8);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID9);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID10);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID11);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID12);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID13);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID14);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID15);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, replacer.prefabID16);
						}
			
			if (GUILayout.Button("Keep prefabs"))
						{
							uint[] prefabList = new uint[]{replacer.prefabID0,replacer.prefabID1,
							replacer.prefabID2,replacer.prefabID3,replacer.prefabID4,
							replacer.prefabID5,replacer.prefabID6,replacer.prefabID7,
							replacer.prefabID8,replacer.prefabID9,replacer.prefabID10,
							replacer.prefabID11,replacer.prefabID12,replacer.prefabID13,
							replacer.prefabID14,replacer.prefabID15,replacer.prefabID16};
							PrefabManager.keepPrefabList(PrefabManager.CurrentMapPrefabs, prefabList);
						}
			
			replacer.rotateToTerrain = EditorGUILayout.ToggleLeft("Rotate to terrain", replacer.rotateToTerrain, GUILayout.MaxWidth(250));
			replacer.rotateToX = EditorGUILayout.ToggleLeft("Rotate to x", replacer.rotateToX, GUILayout.MaxWidth(250));
			replacer.rotateToY = EditorGUILayout.ToggleLeft("Rotate to y", replacer.rotateToY, GUILayout.MaxWidth(250));
			replacer.rotateToZ = EditorGUILayout.ToggleLeft("Rotate to z", replacer.rotateToZ, GUILayout.MaxWidth(250));
			
			replacer.scale = EditorGUILayout.ToggleLeft("Modify Scale", replacer.scale, GUILayout.MaxWidth(250));
			replacer.scaling = EditorGUILayout.Vector3Field("Scaling multiplier", replacer.scaling);
			
			EditorGUI.BeginChangeCheck();
						
			replacerPresetIndex = EditorGUILayout.Popup("Presets:", replacerPresetIndex, replacerList);
						
						if (EditorGUI.EndChangeCheck())
						{
							replacer.title = replacerList[replacerPresetIndex];
							SettingsManager.LoadReplacerPreset(replacer.title);
							replacer = SettingsManager.replacer;
							replacerList = SettingsManager.GetPresetTitles("Presets/Replacer/");
						}
					
					
					if (EditorGUI.EndChangeCheck())
					{
						SettingsManager.replacer = replacer;
					}
					
					replacer.title = EditorGUILayout.TextField("Preset Name", replacer.title);
					
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Save"))
						{
							SettingsManager.replacer = replacer;
                            SettingsManager.SaveReplacerPreset();
							SettingsManager.LoadPresets();							
							replacerList = SettingsManager.GetPresetTitles("Presets/Replacer/");
						}
					if (GUILayout.Button("Apply"))
						{
                            SettingsManager.replacer = replacer;
							PrefabManager.BatchReplace(PrefabManager.CurrentMapPrefabs, replacer);
						}

					EditorGUILayout.EndHorizontal();
			
			if (EditorGUI.EndChangeCheck())
					{
						SettingsManager.replacer = replacer;					
					}
			
		}
		
		public static void Geology(ref GeologyPreset activePreset, ref int presetIndex, ref string [] geologyList, ref int macroIndex, ref string [] macroList, ref string macroTitle, ref string macroDisplay, ref Layers layers)
		{
					EditorGUI.BeginChangeCheck();
					

					GUILayout.Label("Feature Attributes", EditorStyles.boldLabel);

					activePreset.prefabID = (uint)EditorGUILayout.LongField("PREFAB ID:", activePreset.prefabID);
					activePreset.prefabID1 = (uint)EditorGUILayout.LongField("PREFAB ID:", activePreset.prefabID1);
					activePreset.prefabID2 = (uint)EditorGUILayout.LongField("PREFAB ID:", activePreset.prefabID2);
					activePreset.prefabID3 = (uint)EditorGUILayout.LongField("PREFAB ID:", activePreset.prefabID3);
					activePreset.prefabID4 = (uint)EditorGUILayout.LongField("PREFAB ID:", activePreset.prefabID4);
					activePreset.prefabID5 = (uint)EditorGUILayout.LongField("PREFAB ID:", activePreset.prefabID5);
					activePreset.prefabID6 = (uint)EditorGUILayout.LongField("PREFAB ID:", activePreset.prefabID6);
					activePreset.prefabID7 = (uint)EditorGUILayout.LongField("PREFAB ID:", activePreset.prefabID7);
					
					if (GUILayout.Button("Delete prefabs"))
						{
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, activePreset.prefabID);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, activePreset.prefabID1);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, activePreset.prefabID2);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, activePreset.prefabID3);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, activePreset.prefabID4);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, activePreset.prefabID5);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, activePreset.prefabID6);
							PrefabManager.deletePrefabIDs(PrefabManager.CurrentMapPrefabs, activePreset.prefabID7);
						}
					
					GUILayout.Label("Rotation range:", EditorStyles.boldLabel);					
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("x",GUILayout.MaxWidth(10));
					EditorGUILayout.MinMaxSlider(ref activePreset.rotationsLow.x, ref activePreset.rotationsHigh.x, 0f, 360f);
					EditorGUILayout.LabelField(activePreset.rotationsLow.x.ToString("0.#") + " - " + activePreset.rotationsHigh.x.ToString("0.#") ,GUILayout.MaxWidth(80));
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("y",GUILayout.MaxWidth(10));
					EditorGUILayout.MinMaxSlider(ref activePreset.rotationsLow.y, ref activePreset.rotationsHigh.y, 0f, 360f);
					EditorGUILayout.LabelField(activePreset.rotationsLow.y.ToString("0.#") + " - " + activePreset.rotationsHigh.y.ToString("0.#") ,GUILayout.MaxWidth(80));
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("z",GUILayout.MaxWidth(10));
					EditorGUILayout.MinMaxSlider(ref activePreset.rotationsLow.z, ref activePreset.rotationsHigh.z, 0f, 360f);
					EditorGUILayout.LabelField(activePreset.rotationsLow.z.ToString("0.#") + " - " + activePreset.rotationsHigh.z.ToString("0.#") ,GUILayout.MaxWidth(80));
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					activePreset.normalizeX = EditorGUILayout.ToggleLeft("Align to X", activePreset.normalizeX, GUILayout.MaxWidth(250));
					activePreset.normalizeY = EditorGUILayout.ToggleLeft("Align to Y", activePreset.normalizeY, GUILayout.MaxWidth(250));
					activePreset.normalizeZ = EditorGUILayout.ToggleLeft("Align to Z", activePreset.normalizeZ, GUILayout.MaxWidth(250));
					EditorGUILayout.EndHorizontal();
					
					GUILayout.Label("Scale range:", EditorStyles.boldLabel);

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("x",GUILayout.MaxWidth(10));
					EditorGUILayout.MinMaxSlider(ref activePreset.scalesLow.x, ref activePreset.scalesHigh.x, 0f, 30f);
					EditorGUILayout.LabelField(activePreset.scalesLow.x.ToString("0.0#") + " - " + activePreset.scalesHigh.x.ToString("0.0#") ,GUILayout.MaxWidth(80));
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("y",GUILayout.MaxWidth(10));
					EditorGUILayout.MinMaxSlider(ref activePreset.scalesLow.y, ref activePreset.scalesHigh.y, 0f, 30f);
					
					EditorGUILayout.LabelField(activePreset.scalesLow.y.ToString("0.0#") + " - " + activePreset.scalesHigh.y.ToString("0.0#") ,GUILayout.MaxWidth(80));
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("z",GUILayout.MaxWidth(10));
					EditorGUILayout.MinMaxSlider(ref activePreset.scalesLow.z, ref activePreset.scalesHigh.z, 0f, 30f);
					
					EditorGUILayout.LabelField(activePreset.scalesLow.z.ToString("0.0#") + " - " + activePreset.scalesHigh.z.ToString("0.0#") ,GUILayout.MaxWidth(80));
					EditorGUILayout.EndHorizontal();
					
					
					GUILayout.Label("Placement", EditorStyles.boldLabel);
					
							EditorGUILayout.BeginHorizontal();
							activePreset.biomeExclusive = EditorGUILayout.ToggleLeft("Place on biome:", activePreset.biomeExclusive, GUILayout.MaxWidth(250));
							layers.Biome = (TerrainBiome.Enum)Elements.ToolbarEnumPopup(layers.Biome);
							activePreset.biomeIndex = TerrainBiome.TypeToIndex((int)layers.Biome);
							EditorGUILayout.EndHorizontal();
						
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Slope Range: " + activePreset.slopeLow.ToString("0.0#") + "° - " + activePreset.slopeHigh.ToString("0.0#") + "°");
                            EditorGUILayout.EndHorizontal();
							
                            EditorGUILayout.MinMaxSlider(ref activePreset.slopeLow, ref activePreset.slopeHigh, 0f, 90f);
							
							EditorGUILayout.BeginHorizontal();
							activePreset.avoidTopo = EditorGUILayout.ToggleLeft("Dodge road and monument topology", activePreset.avoidTopo, GUILayout.MaxWidth(250));
							EditorGUILayout.EndHorizontal();
				
					activePreset.zOffset = EditorGUILayout.FloatField("Height Offset", activePreset.zOffset);
					activePreset.density = EditorGUILayout.IntField("Density", activePreset.density);
					activePreset.frequency = EditorGUILayout.IntField("Frequency", activePreset.frequency);
					activePreset.floor = EditorGUILayout.IntField("Floor", activePreset.floor);
					activePreset.ceiling = EditorGUILayout.IntField("Ceiling", activePreset.ceiling);
					
					
					GUILayout.Label("Styling", EditorStyles.boldLabel);
					activePreset.flipping = EditorGUILayout.ToggleLeft("Flipping", activePreset.flipping);
					activePreset.tilting = EditorGUILayout.ToggleLeft("Geological shift", activePreset.tilting);
					
					
					
					

					
					EditorGUILayout.Space();

					
					
					EditorGUI.BeginChangeCheck();
						
					presetIndex = EditorGUILayout.Popup("Presets:", presetIndex, geologyList);
						
						if (EditorGUI.EndChangeCheck())
						{
							activePreset.title = geologyList[presetIndex];
							SettingsManager.LoadGeologyPreset(activePreset.title);
							activePreset = SettingsManager.geology;
							geologyList = SettingsManager.GetPresetTitles("Presets/Geology/");
						}
					
					
					if (EditorGUI.EndChangeCheck())
					{
						SettingsManager.geology = activePreset;					
					}
					
					activePreset.title = EditorGUILayout.TextField("Preset Name", activePreset.title);
					
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Save"))
						{
							SettingsManager.geology = activePreset;
                            SettingsManager.SaveGeologyPreset();
							SettingsManager.LoadPresets();							
							geologyList = SettingsManager.GetPresetTitles("Presets/Geology/");
						}
					if (GUILayout.Button("Apply"))
						{
                            GenerativeManager.insertPrefabCliffs(activePreset);
							SettingsManager.geology = activePreset;
						}

					EditorGUILayout.EndHorizontal();
				
					EditorGUILayout.Space();
					
					
					EditorGUI.BeginChangeCheck();
						macroIndex = EditorGUILayout.Popup("Multi Presets:", macroIndex, macroList);
					if (EditorGUI.EndChangeCheck())
					{
						macroTitle = macroList[macroIndex];
						macroDisplay = "PRESETS";
						SettingsManager.LoadGeologyMacro(macroTitle);				
						
						foreach (GeologyPreset pre in SettingsManager.macro)
						{
							macroDisplay += " - " + pre.title;
						}
					}
					
					GUILayout.Label(macroDisplay, EditorStyles.boldLabel);
					
					macroTitle = EditorGUILayout.TextField("Multi Preset Name", macroTitle);
					
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Add"))
						{
							SettingsManager.AddToMacro(macroTitle);
							macroList = SettingsManager.GetPresetTitles("Presets/Geology/Macros/");
							
						}
					if (GUILayout.Button("Apply"))
						{
							foreach (GeologyPreset pre in SettingsManager.macro)
							{
								GenerativeManager.insertPrefabCliffs(pre);
							}
						}
					EditorGUILayout.EndHorizontal();
		}
		
        public static void EditorIO(string mapName = "")
        {
            
            LoadMap();
            SaveMap(mapName);
			
        }

        public static void LoadMap()
        {
            if (Elements.ToolbarButton(ToolTips.loadMap))
                LoadMapPanel();
			
        }
		
		public static void CustomPrefab()
		{
			EditorGUILayout.Space();
			GUILayout.Label("Custom Prefab Editor");
			if (GUILayout.Button("Load"))
				LoadCustomPrefabPanel();
			
			if (GUILayout.Button("Load Merge"))
				MergeCustomPrefabPanel();
			
			if (GUILayout.Button("Save"))
				SaveCustomPrefabPanel();
			
			
				
			
		}

        public static void SaveCustomPrefabPanel(string mapName = "customprefab")
        {
            string saveFile = "";
            saveFile = EditorUtility.SaveFilePanel("Save Prefab File", saveFile, mapName, "prefab");
            if (string.IsNullOrEmpty(saveFile))
                return;
            MapManager.SaveCustomPrefab(saveFile);
        }

        public static void LoadMapPanel()
        {
            string loadFile = "";
            loadFile = EditorUtility.OpenFilePanel("Import Map File", loadFile, "map");
            if (string.IsNullOrEmpty(loadFile))
                return;
            var world = new WorldSerialization();
            world.Load(loadFile);
            MapManager.Load(WorldConverter.WorldToTerrain(world), loadFile);
            ReloadTreeViews();

        }
		public static void LoadCustomPrefabPanel()
        {
            string loadFile = "";
            loadFile = EditorUtility.OpenFilePanel("Import Custom Prefab", loadFile, "prefab");
            if (string.IsNullOrEmpty(loadFile))
                return;
            var world = new WorldSerialization();
            world.LoadREPrefab(loadFile);
            MapManager.LoadREPrefab(WorldConverter.WorldToREPrefab(world), loadFile);
            ReloadTreeViews();

        }

		public static void MergeCustomPrefabPanel()
        {
            string loadFile = "";
            loadFile = EditorUtility.OpenFilePanel("Import Custom Prefab", loadFile, "prefab");
            if (string.IsNullOrEmpty(loadFile))
                return;
            var world = new WorldSerialization();
            world.LoadREPrefab(loadFile);
            MapManager.MergeREPrefab(WorldConverter.WorldToREPrefab(world), loadFile);
            ReloadTreeViews();

       }
	   
	   public static void MergeOffsetCustomPrefabPanel()
        {
            string loadFile = "";
            loadFile = EditorUtility.OpenFilePanel("Import Custom Prefab", loadFile, "prefab");
            if (string.IsNullOrEmpty(loadFile))
                return;
            var world = new WorldSerialization();
            world.LoadREPrefab(loadFile);
            MapManager.MergeOffsetREPrefab(WorldConverter.WorldToREPrefab(world), GameObject.Find("Placement").transform, loadFile);
            ReloadTreeViews();

       }
	   
	   
		public static void DecompressMapPanel()
		{
			string loadFile = "";
            loadFile = EditorUtility.OpenFilePanel("Decompress lz4 file", loadFile, "prefab");
            if (string.IsNullOrEmpty(loadFile))
                return;
			var world = new WorldSerialization();
            world.Decompress(loadFile);
			ReloadTreeViews();
		}
		
		public static void LoadCustomPrefabJSONPanel()
		{
			string loadFile = "";
            loadFile = EditorUtility.OpenFilePanel("Decompress lz4 file", loadFile, "prefab");
            if (string.IsNullOrEmpty(loadFile))
                return;
			var world = new WorldSerialization();
            world.LoadREPrefab(loadFile);
			world.SavePrefabJSON("test.json");
			ReloadTreeViews();
		}
		
        public static void SaveMap(string mapName = "")
        {
            if (Elements.ToolbarButton(ToolTips.saveMap))
                SaveMapPanel(mapName);
        }

        public static void SaveMapPanel(string mapName = "")
        {
            string saveFile = "";
            saveFile = EditorUtility.SaveFilePanel("Save Map File", saveFile, mapName, "map");
            if (string.IsNullOrEmpty(saveFile))
                return;
            MapManager.Save(saveFile);
        }

        public static void NewMap()
        {
            if (Elements.ToolbarButton(ToolTips.newMap))
                NewMapPanel();
        }

        public static void NewMapPanel()
        {
            CreateMapWindow.Init();
        }

        public static void MapInfo()
        {
            if (Land != null)
            {
                Elements.BoldLabel(ToolTips.mapInfoLabel);
                GUILayout.Label("Size: " + Land.terrainData.size.x);
                GUILayout.Label("HeightMap: " + Land.terrainData.heightmapResolution + "x" + Land.terrainData.heightmapResolution);
                GUILayout.Label("SplatMap: " + Land.terrainData.alphamapResolution + "x" + Land.terrainData.alphamapResolution);
            }
        }

        public static void EditorInfo()
        {
            Elements.BoldLabel(ToolTips.editorInfoLabel);
            Elements.Label(ToolTips.systemOS);
            Elements.Label(ToolTips.systemRAM);
            Elements.Label(ToolTips.unityVersion);
            Elements.Label(ToolTips.editorVersion);
        }

        public static void EditorLinks()
        {
            Elements.BoldLabel(ToolTips.editorLinksLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.reportBug))
                ShortcutManager.OpenReportBug();
            if (Elements.ToolbarButton(ToolTips.requestFeature))
                ShortcutManager.OpenRequestFeature();
            if (Elements.ToolbarButton(ToolTips.roadMap))
                ShortcutManager.OpenRoadMap();
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.wiki))
                ShortcutManager.OpenWiki();
            if (Elements.ToolbarButton(ToolTips.discord))
                ShortcutManager.OpenDiscord();
            Elements.EndToolbarHorizontal();
        }

        public static void EditorSettings()
        {
            Elements.BoldLabel(ToolTips.editorSettingsLabel);

			EditorGUI.BeginChangeCheck();
				SettingsManager.style = EditorGUILayout.ToggleLeft(new GUIContent("Ingame Textures"), SettingsManager.style);
			if (EditorGUI.EndChangeCheck())
			{
				TerrainManager.GetTextures();
                TerrainManager.ChangeLandLayer(LandLayers.Ground, 0);
			}

            Elements.MiniBoldLabel(ToolTips.rustDirectory);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.browseRustDirectory))
            {
                var returnDirectory = EditorUtility.OpenFolderPanel("Browse Rust Directory", SettingsManager.RustDirectory, "Rust");
                SettingsManager.RustDirectory = String.IsNullOrEmpty(returnDirectory) ? SettingsManager.RustDirectory : returnDirectory;
                ToolTips.rustDirectoryPath.text = SettingsManager.RustDirectory;
            }
            Elements.ToolbarLabel(ToolTips.rustDirectoryPath);
            Elements.EndToolbarHorizontal();
			
			Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.loadBundle))
                AssetManager.Initialise(SettingsManager.RustDirectory + SettingsManager.BundlePathExt);
            if (Elements.ToolbarButton(ToolTips.unloadBundle))
                AssetManager.Dispose();
            Elements.EndToolbarHorizontal();

			Elements.BeginToolbarHorizontal();
            SettingsManager.LoadBundleOnLaunch = Elements.ToolbarCheckBox(ToolTips.loadBundleOnProjectLoad, SettingsManager.LoadBundleOnLaunch);
            Elements.EndToolbarHorizontal();
			

            Elements.MiniBoldLabel(ToolTips.renderDistanceLabel);
            EditorGUI.BeginChangeCheck();
            SettingsManager.PrefabRenderDistance = Elements.ToolbarSlider(ToolTips.prefabRenderDistance, SettingsManager.PrefabRenderDistance, 0, 5000f);
            SettingsManager.PathRenderDistance = Elements.ToolbarSlider(ToolTips.pathRenderDistance, SettingsManager.PathRenderDistance, 0, 5000f);

            if (EditorGUI.EndChangeCheck())
                SceneManager.SetCullingDistances(SceneView.GetAllSceneCameras(), SettingsManager.PrefabRenderDistance, SettingsManager.PathRenderDistance);

            EditorGUI.BeginChangeCheck();
            SettingsManager.WaterTransparency = Elements.ToolbarSlider(ToolTips.waterTransparency, SettingsManager.WaterTransparency, 0f, 0.5f);
            if (EditorGUI.EndChangeCheck())
                SetWaterTransparency(SettingsManager.WaterTransparency);

            
			Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.saveSettings))
                SettingsManager.SaveSettings();
            if (Elements.ToolbarButton(ToolTips.discardSettings))
            {
                SettingsManager.LoadSettings();
                ToolTips.rustDirectoryPath.text = SettingsManager.RustDirectory;
            }
            if (Elements.ToolbarButton(ToolTips.defaultSettings))
                SettingsManager.SetDefaultSettings();
            Elements.EndToolbarHorizontal();

        }
		
		public static void SaveSettings()
		{
			if (GUI.changed)
			{
				SettingsManager.SaveSettings();
			}
		}

        #endregion

        #region Prefabs
        public static void PrefabTools()
        {
            Elements.MiniBoldLabel(ToolTips.toolsLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.deleteMapPrefabs))
                PrefabManager.DeletePrefabs(PrefabManager.CurrentMapPrefabs);
            if (Elements.ToolbarButton(ToolTips.deleteMapPaths))
                PathManager.DeletePaths(PathManager.CurrentMapPaths);
            Elements.EndToolbarHorizontal();
        }

        public static void AssetBundle()
        {
            Elements.MiniBoldLabel(ToolTips.assetBundleLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.loadBundle))
                AssetManager.Initialise(SettingsManager.RustDirectory + SettingsManager.BundlePathExt);
            if (Elements.ToolbarButton(ToolTips.unloadBundle))
                AssetManager.Dispose();
            Elements.EndToolbarHorizontal();
        }
        #endregion

        #region MapTools
        #region HeightMap
        public static void TerraceMap(ref float terraceErodeFeatureSize, ref float terraceErodeInteriorCornerWeight)
        {
            Elements.MiniBoldLabel(ToolTips.terraceLabel);

            terraceErodeFeatureSize = Elements.ToolbarSlider(ToolTips.featureSize, terraceErodeFeatureSize, 2f, 1000f);
            terraceErodeInteriorCornerWeight = Elements.ToolbarSlider(ToolTips.cornerWeight, terraceErodeInteriorCornerWeight, 0f, 1f);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.terraceMap))
                MapManager.TerraceErodeHeightmap(terraceErodeFeatureSize, terraceErodeInteriorCornerWeight);
            Elements.EndToolbarHorizontal();
        }

        public static void SmoothMap(ref float filterStrength, ref float blurDirection, ref int smoothPasses)
        {
            Elements.MiniBoldLabel(ToolTips.smoothLabel);

            filterStrength = Elements.ToolbarSlider(ToolTips.smoothStrength, filterStrength, 0f, 1f);
            blurDirection = Elements.ToolbarSlider(ToolTips.blurDirection, blurDirection, -1f, 1f);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.smoothMap))
                for (int i = 0; i < smoothPasses; i++)
                    MapManager.SmoothHeightmap(filterStrength, blurDirection);
            smoothPasses = EditorGUILayout.IntSlider(smoothPasses, 1, 100);
            Elements.EndToolbarHorizontal();
        }

        public static void NormaliseMap(ref float normaliseLow, ref float normaliseHigh, ref bool autoUpdate)
        {
            Elements.MiniBoldLabel(ToolTips.normaliseLabel);

            EditorGUI.BeginChangeCheck();
            normaliseLow = Elements.ToolbarSlider(ToolTips.normaliseLow, normaliseLow, 0f, 1000f);
            normaliseHigh = Elements.ToolbarSlider(ToolTips.normaliseHigh, normaliseHigh, 0f, 1000f);
            
			Elements.BeginToolbarHorizontal();
			if (GUILayout.Button("Normalise"))
                MapManager.NormaliseHeightmap(normaliseLow, normaliseHigh, Selections.Terrains.Land, Dimensions.HeightMapDimensions());
            
            if (GUILayout.Button("Snap to map"))
			{
				Vector2 minmax = new Vector2(0f,1000f);
                minmax = GenerativeManager.minmaxHeightmap();				
				normaliseHigh = minmax.x*1000f;
				normaliseLow = minmax.y*1000f;
			}
            Elements.EndToolbarHorizontal();
			
			if (GUILayout.Button("Flip"))
							{
                                GenerativeManager.FlipHeightmap();
							}
        }

        public static void SetHeight(ref float height)
        {
            Elements.MiniBoldLabel(ToolTips.setHeightLabel);

            height = Elements.ToolbarSlider(ToolTips.heightToSet, height, 0f, 1000f);
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.setLandHeight))
                MapManager.SetHeightmap(height, Selections.Terrains.Land, Dimensions.HeightMapDimensions());
            if (Elements.ToolbarButton(ToolTips.setWaterHeight))
                MapManager.SetHeightmap(height, Selections.Terrains.Water, Dimensions.HeightMapDimensions());
            Elements.EndToolbarHorizontal();
        }

        public static void ClampHeight(ref float heightLow, ref float heightHigh)
        {
            Elements.MiniBoldLabel(ToolTips.clampHeightLabel);

            Elements.ToolbarMinMax(ToolTips.minHeight, ToolTips.maxHeight, ref heightLow, ref heightHigh, 0f, 1000f);
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.setMinHeight))
                MapManager.ClampHeightmap(heightLow, 1000f, Selections.Terrains.Land, Dimensions.HeightMapDimensions());
            if (Elements.ToolbarButton(ToolTips.setMaxHeight))
                MapManager.ClampHeightmap(0f, heightHigh, Selections.Terrains.Land, Dimensions.HeightMapDimensions());
            Elements.EndToolbarHorizontal();
        }

        public static void OffsetMap(ref float offset, ref bool clampOffset)
        {
            Elements.MiniBoldLabel(ToolTips.offsetLabel);

            offset = Elements.ToolbarSlider(ToolTips.offsetHeight, offset, -1000f, 1000f);
            Elements.BeginToolbarHorizontal();
            
            if (Elements.ToolbarButton(ToolTips.offsetLand))
                MapManager.OffsetHeightmap(offset, clampOffset, Selections.Terrains.Land, Dimensions.HeightMapDimensions());
            if (Elements.ToolbarButton(ToolTips.offsetWater))
                MapManager.OffsetHeightmap(offset, clampOffset, Selections.Terrains.Water, Dimensions.HeightMapDimensions());
            Elements.EndToolbarHorizontal();
        }

        public static void InvertMap()
        {
            Elements.MiniBoldLabel(ToolTips.invertLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.invertLand))
                MapManager.InvertHeightmap(Selections.Terrains.Land, Dimensions.HeightMapDimensions());
            if (Elements.ToolbarButton(ToolTips.invertWater))
                MapManager.InvertHeightmap(Selections.Terrains.Water, Dimensions.HeightMapDimensions());
            Elements.EndToolbarHorizontal();
        }
        #endregion
		


        public static void ConditionalPaintConditions(ref Conditions cnds, ref int cndsOptions)
        {
            Elements.BoldLabel(ToolTips.conditionalPaintLabel);

            GUIContent[] conditionalPaintMenu = new GUIContent[5];
            conditionalPaintMenu[0] = new GUIContent("Ground");
            conditionalPaintMenu[1] = new GUIContent("Biome");
            conditionalPaintMenu[2] = new GUIContent("Alpha");
            conditionalPaintMenu[3] = new GUIContent("Topology");
            conditionalPaintMenu[4] = new GUIContent("Terrain");
            cndsOptions = GUILayout.Toolbar(cndsOptions, conditionalPaintMenu, EditorStyles.toolbarButton);

            Elements.MiniBoldLabel(ToolTips.conditionsLabel);

            switch (cndsOptions)
            {
                case 0: // Ground
                    Elements.BeginToolbarHorizontal();
                    cnds.GroundConditions.CheckLayer[TerrainSplat.TypeToIndex((int)cnds.GroundConditions.Layer)] = Elements.ToolbarToggle(ToolTips.checkTexture, cnds.GroundConditions.CheckLayer[TerrainSplat.TypeToIndex((int)cnds.GroundConditions.Layer)]);
                    cnds.GroundConditions.Layer = (TerrainSplat.Enum)Elements.ToolbarEnumPopup(cnds.GroundConditions.Layer);
                    Elements.EndToolbarHorizontal();

                    Elements.BeginToolbarHorizontal();
                    cnds.GroundConditions.Weight[TerrainSplat.TypeToIndex((int)cnds.GroundConditions.Layer)] = Elements.ToolbarSlider(ToolTips.conditionalTextureWeight, cnds.GroundConditions.Weight[TerrainSplat.TypeToIndex((int)cnds.GroundConditions.Layer)], 0.01f, 1f);
                    Elements.EndToolbarHorizontal();
                    break;
                case 1: // Biome
                    Elements.BeginToolbarHorizontal();
                    cnds.BiomeConditions.CheckLayer[TerrainBiome.TypeToIndex((int)cnds.BiomeConditions.Layer)] = Elements.ToolbarToggle(ToolTips.checkTexture, cnds.BiomeConditions.CheckLayer[TerrainBiome.TypeToIndex((int)cnds.BiomeConditions.Layer)]);
                    cnds.BiomeConditions.Layer = (TerrainBiome.Enum)Elements.ToolbarEnumPopup(cnds.BiomeConditions.Layer);
                    Elements.EndToolbarHorizontal();

                    Elements.BeginToolbarHorizontal();
                    cnds.BiomeConditions.Weight[TerrainBiome.TypeToIndex((int)cnds.BiomeConditions.Layer)] = Elements.ToolbarSlider(ToolTips.conditionalTextureWeight, cnds.BiomeConditions.Weight[TerrainBiome.TypeToIndex((int)cnds.BiomeConditions.Layer)], 0.01f, 1f);
                    Elements.EndToolbarHorizontal();
                    break;
                case 2: // Alpha
                    Elements.BeginToolbarHorizontal();
                    cnds.AlphaConditions.CheckAlpha = Elements.ToolbarToggle(ToolTips.checkTexture, cnds.AlphaConditions.CheckAlpha);
                    cnds.AlphaConditions.Texture = (AlphaTextures)Elements.ToolbarEnumPopup(cnds.AlphaConditions.Texture);
                    Elements.EndToolbarHorizontal();
                    break;
                case 3: // Topology
                    Elements.BeginToolbarHorizontal();
                    cnds.TopologyConditions.CheckLayer[TerrainTopology.TypeToIndex((int)cnds.TopologyConditions.Layer)] = Elements.ToolbarToggle(ToolTips.checkTopologyLayer, cnds.TopologyConditions.CheckLayer[TerrainTopology.TypeToIndex((int)cnds.TopologyConditions.Layer)]);
                    cnds.TopologyConditions.Layer = (TerrainTopology.Enum)Elements.ToolbarEnumPopup(cnds.TopologyConditions.Layer);
                    Elements.EndToolbarHorizontal();

                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.checkTexture);
                    cnds.TopologyConditions.Texture[TerrainTopology.TypeToIndex((int)cnds.TopologyConditions.Layer)] = (TopologyTextures)Elements.ToolbarEnumPopup(cnds.TopologyConditions.Texture[TerrainTopology.TypeToIndex((int)cnds.TopologyConditions.Layer)]);
                    Elements.EndToolbarHorizontal();
                    break;
                case 4: // Terrain
                    float tempSlopeLow = cnds.TerrainConditions.Slopes.SlopeLow, tempSlopeHigh = cnds.TerrainConditions.Slopes.SlopeHigh;
                    cnds.TerrainConditions.CheckSlopes = Elements.ToolbarToggleMinMax(ToolTips.checkSlopes, ToolTips.rangeLow, ToolTips.rangeHigh, cnds.TerrainConditions.CheckSlopes, ref tempSlopeLow, ref tempSlopeHigh, 0f, 90f);
                    cnds.TerrainConditions.Slopes.SlopeLow = tempSlopeLow; cnds.TerrainConditions.Slopes.SlopeHigh = tempSlopeHigh;

                    float tempHeightLow = cnds.TerrainConditions.Heights.HeightLow, tempHeightHigh = cnds.TerrainConditions.Heights.HeightHigh;
                    cnds.TerrainConditions.CheckHeights = Elements.ToolbarToggleMinMax(ToolTips.checkHeights, ToolTips.rangeLow, ToolTips.rangeHigh, cnds.TerrainConditions.CheckHeights, ref tempHeightLow, ref tempHeightHigh, 0f, 1000f);
                    cnds.TerrainConditions.Heights.HeightLow = tempHeightLow; cnds.TerrainConditions.Heights.HeightHigh = tempHeightHigh;
                    break;
            }
        }

        public static void ConditionalPaintLayerSelect(ref Conditions cnds, ref Layers layers, ref int texture)
        {
            Elements.MiniBoldLabel(ToolTips.textureToPaintLabel);

            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.layerSelect);
            layers.LandLayer = (LandLayers)Elements.ToolbarEnumPopup(layers.LandLayer);
            Elements.EndToolbarHorizontal();

            switch (layers.LandLayer)
            {
                case LandLayers.Ground:
                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.Ground = (TerrainSplat.Enum)Elements.ToolbarEnumPopup(layers.Ground);
                    texture = TerrainSplat.TypeToIndex((int)layers.Ground);
                    Elements.EndToolbarHorizontal();
                    break;
                case LandLayers.Biome:
                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.Biome = (TerrainBiome.Enum)Elements.ToolbarEnumPopup(layers.Biome);
                    texture = TerrainBiome.TypeToIndex((int)layers.Biome);
                    Elements.EndToolbarHorizontal();
                    break;
                case LandLayers.Alpha:
                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.AlphaTexture = (AlphaTextures)Elements.ToolbarEnumPopup(layers.AlphaTexture);
                    texture = (int)layers.AlphaTexture;
                    Elements.EndToolbarHorizontal();
                    break;
                case LandLayers.Topology:
                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.topologyLayerSelect);
                    layers.Topologies = (TerrainTopology.Enum)Elements.ToolbarEnumPopup(layers.Topologies);
                    Elements.EndToolbarHorizontal();

                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.TopologyTexture = (TopologyTextures)Elements.ToolbarEnumPopup(layers.TopologyTexture);
                    texture = (int)layers.TopologyTexture;
                    Elements.EndToolbarHorizontal();
                    break;
            }

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.paintConditional))
                MapManager.PaintConditional(layers.LandLayer, texture, cnds, TerrainTopology.TypeToIndex((int)layers.Topologies));
            Elements.EndToolbarHorizontal();
        }

        public static void ConditionalPaint(ref int cndsOptions, ref int texture, ref Conditions cnds, ref Layers layers)
        {
            ConditionalPaintConditions(ref cnds, ref cndsOptions);
            ConditionalPaintLayerSelect(ref cnds, ref layers, ref texture);
        }

        public static void RotateMap(ref Selections.Objects selection)
        {
            Elements.MiniBoldLabel(ToolTips.rotateMapLabel);

            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.rotateSelection);
            selection = (Selections.Objects)Elements.ToolbarEnumFlagsField(selection);
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.rotate90))
                MapManager.RotateMap(selection, true);
            if (Elements.ToolbarButton(ToolTips.rotate270))
                MapManager.RotateMap(selection, false);
            Elements.EndToolbarHorizontal();
        }
        #endregion

        #region LayerTools
        public static void TextureSelect(LandLayers landLayer, ref Layers layers)
        {
            Elements.MiniBoldLabel(ToolTips.textureSelectLabel);

            switch (landLayer)
            {
                case LandLayers.Ground:
                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.Ground = (TerrainSplat.Enum)Elements.ToolbarEnumPopup(layers.Ground);
                    Elements.EndToolbarHorizontal();
                    break;
                case LandLayers.Biome:
                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.Biome = (TerrainBiome.Enum)Elements.ToolbarEnumPopup(layers.Biome);
                    Elements.EndToolbarHorizontal();
                    break;
            }
        }

		public static void sourceTextureSelect(ref Layers layers)
        {
            Elements.MiniBoldLabel(ToolTips.textureSelectLabel);

                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.Ground = (TerrainSplat.Enum)Elements.ToolbarEnumPopup(layers.Ground);
                    Elements.EndToolbarHorizontal();
        }

        public static void sourceTopologyLayerSelect(ref Layers layers)
        {

            Elements.BeginToolbarHorizontal();
			GUILayout.Label("Source Topology:", EditorStyles.boldLabel);
            layers.Topologies = (TerrainTopology.Enum)Elements.ToolbarEnumPopup(layers.Topologies);
            Elements.EndToolbarHorizontal();

        }

        public static void TopologyLayerSelect(ref Layers layers)
        {
            Elements.MiniBoldLabel(ToolTips.layerSelect);

            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.topologyLayerSelect);
            EditorGUI.BeginChangeCheck();
            layers.Topologies = (TerrainTopology.Enum)Elements.ToolbarEnumPopup(layers.Topologies);
            Elements.EndToolbarHorizontal();

            if (EditorGUI.EndChangeCheck())
                ChangeLandLayer(LandLayers.Topology, TerrainTopology.TypeToIndex((int)layers.Topologies));
        }

        public static void SlopeTools(LandLayers landLayer, int texture, ref SlopesInfo slopeInfo, int erase = 0, int topology = 0)
        {
            Elements.MiniBoldLabel(ToolTips.slopeToolsLabel);
            slopeInfo = ClampValues(slopeInfo);

            float tempSlopeLow = slopeInfo.SlopeLow; float tempSlopeHigh = slopeInfo.SlopeHigh;
            if ((int)landLayer < 2)
            {
                float tempSlopeBlendLow = slopeInfo.SlopeBlendLow; float tempSlopeBlendHigh = slopeInfo.SlopeBlendHigh;
                slopeInfo.BlendSlopes = Elements.ToolbarToggleMinMax(ToolTips.toggleBlend, ToolTips.rangeLow, ToolTips.rangeHigh, slopeInfo.BlendSlopes, ref tempSlopeLow, ref tempSlopeHigh, 0f, 90f);
                slopeInfo.SlopeLow = tempSlopeLow; slopeInfo.SlopeHigh = tempSlopeHigh;

                if (slopeInfo.BlendSlopes)
                {
                    Elements.ToolbarMinMax(ToolTips.blendLow, ToolTips.blendHigh, ref tempSlopeBlendLow, ref tempSlopeBlendHigh, 0f, 90f);
                    slopeInfo.SlopeBlendLow = tempSlopeBlendLow; slopeInfo.SlopeBlendHigh = tempSlopeBlendHigh;
                }

                Elements.BeginToolbarHorizontal();
                if (Elements.ToolbarButton(ToolTips.paintSlopes))
                    MapManager.PaintSlope(landLayer, slopeInfo.SlopeLow, slopeInfo.SlopeHigh, texture);
                if (Elements.ToolbarButton(ToolTips.paintSlopesBlend))
                    MapManager.PaintSlopeBlend(landLayer, slopeInfo.SlopeLow, slopeInfo.SlopeHigh, slopeInfo.SlopeBlendLow, slopeInfo.SlopeBlendHigh, texture);
                Elements.EndToolbarHorizontal();
            }
            else
            {
                Elements.ToolbarMinMax(ToolTips.rangeLow, ToolTips.rangeHigh, ref tempSlopeLow, ref tempSlopeHigh, 0f, 90f);
                slopeInfo.SlopeLow = tempSlopeLow; slopeInfo.SlopeHigh = tempSlopeHigh;

                Elements.BeginToolbarHorizontal();
                if (Elements.ToolbarButton(ToolTips.paintSlopes))
                    MapManager.PaintSlope(landLayer, slopeInfo.SlopeLow, slopeInfo.SlopeHigh, texture, topology);
                if (Elements.ToolbarButton(ToolTips.eraseSlopes))
                    MapManager.PaintSlope(landLayer, slopeInfo.SlopeLow, slopeInfo.SlopeHigh, erase, topology);
                Elements.EndToolbarHorizontal();
            }
        }

        public static void HeightTools(LandLayers landLayer, int texture, ref HeightsInfo heightInfo, int erase = 0, int topology = 0)
        {
            Elements.MiniBoldLabel(ToolTips.heightToolsLabel);
            heightInfo = ClampValues(heightInfo);

            float tempSlopeLow = heightInfo.HeightLow; float tempSlopeHigh = heightInfo.HeightHigh;
            if ((int)landLayer < 2)
            {
                float tempHeightBlendLow = heightInfo.HeightBlendLow; float tempHeightBlendHigh = heightInfo.HeightBlendHigh;
                heightInfo.BlendHeights = Elements.ToolbarToggleMinMax(ToolTips.toggleBlend, ToolTips.rangeLow, ToolTips.rangeHigh, heightInfo.BlendHeights, ref tempSlopeLow, ref tempSlopeHigh, 0f, 1000f);
                heightInfo.HeightLow = tempSlopeLow; heightInfo.HeightHigh = tempSlopeHigh;

                if (heightInfo.BlendHeights)
                {
                    Elements.ToolbarMinMax(ToolTips.blendLow, ToolTips.blendHigh, ref tempHeightBlendLow, ref tempHeightBlendHigh, 0f, 1000f);
                    heightInfo.HeightBlendLow = tempHeightBlendLow; heightInfo.HeightBlendHigh = tempHeightBlendHigh;
                }

                Elements.BeginToolbarHorizontal();
                if (Elements.ToolbarButton(ToolTips.paintHeights))
                    MapManager.PaintHeight(landLayer, heightInfo.HeightLow, heightInfo.HeightHigh, texture);
                if (Elements.ToolbarButton(ToolTips.paintHeightsBlend))
                    MapManager.PaintHeightBlend(landLayer, heightInfo.HeightLow, heightInfo.HeightHigh, heightInfo.HeightBlendLow, heightInfo.HeightBlendHigh, texture);
                Elements.EndToolbarHorizontal();
            }
            else
            {
                Elements.ToolbarMinMax(ToolTips.rangeLow, ToolTips.rangeHigh, ref tempSlopeLow, ref tempSlopeHigh, 0f, 1000f);
                heightInfo.HeightLow = tempSlopeLow; heightInfo.HeightHigh = tempSlopeHigh;

                Elements.BeginToolbarHorizontal();
                if (Elements.ToolbarButton(ToolTips.paintHeights))
                    MapManager.PaintHeight(landLayer, heightInfo.HeightLow, heightInfo.HeightHigh, texture, topology);
                if (Elements.ToolbarButton(ToolTips.eraseHeights))
                    MapManager.PaintHeight(landLayer, heightInfo.HeightLow, heightInfo.HeightHigh, erase, topology);
                Elements.EndToolbarHorizontal();
            }
        }

        public static void TopologyTools()
        {
			/*
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.rotateAll90))
                MapManager.RotateTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING, true);
            if (Elements.ToolbarButton(ToolTips.rotateAll270))
                MapManager.RotateTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING, false);
            Elements.EndToolbarHorizontal();
			*/
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.paintAll))
                MapManager.PaintTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING);
            if (Elements.ToolbarButton(ToolTips.clearAll))
                MapManager.ClearTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING);
            if (Elements.ToolbarButton(ToolTips.invertAll))
                MapManager.InvertTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING);
            Elements.EndToolbarHorizontal();
        }

        public static void AreaSelect()
        {
            Elements.MiniBoldLabel(ToolTips.areaSelectLabel);

            Elements.ToolbarMinMaxInt(ToolTips.fromZ, ToolTips.toZ, ref AreaManager.Area.z0, ref AreaManager.Area.z1, 0, Land.terrainData.alphamapResolution);
            Elements.ToolbarMinMaxInt(ToolTips.fromX, ToolTips.toX, ref AreaManager.Area.x0, ref AreaManager.Area.x1, 0, Land.terrainData.alphamapResolution);

            if (Elements.ToolbarButton(ToolTips.resetArea))
                AreaManager.Reset();
        }

        public static void RiverTools(LandLayers landLayer, int texture, ref bool aboveTerrain, int erase = 0, int topology = 0)
        {
            Elements.MiniBoldLabel(ToolTips.riverToolsLabel);

            if ((int)landLayer > 1)
            {
                Elements.BeginToolbarHorizontal();
                aboveTerrain = Elements.ToolbarToggle(ToolTips.aboveTerrain, aboveTerrain);
                if (Elements.ToolbarButton(ToolTips.paintRivers))
                    MapManager.PaintRiver(landLayer, aboveTerrain, texture, topology);
                if (Elements.ToolbarButton(ToolTips.eraseRivers))
                    MapManager.PaintRiver(landLayer, aboveTerrain, erase, topology);
                Elements.EndToolbarHorizontal();
            }
            else
            {
                Elements.BeginToolbarHorizontal();
                aboveTerrain = Elements.ToolbarToggle(ToolTips.aboveTerrain, aboveTerrain);
                if (Elements.ToolbarButton(ToolTips.paintRivers))
                    MapManager.PaintRiver(landLayer, aboveTerrain, texture);
                Elements.EndToolbarHorizontal();
            }
        }

        public static void RotateTools(LandLayers landLayer, int topology = 0)
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.rotate90))
                MapManager.RotateLayer(landLayer, true, topology);
            if (Elements.ToolbarButton(ToolTips.rotate270))
                MapManager.RotateLayer(landLayer, false, topology);
            Elements.EndToolbarHorizontal();
        }

        public static void LayerTools(LandLayers landLayer, int texture, int erase = 0, int topology = 0)
        {
            Elements.MiniBoldLabel(ToolTips.layerToolsLabel);

            Elements.BeginToolbarHorizontal();
			if ((int)landLayer <= 1)
			{
				if (Elements.ToolbarButton(ToolTips.paintLayer))
					MapManager.PaintLayer(landLayer, texture, topology);
			}
            else if ((int)landLayer > 1)
            {
				if (Elements.ToolbarButton(ToolTips.paintLayer))
					GenerativeManager.fillTopology(topology);
                if (Elements.ToolbarButton(ToolTips.clearLayer))
                    MapManager.ClearLayer(landLayer, topology);
                if (Elements.ToolbarButton(ToolTips.invertLayer))
                    MapManager.InvertLayer(landLayer, topology);
            }
            Elements.EndToolbarHorizontal();
        }
        #endregion

        #region PrefabData
        public static void PrefabCategory(PrefabDataHolder target)
        {
            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.prefabCategory);
            target.prefabData.category = Elements.ToolbarTextField(target.prefabData.category);
            Elements.EndToolbarHorizontal();
        }

        public static void PrefabID(PrefabDataHolder target)
        {
            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.prefabID);
            target.prefabData.id = uint.Parse(Elements.ToolbarDelayedTextField(target.prefabData.id.ToString()));
            Elements.EndToolbarHorizontal();
        }

        public static void SnapToGround(PrefabDataHolder target)
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.snapToGround))
                target.SnapToGround();
            Elements.EndToolbarHorizontal();
        }

        public static void ToggleLights(PrefabDataHolder target)
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.toggleLights))
                target.ToggleLights();
            Elements.EndToolbarHorizontal();
        }
        #endregion

        #region Functions
        /// <summary>Sets the active landLayer to the index.</summary>
        /// <param name="landIndex">The landLayer to change to.</param>
        /// <param name="topology">The Topology layer to set.</param>
        public static void SetLandLayer(LandLayers landIndex, int topology = 0)
        {
            ChangeLandLayer(landIndex, topology);
        }

        public static SlopesInfo ClampValues(SlopesInfo info)
        {
            info.SlopeLow = Mathf.Clamp(info.SlopeLow, 0f, info.SlopeHigh);
            info.SlopeHigh = Mathf.Clamp(info.SlopeHigh, info.SlopeLow, 90f);
            info.SlopeBlendLow = Mathf.Clamp(info.SlopeBlendLow, 0f, info.SlopeLow);
            info.SlopeBlendHigh = Mathf.Clamp(info.SlopeBlendHigh, info.SlopeHigh, 90f);
            return info;
        }

        public static HeightsInfo ClampValues(HeightsInfo info)
        {
            info.HeightLow = Mathf.Clamp(info.HeightLow, 0f, info.HeightHigh);
            info.HeightHigh = Mathf.Clamp(info.HeightHigh, info.HeightLow, 1000f);
            info.HeightBlendLow = Mathf.Clamp(info.HeightBlendLow, 0f, info.HeightLow);
            info.HeightBlendHigh = Mathf.Clamp(info.HeightBlendHigh, info.HeightHigh, 1000f);
            return info;
        }

        public static void ReloadTreeViews()
        {
            PrefabHierarchyWindow.ReloadTree();
            PathHierarchyWindow.ReloadTree();
        }

        public static void CopyText(string text)
        {
            TextEditor editor = new TextEditor();
            editor.text = text;
            editor.SelectAll();
            editor.Copy();
        }
        #endregion

        #region TreeViews
        public static void DisplayPrefabName(string name)
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.prefabName))
                CopyText(name);
            Elements.ToolbarLabel(new GUIContent(name, name));
            Elements.EndToolbarHorizontal();
        }

        public static void DisplayPrefabID(uint prefabID)
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.prefabID))
                CopyText(prefabID.ToString());
            Elements.ToolbarLabel(new GUIContent(prefabID.ToString(), prefabID.ToString()));
            Elements.EndToolbarHorizontal();
        }

        public static void DisplayPrefabPath(string prefabPath)
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.prefabPath))
                CopyText(prefabPath);
            Elements.ToolbarLabel(new GUIContent(prefabPath, prefabPath));
            Elements.EndToolbarHorizontal();
        }
		
		public static void Merger(ref float zOffset)
		{
			zOffset = EditorGUILayout.FloatField("Map Z offset", zOffset);
			if (GUILayout.Button("Merge Maps"))
					{
						var blob = new WorldSerialization();
						string loadFile = "";
						loadFile = UnityEditor.EditorUtility.OpenFilePanel("Merge with", loadFile, ".map");
						blob.Load(loadFile);
						GenerativeManager.pasteMonument(blob, 0,0,zOffset);
					}
			
			
			
			if (GUILayout.Button("Delete Prefabs not on Arid"))
					{
						PrefabManager.deletePrefabsOffArid(PrefabManager.CurrentMapPrefabs);
					}
			if (GUILayout.Button("Scramble Vehicles"))					
			{
					PrefabManager.VehicleScrambler(PrefabManager.CurrentMapPrefabs);
			}
		}

		public static void RustCity(ref RustCityPreset city, ref float breakerZ, ref bool destroy)
		{
			EditorGUI.BeginChangeCheck();
						
									
						
			city.start = EditorGUILayout.IntField("Start:", city.start);
			city.size = EditorGUILayout.IntField("City Size:", city.size);
			city.alley = EditorGUILayout.IntField("Alley width:", city.alley);
			city.street = EditorGUILayout.IntField("Street width:", city.street);
			city.flatness =  EditorGUILayout.FloatField("Steepness:", city.flatness);
			city.zOff = EditorGUILayout.FloatField("Z Offset:", city.zOff);
			
			if (EditorGUI.EndChangeCheck())
						{
							SettingsManager.city = city;
							SettingsManager.SaveSettings();
						}
					if (GUILayout.Button("Rust City"))
					{
						SettingsManager.city = city;
						SettingsManager.SaveSettings();
						var monumentBlob = new WorldSerialization();
						string loadFile = "highrise.monuments.generated.highres.v3.map";
						monumentBlob.Load(loadFile);
						GenerativeManager.createRustCity(monumentBlob, city);
					}
					
					if (GUILayout.Button("RGB Plots"))
					{
						SettingsManager.city = city;
						SettingsManager.SaveSettings();
						var monumentBlob = new WorldSerialization();
						string loadFile = "rgb.buildplots.map";
						monumentBlob.Load(loadFile);
						GenerativeManager.createRustCity(monumentBlob, city);
					}
					
					if (GUILayout.Button("Rust City Buildings"))
					{
						
						GenerativeManager.rustBuildings();
					}
					
					
					
					
					breakerZ =  EditorGUILayout.FloatField("Height offset:", breakerZ);
					destroy = EditorGUILayout.ToggleLeft("Destroy Originals", destroy);
					if (GUILayout.Button("Break Monuments"))
					{
						
						PrefabManager.breakMonument(PrefabManager.CurrentMapPrefabs, breakerZ, destroy);
						PrefabManager.deleteDuplicates(PrefabManager.CurrentMapPrefabs);
					}
					
					if (GUILayout.Button("Delete Duplicates"))
					{
						PrefabManager.deleteDuplicates(PrefabManager.CurrentMapPrefabs);
					}
					
					if (GUILayout.Button("Add player spawnpoints"))
					{
						PrefabManager.addSpawners();
					}
					
		}
		
        public static void SelectPrefabPaths(PrefabsListTreeView treeView, ref bool showAllPrefabs)
        {
            Elements.MiniBoldLabel(ToolTips.optionsLabel);

            Elements.BeginToolbarHorizontal();
            showAllPrefabs = Elements.ToolbarToggle(ToolTips.showAllPrefabs, showAllPrefabs);
            if (Elements.ToolbarButton(ToolTips.treeViewRefresh))
                treeView.RefreshTreeView(showAllPrefabs);
            Elements.EndToolbarHorizontal();
        }


        public static void PrefabHierachyOptions(PrefabHierarchyTreeView treeView, ref string name, ref bool replace)
        {
            Elements.MiniBoldLabel(ToolTips.hierachyOptionsLabel);

            Elements.BeginToolbarHorizontal();
            name = Elements.ToolbarTextField(name);
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.hierachyCategoryRename))
            {
                name = String.IsNullOrEmpty(name) ? "" : name;
                PrefabManager.RenamePrefabCategories(PrefabHierarchyTreeView.PrefabDataFromSelection(treeView), name);
                ReloadTreeViews();
            }
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            replace = Elements.ToolbarToggle(ToolTips.hierachyReplace, replace);
            if (Elements.ToolbarButton(ToolTips.hierachyIDRename))
            {
                if (uint.TryParse(name, out uint result))
                {
                    PrefabManager.RenamePrefabIDs(PrefabHierarchyTreeView.PrefabDataFromSelection(treeView), result, replace);
                    ReloadTreeViews();
                }
            }
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.hierachyDelete))
                PrefabManager.DeletePrefabs(PrefabHierarchyTreeView.PrefabDataFromSelection(treeView));
            Elements.EndToolbarHorizontal();
        }
        #endregion

        #region CreateNewMap
		
		public static void NewMapOptions(ref int mapSize, ref float landHeight, ref Layers layers) 
        {
            mapSize = Elements.ToolbarIntSlider(ToolTips.mapSize, mapSize, 100, 6000);
            landHeight = Elements.ToolbarSlider(ToolTips.newMapHeight, landHeight, 0, 1000);

            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.newMapGround);
            layers.Ground = (TerrainSplat.Enum)Elements.ToolbarEnumPopup(layers.Ground);
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.newMapBiome);
            layers.Biome = (TerrainBiome.Enum)Elements.ToolbarEnumPopup(layers.Biome);
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.createMap))
            {
                int newMap = EditorUtility.DisplayDialogComplex("Warning", "Creating a new map will remove any unsaved changes to your map.", "Create New Map", "Close", "Save and Create New Map");
                switch (newMap)
                {
                    case 1:
                        return;
                    case 2:
                        SaveMapPanel();
                        break;
                }
                MapManager.CreateMap(mapSize, TerrainSplat.TypeToIndex((int)layers.Ground), TerrainBiome.TypeToIndex((int)layers.Biome), landHeight);
            }

            Elements.EndToolbarHorizontal();
        }
		
        public static void NewMapOptions(ref int mapSize, ref float landHeight, ref Layers layers, CreateMapWindow window) 
        {
            mapSize = Elements.ToolbarIntSlider(ToolTips.mapSize, mapSize, 1000, 6000);
            landHeight = Elements.ToolbarSlider(ToolTips.newMapHeight, landHeight, 0, 1000);

            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.newMapGround);
            layers.Ground = (TerrainSplat.Enum)Elements.ToolbarEnumPopup(layers.Ground);
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.newMapBiome);
            layers.Biome = (TerrainBiome.Enum)Elements.ToolbarEnumPopup(layers.Biome);
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.createMap))
            {
                window.Close();
                int newMap = EditorUtility.DisplayDialogComplex("Warning", "Creating a new map will remove any unsaved changes to your map.", "Create New Map", "Close", "Save and Create New Map");
                switch (newMap)
                {
                    case 1:
                        return;
                    case 2:
                        SaveMapPanel();
                        break;
                }
                MapManager.CreateMap(mapSize, TerrainSplat.TypeToIndex((int)layers.Ground), TerrainBiome.TypeToIndex((int)layers.Biome), landHeight);
            }
            if (Elements.ToolbarButton(ToolTips.cancel))
                window.Close();
            Elements.EndToolbarHorizontal();
        }
        #endregion
    }
}