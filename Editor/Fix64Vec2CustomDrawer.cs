using UnityEditor;
using Parallel;
using UnityEngine;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(Fix64Vec2))]
public class Fix64Vec2CustomDrawer : PropertyDrawer
{
    private static Dictionary<string, int> _fieldCounts = new Dictionary<string, int>();
    const float space = 3f;
    const float titleWidth = 150f;
    const float smallTitleWidth = 70f;
    const float fullTitleWidthLimit = 300f;
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
        SerializedProperty rawX = property.FindPropertyRelative("RawX");
        SerializedProperty rawY = property.FindPropertyRelative("RawY");

        //var boxRect = new Rect(position.x + space, position.y + space, position.width - 2 * space, position.height - 2 * space);
        float fullWidth = position.width;
        float dynamicTitleWidth = titleWidth;

        if (fullWidth < fullTitleWidthLimit)
        {
            dynamicTitleWidth = smallTitleWidth;
        }

        float xOffset = position.x + space;
        float yOffset = position.y + space;
        float useableWidth = fullWidth - space;
        float fieldWidth = (useableWidth - 1 * space - 2 * space - dynamicTitleWidth) / 2;
        float lineHeight = EditorGUIUtility.singleLineHeight;

        var titleRect = new Rect(xOffset, yOffset, dynamicTitleWidth, lineHeight);

        var fieldXRect = new Rect(titleRect.xMax + space, yOffset, fieldWidth, lineHeight);

        var fieldYRect = new Rect(fieldXRect.xMax + space, yOffset, fieldWidth, lineHeight);

        GUI.Box(position, "");

        EditorGUI.LabelField(titleRect, new GUIContent(name));

        DrawGUIForFixedValueProperty(rawX, "X", fieldXRect);
        DrawGUIForFixedValueProperty(rawY, "Y", fieldYRect);
    }

    private static void DrawGUIForFixedValueProperty(SerializedProperty obj, string title, Rect rect)
    {
        float titleWidth = 0f;
        Rect titleRect = new Rect(rect.x, rect.y, titleWidth, rect.height);
        Rect valueRect = new Rect(titleRect.xMax, rect.y, rect.width - titleWidth, rect.height);
        Rect rawValueRect = new Rect(titleRect.xMax, rect.y + EditorGUIUtility.singleLineHeight, rect.width - titleWidth, rect.height);

        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleRight;
        //EditorGUI.LabelField(titleRect, title, style);

        EditorGUI.BeginChangeCheck();
        long oldRawValue = obj.longValue;
        Fix64 oldValue = Fix64.FromRaw(oldRawValue);

        EditorGUIUtility.labelWidth = 10f;
        float newVal = EditorGUI.FloatField(valueRect, title, (float)oldValue);
        EditorGUIUtility.labelWidth = 0;

        if (EditorGUI.EndChangeCheck())
        {
            Fix64 newFixedValue = (Fix64)newVal;
            obj.longValue = newFixedValue.Raw;
        }

        style.normal.textColor = Color.gray;
        EditorGUI.LabelField(rawValueRect, $"{obj.longValue}", style);
    }

}
