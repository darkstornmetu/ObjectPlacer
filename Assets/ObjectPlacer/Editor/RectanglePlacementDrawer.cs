using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RectanglePlacement))]
public class RectanglePlacementDrawer : PropertyDrawer
{
    private SerializedProperty _placementMode;
    
    private SerializedProperty _width;
    private SerializedProperty _height;
    
    private SerializedProperty _xInterval;
    private SerializedProperty _yInterval;

    private int _mode;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label, EditorStyles.boldLabel);
        
        _placementMode = property.FindPropertyRelative("_placementMode");
        _width = property.FindPropertyRelative("_width");
        _height = property.FindPropertyRelative("_height");
        _xInterval = property.FindPropertyRelative("_xInterval");
        _yInterval = property.FindPropertyRelative("_yInterval");

        _mode = _placementMode.enumValueIndex;
        
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = indent + 1;
        
        EditorGUILayout.PropertyField(_placementMode);
        EditorGUILayout.PropertyField(_width);
        EditorGUILayout.PropertyField(_height);

        if (_mode == 0)
        {
            EditorGUILayout.PropertyField(_xInterval);
            EditorGUILayout.PropertyField(_yInterval);
        }
        else
            EditorGUILayout.PropertyField(_xInterval, new GUIContent("Min Distance"));

        EditorGUI.indentLevel = indent;
        
        EditorGUI.EndProperty();
    }
}