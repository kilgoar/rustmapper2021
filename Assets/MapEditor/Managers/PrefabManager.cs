using UnityEngine;
using UnityEditor;
using System.Linq;
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
	
	public static string monumentName = "";
    public static GameObject DefaultPrefab { get; private set; }
    public static Transform PrefabParent { get; private set; }
	public static Transform CustomPrefabParent { get; private set; }
	public static Transform ElectricsParent { get; private set; }
    public static GameObject PrefabToSpawn;

    public static PrefabDataHolder[] CurrentMapPrefabs { get => PrefabParent.gameObject.GetComponentsInChildren<PrefabDataHolder>(); }
	public static CircuitDataHolder[] CurrentMapElectrics { get => ElectricsParent.gameObject.GetComponentsInChildren<CircuitDataHolder>(); }
	
		
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
		
		ElectricsParent = GameObject.FindGameObjectWithTag("Electrics").transform;
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
        //if (category == "Custom Prefabs")
		//{
		//	obj.transform.localPosition = PrefabParent.localPosition;
		//}
		//else
		//{
			obj.transform.localPosition = Vector3.zero;
		//}
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
        //prefabDataHolder.Setup();

        go.SetActive(false);
        return go;
    }

	//spawn default prefabs for circuitry
	
	public static void Spawn(GameObject go, CircuitData circuitData, Transform parent)
	{
		
		GameObject newObj = GameObject.Instantiate(go, parent);
        newObj.transform.localPosition = new Vector3(circuitData.wiring.x, circuitData.wiring.y, circuitData.wiring.z);
        newObj.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        string[] path = circuitData.path.Split('/');
		newObj.name = path[path.Length-1];
		
		CircuitDataHolder electrics = newObj.AddComponent<CircuitDataHolder>();
		electrics.circuitData = circuitData;
        newObj.SetActive(true);
		
	}
	
	
    /// <summary>Spawns a prefab, updates the PrefabData and parents to the selected transform.</summary>
    
	public static void Spawn(GameObject go, PrefabData prefabData, Transform parent)
    {
        GameObject newObj = GameObject.Instantiate(go, parent);
        newObj.transform.localPosition = new Vector3(prefabData.position.x, prefabData.position.y, prefabData.position.z);
        newObj.transform.localRotation = Quaternion.Euler(new Vector3(prefabData.rotation.x, prefabData.rotation.y, prefabData.rotation.z));
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
	public static void SpawnPrefabs(PrefabData[] prefabs, int progressID, Transform parent)
    {
        if (!IsChangingPrefabs)
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.SpawnPrefabs(prefabs, progressID, parent));
    }
    public static void SpawnCircuits(CircuitData[] circuits, int progressID)
    {
        if (!IsChangingPrefabs)
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.SpawnCircuits(circuits, progressID));
    }
    /// <summary>Deletes prefabs from scene.</summary>
    public static void DeletePrefabs(PrefabDataHolder[] prefabs, int progressID = 0)
    {
        if (!IsChangingPrefabs)
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.DeletePrefabs(prefabs, progressID));
    }
	
	public static void DeleteCircuits(CircuitDataHolder[] circuits, int progressID = 0)
    {
        if (!IsChangingPrefabs)
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.DeleteCircuits(circuits, progressID));
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

	public static void placeCustomPrefab(string loadPath, Transform spawnItem)
	{
			var world = new WorldSerialization();
            world.LoadREPrefab(loadPath);
			MapManager.MergeOffsetREPrefab(WorldConverter.WorldToREPrefab(world), spawnItem, loadPath);
	}
	
	public static void blacklistedCreatePrefab(string category, uint id, Vector3 adjuster, Transform spawnItem)
    {
		/*
		if(id !=0 && id != 1677700966 && id != 2408482215 && id != 1071933290 && id != 4114395726 && id != 4179136873 && id != 3782603339 && id != 3072760270 && id != 3468856354 && id != 4022753621
		&& id != 3415994284 && id != 447739874 && id != 2316560245 && id != 3052882219 && id != 2962527842 && id != 1872469913 && id != 2364515962 && id != 1369298112 && id != 3450258766 && id != 2483607632)
		{
			if (id == 3286143640)
					id = 3713697082;
		*/	
		
		if (id == 859761354) //system
			return;
		
		if (id == 817541646)
		{
			spawnItem.position -= adjuster;		
			placeCustomPrefab("fenceReplacer.prefab", spawnItem);
			return;
		}
		
			string[] parse;
			parse = spawnItem.name.Split(' ');
			string itemName = parse[0];
			
			if (spawnItem.name.Contains("ball") || spawnItem.name.Contains("mound"))
			{
				//do not remove  snowballs or snowmounds of ice lakes
			}
			else
			{
					if (spawnItem.name.Contains("temperate") || spawnItem.name.Contains("arid") || spawnItem.name.Contains("tundra") || spawnItem.name.Contains("snow"))
					{
						return;
					}
			}
			if (spawnItem.name == "sentry.bandit.static")
			{
				id = 1386184467;
			}
			if (spawnItem.name == "sliding_blast_door_WheelSwitch")
			{
				id = 3767520300;
			}
			if (spawnItem.name == "sliding_blast_door_PressButton")
			{
				id = 4224395968;
			}
			
			if (itemName == "ladder_trigger_sewers" || itemName == "ladder_volume" || itemName == "ladder_volume_dock" || itemName == "ladder_volume_tower_c" || itemName == "ladder_volume_tower_b" 
			|| itemName == "ladder_trigger_chimney" || itemName == "ladder" || itemName == "ladder (1)" || itemName == "ladder_volume_core (2)" ||
			itemName == "ladder_volume (1)" ||
			itemName == "ladder_volume (2)"||
			itemName == "ladder_volume (3)"||
			itemName == "ladder_volume (4)")
			{
				Debug.LogError(spawnItem.name);
				id = 3381066643;
				
			}
			
			if (spawnItem.name == "collider_helper" || spawnItem.name == "collider_helper (1)" || spawnItem.name == "collider_helper (2)" || spawnItem.name == "stairs_colider"
			|| spawnItem.name == "windowhack")
			{
				id = 3000049339;
			}

			
		Vector3 rotation = spawnItem.eulerAngles;
		Vector3 position = spawnItem.position - adjuster;
		Vector3 scale=spawnItem.lossyScale;
			//ladder fixes
			if(id == 3381066643)
			{
				BoxCollider collider = spawnItem.GetComponent(typeof(BoxCollider)) as BoxCollider;
				scale = collider.size;
			if (monumentName == "airfield 1" || monumentName =="trainyard 1" || monumentName == "water treatment plant 1")
				{
					position.y += scale.y /2f;
				}
				else if(monumentName == "military tunnel 1")
				{
					position.y += scale.y *.4f;
				}
			}
		if (id == 3224970585 || id == 316558065 || id == 4190049974  || id == 3521578167 || id == 3381066643 || id == 3000049339 || id == 602278094 || id == 858853278)
		{
			if ((monumentName == "trainyard 1" || monumentName == "sphere tank" || monumentName == "Oilrig 1" ||
			monumentName == "Oilrig 2") && id != 4190049974 && id !=3521578167)
			{
				//scale is correct
			}
			else
			{
				
				if (spawnItem.TryGetComponent(typeof(SphereCollider), out Component comp))
				{
					SphereCollider collider = spawnItem.GetComponent(typeof(SphereCollider)) as SphereCollider;
					scale.x = collider.radius*2f;
					scale.y = collider.radius*2f;
					scale.z = collider.radius*2f;
					
					if (id == 4190049974){ id = 3224970585;}
					
				}
				else if (spawnItem.TryGetComponent(typeof(BoxCollider), out Component compt))
				{
					BoxCollider collider = spawnItem.GetComponent(typeof(BoxCollider)) as BoxCollider;
					//position += collider.center;
					scale = collider.size;
					if(id ==3224970585){ id = 4190049974;}
				}
				else if (spawnItem.TryGetComponent(typeof(CapsuleCollider), out Component compy))
				{
					CapsuleCollider collider = spawnItem.GetComponent(typeof(CapsuleCollider)) as CapsuleCollider;
					scale.x = collider.radius*2f;
					scale.z = collider.radius*2f;
					scale.y = collider.height;
					if(id ==3224970585){ id = 4190049974;}
				}
			}
			
			if (scale.x == 1f && scale.y == 1f && scale.z == 1f)
			{
				scale = spawnItem.lossyScale;
			}
		}
		else if (id == 3028561942 || id == 2493569314 || id == 3279304307)
		{
			rotation.y -= 90f;
		}
		else if (spawnItem.name.Contains("hinged"))
		{
			scale.x = 0.01f;
		}
		else if (id == 0)
		{
			return;
		}
		
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
		/*}*/
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

	public static void Offset(PrefabDataHolder[] prefabs, CircuitDataHolder[] circuits, Vector3 offset)
	{
		for (int k = 0; k < prefabs.Length; k++)
		{
			prefabs[k].prefabData.position += offset;
			prefabs[k].CastPrefabData();
		}
		for (int k = 0; k < circuits.Length; k++)
		{
			
			circuits[k].circuitData.wiring += offset;
						
			for(int l = 0; l < circuits[k].circuitData.connectionsIn.Length; l++)
			{
				circuits[k].circuitData.connectionsIn[l].wiring += offset;	
			}
			
			for(int l = 0; l < circuits[k].circuitData.connectionsOut.Length; l++)
			{
				circuits[k].circuitData.connectionsOut[l].wiring += offset;
			}
			
			circuits[k].CastCircuitData();
			
		}
		
	}

	//i wrote this ugliness before knowing about protobuf
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

	public static void deleteDuplicates(PrefabDataHolder[] prefabs)
	{
		int count = 0;
		Vector3 variance = new Vector3(.005f, .005f, .005f);
		for (int k = 0; k < prefabs.Length; k++)
		{
			for (int l = 0; l <prefabs.Length; l++)
			{
				if (prefabs[k]!=null && prefabs[l]!=null)
				{
					if (prefabs[k].prefabData.id == prefabs[l].prefabData.id 
					&& Mathf.Abs(prefabs[k].prefabData.position.x - prefabs[l].prefabData.position.x) < variance.x 
					&& Mathf.Abs(prefabs[k].prefabData.position.y - prefabs[l].prefabData.position.y) < variance.y
					&& Mathf.Abs(prefabs[k].prefabData.position.z - prefabs[l].prefabData.position.z) < variance.z	
					
					&& Mathf.Abs(prefabs[k].prefabData.rotation.x - prefabs[l].prefabData.rotation.x) < variance.x 
					&& Mathf.Abs(prefabs[k].prefabData.rotation.y - prefabs[l].prefabData.rotation.y) < variance.y
					&& Mathf.Abs(prefabs[k].prefabData.rotation.z - prefabs[l].prefabData.rotation.z) < variance.z	
					
					&& Mathf.Abs(prefabs[k].prefabData.scale.x - prefabs[l].prefabData.scale.x) < variance.x 
					&& Mathf.Abs(prefabs[k].prefabData.scale.y - prefabs[l].prefabData.scale.y) < variance.y
					&& Mathf.Abs(prefabs[k].prefabData.scale.z - prefabs[l].prefabData.scale.z) < variance.z	)
					{
						if (k!=l)
						{
							Debug.LogError(prefabs[k].prefabData.id + " x:" + prefabs[k].prefabData.position.x +
							" y:" + prefabs[k].prefabData.position.y +
							" z:" + prefabs[k].prefabData.position.z);
							GameObject.DestroyImmediate(prefabs[k].gameObject);
							prefabs[k] = null;
							count ++;
						}
						
					}
				}
			}
		}
		Debug.LogError(count + " prefabs deleted.");
	}

	public static void addSpawners()
	{
		uint id = 0;
		Vector3 rotation = new Vector3(0f, 0f, 0f);
		Vector3 position = new Vector3(750f, 1f, 750f);
		Vector3 scale = new Vector3(1f, 1f, 1f);
		createPrefab("Decor", id,	position, rotation, scale);
		position = new Vector3(750f, 1f, 0f);
		createPrefab("Decor", id,	position, rotation, scale);
		position = new Vector3(0f, 1f, 750f);
		createPrefab("Decor", id,	position, rotation, scale);
		
	}

	public static void keepPrefabList(PrefabDataHolder[] prefabs, uint[] keepersList)
	{
		
		bool keeper;
		for (int k = 0; k < prefabs.Length; k++)
		{
			
			keeper = false;
			
			if (prefabs[k] != null)
			{
				for (int l =0; l < keepersList.Length; l++)
				{
					
					if( prefabs[k].prefabData.id == keepersList[l])
					{
						keeper = true;
					}
					
				}
				if (!keeper)
					{
						GameObject.DestroyImmediate(prefabs[k].gameObject);
						prefabs[k] = null;
					}						
			}
		}
		
	}

	public static void keepElectrics(PrefabDataHolder[] prefabs, CircuitDataHolder[] circuits)
	{
		uint[] electricityList = new uint[]{1331920001,3467084113,1523703314,4129440825,500822506,1174518703,
			1479592929, 2864014888,1841596500,3165678508,3622071578,
			2179325520, 4224395968,1802909967,2055550712,850739563,2873681431,1268553078, 
			3767520300, 34236153, 3192000101,4124892809};
		bool electricPrefab;
		for (int k = 0; k < prefabs.Length; k++)
		{
			
			electricPrefab = false;
			
			if (prefabs[k] != null)
			{
				for (int l =0; l < electricityList.Length; l++)
				{
					
					if( prefabs[k].prefabData.id == electricityList[l])
					{
						electricPrefab = true;
					}
					
				}
				if (!electricPrefab)
					{
						GameObject.DestroyImmediate(prefabs[k].gameObject);
						prefabs[k] = null;
					}						
			}
		}
		
	}

	public static void removeElectrics(PrefabDataHolder[] prefabs, CircuitDataHolder[] circuits)
	{
		uint[] electricityList = new uint[]{1331920001,3467084113,1523703314,4129440825,500822506,1174518703,
			1479592929, 2864014888,1841596500,3165678508,3622071578,
			2179325520, 4224395968,1802909967,2055550712,850739563,2873681431,1268553078, 
			3767520300, 34236153, 3192000101,4124892809};
		bool electricPrefab;
		for (int k = 0; k < prefabs.Length; k++)
		{
			
			electricPrefab = false;
			
			if (prefabs[k] != null)
			{
				for (int l =0; l < electricityList.Length; l++)
				{
					
					if( prefabs[k].prefabData.id == electricityList[l])
						electricPrefab = true;					
					
				}
				if (electricPrefab)
					{
						GameObject.DestroyImmediate(prefabs[k].gameObject);
						prefabs[k] = null;
					}						
			}
		}
		
		for (int k = 0; k < circuits.Length; k++)
		{
			
			if (circuits[k] != null)
			{
				GameObject.DestroyImmediate(circuits[k].gameObject);
				circuits[k] = null;
			}
			
		}
		
	}

	public static void breakMonument(PrefabDataHolder[] prefabs, float z, bool destroy)
	{
		//spawner IDs
		uint randomCrate = 2678767843;
		uint miliCrate = 3071563544;
		uint normalCrate = 2486800063;
		uint mineCrate = 3843853141;
		uint eliteCrate = 1428457674;
		uint toolCrate = 2389956165;
		uint woodCrate = 2042207134;
		
		
		uint medsCrate = 3478105708;
		uint foodCrate = 3173379894;
		
		uint barrel = 1076121145;
		uint dieselBarrel = 3903362521;
		uint oilBarrel = 2997114422;
		
		uint vehicleParts = 2499231611;
		uint foodBox = 244382885;
		
		uint randomOre = 1654744457;
		uint minecart = 2913715266;
		
		
		
		int count = 0;
		Transform scanItem;
		Transform spawnItem, childSpawnItem, grandchildSpawnItem, greatGrandchildSpawnItem;
		
		uint replaceID = 0;
		string[] parse;
		string[] parse2;
		string category;
		string itemName;
		float adjustZ = 500f - z;
		float adjustXY = TerrainManager.TerrainSize.x / 2f;
		Vector3 adjuster = new Vector3(adjustXY,adjustZ,adjustXY);

		for (int k = 0; k < prefabs.Length; k++)
		{
			if (prefabs[k] != null)
			{
				monumentName = prefabs[k].name;
				Debug.LogError(monumentName);

					for (int i = 0; i < prefabs[k].transform.childCount; i++)
						{
							scanItem = prefabs[k].transform.GetChild(i);
							category = scanItem.name;
							Debug.LogError(category);
										
										replaceID = 0;
										if(monumentName == "power sub big 1" || monumentName == "power sub big 2" || 
										monumentName == "power sub small 1" || monumentName == "power sub small 2")
										{
											
											for (int j = 0; j < scanItem.transform.childCount; j++)
											{
												spawnItem = scanItem.transform.GetChild(j);
												if(category == "Loot Spawner")
													{
														replaceID = normalCrate;
														blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
														
													}
													else if(category == "Barrel Spawner")
													{
														replaceID = barrel;
														blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
													}
													else if(category == "Prevent Building")
													{
														replaceID = 3224970585;
														blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
													}
													
											}
											
											
											if(category != "Loot Spawner" && category != "Barrel Spawner" && category != "Prevent Building")
											{
												itemName = scanItem.name;
												replaceID = AssetManager.partialToID(itemName, monumentName);
												blacklistedCreatePrefab("Decor", replaceID, adjuster, scanItem);
											}
										}
										//stairs doors wrong chairs ladders excavator list
										if(category == "SafeZoneLarge")
										{
											replaceID = 316558065;
										}
										if(category == "MissionProvider_Stables_A")
										{
											replaceID = 930153435;
										}
										if(category == "MissionProvider_Stables_B")
										{
											replaceID = 3892089538;
										}
										if(category == "MissionProvider_Fishing_A")
										{
											replaceID = 350957926;
										}
										if(category == "MissionProvider_Fishing_B")
										{
											replaceID = 322083179;
										}
										if(category == "shop-music")
										{
											
										}
										if(category == "AlarmSystem")
										{
											replaceID = 680397581;
										}
										if(category == "stables_shopkeeper")
										{
											
											replaceID =7488435;
										}
										
										if(category == "boat_shopkeeper")
										{
											replaceID =2913617060;
										}
										if(category == "NPCVendingMachine_FishExchange")
										{
											replaceID = 712757139;
										}
										if(category == "Shopkeeper_VM_Invis (4)")
										{
											replaceID = 858853278;
										}
										if(category == "SafeZone")
										{
											replaceID = 316558065;
										}
										if(category == "recycler_static")
										{
											replaceID = 1729604075;
										}
										if(category == "Water_cull")
										{
											replaceID = 1206870171;
										}
										if(category == "Excavator_Lights")
										{
											replaceID = 863874129;
										}
										/*
										if(category == "AI" && monumentName == "excavator 1")
										{
											replaceID = 3083644891;
										}//
										*/

										if(category == "Engine")
										{
											replaceID = 2982299738;
										}
										if(category == "Excavator_Output_Pile" || category == "Excavator_Output_Pile (1)")
										{
											replaceID = 673116596;
										}
										
									
										
										
										/*
										if(category == "military_tunnel_coverpoints2")
										{
											replaceID = 2236147019;
										}
										if(category == "TriggerWakeAIZ")
										{
											replaceID = 4041251023;
										}
										if(category == "military_tunnel_patrolpoints")
										{
											replaceID = 2619674770;
										}
										*/
										if(category == "ReinforcementsListener")
										{
											replaceID = 667569163;
										}
										if(category == "LandingTrigger")
										{
											replaceID = 1594218189;
										}
										if(category == "SlotMachine" ||category == "SlotMachine (1)" ||category == "SlotMachine (2)")
										{
											replaceID = 2230162530;
										}
										if(category == "small_refinery_static")
										{
											replaceID = 919097516;
										}
								
										if(category == "carshredder.entity")
										{
											replaceID = 1114045676;
										}
										else if(category == "Water Trigger")
										{
											replaceID = 1165979067;
										}
										else if(category == "prevent_movement" || category == "prevent_movement (1)" || category == "prevent_movement (2)" || category == "prevent_movement (3)" ||
										category == "prevent_movement (4)" )
										{
											replaceID = 3521578167;										
										}
										else if (category == "DropZone")
										{
											replaceID = 4069184361; 
										}
										else if (category == "prevent_building" || category == "PreventBuilding (1)" || category == "Prevent_building")
										{
											replaceID = 3224970585;  //could be either sphere or cube who knows
										}
										else if (category == "ReclaimContainerStatic")
										{
											replaceID = 3273404916;
										}
										else if (category == "PlayerRespawnSpawner")
										{
											if(monumentName == "Bandit town")
											{
												replaceID = 3810400291;
											}
											else
											{
												replaceID = 1919922518;
											}
										}
										
										else if (category == "PreventBuilding")
										{
											replaceID = 3224970585;
										}
										else if (category == "window_hack" || category == "Cube"|| category == "Cube (1)"|| category == "Cube (2)"|| category == "Cube (3)"|| category == "Cube (4)"
										|| category == "Cube (5)"|| category == "Cube (6)"|| category == "Cube (7)"|| category == "Cube (8)"|| category == "Cube (9)")
										{
											replaceID = 3000049339;
										}
										else if (category == "airwolf_compound")
										{
											replaceID = 3614389919;
										}
										else if (category == "elevator.static.top")
										{
											replaceID = 1033358365;
										}
										else if (category == "corridor_train_tunnel_entrance")
										{
											replaceID = 840796039;
										}
										else if (category == "entrance_monuments_b")
										{
											replaceID = 3604512213;
										}
										else if (category == "entrance_monuments_a")
										{
											replaceID = 856899687;
										}
										else if (category == "Water_Culling_Volume")
										{
											replaceID = 1206870171;
										}
										else if(category == "phonebooth.static")
										{
												replaceID = 1009655496;
										}
										else if(category == "Bandit Swamp Fog FX")
										{
												replaceID = 393956209;
										}
										else if(category == "bandit_conversationalist")
										{
											replaceID = 251735616;
										}
										else if (category == "Marketplace")
										{
											replaceID = 3953076030;
										}
										else if(category == "pavement_6x6_nocurb"|| category == "pavement_6x6_nocurb (1)"|| category == "pavement_6x6_nocurb (2)"|| category == "pavement_6x6_nocurb (3)")
										{
											replaceID = 1771757602;
										}
										else if(category == "ExcavatorSignalComputer")
										{
											replaceID = 3268004142;
										}
										else if(category == "exhaust_colliders")
										{
											replaceID = 3000049339;
										}
										

										blacklistedCreatePrefab("Decor", replaceID, adjuster, scanItem);
										
								for (int j = 0; j < scanItem.transform.childCount; j++)
								{
									
									
									spawnItem = scanItem.transform.GetChild(j);
									itemName = spawnItem.name;
									
									parse2 = itemName.Split('_');
										replaceID = AssetManager.partialToID(itemName, monumentName);
										
										if (replaceID == 2094834225)
											replaceID = 4143610332;
										
										
										//non mapping, or problematic branches

										if(category == "Sound" || category == "AirfieldAI" || category == "Terrain Carve" || category == "Terrain Anchor" || category == "Terrain Anchor (1)" || category == "Reverb Zones" || category == "Powerline" ||
										category == "Terrain Anchor (2)" || category == "Water culling" 
										|| category == "Power" || category == "phonebooth.static" || category == "carshredder.entity" || category == "pavement_6x6_nocurb"
										|| category == "pavement_6x6_nocurb (1)"|| category == "pavement_6x6_nocurb (2)"|| category == "pavement_6x6_nocurb (3)" || category == "military_tunnel_scientist_spawner" || category =="military_tunnel_patrolpoints" 
										|| category == "military_tunnel_coverpoints2" || category == "Terrain Check" || category == "Terrain Trigger" || category == "Proxies" 
										|| category == "TriggerWakeAIZ" || category == "Navmesh" || category == "Dungeon_Volumes" || category == "TerrainFilter" || category == "MainReset"
										 || category == "MainReset" || category == "ReinforcementsListener" || category == "Landing Trigger" 
										|| category == "OilrigAI2" || category == "OilrigLargeAudio" || category == "OilrigAI" || category == "OilrigSmallAudio" || category == "entrance_monuments_a" || category == "entrance_monuments_b")
										{ /*ignore*/ }
										else if(category == "TrainyardAI")
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
														childSpawnItem = spawnItem.transform.GetChild(n);
														
														for (int o = 0; o < childSpawnItem.transform.childCount; o++)
															{
																grandchildSpawnItem = childSpawnItem.transform.GetChild(o);
																if (spawnItem.name == "ScientistSpawners")
																{
																	replaceID = 2359528520;
																	blacklistedCreatePrefab("Decor", replaceID, adjuster, grandchildSpawnItem);
																}
															}
													}
										}
										else if(category == "Bradley")
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
														childSpawnItem = spawnItem.transform.GetChild(n);
														if (childSpawnItem.name.Contains("Waypoint"))
														{
															replaceID = 147092032;
														}
														else if (childSpawnItem.name.Contains("InterestPoint"))
														{
															replaceID = 2998914636;
														}
														blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
													}
										}
										else if(category == "HackedCrateSpawn")
										{
											replaceID = 2043434947;
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										else if(category == "Terrain triggers")
										{
											replaceID = 1960680350;
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										//special cases
										else if(category == "Collider helpers" || category == "helpers")
										{	
											replaceID = 3000049339;
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										else if(category == "sci_compound_scientist_spawner")
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
															childSpawnItem = spawnItem.transform.GetChild(n);
															itemName = childSpawnItem.name;
															replaceID = 2359528520;
															if (itemName == "ScientistSpawnPoint")
															{
																blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
															}
													}
										}
										else if(category == "ScientistSpawners" && monumentName == "excavator 1")
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
															childSpawnItem = spawnItem.transform.GetChild(n);
															itemName = childSpawnItem.name;
															replaceID = 2359528520;
															blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
													}
										}
										else if(category == "ScientistSpawners" && monumentName != "excavator 1")
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
															childSpawnItem = spawnItem.transform.GetChild(n);
															itemName = childSpawnItem.name;
															replaceID = 2359528520;
															blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
													}
										}
										else if(category == "military_tunnel_lab_scientist_spawner")
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
														if (spawnItem.name.Contains("HTNSpawn"))
														{
															childSpawnItem = spawnItem.transform.GetChild(n);
															itemName = childSpawnItem.name;
															replaceID = 2359528520;
															blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
														}
													}
										}
										
										else if(category == "Excavators")
										{	

											if(itemName == "stairs" || itemName == "Doors" || itemName == "props")
											{
													for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
														childSpawnItem = spawnItem.transform.GetChild(n);
														itemName = childSpawnItem.name;
														replaceID = AssetManager.partialToID(itemName, monumentName);
														blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
													}
											}
											else
											{
											itemName = spawnItem.name;
											replaceID = AssetManager.partialToID(itemName, monumentName);
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
											}
										}
										else if (category == "AirWolfSpawner")
										{
											replaceID = 3960558419;
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										else if(category == "HorseSpawners")
										{
											replaceID = 4058311563;
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										else if(category == "BoatSpawner")
										{
											replaceID = 2609911909;
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										
										else if(category == "Prevent_movement" || category == "Prevent_movment")
										{
											replaceID =  3521578167;
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										else if(category == "Prevent Building" || category == "PreventBuilding (1)" || category == "PreventBuilding")
										{
											replaceID = 4190049974;
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										else if(category == "SamSites")
										{
											replaceID = 2934818568;
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										else if(category == "Diesel Spawner")
										{
											replaceID = dieselBarrel;
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										else if(category == "ShredableSpawner")
										{
											replaceID = 962565779;
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										else if(category == "blocker")
										{
											replaceID = 3521578167;
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										else if(category == "bandit_town_npc_spawner")
										{
											if (spawnItem.name.Contains("StationaryPoint"))
											{
												replaceID = 967694666;
												blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
											}
										}										
										else if(category == "Bandit Lighting AlwaysOn")
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
														childSpawnItem = spawnItem.transform.GetChild(n);
														
															for (int o = 0; o < childSpawnItem.transform.childCount; o++)
															{
																grandchildSpawnItem = childSpawnItem.transform.GetChild(o);
																itemName = grandchildSpawnItem.name;
																
																Debug.LogError (itemName);
																replaceID = AssetManager.partialToID(itemName, monumentName);
																blacklistedCreatePrefab("Decor", replaceID, adjuster, grandchildSpawnItem);
															}
													}
										}
										else if(category == "Bandit Lighting")
										{											
												if (spawnItem.name == "Cabin Scrap Exchange" || spawnItem.name == "Cabin Stuff" || spawnItem.name == "Cabin Food Market" || 
												spawnItem.name == "Cabins Windmill" || spawnItem.name == "Windmill" || spawnItem.name == "Perimeter" ||
												spawnItem.name == "Cabin Large Edge" || spawnItem.name == "Helipad" || spawnItem.name == "Dredge_Exterior")
												{
													for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
														childSpawnItem = spawnItem.transform.GetChild(n);
														itemName = childSpawnItem.name;
														
														
														
														
														replaceID = AssetManager.partialToID(itemName, monumentName);
														blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
														
													}
												}
												else
												{
													itemName = spawnItem.name;
													
													replaceID = AssetManager.partialToID(itemName, monumentName);
													blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
													
												}
										}
										else if(category == "Train Tracks")
										{											
											if (spawnItem.name == "crane_tracks")
											{
												
												for (int n = 0; n < spawnItem.transform.childCount; n++)
												{
													childSpawnItem = spawnItem.transform.GetChild(n);
													itemName = childSpawnItem.name;
													
													replaceID = AssetManager.partialToID(itemName, monumentName);
													blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
													
												}
											}
											else
											{
												itemName = spawnItem.name;
													
													replaceID = AssetManager.partialToID(itemName, monumentName);
													blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
											}
										}
										else if( category == "Wooden_Cabins")
										{
											if (spawnItem.name == "wooden_cabin_c" || spawnItem.name == "wooden_cabin_a" || spawnItem.name == "wooden_cabin_a (1)" || spawnItem.name == "wooden_cabin_b" || 
												spawnItem.name == "wooden_cabin_d" || spawnItem.name == "wooden_cabin_d (1)" || spawnItem.name == "wooden_cabin_e")
												{
													for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
														childSpawnItem = spawnItem.transform.GetChild(n);
														itemName = childSpawnItem.name;													
														replaceID = AssetManager.partialToID(itemName, monumentName);
														createPrefab("Decor", replaceID, childSpawnItem.position - adjuster, childSpawnItem.eulerAngles, childSpawnItem.lossyScale);
													}
												}
										}
										//puzzle branches
										else if(category == "Puzzle" || category == "Puzzle_Factory" || category == "Puzzle_Jumping")
										{	
											if (spawnItem.name == "Crate Spawner Container Office" || spawnItem.name == "Crate Spawner Tier2"
											|| spawnItem.name == "Crates_spawner_tier2" || spawnItem.name == "Crate Spawner Plant")
											{
												
												for (int n = 0; n < spawnItem.transform.childCount; n++)
												{
													childSpawnItem = spawnItem.transform.GetChild(n);
													replaceID =  randomCrate;
													createPrefab("Decor", replaceID, childSpawnItem.position - adjuster, childSpawnItem.eulerAngles, childSpawnItem.lossyScale);
												}
											}
											else if (spawnItem.name == "Crate Spawner Office" 
											|| spawnItem.name == "Crate Spawner Office (1)")
											{
												for (int n = 0; n < spawnItem.transform.childCount; n++)
												{
													childSpawnItem = spawnItem.transform.GetChild(n);
													replaceID =  miliCrate;
													createPrefab("Decor", replaceID, childSpawnItem.position - adjuster, childSpawnItem.eulerAngles, childSpawnItem.lossyScale);
												}
											}
											else if( spawnItem.name == "Surface Generator" || spawnItem.name == "Green Card Sewer Doors" || spawnItem.name == "Armory")
											{
												for (int n = 0; n < spawnItem.transform.childCount; n++)
												{
													childSpawnItem = spawnItem.transform.GetChild(n);
													itemName = childSpawnItem.name;
													if(itemName == "Crate Spawner Armory")
													{
														for (int o = 0; o < childSpawnItem.transform.childCount; o++)
														{
															grandchildSpawnItem = childSpawnItem.transform.GetChild(o);
															replaceID = randomCrate;
															blacklistedCreatePrefab("Decor", replaceID, adjuster, grandchildSpawnItem);
														}
													}
													else
													{
													itemName = childSpawnItem.name;
													replaceID = AssetManager.partialToID(itemName, monumentName);
													blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
													}
												}
											}
											else if (spawnItem.name == "Crate Spawner High cabin" || spawnItem.name == "Crate Spawner Low cabin")
											{
												for (int n = 0; n < spawnItem.transform.childCount; n++)
												{
													childSpawnItem = spawnItem.transform.GetChild(n);
													replaceID = normalCrate;
													blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
												}
											}
											else if (spawnItem.name == "Warehouse" || spawnItem.name == "Control Room")
											{
												for (int n = 0; n < spawnItem.transform.childCount; n++)
												{
													childSpawnItem = spawnItem.transform.GetChild(n);
													itemName = childSpawnItem.name;
													if (itemName == "Crate Spawner Warehouse" || itemName == "Crate Spawner Control Room")
													{
														for (int o = 0; o < childSpawnItem.transform.childCount; o++)
														{
															grandchildSpawnItem = childSpawnItem.transform.GetChild(o);
															replaceID = randomCrate;
															blacklistedCreatePrefab("Decor", replaceID, adjuster, grandchildSpawnItem);
														}
													}
													else
													{
														itemName = childSpawnItem.name;
														replaceID = AssetManager.partialToID(itemName, monumentName);
														blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
													}
												}
											}
											else if (spawnItem.name == "Electric_poles")
											{
												
												for (int n = 0; n < spawnItem.transform.childCount; n++)
												{
													childSpawnItem = spawnItem.transform.GetChild(n);
													replaceID = 4171653793;
													blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
												}
											}
											else
											{
												for (int n = 0; n < spawnItem.transform.childCount; n++)
												{

													
													childSpawnItem = spawnItem.transform.GetChild(n);
													itemName = childSpawnItem.name;
													
													
													
													if (itemName=="Crate Spawner Bandit Core" || itemName== "Crate Spawner Armory" || itemName == "Crate Spawner_Lab" || itemName == "Crate Spawner Plant" || itemName == "Crate Spawner Tower"
													|| itemName == "Crate Spawner_L4"|| itemName == "Crate Spawner_L6"|| itemName == "Crate Spawner_L5")
													{
														
														for (int o = 0; o < childSpawnItem.transform.childCount; o++)
														{
															grandchildSpawnItem = childSpawnItem.transform.GetChild(o);
															replaceID = randomCrate;
															blacklistedCreatePrefab("Decor", replaceID, adjuster, grandchildSpawnItem);
														}
														
													}
													else if (itemName=="Barrel Spawner Tower")
													{
														for (int o = 0; o < childSpawnItem.transform.childCount; o++)
														{
															grandchildSpawnItem = childSpawnItem.transform.GetChild(o);
															replaceID = barrel;
															createPrefab("Decor", replaceID, grandchildSpawnItem.position - adjuster, grandchildSpawnItem.eulerAngles, grandchildSpawnItem.lossyScale);
														}
														
													}
													else if (itemName== "Elite Spawner_Lab" || itemName == "Elite Spawner_L6")
													{
														for (int o = 0; o < childSpawnItem.transform.childCount; o++)
														{
															grandchildSpawnItem = childSpawnItem.transform.GetChild(o);
															replaceID = eliteCrate;
															createPrefab("Decor", replaceID, grandchildSpawnItem.position - adjuster, grandchildSpawnItem.eulerAngles, grandchildSpawnItem.lossyScale);
														}
													}
													else if (itemName == "military_tunnel_scientist_spawner")
													{
														for (int o = 0; o < childSpawnItem.transform.childCount; o++)
														{
															grandchildSpawnItem = childSpawnItem.transform.GetChild(o);
															for (int p = 0; p < grandchildSpawnItem.transform.childCount; p++)
															{
																if(grandchildSpawnItem.name == "HTNSpawn")
																{
																greatGrandchildSpawnItem = grandchildSpawnItem.transform.GetChild(p);
																replaceID = 2359528520;
																blacklistedCreatePrefab("Decor", replaceID, adjuster, greatGrandchildSpawnItem);
																}
																
															}
														}
													}
													itemName = childSpawnItem.name;
													replaceID = AssetManager.partialToID(itemName, monumentName);		
													blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
													
												}
												
												itemName = spawnItem.name;
												replaceID = AssetManager.partialToID(itemName, monumentName);
												blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
											}
										}
										
										
										
										else if(category == "Barrel Spawner Plant" || category == "Barrel Spawner")
										{
											replaceID = barrel;
											createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);
										}
										else if(category == "Ore LootSpawner")
										{											
											replaceID = randomOre;
											createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);
										}
										else if(category == "Minecart_spawner" || category == "Minecart Spawner")
										{
											replaceID = minecart;
											createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);
										}
										else if(category == "Minecrate_spawner" || category == "Minecrate Spawner" || category == "Mine_crate_spawner")
										{
											replaceID = mineCrate;
											createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);
										}
										else if(category == "Elite Spawner" || category == "Blue Spawner")
										{
											replaceID = eliteCrate;
											createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);
										}
										else if(category == "Vehicle Comp Spawner")
										{
											replaceID = vehicleParts;
											createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);
										}
										else if(category == "Food Spawner")
										{
											
											replaceID = foodCrate;
											if (monumentName == "gas station 1" || monumentName == "supermarket 1")
											{
												replaceID = foodBox;
											}
											createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);
										}
										
										else if(category == "Crate Spawner" || category == "Crates_spawner")
										{
											if( monumentName == "sphere tank")
											{
												if (spawnItem.name.Contains("8"))
												{
													replaceID = miliCrate;
												}
												else
												{
													replaceID = normalCrate;
												}
												blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
											}
											else
											{
											replaceID = randomCrate;
											createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);
											}
										}
										else if(category == "Crate Spawner (1)")
										{		
												replaceID = normalCrate;
												
												if (itemName == "spawnpoint2")
												{
													replaceID = randomCrate;
												}
												
												createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);

											
										}
										
										else if (category == "LootSpawners" || category == "Spawners")
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
											{
												childSpawnItem = spawnItem.transform.GetChild(n);
												itemName = spawnItem.name;
												if (itemName=="Barrel Spawner" || itemName=="Barrel Spawner (1)"|| itemName=="Barrel Spawner (2)")
												{
													
														replaceID = barrel;
														createPrefab("Decor", replaceID, childSpawnItem.position - adjuster, childSpawnItem.eulerAngles, childSpawnItem.lossyScale);
													
													
												}
												if (itemName=="Oil Barrel Spawner")
												{
													
														
														replaceID = oilBarrel;
														createPrefab("Decor", replaceID, childSpawnItem.position - adjuster, childSpawnItem.eulerAngles, childSpawnItem.lossyScale);
													
													
												}
												if (itemName=="Crate Spawner_Average" || itemName == "Crate Spawner Average")
												{
													
														replaceID = normalCrate;
														createPrefab("Decor", replaceID, childSpawnItem.position - adjuster,childSpawnItem.eulerAngles, childSpawnItem.lossyScale);
													
													
												}
												if (itemName=="Crate Spawner_Blend")
												{
													
														replaceID = randomCrate;
														createPrefab("Decor", replaceID, childSpawnItem.position - adjuster,childSpawnItem.eulerAngles, childSpawnItem.lossyScale);
													
													
												}
												if (itemName=="Crate Spawner_Excellent")
												{
													
														replaceID = eliteCrate;
														createPrefab("Decor", replaceID, childSpawnItem.position - adjuster, childSpawnItem.eulerAngles, childSpawnItem.lossyScale);
													
													
												}
											}
										}
										else if (category == "Rocket_Factory_Stairs" || category == "Factory")
										{
											itemName = spawnItem.name;											
											Debug.LogError(itemName);
											replaceID = AssetManager.partialToID(itemName, monumentName);
											if (replaceID == 1864484874  && category == "Factory")
											{
												replaceID= 3664617845;
											}
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										else if (category == "Wooden_Cabins")
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
											{
												childSpawnItem = spawnItem.transform.GetChild(n);
												itemName = childSpawnItem.name;
												replaceID = AssetManager.partialToID(itemName, monumentName);
												blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
											}
										}
										else if (category == "Building" )
										{
												itemName = spawnItem.name;
												replaceID = AssetManager.partialToID(itemName, monumentName);
												blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
											for (int n = 0; n < spawnItem.transform.childCount; n++)
											{
												childSpawnItem = spawnItem.transform.GetChild(n);
												itemName = childSpawnItem.name;
												replaceID = AssetManager.partialToID(itemName, monumentName);
												blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
											}
										}
										else if( category == "structures" || category == "Dredge" || category == "BigWheel" || category == "Buildings" || category == "Structures" || category == "ships")
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
											{
												if(spawnItem.name == "BoatShop Emblem")
												{
													replaceID = 1476825658;
													blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
												}
												
												 if (spawnItem.name == "buildings" || spawnItem.name == "vendors_hut" || spawnItem.name == "helipad"
												 || spawnItem.name == "vegetation" || spawnItem.name == "Props" || spawnItem.name == "perimeter_wall" || spawnItem.name == "props"
												 || spawnItem.name == "BigWheel" || spawnItem.name == "Dish1" || spawnItem.name == "Dish2" || spawnItem.name == "Dish3")
												 {
													 
														
													 
														childSpawnItem = spawnItem.transform.GetChild(n);
														itemName = childSpawnItem.name;
														
														if (itemName == "wooden_cabin_f" || itemName == "props")
														{
																for (int o = 0; o < childSpawnItem.transform.childCount; o++)
																{
																	grandchildSpawnItem = childSpawnItem.transform.GetChild(o);
																	itemName = grandchildSpawnItem.name;
																	Debug.LogError("itemName");
																	replaceID = AssetManager.partialToID(itemName, monumentName);
																	blacklistedCreatePrefab("Decor", replaceID, adjuster, grandchildSpawnItem);
																}
														}
														else
														{
														replaceID = AssetManager.partialToID(itemName, monumentName);
														blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
														}
														
												 }
												 else if( spawnItem.name.Contains("barge_base_"))
												 {
														childSpawnItem = spawnItem.transform.GetChild(n);
														itemName = childSpawnItem.name;
														replaceID = AssetManager.partialToID(itemName, monumentName);
														blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
												 }
												 
											}
														itemName = spawnItem.name;
														
														replaceID = AssetManager.partialToID(itemName, monumentName);
														blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
														
										}
										else if(category == "Additional NPC Shops" || category == "Shops")
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
														childSpawnItem = spawnItem.transform.GetChild(n);
														itemName = childSpawnItem.name;
														replaceID = AssetManager.partialToID(itemName, monumentName);
														blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
													}
										}
										else if (category == "Sewers" || category == "Cave" || category == "cave")
										{
											itemName = spawnItem.name;
											
											
											if (itemName == "mine_entrance" || itemName == "mine_tunnel" || itemName == "cave_ladder")
											{
												for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
														childSpawnItem = spawnItem.transform.GetChild(n);
														itemName = childSpawnItem.name;
														
														
														if (itemName == "_Glowworms" || itemName == "cave_puzzle_a" || itemName == "mine_tunnel (5)" ||
														itemName == "mine_tunnel (2)")
														{
															for (int o = 0; o < childSpawnItem.transform.childCount; o++)
																{
																	grandchildSpawnItem = childSpawnItem.transform.GetChild(o);
																	itemName = grandchildSpawnItem.name;
																	replaceID = AssetManager.partialToID(itemName, monumentName);
																	blacklistedCreatePrefab("Decor", replaceID, adjuster, grandchildSpawnItem);
																}
														}
														else
														{
															itemName = childSpawnItem.name;
															replaceID = AssetManager.partialToID(itemName, monumentName);
															blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
														}
													}
											}
											else
											{
													itemName = spawnItem.name;
													replaceID = AssetManager.partialToID(itemName, monumentName);
													blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
											}
											
										}
										else if (category == "Sewers_Bunker")
										{
											
												itemName = spawnItem.name;
												if (itemName == "tunnel_1" ||itemName == "tunnel_2" ||itemName == "tunnel_3" ||itemName == "tunnel_4" ||itemName == "tunnel_5"||
												 itemName == "tunnel_6" ||itemName == "bunker_room_1" ||itemName == "bunker_room_2" ||itemName == "bunker_room_3" 
												 ||itemName == "cave_tunnel_a" ||itemName == "cave_tunnel_c" ||itemName == "sewer_room")
												{
													for (int n = 0; n < spawnItem.transform.childCount; n++)
													{
														childSpawnItem = spawnItem.transform.GetChild(n);
														itemName = childSpawnItem.name;
														replaceID = AssetManager.partialToID(itemName, monumentName);
														blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
													}
												}
												else
												{
														itemName = spawnItem.name;
														replaceID = AssetManager.partialToID(itemName, monumentName);
														blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
												}
											
										}
										else if (category == "Tunnels" || category == "Mine_tunnels" || category == "Military_complex" || category == "Oil Rig Lighting" || category == "Small Oil Rig Lighting" 
										|| category == "Compound Lighting")
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
											{
												childSpawnItem = spawnItem.transform.GetChild(n);
												itemName = childSpawnItem.name;
												
												if (itemName == "buildings" || itemName == "Inner Red Corridor" || itemName == "L3" || itemName == "L0" || itemName == "L1" || itemName == "L1-2" || itemName == "L2" || itemName == "L4" ||
												 itemName == "Red Strobes Helipad" || itemName == "Marker Strobes" || itemName.Substring(0,1) == "_" || itemName == "buildings" || itemName == "BBQ Area")
												{
													for (int o = 0; o < childSpawnItem.transform.childCount; o++)
													{
														grandchildSpawnItem = childSpawnItem.transform.GetChild(o);
														itemName = grandchildSpawnItem.name;
														if (itemName.Substring(0, 1) == "_")
														{
															Debug.LogError(itemName);
															for (int p = 0; p < grandchildSpawnItem.transform.childCount; p++)
															{
																greatGrandchildSpawnItem = grandchildSpawnItem.transform.GetChild(o);
																itemName = greatGrandchildSpawnItem.name;
																Debug.LogError(itemName);
																replaceID = AssetManager.partialToID(itemName, monumentName);
																blacklistedCreatePrefab("Decor", replaceID, adjuster, grandchildSpawnItem);
															}
														}
														else
														{
															itemName = grandchildSpawnItem.name;
															replaceID = AssetManager.partialToID(itemName, monumentName);
															blacklistedCreatePrefab("Decor", replaceID, adjuster, grandchildSpawnItem);
														}
														
														
													}
												}
												else
												{
													
													replaceID = AssetManager.partialToID(itemName, monumentName);
													blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);

												}
											}
										}
										
										else if (category == "Signs" || category == "Militarytunnel Lighting v2"  )
										{
											for (int n = 0; n < spawnItem.transform.childCount; n++)
											{
												childSpawnItem = spawnItem.transform.GetChild(n);
												itemName = childSpawnItem.name;
												replaceID = AssetManager.partialToID(itemName, monumentName);
												blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
												
													for (int o = 0; o < childSpawnItem.transform.childCount; o++)
													{
														grandchildSpawnItem = childSpawnItem.transform.GetChild(o);
														
														if (childSpawnItem.name == "Cloning Vats")
														{
															
															
															for (int p = 0; p < grandchildSpawnItem.transform.childCount; p++)
															{
																greatGrandchildSpawnItem = grandchildSpawnItem.transform.GetChild(p);
																itemName = greatGrandchildSpawnItem.name;
																Debug.LogError(itemName);													
																replaceID = AssetManager.partialToID(itemName, monumentName);
																blacklistedCreatePrefab("Decor", replaceID, adjuster, greatGrandchildSpawnItem);
															}
														}
														else
														{
														
														
														itemName = grandchildSpawnItem.name;
														Debug.LogError (itemName);
														replaceID = AssetManager.partialToID(itemName, monumentName);
														blacklistedCreatePrefab("Decor", replaceID, adjuster, grandchildSpawnItem);
														
														}
													
												}
												
											}
											itemName = spawnItem.name;
											replaceID = AssetManager.partialToID(itemName, monumentName);
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
										}
										
										else if(category == "Oil Barrel Spawner" || category == "Oil_Barrels_spawner")
										{
											replaceID = oilBarrel;
											createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);
										}
										else if(category == "Food Spawner")
										{											
												replaceID = foodCrate;
												createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);
										}
										else if(category == "Barrel Spawner" || category == "BarrelSpawner" || category == "Barrels_spawner")
										{
											
												replaceID = barrel;
												createPrefab("Decor", replaceID, spawnItem.position - adjuster, spawnItem.eulerAngles, spawnItem.lossyScale);
										
										}
										else
										{
											itemName = spawnItem.name;
											
											replaceID = AssetManager.partialToID(itemName, monumentName);
											blacklistedCreatePrefab("Decor", replaceID, adjuster, spawnItem);
											/*
											for (int n = 0; n < spawnItem.transform.childCount; n++)
											{
												childSpawnItem = spawnItem.transform.GetChild(n);
												itemName = childSpawnItem.name;
												replaceID = AssetManager.partialToID(itemName, monumentName);
												blacklistedCreatePrefab("Decor", replaceID, adjuster, childSpawnItem);
											}
											*/
										}
									
								}
								
							
						}
					
					if (destroy)
					{
					GameObject.DestroyImmediate(prefabs[k].gameObject);
					prefabs[k] = null;
					}
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
	
	public static void addMonumentMarker(string mark)
	{
		Vector3 location = new Vector3(0,0,0);
		Vector3 rotation = new Vector3(0,0,0);
		Vector3 scale = new Vector3(1,1,1);
		createPrefab(mark, 1724395471, location, rotation, scale);
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
		
		public static IEnumerator SpawnPrefabs(PrefabData[] prefabs, int progressID, Transform parent)
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
				
                Spawn(Load(prefabs[i].id), prefabs[i], parent);
            }
            Progress.Report(progressID, 0.99f, "Spawned " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);
        }
		
		public static IEnumerator SpawnCircuits(CircuitData[] circuitData, int progressID)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < circuitData.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 4f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / circuitData.Length, "Spawning Electric: " + i + " / " + circuitData.Length);
                    sw.Restart();
                }
                Spawn(DefaultPrefab, circuitData[i], ElectricsParent);
            }
            Progress.Report(progressID, 0.99f, "Spawned " + circuitData.Length + " circuits.");
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
		
		public static IEnumerator DeleteCircuits(CircuitDataHolder[] circuits, int progressID = 0)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            if (progressID == 0)
                progressID = Progress.Start("Delete Circuits", null, Progress.Options.Sticky);

            for (int i = 0; i < circuits.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 0.25f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / circuits.Length, "Deleting Circuits: " + i + " / " + circuits.Length);
                    sw.Restart();
                }
                GameObject.DestroyImmediate(circuits[i].gameObject);
            }
            Progress.Report(progressID, 0.99f, "Deleted " + circuits.Length + " circuits.");
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
			string shopkeeper = "";

            for (int i = 0; i < prefabs.Length; i++)
            {
				
				if (prefabs[i].prefabData.id == 1724395471)
				{
					//do not rename monument marker
				}
				else if (prefabs[i].prefabData.id == 856899687 || prefabs[i].prefabData.id == 3604512213)
				{
					prefabs[i].prefabData.category = "Dungeon";
				}
				else if (prefabs[i].prefabData.category == "Dungeon")
				{
					//do not rename Dungeon
				}				
				else if (prefabs[i].prefabData.id == 858853278)
				{
					//preserve shopkeeper tags
					shopkeeper = prefabs[i].prefabData.category.Split(':').Last();
					prefabs[i].prefabData.category = name + shopkeeper;
				}
				else
				{					
					prefabs[i].prefabData.category = name;
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