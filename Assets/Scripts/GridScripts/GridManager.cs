using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
//using UnityEngine.Experimental.PlayerLoop;

public class GridManager : MonoBehaviour
{
    private Grid _grid;
    public int SizeX;
    public int SizeY;
    public static GridManager Instance;
    public Transform Plane;
    private void Awake()
    {
        Instance = this;
        _grid = new Grid(SizeX, SizeY, 1f, Vector3.zero);
        if (Plane!= null)
        {
            Plane.localScale = new Vector3(SizeX, transform.localScale.y, SizeY);
            Plane.position = new Vector3(SizeX / 2, Plane.position.y, SizeY / 2);
        }
    }


    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 0.6f;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    public bool BuildingIsPlacable(Vector3 position, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (_grid.GetValue((int) position.x +x, (int) position.z+y) != 0)
                {
                    return false;
                }
            }
        }

        return true;
    }
    public void PlaceBuilding(Vector3 position, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                _grid.SetValue(position+ new Vector3(-2,0,-2) + new Vector3(x,0,y),1);
            }
        }
    }
    public void PlaceMovingObstacle(Vector3 position, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if(_grid.GetValue(position + new Vector3(-1, 0, -1) + new Vector3(x, 0, y))!=1)
                    _grid.SetValue(position + new Vector3(-1, 0, -1) + new Vector3(x, 0, y), 2);
            }
        }
    }
    public void UnPlaceMovingObstacle(Vector3 position, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (_grid.GetValue(position + new Vector3(-1, 0, -1) + new Vector3(x, 0, y)) != 1)
                    _grid.SetValue(position + new Vector3(-1, 0, -1) + new Vector3(x, 0, y), 0);
            }
        }
    }
    
    public bool TargetIsValid(Vector3 position)
    {
        
        if (_grid.GetValue((int)position.x, (int)position.z) != 0)
        {
            return false;
        }
        
        return true;
    }

  
    public void MoveObstacle(Vector3 startPos, Vector3 endPos)
    {
        _grid.SetValue((int)startPos.x, (int)startPos.z, 0);
        _grid.SetValue((int)endPos.x, (int)endPos.z, 2);
    }

    public void ResetPoint(Vector3 position)
    {
        _grid.SetValue((int)position.x, (int)position.z, 0);
    }
    public int IsWalkable(int x, int y)
    {
        return _grid.GetValue(x, y);
    }
}
