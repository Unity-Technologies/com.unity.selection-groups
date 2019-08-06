// (C) UTJ
using System;
using UnityEngine;
using Compositor = Utj.Film.Compositor;
using Compositor;
using Compositor.Util;

namespace Compositor.Util {

[System.AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class EditorFilenameAttribute : PropertyAttribute {
	string expectedFileExtension = "";

	public string ExpectedFileExtension {
		get {
			return expectedFileExtension;
		}
		set {
			var s = value;
			if(string.IsNullOrEmpty(s)) {
				s = "";
			} else {
				s = s.TrimStart('*').TrimStart('.');
			}
			expectedFileExtension = s;
		}
	}

	public bool AllowOutOfProjectPath { get; set; }

	public EditorFilenameAttribute() {}

	public EditorFilenameAttribute(string expectedFileExtension) {
		this.ExpectedFileExtension = expectedFileExtension;
	}

	public EditorFilenameAttribute(string expectedFileExtension, bool allowOutOfProjectPath) {
		this.ExpectedFileExtension = expectedFileExtension;
		this.AllowOutOfProjectPath = allowOutOfProjectPath;
	}
}

} // namespace
