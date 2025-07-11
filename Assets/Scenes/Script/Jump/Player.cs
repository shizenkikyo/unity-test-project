using UnityEngine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.TouchPhase;

public class Player : MonoBehaviour
{
    [Header("ジャンプ設定")]
    [SerializeField] private float _jumpForce = 8f;
    [SerializeField] private float _maxJumpForce = 12f;
    [SerializeField] private float _jumpHoldMultiplier = 1.5f;
    [SerializeField] private float _jumpHoldTime = 0.3f;
    
    [Header("重力設定")]
    [SerializeField] private float _gravity = 20f;
    [SerializeField] private float _maxFallSpeed = 15f;
    
    [Header("回転設定")]
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _maxRotationAngle = 45f;
    [SerializeField] private float _minRotationAngle = -90f;
    
    [Header("音効果設定")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _jumpSound;
    [SerializeField] private AudioClip _fallSound;
    [SerializeField] private float _jumpVolume = 0.7f;
    [SerializeField] private float _fallVolume = 0.5f;
    [SerializeField] private bool _enableSound = true;
    
    [Header("パーティクル効果")]
    [SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private ParticleSystem _fallParticles;
    
    // プライベート変数
    private Rigidbody _rigidbody;
    private bool _isJumping = false;
    private bool _isHoldingJump = false;
    private float _jumpHoldTimer = 0f;
    private Vector3 _startPosition;
    
    // イベント
    public System.Action OnPlayerDied;
    public System.Action OnPlayerJumped;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        // AudioSourceの設定
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Rigidbodyの初期設定
        SetupRigidbody();
        
        // 初期位置を保存
        _startPosition = transform.position;
    }
    
    private void SetupRigidbody()
    {
        _rigidbody.useGravity = false; // カスタム重力を使用
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rigidbody.linearDamping = 0f;
        _rigidbody.angularDamping = 0f;
    }
    
    private void Update()
    {
        HandleInput();
        ApplyGravity();
        UpdateRotation();
        UpdateJumpHold();
    }
    
    private void HandleInput()
    {
        // キーボード入力
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                StartJump();
            }
            
            if (keyboard.spaceKey.isPressed)
            {
                _isHoldingJump = true;
            }
            else
            {
                _isHoldingJump = false;
            }
        }
        
        // マウス入力
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            StartJump();
        }
        
        // タッチ入力（モバイル）
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                StartJump();
            }
        }
    }
    
    private void StartJump()
    {
        if (_isJumping) return;
        
        _isJumping = true;
        _isHoldingJump = true;
        _jumpHoldTimer = 0f;
        
        // ジャンプ力を適用
        float jumpPower = _jumpForce;
        _rigidbody.linearVelocity = new Vector3(0, jumpPower, 0);
        
        // 音効果
        PlayJumpSound();
        
        // パーティクル効果
        if (_jumpParticles != null)
        {
            _jumpParticles.Play();
        }
        
        // イベント通知
        OnPlayerJumped?.Invoke();
        
        Debug.Log($"Jump started with force: {jumpPower}");
    }
    
    private void UpdateJumpHold()
    {
        if (_isHoldingJump && _jumpHoldTimer < _jumpHoldTime)
        {
            _jumpHoldTimer += Time.deltaTime;
            
            // ホールド中は追加のジャンプ力を適用
            float additionalForce = _jumpHoldMultiplier * Time.deltaTime;
            Vector3 currentVelocity = _rigidbody.linearVelocity;
            currentVelocity.y = Mathf.Min(currentVelocity.y + additionalForce, _maxJumpForce);
            _rigidbody.linearVelocity = currentVelocity;
        }
    }
    
    private void ApplyGravity()
    {
        if (_rigidbody == null) return;
        
        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.y -= _gravity * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, -_maxFallSpeed);
        _rigidbody.linearVelocity = velocity;
        
        // 地面に着地したかチェック
        if (velocity.y <= 0 && transform.position.y <= _startPosition.y + 0.1f)
        {
            if (_isJumping)
            {
                _isJumping = false;
                PlayFallSound();
                
                if (_fallParticles != null)
                {
                    _fallParticles.Play();
                }
            }
        }
    }
    
    private void UpdateRotation()
    {
        if (_rigidbody == null) return;
        
        float targetRotation = 0f;
        float velocityY = _rigidbody.linearVelocity.y;
        
        // 速度に基づいて回転角度を決定
        if (velocityY > 0)
        {
            // 上昇中
            targetRotation = _maxRotationAngle;
        }
        else if (velocityY < 0)
        {
            // 下降中
            targetRotation = _minRotationAngle;
        }
        
        // 現在の回転を取得
        Vector3 currentRotation = transform.eulerAngles;
        float currentZRotation = currentRotation.z;
        
        // 角度を-180から180の範囲に正規化
        if (currentZRotation > 180f)
        {
            currentZRotation -= 360f;
        }
        
        // 滑らかな回転
        float newZRotation = Mathf.Lerp(currentZRotation, targetRotation, _rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(currentRotation.x, currentRotation.y, newZRotation);
    }
    
    private void PlayJumpSound()
    {
        if (!_enableSound || _audioSource == null || _jumpSound == null) return;
        
        _audioSource.clip = _jumpSound;
        _audioSource.volume = _jumpVolume;
        _audioSource.Play();
    }
    
    private void PlayFallSound()
    {
        if (!_enableSound || _audioSource == null || _fallSound == null) return;
        
        _audioSource.clip = _fallSound;
        _audioSource.volume = _fallVolume;
        _audioSource.Play();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // 障害物との衝突
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Die();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // トリガーとの衝突（スコアアイテムなど）
        if (other.CompareTag("ScoreItem"))
        {
            // スコア加算などの処理
            Debug.Log("Score item collected!");
        }
    }
    
    private void Die()
    {
        Debug.Log("Player died!");
        
        // ゲームオーバー処理
        OnPlayerDied?.Invoke();
        
        // プレイヤーを無効化
        enabled = false;
        _rigidbody.isKinematic = true;
    }
    
    public void ResetPlayer()
    {
        // プレイヤーを初期位置にリセット
        transform.position = _startPosition;
        transform.rotation = Quaternion.identity;
        
        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = false;
        }
        
        _isJumping = false;
        _isHoldingJump = false;
        _jumpHoldTimer = 0f;
        
        enabled = true;
        
        Debug.Log("Player reset to start position");
    }
    
    // デバッグ用のGizmos
    private void OnDrawGizmosSelected()
    {
        // ジャンプ範囲の可視化
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // 重力の方向を可視化
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * 2f);
    }
    
    // パブリックプロパティ
    public bool IsJumping => _isJumping;
    public bool IsHoldingJump => _isHoldingJump;
    public float CurrentVelocityY => _rigidbody != null ? _rigidbody.linearVelocity.y : 0f;
    public Vector3 Position => transform.position;
} 