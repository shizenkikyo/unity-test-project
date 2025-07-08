using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("追跡設定")]
    [SerializeField] private Transform _target; // 追跡対象（Cube）
    [SerializeField] private Vector3 _offset = new Vector3(0, 5, -7); // カメラの相対位置
    [SerializeField] private float _smoothSpeed = 5f; // 追跡の滑らかさ
    
    [Header("高度設定")]
    [SerializeField] private bool _enableHeightAdjustment = true; // 高度調整を有効にするか
    [SerializeField] private float _minHeight = 3f; // 最小高度
    [SerializeField] private float _maxHeight = 8f; // 最大高度
    
    [Header("回転設定")]
    [SerializeField] private bool _enableRotation = false; // カメラ回転を有効にするか
    [SerializeField] private float _rotationSpeed = 2f; // 回転速度
    
    private Vector3 _desiredPosition;
    private Vector3 _smoothedPosition;
    
    void Start()
    {
        // ターゲットが設定されていない場合、シーン内のCubeを自動で探す
        if (_target == null)
        {
            GameObject cube = GameObject.Find("Cube");
            if (cube != null)
            {
                _target = cube.transform;
                Debug.Log("Cubeを自動で見つけて追跡対象に設定しました");
            }
            else
            {
                Debug.LogWarning("Cubeが見つかりません。手動でターゲットを設定してください。");
            }
        }
        
        // 初期位置を設定
        if (_target != null)
        {
            transform.position = _target.position + _offset;
        }
    }
    
    void LateUpdate()
    {
        if (_target == null) return;
        
        // 目標位置を計算
        _desiredPosition = _target.position + _offset;
        
        // 高度調整
        if (_enableHeightAdjustment)
        {
            float currentHeight = _desiredPosition.y;
            float adjustedHeight = Mathf.Clamp(currentHeight, _minHeight, _maxHeight);
            _desiredPosition.y = adjustedHeight;
        }
        
        // 滑らかな移動
        _smoothedPosition = Vector3.Lerp(transform.position, _desiredPosition, _smoothSpeed * Time.deltaTime);
        transform.position = _smoothedPosition;
        
        // カメラをターゲットに向ける
        if (_target != null)
        {
            Vector3 lookDirection = _target.position - transform.position;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _smoothSpeed * Time.deltaTime);
            }
        }
        
        // 回転機能（オプション）
        if (_enableRotation)
        {
            HandleRotation();
        }
    }
    
    void HandleRotation()
    {
        // マウスの右クリックでカメラを回転
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * _rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * _rotationSpeed;
            
            // Y軸回転（水平回転）
            transform.RotateAround(_target.position, Vector3.up, mouseX);
            
            // X軸回転（垂直回転）は制限を設ける
            Vector3 right = transform.right;
            transform.RotateAround(_target.position, right, -mouseY);
            
            // カメラが下向きになりすぎないように制限
            Vector3 directionToTarget = _target.position - transform.position;
            float angle = Vector3.Angle(transform.forward, Vector3.up);
            if (angle < 10f || angle > 170f)
            {
                transform.RotateAround(_target.position, right, mouseY);
            }
        }
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
    }
    
    // スムース速度を動的に変更するメソッド
    public void SetSmoothSpeed(float newSpeed)
    {
        _smoothSpeed = newSpeed;
    }
    
    // デバッグ用：Gizmosで追跡範囲を表示
    void OnDrawGizmosSelected()
    {
        if (_target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_target.position, 0.5f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _target.position);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_desiredPosition, 0.3f);
        }
    }
} 