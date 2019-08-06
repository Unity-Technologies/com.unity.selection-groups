// (C) UTJ
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Compositor = Utj.Film.Compositor;
using Compositor;
using Compositor.Util;

namespace Utj.Film.Compositor {

[DefaultExecutionOrder(Config.ExecutionOrder.Default)]
class ObjectIdRenderer : PostEffectBase
{
	public Camera masterCamera = null;

	[EditorFilename(ExpectedFileExtension = ".csv", AllowOutOfProjectPath = true)]
	public string csvFilename;

	const string ReplacementShaderName = "Utj/ObjectIdRenderer";
	public Color backgroundColor = new Color(0f, 0f, 0f, 0f);
	public Color defaultColor = new Color(0.5f, 0.5f, 0.5f, 0f);
	public List<string> pseudoPrefabRootNames = new List<string>();
	List<Regex> pseudoPrefabRootNameRegexes;

	[System.Serializable]
	public class GameObjectAndColorPair {
		public GameObject	gameObject = null;
		public Color		color = Color.black;
	}
	public List<GameObjectAndColorPair> gameObjectAndColorPairs = new List<GameObjectAndColorPair>();

	protected override void onPreStart() {
		MasterCamera = masterCamera;
	}

	protected override void onStart() {
		ThisCamera.clearFlags = CameraClearFlags.SolidColor;
		ThisCamera.backgroundColor = backgroundColor;
		ThisCamera.SetReplacementShader(Shader.Find(ReplacementShaderName), null);

		var table = new ObjectIdRendererTable();
		table.Load(csvFilename);

		pseudoPrefabRootNameRegexes = new List<Regex>();
		foreach(var wildcard in pseudoPrefabRootNames) {
			pseudoPrefabRootNameRegexes.Add(MakeWildcardRegex(wildcard));
		}

		var gameObjectNameToColor = new Dictionary<GameObject, Color>();
		foreach(var gocPair in gameObjectAndColorPairs) {
			if(gocPair.gameObject == null) {
				continue;
			}
			gameObjectNameToColor[gocPair.gameObject] = gocPair.color;
		}

		SceneObjectTraverser.TraverseAllScenesAndGameObjects((scene, go, depth) => {
			if(go == null) {
				return;
			}
			var prefab = FindPrefabRoot2(go);
			foreach(Renderer renderer in go.GetComponents<Renderer>()) {
				Color color;
				if(gameObjectNameToColor.TryGetValue(go, out color)) {
					//
				} else {
					if(prefab == null) {
//						if(gameObjectNameToColor.TryGetValue(go, out color)) {
						color = defaultColor;
//						}
					} else {
						if(gameObjectNameToColor.TryGetValue(prefab, out color)) {
							//
						} else if(gameObjectNameToColor.TryGetValue(PrefabUtility.GetPrefabParent(prefab) as GameObject, out color)) {
							//
						} else {
							color = table.GetColorByPrefabName(prefab.name, defaultColor);
						}
					}
				}
				var mpb = new MaterialPropertyBlock();
				renderer.GetPropertyBlock(mpb);
				mpb.SetColor("_IdColor", color);
				renderer.SetPropertyBlock(mpb);
			}
		});
/*
		{
			var movieRecorder = GetComponent<UTJ.FrameCapturer.MovieRecorder>();
			if(movieRecorder != null && movieRecorder.isActiveAndEnabled) {
				SetupRenderTextureAndMovieRecorder(ThisCamera, movieRecorder);
			}
		}
*/
		{
			var movieRecorder = GetComponent("MovieRecorder") as Behaviour;
//			Debug.LogFormat("movieRecorder = {0}", movieRecorder);
			if(movieRecorder != null && movieRecorder.isActiveAndEnabled) {
				SetupRenderTextureAndMovieRecorder(ThisCamera, movieRecorder);
			}
		}
	}

	GameObject FindPrefabRoot2(GameObject go) {
		if(go == null) {
			return null;
		}
		{
			GameObject root = FindPrefabRoot(go);
			if(root != null) {
				return root;
			}
		}

		foreach(var wildcardRegex in pseudoPrefabRootNameRegexes) {
			GameObject t = go;
			while(t != null) {
				if(wildcardRegex.IsMatch(t.name)) {
					return t;
				}
				var p = t.transform.parent;
				if(p == null) {
					break;
				}
				t = p.gameObject;
			}
		}

		return null;
	}

	static GameObject FindPrefabRoot(GameObject go) {
		if(go == null) {
			return null;
		}
		if(! IsPrefab(go)) {
			return null;
		}
		return UnityEditor.PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);
	}

	static bool IsPrefab(GameObject go) {
		switch(UnityEditor.PrefabUtility.GetPrefabType(go)) {
		case UnityEditor.PrefabType.None:
			return false;
		default:
			return true;
		}
	}

	static Regex MakeWildcardRegex(string wildcard) {
		return new Regex(
			"^" + Regex.Escape(wildcard).Replace("\\*", ".*").Replace("\\?", ".") + "$",
			RegexOptions.IgnoreCase | RegexOptions.Singleline
		);
	}
}
}
