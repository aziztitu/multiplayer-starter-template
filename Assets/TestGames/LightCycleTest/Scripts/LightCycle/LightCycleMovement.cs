using AZUtils;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;

public class LightCycleMovement : NetworkBehaviour
{
    private LightCycle lightCycle;
    private Rigidbody rb;
    private NetworkRigidbody networkRb;
    private ClientNetworkTransform networkTransform;

    [Header("Movement")]
    public float minMoveSpeed = 8f;
    public float maxMoveSpeed = 15f;
    public float maxYSpeed = 30f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    public float moveForce = 10f;
    public float turnSpeed = 100f;
    public float airTurnSpeedModifier = 0.5f;
    public float jumpForce = 500f;
    public float levelSpeed = 30f;
    public float angularVelocityLeveler = 0.9f;
    public float maxAngle = 45f;
    public float minAngle = -45f;
    public float groundCheckDistance = 1f;
    public float blockCheckDistance = 1f;
    public LayerMask groundCheckMask;

    [Header("References")]
    public Transform groundCheckFront;
    public Transform groundCheckBack;
    public Transform jumpForcePoint;
    public Transform frontOfTheBike;

    float moveSpeed = 0;
    bool canJump => lightCycle.HasPowerUp_Jump.Value;

    private void Awake()
    {
        lightCycle = GetComponentInParent<LightCycle>();
        rb = GetComponent<Rigidbody>();
        networkRb = GetComponent<NetworkRigidbody>();
        networkTransform = GetComponent<ClientNetworkTransform>();

        //rb.maxLinearVelocity = maxMoveSpeed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void Update()
    {
        //
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        if (LightCycleLevelManager.Instance.IsGameOver.Value)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        if (rb.isKinematic)
        {
            return;
        }

        if (!lightCycle.IsAlive.Value)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Move();
    }

    void Move()
    {
        var isGrounded = IsGrounded();
        var _input = lightCycle.inputs;

        // Compute the desired force
        Vector3 forceVector = transform.forward * maxMoveSpeed;
        forceVector.y = 0;
        rb.AddForce(forceVector * moveForce);

        // Compute desired rotation
        float turnInput = _input.move.x;
        var selectedTurnSpeed = turnSpeed;
        if (!isGrounded)
        {
            selectedTurnSpeed *= airTurnSpeedModifier;
        }
        Quaternion desiredRotation = rb.rotation * Quaternion.Euler(Vector3.up * turnInput * selectedTurnSpeed * Time.fixedDeltaTime);
        var euler = desiredRotation.eulerAngles;
        euler.x = NormalizeAngle(euler.x);
        euler.y = NormalizeAngle(euler.y);
        euler.z = NormalizeAngle(euler.z);

        euler.x = Mathf.MoveTowardsAngle(euler.x, 0f, levelSpeed * Time.fixedDeltaTime);
        if (euler.x < minAngle || euler.x > maxAngle)
        {
            euler.x = Mathf.Clamp(euler.x, minAngle, maxAngle);
            rb.angularVelocity *= angularVelocityLeveler;
        }
        euler.z = 0f;
        
        desiredRotation = Quaternion.Euler(euler);
        rb.MoveRotation(desiredRotation);

        // Jump
        if (_input.jump)
        {
            if (canJump && isGrounded)
            {
                lightCycle.ConsumePowerUpRpc(LightCycle.PowerUpType.Jump);
                //rb.AddForce(Vector3.up * jumpForce);
                rb.AddForceAtPosition(Vector3.up * jumpForce, jumpForcePoint.position);
            }
            _input.jump = false;
        }

        // Clamp Velocities
        rb.angularVelocity = new Vector3(rb.angularVelocity.x, 0, rb.angularVelocity.z);

        var targetMoveSpeed = HelperUtilities.Remap(_input.move.y, -1, 1, minMoveSpeed, maxMoveSpeed);
        var localLinearVelocity = transform.InverseTransformVector(rb.linearVelocity);
        moveSpeed = Mathf.Min(moveSpeed, localLinearVelocity.magnitude);
        if (targetMoveSpeed > moveSpeed)
        {
            moveSpeed += acceleration * Time.fixedDeltaTime;
            if (moveSpeed > targetMoveSpeed)
            {
                moveSpeed = targetMoveSpeed;
            }
        }
        else if (targetMoveSpeed < moveSpeed)
        {
            moveSpeed -= deceleration * Time.fixedDeltaTime;
            if (moveSpeed < targetMoveSpeed)
            {
                moveSpeed = targetMoveSpeed;
            }
        }

        var horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > moveSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * moveSpeed;
        }

        var velocityY = rb.linearVelocity.y;
        if (Mathf.Abs(velocityY) > maxYSpeed)
        {
            velocityY = Mathf.Sign(velocityY) * maxYSpeed;
        }
        rb.linearVelocity = new Vector3(horizontalVelocity.x, velocityY, horizontalVelocity.z);
    }

    // Basic grounded check using a raycast
    bool IsGrounded()
    {
        var front = Physics.Raycast(groundCheckFront.position, Vector3.down, groundCheckDistance, groundCheckMask);
        if (front)
        {
            return true;
        }

        return Physics.Raycast(groundCheckBack.position, Vector3.down, groundCheckDistance, groundCheckMask);
    }

    bool IsBlocked()
    {
        // Raycast forward a short distance
        float checkDistance = 1.0f;
        return Physics.Raycast(frontOfTheBike.position, transform.forward, checkDistance, groundCheckMask);
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }

    // Called on the owner
    public async UniTask Teleport(Vector3 position, Quaternion rotation)
    {
        rb.isKinematic = true;
        networkRb.enabled = false;

        //rb.position = position;
        //rb.rotation = rotation;

        networkTransform.Teleport(position, rotation, networkTransform.GetScale());

        await UniTask.Delay(1000);

        rb.isKinematic = false;
        networkRb.enabled = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
