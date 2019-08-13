using UnityEngine;


namespace Utj.Film
{
    public class SelectionGroupMember : MonoBehaviour
    {
        public Light[] lights;

        void OnWillRenderObject()
        {
            SelectionGroups.Instance.DisableLightGroups();
            foreach (var i in lights)
            {
                i.enabled = true;
            }
        }

    }

}