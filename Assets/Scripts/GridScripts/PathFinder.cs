using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

public class PathFinder : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    public static PathFinder Instance;
    private void Awake()
    {
        Instance = this;
    }

    public bool GetNextPoint(Vector3 startPosition, Vector3 endPosition, out Vector3 nextPoint)
    {
        NativeList<int2> MyPath;
        int2 startİnt2 = new int2((int) startPosition.x, (int) startPosition.z);
        int2 endİnt2 = new int2((int)endPosition.x,(int)endPosition.z);

        if (FindPath(startİnt2, endİnt2, out MyPath))
        {
            int2 target = MyPath[MyPath.Length - 2];
            //Debug.Log(MyPath.Length);
            nextPoint=new Vector3(target.x,1f,target.y);
            return true;
        }
        nextPoint=Vector3.zero;
        return false;
    }
    private bool FindPath(int2 startPosition, int2 endPosition, out NativeList<int2> pathList)
    {
        int2 gridSize=new int2(200,500);
        NativeArray<PathNode> pathNodeArray= new NativeArray<PathNode>(gridSize.x*gridSize.y,Allocator.Temp);
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                PathNode pathNode = new PathNode
                {
                    x = x,
                    y = y,
                    index = CalculateIndex(x, y, gridSize.x),
                    gCost = int.MaxValue,
                    hCost = CalculateDistanceCost(new int2(x, y), endPosition)
                };

                pathNode.CalculateFCost();

                pathNode.isWalkable = GridManager.Instance.IsWalkable(x,y);
                pathNode.cameFromNodeIndex = -1;

                pathNodeArray[pathNode.index] = pathNode;
            }
        }

        NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(new int2[]
        {
            new int2(-1,0),
            new int2(1,0),
            new int2(0,1),
            new int2(0,-1),
            new int2(-1,-1),
            new int2(-1,1),
            new int2(1,-1),
            new int2(1,1), 
        },Allocator.Temp );

        int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);
        PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
        startNode.gCost = 0;
        startNode.CalculateFCost();
        pathNodeArray[startNode.index] = startNode;

        NativeList<int> openList =new NativeList<int>(Allocator.Temp);
        NativeList<int> closedList =new NativeList<int>(Allocator.Temp);

        openList.Add(startNode.index);

        while (openList.Length>0)
        {
            int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
            PathNode currentNode = pathNodeArray[currentNodeIndex];

            if (currentNodeIndex == endNodeIndex)
            {
                break;
            }

            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i] == currentNodeIndex)
                {
                    openList.RemoveAtSwapBack(i);
                    break;
                }
            }
            closedList.Add(currentNodeIndex);

            for (int i = 0; i < neighbourOffsetArray.Length; i++)
            {
                int2 neighbourOffest = neighbourOffsetArray[i];
                int2 neighbourPosition = new int2(currentNode.x + neighbourOffest.x, currentNode.y + neighbourOffest.y);
                if (!isInsideGrid(neighbourPosition, gridSize))
                {
                    continue;
                }

                int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);

                if (closedList.Contains(neighbourNodeIndex))
                {
                    continue;
                }

                PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                //if (neighbourNode.isWalkable==1 || neighbourNode.isWalkable == 2)
                if(GridManager.Instance.IsWalkable(neighbourNode.x,neighbourNode.y)!=0)
                {
                    continue;
                }

                int2 currentNodePosition = new int2(currentNode.x,currentNode.y);

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNodeIndex = currentNodeIndex;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.CalculateFCost();
                    pathNodeArray[neighbourNodeIndex] = neighbourNode;

                    if (!openList.Contains(neighbourNodeIndex))
                    {
                        openList.Add(neighbourNodeIndex);
                    }
                }
            }
        }

        PathNode endNode = pathNodeArray[endNodeIndex];
        if (endNode.cameFromNodeIndex == -1)
        {
            pathNodeArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
            neighbourOffsetArray.Dispose();
            Debug.Log("No Paths Available");
            pathList = new NativeList<int2>();
            return false;
        }
        else
        {
            NativeList<int2> path = CalculatePath(pathNodeArray, endNode);
            
            // for (int i = 0; i < path.Length; i++)
            // {
            //     Debug.Log(path[i]);
            // }

            pathList = path;
            pathNodeArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
            neighbourOffsetArray.Dispose();
            return true;
        }
        
    }

    private NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
    {
        if (endNode.cameFromNodeIndex==-1)
        {
            return  new NativeList<int2>(Allocator.Temp);
        }
        else
        {
            NativeList<int2> path =new NativeList<int2>(Allocator.Temp);
            path.Add(new int2(endNode.x,endNode.y));

            PathNode currentNode = endNode;
            while (currentNode.cameFromNodeIndex != -1)
            {
                PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                path.Add(new int2(cameFromNode.x,cameFromNode.y));
                currentNode = cameFromNode;

            }

            return path;
        }
    }
    private bool isInsideGrid(int2 gridPosition, int2 gridSize)
    {
        return
            gridPosition.x >= 0 && gridPosition.y >= 0 &&
            gridPosition.x < gridSize.x && gridPosition.y < gridSize.y;
    }
    private int CalculateIndex(int x, int y, int gridWidth)
    {
        return x + y * gridWidth;
    }

    private int CalculateDistanceCost(int2 aPosition, int2 bPosition)
    {
        int xDistance = math.abs(aPosition.x - bPosition.x);
        int yDistance = math.abs(aPosition.y - bPosition.y);
        int remaining = math.abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
    {
        PathNode lowestCostPathNode = pathNodeArray[openList[0]];
        for (int i = 0; i < openList.Length; i++)
        {
            PathNode testPathNode = pathNodeArray[openList[i]];
            if (testPathNode.fCost < lowestCostPathNode.fCost)
            {
                lowestCostPathNode = testPathNode;
            }
        }

        return lowestCostPathNode.index;
    }
    private struct PathNode
    {
        public int x;
        public int y;

        public int index;

        public int gCost;
        public int hCost;
        public int fCost;

        public int isWalkable;

        public int cameFromNodeIndex;

        public void CalculateFCost()
        {
            fCost = hCost + gCost;
        }
    }
}
