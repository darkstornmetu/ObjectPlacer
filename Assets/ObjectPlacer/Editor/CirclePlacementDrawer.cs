using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CirclePlacement))]
public class CirclePlacementDrawer : PropertyDrawer
{
    private SerializedProperty _placementMode;
    private SerializedProperty _lookToCenter;
    
    private SerializedProperty _radius;
    private SerializedProperty _resolution;
    private SerializedProperty _interval;
    private SerializedProperty _totalRingCount;
    private SerializedProperty _overrideRingCount;
    private SerializedProperty _overriddenRingCount;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label, EditorStyles.boldLabel);
        
        _placementMode = property.FindPropertyRelative("_placementMode");
        _lookToCenter = property.FindPropertyRelative("_lookToCenter");
        _resolution = property.FindPropertyRelative("_resolution");
        _radius = property.FindPropertyRelative("_radius");
        _interval = property.FindPropertyRelative("_interval");
        _totalRingCount = property.FindPropertyRelative("_totalRingCount");
        _overrideRingCount = property.FindPropertyRelative("_overrideRingCount");
        _overriddenRingCount = property.FindPropertyRelative("_overriddenRingCount");

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = indent + 1;
        
        EditorGUILayout.PropertyField(_placementMode);
        EditorGUILayout.PropertyField(_lookToCenter);
        
        //Even Placement
        if (_placementMode.enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(_resolution);
            EditorGUILayout.PropertyField(_radius);
            EditorGUILayout.PropertyField(_interval);
            
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(_totalRingCount);
        
            EditorGUILayout.PropertyField(_overrideRingCount);
        
            if(_overrideRingCount.boolValue)
                EditorGUILayout.PropertyField(_overriddenRingCount, new GUIContent("Ring Count"));
        }
        else if (_placementMode.enumValueIndex == 1)
        {
            EditorGUILayout.PropertyField(_radius);
            EditorGUILayout.PropertyField(_interval, new GUIContent("Min Distance"));
        }

        EditorGUI.indentLevel = indent;
        
        EditorGUI.EndProperty();
    }
}