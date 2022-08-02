﻿using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using static TerrainManager;

[SelectionBase, DisallowMultipleComponent]
public class PrefabDataHolder : MonoBehaviour
{
    public WorldSerialization.PrefabData prefabData;

    public void Setup()
    {
        for (int i = 0; i < GetComponents<Component>().Length; i++)
            ComponentUtility.MoveComponentUp(this);
    }

    public void UpdatePrefabData()
    {
        prefabData.position = gameObject.transform.localPosition;
        prefabData.rotation = transform.rotation;
        prefabData.scale = transform.localScale;
    }
	
	public void EnableColliders()
	{
			Collider[] colliders = gameObject.GetComponentsInChildren(typeof(Collider), true) as Collider[];
			if (colliders!= null)
			{
				foreach (MeshCollider collider in colliders)
				{
					collider.enabled = true;
					collider.convex = true;
				}
			}
	}
	
	public void DisableColliders()
	{
			Collider[] colliders = gameObject.GetComponentsInChildren(typeof(Collider), true) as Collider[];
			
			if (colliders!= null)
			{				
				foreach (MeshCollider collider in colliders)
				{
					collider.enabled = false;
					collider.convex = false;
				}
			}
	}
	
	public void AlwaysBreakPrefabs()
    {
		
        prefabData.position = gameObject.transform.position - PrefabManager.PrefabParent.position;
        prefabData.rotation = transform.rotation;
        prefabData.scale = transform.localScale;
    }
	
	public void CastPrefabData()
	{
		gameObject.transform.localPosition = prefabData.position;
		transform.rotation = prefabData.rotation;
		transform.localScale = prefabData.scale;
	}
     
    public void SnapToGround()
    {
        Vector3 newPos = transform.position;
        Undo.RecordObject(transform, "Snap to Ground");
        newPos.y = Land.SampleHeight(transform.position);
        transform.position = newPos;
    }

    public void ToggleLights()
    {
        foreach (var item in gameObject.GetComponentsInChildren<Light>(true))
            item.enabled = !item.enabled;
    }
}