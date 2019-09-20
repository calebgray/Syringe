using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SyringeInjection.Editor {
	[CustomPropertyDrawer(typeof(Syringe))]
	public class SyringePropertyDrawer : PropertyDrawer {
		private static int GetIndexFromSerializedPropertyPath(string path) {
			var start = path.IndexOf('[') + 1;
			return start == 0 ? -1 : int.Parse(path.Substring(start, path.IndexOf(']') - start));
		}

		public static Syringe GetSelectedValueFromSerializedProperty(FieldInfo fieldInfo, SerializedProperty serializedProperty, out int arrayIndex, out int listIndex) {
			var rawSyringe = fieldInfo.GetValue(serializedProperty.serializedObject.targetObject);
			Syringe syringe;
			if (rawSyringe.GetType().IsAssignableFrom(typeof(Syringe[]))) {
				listIndex = -1;
				arrayIndex = GetIndexFromSerializedPropertyPath(serializedProperty.propertyPath);
				syringe = ((Syringe[]) rawSyringe)[arrayIndex];
			} else if (rawSyringe.GetType().IsAssignableFrom(typeof(List<Syringe>))) {
				arrayIndex = -1;
				listIndex = GetIndexFromSerializedPropertyPath(serializedProperty.propertyPath);
				syringe = ((List<Syringe>) rawSyringe)[listIndex];
			} else {
				arrayIndex = -1;
				listIndex = -1;
				syringe = (Syringe) rawSyringe;
			}

			return syringe;
		}

		public static void MarkObjectAsDirty(Object obj) {
			EditorUtility.SetDirty(obj);

			var gameObject = obj as GameObject;
			if (gameObject == null && obj is Component) gameObject = ((Component) obj).gameObject;
			if (gameObject != null) EditorSceneManager.MarkSceneDirty(gameObject.scene);
		}

		public override void OnGUI(Rect position, SerializedProperty serializedProperty, GUIContent label) {
			// Arrays/Lists Need Extra Love
			var realSerializedProperty = serializedProperty.serializedObject.FindProperty(fieldInfo.Name);
			if (realSerializedProperty.isArray) {
				var rawSyringe = fieldInfo.GetValue(serializedProperty.serializedObject.targetObject);
				var syringes = new List<Syringe>();
				var isArray = false;
				if (rawSyringe.GetType().IsAssignableFrom(typeof(Syringe[]))) {
					syringes = ((Syringe[]) rawSyringe).ToList();
					isArray = true;
				} else if (rawSyringe.GetType().IsAssignableFrom(typeof(List<Syringe>))) {
					syringes = (List<Syringe>) rawSyringe;
				}

				while (syringes.Count < realSerializedProperty.arraySize) {
					syringes.Add(new Syringe(typeof(Syringe)));
				}

				while (syringes.Count > realSerializedProperty.arraySize) {
					syringes.RemoveAt(syringes.Count - 1);
				}

				if (isArray) {
					fieldInfo.SetValue(serializedProperty.serializedObject.targetObject, syringes.ToArray());
				}
			}

			// Single Element
			EditorGUI.BeginProperty(position, label, serializedProperty);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			int _arrayIndex, _listIndex;
			var selectedValue = GetSelectedValueFromSerializedProperty(fieldInfo, serializedProperty, out _arrayIndex, out _listIndex);
			if (GUI.Button(new Rect(position.xMin + position.width - 18, position.yMin, 18, position.height), "X")) {
				// Remove the Value
				selectedValue = null;
				if (_arrayIndex > -1) {
					((Syringe[]) fieldInfo.GetValue(serializedProperty.serializedObject.targetObject))[_arrayIndex] = null;
				} else if (_listIndex > -1) {
					((List<Syringe>) fieldInfo.GetValue(serializedProperty.serializedObject.targetObject))[_listIndex] = null;
				} else {
					fieldInfo.SetValue(serializedProperty.serializedObject.targetObject, null);
				}

				// Mark the Target as Dirty
				MarkObjectAsDirty(serializedProperty.serializedObject.targetObject);
			}

			if (GUI.Button(new Rect(position.xMin, position.yMin, position.width - 18, position.height), selectedValue == null ? "Select Type" : selectedValue.FriendlyName)) {
				SyringeEditorWindow.Show(fieldInfo, serializedProperty);
			}

			EditorGUI.EndProperty();
		}
	}
}