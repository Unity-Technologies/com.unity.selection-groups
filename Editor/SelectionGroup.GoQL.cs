namespace Unity.SelectionGroupsEditor
{
    internal partial class SelectionGroup
    {
        private GoQL.GoQLExecutor executor = new GoQL.GoQLExecutor();

        public void RefreshQueryResults()
        {
            if (string.IsNullOrEmpty(query)) return;
            executor.Code = query;
            var objects = executor.Execute();
            PersistentReferenceCollection.Update(objects);
        }
    }
}