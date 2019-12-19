using System;
using UnityEditor;
using UnityEngine;

namespace Unity.SelectionGroups
{
    internal class BlockTimer : IDisposable
    {
        string name;
        System.Diagnostics.Stopwatch clock;
        static string version = "0.0.0";

        public BlockTimer(string name)
        {
            this.name = name;
            clock = new System.Diagnostics.Stopwatch();
            clock.Start();
        }

        public void Dispose()
        {
            clock.Stop();
            SendEvent("Completed");
        }

        internal void SendEvent(string msg = "")
        {
            var resume = false;
            if (clock.IsRunning)
            {
                clock.Stop();
                resume = true;
            }
            if (Application.isEditor)
            {
                EditorAnalytics.SendEventWithLimit(name, new { duration = clock.Elapsed.TotalSeconds, msg = msg });
            }
            if (resume)
            {
                clock.Start();
            }
        }
    }
}