using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;
    public float SuccessPercentage = 0;
    public int Trials = 0;
    public int SuccessfullTrials = 0;
    public int FailedTrials = 0;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        AircraftAgent agent = GameObject.FindWithTag("Player").GetComponent<AircraftAgent>();
        agent.episodeListener += updateStats;
        
    }

    private void updateStats(string s)
    {
        if(s == "Wall")
        {
            FailedTrials += 1;
        }
        else if(s == "Target")
        {
            SuccessfullTrials += 1;
        }
        else { FailedTrials += 1;}

        Trials += 1;

        SuccessPercentage = (float)SuccessfullTrials / Trials * 100f;

        Debug.Log("Success Percentage is : " + SuccessPercentage.ToString());
    }
}
