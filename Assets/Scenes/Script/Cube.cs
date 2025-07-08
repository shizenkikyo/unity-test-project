using UnityEngine;
using UnityEngine.InputSystem;

public class Cube : MonoBehaviour
{
    private enum STATE
    {
        IDLE,
        ROLLING
    }
    private STATE _state = STATE.IDLE;

    private float _angle = 0.0f;
    private Vector3 _axis;
    private Vector3 _pivot;
    private Vector3 _start;
    private Vector3 _end;

    private bool _isAutoMoving = false;
    private Vector3 _currentMoveDirection;
    private Vector3 _currentRotationAxis;

    private Vector3? _nextMoveDirection = null;
    private Vector3? _nextRotationAxis = null;

    [SerializeField]
    private float _rotateSpeed = 220f;

    void Update()
    {
        UpdateControl();
        UpdateRotation();
    }

    void UpdateControl()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        Vector3? newMoveDirection = null;
        Vector3? newRotationAxis = null;

        if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            newMoveDirection = Vector3.left;
            newRotationAxis = Vector3.forward;
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            newMoveDirection = Vector3.right;
            newRotationAxis = Vector3.back;
        }
        else if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            newMoveDirection = Vector3.forward;
            newRotationAxis = Vector3.right;
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            newMoveDirection = Vector3.back;
            newRotationAxis = Vector3.left;
        }

        if (newMoveDirection.HasValue)
        {
            if (_isAutoMoving && newMoveDirection.Value == _currentMoveDirection)
            {
                _isAutoMoving = false;
                _nextMoveDirection = null;
                _nextRotationAxis = null;
                return;
            }

            if (_state == STATE.ROLLING)
            {
                _nextMoveDirection = newMoveDirection.Value;
                _nextRotationAxis = newRotationAxis.Value;
            }
            else
            {
                StartAutoRoll(newMoveDirection.Value, newRotationAxis.Value);
            }
        }
    }

    void StartAutoRoll(Vector3 moveDirection, Vector3 rotationAxis)
    {
        _isAutoMoving = true;
        _currentMoveDirection = moveDirection;
        _currentRotationAxis = rotationAxis;
        StartRoll(moveDirection, rotationAxis);
    }

    void StartRoll(Vector3 moveDirection, Vector3 rotationAxis)
    {
        _state = STATE.ROLLING;
        _angle = 0.0f;
        _start = transform.position;
        _end = _start + moveDirection;
        _axis = rotationAxis;
        _pivot = _start + (moveDirection / 2.0f) + (Vector3.down / 2.0f);
    }

    void UpdateRotation()
    {
        if (_state != STATE.ROLLING)
        {
            if (_nextMoveDirection.HasValue)
            {
                StartAutoRoll(_nextMoveDirection.Value, _nextRotationAxis.Value);
                _nextMoveDirection = null;
                _nextRotationAxis = null;
            }
            else if (_isAutoMoving)
            {
                StartRoll(_currentMoveDirection, _currentRotationAxis);
            }
            return;
        }

        _angle += _rotateSpeed * Time.deltaTime;
        transform.RotateAround(_pivot, _axis, _rotateSpeed * Time.deltaTime);

        if (_angle >= 90.0f)
        {
            transform.position = _end;
            _state = STATE.IDLE;
        }
    }
}