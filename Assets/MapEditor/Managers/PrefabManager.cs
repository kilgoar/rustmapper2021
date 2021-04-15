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
					count++;
					xCheck = ((prefabs[k].prefabData.position.z/ratio)+res/2f);
					yCheck = ((prefabs[k].prefabData.position.x/ratio)+res/2f);
					normal = land.terrainData.GetInterpolatedNormal(1f*yCheck/res, 1f*xCheck/res);
					qRotate = Quaternion.LookRotation(normal);
					preRotate = qRotate.eulerAngles;
					rRotate.y = preRotate.y;
					
					createPrefab("Decor", prefabs[k].prefabData.id, prefabs[k].prefabData.position, rRotate, prefabs[k].prefabData.scale);
					
					GameObject.DestroyImmediate(prefabs[k].gameObject);
								prefabs[k] = null;
								
				}
				else if(flag && !replace.rotateToTerrain)
				{
					createPrefab("Decor", prefabs[k].prefabData.id, prefabs[k].prefabData.position, prefabs[k].prefabData.rotation, prefabs[k].prefabData.scale);
					
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