using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace RustMapEditor.Variables
{

	
	public struct Point
	{
		public Point(int x, int y)
		{
			X=x;
			Y=y;
		}
		public int X;
		public int Y;
	}
	
    public struct Conditions
    {
        public GroundConditions GroundConditions;
        public BiomeConditions BiomeConditions;
        public AlphaConditions AlphaConditions;
        public TopologyConditions TopologyConditions;
        public TerrainConditions TerrainConditions;
        public AreaConditions AreaConditions;
    }
    public struct GroundConditions
    {
        public GroundConditions(TerrainSplat.Enum layer)
        {
            Layer = layer;
            Weight = new float[TerrainSplat.COUNT];
            CheckLayer = new bool[TerrainSplat.COUNT];
        }
        public TerrainSplat.Enum Layer;
        public float[] Weight;
        public bool[] CheckLayer;
    }
    public struct BiomeConditions
    {
        public BiomeConditions(TerrainBiome.Enum layer)
        {
            Layer = layer;
            Weight = new float[TerrainBiome.COUNT];
            CheckLayer = new bool[TerrainBiome.COUNT];
        }
        public TerrainBiome.Enum Layer;
        public float[] Weight;
        public bool[] CheckLayer;
    }
    public struct AlphaConditions
    {
        public AlphaConditions(AlphaTextures texture)
        {
            Texture = texture;
            CheckAlpha = false;
        }
        public AlphaTextures Texture;
        public bool CheckAlpha;
    }
    public struct TopologyConditions
    {
        public TopologyConditions(TerrainTopology.Enum layer)
        {
            Layer = layer;
            Texture = new TopologyTextures[TerrainTopology.COUNT];
            CheckLayer = new bool[TerrainTopology.COUNT];
        }
        public TerrainTopology.Enum Layer;
        public TopologyTextures[] Texture;
        public bool[] CheckLayer;
    }
    public struct TerrainConditions
    {
        public HeightsInfo Heights;
        public bool CheckHeights;
        public SlopesInfo Slopes;
        public bool CheckSlopes;
    }
    public struct AreaConditions
    {
        public Dimensions Area;
        public bool CheckArea;
    }
    public struct TopologyLayers
    {
        public float[,,] Topologies
        {
            get; set;
        }
    }
    public class Dimensions
    {
        public int x0;
        public int x1;
        public int z0;
        public int z1;

        public Dimensions(int x0, int x1, int z0, int z1)
        {
            this.x0 = x0;
            this.x1 = x1;
            this.z0 = z0;
            this.z1 = z1;
        }

        public static Dimensions HeightMapDimensions()
        {
            return new Dimensions(0, TerrainManager.HeightMapRes, 0, TerrainManager.HeightMapRes);
        }
    }
	
    public enum LandLayers
    {
        Ground = 0,
        Biome = 1,
        Alpha = 2,
        Topology = 3,
    }
    public enum AlphaTextures
    {
        Visible = 0,
        InVisible = 1,
    }
    public enum TopologyTextures
    {
        Active = 0,
        InActive = 1,
    }
    public struct SlopesInfo
    {
        public bool BlendSlopes;
        public float SlopeBlendLow;
        public float SlopeLow;
        public float SlopeHigh;
        public float SlopeBlendHigh;
    }
    public struct HeightsInfo
    {
        public bool BlendHeights;
        public float HeightBlendLow;
        public float HeightLow;
        public float HeightHigh;
        public float HeightBlendHigh;
    }
	
	public enum ColliderLayer
	{
		Prefabs = 1<<8,
		Paths = 1<<9,
		Land = 1<<10,
	}
	
    public class Selections
    {
        public enum Objects
        {
            Ground = 1 << 0,
            Biome = 1 << 1,
            Alpha = 1 << 2,
            Topology = 1 << 3,
            Heightmap = 1 << 4,
            Watermap = 1 << 5,
            Prefabs = 1 << 6,
            Paths = 1 << 7,
        }
        public enum Terrains
        {
            Land = 1 << 0,
            Water = 1 << 1,
        }
        public enum Layers
        {
            Ground = 1 << 0,
            Biome = 1 << 1,
            Alpha = 1 << 2,
            Topology = 1 << 3,
        }
    }
	
	public struct monumentData
	{
		public monumentData(int X, int Y, int Width, int Height)
		{
			x=X;
			y=Y;
			width=Width;
			height=Height;
		}
		public int x,y,width,height;
	}
		
	[Serializable]
	public class GeologyPresetCollection
	{
		public GeologyPreset[] geoPresets;
	}
	
	[Serializable]
	public struct TerracingPreset
	{
		public bool flatten, perlinBanks, circular;
		public float weight;
		public int zStart, gateBottom, gateTop, gates, descaleFactor, perlinDensity;
	}
	
	
	[Serializable]
	public struct PerlinPreset
	{
		public int layers, period, scale;
		public bool simple;
	}
	
	[Serializable]
	public struct PerlinSplatPreset
	{
		public int scale, splatLayer;
		public TerrainBiome.Enum biomeLayer;
		public float strength;
		public bool invert, paintBiome;
	}
	
	[Serializable]
	public struct RipplePreset
	{
		public int size, density;
		public float weight;
	}
	
	[Serializable]
	public struct CrazingPreset
	{
		public string title;
		public int zones, minSize, maxSize, splatLayer;
		
	}
	
	[Serializable]
	public struct BreakerPreset
	{
		public string title;
		public MonumentData monument;
	}
	
	[Serializable]
	public struct OceanPreset
	{
		public string title;
		public int radius, gradient, xOffset, yOffset, s, seafloor;
		public bool perlin;
	}
	//int radius, int gradient, float seafloor, int xOffset, int yOffset, bool perlin, int s
	
	[Serializable]
	public struct Colliders
	{
		public Vector3 box, sphere, capsule;
	}
	
	[Serializable]
	public struct BreakingData
	{
		public string name;
		public uint id;
		public bool ignore;
		public int treeID;
		public Colliders colliderScales;
		public WorldSerialization.PrefabData prefabData;
		public string parent;
		
	}
	
	[Serializable]
	public class MonumentData
	{
		public List<CategoryData> category = new List<CategoryData>();
		public string monumentName;
	}
	
	[Serializable]
	public class GreatGreatGrandchildrenData
	{
		public BreakingData breakingData = new BreakingData();
		
		public GreatGreatGrandchildrenData(BreakingData breakingData)
		{
			this.breakingData = breakingData;
		}
	}
	
	[Serializable]
	public class GreatGrandchildrenData
	{
		public BreakingData breakingData = new BreakingData();
		public List<GreatGreatGrandchildrenData> greatgreatgrandchild = new List<GreatGreatGrandchildrenData>();
		
		public GreatGrandchildrenData(BreakingData breakingData)
		{
			this.breakingData = breakingData;
		}
	}
	
	[Serializable]
	public class GrandchildrenData
	{
		public BreakingData breakingData = new BreakingData();
		public List<GreatGrandchildrenData> greatgrandchild = new List<GreatGrandchildrenData>();
		
		public GrandchildrenData(BreakingData breakingData)
		{
			this.breakingData = breakingData;
		}
	}
	
	[Serializable]
	public class ChildrenData
	{
		public BreakingData breakingData = new BreakingData();
		public List<GrandchildrenData> grandchild = new List<GrandchildrenData>();
		
		public ChildrenData(BreakingData breakingData)
		{
			this.breakingData = breakingData;
		}
	}
	
	[Serializable]
	public class CategoryData
	{
		public BreakingData breakingData = new BreakingData();
		public List<ChildrenData> child = new List<ChildrenData>();
		
		public CategoryData(BreakingData breakingData)
		{
			this.breakingData = breakingData;
		}
	}

	public class IconTextures
	{
		public Texture2D gears;
		public Texture2D scrap;
		public Texture2D stop;
		public Texture2D tarp;
		public Texture2D trash;
		public IconTextures(Texture2D gears, Texture2D scrap, Texture2D stop, Texture2D tarp, Texture2D trash)
		{
			this.gears = gears; this.scrap = scrap; this.stop = stop; this.tarp = tarp; this.trash = trash;
		}
	}
	
		
	public class BreakingItem : TreeViewItem 
	{
		public BreakingData breakingData;
		
		public BreakingItem(TreeViewItem treeItem, BreakingData breakingData)
		{
			this.displayName = breakingData.name;
			
			this.breakingData = breakingData;
		}
	}
	
	public class BreakerTreeView : TreeView
	{
		public MonumentData monumentFragments;
		public IconTextures icons;
		public List<BreakingData> fragment = new List<BreakingData>();
		
		public IList<int> ChildList(int ID)
		{
			IList<int> IDlist = new List<int>();
			TreeViewItem parent =  this.FindItem(ID, rootItem);
			
			if (parent.hasChildren)
			{
				
				List<TreeViewItem> childList =  parent.children;
				foreach (TreeViewItem item in this.FindItem(ID, rootItem).children)
				{
					IDlist.Add(item.id);
				}
			}
			else
			{
				IDlist.Add(parent.id);
			}
			return IDlist;
		}
		
		public void ClearSelection()
		{
			IList<int> IDlist = new List<int>();
			this.SetSelection(IDlist);
		}
		
		public void ConcatSelection(IList<int> newSelection)
		{
			this.SetSelection(this.GetSelection().Concat(newSelection).ToList());
		}
		
		public void LoadFragments(MonumentData fragments)
		{
			monumentFragments = fragments;
			Reload();
		}
		
		public void LoadIcons(IconTextures iconLoader)
		{
			icons = iconLoader;
		}
		
		public void Update()
		{	
			if (monumentFragments != null)
			{
					for (int i = 0; i < monumentFragments.category.Count; i++) 
					{
						
						monumentFragments.category[i].breakingData = fragment[monumentFragments.category[i].breakingData.treeID];
						
						for (int j = 0; j <monumentFragments.category[i].child.Count; j++)
						{
							monumentFragments.category[i].child[j].breakingData = fragment[monumentFragments.category[i].child[j].breakingData.treeID];
								
							for (int k = 0; k <monumentFragments.category[i].child[j].grandchild.Count; k++)
							{
								monumentFragments.category[i].child[j].grandchild[k].breakingData = fragment[monumentFragments.category[i].child[j].grandchild[k].breakingData.treeID];
								
								for (int m = 0; m <monumentFragments.category[i].child[j].grandchild[k].greatgrandchild.Count; m++)
								{
									monumentFragments.category[i].child[j].grandchild[k].greatgrandchild[m].breakingData = fragment[monumentFragments.category[i].child[j].grandchild[k].greatgrandchild[m].breakingData.treeID];
								}
							}
						}
					}
			}
			Reload();
		}
		
		public BreakerTreeView(TreeViewState treeViewState)
			: base(treeViewState)
		{
			Reload();
		}
		
		
		
		protected override TreeViewItem BuildRoot ()
		{
			// BuildRoot is called every time Reload is called to ensure that TreeViewItems 
			// are created from data. Here we create a fixed set of items. In a real world example,
			// a data model should be passed into the TreeView and the items created from the model.

			// This section illustrates that IDs should be unique. The root item is required to 
			// have a depth of -1, and the rest of the items increment from that.
			var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
			int idCount = 0;
			fragment = new List<BreakingData>();
			
			BreakingItem childTree, grandchildTree, greatgrandchildTree, greatgreatgrandchildTree;
			if (monumentFragments != null)
			{
					for (int i = 0; i < monumentFragments.category.Count; i++) 
					{
						monumentFragments.category[i].breakingData.treeID = idCount;						
						childTree = new BreakingItem (new TreeViewItem {id = idCount, displayName = monumentFragments.category[i].breakingData.name}, monumentFragments.category[i].breakingData);
						fragment.Add(monumentFragments.category[i].breakingData);
						childTree.id = idCount;
						childTree.icon = PrefabManager.GetIcon(monumentFragments.category[i].breakingData, icons);
						childTree.displayName = monumentFragments.category[i].breakingData.name;
						root.AddChild (childTree);
						idCount++;
						
						for (int j = 0; j <monumentFragments.category[i].child.Count; j++)
						{
							monumentFragments.category[i].child[j].breakingData.treeID = idCount;
							grandchildTree = new BreakingItem (new TreeViewItem {id = idCount, displayName = monumentFragments.category[i].child[j].breakingData.name}, monumentFragments.category[i].child[j].breakingData);
							fragment.Add(monumentFragments.category[i].child[j].breakingData);
							grandchildTree.id = idCount;
							grandchildTree.icon = PrefabManager.GetIcon(monumentFragments.category[i].child[j].breakingData, icons);
							childTree.AddChild(grandchildTree);
							idCount++;
							
							for (int k = 0; k <monumentFragments.category[i].child[j].grandchild.Count; k++)
							{
								monumentFragments.category[i].child[j].grandchild[k].breakingData.treeID = idCount;
								greatgrandchildTree = new BreakingItem(new TreeViewItem {id = idCount, displayName = monumentFragments.category[i].child[j].grandchild[k].breakingData.name}, monumentFragments.category[i].child[j].grandchild[k].breakingData);
								fragment.Add(monumentFragments.category[i].child[j].grandchild[k].breakingData);
								greatgrandchildTree.id = idCount;
								greatgrandchildTree.icon = PrefabManager.GetIcon(monumentFragments.category[i].child[j].grandchild[k].breakingData, icons);
								grandchildTree.AddChild(greatgrandchildTree);
								idCount++;
								
								for (int m = 0; m <monumentFragments.category[i].child[j].grandchild[k].greatgrandchild.Count; m++)
								{
									monumentFragments.category[i].child[j].grandchild[k].greatgrandchild[m].breakingData.treeID = idCount;
									greatgreatgrandchildTree = new BreakingItem(new TreeViewItem {id = idCount, displayName = monumentFragments.category[i].child[j].grandchild[k].greatgrandchild[m].breakingData.name}, monumentFragments.category[i].child[j].grandchild[k].greatgrandchild[m].breakingData);
									fragment.Add(monumentFragments.category[i].child[j].grandchild[k].greatgrandchild[m].breakingData);
									greatgreatgrandchildTree.id = idCount;
									greatgreatgrandchildTree.icon = PrefabManager.GetIcon(monumentFragments.category[i].child[j].grandchild[k].greatgrandchild[m].breakingData, icons);
									greatgrandchildTree.AddChild(greatgreatgrandchildTree);
									idCount++;
								}
							}
							
						}
						
					}
			}
			else
			{
				root.AddChild(new TreeViewItem   { id = 1, displayName = " " });
			}
			
			SetupDepthsFromParentsAndChildren(root);

			return root;
		}
	}
	
	[Serializable]
	public struct FragmentPair
	{
		public string fragment;
		public uint id;
		
		public FragmentPair(string fragment,uint id)
		{
			this.fragment = fragment;
			this.id = id;
		}
	}
	
	[Serializable]
	public class FragmentLookup
	{
		public List<FragmentPair> fragmentPairs = new List<FragmentPair>();
		public Dictionary<string,uint> fragmentNamelist = new Dictionary<string,uint>();
		
		public void LoadPairList(List<FragmentPair> fragmentPairs)
		{
			this.fragmentPairs = fragmentPairs;
		}
		
		public void Deserialize()
		{
			this.fragmentNamelist = SettingsManager.ListToDict(this.fragmentPairs);
		}
		
		public void Serialize()
		{
			this.fragmentPairs = SettingsManager.DictToList(this.fragmentNamelist);
		}
		
	}
	
	[Serializable]
	public struct ReplacerPreset
	{

				public uint prefabID0;
				public uint prefabID1;
				public uint prefabID2;
				public uint prefabID3;
				public uint prefabID4;
				public uint prefabID5;
				public uint prefabID6;
				public uint prefabID7;				
				public uint prefabID8;
				public uint prefabID9;
				public uint prefabID10;
				public uint prefabID11;
				public uint prefabID12;
				public uint prefabID13;
				public uint prefabID14;
				public uint prefabID15;
				public uint prefabID16;
				
				public uint replaceID0;
				public uint replaceID1;
				public uint replaceID2;
				public uint replaceID3;
				public uint replaceID4;
				public uint replaceID5;
				public uint replaceID6;
				public uint replaceID7;				
				public uint replaceID8;
				public uint replaceID9;
				public uint replaceID10;
				public uint replaceID11;
				public uint replaceID12;
				public uint replaceID13;
				public uint replaceID14;
				public uint replaceID15;
				public uint replaceID16;
				
				public bool rotateToTerrain, rotateToX, rotateToY, rotateToZ;
				public string title;
				public bool scale;
				public Vector3 scaling;
	}

	[Serializable]
	public class GeologyItem
	{
		public string customPrefab;
		public bool custom;
		public uint prefabID;
		public int emphasis;
		
		public GeologyItem(uint prefabID)
		{
			this.prefabID = prefabID;
		}
		public GeologyItem(GeologyItem geoItem)
		{
			this.prefabID = geoItem.prefabID;
			this.custom =  geoItem.custom;
			this.emphasis = geoItem.emphasis;
			this.customPrefab = geoItem.customPrefab;
		}
		public GeologyItem()
		{
		}
	}
		
	[Serializable]
	public struct GeologyPreset
	{
				public List<GeologyItem> geologyItems;
				public string filename;
				public string title;
				public uint prefabID;
				public uint prefabID0;
				public uint prefabID1;
				public uint prefabID2;
				public uint prefabID3;
				public uint prefabID4;
				public uint prefabID5;
				public uint prefabID6;
				public uint prefabID7;
				public int density, frequency, floor, ceiling, biomeIndex;
				public TerrainBiome.Enum biomeLayer;
				public ColliderLayer colliderLayer, closeColliderLayer;				
				public bool avoidTopo, flipping, tilting, normalizeX, normalizeY, normalizeZ, biomeExclusive, cliffTest, overlap, closeOverlap; 
				public Vector3 scalesLow, scalesHigh, rotationsLow, rotationsHigh;
				public float zOffset, slopeLow, slopeHigh, colliderDistance, closeColliderDistance;
				
				public GeologyPreset(string title) : this()
				{
					this.title = title;
				}
	}
	
	[Serializable]
	public struct RustCityPreset
	{
		public string title;
		public int size, alley, street, start;
		public float flatness;
		public float zOff;
		public int x, y;
	}
	
	
    public class PrefabExport
    {
        public int PrefabNumber
        {
            get; set;
        }
        public uint PrefabID
        {
            get; set;
        }
        public string PrefabPath
        {
            get; set;
        }
        public string PrefabPosition
        {
            get; set;
        }
        public string PrefabScale
        {
            get; set;
        }
        public string PrefabRotation
        {
            get; set;
        }
    }
    public class Layers
    {
        public TerrainSplat.Enum Ground
        {
            get; set;
        }
        public TerrainBiome.Enum Biome
        {
            get; set;
        }
        public TerrainTopology.Enum Topologies
        {
            get; set;
        }
        public LandLayers LandLayer
        {
            get; set;
        }
        public AlphaTextures AlphaTexture
        {
            get; set;
        }
        public TopologyTextures TopologyTexture
        {
            get; set;
        }
    }
}