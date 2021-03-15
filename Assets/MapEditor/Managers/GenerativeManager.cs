using UnityEngine;
using UnityEditor;
using RustMapEditor.Variables;
using static WorldSerialization;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

public static class GenerativeManager
{
	
	public static void oceans(OceanPreset ocean)
	{
		
		//should fix with proper inputs
		int radius = ocean.radius;
		int gradient = ocean.gradient;
		float seafloor = ocean.seafloor / 1000f;
		int xOffset = ocean.xOffset;
		int yOffset = ocean.yOffset;
		bool perlin = ocean.perlin;
		int s = ocean.s;
			
				
				float	r = UnityEngine.Random.Range(0,10000)/100f;
				float	r1 =  UnityEngine.Random.Range(0,10000)/100f;
			
			Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
			float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
			
			float[,] perlinShape = new float[baseMap.GetLength(0),baseMap.GetLength(0)];
			
			float[,] puckeredMap = new float[baseMap.GetLength(0),baseMap.GetLength(0)];
			int distance = 0;
			
			Vector2 focusA = new Vector2(baseMap.GetLength(0)/2f+xOffset,baseMap.GetLength(0)/2f+yOffset);
			Vector2 focusB = new Vector2(baseMap.GetLength(0)/2f-xOffset,baseMap.GetLength(0)/2f-yOffset);
			
			
			Vector2 center = new Vector2(baseMap.GetLength(0)/2f,baseMap.GetLength(0)/2f);
			Vector2 scanCord = new Vector2(0f,0f);
			
			int res = baseMap.GetLength(0);
			
				for (int i = 0; i < res; i++)
				{
					EditorUtility.DisplayProgressBar("Puckering", "making island",(i*1f/res));
					for (int j = 0; j < res; j++)
					{
						scanCord.x = i; scanCord.y = j;
						//circular
						//distance = (int)Vector2.Distance(scanCord,center);
						distance = (int)(Mathf.Pow((Mathf.Pow((scanCord.x - focusA.x),4f) + Mathf.Pow((scanCord.y - focusA.y),4f)),1f/4f));
						
						//distance = (int)Mathf.Sqrt(Vector2.Distance(scanCord,focusA)) + (int)Mathf.Sqrt(Vector2.Distance(scanCord,focusB));
						
						//if distance from center less than radius, value is 1
						if (distance < radius*2f)
						{
							puckeredMap[i,j] = 1f;
						}
						//otherwise the value should proceed to 0
						else if (distance>=radius *2f && distance <=radius*2f + gradient)
						{
							perlinShape[i,j] = Mathf.PerlinNoise(i*1f/s+r, j*1f/s+r1)*2f;
							if (perlinShape[i,j] > 1f)
								perlinShape[i,j] = 1f;
							puckeredMap[i,j] = .5f+Mathf.Cos(((distance-radius*2f)/gradient)*Mathf.PI)*.5f - (Mathf.Sin(((distance-radius*2f)/gradient)*Mathf.PI)*perlinShape[i,j]*.5f);
							
							if (puckeredMap[i,j] < 0)
								puckeredMap[i,j] = 0;
						}
						else
						{
							puckeredMap[i,j] = 0f;
						}
						
						puckeredMap[i,j] = Mathf.Lerp(seafloor, baseMap[i,j], puckeredMap[i,j]);
					}
				}
												

						
			
			EditorUtility.ClearProgressBar();
			land.terrainData.SetHeights(0, 0, puckeredMap);
	}
	
	public static void perlinRidiculous(PerlinPreset perlin)
	{
			int l = perlin.layers;
			int p = perlin.period;
			int s = perlin.scale;
			
			Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
			float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
			float[,] perlinSum = baseMap;
			
			
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					perlinSum[i,j] = (0);
				}
			}
			
			
			float r = 0;
			float r1 = 0;
			float amplitude = 1f;
			
			
			for (int u = 1; u <= l; u++)
			{
				
				r = UnityEngine.Random.Range(0,10000)/100f;
				r1 =  UnityEngine.Random.Range(0,10000)/100f;
				amplitude *= .3f;
				
				
				
				for (int i = 0; i < baseMap.GetLength(0); i++)
				{
		
					for (int j = 0; j < baseMap.GetLength(0); j++)
					{
						
						perlinSum[i,j] +=  amplitude * Mathf.PerlinNoise((Mathf.PerlinNoise((Mathf.PerlinNoise(Mathf.PerlinNoise(Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r), Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r1)), Mathf.PerlinNoise(Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r), Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r1)))), (Mathf.PerlinNoise(Mathf.PerlinNoise(Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r), Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r1)), Mathf.PerlinNoise(Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r), Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r1)))))),(Mathf.PerlinNoise((Mathf.PerlinNoise(Mathf.PerlinNoise(Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r), Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r1)), Mathf.PerlinNoise(Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r), Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r1)))), (Mathf.PerlinNoise(Mathf.PerlinNoise(Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r), Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r1)), Mathf.PerlinNoise(Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r), Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r1)))))));
					}
					EditorUtility.DisplayProgressBar("Generating layer " + u.ToString(), "", (i*1f / baseMap.GetLength(0)*1f));
				}
												
				s = s + p;
				
			}
			EditorUtility.ClearProgressBar();
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					perlinSum[i,j] = (perlinSum[i,j]/(l)*3f)+.3525f;
				}
			}
			
			
	
			land.terrainData.SetHeights(0, 0, perlinSum);
			//changeLandLayer();
	
	}	
	
	public static void perlinSimple(PerlinPreset perlin)
	{
			int l = perlin.layers;
			int p = perlin.period;
			int s = perlin.scale;
			
			Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
			float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
			float[,] perlinSum = baseMap;
			
			
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					perlinSum[i,j] = (0);
				}
			}
			
			
			float r = 0;
			float amplitude = .5f;
			float height  = .15f;
			
			
			for (int u = 1; u <= l; u++)
			{
				
				r = UnityEngine.Random.Range(0,10000)/100f;
				//r1 =  UnityEngine.Random.Range(0,10000)/100f;
				
				
				
				
				
				for (int i = 0; i < baseMap.GetLength(0); i++)
				{
		
					for (int j = 0; j < baseMap.GetLength(0); j++)
					{
						
						perlinSum[i,j] += Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r)*height + amplitude;
					}
					EditorUtility.DisplayProgressBar("Generating layer " + u.ToString(), "", (i*1f / baseMap.GetLength(0)*1f));
				}
												
				s = s - p;
				amplitude=0;
				height *= .5f;
				
			}
			EditorUtility.ClearProgressBar();
			
			
	
			land.terrainData.SetHeights(0, 0, perlinSum);
			//changeLandLayer();
	
	}	
	
	public static void randomTerracing(TerracingPreset terracing)
	{
			bool flatten = terracing.flatten;
			bool perlinBanks = terracing.perlinBanks;
			bool circular = terracing.circular;
			float terWeight = terracing.weight;
			int zStart = terracing.zStart;
			int gBot = terracing.gateBottom;
			int gTop = terracing.gateTop;
			int gates = terracing.gates;
			int descaler = terracing.descaleFactor;
			int density = terracing.perlinDensity;
			
			Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
			float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
			float[,] perlinSum = baseMap;
			
			float gateTop = zStart/1000f;
			float gateBottom = .5f;
			float gateRange = 0;
			float gateLoc =0;
			
			float r = 0;
			float r1 = 0;
			int s = density;
	
					
			r = UnityEngine.Random.Range(0,10000)/100f;
			r1 =  UnityEngine.Random.Range(0,10000)/100f;
	
	for (int g = 0; g < gates; g++)
			{
			gateBottom = gateTop*1f;
			gateRange = UnityEngine.Random.Range(gBot,gTop)/1000f;
			gateTop = gateTop + gateRange;
			
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					
					if (flatten && (baseMap[i,j] <= gateBottom) && g==0)
					{
						baseMap[i,j] = baseMap[i,j] / descaler + gateBottom - (gateBottom/descaler);
					}
					

					if ((baseMap[i,j] > gateBottom) && (baseMap[i,j] < gateTop))
					{
						
									gateLoc = (baseMap[i,j]-gateBottom)/(gateTop-gateBottom);
									
									
									
									
									if (circular)
									{
										if (perlinBanks)
										{
											baseMap[i,j] = baseMap[i,j]-(Mathf.Sin(3.12f*gateLoc)* (gateTop-gateBottom)*(perlinSum[i,j] * terWeight* (Mathf.PerlinNoise(i*1f/s+r, j*1f/s+r1)) ));										
										}
										else
										{
											baseMap[i,j] = baseMap[i,j]-(Mathf.Sin(3.12f * gateLoc * .7f) * (gateTop-gateBottom) * terWeight );
										}
									}
									else
									{
										if (perlinBanks)
										{
											baseMap[i,j] = baseMap[i,j]-(gateLoc*.7f) * (gateTop-gateBottom)*(perlinSum[i,j] * terWeight) * (Mathf.PerlinNoise(i*1f/s+r, j*1f/s+r1));										
										}
										else
										{
											baseMap[i,j] = baseMap[i,j]-((gateLoc*.7f) * (gateTop-gateBottom)*terWeight);
										}
									}
					}
	
				}
			}
			
			
			
	
		}
	
		land.terrainData.SetHeights(0, 0, baseMap);
	}
	
	public static void rippledFiguring(RipplePreset ripple)
	{
			
			int size = ripple.size;
			int density = ripple.density;
			float weight = ripple.weight;
			
			Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
			float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
			float[,] perlinMap = baseMap;
			
				float r = 0;
				float r1 = 0;
				int s = size;
				float field = 0;
				float rippling = 0;
					r = UnityEngine.Random.Range(0,10000)/100f;
					r1 =  UnityEngine.Random.Range(0,10000)/100f;
					
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					field = (-.5f+2f*(Mathf.PerlinNoise(i*1f/s*density+r, j*1f/s*density+r1)));
					if (field < 0f){field=0f;}
					rippling = -.65f * Mathf.Abs(Mathf.Pow((.011f * (Mathf.PerlinNoise(i * 1.8f/s+r, j*1.8f/s+r1))),3f) - (.011f * Mathf.PerlinNoise(i*1f/s+r, j*1f/s+r1)));
					baseMap[i,j] = baseMap[i,j] - weight * field * rippling;
				}
			}
			
			
			land.terrainData.SetHeights(0, 0, baseMap);
						
	}
	
	public static void perlinSplat(PerlinSplatPreset perlinSplat)
	{
		int s = perlinSplat.scale;
		float c = perlinSplat.strength;
		bool invert = perlinSplat.invert;
		bool paintBiome = perlinSplat.paintBiome;
		int t = perlinSplat.splatLayer;
		int index = TerrainBiome.TypeToIndex((int)perlinSplat.biomeLayer);
		
		float[,,] newBiome = TerrainManager.BiomeArray;
		float[,,] newGround = TerrainManager.GroundArray;
		

		float o = 0;
		float r = UnityEngine.Random.Range(0,10000)/100f;
		float r1 = UnityEngine.Random.Range(0,10000)/100f;
		
		int res = newGround.GetLength(0);
		
		for (int i = 0; i < res; i++)
        {
			EditorUtility.DisplayProgressBar("Gradient Noise", "Textures",(i*1f/res));
            for (int j = 0; j < res; j++)
            {
					o = Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r1);
					
					o *= c;
					
					if (o > 1f)
						o=1f;
					
					if (invert)
						o = 1f - o; 
					
					
					if (paintBiome)
						o *= (newBiome[i,j, index]);
					
										
					
					if (o > 0f)
					{
					newGround[i,j,t] = Math.Max(o, newGround[i,j,t]);
					
					for (int m = 0; m <=7; m++)
									{
										if (m!=t)
											newGround[i,j,m] *= 1f-o;
										
									}
					
					
						
					}						
				
            }
        }
		EditorUtility.ClearProgressBar();
		//dont forget this shit again
		TerrainManager.SetData(newGround, TerrainManager.LandLayer, 0);
		TerrainManager.SetLayer(TerrainManager.LandLayer, 0);

	}
	
	
	
	public static void terrainToTopology(Layers layer, Layers sourceLayer, float threshhold)
	{
		float[,,] splatMap = TerrainManager.GetSplatMap(LandLayers.Topology, TerrainTopology.TypeToIndex((int)layer.Topologies));
		float[,,] targetGround = TerrainManager.GroundArray;
		int t = TerrainSplat.TypeToIndex((int)sourceLayer.Ground);
		
		int res = targetGround.GetLength(0);
		for (int i = 0; i < res; i++)
        {
			EditorUtility.DisplayProgressBar("Copying", "Terrains to Topology",(i*1f/res));
            for (int j = 0; j < res; j++)
            {
                if (targetGround[i,j,t] >= threshhold)
				{
					splatMap[i, j, 0] = float.MaxValue;
					splatMap[i, j, 1] = float.MinValue;
				}
            }
        }
		EditorUtility.ClearProgressBar();
		TerrainManager.SetData(splatMap, LandLayers.Topology,  TerrainTopology.TypeToIndex((int)layer.Topologies));
        TerrainManager.SetLayer(LandLayers.Topology,  TerrainTopology.TypeToIndex((int)layer.Topologies));
	}	
	
	public static void copyTopologyLayer(Layers layer, Layers sourceLayer)
	{
		float[,,] splatMap = TerrainManager.GetSplatMap(LandLayers.Topology, TerrainTopology.TypeToIndex((int)sourceLayer.Topologies));
		TerrainManager.SetData(splatMap, LandLayers.Topology, TerrainTopology.TypeToIndex((int)layer.Topologies));
        TerrainManager.SetLayer(LandLayers.Topology, TerrainTopology.TypeToIndex((int)layer.Topologies));
	}
	
	
	public static void paintTopologyOutline(Layers layer, Layers sourceLayer, int w)
	{

		float[,,] sourceMap = TerrainManager.GetSplatMap(LandLayers.Topology, TerrainTopology.TypeToIndex((int)sourceLayer.Topologies));	
		int res = sourceMap.GetLength(0);
		float[,,] splatMap = new float[res,res,2];
		float[,,] scratchMap = new float[res,res,2];
		float[,,] hateMap = new float[res,res,2];
		
		for (int i = 1; i < sourceMap.GetLength(0)-1; i++)
			{
				
				for (int j = 1; j < sourceMap.GetLength(1)-1; j++)
				{
					if (sourceMap[i, j, 0] <= .5f)
								{
									scratchMap[i, j, 0] = float.MaxValue;
									scratchMap[i, j, 1] = float.MinValue;
									hateMap[i, j, 0] = float.MaxValue;
									hateMap[i, j, 1] = float.MinValue;
								}
								else
								{
									scratchMap[i, j, 0] = float.MinValue;
									scratchMap[i, j, 1] = float.MaxValue;
									hateMap[i, j, 0] = float.MinValue;
									hateMap[i, j, 1] = float.MaxValue;
								}
				}
			}
		
		
		for (int n = 1; n <= w; n++)
		{
			
			for (int i = 1; i < sourceMap.GetLength(0)-1; i++)
			{
				EditorUtility.DisplayProgressBar("Outlining", " Topology",(i*1f/res));
				for (int j = 1; j < sourceMap.GetLength(1)-1; j++)
				{
					for (int k = -1; k <= 1; k++)
					{
						for (int l = -1; l <= 1; l++)
						{
							if (scratchMap[i-1, j-1, 1] >= 1f
								|| scratchMap[i-1, j, 1] >= 1f
								|| scratchMap[i-1, j+1, 1] >= 1f
								|| scratchMap[i+1, j+1, 1] >= 1f
								|| scratchMap[i+1, j, 1] >= 1f
								|| scratchMap[i+1, j-1, 1] >= 1f
								|| scratchMap[i, j+1, 1] >= 1f
								|| scratchMap[i, j-1, 1] >= 1f
								|| scratchMap[i, j, 1] >= 1f)
								{
									splatMap[i, j, 1] = 1f;
									splatMap[i, j, 0] = 0f;
								}
								else
								{
									splatMap[i, j, 1] = 0f;
									splatMap[i, j, 0] = 1f;
								}
						}					
					}
				}
			}
			
			for (int i = 1; i < sourceMap.GetLength(0)-1; i++)
			{
				for (int j = 1; j < sourceMap.GetLength(1)-1; j++)
				{
					if (splatMap[i,j,1] ==1f)
					{
					scratchMap[i, j, 0] = splatMap[i, j, 0];
					scratchMap[i, j, 1] = splatMap[i, j, 1];
					}
				}
			}
			EditorUtility.ClearProgressBar();
		}
		
		
		for (int m = 0; m < sourceMap.GetLength(0); m++)
		{
			for (int o = 0; o < sourceMap.GetLength(0); o++)
			{
				if (hateMap[m, o, 0] > 0f  ^ scratchMap[m, o, 0] > 0f)
				{
					splatMap[m, o, 0] = float.MaxValue;
					splatMap[m, o, 1] = float.MinValue;
				}
				else
				{
					splatMap[m, o, 0] = float.MinValue;
					splatMap[m, o, 1] = float.MaxValue;
				}
				
			}
		}
		
        TerrainManager.SetData(splatMap, LandLayers.Topology,  TerrainTopology.TypeToIndex((int)layer.Topologies));
        TerrainManager.SetLayer(LandLayers.Topology,  TerrainTopology.TypeToIndex((int)layer.Topologies));
	}

	public static void notTopologyLayer(Layers layer, Layers sourceLayer)
	{
		float[,,] sourceMap = TerrainManager.GetSplatMap(LandLayers.Topology, TerrainTopology.TypeToIndex((int)sourceLayer.Topologies));
		float[,,] splatMap = TerrainManager.GetSplatMap(LandLayers.Topology, TerrainTopology.TypeToIndex((int)layer.Topologies));
		int res = sourceMap.GetLength(0);
		
		for (int m = 0; m < res; m++)
		{
			for (int o = 0; o < res; o++)
			{
				if ((splatMap[m, o, 0] > 0f && sourceMap[m, o, 0] > 0f))
				{
					splatMap[m, o, 0] = float.MinValue;
					splatMap[m, o, 1] = float.MaxValue;
				}
								
			}
		}
		
        TerrainManager.SetData(splatMap, LandLayers.Topology, TerrainTopology.TypeToIndex((int)layer.Topologies));
        TerrainManager.SetLayer(LandLayers.Topology, TerrainTopology.TypeToIndex((int)layer.Topologies));
		
	}

	public static void splatCrazing(CrazingPreset crazing)
	{
		int z = crazing.zones;
		int a = crazing.minSize;
		int b = crazing.maxSize;		
		int t = crazing.splatLayer;
		
		float[,,] newGround = TerrainManager.GroundArray;

		
		int s = UnityEngine.Random.Range(a, b);
		int uB = newGround.GetLength(0);
		
		for (int i = 0; i < z; i++)
        {
			EditorUtility.DisplayProgressBar("Painting", "Mottles",(i*1f/z));
			int x = UnityEngine.Random.Range(1, newGround.GetLength(0));
			int y = UnityEngine.Random.Range(1, newGround.GetLength(0));
            for (int j = 0; j < s; j++)
            {
					x = x + UnityEngine.Random.Range(-1,2);
					y = y + UnityEngine.Random.Range(-1,2);

					if (x <= 1)
						x = 2;
					
					if (y <= 1)
						y = 2;
					
					if (x >= uB)
						x = uB-1;
					
					if (y >= uB)
						y = uB-1;
						
					
					newGround[x, y, 0] = 0;
					newGround[x, y, 1] = 0;
					newGround[x, y, 2] = 0;
					newGround[x, y, 3] = 0;
					newGround[x, y, 4] = 0;
					newGround[x, y, 5] = 0;
					newGround[x, y, 6] = 0;
					newGround[x, y, 7] = 0;
					newGround[x, y, t] = 1;								
				
            }
        }
		EditorUtility.ClearProgressBar();
		TerrainManager.SetData(newGround, TerrainManager.LandLayer, 0);
		TerrainManager.SetLayer(TerrainManager.LandLayer, 0);
	}

		public static void pasteMonument(WorldSerialization blob, int x, int y, float zOffset)
	{
		
		
		EditorUtility.DisplayProgressBar("reeeLoading", "Monument File", .75f);
		//selectedLandLayer = null;
		WorldConverter.MapInfo terrains = WorldConverter.WorldToTerrain(blob);
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		var terrainPosition = 0.5f * terrains.size;
		
		float[,] pasteMap = terrains.land.heights;
		float[,] pasteWater = terrains.water.heights;
		float[,,] pSplat = terrains.splatMap;
		float[,,] pBiome = terrains.biomeMap;
		bool[,] pAlpha = terrains.alphaMap;
		//var topos = terrains.topology;
		int res = pSplat.GetLength(0);
		
		TerrainMap<int> pTopoMap = terrains.topology;
		TerrainMap<int> topTerrainMap = TopologyData.GetTerrainMap();
		
			
		land.transform.position = terrainPosition;
        float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
		float ratio = terrains.size.x / (baseMap.GetLength(0));
		
		int dim=baseMap.GetLength(0)-4;
		
		float x1 = x/2f;
		float y1 = y/2f;
		x=(int)(x/ratio);
		y=(int)(y/ratio);
		
		
		//arrays	
		float[,,] newGround = TerrainManager.GroundArray;
		float[,,] newBiome = TerrainManager.BiomeArray;
		bool[,] newAlpha = TerrainManager.AlphaArray;
		
		EditorUtility.ClearProgressBar();
		
		//find monument dimensions
		//and take a nap or something
		int helk = 0;
		
		for (int i = 0; i < dim; i++)
		{
			for (int j = 0; j < dim; j++)
			{

			if (pBiome[i, j,3] > 0.2f)
				{	
				 dim = j;
				 break;
				}
				
				helk = j;

			}

			if (pBiome[i, helk, 3] > 0.2f)
				{	
				 dim = i;
				 break;
				}
        }
		
		dim = dim + 25;
		if(dim == 25)
		{	dim = 0;   }
	
		//here comes the finale
		dim = 2040;
		
		for (int i = 0; i < dim; i++)
		{
			EditorUtility.DisplayProgressBar("Loading", "Monument Layers", (i*1f/dim));
			for (int j = 0; j < dim; j++)
			{

				baseMap[i + x, j + y] = Mathf.Lerp(baseMap[i+x, j+y], pasteMap[i,j]+zOffset, pBiome[i,j,0]);
				
				for (int k = 0; k < 8; k++)
				{
					newGround[i + x, j + y, k] = Mathf.Lerp(newGround[i+x,j+y,k], pSplat[i,j,k], pBiome[i,j,0]);
				}
				
				if (pBiome[i,j,0] > 0.1f)
				{
					topTerrainMap[i + x, j + y] = pTopoMap[i, j];
					
					newAlpha[i + x, j + y] = pAlpha[i, j];
				}
			
				
				
			}
			
			
        }
		
		EditorUtility.ClearProgressBar();
		TopologyData.InitMesh(topTerrainMap);
		
		land.terrainData.SetHeights(0,0,baseMap);
		TerrainManager.SetData(newGround, LandLayers.Ground, 0);
		TerrainManager.SetData(newBiome, LandLayers.Biome, 0);
        TerrainManager.SetData(newAlpha, LandLayers.Alpha);
		
		for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            //TerrainManager.SetData(TerrainManager.GetSplatMap(TerrainTopology.IndexToType(i)), LandLayers.Topology, i);
			TerrainManager.SetData(TerrainManager.GetSplatMap(LandLayers.Topology, i), LandLayers.Topology, i);
        }
		
		
		
		
		
		Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
		GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        
		
		for (int i = 0; i < terrains.prefabData.Length; i++)
        {
			
			terrains.prefabData[i].position.x = terrains.prefabData[i].position.x+y1*2f;
			terrains.prefabData[i].position.z = terrains.prefabData[i].position.z+x1*2f;
			terrains.prefabData[i].position.y = terrains.prefabData[i].position.y + zOffset*1000f;
			
			GameObject newObj = PrefabManager.SpawnPrefab(defaultObj, terrains.prefabData[i], prefabsParent);
            newObj.GetComponent<PrefabDataHolder>().prefabData = terrains.prefabData[i];
			
			
        }
		
		Transform pathsParent = GameObject.FindGameObjectWithTag("Paths").transform;
        GameObject pathObj = Resources.Load<GameObject>("Paths/Path");
        for (int i = 0; i < terrains.pathData.Length; i++)
        {
            Vector3 averageLocation = Vector3.zero;
            for (int j = 0; j < terrains.pathData[i].nodes.Length; j++)
            {
                averageLocation += terrains.pathData[i].nodes[j];
				terrains.pathData[i].nodes[j].x = terrains.pathData[i].nodes[j].x + y1*2f;
				terrains.pathData[i].nodes[j].z = terrains.pathData[i].nodes[j].z + x1*2f;
				terrains.pathData[i].nodes[j].y = terrains.pathData[i].nodes[j].y + zOffset*1000f;
				
            }
            
			averageLocation /= terrains.pathData[i].nodes.Length;
            
			GameObject newObject = GameObject.Instantiate(pathObj, averageLocation + terrainPosition, Quaternion.identity, pathsParent);
            newObject.GetComponent<PathDataHolder>().pathData = terrains.pathData[i];
        }
		
		
	}
	
	public static void insertPrefabCliffs(GeologyPreset geo)
	{
		
		uint featPrefabID = geo.prefabID;
		Vector3 rotationRange1 = geo.rotationsLow;
		Vector3 rotationRange2 = geo.rotationsHigh;
		Vector3 scaleRange1 = geo.scalesLow;
		Vector3 scaleRange2 = geo.scalesHigh;
		
		float zOffset = geo.zOffset;
		float floor = geo.floor/1000f;
		float ceiling = geo.ceiling/1000f;
		
		float s1 = geo.slopeLow;
		float s2 = geo.slopeHigh;
		int density = geo.density;
		int thinnitude = geo.frequency;
		
		bool avoid = geo.avoidTopo;
		bool tilting = geo.tilting;
		bool flipping = geo.flipping;
		
		bool normalizeX = false, normalizeY = false, normalizeZ = false;
		
		
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
		float[,,] avoidMap = TerrainManager.TopologyArray[TerrainTopology.TypeToIndex(1024)];
		float[,,] avoidMap1 = TerrainManager.TopologyArray[TerrainTopology.TypeToIndex(2048)];
		
		
		Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
		GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        
		int count = 0;
		int res = baseMap.GetLength(0);
		int size = (int)land.terrainData.size.x;
		int sizeZ = (int)land.terrainData.size.y;
		
		Vector3 position;
		Vector3 rRotate;
		Vector3 rScale;
		Vector3 normal = new Vector3(0,0,0);
		
		float[] heights = new float[9];
		
		float geology = new float();
		int flipX=0;
		int flipZ=0;
		float slope=0;
		float avoider = 1f;
		float[,] cliffMap = new float[res,res];
		
		float slopeDiff = 0f;
		float oldPixel = 0f;
		float newPixel = 0f;
		float randomizer = 0f;
		
		float xNormalizer = 0f;
		float yNormalizer = 0f;
		float zNormalizer = 0f;
		
		float quotient = 0f;		
		
		float height=0f;
		float tempHeight=0f;
		int xOff=0;
		int yOff=0;
		
		float ratio = land.terrainData.size.x / (baseMap.GetLength(0));
		
		if (avoid)
		{
			avoider=.01f;
		}		
		
		for (int i = 0; i < res; i++)
        {
			EditorUtility.DisplayProgressBar("Generating", "Slope Map",(i*1f/res));
            for (int j = 0; j < res; j++)
            {
				
				cliffMap[i,j] = (land.terrainData.GetSteepness(1f*j/res, 1f*i/res))/90 * (density / 100f);
			
			}
		}
		EditorUtility.ClearProgressBar();

		for (int i = 2; i < res-2; i++)
        {
			EditorUtility.DisplayProgressBar("Dithering", "Cliff Map",(i*1f/res));
            for (int j = 2; j < res-2; j++)
            {
				
				oldPixel = cliffMap[i,j];
				
				if (cliffMap[i,j] >= .5f)
					{
						newPixel = 1f;
					}
				else
					{
						newPixel = 0f;
					}
					
				cliffMap[i,j] = newPixel;
				
				slopeDiff = (oldPixel-newPixel);
				
				
				randomizer = UnityEngine.Random.Range(0f,1f);
				quotient = 42f;
				
				cliffMap[i+1,j] = cliffMap[i+1,j] + (slopeDiff * 8f * randomizer / quotient);
				cliffMap[i-1,j+1] = cliffMap[i-1,j+1] + (slopeDiff * 4f * randomizer /quotient);
				cliffMap[i,j+1] = cliffMap[i,j+1] + (slopeDiff * 8f * randomizer / quotient);
				cliffMap[i+1,j+1] = cliffMap[i+1,j+1] + (slopeDiff * 4f * randomizer / quotient);

				randomizer = UnityEngine.Random.Range(1f,1.5f);
				
				cliffMap[i+2,j] = cliffMap[i+2,j] + (slopeDiff * 4f * randomizer / quotient);
				cliffMap[i-2,j+1] = cliffMap[i-2,j+1] + (slopeDiff * 2f * randomizer / quotient);
				cliffMap[i,j+2] = cliffMap[i,j+2] + (slopeDiff * 4f * randomizer / quotient);
				cliffMap[i+2,j+1] = cliffMap[i+2,j+1] + (slopeDiff * 2f * randomizer / quotient);
				
				randomizer = UnityEngine.Random.Range(1f,2f);
				
				cliffMap[i+1,j+2] = cliffMap[i+1,j+2] + (slopeDiff * 2f * randomizer / quotient);
				cliffMap[i-2,j+2] = cliffMap[i-2,j+2] + (slopeDiff * 1f * randomizer /quotient);
				cliffMap[i-1,j+2] = cliffMap[i-1,j+2] + (slopeDiff * 2f * randomizer / quotient);
				cliffMap[i+2,j+2] = cliffMap[i+2,j+2] + (slopeDiff * 1f * randomizer / quotient);
				
			}
		}
		EditorUtility.ClearProgressBar();
		for (int i = 1; i < res-1; i++)
        {
			EditorUtility.DisplayProgressBar("Spawning", "Geology features",(i*1f/res));
            for (int j = 1; j < res-1; j++)
            {
				slope = land.terrainData.GetSteepness(1f*j/res, 1f*i/res);
				
						
				
				if(baseMap[i,j] > floor && baseMap[i,j] < ceiling && avoidMap[i,j,0] < avoider && avoidMap1[i,j,0] < avoider && cliffMap[i,j]>=.5f && slope > s1 && slope < s2)
				{

									
									height = 0f;
									tempHeight= 0f;
									for (int n = -1; n < 2; n++)
									{
										for (int o = -1; o < 2; o++)
										{
											tempHeight = baseMap[i+n, j+o];
											
											if (height < tempHeight)
											{
												height = tempHeight;
												xOff = n;
												yOff = o;
											}
											
											
											
										}
									}
									
									
								
									if(flipping)
									{
									flipX = UnityEngine.Random.Range(0,2) * 180;
									flipZ = UnityEngine.Random.Range(0,2) * 180;
									}
									//lean and displace each rock for 'geology'
									if(tilting)
									{
									geology = (Mathf.PerlinNoise(i*1f/80,j*1f/80))*20;
									}
									//geolog = 0f;
									//position is nearest highest pixel + zoffset
									position = new Vector3(j *ratio-(size/2f)+yOff*ratio, height * sizeZ - (sizeZ*.5f) + zOffset,i *ratio-(size/2f)+xOff*ratio);
									
									//rotation gets geology and randomization
									//normalization
									
									normal = land.terrainData.GetInterpolatedNormal(1f*j/res, 1f*i/res);
									
									if(normalizeX)
									{
									xNormalizer = normal.x*90f;
									}
									
									if(normalizeY)
									{
									yNormalizer = normal.y*90f;
									}
									
									if(normalizeZ)
									{
									zNormalizer = normal.z*90f;
									}
									
									
									rRotate = new Vector3(xNormalizer + UnityEngine.Random.Range(rotationRange1.x, rotationRange2.x) + geology + flipX, yNormalizer + UnityEngine.Random.Range(rotationRange1.y, rotationRange2.y), zNormalizer + UnityEngine.Random.Range(rotationRange1.z,rotationRange2.z) + flipZ);
									rScale = new Vector3(UnityEngine.Random.Range(scaleRange1.x, scaleRange2.x), UnityEngine.Random.Range(scaleRange1.y, scaleRange2.y), UnityEngine.Random.Range(scaleRange1.z,scaleRange2.z));
									
									//public static void createPrefab(string category, uint id, Vector3 position, Vector3 rotation, Vector3 scale)
									if(UnityEngine.Random.Range(0,thinnitude) == 2)
									{
									PrefabManager.createPrefab("Decor", featPrefabID, position, rRotate, rScale);
									count++;
									}
									
									
									
				}
				
				
            }
			
        }
		EditorUtility.ClearProgressBar();
		Debug.LogError("Geology Complete: " + count + " Features Placed.");
	}
}