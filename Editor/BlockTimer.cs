using System;
using UnityEditor;
using UnityEngine;

namespace Unity.SelectionGroups
{
    internal class AnalyticsTimer : IDisposable
    {
        string name;
        System.Diagnostics.Stopwatch clock;

        public AnalyticsTimer(string name)
        {
            this.name = name;
            clock = new System.Diagnostics.Stopwatch();
            SendEvent("Started");
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