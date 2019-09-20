using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SyringeInjection.Editor {
	public class SyringeEditorWindow : EditorWindow {
		// Configurable
		private const int MaxResults = 30;

		// Interface Cache
		private static readonly Dictionary<string, string> _types;
		private static string _searchString = "";
		private static FieldInfo _fieldInfo;
		private static SerializedProperty _serializedProperty;
		private static int _arrayIndex;
		private static int _listIndex;
		private static GUIStyle _style;

		static SyringeEditorWindow() {
			_types = new Dictionary<string, string>();
			var errorLogCache = new Dictionary<string, Type>();
			var typesCache = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
			foreach (var type in typesCache) {
				// as of Unity 2018.3, 2 dynamic assemblies of Microsoft Serialization are returned from GetAssemblies() 
				// the following code is to prevent the duplicate dictionary entry in Syringe
				if (_types.ContainsKey(type.AssemblyQualifiedName)) {
					var used = errorLogCache[type.AssemblyQualifiedName];

					if (used.Assembly.IsDynamic && type.Assembly.IsDynamic) {
						// If both types are dynamic, this is expected and ignored here.
						continue;
					}

					Debug.LogError("SyringEditorWindow() found a duplicate assembly qualified name: "
					               + type.AssemblyQualifiedName
					               + "\r\nUsing: "
					               + used
					               + " from "
					               + (used.Assembly.IsDynamic ? "Dynamically generated" : used.Assembly.Location)
					               + "\r\nSkipped: "
					               + type.FullName
					               + " from "
					               + (type.Assembly.IsDynamic ? "Dynamically generated" : type.Assembly.Location));

					continue;
				}

				errorLogCache.Add(type.AssemblyQualifiedName, type);
				_types.Add(type.AssemblyQualifiedName, type.FullName);
			}
		}

		public static void Show(FieldInfo fieldInfo, SerializedProperty serializedProperty) {
			// Reset to Default Values
			_fieldInfo = fieldInfo;
			_serializedProperty = serializedProperty;

			// Set _searchString to the value of the serialized property.
			var syringe = SyringePropertyDrawer.GetSelectedValueFromSerializedProperty(fieldInfo, serializedProperty, out _arrayIndex, out _listIndex);
			_searchString = syringe == null ? "" : syringe.FullName;

			// Open the SyringeEditorWindow near the InspectorWindow.
			GetWindowWithRect(typeof(SyringeEditorWindow), GetWindow(Type.GetType(typeof(EditorWindow).AssemblyQualifiedName.Replace("EditorWindow", "InspectorWindow"))).position, true, "Syringe - Select Type");
		}

		/// <summary>
		/// Returns [Parent].[TypeName] but no further in lineage.	
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public static string GetSimplerTypeName(string typeName) {
			if (typeName == null) {
				Debug.LogError("SyringeEditorWindow.GetSimplerTypeName received a null type name");
				return "[NULL TYPE NAME]";
			}

			var lineage = typeName.Split('.');
			if (lineage.Length == 1) {
				return typeName;
			}

			return lineage[lineage.Length - 2] + "." + lineage[lineage.Length - 1];
		}

		private void OnGUI() {
			if (_style == null) {
				_style = new GUIStyle(GUI.skin.button);
				_style.alignment = TextAnchor.MiddleLeft;
			}

			// The header.
			GUILayout.Label("Select a Type", EditorStyles.boldLabel);

			// Search text field.
			_searchString = GUILayout.TextField(_searchString ?? "");

			var i = 0;
			foreach (var type in _types) {
				// Skip non-matches.
				if (!type.Value.ToLower().Contains(_searchString.ToLower())) continue;

				var simplerName = GetSimplerTypeName(type.Value);

				if (GUILayout.Button(simplerName, _style)) {
					// Create the new Syringe type wrapper.
					var selectedType = new Syringe(Type.GetType(type.Key));

					// Replace the value in the serialized property.
					if (_arrayIndex > -1) {
						((Syringe[]) _fieldInfo.GetValue(_serializedProperty.serializedObject.targetObject))[_arrayIndex] = selectedType;
					} else if (_listIndex > -1) {
						((List<Syringe>) _fieldInfo.GetValue(_serializedProperty.serializedObject.targetObject))[_listIndex] = selectedType;
					} else {
						_fieldInfo.SetValue(_serializedProperty.serializedObject.targetObject, selectedType);
					}

					// Mark the Target as Dirty
					SyringePropertyDrawer.MarkObjectAsDirty(_serializedProperty.serializedObject.targetObject);

					// Close the SyringeEditorWindow.
					Close();
				}

				// Limit results.
				if (i++ > MaxResults) break;
			}
		}
	}
}