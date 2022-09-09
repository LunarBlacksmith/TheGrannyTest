using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerMovement : MonoBehaviour
{
    #region Public Fields
    [Tooltip("Each element in this list represents two wheels - near side and offside." +
        "\nThese two wheels are theoretically operating on the same axel." +
        "\nNote: if one wheel is null/empty, force and other axle-related operations will still " +
        "be applied to the wheel that is not null.")]
    public List<AxleInfo> axles;
    [Tooltip("This value must be negative. " +
        "\nIt represents the maximum force at the wheels of the car when reversing. " +
        "\nTypically, this value is a fraction of the max forward torque. " +
        "\nDefault value is -50.")]
    public float maxReverseTorque = -50f;
    [Tooltip("The max angle each wheel on an axle will be able to turn. " +
        "\nTypically most cars have an average turning degree of +-30." +
        "WARNING: Anything above 45 degrees will cause unusual behaviour for a car!")]
    public float maxTurningAngle = 30f;
    #endregion
    #region Private Fields
    private float _motorTorque;
    private float _maxMotorTorque;
    private float _steeringAngle;
    private float _maxSteeringAngle;
    #endregion
    #region Public Properties
    // the max value the motor's torque can possibly reach. Left high range for testing.
    // 1.8L engines average at about 160 NM
    // high-end race cars average at about 1200 NM
    public float MaxMotorTorque 
    { 
        get { return _maxMotorTorque; } 
        // if the absolute of the value entered is less than or equal to 1000 (hardcoded max limit)
        // set the max possible torque to the value, otherwise set it to itself
        set { _maxMotorTorque = Mathf.Abs(value) <= 1000f ? value : _maxMotorTorque; }
    }
    // the actual torque value the motor is set to and therefore applied to the
    // wheels (colliders)
    public float MotorTorque
    {
        get { return _motorTorque; }
        private set 
        { 
            // if the value entered is greater than the max value torque can be set to
            // set the value to the max torque, otherwise set it as itself.
            value = value > MaxMotorTorque ? MaxMotorTorque : value;
            // if the value entered is greater than or equal to the reverse torque variable
            // AND it's less than or equal to 1000 (the hardcoded max torque limit)
            // set private motorTorque to the value, otherwise to itself.
            _motorTorque = (value >= maxReverseTorque && value <= 1000f) ? value : _motorTorque; 
        }
    }
    // the actual steering angle of the wheels on the axle
    public float SteeringAngle
    {
        get { return _steeringAngle; }
        // 
        private set { _steeringAngle = value;}
    }
    // the max angle each wheel on an axle will be able to turn
    // typically most cars have an average turning degree of +-30
    public float MaxSteeringAngle
    {
        get { return _maxSteeringAngle;}
        set 
        {
            // if the max turning angle value is a negative number (negative degrees)
            if (Mathf.Sign(maxTurningAngle) == -1)
            // then if the value passed is greater than or equal to the max turning angle, set it to the value
            // otherwise set it to itself.
            { _maxSteeringAngle = value >= maxTurningAngle ? value : _maxSteeringAngle; }
            // else if the max turning angle value is positive
            else
            // then if the absolute of the value passed is less than or equal to the (public, Editor-Accessible) max
            // turning angle allowed then set private steering angle variable to the value
            // otherwise set it to itself.
            { _maxSteeringAngle = Mathf.Abs(value) <= maxTurningAngle ? value : _maxSteeringAngle; }
        }
    }
    #endregion

    void Start()
    {
        // checking on the first frame of the game that the max reverse
        // torque allowed for a motor is not positive (since negative value
        // means opposite direction to forwards.)
        if (Mathf.Sign(maxReverseTorque) != -1)
        // if not, make it negative.
        { maxReverseTorque = -maxReverseTorque; }
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }
}
