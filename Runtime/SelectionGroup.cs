using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    public class SelectionGroup : MonoBehaviour
    {
        public int groupId;
        public Color color;
        public string query;
        public List<Object> members;

        GoQL.ParseResult parseResult;
        List<object> code;
        GoQL.GoQLExecutor executor;

        void OnEnable()
        {
            executor = new GoQL.GoQLExecutor();
            executor.Code = query;
        }

        public void RefreshQueryResults()
        {
            executor.Code = query;
            members.Clear();
            members.AddRange(executor.Execute());
        }

    }
}
