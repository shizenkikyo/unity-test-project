using UnityEngine;

public class JumpCameraFollow : MonoBehaviour
{
    [Header("追跡設定")]
    [SerializeField] private Transform _target; // 追跡対象（Player）
    [SerializeField] private Vector3 _offset = new Vector3(0, 2, -5); // カメラの相対位置
    [SerializeField] private float _smoothSpeed = 3f; // 追跡の滑らかさ
    
    [Header("ジャンプゲーム用設定")]
    [SerializeField] private bool _lockSideView = true; // サイドビューに固定するか
    [SerializeField] private float _minYOffset = 2f; // 最小Y軸オフセット
    [SerializeField] private float _maxYOffset = 8f; // 最大Y軸オフセット
    
    [Header("視点設定")]
    [SerializeField] private Vector3 _fixedRotation = new Vector3(0, 0, 0); // 固定回転角度（サイドビュー用）
    
    [Header("境界設定")]
    [SerializeField] private bool _enableBoundaries = true; // 境界を有効にするか
    [SerializeField] private float _minX = -10f; // 最小X座標
    [SerializeField] private float _maxX = 10f; // 最大X座標
    [SerializeField] private float _minY = 0f; // 最小Y座標
    [SerializeField] private float _maxY = 100f; // 最大Y座標
    
    private Vector3 _desiredPosition;
    private Vector3 _smoothedPosition;
    private Vector3 _initialOffset;
    
    void Start()
    {
        // ターゲットが設定されていない場合はエラーを出力
        if (_target == null)
        {
            Debug.LogError("JumpCameraFollow: _targetが設定されていません。InspectorでPlayerを設定してください。");
            return;
        }
        
        // 初期オフセットを保存
        _initialOffset = _offset;
        
        // 初期位置を設定
        Vector3 initialPosition = _target.position + _offset;
        transform.position = initialPosition;
        
        // 初期視点を設定（サイドビュー固定）
        transform.rotation = Quaternion.Euler(_fixedRotation);
    }
    
    void LateUpdate()
    {
        if (_target == null) return;
        
        // 目標位置を計算
        _desiredPosition = CalculateDesiredPosition();
        
        // 境界チェック
        if (_enableBoundaries)
        {
            _desiredPosition = ClampPosition(_desiredPosition);
        }
        
        // 滑らかな移動
        _smoothedPosition = Vector3.Lerp(transform.position, _desiredPosition, _smoothSpeed * Time.deltaTime);
        transform.position = _smoothedPosition;
        
        // サイドビューモードでは回転を固定
        if (_lockSideView)
        {
            transform.rotation = Quaternion.Euler(_fixedRotation);
        }
    }
    
    private Vector3 CalculateDesiredPosition()
    {
        Vector3 targetPosition = _target.position;
        Vector3 offset = _offset;
        
        // シンプルなY軸同期（X軸とZ軸は固定）
        if (_lockSideView)
        {
            // サイドビュー: Y軸のみ同期、X軸とZ軸は現在位置を維持
            targetPosition.x = transform.position.x;
            targetPosition.z = _target.position.z + _offset.z; // Z軸はPlayerとのoffsetを維持
            targetPosition.y = _target.position.y + _offset.y; // Y軸もPlayerとのoffsetを維持
            
            // 動的オフセット調整を無効化（固定オフセットを使用）
            offset = Vector3.zero;
        }
        else
        {
            // 通常モード: 全軸追跡（デフォルト動作）
            // Y軸の動的オフセット調整（プレイヤーの高さに応じて）
            float playerHeight = _target.position.y;
            float dynamicOffset = Mathf.Lerp(_minYOffset, _maxYOffset, playerHeight / 20f);
            offset.y = Mathf.Clamp(dynamicOffset, _minYOffset, _maxYOffset);
        }
        
        return targetPosition + offset;
    }
    
    private Vector3 ClampPosition(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, _minX, _maxX),
            Mathf.Clamp(position.y, _minY, _maxY),
            position.z
        );
    }
    
    // ターゲットを動的に変更するメソッド
    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
    }
    
    // オフセットを動的に変更するメソッド
    public void SetOffset(Vector3 newOffset)
    {
        _offset = newOffset;
        _initialOffset = newOffset;
    }
    
    // スムース速度を動的に変更するメソッド
    public void SetSmoothSpeed(float newSpeed)
    {
        _smoothSpeed = newSpeed;
    }
    
    // サイドビューモードを動的に変更するメソッド
    public void SetSideViewMode(bool lockSideView)
    {
        _lockSideView = lockSideView;
    }
    
    // カメラをリセットするメソッド
    public void ResetCamera()
    {
        if (_target != null)
        {
            Vector3 resetPosition = _target.position + _initialOffset;
            transform.position = resetPosition;
            
            // サイドビュー固定
            transform.rotation = Quaternion.Euler(_fixedRotation);
        }
    }
    
    // デバッグ用：Gizmosで追跡範囲を表示
    void OnDrawGizmosSelected()
    {
        if (_target != null)
        {
            // ターゲットの位置
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_target.position, 0.5f);
            
            // カメラからターゲットへの線
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _target.position);
            
            // 目標位置
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_desiredPosition, 0.3f);
            
            // 境界を表示
            if (_enableBoundaries)
            {
                Gizmos.color = Color.red;
                Vector3 minBound = new Vector3(_minX, _minY, transform.position.z);
                Vector3 maxBound = new Vector3(_maxX, _maxY, transform.position.z);
                Gizmos.DrawWireCube((minBound + maxBound) * 0.5f, maxBound - minBound);
            }
        }
    }
} 