using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Camera ReferenceCamera;
    public LayerMask ReferenceLayer;
    public AircraftAgent TargetAirCraft;
    public Transform InitialTargetPosition;

    private Ray _myRay;

    private void Start()
    {
        //Debug.Log(InitialTargetPosition);
        //TargetAirCraft.SetSelfTarget(InitialTargetPosition.position);
    }
    void Update()
    {
        void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (Time.timeScale == 1.0f)
                    Time.timeScale = 5.0f;
                else
                    Time.timeScale = 1.0f;
            }
        }
    }
}
