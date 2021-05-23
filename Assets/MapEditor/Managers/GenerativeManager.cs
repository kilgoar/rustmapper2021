using UnityEngine;
using UnityEditor;
using RustMapEditor.Variables;
using static WorldSerialization;
using static TerrainManager;
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
							if (perlin)
							{
								perlinShape[i,j] = Mathf.PerlinNoise(i*1f/s+r, j*1f/s+r1)*2f;
							}
							else
							{
								perlinShape[i,j] = 1f;
							}
							
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
	
	public static Vector2 minmaxHeightmap()
	{
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
		Vector2 minmax = new Vector2(0f,1000f);
		for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					minmax.x = Math.Max(minmax.x, baseMap[i,j]);
					minmax.y = Math.Min(minmax.y, baseMap[i,j]);
				}
			}
			return minmax;
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
	
	public static void FlipHeightmap()
	{
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					baseMap[i,j] = baseMap[i,j]*-1f+1f;
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
	
	public static void lakeTopologyFill(Layers layer)
	{
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
		float[,,] topoMap = TerrainManager.GetSplatMap(LandLayers.Topology, TerrainTopology.TypeToIndex((int)layer.Topologies));
		int res = topoMap.GetLength(0);
		int dim = heightMap.GetLength(0);
		float ratio  = 1f*dim/res;
		int xCheck = 0;
		int yCheck = 0;
		float[,,] lakeMap = new float[res,res,2];
		for (int i = 0; i < res; i++)
		{
			for (int j = 0; j < res; j++)
			{
				xCheck = ((int)(1f*i*ratio));
				yCheck =((int)(1f*j*ratio));
				if (heightMap[xCheck,yCheck] < .499f)
				{
					lakeMap[i,j,1] = 0f;
					lakeMap[i,j,0] = 1f;
				}
				else
				{
					lakeMap[i,j,1] = 1f;
					lakeMap[i,j,0] = 0f;
				}
			}
		}
		
		Point lake;
		lake.X = 3;
		lake.Y = 3;

		lakeMap = topoDeleteFill(lake, lakeMap);
		TerrainManager.SetData(lakeMap, LandLayers.Topology,  TerrainTopology.TypeToIndex((int)layer.Topologies));
        TerrainManager.SetLayer(LandLayers.Topology,  TerrainTopology.TypeToIndex((int)layer.Topologies));

	}
	
	
	public static int monumentDataLength(monumentData [] array)
	{
		int count=0;
		foreach (monumentData monument in array)
		{
			if (monument.x != 0)
				count++;
		}
		
		return count;
	}

	public static monumentData [] monumentLocations(float [,,] biomeMap)
	{
		int res = biomeMap.GetLength(0);
		float[,,] analysisMap = new float[res,res,2];
		
		monumentData[] monument = new monumentData[300];
		
		for (int i = 0; i < res; i++)
		{
			for (int j = 0; j < res; j++)
			{
				if (biomeMap[i,j,0] != 0)
				{
					analysisMap[i,j,1] = 0f;
					analysisMap[i,j,0] = 1f;
				}
				else
				{
					analysisMap[i,j,1] = 1f;
					analysisMap[i,j,0] = 0f;
				}
			}
		}
		
		Stack<Point> pixels = new Stack<Point>();
		Point p;
		int maxX, maxY, minX, minY;
		int count = -1;
		
		for (int i = 0; i < res; i++)
		{
			for (int j = 0; j < res; j++)
			{
				if (analysisMap[i,j,1] == 0f && count < 299)
				{
							pixels = new Stack<Point>();
							p.X = i;
							p.Y = j;
							
							maxX = i;
							minX = i;
							maxY = j;
							minY = j;
							
							count++;
							
							pixels.Push(p);
							
							float target =0f;
							
							while (pixels.Count != 0)
							{
								
								Point temp = pixels.Pop();
								int y1 = temp.Y;
								
								while (y1 >= 0 && analysisMap[temp.X, y1,1] == target)
								{
									y1--;
								}
								y1++;
								bool spanLeft =false;
								bool spanRight =false;
								while (y1 < res && analysisMap[temp.X,y1,1] == target)
								{
									analysisMap[temp.X, y1, 1] = 1f;
									analysisMap[temp.X, y1, 0] = 0f;
									
									maxX = Math.Max(temp.X, maxX);
									minX = Math.Min(temp.X, minX);
									maxY = Math.Max(y1, maxY);
									minY = Math.Min(y1, minY);
									
									if(!spanLeft && temp.X > 0 && analysisMap[temp.X-1, y1,1] == target)
									{
										pixels.Push(new Point(temp.X -1, y1));
										spanLeft=true;
									}
									else if(spanLeft && temp.X -1 >= 0 && (analysisMap[temp.X-1, y1, 1] != target))
									{
										spanLeft=false;
									}
									if(!spanRight && temp.X < res - 1 && analysisMap[temp.X+1, y1, 1] == target)
									{
										pixels.Push(new Point(temp.X +1, y1));
										spanRight=true;
									}
									else if(spanRight && temp.X < res - 1 && (analysisMap[temp.X+1, y1, 1] != target))
									{
										spanRight=false;
									}
									y1++;
								}
									
							}
							
					monument[count] = new monumentData(minX, minY, maxX-minX, maxY-minY);
				}
			}
		}
		Debug.LogError(count);
		return monument;
		
	}
	
	public static void oceanTopologyFill(Layers layer)
	{
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
		float[,,] topoMap = TerrainManager.GetSplatMap(LandLayers.Topology, TerrainTopology.TypeToIndex((int)layer.Topologies));
		
		
		int res = topoMap.GetLength(0);
		int dim = heightMap.GetLength(0);
		float ratio  = 1f*dim/res;
		float[,,] oceanMap = new float[res,res,2];
		float[,,] lakeMap = new float[res,res,2];
		int xCheck = 0;
		int yCheck = 0;
		for (int i = 0; i < res; i++)
		{
			for (int j = 0; j < res; j++)
			{
				xCheck = ((int)(1f*i*ratio));
				yCheck =((int)(1f*j*ratio));
				
				if (heightMap[xCheck,yCheck] < .499f)
				{
					lakeMap[i,j,1] = 0f;
					lakeMap[i,j,0] = 1f;
					oceanMap[i,j,1] = 0f;
					oceanMap[i,j,0] = 1f;
				}
				else
				{
					lakeMap[i,j,1] = 1f;
					lakeMap[i,j,0] = 0f;
					oceanMap[i,j,1] = 1f;
					oceanMap[i,j,0] = 0f;
				}
			}
		}
		
		Point lake;
		lake.X = 3;
		lake.Y = 3;

		lakeMap = topoDeleteFill(lake, lakeMap);
		
		
		for (int i = 0; i < res; i++)
		{
			for (int j = 0; j < res; j++)
			{
				if (oceanMap[i,j,1] == lakeMap[i,j,1])
				{
					oceanMap[i,j,1] = 1f;
					oceanMap[i,j,0] = 0f;
				}
			}
		}
		
		TerrainManager.SetData(oceanMap, LandLayers.Topology,  TerrainTopology.TypeToIndex((int)layer.Topologies));
        TerrainManager.SetLayer(LandLayers.Topology,  TerrainTopology.TypeToIndex((int)layer.Topologies));

	}
	
	public static float [,,] topoDeleteFill(Point p, float[,,] topoMap)
	{

		Stack<Point> pixels = new Stack<Point>();
		int count=0;
		int res = topoMap.GetLength(0);
		pixels.Push(p);
		
		float target =0f;
		
		while (pixels.Count != 0)
		{
			count++;
			Point temp = pixels.Pop();
			int y1 = temp.Y;
			
			while (y1 >= 0 && topoMap[temp.X, y1,1] == target)
			{
				y1--;
			}
			y1++;
			bool spanLeft =false;
			bool spanRight =false;
			while (y1 < res && topoMap[temp.X,y1,1] == target)
			{
				topoMap[temp.X, y1, 1] = 1f;
				topoMap[temp.X, y1, 0] = 0f;
				
				if(!spanLeft && temp.X > 0 && topoMap[temp.X-1, y1,1] == target)
				{
					pixels.Push(new Point(temp.X -1, y1));
					spanLeft=true;
				}
				else if(spanLeft && temp.X -1 >= 0 && (topoMap[temp.X-1, y1, 1] != target))
				{
					spanLeft=false;
				}
				if(!spanRight && temp.X < res - 1 && topoMap[temp.X+1, y1, 1] == target)
				{
					pixels.Push(new Point(temp.X +1, y1));
					spanRight=true;
				}
				else if(spanRight && temp.X < res - 1 && (topoMap[temp.X+1, y1, 1] != target))
				{
					spanRight=false;
				}
				y1++;
			}
				
		}
		Debug.LogError(count);
		return topoMap;
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
		WorldConverter.MapInfo terrains = WorldConverter.WorldToTerrain(blob);
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		var terrainPosition = 0.5f * terrains.size;
		
		float[,] pasteMap = terrains.land.heights;
		float[,] pasteWater = terrains.water.heights;
		float[,,] pSplat = terrains.splatMap;
		float[,,] pBiome = terrains.biomeMap;
		bool[,] pAlpha = terrains.alphaMap;
		
		TerrainMap<int> pTopoMap = terrains.topology;
		TerrainMap<int> topTerrainMap = TopologyData.GetTerrainMap();
		
		land.transform.position = terrainPosition;
        float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
		float ratio = terrains.size.x / (baseMap.GetLength(0));
		
		int dim=pSplat.GetLength(0)-4;
		int heightmapDim = baseMap.GetLength(0)-4;
		float ratioMaps = 1f * heightmapDim / dim;
		
		float x1 = x/2f;
		float y1 = y/2f;
		x=(int)(x/ratio);
		y=(int)(y/ratio);
		
		float[,,] newGround = TerrainManager.GroundArray;
		float[,,] newBiome = TerrainManager.BiomeArray;
		bool[,] newAlpha = TerrainManager.AlphaArray;
		
		float[][,,] topologyArray  = TerrainManager.TopologyArray;
		float[][,,] pTopologyArray =  new float[30][,,];
		
		int splatMapsX=0;
		int splatMapsY=0;
		
		pTopologyArray = TopomapToArray(pTopoMap,dim);
			
		
		for (int i = 0; i < heightmapDim; i++)
		{
			EditorUtility.DisplayProgressBar("Loading", "Heightmap", (i*1f/heightmapDim));
			for (int j = 0; j < heightmapDim; j++)
			{
				splatMapsX = (int)(1f* i / ratioMaps);
				splatMapsY = (int)(1f* j / ratioMaps);
				splatMapsX = (int)Mathf.Clamp(splatMapsX, 0f, heightmapDim *1f);
				splatMapsY = (int)Mathf.Clamp(splatMapsY, 0f, heightmapDim *1f);
				baseMap[i + x, j + y] = Mathf.Lerp(baseMap[i+x, j+y], pasteMap[i,j]+zOffset, pBiome[splatMapsX,splatMapsY,0]);
			}
		}
		
		EditorUtility.ClearProgressBar();
		for (int i = 0; i < dim; i++)
		{
			EditorUtility.DisplayProgressBar("Loading", "Monument Layers", (i*1f/dim));
			for (int j = 0; j < dim; j++)
			{
				for (int k = 0; k < 8; k++)
				{
					newGround[i + x, j + y, k] = Mathf.Lerp(newGround[i+x,j+y,k], pSplat[i,j,k], pBiome[i,j,0]);
				}
				
				if (pBiome[i,j,0] > 0f)
				{
					for(int k = 0; k < TerrainTopology.COUNT; k++)
					{
						topologyArray[k][i + x, j + y,0] = pTopologyArray[k][i, j,0];
						topologyArray[k][i + x, j + y,1] = pTopologyArray[k][i, j,1];
					}
					
					newAlpha[i + x, j + y] = pAlpha[i, j];
				}
			
			}
			
			
        }
		
		EditorUtility.ClearProgressBar();
		land.terrainData.SetHeights(0,0,baseMap);
		TerrainManager.SetData(newGround, LandLayers.Ground, 0);
		TerrainManager.SetData(newBiome, LandLayers.Biome, 0);
        TerrainManager.SetData(newAlpha, LandLayers.Alpha);
		TerrainManager.SetLayer(TerrainManager.LandLayer, 0);
		
		
		for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
			TerrainManager.SetData(topologyArray[i], LandLayers.Topology, i);
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
		
		Transform PathParent = GameObject.FindGameObjectWithTag("Paths").transform;
		int progressID = Progress.Start("Load: " + "", "Preparing Map", Progress.Options.Sticky);
		int spwPath = Progress.Start("Paths", null, Progress.Options.Sticky, progressID);
		PathManager.SpawnPaths(terrains.pathData,spwPath);
		/*
		Debug.LogError(terrains.pathData.Length);
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
		*/
		
	}
	
	public static void rustBuildings()
	{
		int buildings = 300;
		int dim = (int)Mathf.Sqrt(buildings);
		int maxWidth = 4;
		int maxBreadth = 3;
		int maxHeight = 5;
		
		int start = -1000;
		int buildingSize = Math.Max(maxWidth,maxBreadth)*6+18;
		
		for (int i = 0; i < dim; i++)
		{
			for (int j = 0; j < dim; j++)
			{
				createRustBuilding(i*buildingSize+start,j*buildingSize+start, UnityEngine.Random.Range(1,maxWidth +1),UnityEngine.Random.Range(1,maxBreadth +1),UnityEngine.Random.Range(1,maxHeight +1));
			}
		}
	}
	
	
	public static void createRustBuilding(int x, int y, int width, int breadth, int tallest)
	{
		uint industrialglassnt = 1048750230;
		uint yellow = 2337881356;
		uint sewerCorner = 2032918088;
		int glassScale = 6;
		float z = 9.36f;
		float yRotation;
		int height = 0;
		int tallness = 0;
		Vector3 position = new Vector3(0f,0f,0f);
		Vector3 rotation = new Vector3(0f,0f,0f);
		Vector3 containerRotation = new Vector3(0f,0f,0f);
		Vector3 scale = new Vector3(glassScale,glassScale,glassScale);
		
		Vector3 containerScale = new Vector3(.689f, .510f, .675f);
		Vector3 sewerScale = new Vector3(1f,1f,1f);
		Vector3 sewerRotation = new Vector3(0f,0f,0f);
		Vector3 sewerRotation2 = new Vector3(0f,180f,0f);
		float containerYoffset = 1.511f;
		float containerZoffset = 2.03f;
		float foundationOffset = 2.9f;
		//PrefabManager.createPrefab("Decor", foliage, foliageLocation, foliageRotation, foliageScale);
		
		
		for (int i = 0; i < width; i++)
		{
			for(int j = 0; j < breadth; j++)
			{
				tallness = UnityEngine.Random.Range(0, tallest +1);
				for(int k = 0; k<tallness; k++)
				{
					rotation.y = UnityEngine.Random.Range(0, 4) * 90f;
					position.x = i*glassScale+y;
					position.z = j*glassScale+x;
					position.y = k*glassScale + z;
					PrefabManager.createPrefab("Decor", industrialglassnt, position, rotation, scale);
					position.y = position.y + containerYoffset;
					PrefabManager.createPrefab("Decor", yellow, position, containerRotation, containerScale);
					position.z = position.z + containerZoffset;
					PrefabManager.createPrefab("Decor", yellow, position, containerRotation, containerScale);
					position.z = position.z - (containerZoffset * 2f);
					PrefabManager.createPrefab("Decor", yellow, position, containerRotation, containerScale);
					if (k==0)
					{					
					position.x = i*glassScale+y-foundationOffset;
					position.z = j*glassScale+x-foundationOffset;
					position.y = k*glassScale+z-foundationOffset;
					
					PrefabManager.createPrefab("Decor", sewerCorner, position, sewerRotation2, sewerScale);
					
					position.x = i*glassScale+y+foundationOffset;
					position.z = j*glassScale+x+foundationOffset;
					position.y = k*glassScale+z-foundationOffset;
					
					PrefabManager.createPrefab("Decor", sewerCorner, position, sewerRotation, sewerScale);
					
					
					}
				}
			}
		}
		
		
	}
	
	
	public static void createRustCity(WorldSerialization blob, RustCityPreset city)
	{
		WorldConverter.MapInfo terrains = WorldConverter.WorldToTerrain(blob);
		monumentData [] monuments = monumentLocations(terrains.biomeMap);
		int lane = city.street;
		int height = city.alley;
		int dim = city.size;
		int start = city.start;
		int x = start;
		int y = start;
		city.x = x;
		city.y = y;
		int k = 0;
		int buildings = monumentDataLength(monuments);
		
		
		EditorUtility.DisplayProgressBar("Generating", "building: " + k, ((y*x*1f)/(dim*dim)));
		while (y < start + dim)
		{
			
			while (x < start + dim)
			{
				
				k = UnityEngine.Random.Range(0,buildings);
				city.x = x;
				city.y = y;
				RustCity(terrains, monuments[k], city);
				x+= (monuments[k].width + lane);
			}
			y += height;
			x = start; 
		}
		EditorUtility.ClearProgressBar();
		
	}
	
	public static float[][,,] TopomapToArray(TerrainMap<int> pTopoMap, int res)
	{
	float[][,,] pTopologyArray =  new float[TerrainTopology.COUNT][,,];
	
				for(int k = 0; k < TerrainTopology.COUNT;k++)
				{
					pTopologyArray[k] = new float[res,res,2];
				}
		
		for (int i = 0; i < res; i++)
		{
			for (int j = 0; j < res; j++)
			{
				for(int k = 0; k < TerrainTopology.COUNT;k++)
				{
					
					if((pTopoMap[i,j] & TerrainTopology.IndexToType(k)) != 0)
					{
						pTopologyArray[k][i,j,0] = 1f;
						pTopologyArray[k][i,j,1] = 0f;
					}
					else
					{
						pTopologyArray[k][i,j,1] = 1f;
						pTopologyArray[k][i,j,0] = 0f;
					}
				}
			}
		}
		
	return pTopologyArray;
	}
	
	public static void RustCity(WorldConverter.MapInfo terrains, monumentData monument, RustCityPreset city)
	{
		int x = city.x;
		int y = city.y;
		float zOff = city.zOff;
		float steepness = city.flatness;
		float zOffset=0;
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		var terrainPosition = 0.5f * terrains.size;
		
		float[,] pasteMap = terrains.land.heights;
		float[,] pasteWater = terrains.water.heights;
		float[,,] pSplat = terrains.splatMap;
		float[,,] pBiome = terrains.biomeMap;
		bool[,] pAlpha = terrains.alphaMap;
		
		int res = pasteMap.GetLength(0);
		int splatRes = pSplat.GetLength(0);
		
		TerrainMap<int> pTopoMap = terrains.topology;
		float[][,,] pTopologyArray =  new float[TerrainTopology.COUNT][,,];
		pTopologyArray = TopomapToArray(pTopoMap, splatRes);
		float[][,,] topologyArray  = TerrainManager.TopologyArray;
		
		land.transform.position = terrainPosition;
        float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
		float ratio = terrains.size.x / (baseMap.GetLength(0));
		
		int dim=baseMap.GetLength(0)-4;
		
		int monumentX = monument.x;
		int monumentY = monument.y;
		int width = monument.width;
		int height = monument.height;
		

		
		float[,,] newGround = TerrainManager.GetSplatMap(LandLayers.Ground, 0);
		float[,,] newBiome = TerrainManager.GetSplatMap(LandLayers.Biome, 0);
		bool[,] newAlpha = TerrainManager.AlphaArray;
		
		
		
		float splatRatio = terrains.size.x / pBiome.GetLength(0);
		
		float mapSplatRatio = splatRes / newBiome.GetLength(0);
		
		
		
		EditorUtility.ClearProgressBar();
		
		float ratioMaps = 1f * res / splatRes;
		int heightmapDim = baseMap.GetLength(0)-4;
		
		float x2 = monumentX*splatRatio;
		float y2 = monumentY*splatRatio;
		float x1 = x*splatRatio;
		float y1 = y*splatRatio;
		//x=(int)(x/ratio);
		//y=(int)(y/ratio);
		
		float sum = 0;
		float sum1= 0;
		int count = 0;
		int xCheck = 0;
		int yCheck = 0;
		
		float maxZ = 0;
		float minZ = 1;
		float zDiff = 0;

		int heightMapsX = 0;
		int heightMapsY = 0;
		int heightMapsX1 = 0;
		int heightMapsY1 = 0;
		int biomeMaskX = 0;
		int biomeMaskY = 0;
		
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (pBiome[i,j,1] > 0f)
				{
				heightMapsX = (int)(1f*(i+x) * ratioMaps);
				heightMapsY = (int)(1f*(j+y) * ratioMaps);
				heightMapsX1 = (int)(1f*(i+monumentX) * ratioMaps);
				heightMapsY1 = (int)(1f*(j+monumentY) * ratioMaps);

				count++;
				maxZ = Math.Max(maxZ, baseMap[heightMapsX,heightMapsY]);
				minZ = Math.Min(minZ, baseMap[heightMapsX,heightMapsY]);
				sum += pasteMap[heightMapsX1,heightMapsY1];
				sum1 += baseMap[heightMapsX,heightMapsY];
				}
			}
		}
		
		zDiff = maxZ-minZ;
		zOffset = (sum1/count)-(sum/count)+(.5f*zDiff) + zOff;

		if (zDiff<steepness)
		{
			for (int i = 0; i < width; i++)
					{
						
						
						EditorUtility.DisplayProgressBar("Loading", "Heightmap", (i*1f/heightmapDim));
						
						for (int j = 0; j < height; j++)
						{
							/*
							heightMapsX = (int)(1f*i+(x * ratioMaps));
							heightMapsY = (int)(1f*j+(y * ratioMaps));
							
							heightMapsX1 = (int)(1f*i+(monumentX * ratioMaps));
							heightMapsY1 = (int)(1f*j+(monumentY * ratioMaps));
							biomeMaskX = (int)(1f*(i/ ratioMaps)+monumentX);
							biomeMaskY = (int)(1f*(j/ ratioMaps)+monumentY);
							*/
							baseMap[i+x, j+y] = Mathf.Lerp(baseMap[i+x, j+y], pasteMap[i+monumentX,j+monumentY]+zOffset, pBiome[i+monumentX,j+monumentY,0]);
						}
					}
					
					//width = (int)(width / ratioMaps);
					//height = (int)(height / ratioMaps);
					
					EditorUtility.ClearProgressBar();
					for (int i = 0; i < (int)(width/mapSplatRatio); i++)
					{
						EditorUtility.DisplayProgressBar("Loading", "Monument Layers", (i*1f/dim));
						for (int j = 0; j < (int)(height/mapSplatRatio); j++)
						{
							
							
							for (int k = 0; k < 8; k++)
							{
								newGround[i + (int)(x/mapSplatRatio), j + (int)(y/mapSplatRatio), k] = Mathf.Lerp(newGround[i + (int)(x/mapSplatRatio), j + (int)(y/mapSplatRatio), k], pSplat[(int)(i*mapSplatRatio)+monumentX,(int)(j*mapSplatRatio)+monumentY,k], pBiome[(int)(i*mapSplatRatio)+monumentX,(int)(j*mapSplatRatio)+monumentY,0]);
							}
							
							if (pBiome[i,j,0] > 0f)
							{
								for(int k = 0; k < TerrainTopology.COUNT; k++)
								{
									topologyArray[k][i + (int)(x/mapSplatRatio), j + (int)(y/mapSplatRatio),0] = pTopologyArray[k][(int)(i*mapSplatRatio)+monumentX, (int)(j*mapSplatRatio)+monumentY,0];
									topologyArray[k][i + (int)(x/mapSplatRatio), j + (int)(y/mapSplatRatio),1] = pTopologyArray[k][(int)(i*mapSplatRatio)+monumentX, (int)(j*mapSplatRatio)+monumentY,1];
								}
								
								//newAlpha[i + x, j + y] = pAlpha[i+monumentX, j+monumentY];
							}
							
						}
						
						
					}
				
						
				land.terrainData.SetHeights(0,0,baseMap);

				Debug.LogError(topologyArray[0].GetLength(0)+" topology length");
				SetData(newGround, LandLayers.Ground, 0);
				
				//TerrainManager.SetData(newBiome, LandLayers.Biome, 0);
				//TerrainManager.SetData(newAlpha, LandLayers.Alpha);
				
				for (int i = 0; i < TerrainTopology.COUNT; i++)
				{
					TerrainManager.SetData(topologyArray[i], LandLayers.Topology, i);
				}
				SetLayer(TerrainManager.LandLayer, 0);
				
				Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
				GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
				
				int prefabcounter=0;
				uint id = 0;

				uint industrialglassnt = 1048750230;

				
				Vector3 holderPosition = new Vector3(0,0,0);
				
				int palette = UnityEngine.Random.Range(0,9);
				
				for (int i = 0; i < terrains.prefabData.Length; i++)
				{
					xCheck = (int)((terrains.prefabData[i].position.z/splatRatio)+res/2f);
					yCheck = (int)((terrains.prefabData[i].position.x/splatRatio)+res/2f);
					
					
					if( xCheck > monumentX && xCheck < monumentX+width && yCheck > monumentY && yCheck < monumentY+height )
					{
							id = terrains.prefabData[i].id;
							
							holderPosition.x = terrains.prefabData[i].position.x+y1-y2;
							holderPosition.z = terrains.prefabData[i].position.z+x1-x2;
							holderPosition.y = terrains.prefabData[i].position.y + zOffset*1000f;
							id = PrefabManager.ScrambleContainer(terrains.prefabData[i].id, palette);
							
							if(terrains.prefabData[i].id == industrialglassnt)
							{	
								PrefabManager.FoliageCube(holderPosition, terrains.prefabData[i].scale);
							}

							PrefabManager.createPrefab(terrains.prefabData[i].category, id, holderPosition, terrains.prefabData[i].rotation, terrains.prefabData[i].scale);
							prefabcounter++;
					}
					
					
				}
				/*
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
				*/
		}
	}
	
	public static void insertPrefabCliffs(GeologyPreset geo)
	{
		uint featPrefabID = 0;
		int roll = 0;
		
		
		
		Vector3 rotationRange1 = geo.rotationsLow;
		Vector3 rotationRange2 = geo.rotationsHigh;
		Vector3 scaleRange1 = geo.scalesLow;
		Vector3 scaleRange2 = geo.scalesHigh;
		
		float zOffset = geo.zOffset;
		float floor = geo.floor/1000f;
		float ceiling = geo.ceiling/1000f;
		float cliffFade = 0f;
		float s1 = geo.slopeLow;
		float s2 = geo.slopeHigh;
		int density = geo.density;
		int thinnitude = geo.frequency;
		
		bool avoid = geo.avoidTopo;
		bool tilting = geo.tilting;
		bool flipping = geo.flipping;
		
		
		
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution);
		float[,,] avoidMap = TerrainManager.TopologyArray[TerrainTopology.TypeToIndex(1024)];
		float[,,] avoidMap1 = TerrainManager.TopologyArray[TerrainTopology.TypeToIndex(2048)];
		float[,,] biomeMap = TerrainManager.BiomeArray;
		
		Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
		GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        
		int count = 0;
		int res = baseMap.GetLength(0);
		int splatRes = avoidMap.GetLength(0);
		float resRatio = 1f * res/splatRes;
		int heightMapsX, heightMapsY;
		
		int size = (int)land.terrainData.size.x;
		int sizeZ = (int)land.terrainData.size.y;
		
		Vector3 position;
		Vector3 rRotate, preRotate;
		Vector3 rScale;
		Vector3 normal = new Vector3(0,0,0);
		Quaternion qRotate;
		
		float[] heights = new float[9];
		
		float geology = new float();
		int flipX=0;
		int flipZ=0;
		float slope=0;
		float avoider = 1f;
		float[,] cliffMap = new float[splatRes,splatRes];
		
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
		
		float ratio = 1f* land.terrainData.size.x / splatRes;
		
		
		if (avoid)
		{
			avoider=.01f;
		}		
		
		for (int i = 0; i < splatRes; i++)
        {
			EditorUtility.DisplayProgressBar("Generating", "Slope Map",(i*1f/splatRes));
            for (int j = 0; j < splatRes; j++)
            {
				
					cliffMap[i,j] = (land.terrainData.GetSteepness(1f*j/splatRes, 1f*i/splatRes))/90 * (density / 100f);
			
			}
		}
		EditorUtility.ClearProgressBar();

		for (int i = 2; i < splatRes-2; i++)
        {
			EditorUtility.DisplayProgressBar("Dithering", "Cliff Map",(i*1f/splatRes));
            for (int j = 2; j < splatRes-2; j++)
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
		for (int i = 1; i < splatRes-1; i++)
        {
			EditorUtility.DisplayProgressBar("Spawning", "Geology features",(i*1f/splatRes));
            for (int j = 1; j < splatRes-1; j++)
            {
				slope = land.terrainData.GetSteepness(1f*j/splatRes, 1f*i/splatRes);
				
				if (geo.biomeExclusive)
				{
					cliffFade = cliffMap[i,j] * biomeMap[i,j,geo.biomeIndex];		
				}
				else
				{
					cliffFade = cliffMap[i,j];
				}
				
				heightMapsX = (int)(1f* i * resRatio);
				heightMapsY = (int)(1f* j * resRatio);
				heightMapsX = (int)Mathf.Clamp(heightMapsX, 0f, res *1f);
				heightMapsY = (int)Mathf.Clamp(heightMapsY, 0f, res *1f);
				
				if(baseMap[heightMapsX,heightMapsY] > floor && baseMap[heightMapsX,heightMapsY] < ceiling && avoidMap[i,j,0] < avoider && avoidMap1[i,j,0] < avoider && cliffFade > .5f && slope > s1 && slope < s2)
				{

									
									height = 0f;
									
									tempHeight= 0f;
									for (int n = -1; n < 2; n++)
									{
										for (int o = -1; o < 2; o++)
										{
											tempHeight = baseMap[heightMapsX+n, heightMapsY+o];
											
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
									
									
									

									
									
									rRotate = new Vector3(UnityEngine.Random.Range(rotationRange1.x, rotationRange2.x) + geology + flipX, UnityEngine.Random.Range(rotationRange1.y, rotationRange2.y), UnityEngine.Random.Range(rotationRange1.z,rotationRange2.z) + flipZ);
									rScale = new Vector3(UnityEngine.Random.Range(scaleRange1.x, scaleRange2.x), UnityEngine.Random.Range(scaleRange1.y, scaleRange2.y), UnityEngine.Random.Range(scaleRange1.z,scaleRange2.z));
									
									if(geo.normalizeY)
									{
									normal = land.terrainData.GetInterpolatedNormal(1f*j/splatRes, 1f*i/splatRes);
									qRotate = Quaternion.LookRotation(normal);
									preRotate = qRotate.eulerAngles;
									rRotate.y += preRotate.y;
									}
									
									if(geo.normalizeX)
									{
									normal = land.terrainData.GetInterpolatedNormal(1f*j/splatRes, 1f*i/splatRes);
									qRotate = Quaternion.LookRotation(normal);
									preRotate = qRotate.eulerAngles;
									rRotate.x += preRotate.x;
									}
									
									if(geo.normalizeZ)
									{
									normal = land.terrainData.GetInterpolatedNormal(1f*j/splatRes, 1f*i/splatRes);
									qRotate = Quaternion.LookRotation(normal);
									preRotate = qRotate.eulerAngles;
									rRotate.z += preRotate.z;
									}
									
									//public static void createPrefab(string category, uint id, Vector3 position, Vector3 rotation, Vector3 scale)
									if(UnityEngine.Random.Range(0,thinnitude) == 2)
									{
										featPrefabID=0;
										while (featPrefabID == 0)
										{
										roll = UnityEngine.Random.Range(0,9);
										switch(roll)
											{
											case 0:
											featPrefabID = geo.prefabID;
											break;
											
											case 1:
											featPrefabID = geo.prefabID0;
											break;
											
											case 2:
											featPrefabID = geo.prefabID1;
											break;
											
											case 3:
											featPrefabID = geo.prefabID2;
											break;
											
											case 4:
											featPrefabID = geo.prefabID3;
											break;
											
											case 5:
											featPrefabID = geo.prefabID4;
											break;
											
											case 6:
											featPrefabID = geo.prefabID5;
											break;
											
											case 7:
											featPrefabID = geo.prefabID6;
											break;
											
											case 8:
											featPrefabID = geo.prefabID7;
											break;
											
											}
										}
									
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
