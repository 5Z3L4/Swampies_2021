using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Example.Prediction.Transforms;
using FishNet.Object;
using FishNet.Object.Prediction;
using TarodevController;
using UnityEngine;

public class PlayerMotor : NetworkBehaviour
{
    public FrameInput Input { get; private set; }
    private PlayerInput _input;

    public struct MoveData
    {
        public float Horizontal;
        public float Vertical;
        public bool JumpHeld;
        public bool Jump;

        public MoveData(float horizontal, float vertical, bool jumpHeld, bool jump)
        {
            Horizontal = horizontal;
            Vertical = vertical;
            JumpHeld = jumpHeld;
            Jump = jump;
        }
    }
    public struct ReconcileData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public float AngularVelocity;

        public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, float angularVelocity)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
        }
    }

    private MoveData _clientMoveData;
    private Vector3 _velocity;
    //true if subscribed to events
    private bool _subscribed;
    private Rigidbody2D _rigidbody;
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _input = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (IsServer || IsOwner)
        {
            Subscribe(true);
        }
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        Subscribe(false);
    }

    private void Subscribe(bool subscribe)
    {
        if (subscribe == _subscribed) return;
        if (TimeManager is null) return;

        _subscribed = subscribe;
        if (subscribe)
        {
            TimeManager.OnTick += TimeManager_OnTick;
            TimeManager.OnPostTick += TimeManager_OnPostTick;
            TimeManager.OnUpdate += TimeManager_OnFixedUpdate;
        }
        else
        {
            TimeManager.OnTick -= TimeManager_OnTick;
            TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }
    }
    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer)
    {
        transform.position = rd.Position;
        // transform.rotation = rd.Rotation;
        _rigidbody.velocity = rd.Velocity;
        _rigidbody.angularVelocity = rd.AngularVelocity;
    }
        
    private void TimeManager_OnTick()
    {
        if (IsOwner)
        {
            Reconciliation(default, false);
            CheckInput(out MoveData md);
            Move(md, false);
        }

        if (IsServer)
        {
            Move(default, true);
            ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _rigidbody.velocity, _rigidbody.angularVelocity);
            Reconciliation(rd, true);
        }
    }

    [Server]
    private void TimeManager_OnPostTick()
    {
        // if (IsServer)
        // {
        //     ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _rigidbody.velocity, _rigidbody.angularVelocity);
        //     Reconciliation(rd, true);
        // }
    }

    private void TimeManager_OnFixedUpdate()
    {
        if (IsOwner)
        {
            //_playerMovement.PlayerPressedJump(_clientMoveData.Jump);
            _playerMovement.RunCollisionChecks();
            //_playerMovement.CalculateJumpApex();
            //_playerMovement.CalculateGravity();
            //_playerMovement.CalculateJump(_clientMoveData.JumpHeld);
            _playerMovement.SetMoveClamp();
            _playerMovement.CalculateHorizontalMovement(_clientMoveData.Horizontal, (float)TimeManager.TickDelta);
            _playerMovement.MoveCharacter((float)TimeManager.TickDelta);
        }
    }

    [Replicate]
    private void Move(MoveData md, bool asServer, bool replaying = false)
    {
        if (asServer || replaying)
        {
            //here time track
            //_playerMovement.PlayerPressedJump(md.Jump);
            _playerMovement.RunCollisionChecks();
            //_playerMovement.CalculateJumpApex();
            //time multiplied
            //_playerMovement.CalculateGravity();
            //time track
            //_playerMovement.CalculateJump(md.JumpHeld);
            //hard to say
            _playerMovement.SetMoveClamp();
            //time
            _playerMovement.CalculateHorizontalMovement(md.Horizontal, (float)TimeManager.TickDelta);
            //time
            _playerMovement.MoveCharacter((float)TimeManager.TickDelta);
        }
        else
        {
            _clientMoveData = md;
        }
        
    }
    
    private void CheckInput(out MoveData md)
    {
        md = default;
        Input = _input.GatherInput();

        bool pressedJump = Input.JumpDown;
        bool jumpHeld = Input.JumpHeld;
        float horizontal = Input.X;
        float vertical = Input.Y;
        
        //if player is not moving, and not jumping we don't wanna update anything
        if (horizontal == 0f && !Input.JumpHeld && !pressedJump) return;
        md = new MoveData(horizontal, vertical, jumpHeld, pressedJump);
    }
}
