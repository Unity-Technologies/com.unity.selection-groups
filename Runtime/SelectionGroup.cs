using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    public class SelectionGroup : MonoBehaviour
    {
        public int groupId;
        public Color color;
        public string query;
        public List<UnityEngine.Object> members;

        GoQL.ParseResult parseResult;
        List<object> code;
        GoQL.GoQLExecutor executor;

        void OnEnable()
        {
            executor = new GoQL.GoQLExecutor();
            executor.Code = query;
        }

        /// <summary>
        /// Run the GoQL query attached to this group, adding any new members that are discovered.
        /// </summary>
        public void RefreshQueryResults()
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
                    foreach (var component in go.GetComponents<T>()) {
                        yield return component;
                    }
                }
            }
        }
    }
}
