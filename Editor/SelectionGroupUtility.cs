using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Utj.Film
{

    internal static class SelectionGroupUtility
    {

        internal static void UpdateInternalState(SerializedProperty property)
        {
            var isLightGroup = false;
            foreach (var i in IterateArrayProperty(property.FindPropertyRelative("objects"), property.FindPropertyRelative("queryResults")))
            {
                Light light;
                var g = (GameObject)i.objectReferenceValue;
                if (g != null && g.TryGetComponent<Light>(out light))
                    isLightGroup = true;
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

        static void PackArrayProperty(SerializedProperty arrayProperty, IEnumerable<Object> objects)
        {
            arrayProperty.ClearArray();
            foreach (var g in objects)
            {
                if (g != null)
                {
                    arrayProperty.InsertArrayElementAtIndex(0);
                    arrayProperty.GetArrayElementAtIndex(0).objectReferenceValue = g;
                }
            }
        }

        static void UnpackArrayProperty(SerializedProperty arrayProperty, ICollection<Object> objects)
        {
            var count = arrayProperty.arraySize;
            for (int i = 0; i < count; i++)
            {
                var g = arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                if (g != null)
                    objects.Add(g);
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
                        Renderer renderer;
                        if (!i.TryGetComponent<Renderer>(out renderer))
                            continue;
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
                        Renderer renderer;
                        if (!i.TryGetComponent<Renderer>(out renderer))
                            continue;
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
            UpdateInternalState(property);

        }

        internal static void RemoveObjects(SerializedProperty property, IList<Object> objects)
        {
            var hash = new HashSet<Object>();
            var objectsProperty = property.FindPropertyRelative("objects");
            UnpackArrayProperty(objectsProperty, hash);
            hash.ExceptWith(objects);
            PackArrayProperty(objectsProperty, hash);
            property.serializedObject.ApplyModifiedProperties();
            UpdateInternalState(property);
        }

        internal static void ClearObjects(SerializedProperty property)
        {
            var objectsProperty = property.FindPropertyRelative("objects");
            objectsProperty.ClearArray();
            property.FindPropertyRelative("selectionQuery").FindPropertyRelative("enabled").boolValue = false;
            property.serializedObject.ApplyModifiedProperties();
            UpdateInternalState(property);
        }

        internal static UnityEngine.Object[] FetchObjects(SerializedProperty property)
        {
            var list = new List<Object>();
            UnpackArrayProperty(property.FindPropertyRelative("objects"), list);
            if (property.FindPropertyRelative("selectionQuery").FindPropertyRelative("enabled").boolValue)
                UnpackArrayProperty(property.FindPropertyRelative("queryResults"), list);
            return list.ToArray();
        }

        internal static IEnumerable<SerializedProperty> FetchObjectProperties(SerializedProperty property)
        {
            return IterateArrayProperty(property.FindPropertyRelative("objects"));
        }

    }
}
