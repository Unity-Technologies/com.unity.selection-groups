using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    /// <summary>
    /// This class is used to provide selection group information during play-mode. It reflects the information in the Editor-only class.
    /// </summary>
    public class SelectionGroup : MonoBehaviour
    {
        /// <summary>
        /// Unique ID of this group.
        /// </summary>
        public int groupId;
        /// <summary>
        /// A color assigned to this group.
        /// </summary>
        public Color color;
        /// <summary>
        /// If not empty, this is a GoQL query string used to create the set of matching objects for this group.
        /// </summary>
        public string query;
        /// <summary>
        /// The members of this group.
        /// </summary>
        public List<UnityEngine.Object> members;

        GoQL.ParseResult parseResult;
        List<object> code;
        GoQL.GoQLExecutor executor;

        /// <summary>
        /// An enumerator that matches only the GameObject members of this group.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GameObject> EnumerateGameObjects()
        {
            foreach (var i in members)
                if (i is GameObject)
                    yield return i as GameObject;
        }

        void OnEnable()
        {
            executor = new GoQL.GoQLExecutor();
            executor.Code = query;
        }

        /// <summary>
        /// Run the GoQL query attached to this group, adding any new members that are discovered.
        /// </summary>
        void RefreshQueryResults()
        {
            executor.Code = query;
            members.Clear();
            members.AddRange(executor.Execute());
        }

        /// <summary>
        /// Get components from all members of a group that are GameObjects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetMemberComponents<T>() where T : Component
        {
            foreach (var member in members)
            {
                var go = member as GameObject;
                if (go != null)
                {
                    foreach (var component in go.GetComponentsInChildren<T>())
                    {
                        yield return component;
                    }
                }
            }
        }
    }
}
