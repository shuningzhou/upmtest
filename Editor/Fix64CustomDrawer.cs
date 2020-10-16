using UnityEditor;
using Parallel;
using UnityEngine;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(Fix64))]
public class Fix64CustomDrawer : PropertyDrawer
{
    private static Dictionary<string, int> _fieldCounts = new Dictionary<string, int>();
    const float space = 3f;
    static Texture2D backgroundTexture;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //-----
        //space
        //field
        //raw
        //space
        //-----
        return EditorGUIUtility.singleLineHeight * 2 + 2 * space;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string name = property.displayName;
        SerializedProperty raw = property.FindPropertyRelative("Raw");

        //var boxRect = new Rect(position.x + space, position.y + space, position.width - 2 * space, position.height - 2 * space);

        var fieldRect = new Rect(position.x + space, position.y + space, position.width - 2 * space, EditorGUIUtility.singleLineHeight);
        var rawRect = new Rect(position.x + space, fieldRect.y + EditorGUIUtility.singleLineHeight, position.width - 2 * space, EditorGUIUtility.singleLineHeight);

        GUI.Box(position, "");

        EditorGUI.BeginChangeCheck();
        long oldRawValue = raw.longValue;
        Fix64 oldValue = Fix64.FromRaw(oldRawValue);
        float newVal = EditorGUI.FloatField(fieldRect, new GUIContent(name), (float)oldValue);
        if (EditorGUI.EndChangeCheck())
        {
            Fix64 newFixedValue = (Fix64)newVal;
            raw.longValue = newFixedValue.Raw;
        }

        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleRight;
        style.normal.textColor = Color.gray;
        EditorGUI.LabelField(rawRect, $"RawValue: {raw.longValue}", style);
    }

    private static int GetFieldCount(SerializedProperty property)
    {
        int count;
        if (!_fieldCounts.TryGetValue(property.type, out count))
        {
            var children = property.Copy().GetEnumerator();
            while (children.MoveNext())
            {
                count++;
            }

            _fieldCounts[property.type] = count;
        }

        return count;
    }

}
