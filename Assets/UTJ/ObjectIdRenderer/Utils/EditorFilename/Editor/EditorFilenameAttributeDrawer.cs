// (C) UTJ
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Compositor = Utj.Film.Compositor;
using Compositor;
using Compositor.Util;

namespace Compositor.Util
{
	[CustomPropertyDrawer(typeof(EditorFilenameAttribute))]
	class EditorFilenameAttributeDrawer : PropertyDrawer
	{
		static string currentDirectory = NormalizePath(Directory.GetCurrentDirectory());

		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			if(property.propertyType != SerializedPropertyType.String) {
				EditorGUI.PropertyField(rect, property, label);
				return;
			}

            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);

			string filename = PrettifyPath(property.stringValue);
			{
				var editorFilenameAttribute = (EditorFilenameAttribute)attribute;
				float buttonWidth = 22;
				float padWidth = 5;
				float pathWidth = rect.width - padWidth - buttonWidth;
				var pathRect = new Rect(rect.x, rect.y, pathWidth, rect.height);
				var buttonRect = new Rect(pathRect.xMax + padWidth, rect.y, buttonWidth, rect.height);

				// Text Field
				filename = EditorGUI.TextField(pathRect, filename);

				// Drag & Drop
				filename = DragAndDropString(pathRect, filename, (fname) => { return AcceptsFile(editorFilenameAttribute, fname); });

				// [...] Button File dialog
				if (GUI.Button(buttonRect, "..."))
				{
					var dir = string.IsNullOrEmpty(filename) ? currentDirectory : Path.GetDirectoryName(filename);
					var path = EditorUtility.OpenFilePanel("Select File", dir, editorFilenameAttribute.ExpectedFileExtension);
					if (AcceptsFile(editorFilenameAttribute, path))
					{
						filename = path;
					}
				}
			}
			property.stringValue = PrettifyPath(filename);
		}

		static bool AcceptsFile(EditorFilenameAttribute editorFilenameAttribute, string filename)
		{
			if (string.IsNullOrEmpty(filename))
			{
				return false;
			}

			filename = NormalizePath(Path.GetFullPath(filename));

			string ext = editorFilenameAttribute.ExpectedFileExtension;
			bool validExtension = string.IsNullOrEmpty(ext) || filename.EndsWith("." + ext);
			if (!validExtension)
			{
				return false;
			}

			bool validDir = editorFilenameAttribute.AllowOutOfProjectPath;
			if (!validDir)
			{
				if (Path.IsPathRooted(filename))
				{
					validDir = filename.StartsWith(currentDirectory);
				}
				else
				{
					validDir = true;
				}
			}
			return validDir;
		}

		static string NormalizePath(string path)
		{
			return path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		static string PrettifyPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return "";
			}
			path = NormalizePath(path);
			if (!path.StartsWith(currentDirectory))
			{
				return path;
			}
			return path.Substring(currentDirectory.Length).TrimStart(Path.AltDirectorySeparatorChar);
		}

		static string DragAndDropString(Rect rect, string str, Func<string, bool> validator)
		{
			string result = str;
			Event currentEvent = Event.current;
			EventType eventType = currentEvent.type;
			if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
			{
				if (rect.Contains(currentEvent.mousePosition))
				{
					var m = DragAndDropVisualMode.Rejected;
					if (DragAndDrop.paths.Length == 1 && validator(DragAndDrop.paths[0]))
					{
						m = DragAndDropVisualMode.Generic;
						if (eventType == EventType.DragPerform)
						{
							result = DragAndDrop.paths[0];
							DragAndDrop.AcceptDrag();
						}
					}
					DragAndDrop.visualMode = m;
					currentEvent.Use();
				}
			}
			return result;
		}
	}

} // namespace
#endif
