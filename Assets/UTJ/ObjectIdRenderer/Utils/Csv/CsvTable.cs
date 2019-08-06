// (C) UTJ
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Compositor = Utj.Film.Compositor;
using Compositor;
using Compositor.Util;

namespace Compositor.Util
{
    public class CsvTable
    {
        List<Dictionary<int, string>> Lines;
        Dictionary<string, int> LabelToIndices;
        Dictionary<string, Regex> PatternCache;

        public void Clear()
        {
            Lines = new List<Dictionary<int, string>>();
            PatternCache = new Dictionary<string, Regex>();
            LabelToIndices = new Dictionary<string, int>();
        }

        public int Height
        {
            get
            {
                if (Lines == null)
                {
                    return 0;
                }
                return Lines.Count;
            }
        }

        public int LabelToColumnIndex(string label)
        {
            int index = -1;
            if (!LabelToIndices.TryGetValue(label, out index))
            {
                return -1;
            }
            return index;
        }

        public int FindLineByWildcardColumn(string label, string str, int defaultLine = -1)
        {
            return FindLineByWildcardColumn(LabelToColumnIndex(label), str, defaultLine);
        }

        public int FindLineByWildcardColumn(int wildcardColumnIndex, string str, int defaultLine = -1)
        {
            if (wildcardColumnIndex >= 0)
            {
                int nLine = Height;
                for (int iLine = 0; iLine < nLine; ++iLine)
                {
                    if (IsWildcardMatch(iLine, wildcardColumnIndex, str))
                    {
                        return iLine;
                    }
                }
            }
            return defaultLine;
        }

        public bool IsWildcardMatch(int lineNumber, string label, string str)
        {
            return IsWildcardMatch(lineNumber, LabelToColumnIndex(label), str);
        }

        public bool IsWildcardMatch(int lineNumber, int columnIndex, string str)
        {
            if (columnIndex < 0)
            {
                return false;
            }
            var pattern = Get(lineNumber, columnIndex);
            if (String.IsNullOrEmpty(pattern))
            {
                return false;
            }

            Regex regex;
            if (!PatternCache.TryGetValue(pattern, out regex))
            {
                regex = new Regex(
                    "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline
                );
            }
            return regex.IsMatch(str);
        }

        public string Get(int lineNumber, string label, string defaultValue = "")
        {
            return Get(lineNumber, LabelToColumnIndex(label), defaultValue);
        }

        public string Get(int lineNumber, int columnIndex, string defaultValue = "")
        {
            return GetString(lineNumber, columnIndex, defaultValue);
        }

        public string GetString(int lineNumber, string label, string defaultValue = "")
        {
            return GetString(lineNumber, LabelToColumnIndex(label), defaultValue);
        }

        public string GetString(int lineNumber, int columnIndex, string defaultValue = "")
        {
            if (Lines == null || lineNumber < 0 || lineNumber >= Lines.Count || columnIndex < 0)
            {
                return defaultValue;
            }
            var line = Lines[lineNumber];
            string result;
            if (!line.TryGetValue(columnIndex, out result))
            {
                return defaultValue;
            }
            return result;
        }

        public int GetInt(int lineNumber, string label, int defaultValue = 0)
        {
            return GetInt(lineNumber, LabelToColumnIndex(label), defaultValue);
        }

        public int GetInt(int lineNumber, int columnIndex, int defaultValue = 0)
        {
            var e = GetString(lineNumber, columnIndex);
            int v = int.TryParse(e, out v) ? v : defaultValue;
            return v;
        }

        public float GetFloat(int lineNumber, string label, float defaultValue = 0.0f)
        {
            return GetFloat(lineNumber, LabelToColumnIndex(label), defaultValue);
        }

        public float GetFloat(int lineNumber, int columnIndex, float defaultValue = 0.0f)
        {
            var e = GetString(lineNumber, columnIndex);
            float v = float.TryParse(e, out v) ? v : defaultValue;
            return v;
        }

        public Color GetRgb(int lineNumber, string labelR, string labelG, string labelB, Color defaultValue = default(Color))
        {
            return GetRgb(lineNumber, LabelToColumnIndex(labelR), LabelToColumnIndex(labelG), LabelToColumnIndex(labelB), defaultValue);
        }

        public Color GetRgb(int lineNumber, int columnIndexR, int columnIndexG, int columnIndexB, Color defaultValue = default(Color))
        {
            var r = GetFloat(lineNumber, columnIndexR, defaultValue.r);
            var g = GetFloat(lineNumber, columnIndexG, defaultValue.g);
            var b = GetFloat(lineNumber, columnIndexB, defaultValue.b);
            var a = defaultValue.a;
            return new Color(r, g, b, a);
        }

        public bool Load(string csvFilename)
        {
            Clear();

            int iLine = 0;
            CsvReader.Read(csvFilename, (values) =>
            {
                if (++iLine == 1)
                {
                    for (int iCol = 0; iCol < values.Length; ++iCol)
                    {
                        LabelToIndices[values[iCol]] = iCol;
                    }
                }
                else
                {
                    var newLine = new Dictionary<int, string>();
                    for (int iCol = 0; iCol < values.Length; ++iCol)
                    {
                        newLine[iCol] = values[iCol];
                    }
                    Lines.Add(newLine);
                }
            });
            return true;
        }
    }
} // namespace
