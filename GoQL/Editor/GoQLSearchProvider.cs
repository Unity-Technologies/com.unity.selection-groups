// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using Unity.QuickSearch;
// using UnityEditor;
// using UnityEditor.Experimental.SceneManagement;
// using UnityEngine;

// namespace Unity.GoQL
// {
//     public static class GoQLSearchProvider
//     {
//         const string k_ProviderId = "goql";

//         static GoQLExecutor goqlMachine = goqlMachine = new GoQLExecutor();

//         [SearchItemProvider]
//         internal static SearchProvider CreateProvider()
//         {
//             return new SearchProvider(k_ProviderId, "GoQL")
//             {
//                 priority = 50,
//                 filterId = k_ProviderId+":",

//                 fetchItems = (context, items, provider) => SearchItems(context, provider),

//                 fetchLabel = (item, context) =>
//                 {
//                     var go = ObjectFromItem(item);
//                     return GetTransformPath(go.transform);
//                 },

//                 fetchDescription = (item, context) =>
//                 {
//                     var go = ObjectFromItem(item);
//                     return (item.description = GetHierarchyPath(go));
//                 },

//                 fetchThumbnail = (item, context) =>
//                 {
//                     var obj = ObjectFromItem(item);
//                     return (item.thumbnail = AssetPreview.GetMiniThumbnail(obj));
//                 },

//                 startDrag = (item, context) =>
//                 {
//                     var obj = ObjectFromItem(item);
//                     if (obj != null)
//                     {
//                         DragAndDrop.PrepareStartDrag();
//                         DragAndDrop.objectReferences = new[] { obj };
//                         DragAndDrop.StartDrag(item.label ?? obj.name);
//                     }
//                 },

//                 trackSelection = (item, context) => PingItem(item)
//             };
//         }

//         [SearchActionsProvider]
//         internal static IEnumerable<SearchAction> ActionHandlers()
//         {
//             return new [] { new SearchAction(k_ProviderId, "select") { handler = (item, context) => SelectObject(item) } };
//         }

//         private static void SelectObject(SearchItem item)
//         {
//             var pingedObject = PingItem(item) as GameObject;
//             if (!pingedObject)
//                 return;
//             Selection.activeGameObject = pingedObject;
//             if (SceneView.lastActiveSceneView != null)
//                 SceneView.lastActiveSceneView.FrameSelected();
//         }

//         private static IEnumerator SearchItems(SearchContext context, SearchProvider provider)
//         {
//             ParseResult parseResult;
//             Parser.Parse(context.searchQuery, out parseResult);
//             if (parseResult != ParseResult.OK)
//                 yield break;
//             goqlMachine.Code = context.searchQuery;
//             yield return goqlMachine.Execute().Select(go =>
//             {
//                 var item = provider.CreateItem(go.GetInstanceID().ToString());
//                 item.descriptionFormat = SearchItemDescriptionFormat.Ellipsis | 
//                     SearchItemDescriptionFormat.RightToLeft | 
//                     SearchItemDescriptionFormat.Highlight;
//                 return item;
//             });
//         }

//         private static GameObject PingItem(SearchItem item)
//         {
//             var obj = ObjectFromItem(item);
//             if (obj == null)
//                 return null;
//             EditorGUIUtility.PingObject(obj);
//             return obj;
//         }

//         private static GameObject ObjectFromItem(SearchItem item)
//         {
//             var instanceID = Convert.ToInt32(item.id);
//             return EditorUtility.InstanceIDToObject(instanceID) as GameObject;
//         }

//         private static string GetTransformPath(Transform tform)
//         {
//             if (tform.parent == null)
//                 return "/" + tform.name;
//             return GetTransformPath(tform.parent) + "/" + tform.name;
//         }

//         public static string GetHierarchyPath(GameObject gameObject)
//         {
//             if (gameObject == null)
//                 return null;

//             StringBuilder sb = new StringBuilder(200);
//             var sceneName = gameObject.scene.name;
//             if (sceneName == string.Empty)
//             {
//                 var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
//                 if (prefabStage != null)
//                     sceneName = "Prefab Stage";
//                 else
//                     sceneName = "Unsaved Scene";
//             }

//             sb.Append("<b>" + sceneName + "</b>");
//             sb.Append(GetTransformPath(gameObject.transform));

//             return sb.ToString();
//         }
//     }
// }
