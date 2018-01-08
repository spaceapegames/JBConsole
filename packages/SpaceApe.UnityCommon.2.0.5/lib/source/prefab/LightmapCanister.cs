using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

public class LightmapCanister : MonoBehaviour
{
	/*
	 * WARNING WARNING WARNING WARNING WARNING WARNING
	 * WARNING WARNING WARNING WARNING WARNING WARNING
	 * WARNING WARNING WARNING WARNING WARNING WARNING
	 * 
	 * If lightmaps are not working on device, but it is working on editor...
	 * 1. Go to Edit > Project Settings > Graphics
	 * 2. Set Lightmap Modes to manual
	 * 3. Tick all Baked *
	 * 
	 * WARNING WARNING WARNING WARNING WARNING WARNING
	 * WARNING WARNING WARNING WARNING WARNING WARNING
	 * WARNING WARNING WARNING WARNING WARNING WARNING
	 */
	static LightmapCanister current;
	
	[Serializable]
	internal class LightmapCanisterData
	{
		[SerializeField]
		internal Texture2D lightmapFar;
		[SerializeField]
		internal Texture2D lightmapNear;
	}
	
	[Serializable]
	internal class MeshLightmapData
	{
		[SerializeField]
		internal Renderer renderer;
		[SerializeField]
		internal string rendererPath;
		[SerializeField]
		internal int lightmapIndex;
		[SerializeField]
		internal Vector4 lightmapScaleOffset;
		[SerializeField]
		internal float scaleInLightmap; // TODO: This should go into an Editor asset, as the game doesn't care about it.
	}
	
	[Header("<<<SETTINGS>>>")]
	[SerializeField]
	[Tooltip("Adds to list of lightmaps instead of replacing. If you plan to add an remove a lot within one scene, consider another option.")]
	bool additive = true;

	[SerializeField]
	[Tooltip("Stores path to all renderers in string format (slower, but you can switch out stuff?))")]
	bool useObjectPath;

	[SerializeField]
	bool applyAmbient;

	[Header("<<<INTERNAL DO NOT EDIT>>>")]
	
	[SerializeField]
	LightmapsMode lightmapsMode;
	
	[SerializeField]
	LightProbes lightProbes;
	
	[SerializeField]
	MeshLightmapData[] meshLightmapData;

	[SerializeField]
	AmbientMode ambientMode = AmbientMode.Flat;
	[SerializeField]
	Color ambientLight;
	[SerializeField]
	Color ambientEquatorColour;
	[SerializeField]
	Color ambientSkyColour;
	[SerializeField]
	Color ambientGroundColour;
	[SerializeField]
	float ambientIntensity;
	[SerializeField]
	LightmapCanisterData[] lightmaps;

	int lightmapStartIndex = -1;

	void OnEnable()
	{
		Deploy();
	}
	
	void OnDisable()
	{
		if (current == this && !additive)
		{
			LightmapSettings.lightmaps = null;
			current = null;
		}
	}
	
	void OnDestroy()
	{
		if(additive && lightmapStartIndex >= 0)
		{
			var sceneLightmaps = LightmapSettings.lightmaps;
			var end = lightmapStartIndex + lightmaps.Length;
			if(end <= sceneLightmaps.Length)
			{
				for(var i = lightmapStartIndex; i < end; i++)
				{
					var lm = sceneLightmaps[i];
#if UNITY_5_6_OR_NEWER
					lm.lightmapColor = null;
#elif UNITY_5_4_OR_NEWER
					lm.lightmapFar = null;
					lm.lightmapNear = null;
#else
# error Undefined Unity version
#endif
				}
				LightmapSettings.lightmaps = sceneLightmaps;
			}
		}
		if (current == this)
		{
			LightmapSettings.lightmaps = null;
			current = null;
		}
	}
	
	public void Deploy()
	{
		if(lightmapStartIndex >= 0)
		{
			return; // already additively applied;
		}

		if (current != null && current.gameObject != null)
		{
			if(!additive || !current.additive)
			{
				Logger.Error(UnityErrorCodes.LightmapCanister, "Detected multiple lightmap canisters in the same scene: " + current + " and " + this + " perhaps turn on 'additive' mode?");
			}
			else if(additive && lightmapsMode != current.lightmapsMode)
			{
				Logger.Error(UnityErrorCodes.LightmapCanister, "Conflicting lightmap modes with existing LightmapCanister: " + lightmapsMode + " vs " + current.lightmapsMode + ".");
			}
		}
		current = this;
		DeployLightmaps ();
		if(applyAmbient)
		{
			DeployAmbient();
		}
	}
	
	void DeployLightmaps()
	{
		var indexOffset = 0;

#if UNITY_5_6_OR_NEWER
		var newLightmaps = Array.ConvertAll (lightmaps, l => new LightmapData () { lightmapColor = l.lightmapNear });
#elif UNITY_5_4_OR_NEWER
		var newLightmaps = Array.ConvertAll (lightmaps, l => new LightmapData () { lightmapFar = l.lightmapFar, lightmapNear = l.lightmapNear });
#else
# error Undefined Unity version
#endif

		if(additive)
		{
			var existing = LightmapSettings.lightmaps;
			indexOffset = existing.Length;
			var combinedLightmaps = new LightmapData[existing.Length + newLightmaps.Length];
			existing.CopyTo(combinedLightmaps, 0);
			newLightmaps.CopyTo (combinedLightmaps, indexOffset);
			
			LightmapSettings.lightmaps = combinedLightmaps;
			LightmapSettings.lightmapsMode = lightmapsMode;

			if(lightProbes != null)
			{
				LightmapSettings.lightProbes = lightProbes;
			}
			lightmapStartIndex = indexOffset;
		}
		else
		{
			LightmapSettings.lightmaps = newLightmaps;
			LightmapSettings.lightmapsMode = lightmapsMode;
			LightmapSettings.lightProbes = lightProbes;
		}

		if (meshLightmapData != null)
		{
			Transform rtran;
			Renderer r;
			
			foreach (var data in meshLightmapData)
			{
				r = data.renderer;
				if (r || ((rtran = transform.Find(data.rendererPath)) != null && (r = rtran.GetComponent<Renderer>()) != null))
				{
					r.lightmapIndex = data.lightmapIndex + indexOffset;
					r.lightmapScaleOffset = data.lightmapScaleOffset;
					
					
					#if UNITY_EDITOR
					UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(r);
					var prop = so.FindProperty("m_ScaleInLightmap");
					if (prop != null)
					{
						prop.floatValue = data.scaleInLightmap;
					}
					so.ApplyModifiedProperties();
					#endif
				}
			}
		}
	}

	void DeployAmbient()
	{
		RenderSettings.ambientMode = ambientMode;
		RenderSettings.ambientLight = ambientLight;
		RenderSettings.ambientIntensity = ambientIntensity;
		RenderSettings.ambientEquatorColor = ambientEquatorColour;
		RenderSettings.ambientGroundColor = ambientGroundColour;
		RenderSettings.ambientSkyColor = ambientSkyColour;
	}

	public void Capture()
	{
		#if UNITY_EDITOR
		ambientLight = RenderSettings.ambientLight;
		ambientIntensity = RenderSettings.ambientIntensity;
		ambientEquatorColour = RenderSettings.ambientEquatorColor;
		ambientGroundColour = RenderSettings.ambientGroundColor;
		ambientSkyColour = RenderSettings.ambientSkyColor;

		lightmaps = Array.ConvertAll(LightmapSettings.lightmaps, l => new LightmapCanisterData() { lightmapFar = l.lightmapFar, lightmapNear = l.lightmapNear });
		lightmapsMode = LightmapSettings.lightmapsMode;
		lightProbes = LightmapSettings.lightProbes;

		if (RenderSettings.ambientMode != AmbientMode.Skybox)
		{
			ambientMode = RenderSettings.ambientMode;
		}
		
		meshLightmapData = GetComponentsInChildren<MeshRenderer>()
			.Where(r => r.lightmapIndex < 0xFFFE && r.lightmapIndex >= 0)
				.Select(r => {
					var data = new MeshLightmapData()
					{ 
						lightmapIndex = r.lightmapIndex,
						lightmapScaleOffset = r.lightmapScaleOffset
					};
					if(useObjectPath)
					{
						data.rendererPath = GetGameObjectPath(r.gameObject, gameObject);
					}
					else
					{
						data.renderer = r;
					}

					UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(r);
					var prop = so.FindProperty("m_ScaleInLightmap");
					if (prop != null)
					{
						data.scaleInLightmap = prop.floatValue;
					}

					return data;
				}).ToArray();
		#endif
	}
	
	#if UNITY_EDITOR
	private static string GetGameObjectPath(GameObject obj, GameObject root)
	{
		string path = "";
		do
		{
			path = "/" + obj.name + path;
			obj = obj.transform.parent.gameObject;
		}
		while (obj != null && obj != root);
		return path.TrimStart('/');
	}
	#endif
}