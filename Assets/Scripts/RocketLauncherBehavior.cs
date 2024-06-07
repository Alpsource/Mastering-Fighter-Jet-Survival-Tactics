using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class RocketLauncherBehavior : MonoBehaviour
{
    private Rigidbody mRigid;
    public bool Detected;
    public bool Launched;
    public bool Used;
    public float radius;
    private Transform targetTransform;
    void Start()
    {
        mRigid = GetComponent<Rigidbody>();
        Launched = false;
        Detected = false;
        Used = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Detected && !Used)
        {
            Collider[] obsList = Physics.OverlapSphere(transform.position, radius, 1 << 9);
            if (obsList.Length > 0)
            {
                //Debug.Log("Found a moving obs!");
                foreach (Collider col in obsList)
                {
                    if (col.gameObject.tag == "Player")
                    {
                        targetTransform = col.transform;
                        
                        Detected = true;
                        targetTransform.GetComponent<AircraftAgent>().SwitchBehavior(AircraftAgent.BehaviorType.DQN);
                        Debug.Log("Switched to DQN");
                        break;
                    }
                }
            }
        }
        if(Detected && !Launched)
        {
            if(targetTransform.position.z + 5 > transform.position.z)
            {
                Vector3 direction = targetTransform.position - transform.position;
                direction.Normalize();
                mRigid.AddForce(direction*7,ForceMode.Impulse);
                Launched = true;
            }
        }
        if (Detected && Launched)
        {
            if(Vector3.Distance(transform.position, targetTransform.position) > 10)
            {
                targetTransform.GetComponent<AircraftAgent>().SwitchBehavior(AircraftAgent.BehaviorType.AStar);
                mRigid.velocity = Vector3.zero;
                mRigid.angularVelocity = Vector3.zero;
                Launched = false;
                Detected = false;
                Used = true;
                //Destroy(gameObject);
            }
        }

    }
    public void ResetSelf()
    {
        mRigid.velocity = Vector3.zero;
        mRigid.angularVelocity = Vector3.zero;
        Launched = false;
        Detected = false;
        Used = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
