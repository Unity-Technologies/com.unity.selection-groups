using UnityEngine;

namespace Unity.SelectionGroups
{
    public partial class SelectionGroup
    {
        [SerializeField] private PersistentReferenceCollection _persistentReferenceCollection;

        private PersistentReferenceCollection PersistentReferenceCollection
        {
            get
            {
                if (_persistentReferenceCollection == null)
                {
                    _persistentReferenceCollection = new PersistentReferenceCollection();
                    _persistentReferenceCollection.LoadObjects();
                }

                return _persistentReferenceCollection;
            }
        }
    }
}