using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Utj.Film
{

    internal static class SelectionGroupUtility
    {
        // public IEnumerable<T> GetComponents<T>() where T : Component
        // {
        //     foreach (var i in objects)
        //     {
        //         if (i is T) yield return (T)i;
        //         if (i is GameObject)
        //         {
        //             foreach (var j in ((GameObject)i).GetComponents<T>())
        //                 yield return j;
        //         }
        //     }
        // }

        internal static void UpdateUsageFlags(SerializedProperty property)
        {
            var isLightGroup = false;
            foreach (var g in IterateArrayProperty(property.FindPropertyRelative("objects"), property.FindPropertyRelative("queryResults")))
            {
                if (g.objectReferenceValue is Light) isLightGroup = true;
                if (g.objectReferenceValue is GameObject)
                {
                    if (((GameObject)g.objectReferenceValue).GetComponents<Light>().Length > 0)
                        isLightGroup = true;
                }
            }
            property.FindPropertyRelative("isLightGroup").boolValue = isLightGroup;
            property.serializedObject.ApplyModifiedProperties();
        }

        internal static IEnumerable<SerializedProperty> IterateArrayProperty(params SerializedProperty[] properties)
        {
            foreach (var property in properties)
            {
                var count = property.arraySize;
                for (var i = 0; i < count; i++)
                    yield return property.GetArrayElementAtIndex(i);
            }
        }

        internal static void PackArrayProperty(SerializedProperty arrayProperty, IEnumerable<Object> objects)
        {
            arrayProperty.ClearArray();
            foreach (var g in objects)
            {
                arrayProperty.InsertArrayElementAtIndex(0);
                arrayProperty.GetArrayElementAtIndex(0).objectReferenceValue = g;
            }
        }

        internal static void UnpackArrayProperty(SerializedProperty arrayProperty, ICollection<Object> objects)
        {
            var count = arrayProperty.arraySize;
            for (int i = 0; i < count; i++)
            {
                objects.Add(arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue);
            }
        }

        internal static void UpdateQueryResults(SerializedProperty property)
        {
            var transforms = GameObject.FindObjectsOfType<Transform>();
            var queryProperty = property.FindPropertyRelative("selectionQuery");
            var nameQuery = queryProperty.FindPropertyRelative("nameQuery").stringValue;
            var requiredTypes = (from i in IterateArrayProperty(queryProperty.FindPropertyRelative("requiredTypes")) select i.stringValue).ToArray();
            var requiredMaterials = (from i in IterateArrayProperty(queryProperty.FindPropertyRelative("requiredMaterials")) select (Material)i.objectReferenceValue).ToArray();
            var requiredShaders = (from i in IterateArrayProperty(queryProperty.FindPropertyRelative("requiredShaders")) select (Shader)i.objectReferenceValue).ToArray();
            var queryResults = property.FindPropertyRelative("queryResults");
            queryResults.ClearArray();
            if (queryProperty.FindPropertyRelative("enabled").boolValue)
                foreach (var i in transforms)
                {
                    if (!string.IsNullOrEmpty(nameQuery))
                    {
                        if (!i.name.Contains(nameQuery)) continue;
                    }
                    if (requiredTypes.Length > 0)
                    {
                        var missingComponents = false;
                        foreach (var c in requiredTypes)
                        {
                            var component = i.GetComponent(c);
                            if (component == null)
                            {
                                missingComponents = true;
                                break;
                            }
                        }
                        if (missingComponents) continue;
                    }
                    if (requiredMaterials.Length > 0)
                    {
                        var renderer = i.GetComponent<Renderer>();
                        if (renderer == null) continue;
                        var missingMaterials = false;
                        foreach (var m in requiredMaterials)
                        {
                            if (System.Array.IndexOf(renderer.sharedMaterials, m) == -1)
                            {
                                missingMaterials = true;
                                break;
                            }
                        }
                        if (missingMaterials) continue;
                    }
                    if (requiredShaders.Length > 0)
                    {
                        var renderer = i.GetComponent<Renderer>();
                        if (renderer == null) continue;
                        var shaders = (from m in renderer.sharedMaterials select m.shader).ToArray();
                        var missingShaders = false;
                        foreach (var s in requiredShaders)
                        {
                            if (System.Array.IndexOf(shaders, s) == -1)
                            {
                                missingShaders = true;
                                break;
                            }
                        }
                        if (missingShaders) continue;
                    }
                    queryResults.InsertArrayElementAtIndex(0);
                    queryResults.GetArrayElementAtIndex(0).objectReferenceValue = i.gameObject;
                }
            property.serializedObject.ApplyModifiedProperties();
        }

        internal static void AddObjects(SerializedProperty property, IList<Object> objects)
        {
            var hash = new HashSet<Object>();
            var objectsProperty = property.FindPropertyRelative("objects");
            UnpackArrayProperty(objectsProperty, hash);
            hash.UnionWith(objects);
            PackArrayProperty(objectsProperty, hash);
            property.serializedObject.ApplyModifiedProperties();
        }

        internal static void RemoveObjects(SerializedProperty property, IList<Object> objects)
        {
            var hash = new HashSet<Object>();
            var objectsProperty = property.FindPropertyRelative("objects");
            UnpackArrayProperty(objectsProperty, hash);
            hash.IntersectWith(objects);
            PackArrayProperty(objectsProperty, hash);
            property.serializedObject.ApplyModifiedProperties();
        }

        internal static void ClearObjects(SerializedProperty property)
        {
            var objectsProperty = property.FindPropertyRelative("objects");
            objectsProperty.ClearArray();
            property.serializedObject.ApplyModifiedProperties();
        }

        internal static UnityEngine.Object[] FetchObjects(SerializedProperty property)
        {
            var list = new List<Object>();
            UnpackArrayProperty(property.FindPropertyRelative("objects"), list);
            if (property.FindPropertyRelative("selectionQuery").FindPropertyRelative("enabled").boolValue)
                UnpackArrayProperty(property.FindPropertyRelative("queryResults"), list);
            return list.ToArray();
        }

    }
}
