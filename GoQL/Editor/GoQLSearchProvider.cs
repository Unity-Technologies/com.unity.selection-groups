
#if AT_USE_QUICKSEARCH || UNITY_2021_2_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

#if AT_USE_QUICKSEARCH 
using Unity.QuickSearch;
#else //newer than 2021.2
using UnityEditor.Search;
#endif

namespace Unity.GoQL.Editor {

internal static class GoQLSearchProvider {

    [SearchItemProvider]
    internal static SearchProvider CreateProvider() {
        return new SearchProvider(PROVIDER_ID, "GoQL") {
            priority = 50,
            filterId = PROVIDER_ID + ":",

            fetchItems = (context, items, provider) => SearchItems(context, provider),

            fetchLabel = (item, context) => {
                GameObject go = ObjectFromItem(item);
                return GetTransformPath(go.transform);
            },

            fetchDescription = (item, context) => {
                GameObject go;
                go = ObjectFromItem(item);
                return (item.description = GetHierarchyPath(go));
            },

            fetchThumbnail = (item, context) => {
                GameObject obj = ObjectFromItem(item);
                return (item.thumbnail = AssetPreview.GetMiniThumbnail(obj));
            },

            startDrag = (item, context) => {
                GameObject obj = ObjectFromItem(item);
                if (obj == null) 
                    return;
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[] { obj };
                DragAndDrop.StartDrag(item.label ?? obj.name);
            },

            trackSelection = (item, context) => PingItem(item)
        };
    }

    [SearchActionsProvider]
    internal static IEnumerable<SearchAction> ActionHandlers() {
        return new[] { new SearchAction(PROVIDER_ID, "select") { handler = (SearchItem item) => SelectObject(item) } };
    }

    private static void SelectObject(SearchItem item) {
        GameObject pingedObject = PingItem(item) as GameObject;
        if (!pingedObject)
            return;
        Selection.activeGameObject = pingedObject;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();
    }

    private static IEnumerator SearchItems(SearchContext context, SearchProvider provider) {
        Parser.Parse(context.searchQuery, out ParseResult parseResult);
        if (parseResult != ParseResult.OK)
            yield break;
        m_goqlMachine.Code = context.searchQuery;

        GameObject[] objects = m_goqlMachine.Execute();
        foreach (GameObject go in objects) {
            SearchItem item = provider.CreateItem(go.GetInstanceID().ToString());
            item.options = SearchItemOptions.Ellipsis |
                SearchItemOptions.RightToLeft |
                SearchItemOptions.Highlight;
            yield return item;
        }
    }

    private static GameObject PingItem(SearchItem item) {
        GameObject obj = ObjectFromItem(item);
        if (obj == null)
            return null;
        EditorGUIUtility.PingObject(obj);
        return obj;
    }

    private static GameObject ObjectFromItem(SearchItem item) {
        int instanceID = Convert.ToInt32(item.id);
        return EditorUtility.InstanceIDToObject(instanceID) as GameObject;
    }

    private static string GetTransformPath(Transform tform) {
        if (tform.parent == null)
            return "/" + tform.name;
        return GetTransformPath(tform.parent) + "/" + tform.name;
    }

    private static string GetHierarchyPath(GameObject gameObject) {
        if (gameObject == null)
            return null;

        StringBuilder sb        = new StringBuilder(200);
        string           sceneName = gameObject.scene.name;
        if (sceneName == string.Empty) {            
            PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
            if (prefabStage != null)
                sceneName = "Prefab Stage";
            else
                sceneName = "Unsaved Scene";
        }

        sb.Append("<b>" + sceneName + "</b>");
        sb.Append(GetTransformPath(gameObject.transform));

        return sb.ToString();
    }

//----------------------------------------------------------------------------------------------------------------------
    
    const string PROVIDER_ID = "com.unity.goql";

    static readonly GoQLExecutor m_goqlMachine = new GoQLExecutor();
    
}

} //end namespace

#endif //AT_USE_QUICKSEARCH