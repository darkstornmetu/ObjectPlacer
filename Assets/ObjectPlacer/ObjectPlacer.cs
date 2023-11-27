using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ObjectPlacer : MonoBehaviour
{
    [SerializeField] private GameObject _objectToPlace;
    
    [SerializeField] private PlacementType _placementType = PlacementType.Rectangle;
    
    [SerializeField] private CirclePlacement _circlePlacement;
    [SerializeField] private RectanglePlacement _rectanglePlacement;

    public PlacementType PlacementType => _placementType;
    public Placement CurrentPlacement => _currentPlacement;

    private Placement _currentPlacement;
    
    public void ChangeCurrentPlacer()
    {
        _currentPlacement = _placementType switch
        {
            PlacementType.Circle => _circlePlacement,
            PlacementType.Rectangle => _rectanglePlacement,
            _ => null,
        };
    }

    public void Place()
    {
#if UNITY_EDITOR        
        //delete child objects if any
        int count = transform.childCount;
        if (count > 0)
        {
            for (int i = count - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        //Get Positions
        Vector3[] placePositions = _currentPlacement.GetPlacement();

        //Instantiate Prefabs
        for (int i = 0; i < placePositions.Length; i++)
        {
            GameObject go = PrefabUtility.InstantiatePrefab(_objectToPlace, transform) as GameObject;
            go.transform.position = placePositions[i];
        }
#endif        
    }
}

[System.Serializable]
public abstract class Placement
{
    public Vector3[] Boundaries { get; protected set; }
    protected Vector3 Origin { get; set; }
    
    [SerializeField]
    private PlacementMode _placementMode = PlacementMode.Even;
    
    public Vector3[] GetPlacement()
    {
        return _placementMode switch
        {
            PlacementMode.Even => GetEvenPlacement(),
            PlacementMode.Random => GetRandomPlacement(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public abstract Vector3[] GetEvenPlacement();
    public abstract Vector3[] GetRandomPlacement();
    public abstract void CalculateBoundaries(Vector3 origin);
}

[System.Serializable]
public class CirclePlacement : Placement
{
    [SerializeField] 
    private bool _lookToCenter;
    [Min(0.01f)] [SerializeField]
    private float _radius;
    [Range(2, 32)] [SerializeField]
    private int _resolution = 16;
    [Min(0.1f)] [SerializeField]
    private float _interval;
    [SerializeField]
    private bool _overrideRingCount;
    [Min(1)] [SerializeField]
    private int _overriddenRingCount;
    [SerializeField]
    private int _totalRingCount;
    
    public override Vector3[] GetEvenPlacement()
    {
        List<Vector3> positions = new List<Vector3>();

        for (int x = _totalRingCount - 1; x >= _totalRingCount - _overriddenRingCount; x--)
        {
            if (x == 0)
            {
                positions.Add(Origin);
                break;
            }
            
            float currentRadius = _interval * x;

            int pointCount = Mathf.FloorToInt(Mathf.Lerp(0, Boundaries.Length, currentRadius / _radius));
            float pieceRad = Mathf.PI * 2 / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                Vector3 pointPos = Origin + new Vector3(Mathf.Cos(pieceRad * i) * currentRadius, 0, Mathf.Sin(pieceRad * i) * currentRadius);
                positions.Add(pointPos);
            }
        }
        
        return positions.ToArray();
    }

    public override Vector3[] GetRandomPlacement()
    {
        List<Vector2> randomPlacements = PoissonDisk.GeneratePoints(_interval, new Vector2(_radius * 2, _radius * 2));

        List<Vector3> positions = new();

        for (int i = 0; i < randomPlacements.Count; i++)
        {
            Vector2 placement = randomPlacements[i];

            Vector3 pos;
            pos.x = Origin.x + placement.x - _radius;
            pos.y = Origin.y;
            pos.z = Origin.z + placement.y - _radius;

            if (Vector3.Distance(Origin, pos) <= _radius) //Check if a point inside the circle
                positions.Add(pos);
        }

        return positions.ToArray();
    }

    public override void CalculateBoundaries(Vector3 origin)
    {
        Origin = origin;

        Boundaries = new Vector3[_resolution];

        float pieceRad = Mathf.PI * 2 / Boundaries.Length;

        for (int i = 0; i < Boundaries.Length; i++)
            Boundaries[i] = origin + _radius * new Vector3(Mathf.Cos(pieceRad * i), 0, Mathf.Sin(pieceRad * i));

        _totalRingCount = Mathf.FloorToInt(_radius / _interval) + 1;

        if (!_overrideRingCount)
            _overriddenRingCount = _totalRingCount;
    }
}

[System.Serializable]
public class RectanglePlacement : Placement
{
    [Min(0.01f)] [SerializeField]
    private float _width;
    [Min(0.01f)] [SerializeField]
    private float _height;
    [SerializeField]
    private float _xInterval;
    [SerializeField]
    private float _yInterval;

    public override Vector3[] GetEvenPlacement()
    {
        List<Vector3> positions = new List<Vector3>();

        int wPointCount = Mathf.FloorToInt(_width / _xInterval);
        int hPointCount = Mathf.FloorToInt(_height / _yInterval);

        float halfWInterval = _xInterval * wPointCount / 2;
        float halfHInterval = _yInterval * hPointCount / 2;

        for (int x = 0; x < wPointCount + 1; x++)
        {
            for (int y = 0; y < hPointCount + 1; y++)
            {
                Vector3 pointPos = new Vector3(
                    Origin.x - halfWInterval + _xInterval * x,
                    Origin.y,
                    Origin.z - halfHInterval + _yInterval * y);

                positions.Add(pointPos);
            }
        }

        return positions.ToArray();
    }

    public override Vector3[] GetRandomPlacement()
    {
        List<Vector2> randomPlacements = PoissonDisk.GeneratePoints(_xInterval, new Vector2(_width, _height));

        Vector3[] positions = new Vector3[randomPlacements.Count];

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 pos;
            Vector2 placement = randomPlacements[i];

            pos.x = Origin.x + placement.x - _width / 2;
            pos.y = Origin.y;
            pos.z = Origin.z + placement.y - _height / 2;

            positions[i] = pos;
        }

        return positions;
    }

    public override void CalculateBoundaries(Vector3 origin)
    {
        Origin = origin;
        Boundaries = new Vector3[4];

        float halfWidth = _width / 2;
        float halfHeight = _height / 2;

        Boundaries[0] = origin + new Vector3(-halfWidth, 0, -halfHeight);
        Boundaries[1] = origin + new Vector3(-halfWidth, 0, halfHeight);
        Boundaries[2] = origin + new Vector3(halfWidth, 0, halfHeight);
        Boundaries[3] = origin + new Vector3(halfWidth, 0, -halfHeight);
    }
}

public enum PlacementType
{
    Circle,
    Rectangle,
}

public enum PlacementMode
{
    Even,
    Random
}
