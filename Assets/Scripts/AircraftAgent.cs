using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.Burst.Intrinsics;
using static UnityEngine.GraphicsBuffer;
using Unity.MLAgents.Policies;
using NUnit.Framework;
using static AircraftAgent;

public class AircraftAgent : Agent
{
    [SerializeField] private Transform targetTansform;
    [SerializeField] private Transform obstacleTansform;
    [SerializeField] private Material positiveRewardMat;
    [SerializeField] private Material negativeRewardMat;
    [SerializeField] private MeshRenderer platformMeshRenderer;

    public bool training;
    public bool testing;
    public bool armed;
    public bool DQNActive;
    private Vector3 relativeVector;
    public Vector3 TargetPosition;
    public Vector3 SelfPosition;
    private Vector3 _nextPoint;
    private Vector3 _oldPoint;

    class MovingObject
    {
        public Vector3 pos;
        public Vector2Int size;

        public MovingObject(Vector3 pos, Vector2Int size)
        {
            this.pos = pos;
            this.size = size;
        }
    }
    private List<MovingObject> movingObjects;

    private PID yAxisPID;
    public float Kp, Ki, Kd;

    private Rigidbody mrigid;
    private BehaviorParameters AIBehavior;
    public enum BehaviorType
    {
        DQN,
        AStar
    }

    public Vector3 MotorTorque;
    public float RotationTorque;
    public float YawTorque;
    private float initialTime;
    public float xAngleError;
    public float targetRotationY;
    public Vector3 TargetPoint;

    public delegate void EpisodeListener(string s);
    public EpisodeListener episodeListener;

    private Coroutine navigationCoroutine;

    private void Start()
    {
        mrigid = transform.GetComponent<Rigidbody>();
        movingObjects = new List<MovingObject>();
        AIBehavior = transform.GetComponent<BehaviorParameters>();
        //armed = false;
        DQNActive = false;
        yAxisPID = new PID(Kp, Ki, Kd);
        mrigid.maxLinearVelocity = 5;

    }

    private void Update()
    {
        yAxisPID.Kp = Kp;
        yAxisPID.Ki = Ki;
        yAxisPID.Kd = Kd;

        if (training)
        {
            if (Time.time - initialTime > 200)
            {
                float dist = Vector3.Distance(transform.localPosition, targetTansform.localPosition);
                SetReward(-1f - dist / 100);
                platformMeshRenderer.material = negativeRewardMat;
                episodeListener("Time");
                EndEpisode();
            }
        }
    }

    private void FixedUpdate()
    {
        if (armed)
        {
            Vector3 targetDirection = transform.position - _nextPoint;
            Vector3 rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360, 0.0f);
            Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);

            var localTarget = transform.InverseTransformPoint(_nextPoint);
            var targetAngle = localTarget.x / localTarget.magnitude;
            //Debug.Log(targetAngle);

            xAngleError = Mathf.DeltaAngle(targetRotation.eulerAngles.y, transform.rotation.eulerAngles.y);
            YawTorque = yAxisPID.Calculate(xAngleError, Time.fixedDeltaTime);
            mrigid.AddTorque(YawTorque * Vector3.up);
            mrigid.AddForce(transform.forward * 30f);

        }
        if (DQNActive)
        {
            Vector3 targetDirection = transform.position - TargetPoint;
            Vector3 rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360, 0.0f);
            Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);

            var localTarget = transform.InverseTransformPoint(_nextPoint);
            var targetAngle = localTarget.x / localTarget.magnitude;
            //Debug.Log(targetAngle);

            xAngleError = Mathf.DeltaAngle(targetRotation.eulerAngles.y, transform.rotation.eulerAngles.y);
            YawTorque = yAxisPID.Calculate(xAngleError, Time.fixedDeltaTime);
            mrigid.AddTorque(YawTorque * Vector3.up);
            mrigid.AddForce(transform.forward * 30f);
        }





    }

    public void SwitchBehavior(BehaviorType behavior)
    {
        if (!training)
        {
            if (behavior == BehaviorType.DQN)
            {
                AIBehavior.BehaviorType = Unity.MLAgents.Policies.BehaviorType.InferenceOnly;
                armed = false;
                DQNActive = true;
            }
            else if (behavior == BehaviorType.AStar)
            {
                AIBehavior.BehaviorType = Unity.MLAgents.Policies.BehaviorType.HeuristicOnly;
                DQNActive = false;
                SetSelfTarget(TargetPosition);
            }
        }
       
    }

    public void SetSelfTarget(Vector3 obj)
    {

        if (GridManager.Instance.TargetIsValid(obj))
        {

            TargetPosition = new Vector3(Mathf.FloorToInt(obj.x), 0.7f, Mathf.FloorToInt(obj.z));
            Debug.Log("target position is: " + TargetPosition);
            if (!armed)
            {
                _oldPoint = transform.position;
                if(navigationCoroutine != null)
                {
                    StopCoroutine(navigationCoroutine);
                }
                navigationCoroutine = StartCoroutine(MoveAirCraft());
                armed = true;
            }

        }

    }

    private IEnumerator MoveAirCraft()
    {
        
        while (Vector3.Distance(TargetPosition, transform.position) > 0.2f)
        {
            //yield return StartCoroutine(RotateAtStart());
            if (PathFinder.Instance.GetNextPoint(_oldPoint, TargetPosition, out _nextPoint))
            {

                while (DistanceXZ(_nextPoint, transform.position) > 0.7f && transform.position.z < _nextPoint.z)
                {
                    relativeVector = transform.InverseTransformPoint(_nextPoint);
                    //YawTorque = (relativeVector.x / relativeVector.magnitude);
                    //AckermannSteering(newSteer, 1, 0.7f, 0.5f);
                    yield return new WaitForSeconds(0.01f);
                }
                SelfPosition = transform.position;
                _oldPoint = _nextPoint;
            }
            else
            {
                
                break;
            }
        }

        SelfPosition = transform.position;
        armed = false;
        EndEpisode();
    }

    private float DistanceXZ(Vector3 vec1, Vector3 vec2)
    {
        float distx = Mathf.Abs(vec1.x - vec2.x);
        float distz = Mathf.Abs(vec1.z - vec2.z);

        return Mathf.Sqrt(distx * distx + distz * distz);
    }

    public override void OnEpisodeBegin()
    {
        float RandX = Random.Range(10f, 90f);
        initialTime = Time.time;
        if (!(AIBehavior.BehaviorType == Unity.MLAgents.Policies.BehaviorType.HeuristicOnly))
        {
            transform.localPosition = new Vector3(RandX, 1f, Random.Range(4, 35));
            targetTansform.localPosition = new Vector3(RandX, 1f, Random.Range(95f, 140f));
            obstacleTansform.localPosition = new Vector3(RandX + Random.Range(-6f, 6f), -5f, Random.Range(40f, 90f));
            obstacleTansform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        if (testing)
        {
            RandX = Random.Range(1f, 95f);
            transform.localPosition = new Vector3(RandX, 1f, Random.Range(1, 15));
            targetTansform.localPosition = new Vector3(Random.Range(50f, 70f), 1f, Random.Range(190f, 199f));
            obstacleTansform.localPosition = new Vector3( Random.Range(50f, 65f), -5f, Random.Range(135f, 155f));
            mrigid.velocity = Vector3.zero;
            mrigid.angularVelocity = Vector3.zero;
            DQNActive = false;
            obstacleTansform.GetComponent<RocketLauncherBehavior>().ResetSelf();
            SetSelfTarget(targetTansform.localPosition);
            SwitchBehavior(BehaviorType.AStar);
        }
        //if(!training)
        //{
        //    obstacleTansform.localPosition = new Vector3(Random.Range(51f, 60f), -5f, Random.Range(145f, 160f));
        //}

    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.localRotation.eulerAngles);
        sensor.AddObservation(targetTansform.localPosition);
        sensor.AddObservation(obstacleTansform.localPosition);
        //sensor.AddObservation(mrigid.velocity);
        //sensor.AddObservation(mrigid.angularVelocity);
        sensor.AddObservation(Vector3.Distance(transform.localPosition, targetTansform.localPosition));
        sensor.AddObservation(Vector3.Distance(transform.localPosition, obstacleTansform.localPosition));
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        //targetRotationY = Mathf.Clamp(actions.ContinuousActions[0] * 60f,-60,60);
        Vector3 deltaPoint = new Vector3(actions.ContinuousActions[0], 0, Mathf.Clamp(actions.ContinuousActions[1],0.1f,1f));
        if (training || DQNActive)
        {
            float dist = Vector3.Distance(transform.localPosition, targetTansform.localPosition);
            float obsdist = Vector3.Distance(transform.localPosition, obstacleTansform.localPosition);
            //AddReward((1000 / dist) + (obsdist / 1000));

            float moveSpeed = 1f;

            //transform.localEulerAngles = new Vector3(0, targetRotationY, 0);
            //transform.localPosition += deltaPoint * moveSpeed;
        }
        TargetPoint = transform.localPosition + deltaPoint;
        
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        //continuousActions[0] = Input.GetAxisRaw("Horizontal");
        //continuousActions[1] = Input.GetAxisRaw("Vertical");
        MotorTorque = transform.forward*30f;
        //RotationTorque = Input.GetAxisRaw("Horizontal");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            float dist = Vector3.Distance(transform.localPosition, targetTansform.localPosition);
            float obsdist = Vector3.Distance(transform.localPosition, obstacleTansform.localPosition);
            AddReward(-1 + (-dist / 10));// + (obsdist / 1000));
            platformMeshRenderer.material = negativeRewardMat;
            episodeListener("Wall");
            DQNActive = false;
        }
        if (other.CompareTag("Target"))
        {
            AddReward(100f);
            platformMeshRenderer.material = positiveRewardMat;
            episodeListener("Target");
        }
        StopCoroutine(navigationCoroutine);
        obstacleTansform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        obstacleTansform.GetComponent<RocketLauncherBehavior>().Used = false;
        armed = false;

        EndEpisode();
    }
}
