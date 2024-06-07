using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private int _width;
    private int _height;
    private float _cellSize;
    private Vector3 _originPosition;
    private int[,] _gridArray;
    private TextMesh[,] _debugTextArray;

    public Grid(int width, int height, float cellSize,Vector3 originPosition)
    {
        this._width = width;
        this._height = height;
        this._cellSize = cellSize;
        this._originPosition = originPosition;

        _gridArray=new int[width,height];
        _debugTextArray=new TextMesh[width,height];

        //for (int x = 0; x < _gridArray.GetLength(0); x++)
        //{
        //    for (int y = 0; y < _gridArray.GetLength(1); y++)
        //    {
        //        _debugTextArray[x, y] = CreateTextMesh(null, _gridArray[x, y].ToString(),
        //            GetWorldPosition(x, y) + new Vector3(cellSize, 0, cellSize) * 0.5f, 7, Color.white,
        //            TextAnchor.MiddleCenter, 0);
        //        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
        //        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
        //    }
        //}
        //Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        //Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
    }

    private string PosToString(int x,int y)
    {
        return x.ToString() + "," + y.ToString();
    }
    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, 1, y) * _cellSize + _originPosition;
    }

    private void GetXY(Vector3 worldPosition,out int x,out int y)
    {
        x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
        y = Mathf.FloorToInt((worldPosition - _originPosition).z / _cellSize);//get z for our plane
    }
    public void SetValue(int x, int y, int value)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
        {
            _gridArray[x, y] = value;
            //_debugTextArray[x, y].text = _gridArray[x, y].ToString();
        }
    }
    public void SetValue(Vector3 worldPosition, int value)
    {
        int x, y;
        GetXY(worldPosition,out x,out y);
        SetValue(x,y,value);
    }

    public int GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
        {
            return _gridArray[x, y];
        }
        else
        {
            return -1;
        }
    }

    public int GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition,out x,out y);
        return GetValue(x, y);
    }
    private TextMesh CreateTextMesh(Transform parent, string text, Vector3 localPosition, int fontSize, Color color,
        TextAnchor textAnchor, int sortingOrder)
    {
        GameObject gameObject=new GameObject("TextObj",typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.eulerAngles=new Vector3(90,0,0);
        transform.SetParent(parent,false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = TextAlignment.Center;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }
}
