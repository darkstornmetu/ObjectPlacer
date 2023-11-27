using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectPlacer)), CanEditMultipleObjects]
public class ObjectPlacerEditor : Editor
{
    private ObjectPlacer _placer;

    private SerializedObject _so;

    private SerializedProperty _objectToPlace;
    private SerializedProperty _placementType;
    private SerializedProperty _currentPlacer;
    private SerializedProperty _placementMode;

    private SerializedProperty _evenPlacementProp;

    private Vector3[] _handleBoundaries;

    private void OnEnable()
    {
        _placer = target as ObjectPlacer;
       
        _so = serializedObject;
        _placementType = _so.FindProperty("_placementType");
        _objectToPlace = _so.FindProperty("_objectToPlace");
        
        ChangeCurrentPlacer();
        ConstructBoundaries();
    }

    public override void OnInspectorGUI()
    {
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.ObjectField("Script", 
                MonoScript.FromMonoBehaviour((MonoBehaviour)_placer), 
                GetType(), false);

        ChangeProperties();

        if (GUILayout.Button("Place Objects"))
            _placer.Place();
    }

    public void OnSceneGUI()
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Handles.color = Color.blue;
        Handles.DrawAAPolyLine(_handleBoundaries);
    }

    private void ChangeProperties()
    {
        _so.Update();

        EditorGUILayout.PropertyField(_objectToPlace);
        EditorGUILayout.PropertyField(_placementType);

        if (_so.ApplyModifiedProperties())
        {
            ChangeCurrentPlacer();
            ConstructBoundaries();
        }

        EditorGUILayout.PropertyField(_currentPlacer);

        if (_so.ApplyModifiedProperties())
            ConstructBoundaries();
    }

    private void ChangeCurrentPlacer()
    {
        _currentPlacer = _placer.PlacementType switch
        {
            PlacementType.Circle => _so.FindProperty("_circlePlacement"),
            PlacementType.Rectangle => _so.FindProperty("_rectanglePlacement"),
            _ => null,
        };

        _placer.ChangeCurrentPlacer();
    }

    private void ConstructBoundaries()
    {
        _placer.CurrentPlacement.CalculateBoundaries(_placer.transform.position);

        Vector3[] placerBoundaries = _placer.CurrentPlacement.Boundaries;

        _handleBoundaries = new Vector3[_placer.CurrentPlacement.Boundaries.Length + 1];
        Array.Copy(placerBoundaries, _handleBoundaries, placerBoundaries.Length);
        _handleBoundaries[^1] = placerBoundaries[0];
    }
}
