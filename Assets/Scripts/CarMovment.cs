using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CarMovment : MonoBehaviour
{
    #region Variables
    [Header("Character")]
    [Tooltip("Use this to apply the direction we are moving")]
    private Vector3 moveDir;
    private CharacterController charC;
    private Vector2 input;
    
    [SerializeField] private float gravity = 20f;
    // the current speed of the car
    [Header("Speed")]
    [SerializeField] private float _speed = 25f;
    // the maximum speed the car can go
    //[SerializeField] private float _maxSpeed = 50.0f;
    // speed added to the current speed as a percentage of the current speed
    //[SerializeField] private float _boostValue = 0.0f;
    // check if our boost multiplier value is higher than 0.0 (no extra speed)
    //[SerializeField] private bool _isBoosted = false;

    //[SerializeField] private Rigidbody _carRigidbody;

    private Vector3 _downDir;
    #endregion

    public float Speed { get { return _speed; } set { _speed = value; } }
    //public float BoostValue { get { return _boostValue; } set { _boostValue = value; } }

    // ------------------------------------------------------------------------------------------------


    void Start()
    {
        charC = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (_isGrounded)
        //{
        //    // adding any boost speed
        //    //_speed = _isBoosted ? (_speed += _boostValue * _speed) : _speed;

        //    if (Input.GetKeyDown(KeyCode.W))
        //    {
        //        // adding movement to car
        //        transform.Translate(Vector3.forward * Time.deltaTime * _speed);
        //    }
        //    if (Input.GetKeyDown(KeyCode.D))
        //    { transform.Translate(Vector3.right * Time.deltaTime * _speed); }
        //    if (Input.GetKeyDown(KeyCode.S))
        //    { transform.Translate(Vector3.back * Time.deltaTime * _speed); }
        //    if (Input.GetKeyDown(KeyCode.A))
        //    { transform.Translate(Vector3.left * Time.deltaTime * _speed); }
        //}
    }

    private void FixedUpdate()
    {
        if (charC.isGrounded)
        {
            /*Will be in moveDir z axis*/
            input.y = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            /*Will be in moveDir x axis*/
            input.x = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
            /*Moving according to our inputs and forward direction*/
            moveDir = transform.TransformDirection(new Vector3(input.x, 0, input.y));
            /*movement is affected by our speed*/
            moveDir *= Speed;
        }
        moveDir.y -= gravity;
        charC.Move(moveDir * Time.deltaTime);
    }

    // "other" collider is the object we are colliding with
    // this also de-parents the collided object
    private void OnCollisionEnter(Collision collision)
    {
        // check if other object has rigidbody (ragdoll or cubes)
            // if yes: YEET the collided object
            // if no: it means it's not yeetable, yeet ourselves with explosive force
    }

    private void YEET(GameObject objectToYeet_p)
    {
        // check if parent is scene (nothing)
            // if not: set parent of object to scene (nothing basically) - worldPositionStays parameter to true
        // yeet the child
    }

    
}