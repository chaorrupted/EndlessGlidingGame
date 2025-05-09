using System;
using Unity.VisualScripting;
using UnityEngine;


public class Launcher : MonoBehaviour
{
    [SerializeField] private RocketmanController rocketmanController;
    [SerializeField] private Follower mainCameraFollower;
    [SerializeField] private Animator stickAnimationController;
    [SerializeField] private GameObject stickTopperRocketman;
    
    [SerializeField] private float minimumPull = 0f;
    [SerializeField] private float maximumPull = 500f;
    
    private Touch _playerTouch;
    private Vector2 _startingPosition;
    private float _offset;
    private bool _waitForStickReleaseAnimation = false;
    
    
    private static readonly int Stretch = Animator.StringToHash("stretch");
    private static readonly int Release = Animator.StringToHash("release");
    private static readonly int Restart = Animator.StringToHash("restart");

    void Update()
    {
        if (_waitForStickReleaseAnimation)
        {
            if (CheckStickReleaseAnimationComplete())
            {
                _waitForStickReleaseAnimation = false;
                Launch();
            }

        }
        else if (Input.touchCount > 0)
        {
            _playerTouch = Input.GetTouch(0);
            switch (_playerTouch.phase)
            {
                case TouchPhase.Began:
                    _startingPosition = _playerTouch.position;
                    break;
                case TouchPhase.Moved:
                    ProcessTouchMoved();
                    break;
                case TouchPhase.Stationary:
                    break;
                case TouchPhase.Ended:
                    ProcessTouchEnded();
                    break;
                case TouchPhase.Canceled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void Launch()
    {
        stickTopperRocketman.SetActive(false);
        rocketmanController.Launch(_offset);
        mainCameraFollower.RotateAfterLaunch();
        this.GameObject().SetActive(false);
    }

    private bool CheckStickReleaseAnimationComplete()
    {
        return stickTopperRocketman.transform.position.z >= rocketmanController.transform.position.z;
    }

    private void ProcessTouchEnded()
    {
        stickAnimationController.SetTrigger(Release);
        _waitForStickReleaseAnimation = true;
    }

    private void ProcessTouchMoved()
    {
        _offset = _startingPosition.x - _playerTouch.position.x;
        if (_offset < minimumPull)
        {
            _offset = minimumPull;
        }
        if (_offset > maximumPull)
        {
            _offset = maximumPull;
        }

        _offset /= maximumPull;
        
        stickAnimationController.SetFloat(Stretch, _offset);
    }

    public void Reset()
    {
        stickTopperRocketman.SetActive(true);
        _offset = 0;
        _waitForStickReleaseAnimation = false;
        stickAnimationController.SetFloat(Stretch, 0);
        stickAnimationController.SetTrigger(Restart);
    }
}
