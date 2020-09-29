using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.GoQL
{
    public class GoQLWindow : EditorWindow
    {
        string query = string.Empty;
        GoQLExecutor goqlMachine = new GoQLExecutor();
        List<object> instructions;
        GameObject[] selection;
        List<Token> tokens;

        [MenuItem("Window/General/GoQL")]
        static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<GoQLWindow>();
            window.Show();
        }

        // Update is called once per frame
        void Update()
        {
            tokens = new Tokenizer().Tokenize(query);
            instructions = Parser.Parse(query, out ParseResult result);
            goqlMachine.Code = query;
            selection = goqlMachine.Execute();
            // if (goqlMachine.selection != null)
            //     Selection.objects = goqlMachine.selection.ToArray();
        }

        void OnGUI()
        {
            query = EditorGUILayout.TextField(query);
            if (goqlMachine.Error != string.Empty)
            {
                EditorGUILayout.HelpBox(goqlMachine.Error, MessageType.Error);
            }
            if(tokens != null) {
                foreach(var t in tokens) {
                    GUILayout.Label(t.ToString());
                }
            }
            if (instructions != null)
                foreach (var i in instructions)
                {
                    GUILayout.Label(i.ToString());
                }
            // foreach (var i in goqlMachine.messages)
            // {
            //     GUILayout.Label(i);
            // }
            GUILayout.BeginVertical("box");
            Selection.objects = selection;

            GUILayout.EndVertical();
        }
    }
}