using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Public Fields
    [Tooltip("Each element in this list represents two wheels - near side and offside." +
        "\nThese two wheels are theoretically operating on the same axel." +
        "\nNote: if one wheel is null/empty, force and other axle-related operations will still " +
        "be applied to the wheel that is not null.")]
    public List<AxleInfo> axles;
    #endregion
    #region Private Fields
    //[Tooltip("DELETE MY SERIALIZEDFIELD TAG WHEN YOU FINISH TESTING THE MAX TORQUE LIMIT, I AM A TESTER VARIABLE ONLY.\nEND ME. PLEASE.")]
    //[SerializeField] private float _testerMaxMotorTorque = 1000f;
    //[Tooltip("DELETE MY SERIALIZEDFIELD TAG WHEN YOU FINISH TESTING THE MAX TURNING ANGLE LIMIT, I AM A TESTER VARIABLE ONLY.")]
    //[SerializeField] private float _testerMaxTurningAngle = 90f;

    
    //[SerializeField] private float maxReverseTorque = -50f;
    //[SerializeField] private float maxTurningAngle = 30f;

    [Tooltip("This value represents how many axles (each axle having two wheels) there are on the vehicle." +
        "\nTypically, unless it's a lorry or construction vehicle, they will have two axles. " +
        "Bi-wheeled vehicles would have 1 axle, containing only the front and rear wheels." +
        "\nThe default value is 2.")]
    [Range(0, 1000)]
    [SerializeField] private ushort axleCount = 2;

    [Tooltip("This variable is the index value of the child of the Wheel Collider game objects " +
        "where the child game object is holding the visual aspect of the wheel." +
        "\nFor example, if WheelCollider game object has 3 children ordered in the hierarchy like so:" +
        "\n- Hub \n- VisualWheel \n- Bolts" +
        "\nThen wheelColliderChildIndex has a value of 1 because VisualWheel is the second child in the array." +
        "\nWARNING: All wheel colliders must therefore have the visual wheels all in the same child index spot!")]
    [SerializeField] private ushort wheelColliderChildIndex = 0;

    // actual value used for setting the motor's max torque on the wheel colliders
    [Tooltip("This value represents the maximum amount of force (torque) the wheel colliders " +
        "are allowed to reach." +
        "\n1.8L engines average at about 160 NM (which is the default value), and high-end " +
        "race cars average at about 1200 NM")]
    [Range(0f, 1500f)]
    [SerializeField] private float _maxMotorTorque = 160f;

    // actual value used for setting the motor's max torque in a backwards direction on the wheel colliders
    [Tooltip("This value must be negative or 0." +
        "\nIt represents the maximum force at the wheels of the car when reversing. " +
        "\nTypically, this value is a fraction of the max forward torque. " +
        "\nDefault value is -50.")]
    [Range(-100f, 0f)]
    [SerializeField] private float _maxReverseTorque = -50f;

    // actual value used to set the max wheel turning angles
    [Tooltip("This value must be positive or 0." +
        "\nThe max angle each wheel on an axle will be able to turn. " +
        "\nTypically most cars have an average turning degree of +-30." +
        "\nWARNING: Anything above 45 degrees will cause unusual behaviour for a car!")]
    [Range(0f, 90f)]
    [SerializeField] private float _maxSteeringAngle = 30f;

    private float _motorTorque;     // actual value used to set the motor's torque value on the wheel colliders
    private float _reverseTorque;   // actual value used for setting the motor's current torque in a backwards direction
    private float _steeringAngle;   // actual value used for calulating current wheel turn angles
    
    private Transform _visualWheelTransform;    // used to apply visual changes to the wheel model when the wheel collider is changing
    private Vector3 _visualWheelPos;            // used to apply visual changes to the wheel model when the wheel collider is changing
    private Quaternion _visualWheelRot;         // used to apply visual changes to the wheel model when the wheel collider is changing
    #endregion
    #region Public Properties
    // the max value the motor's torque can possibly reach.
    // 1.8L engines average at about 160 NM
    // high-end race cars average at about 1200 NM
    public float MaxMotorTorque 
    { get { return _maxMotorTorque; } set { _maxMotorTorque = value;  } }
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
            // AND it's less than or equal to _testerMaxMotorTorque (the hardcoded max torque limit)
            // set private motorTorque to the value, otherwise to itself.
            // (We are clamping the max reverse torque between -100 and 0 in Start() and OnValidate())
            _motorTorque = (value >= MaxReverseTorque && value <= MaxMotorTorque) ? value : _motorTorque; 
        }
    }
    // the actual steering angle of the wheels on the axle
    public float SteeringAngle
    {
        get { return _steeringAngle; }
        // 
        set 
        {
            // if the value passed is a negative number
            if (value == -1)
            // check if the value is greater than or equal to the minimum angle (the negative sign
            // of the max turning angle)
            // if it is, set it to the value, otherwise set it to the max angle it can go in that direction
            { _steeringAngle = value >= -MaxSteeringAngle ? value : -MaxSteeringAngle; }
            // else if the value passed is 0 or a positive number
            else
            // check if the value is less than or equal to the max angle (the positive sign
            // of the max turning angle)
            // if it is, set it to the value, otherwise set it to the max angle it can go in that direction
            { _steeringAngle = value <= MaxSteeringAngle ? value : MaxSteeringAngle; }
        }
    }
    // the max angle each wheel on an axle will be able to turn
    // typically most cars have an average turning degree of +-30
    // This will never be negative
    public float MaxSteeringAngle
    {
        get { return _maxSteeringAngle;}
        private set 
        {
            _maxSteeringAngle = value;
            // then if the absolute of the value passed is less than or equal to the (Editor-Accessible) max
            // turning angle allowed
            // AND the value is greater than or equal to 0 (since the turning angle MUST be positive)
            // then set private steering angle variable to the value, otherwise set it to itself.
            // (Mathf.Abs() is not neccessary here but I have left it in as a failsafe.)
             //_maxSteeringAngle = (Mathf.Abs(value) <= _maxSteeringAngle && value >= 0f) ? value : _maxSteeringAngle; 
        }
    }
    // Reverse Torque can never be positive since it represents the oppostive direction
    public float ReverseTorque
    {
        get { return _reverseTorque; }
        // if the value passed is less than 0 (since reverse torque can never be positive)
        // AND the value is greater than or equal to the maximum value it can go (remember the max is negative)
        // then set it to the value, otherwise set it to itself
        set { _reverseTorque = (value <= 0 && value >= MaxReverseTorque) ? value : _reverseTorque; }
    }
    // Max Reverse Torque can never be positive since it represents the opposite direction
    public float MaxReverseTorque 
    { 
        get { return _maxReverseTorque; } 
        // if the value is less than or equal to 0
        // set the private max reverse torque to the value, otherwise set it to itself
        set { _maxReverseTorque =  value <= 0 ? value : _maxReverseTorque; }
    }
    #endregion

    void Start()
    {
        // preventing the max reverse torque being less than -100 NM or above 0 NM
        MaxReverseTorque = Mathf.Clamp(MaxReverseTorque, _maxReverseTorque, 0f);

        MaxSteeringAngle = Mathf.Clamp(MaxSteeringAngle, 0f, _maxSteeringAngle);
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // if the value of the directional input we're getting is less than 0
        // meaning going backwards. (remember Input.GetAxis() is between 1 and 0 either side)
        if (Input.GetAxis("Vertical") < 0)
        // multiply our maximum backwards torque (reverse) by the -1 to 0 value to get
        // the actual torque for that direction. (Changing the Input sign to positive (+) because
        // Reverse torque is negative and so is the input, and a backwards motion is negative not the
        // resulting positive it would be without first changing the Input to positive.
        { MotorTorque = MaxReverseTorque * -(Input.GetAxis("Vertical"));}
        else
        // else if the Input value is positive
        // multiply our max motor torque for forward motion by the Input value (between 1 and 0)
        // to get the actual torque value
        { MotorTorque = MaxMotorTorque * Mathf.Abs(Input.GetAxis("Vertical")); }

        // setting the actual steering angle to the maximum value it can be multiplied
        // by the 1 to 0 value either side from the Horizontal axis to result in
        // the actual steering angle used.
        SteeringAngle = MaxSteeringAngle * Input.GetAxis("Horizontal");

        // a for loop to cycle through the axles (determined by axleCount)
        for (int loopCount = 0; loopCount < axleCount-1; ++loopCount)
        {
            // if axle x has the Steering boolean true
            if (axles[loopCount].hasSteering)
            {
                // set axle x's offside and nearside wheel's steer angles to our
                // previously calculated SteeringAngle property
                axles[loopCount].nsWheelCol.steerAngle = SteeringAngle;
                axles[loopCount].osWheelCol.steerAngle = SteeringAngle;
            }
            // set axle x's offside and nearside wheel's torque to our
            // previously calculated MotorTorque property
            if (axles[loopCount].hasMotor)
            {
                axles[loopCount].nsWheelCol.motorTorque = MotorTorque;
                axles[loopCount].osWheelCol.motorTorque = MotorTorque;
            }
            ApplyTransformToVisualWheels(axles[loopCount].nsWheelCol);
            ApplyTransformToVisualWheels(axles[loopCount].osWheelCol);
        }
    }

    private void OnValidate()
    {
        // preventing the max reverse torque ever becoming less than -100 NM
        // or above 0 NM when changing the value in the Editor
        //MaxReverseTorque = Mathf.Clamp(MaxReverseTorque, _maxReverseTorque, 0f);

        // preventing the max turning angle ever becoming more than 90 degrees
        // when changing the value in the Editor
        //MaxSteeringAngle = Mathf.Clamp(MaxSteeringAngle, 0f, _maxSteeringAngle);

        // max reverse torque can't be negative
        //if (Mathf.Sign(maxReverseTorque) != -1)
        //{ maxReverseTorque = -maxReverseTorque; }
    }

    public void ApplyTransformToVisualWheels(WheelCollider wheelCol_p)
    {
        // if there are no children under the Wheel Collider game object
        // return out of the method because there's nothing to apply the Transform to
        if(wheelCol_p.transform.childCount < 1) { return; }

        // setting the private visual wheel Transform variable to the Transform of the
        // actual game object's visual wheel, where the visual wheel is a child in the Editor
        // hierarchy of Wheel Collider game object.
        _visualWheelTransform = wheelCol_p.transform.GetChild(wheelColliderChildIndex);
        
        // resetting the position and rotation variables before they are re-applied to avoid bugs
        _visualWheelPos = Vector3.zero;
        _visualWheelRot = Quaternion.identity;

        wheelCol_p.GetWorldPose(out _visualWheelPos, out _visualWheelRot);
        _visualWheelTransform.transform.position = _visualWheelPos;
        _visualWheelTransform.transform.rotation = _visualWheelRot;
    }
}
