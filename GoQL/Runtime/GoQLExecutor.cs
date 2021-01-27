using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.GoQL
{
    /// <summary>
    /// This class is responsible for taking GoQL source code and evaluating it.
    /// </summary>
    public partial class GoQLExecutor
    {
        readonly Stack<object> stack = new Stack<object>();
        readonly DoubleBuffer<GameObject> selection = new DoubleBuffer<GameObject>();
        readonly List<object> instructions = new List<object>();

        bool refresh = true;
        string code;

        /// <summary>
        /// The GoQL source code to be executed.
        /// </summary>
        /// <value></value>
        public string Code
        {
            get => code;
            set
            {
                if (value != code)
                    refresh = true;
                code = value;
            }
        }

        /// <summary>
        /// The result of parsing the Code property.
        /// </summary>
        public ParseResult parseResult;

        /// <summary>
        /// An error message for the user if the Code property could not be parsed.
        /// </summary>
        /// <value></value>
        public string Error { get; set; } = string.Empty;

        static Dictionary<string, Type> typeCache;
        static object typeCacheLock = new object();
        static Dictionary<GameObject, string> nameCache = new Dictionary<GameObject, string>();

        static string GetName(GameObject gameObject)
        {
            if (!nameCache.TryGetValue(gameObject, out string name))
                name = nameCache[gameObject] = gameObject.name;
            return name;
        }

        static Type FindType(string name)
        {
            lock (typeCacheLock)
            {
                if (typeCache == null)
                {
                    typeCache = new Dictionary<string, Type>();
                    var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var i in assemblies)
                    {
                        foreach (var j in i.GetTypes())
                        {
                            if (j.IsSubclassOf(typeof(Component)))
                            {
                                if (typeCache.ContainsKey(j.Name))
                                {
                                    typeCache.Add($"{j.Namespace}.{j.Name}", j);
                                    var existingType = typeCache[j.Name];
                                    typeCache.Add($"{existingType.Namespace}.{existingType.Name}", j);
                                }
                                else
                                {
                                    typeCache.Add(j.Name, j);
                                }
                            }
                        }
                    }
                }

                if (typeCache.TryGetValue(name, out Type type))
                    return type;
                return null;
            }
        }

        /// <summary>
        /// Execute the query and return the array of matching objects.
        /// </summary>
        /// <returns></returns>
        public GameObject[] Execute()
        {
            if (refresh)
            {
                instructions.Clear();
                GoQL.Parser.Parse(code, instructions, out parseResult);
            }
            
            stack.Clear();
            selection.Clear();
            Error = string.Empty;
            if (instructions.Count > 0)
            {
                var firstCode = instructions[0];
                if (firstCode is GoQLCode && ((GoQLCode) firstCode) == GoQLCode.EnterChildren)
                {
                    CollectRootObjects();
                }
                else
                {
                    CollectAllObjects();
                }

                foreach (var i in instructions)
                {
                    if (i is GoQLCode)
                    {
                        ExecuteCode((GoQLCode) i);
                    }
                    else
                    {
                        stack.Push(i);
                    }
                }
            }
            refresh = false;

            return selection.ToArray();
        }

        void ExecuteCode(GoQLCode i)
        {
            switch (i)
            {
                case GoQLCode.EnterChildren:
                    EnterChildren();
                    break;
                case GoQLCode.FilterByDiscriminators:
                    FilterByDiscriminators();
                    break;
                case GoQLCode.FilterIndex:
                    FilterIndex();
                    break;
                case GoQLCode.FilterName:
                    FilterName();
                    break;
                case GoQLCode.CollectAllAncestors:
                    CollectAllAncestors();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(i), i, null);
            }
        }

        private void CollectAllAncestors()
        {
            foreach (var i in selection)
            {
                foreach(var j in i.GetComponentsInChildren<Transform>()) 
                    selection.Add(j.gameObject);
            }
            selection.Swap();
        }

        void CollectAllObjects()
        {
            //populate with all gameobjects ready for filtering.
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                foreach (var j in scene.GetRootGameObjects())
                {
                    _CollectAllGameObjects(j);
                }
            }

            selection.Swap();
        }

        void _CollectAllGameObjects(GameObject gameObject)
        {
            selection.Add(gameObject);
            foreach (Transform transform in gameObject.transform)
            {
                _CollectAllGameObjects(transform.gameObject);
            }
        }

        void FilterName()
        {
            var q = stack.Pop().ToString();
            var isWildCardOnly = q == "*";
            if (isWildCardOnly)
            {
                //TODO: can this be reduced to a nop?
                foreach (var i in selection)
                    selection.Add(i);
                selection.Swap();
            }
            else
            {
                var isWildCardFirst = q.First() == '*';
                var isWildCardLast = q.Last() == '*';
                if (isWildCardFirst)
                    q = q.Substring(1);
                if (isWildCardLast)
                    q = q.Substring(0, q.Length - 1);

                foreach (var i in selection)
                {
                    if (IsNameMatch(GetName(i), q, isWildCardFirst, isWildCardLast))
                        selection.Add(i);
                }

                selection.Swap();
            }
        }

        bool IsNameMatch(string name, string q, bool isWildCardFirst, bool isWildCardLast)
        {
            if (isWildCardFirst && isWildCardLast)
                return name.Contains(q);
            if (isWildCardFirst)
                return name.EndsWith(q);
            if (isWildCardLast)
                return name.StartsWith(q);
            return name == q;
        }

        void FilterIndex()
        {
            if (selection.Count > 0)
            {
                var argCount = (int) stack.Pop();
                var indices = new int[selection.Count];
                var lengths = new int[selection.Count];
                for (var i = 0; i < selection.Count; i++)
                {
                    indices[i] = selection[i].transform.GetSiblingIndex();
                    lengths[i] = selection[i].transform.parent == null ? 1 : selection[i].transform.parent.childCount;
                }

                for (var i = 0; i < argCount; i++)
                {
                    var arg = stack.Pop();
                    if (arg is int)
                    {
                        for (var j = 0; j < selection.Count; j++)
                        {
                            var index = mod(((int) arg), lengths[i]);
                            if (index == indices[j])
                            {
                                selection.Add(selection[j]);
                            }
                        }
                    }
                    else if (arg is Range)
                    {
                        var range = (Range) arg;
                        for (var index = range.start; index < range.end; index++)
                        {
                            for (var j = 0; j < selection.Count; j++)
                            {
                                if (mod(index, lengths[j]) == indices[j])
                                {
                                    selection.Add(selection[j]);
                                }
                            }
                        }
                    }
                }

                selection.Swap();
                selection.Reverse();
            }
        }

        void FilterByDiscriminators()
        {
            var argCount = (int) stack.Pop();
            for (var i = 0; i < argCount; i++)
            {
                var arg = stack.Pop();
                if (arg is Discrimator)
                {
                    var discriminator = (Discrimator) arg;
                    PerformDiscrimination(discriminator);
                }
            }
        }

        void PerformDiscrimination(Discrimator discriminator)
        {
            var discriminatorType = discriminator.type;
            var value = discriminator.value;
            switch (discriminatorType)
            {
                case "t":
                    PerformTypeDiscrimination(value);
                    break;
                case "m":
                    PerformMaterialDiscrimination(value);
                    break;
                case "s":
                    PerformShaderDiscrimination(value);
                    break;
                default:
                    Error = $"Unknown discrimator type: {discriminatorType}";
                    break;
            }
        }

        System.Func<string, string, bool> MatchName(string discriminator)
        {
            if (discriminator.EndsWith("*") && discriminator.StartsWith("*"))
                return (A, B) => A.ToLower().Contains(B.ToLower().Substring(1, B.Length - 2));
            else if (discriminator.EndsWith("*"))
                return (A, B) => A.ToLower().StartsWith(B.ToLower().Substring(0, B.Length - 1));
            else if (discriminator.StartsWith("*"))
                return (A, B) => A.ToLower().EndsWith(B.ToLower().Substring(1, B.Length - 1));
            return (A, B) => A.ToLower() == B.ToLower();
        }

        void PerformMaterialDiscrimination(string discriminator)
        {
            var matcher = MatchName(discriminator);
            foreach (var g in selection)
            {
                if (g.TryGetComponent<Renderer>(out Renderer component))
                {
                    foreach (var m in component.sharedMaterials)
                    {
                        if (m != null && matcher(m.name, discriminator))
                            selection.Add(g);
                    }
                }
            }
            selection.Swap();
        }

        void PerformShaderDiscrimination(string discriminator)
        {
            var matcher = MatchName(discriminator);
            foreach (var g in selection)
            {
                if (g.TryGetComponent<Renderer>(out Renderer component))
                {
                    foreach (var m in component.sharedMaterials)
                    {
                        if (m != null && matcher(m.name, discriminator))
                            selection.Add(g);
                    }
                }
            }

            selection.Swap();
        }

        void PerformTypeDiscrimination(string value)
        {
            var type = FindType(value);
            if (type != null)
            {
                foreach (var g in selection)
                {
                    if (g.TryGetComponent(type, out Component component))
                    {
                        selection.Add(g);
                    }
                }

                selection.Swap();
            }
            else
            {
                Error = $"Cannot load type {value}";
            }
        }

        void CollectRootObjects()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if(scene.isLoaded)
                    selection.AddRange(scene.GetRootGameObjects());
            }
        }

        void EnterChildren()
        {
            foreach (var i in selection)
            {
                for (var j = 0; j < i.transform.childCount; j++)
                    selection.Add(i.transform.GetChild(j).gameObject);
            }

            selection.Swap();
        }

        int mod(int a, int b) => a - b * Mathf.FloorToInt(1f * a / b);
    }
}