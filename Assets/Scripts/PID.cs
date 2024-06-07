using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PID
{
    public float Kp;
    public float Ki;
    public float Kd;
    private float previousError;
    private float p, i, d;
    public PID(float Kp,float Ki,float Kd)
    {
        this.Kp = Kp;
        this.Ki = Ki;
        this.Kd = Kd;
    }

    public float Calculate(float currentVal,float targetVal,float deltaTime)
    {
        //float error = targetVal - currentVal;
        // If Using angles
        float error = Mathf.DeltaAngle(currentVal, targetVal);
        p = error;
        i += error * deltaTime;
        d = (error - previousError) / deltaTime;
        previousError = error;

        return Kp * p + Ki * i + Kd * d;
    }

    public float Calculate(float error, float deltaTime)
    {
        //float error = targetVal - currentVal;
        p = error;
        i += error * deltaTime;
        d = (error - previousError) / deltaTime;
        previousError = error;

        return Kp * p + Ki * i + Kd * d;
    }
}
