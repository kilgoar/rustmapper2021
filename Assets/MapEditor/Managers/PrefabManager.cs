using UnityEngine;
using UnityEditor;
using static WorldSerialization;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Collections.Generic;
using RustMapEditor.Variables;

public static class PrefabManager
{
    public static class Callbacks
    {
        public delegate void PrefabManagerCallback(GameObject prefab);

        /// <summary>Called after prefab is loaded and setup from bundle. </summary>
        public static event PrefabManagerCallback PrefabLoaded;

        public static void OnPrefabLoaded(GameObject prefab) => PrefabLoaded?.Invoke(prefab);
    }

    public static GameObject DefaultPrefab { get; private set; }
    public static Transform PrefabParent { get; private set; }
    public static GameObject PrefabToSpawn;

    public static PrefabDataHolder[] CurrentMapPrefabs { get => PrefabParent.gameObject.GetComponentsInChildren<PrefabDataHolder>(); }

    public static Dictionary<string, Transform> PrefabCategories = new Dictionary<string, Transform>();

    public static bool IsChangingPrefabs { get; private set; }

    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorApplication.update += OnProjectLoad;
    }

    private static void OnProjectLoad()
    {
        DefaultPrefab = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        PrefabParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        if (DefaultPrefab != null && PrefabParent != null)
        {
            EditorApplication.update -= OnProjectLoad;
            if (!AssetManager.IsInitialised && SettingsManager.LoadBundleOnLaunch)
                AssetManager.Initialise(SettingsManager.RustDirectory + SettingsManager.BundlePathExt);
        }
    }

    /// <summary>Loads, sets up and returns the prefab at the asset path.</summary>
    /// <param name="path">The prefab path in the bundle file.</param>
    public static GameObject Load(string path)
    {
        if (AssetManager.IsInitialised)
            return AssetManager.LoadPrefab(path);
        return DefaultPrefab;
    }

    /// <summary>Loads, sets up and returns the prefab at the prefab id.</summary>
    /// <param name="id">The prefab manifest id.</param>
    public static GameObject Load(uint id)
    {
        return Load(AssetManager.ToPath(id));
    }


    /// <summary>Gets the parent prefab category transform from the hierachy.</summary>
    public static Transform GetParent(string category)
    {
        if (PrefabCategories.TryGetValue(category, out Transform transform))
            return transform;

        var obj = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PrefabCategory"), PrefabParent, false);
        obj.transform.localPosition = Vector3.zero;
        obj.name = category;
        PrefabCategories.Add(category, obj.transform);
        return obj.transform;
    }

    /// <summary>Sets up the prefabs loaded from the bundle file for use in the editor.</summary>
    /// <param name="go">GameObject to process, should be from one of the asset bundles.</param>
    /// <param name="filePath">Asset filepath of the gameobject, used to get and set the PrefabID.</param>
    public static GameObject Setup(GameObject go, string filePath)
    {
        go.SetLayerRecursively(8);
        go.SetTagRecursively("Untagged");
        go.SetStaticRecursively(true);
        go.RemoveNameUnderscore();

        foreach (var item in go.GetComponentsInChildren<MeshCollider>())
        {
            item.cookingOptions = MeshColliderCookingOptions.None;
            item.enabled = false;
            item.isTrigger = false;
            item.convex = false;
        }
        foreach (var item in go.GetComponentsInChildren<Animator>())
        {
            item.enabled = false;
            item.runtimeAnimatorController = null;
        }
        foreach (var item in go.GetComponentsInChildren<Light>())
            item.enabled = false;
        foreach (var item in go.GetComponentsInChildren<Canvas>())
            item.enabled = false;
        foreach (var item in go.GetComponentsInChildren<CanvasGroup>())
            item.enabled = false;
        foreach (var item in go.GetComponentsInChildren<ParticleSystem>())
        {
            var emission = item.emission;
            emission.enabled = false;
        }

        PrefabDataHolder prefabDataHolder = go.AddComponent<PrefabDataHolder>();
        prefabDataHolder.prefabData = new PrefabData() { id = AssetManager.ToID(filePath) };
        prefabDataHolder.Setup();

        go.SetActive(false);
        return go;
    }

    /// <summary>Spawns a prefab, updates the PrefabData and parents to the selected transform.</summary>
    public static void Spawn(GameObject go, PrefabData prefabData, Transform parent)
    {
        GameObject newObj = GameObject.Instantiate(go, parent);
        newObj.transform.localPosition = new Vector3(prefabData.position.x, prefabData.position.y, prefabData.position.z);
        newObj.transform.rotation = Quaternion.Euler(new Vector3(prefabData.rotation.x, prefabData.rotation.y, prefabData.rotation.z));
        newObj.transform.localScale = new Vector3(prefabData.scale.x, prefabData.scale.y, prefabData.scale.z);
        newObj.name = go.name;
        newObj.GetComponent<PrefabDataHolder>().prefabData = prefabData;
        newObj.SetActive(true);
    }

    /// <summary>Spawns a prefab and parents to the selected transform.</summary>
    public static void Spawn(GameObject go, Transform transform, string name)
    {
        GameObject newObj = GameObject.Instantiate(go, PrefabParent);
        newObj.transform.position = transform.position;
        newObj.transform.rotation = transform.rotation;
        newObj.transform.localScale = transform.localScale;
        newObj.name = name;
        newObj.SetActive(true);
    }

    /// <summary>Spawns the prefab set in PrefabToSpawn at the spawnPos</summary>
    public static void SpawnPrefab(Vector3 spawnPos)
    {
        if (PrefabToSpawn != null)
        {
            GameObject newObj = GameObject.Instantiate(PrefabToSpawn, spawnPos, Quaternion.Euler(0, 0, 0));
            newObj.name = PrefabToSpawn.name;
            newObj.SetActive(true);
            PrefabToSpawn = null;
        }
    }

    /// <summary>Spawns prefabs for map load.</summary>
    public static void SpawnPrefabs(PrefabData[] prefabs, int progressID)
    {
        if (!IsChangingPrefabs)
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.SpawnPrefabs(prefabs, progressID));
    }

    /// <summary>Deletes prefabs from scene.</summary>
    public static void DeletePrefabs(PrefabDataHolder[] prefabs, int progressID = 0)
    {
        if (!IsChangingPrefabs)
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.DeletePrefabs(prefabs, progressID));
    }
	
	public static GameObject SpawnPrefab(GameObject g, PrefabData prefabData, Transform parent = null)
    {

        GameObject newObj = GameObject.Instantiate(g);
        newObj.transform.parent = parent;
        newObj.transform.position = new Vector3(prefabData.position.x, prefabData.position.y, prefabData.position.z) + TerrainManager.GetMapOffset();
        newObj.transform.rotation = Quaternion.Euler(new Vector3(prefabData.rotation.x, prefabData.rotation.y, prefabData.rotation.z));
        newObj.transform.localScale = new Vector3(prefabData.scale.x, prefabData.scale.y, prefabData.scale.z);
        newObj.GetComponent<PrefabDataHolder>().prefabData = prefabData;
        return newObj;

    }
	
	public static void createPrefab(string category, uint id, Vector3 position, Vector3 rotation, Vector3 scale)
    {
		Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
		GameObject defaultObj = Load(id);
		PrefabData newPrefab = new PrefabData();
		defaultObj.SetActive(true);
		var prefab = new PrefabData();

		prefab.category = category;
		prefab.id = id;
		prefab.position = position;
		prefab.rotation = rotation;
		prefab.scale = scale;
		SpawnPrefab(defaultObj, prefab, prefabsParent);
    }

	public static void BatchReplace(PrefabDataHolder[] prefabs, ReplacerPreset replace)
	{
		bool flag = false;
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		int res = land.terrainData.heightmapResolution;
		float ratio = 1f* TerrainManager.TerrainSize.x / res;
		Quaternion qRotate;
		Vector3 preRotate;
		Vector3 rRotate = new Vector3(0,0,0);
		Vector3 normal = new Vector3(0,0,0);
		Vector3 position = new Vector3(0,0,0);
		Vector3 scale = new Vector3(0,0,0);
		
		float xCheck =0f;
		float yCheck =0f;
		int count = 0;
		int count1= 0;
		for (int k = 0; k < prefabs.Length; k++)
		{
			flag = false;
			
			if (prefabs[k] != null)
			{
				if(prefabs[k].prefabData.id == replace.prefabID0)
				{
					prefabs[k].prefabData.id = replace.replaceID0;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID1)
				{
					prefabs[k].prefabData.id = replace.replaceID1;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID2)
				{
					prefabs[k].prefabData.id = replace.replaceID2;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID3)
				{
					prefabs[k].prefabData.id = replace.replaceID3;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID4)
				{
					prefabs[k].prefabData.id = replace.replaceID4;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID5)
				{
					prefabs[k].prefabData.id = replace.replaceID5;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID6)
				{
					prefabs[k].prefabData.id = replace.replaceID6;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID7)
				{
					prefabs[k].prefabData.id = replace.replaceID7;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID8)
				{
					prefabs[k].prefabData.id = replace.replaceID8;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID9)
				{
					prefabs[k].prefabData.id = replace.replaceID9;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID10)
				{
					prefabs[k].prefabData.id = replace.replaceID10;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID11)
				{
					prefabs[k].prefabData.id = replace.replaceID11;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID12)
				{
					prefabs[k].prefabData.id = replace.replaceID12;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID13)
				{
					prefabs[k].prefabData.id = replace.replaceID13;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID14)
				{
					prefabs[k].prefabData.id = replace.replaceID14;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID15)
				{
					prefabs[k].prefabData.id = replace.replaceID15;
					flag = true;
				}
				else if(prefabs[k].prefabData.id == replace.prefabID16)
				{
					prefabs[k].prefabData.id = replace.replaceID16;
					flag = true;
				}
				

				
				if(flag && replace.rotateToTerrain)
				{
					position.x = prefabs[k].prefabData.position.x;
					position.y = prefabs[k].prefabData.position.y;
					position.z = prefabs[k].prefabData.position.z;
					scale.x = prefabs[k].prefabData.scale.x;
					scale.y = prefabs[k].prefabData.scale.y;
					scale.z = prefabs[k].prefabData.scale.z;
					count++;
					xCheck = ((prefabs[k].prefabData.position.z/ratio)+res/2f);
					yCheck = ((prefabs[k].prefabData.position.x/ratio)+res/2f);
					normal = land.terrainData.GetInterpolatedNormal(1f*yCheck/res, 1f*xCheck/res);
					qRotate = Quaternion.LookRotation(normal);
					preRotate = qRotate.eulerAngles;
					
					if(replace.rotateToY)
					{
						rRotate.y = preRotate.y;
					}
					
					if(replace.rotateToX)
					{
						rRotate.x = preRotate.x+90f;
					}
					
					if(replace.rotateToZ)
					{
						rRotate.z = preRotate.z;
					}
					
					if(replace.scale)
					{
						scale = Vector3.Scale(prefabs[k].prefabData.scale, replace.scaling);
					}

					createPrefab("Decor", prefabs[k].prefabData.id, position, rRotate, scale);
					
					GameObject.DestroyImmediate(prefabs[k].gameObject);
								prefabs[k] = null;

								
				}
				else if(flag && !replace.rotateToTerrain)
				{
					position.x = prefabs[k].prefabData.position.x;
					position.y = prefabs[k].prefabData.position.y;
					position.z = prefabs[k].prefabData.position.z;
					scale.x = prefabs[k].prefabData.scale.x;
					scale.y = prefabs[k].prefabData.scale.y;
					scale.z = prefabs[k].prefabData.scale.z;
					if(replace.scale)
					{
						scale = Vector3.Scale(prefabs[k].prefabData.scale, replace.scaling);
					}
					
					createPrefab("Decor", prefabs[k].prefabData.id, position, prefabs[k].prefabData.rotation, scale);
					

					GameObject.DestroyImmediate(prefabs[k].gameObject);
								prefabs[k] = null;
								count1++;

				}
			}
		}
		Debug.LogError(count1 + count + " prefabs replaced." + count + " prefabs rotated to terrain. " );
	}

	public static void breakMonument(PrefabDataHolder[] prefabs, uint ID)
	{
		int count = 0;
		Transform scanItem;
		Transform spawnItem;
		Transform childSpawnItem;
		uint replaceID = 0;
		string[] parse;
		string itemName;
		Vector3 adjuster = new Vector3(2049f,500f,2049f);

		for (int k = 0; k < prefabs.Length; k++)
		{
			if (prefabs[k] != null)
			{


					for (int i = 0; i < prefabs[k].transform.childCount; i++)
						{
							scanItem = prefabs[k].transform.GetChild(i);
							
								for (int j = 0; j < scanItem.transform.childCount; j++)
								{
									spawnItem = scanItem.transform.GetChild(j);
									itemName = spawnItem.name;
									parse = itemName.Split(' ');
									itemName = parse[0];
									replaceID = AssetManager.partialToID(itemName);
									
									if (replaceID != 0)
										createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);
									else
									{
										for (int n = 0; n < spawnItem.transform.childCount; n++)
										{
											childSpawnItem = spawnItem.transform.GetChild(n);
											itemName = childSpawnItem.name;
											parse = itemName.Split(' ');
											itemName = parse[0];
											replaceID = AssetManager.partialToID(itemName);
											
											if(replaceID !=0)
												createPrefab("Decor", replaceID, childSpawnItem.position - adjuster, childSpawnItem.eulerAngles, childSpawnItem.lossyScale);
										}
									}
								}
								
							
						}
					
					GameObject.DestroyImmediate(prefabs[k].gameObject);
					prefabs[k] = null;
					
			}
		}
	}
	
	public static void FoliageCube(Vector3 glassPosition, Vector3 glassScale)
	{
				Vector3 distance = new Vector3(0,0,0);
				Vector3 foliageScale = new Vector3(0,0,0);
				Vector3 foliageRotation = new Vector3(0,0,0);
				Vector3 foliageLocation = new Vector3(0,0,0);
				
				uint creepingcornerB = 2447885804;
				uint creepingcornerA = 738251630;
				uint creepingcornerC = 648907673;
				uint creepingplantfall = 2166677703;
				uint creepingwall600 = 1431389280;
				
				int roll;
				uint foliage=0;
				float foliageRatio = 0f;
				int foliageRoll = 0;
				int cornerRoll=0;
			
				
				foliageRatio = ((glassScale.x / glassScale.y) + (glassScale.x / glassScale.z)) / 2f;
				
								
								if(foliageRatio > .8f && foliageRatio < 1.2f)
								{
									distance.x = glassScale.x / 2f;
									distance.y = glassScale.y / 2f;
									distance.z = glassScale.z / 2f;
									
									foliageScale.x = glassScale.x /6.8f;
									foliageScale.y = glassScale.y /6.8f;
									foliageScale.z = glassScale.z /6.8f;
									
									
									foliageRoll = UnityEngine.Random.Range(2,5);
									
									for (int f = 0; f < foliageRoll; f++)
									{										
									roll = UnityEngine.Random.Range(0,5);
										cornerRoll = UnityEngine.Random.Range(0,4);
										
										switch (roll)
										{
											case 0:
											
												foliage = creepingcornerB;
												
													switch (cornerRoll)
													{
														case 0:
															foliageLocation.x = glassPosition.x + distance.x;
															foliageLocation.y = glassPosition.y - distance.y;
															foliageLocation.z = glassPosition.z - distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 270f;
															foliageRotation.z = 0f;
															break;
														case 1:
															foliageLocation.x = glassPosition.x - distance.x;
															foliageLocation.y = glassPosition.y - distance.y;
															foliageLocation.z = glassPosition.z - distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 0f;
															foliageRotation.z = 0f;
															break;
														case 2:
															foliageLocation.x = glassPosition.x + distance.x;
															foliageLocation.y = glassPosition.y - distance.y;
															foliageLocation.z = glassPosition.z + distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 180f;
															foliageRotation.z = 0f;
															break;
														case 3:
															foliageLocation.x = glassPosition.x - distance.x;
															foliageLocation.y = glassPosition.y - distance.y;
															foliageLocation.z = glassPosition.z + distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 90f;
															foliageRotation.z = 0f;
															break;
													}
												break;
												
											case 1:
											
												foliage = creepingcornerA;
												
													switch (cornerRoll)
													{
														case 0:
															foliageLocation.x = glassPosition.x + distance.x;
															foliageLocation.y = glassPosition.y - distance.y;
															foliageLocation.z = glassPosition.z - distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 90f;
															foliageRotation.z = 0f;
															break;
														case 1:
															foliageLocation.x = glassPosition.x - distance.x;
															foliageLocation.y = glassPosition.y - distance.y;
															foliageLocation.z = glassPosition.z - distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 180f;
															foliageRotation.z = 0f;
															break;
														case 2:
															foliageLocation.x = glassPosition.x + distance.x;
															foliageLocation.y = glassPosition.y - distance.y;
															foliageLocation.z = glassPosition.z + distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 0f;
															foliageRotation.z = 0f;
															break;
														case 3:
															foliageLocation.x = glassPosition.x - distance.x;
															foliageLocation.y = glassPosition.y - distance.y;
															foliageLocation.z = glassPosition.z + distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 270f;
															foliageRotation.z = 0f;
															break;
													}
												break;

											case 2:
											
												foliage = creepingcornerC;
												
													switch (cornerRoll)
													{
														case 0:
															foliageLocation.x = glassPosition.x + distance.x;
															foliageLocation.y = glassPosition.y - distance.y;
															foliageLocation.z = glassPosition.z - distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 90f;
															foliageRotation.z = 0f;
															break;
														case 1:
															foliageLocation.x = glassPosition.x - distance.x;
															foliageLocation.y = glassPosition.y - distance.y;
															foliageLocation.z = glassPosition.z - distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 180f;
															foliageRotation.z = 0f;
															break;
														case 2:
															foliageLocation.x = glassPosition.x + distance.x;
															foliageLocation.y = glassPosition.y - distance.y;
															foliageLocation.z = glassPosition.z + distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 0f;
															foliageRotation.z = 0f;
															break;
														case 3:
															foliageLocation.x = glassPosition.x - distance.x;
															foliageLocation.y = glassPosition.y - distance.y;
															foliageLocation.z = glassPosition.z + distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 270f;
															foliageRotation.z = 0f;
															break;
													}
												break;	
												
											case 3:
											
												foliage = creepingplantfall;
												
													switch (cornerRoll)
													{
														case 0:
															foliageLocation.x = glassPosition.x;
															foliageLocation.y = glassPosition.y + distance.y;
															foliageLocation.z = glassPosition.z + distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 180f;
															foliageRotation.z = 0f;
															break;
														case 1:
															foliageLocation.x = glassPosition.x;
															foliageLocation.y = glassPosition.y + distance.y;
															foliageLocation.z = glassPosition.z - distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 0f;
															foliageRotation.z = 0f;
															break;
														case 2:
															foliageLocation.x = glassPosition.x - distance.x;
															foliageLocation.y = glassPosition.y + distance.y;
															foliageLocation.z = glassPosition.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 90f;
															foliageRotation.z = 0f;
															break;
														case 3:
															foliageLocation.x = glassPosition.x + distance.x;
															foliageLocation.y = glassPosition.y + distance.y;
															foliageLocation.z = glassPosition.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 270f;
															foliageRotation.z = 0f;
															break;
													}
												break;
											
											case 4:
											
												foliage = creepingwall600;
												
													switch (cornerRoll)
													{
														case 0:
															foliageLocation.x = glassPosition.x + distance.x;
															foliageLocation.y = glassPosition.y;
															foliageLocation.z = glassPosition.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 90f;
															foliageRotation.z = 0f;
															break;
														case 1:
															foliageLocation.x = glassPosition.x - distance.x;
															foliageLocation.y = glassPosition.y;
															foliageLocation.z = glassPosition.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 270f;
															foliageRotation.z = 0f;
															break;
														case 2:
															foliageLocation.x = glassPosition.x;
															foliageLocation.y = glassPosition.y;
															foliageLocation.z = glassPosition.z + distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 0f;
															foliageRotation.z = 0f;
															break;
														case 3:
															foliageLocation.x = glassPosition.x;
															foliageLocation.y = glassPosition.y;
															foliageLocation.z = glassPosition.z - distance.z;
															foliageRotation.x = 0f;
															foliageRotation.y = 180f;
															foliageRotation.z = 0f;
															break;
													}
												break;
										}
									
									createPrefab("Decor", foliage, foliageLocation, foliageRotation, foliageScale);
									}
								}
	}
	
	public static uint GetPalette(int index)
	{
	uint id=0;
				
				uint yellow = 2337881356;
				uint white = 2269472079;
				uint red = 579459297;
				uint navy = 241986762;
				uint junkyard = 1115909638;
				uint green = 1776925867;
				uint snowblue = 2600171998;
				uint blue = 2473172851;
				uint black = 2722544497;
				
		uint palette1=0;
		uint palette2=0;
		uint palette3=0;
		uint palette4=0;
			
		int roll2 = UnityEngine.Random.Range(0,3);						
		switch (index)
								{
																		case 0:
									palette1=yellow;
										palette2=yellow;
											palette3=yellow;
												palette4=yellow;
								break;
									
																		case 1:
									palette1=white;
										palette2=white;
											palette3=white;
												palette4=white;
								break;
									
																		case 2:
									palette1=red;
										palette2=red;
											palette3=red;
												palette4=red;
								break;
									
																		case 3:
									palette1=navy;
										palette2=navy;
											palette3=navy;
												palette4=navy;
								break;
									
																		case 4:
									palette1=junkyard;
										palette2=junkyard;
											palette3=junkyard;
												palette4=junkyard;
								break;
									
																		case 5:
									palette1=green;
										palette2=green;
											palette3=green;
												palette4=green;
								break;
									
																		case 6:
									palette1=blue;
										palette2=blue;
											palette3=blue;
												palette4=blue;
								break;

									
															default:
									palette1=black;
										palette2=black;
											palette3=black;
												palette4=black;
								break;
								}
		
		int roll = UnityEngine.Random.Range(0,4);
								
								switch (roll)
								{
									case 0:
										id = palette1;
										break;
									case 1:
										id = palette2;
										break;
									case 2:
										id = palette3;
										break;
									case 3:
										id = palette4;
										break;
								}
		return id;
	}
	
	public static void VehicleScrambler(PrefabDataHolder[] prefabs)
	{
		Vector3 position, rotation, scale;
		uint prefabID=0;
		int count=0;
		for (int k = 0; k < prefabs.Length; k++)
		{
			if (prefabs[k] != null)
			{
				prefabID = prefabs[k].prefabData.id;
				if (prefabID == 79597103 || prefabID == 3004602158|| prefabID == 3264578399|| prefabID == 2420618133|| prefabID == 246584577|| prefabID == 1837053258
				|| prefabID == 2832497307|| prefabID == 2300745015|| prefabID == 2082081653)
				{
					position = prefabs[k].prefabData.position;
					rotation = prefabs[k].prefabData.rotation;
					scale = prefabs[k].prefabData.scale;
					prefabID = scrambleVehicles(prefabID);
					createPrefab("Decor",prefabID,position,rotation,scale);	
						
						GameObject.DestroyImmediate(prefabs[k].gameObject);
						prefabs[k] = null;
						count ++;
				}
			}
		}
		Debug.LogError(count + "vehicles scrambled");
	}
	
	public static uint scrambleVehicles(uint prefabID)
	{
		//3273130215 - snow compact
		//3527606348 - snow van
		//3031482036 - snow pickup
		uint[] vehicles = new uint[]{/*compact cars*/79597103,2515395145,1460121016,4193915652,786199644,3435737336,
		2832497307,1896963608,3379016644,2722459803,2665725503,1837053258,677248866,1195466190,
		4214970653,302351425,1247022702,2300745015,4144215382,3367157997,3273130215,
		/*sedans*/3463123806,788274617,2784745032,836014881,3774700171,579044360,1677134398,1677134398,2420618133,
		246584577,8081016,
		/*vans*/2578743929,1236856645,3176682456,2587701539,3420081938,1680874363,946452295,1257336463,3234787709,
		2009259066,3017742883,3489554649,2052946794,1989568183,201458809,696480706,3030494899,283413573,2824451945,
		2076044469,647644014,3497012406,469377923,284962212,536546043,3293125979,3199560384,4048358114,1703786065,
		3246835063,1877110501,1875099655,3674679932,3095093162,3137641591,1536042368,802735396,
		/*pickuptrucks*/1304550409,1427838182,2636007931,997608629,3417439261,2353996331,2742523209,1660134405,
		3539282559,2403566413,3990694053,3957097701,1357952166,1550102226,3899602347,3116567664,2804921169,700152619,
		2661281391,1831500501,3264578399,3004602158,2082081653,1300485700,1779061701,165520484,1529531764,933232738,1047391926};
		
		int vehicleMax = vehicles.GetLength(0);
		int index = UnityEngine.Random.Range(0, vehicleMax);
		return vehicles[index];
		
	}
	
	public static uint ScrambleContainer(uint containerID, int palette)
	{
	uint id = containerID;
	
				uint yellow = 2337881356;
				uint white = 2269472079;
				uint red = 579459297;
				uint navy = 241986762;
				uint junkyard = 1115909638;
				uint green = 1776925867;
				uint snowblue = 2600171998;
				uint blue = 2473172851;
				uint black = 2722544497;
				
				
					
					if(containerID == blue || containerID == red ||
							containerID == yellow || containerID == black ||
							containerID == white || containerID == snowblue || 
							containerID == green || containerID == navy ||
							containerID == junkyard)
							{
								id = GetPalette(palette);
							}
		return id;
	}

	public static void deletePrefabsOffArid(PrefabDataHolder[] prefabs)
	{
		int count = 0;
		int xCheck =0;
		int yCheck =0;
		float[,,] biomeMap = TerrainManager.BiomeArray;
		int res = biomeMap.GetLength(0);
		float ratio = 1f* TerrainManager.TerrainSize.x / res;
		
		for (int k = 0; k < prefabs.Length; k++)
		{
			if (prefabs[k] != null)
			{
								xCheck = (int)((prefabs[k].prefabData.position.z/ratio)+res/2f);
								yCheck = (int)((prefabs[k].prefabData.position.x/ratio)+res/2f);
								if (xCheck > 0 && yCheck > 0 && xCheck < res && yCheck < res)
								{
									if (biomeMap[xCheck,yCheck,0] == 0f)
									{
										GameObject.DestroyImmediate(prefabs[k].gameObject);
										prefabs[k] = null;
										count ++;
									}
								}
			}
		}
		Debug.LogError(count + " prefabs removed");
	}

	public static void deletePrefabIDs(PrefabDataHolder[] prefabs, uint ID)
	{
		int count = 0;

		for (int k = 0; k < prefabs.Length; k++)
		{
			if (prefabs[k] != null)
			{
				if ( prefabs[k].prefabData.id == ID )
				{			
								GameObject.DestroyImmediate(prefabs[k].gameObject);
								prefabs[k] = null;
								count ++;
				}
			}
		}
		Debug.LogError(count + " prefabs removed");
	}

	

	public static void deleteAllPrefabs(PrefabDataHolder[] prefabs)
	{
		int count = 0;

		for (int k = 0; k < prefabs.Length; k++)
		{

				
							GameObject.DestroyImmediate(prefabs[k].gameObject);
							prefabs[k] = null;
							count ++;
		}
		Debug.LogError(count + " prefabs removed");
	}

    /// <summary>Replaces the selected prefabs with ones from the Rust bundles.</summary>
    public static void ReplaceWithLoaded(PrefabDataHolder[] prefabs, int progressID)
    {
        if (AssetManager.IsInitialised && !IsChangingPrefabs)
        {
            IsChangingPrefabs = true;
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.ReplaceWithLoaded(prefabs, progressID));
        }
    }

    /// <summary>Replaces the selected prefabs with the default prefabs.</summary>
    public static void ReplaceWithDefault(PrefabDataHolder[] prefabs, int progressID)
    {
        if (!IsChangingPrefabs)
        {
            IsChangingPrefabs = true;
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.ReplaceWithDefault(prefabs, progressID));
        }
    }

    public static void RenamePrefabCategories(PrefabDataHolder[] prefabs, string name)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.RenamePrefabCategories(prefabs, name));
    }

    public static void RenamePrefabIDs(PrefabDataHolder[] prefabs, uint id, bool replace)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.RenamePrefabIDs(prefabs, id, replace));
    }

    private static class Coroutines
    {
        public static IEnumerator SpawnPrefabs(PrefabData[] prefabs, int progressID)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 4f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / prefabs.Length, "Spawning Prefabs: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
                Spawn(Load(prefabs[i].id), prefabs[i], GetParent(prefabs[i].category));
            }
            Progress.Report(progressID, 0.99f, "Spawned " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);
        }

        public static IEnumerator DeletePrefabs(PrefabDataHolder[] prefabs, int progressID = 0)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            if (progressID == 0)
                progressID = Progress.Start("Delete Prefabs", null, Progress.Options.Sticky);

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 0.25f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / prefabs.Length, "Deleting Prefabs: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }
            Progress.Report(progressID, 0.99f, "Deleted " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);
        }

        public static IEnumerator ReplaceWithLoaded(PrefabDataHolder[] prefabs, int progressID)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 4f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / prefabs.Length, "Replacing Prefabs: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
                prefabs[i].UpdatePrefabData();
                Spawn(Load(prefabs[i].prefabData.id), prefabs[i].prefabData, GetParent(prefabs[i].prefabData.category));
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }
            Progress.Report(progressID, 0.99f, "Replaced " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);
            IsChangingPrefabs = false;
        }

        public static IEnumerator ReplaceWithDefault(PrefabDataHolder[] prefabs, int progressID)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 0.05f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / prefabs.Length, "Replacing Prefabs: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
                prefabs[i].UpdatePrefabData();
                Spawn(DefaultPrefab, prefabs[i].prefabData, GetParent(prefabs[i].prefabData.category));
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }
            Progress.Report(progressID, 0.99f, "Replaced " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);

            IsChangingPrefabs = false;
        }

        public static IEnumerator RenamePrefabCategories(PrefabDataHolder[] prefabs, string name)
        {
            ProgressManager.RemoveProgressBars("Rename Prefab Categories");
            int progressId = Progress.Start("Rename Prefab Categories", null, Progress.Options.Sticky);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                prefabs[i].prefabData.category = name;
                if (sw.Elapsed.TotalSeconds > 0.2f)
                {
                    yield return null;
                    Progress.Report(progressId, (float)i / prefabs.Length, "Renaming Prefab: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
            }

            Progress.Report(progressId, 0.99f, "Renamed: " + prefabs.Length + " prefabs.");
            Progress.Finish(progressId);
        }

        public static IEnumerator RenamePrefabIDs(PrefabDataHolder[] prefabs, uint id, bool replace)
        {
            ProgressManager.RemoveProgressBars("Rename Prefab IDs");
            int progressId = Progress.Start("Rename Prefab IDs", null, Progress.Options.Sticky);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                prefabs[i].prefabData.id = id;
                if (replace)
                {
                    prefabs[i].UpdatePrefabData();
                    Spawn(Load(prefabs[i].prefabData.id), prefabs[i].prefabData, GetParent(prefabs[i].prefabData.category));
                    GameObject.DestroyImmediate(prefabs[i].gameObject);
                }
                if (sw.Elapsed.TotalSeconds > 0.2f)
                {
                    yield return null;
                    Progress.Report(progressId, (float)i / prefabs.Length, "Renaming Prefab: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
            }

            Progress.Report(progressId, 0.99f, "Renamed: " + prefabs.Length + " prefabs.");
            Progress.Finish(progressId);
        }
    }
}