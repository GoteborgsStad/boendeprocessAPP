using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	[CustomPropertyDrawer(typeof(TextTagAttribute))]
	public class TextTagAttributeDrawer : PropertyDrawer 
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.String)
			{
				EditorGUI.LabelField(position, "! Use TextTag only on string");
				return;
			}

			Rect popupArea = position;
			popupArea.width = popupArea.height;
			position.width -= popupArea.width;
			popupArea.x += position.width;
			position.width -= 5;
			
			EditorGUI.PropertyField(position, property, label);

			// Tag popup
			int chosen = EditorGUI.Popup(popupArea, -1, TextManager.InspectorTags);

			if (chosen != -1)
			{
				if (TextManager.InspectorTags[chosen] == TextManager.REFRESH_TAG)
				{
					TextManager.EditorRefresh();
				}
				else
				{
					Undo.RecordObject(property.serializedObject.targetObject, "Pick Text Tag from attribute");
					property.stringValue = TextManager.AllTags[chosen];
					EditorUtility.SetDirty(property.serializedObject.targetObject);
				}
			}
		}
	}
}