using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Utj.Film
{
    public class LoadRecorderSettings : MonoBehaviour
    {
        public Object preset;
        // Start is called before the first frame update
        [PostProcessScene]
        void OnEnable()
        {
            // RecorderControllerSettingsExtensions.OnOpenAsset(preset.GetInstanceID(), 0);
        }
    }
}