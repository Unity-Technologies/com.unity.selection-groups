// https://stackoverflow.com/a/769713
using System;
using System.IO;
using System.Text.RegularExpressions;
using Compositor = Utj.Film.Compositor;
using Compositor;
using Compositor.Util;

namespace Compositor.Util {
	public static class CsvReader {
		public static bool Read(string filename, Action<string[]> action) {
			bool result = false;
			try {
				using(var stream = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
					result = Read(stream, action);
				}
			} catch(ArgumentException) {
				// TODO : implement exception handler
			}
			return result;
		}

		public static bool Read(Stream stream, Action<string[]> action) {
			bool result = false;
			try {
				using(var reader = new StreamReader(stream)) {
					result = Read(reader, action);
				}
			} catch(ArgumentException) {
				// TODO : implement exception handler
			}
			return result;
		}

		public static bool Read(TextReader reader, Action<string[]> action) {
			bool result = false;
			try {
				for(;;) {
					string line = reader.ReadLine();
					if(line == null) {
						break;
					}

					for(;;) {
						if(! rexRunOnLine.IsMatch(line)) {
							break;
						}
						string nextLine = reader.ReadLine();
						if(nextLine == null) {
							break;
						}
						line += "\n" + nextLine;
					}

					string[] values = Array.ConvertAll(
						  rexCsvSplitter.Split(line)
						, (s) => {
							if(s.StartsWith("\"") && s.EndsWith("\"")) {
								s = s.Substring(1, s.Length - 2);
								s = s.Replace("\"\"", "\"");
							}
							return s;
						}
					);
					action(values);
				}
				result = true;
			} catch(ArgumentException) {
				// TODO : implement exception handler
			}
			return result;
		}

		private static Regex rexRunOnLine = new Regex(@"^[^""]*(?:""[^""]*""[^""]*)*""[^""]*$");
		private static Regex rexCsvSplitter = new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))");
	}
} // namespace
