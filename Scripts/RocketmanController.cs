using System;
using DG.Tweening;
using UnityEngine;

public class RocketmanController : MonoBehaviour
{
    // velocity changes instantly. sometimes feels odd
    // polishing: set target velocity and change actual velocity towards target velocity in increments
    
    [SerializeField] private GameObject rocketmanAnimation;
    [SerializeField] private GameObject tiltParent;
    [SerializeField] private GameObject failAndRestartUI;
    
    [SerializeField] private float gravity = -0.02f; 
    [SerializeField] private float forwardLaunchVelocity = 2f;
    [SerializeField] private float forwardDeceleration= 0.02f;
    [SerializeField] private float forwardMinimumVelocity = 1f;
    [SerializeField] private float yLaunchVelocity = 3f;
    [SerializeField] private float rotationalVelocityMultiplier = 600f;
    [SerializeField] private float glidingYVelocity = -0.2f;
    [SerializeField] private float minimumY = 0.08f;
    [SerializeField] private float slowDownAfterFailMultiplier = 0.99f;
    [SerializeField] private float turningSpeedWhileGliding = 0.01f;
    [SerializeField] private float weakTrampolineBounceVelocity = 3.5f;
    [SerializeField] private float strongTrampolineBounceVelocity = 5f;

    [SerializeField] private float touchMovementOffsetAbsoluteMaximum = 550f;
    [SerializeField] private float touchMovementOffsetDivisor = 20f;
    [SerializeField] private float minimumMultiplier = 0.5f;
    [SerializeField] private float maximumMultiplier = 2f;
    [SerializeField] private float maximumRotationalSpeed = 2f;

    private bool _wingsAreOpen = false;
    private bool _launched = false;
    private bool _failed = false;
    private float _forwardVelocity;
    private float _yVelocity;
    
    private Vector3 _rocketmanParentStartingPosition;
    private Quaternion _rocketmanParentStartingRotation;
    private Quaternion _rocketmanAnimationStartingRotation;
    
    private Touch _playerTouch;
    private Vector2 _touchStartingPosition;
    private float _touchMovementOffset = 0f;

    private Animator _rocketmanAnimationController;
    private static readonly int Open = Animator.StringToHash("open");
    private static readonly int Close = Animator.StringToHash("close");


    private void Awake()
    {
        _rocketmanParentStartingPosition = transform.position;
        _rocketmanParentStartingRotation = transform.rotation;
        
        _rocketmanAnimationStartingRotation = rocketmanAnimation.transform.rotation;

        _rocketmanAnimationController = rocketmanAnimation.GetComponent<Animator>();
    }
    
    
    private void Update()
    {
        if (!_launched)
        {
            return;
        }
        
        if (Input.touchCount > 0 && !_failed) // glide
        {
            // todo: glide particles at the tip of the wings
            // add particle systems to wing.L_end and wing.R_end objects under rocketman animation.
            // enable/disable particles as wings open/close
            if (!_wingsAreOpen)
            {
                _rocketmanAnimationController.SetTrigger(Open);
                // todo: wings should open/close faster
                // bug: spam clicking makes wings stay open while falling
                _wingsAreOpen = true;
                rocketmanAnimation.transform.DOLocalRotate(endValue: new Vector3(90, 0, 0), duration: 0.15f);
            }
            _yVelocity = glidingYVelocity;
            // bug: this can override bounce velocity from trampolines and prevent players jump.
            // todo: maybe force end/cancel player touch when bouncing from trampolines, or ignore gliding input for some time

            _playerTouch = Input.GetTouch(0);
            switch (_playerTouch.phase)
            {
                case TouchPhase.Began:
                    _touchStartingPosition = _playerTouch.position;
                    break;
                case TouchPhase.Moved:
                    _touchMovementOffset = _touchStartingPosition.x - _playerTouch.position.x;
                    break;
                case TouchPhase.Stationary:
                    _touchMovementOffset = _touchStartingPosition.x - _playerTouch.position.x;
                    break;
                case TouchPhase.Ended:
                    _touchMovementOffset = 0f;
                    break;
                case TouchPhase.Canceled:
                    _touchMovementOffset = 0f;
                    break;
                default:
                    _touchMovementOffset = 0f;
                    throw new ArgumentOutOfRangeException(); // :-)
            }

            if (_touchMovementOffset > touchMovementOffsetAbsoluteMaximum)
            {
                _touchMovementOffset = touchMovementOffsetAbsoluteMaximum;
            }
            if (_touchMovementOffset < -touchMovementOffsetAbsoluteMaximum)
            {
                _touchMovementOffset = -touchMovementOffsetAbsoluteMaximum;
            }

            _touchMovementOffset /= touchMovementOffsetDivisor;
            // high speeds are ok but low speeds feel kinda slow and unresponsive
            // perhaps do if(math.abs(_touchMovementOffset) < minOffset) something something
            
            transform.Rotate(0, -_touchMovementOffset * turningSpeedWhileGliding, 0);
            tiltParent.transform.DOLocalRotate(new Vector3(0, 0, _touchMovementOffset), 0.05f);
            print("debug: current turning speed is " + _touchMovementOffset + "mult by" + turningSpeedWhileGliding);
            
        }
        else // fall
        {
            if (_wingsAreOpen)
            {
                _rocketmanAnimationController.SetTrigger(Close);
                _wingsAreOpen = false;
            }

            var velocityForRotation = _forwardVelocity;
            if (velocityForRotation > maximumRotationalSpeed)
            {
                velocityForRotation = maximumRotationalSpeed;
            }
            rocketmanAnimation.transform.Rotate(  velocityForRotation * rotationalVelocityMultiplier * Time.deltaTime, 0, 0);
        
            _yVelocity += gravity;
        }
        
        if (_failed)
        {
            _forwardVelocity *= slowDownAfterFailMultiplier;
        }
        else
        {
            _forwardVelocity -= forwardDeceleration;
            if (_forwardVelocity < forwardMinimumVelocity)
            {
                _forwardVelocity = forwardMinimumVelocity;
            }
        }
        
        var position = transform.position;
        var yPosition = Math.Max(minimumY, position.y + _yVelocity * Time.deltaTime);
        var xzMovement = position + transform.forward * (_forwardVelocity * Time.deltaTime);
        transform.position =  new Vector3(xzMovement.x, yPosition, xzMovement.z);
    }

    public void Launch(float pullAmount)
    {
        rocketmanAnimation.SetActive(true);
        
        var launchMultiplier = minimumMultiplier + pullAmount * maximumMultiplier;
        
        _forwardVelocity = forwardLaunchVelocity * launchMultiplier;
        _yVelocity = yLaunchVelocity * launchMultiplier;
        _launched = true;
    }

    public void Restart()
    {
        transform.position = _rocketmanParentStartingPosition;
        transform.rotation = _rocketmanParentStartingRotation;
        rocketmanAnimation.transform.rotation = _rocketmanAnimationStartingRotation;
        rocketmanAnimation.SetActive(false);
        _launched = false;
        _failed = false;
        _wingsAreOpen = false;
        _forwardVelocity = 0f;
        _yVelocity = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        // todo: prevent fail + jumps at the same trampoline ( eg. ignore other for 0.1 s after one happens)
        
        if (other.gameObject.CompareTag("floor"))
        {
            _failed = true;
            failAndRestartUI.SetActive(true);
            _yVelocity = Math.Abs(_yVelocity) / 2;
            
            // todo: prevent falling into trampolines, change movement direction to be away from trampoline center
        }
        
        if (other.gameObject.CompareTag("trampolineWeak"))
        {
            _yVelocity = weakTrampolineBounceVelocity;
            // could add forward velocity from trampoline bounces if we wanted to
        }
        
        if (other.gameObject.CompareTag("trampolineStrong"))
        {
            _yVelocity = strongTrampolineBounceVelocity;
        }
    }
}
