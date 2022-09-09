using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider nsWheelCol;
    public WheelCollider osWheelCol;
    public bool hasMotor;
    public bool hasSteering;
}
