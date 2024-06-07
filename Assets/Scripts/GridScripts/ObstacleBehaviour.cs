using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBehaviour : MonoBehaviour
{

    void Start()
    {
        GridManager.Instance.PlaceBuilding(transform.position, new Vector2Int(4, 4));
    }

}
