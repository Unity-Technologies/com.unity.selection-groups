// (C) UTJ
using UnityEngine;
using System.Collections.Generic;
using Compositor = Utj.Film.Compositor;
using Compositor;
using Compositor.Util;

namespace Compositor
{
    public class ObjectIdRendererTable
    {
        CsvTable csvTable = new CsvTable();
        Dictionary<string, Color> prefabNameToColor = new Dictionary<string, Color>();

        public bool Load(string csvFilename)
        {
            csvTable.Load(csvFilename);
            for (int iLine = 0; iLine < csvTable.Height; ++iLine)
            {
                var rgb = csvTable.GetRgb(iLine, "*ColorR", "*ColorG", "*ColorB", Color.black);
                var prefabName = csvTable.GetString(iLine, "*PrefabName");
                prefabNameToColor[prefabName] = rgb;
            }
            return true;
        }

        public Color GetColorByPrefabName(string prefabNameOrWildcard, Color defaultColor = default(Color))
        {
            int iLine = csvTable.FindLineByWildcardColumn("*PrefabName", prefabNameOrWildcard);
            if (iLine >= 0)
            {
                Color c;
                var prefabName = csvTable.GetString(iLine, "*PrefabName");
                if (prefabNameToColor.TryGetValue(prefabName, out c))
                {
                    return c;
                }
            }
            return defaultColor;
        }
    }
}
