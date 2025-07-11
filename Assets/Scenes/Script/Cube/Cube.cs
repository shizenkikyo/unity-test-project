using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cube : MonoBehaviour
{
    [Header("回転設定")]
    [SerializeField] private float _rotatingSpeed = 0.8f;
    [SerializeField] private float _rotatingAngle = 90f;
    [SerializeField] private AnimationCurve _rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool _useSmoothEasing = true;
    [SerializeField] private float _easingStrength = 0.3f;
    
    [Header("音効果設定")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _rollSound;
    [SerializeField] private AudioClip _landSound;
    [SerializeField] private float _rollVolume = 0.5f;
    [SerializeField] private float _landVolume = 0.7f;
    [SerializeField] private bool _enableSound = true;
    
    [Header("衝突検出設定")]
    [SerializeField] private bool _enableCollisionDetection = true;
    [SerializeField] private LayerMask _obstacleLayer = 1 << 8; // Layer 8をデフォルトに設定
    [SerializeField] private float _collisionCheckDistance = 1.0f;
    
    private Vector3 _halfSize;
    private Vector3 _axis = Vector3.zero;
    private Vector3 _point = Vector3.zero;
    private bool _isRotating = false;
    
    private void Awake()
    {
        _halfSize = transform.localScale / 2f;
        
        // AudioSourceが設定されていない場合は自動で追加
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // AudioSourceの初期設定
        if (_audioSource != null)
        {
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
        }
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // 回転中は入力を無視
        if (_isRotating) return;
        
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // 入力に基づいて回転軸と回転中心を設定
        if (keyboard.upArrowKey.isPressed)
        {
            _axis = Vector3.right;
            _point = transform.position + new Vector3(0f, -_halfSize.y, _halfSize.z);
        }
        else if (keyboard.downArrowKey.isPressed)
        {
            _axis = Vector3.left;
            _point = transform.position + new Vector3(0f, -_halfSize.y, -_halfSize.z);
        }
        else if (keyboard.leftArrowKey.isPressed)
        {
            _axis = Vector3.forward;
            _point = transform.position + new Vector3(-_halfSize.x, -_halfSize.y, 0f);
        }
        else if (keyboard.rightArrowKey.isPressed)
        {
            _axis = Vector3.back;
            _point = transform.position + new Vector3(_halfSize.x, -_halfSize.y, 0f);
        }
        
        // 回転開始
        if (_point != Vector3.zero)
        {
            // 衝突チェック
            if (_enableCollisionDetection && CheckCollision())
            {
                Debug.Log("Collision detected - movement blocked");
                return;
            }
            
            PlayRollSound();
            StartCoroutine(StartRotate());
        }
    }
    
    private IEnumerator StartRotate()
    {
        _isRotating = true;
        float elapsedTime = 0f;
        float totalAngle = 0f;
        
        Debug.Log($"Starting rotation - Axis: {_axis}, Point: {_point}, Speed: {_rotatingSpeed}");
        
        while (elapsedTime < _rotatingSpeed)
        {
            // アニメーションカーブを使用して滑らかな回転
            float progress = elapsedTime / _rotatingSpeed;
            float curveValue = _rotationCurve.Evaluate(progress);
            
            // イージングを適用
            if (_useSmoothEasing)
            {
                curveValue = ApplyEasing(curveValue, _easingStrength);
            }
            
            // このフレームで回転する角度を計算
            float targetAngle = _rotatingAngle * curveValue;
            float angleThisFrame = targetAngle - totalAngle;
            
            if (angleThisFrame > 0)
            {
                transform.RotateAround(_point, _axis, angleThisFrame);
                totalAngle = targetAngle;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 最終的な角度調整（精度を保つため）
        float finalAngle = _rotatingAngle - totalAngle;
        if (finalAngle > 0)
        {
            transform.RotateAround(_point, _axis, finalAngle);
        }
        
        // 回転完了時の処理
        _axis = Vector3.zero;
        _point = Vector3.zero;
        _isRotating = false;
        
        // 着地音を再生
        PlayLandSound();
        
        Debug.Log("Rotation completed");
    }
    
    // イージング関数
    private float ApplyEasing(float t, float strength)
    {
        // 滑らかなイージング（SmoothStepの改良版）
        float eased = t * t * (3f - 2f * t);
        return Mathf.Lerp(t, eased, strength);
    }
    
    // 転がり音を再生
    private void PlayRollSound()
    {
        if (!_enableSound || _audioSource == null || _rollSound == null) return;
        
        _audioSource.clip = _rollSound;
        _audioSource.volume = _rollVolume;
        _audioSource.Play();
        
        Debug.Log("Playing roll sound");
    }
    
    // 着地音を再生
    private void PlayLandSound()
    {
        if (!_enableSound || _audioSource == null || _landSound == null) return;
        
        _audioSource.clip = _landSound;
        _audioSource.volume = _landVolume;
        _audioSource.Play();
        
        Debug.Log("Playing land sound");
    }
    
    // 音効果の設定をリセット
    [ContextMenu("Reset Audio Settings")]
    private void ResetAudioSettings()
    {
        if (_audioSource != null)
        {
            _audioSource.Stop();
            _audioSource.clip = null;
        }
        Debug.Log("Audio settings reset");
    }
    
    // 衝突検出
    private bool CheckCollision()
    {
        if (!_enableCollisionDetection) return false;
        
        // 現在の入力から移動方向を計算
        Vector3 moveDirection = Vector3.zero;
        
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.upArrowKey.isPressed)
                moveDirection = Vector3.forward;
            else if (keyboard.downArrowKey.isPressed)
                moveDirection = Vector3.back;
            else if (keyboard.leftArrowKey.isPressed)
                moveDirection = Vector3.left;
            else if (keyboard.rightArrowKey.isPressed)
                moveDirection = Vector3.right;
        }
        
        if (moveDirection.magnitude < 0.1f) return false;
        
        // 移動先の位置を計算
        Vector3 targetPosition = transform.position + moveDirection;
        
        // 衝突チェック
        Collider[] colliders = Physics.OverlapBox(targetPosition, Vector3.one * _collisionCheckDistance, transform.rotation, _obstacleLayer);
        
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Obstacle") || collider.GetComponent<Obstacle>() != null)
            {
                return true; // 衝突を検出
            }
        }
        
        return false;
    }
    
    // デバッグ用：Gizmosで回転軸と回転中心を表示
    private void OnDrawGizmosSelected()
    {
        if (_point != Vector3.zero)
        {
            // 回転中心を表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_point, 0.1f);
            
            // 回転軸を表示
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_point, _axis * 2f);
            
            // Cubeの中心から回転中心への線を表示
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _point);
        }
        
        // Cubeのサイズを表示
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // 衝突検出範囲を表示
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * _collisionCheckDistance * 2f);
    }
}