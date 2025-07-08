using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("追跡設定")]
    [SerializeField] private Transform target; // 追跡対象（Cube）
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -7); // カメラの相対位置
    [SerializeField] private float smoothSpeed = 5f; // 追跡の滑らかさ
    
    [Header("高度設定")]
    [SerializeField] private bool enableHeightAdjustment = true; // 高度調整を有効にするか
    [SerializeField] private float minHeight = 3f; // 最小高度
    [SerializeField] private float maxHeight = 8f; // 最大高度
    
    [Header("回転設定")]
    [SerializeField] private bool enableRotation = false; // カメラ回転を有効にするか
    [SerializeField] private float rotationSpeed = 2f; // 回転速度
    
    private Vector3 desiredPosition;
    private Vector3 smoothedPosition;
    
    void Start()
    {
        // ターゲットが設定されていない場合、シーン内のCubeを自動で探す
        if (target == null)
        {
            GameObject cube = GameObject.Find("Cube");
            if (cube != null)
            {
                target = cube.transform;
                Debug.Log("Cubeを自動で見つけて追跡対象に設定しました");
            }
            else
            {
                Debug.LogWarning("Cubeが見つかりません。手動でターゲットを設定してください。");
            }
        }
        
        // 初期位置を設定
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // 目標位置を計算
        desiredPosition = target.position + offset;
        
        // 高度調整
        if (enableHeightAdjustment)
        {
            float currentHeight = desiredPosition.y;
            float adjustedHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);
            desiredPosition.y = adjustedHeight;
        }
        
        // 滑らかな移動
        smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
        
        // カメラをターゲットに向ける
        if (target != null)
        {
            Vector3 lookDirection = target.position - transform.position;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
            }
        }
        
        // 回転機能（オプション）
        if (enableRotation)
        {
            HandleRotation();
        }
    }
    
    void HandleRotation()
    {
        // マウスの右クリックでカメラを回転
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
            
            // Y軸回転（水平回転）
            transform.RotateAround(target.position, Vector3.up, mouseX);
            
            // X軸回転（垂直回転）は制限を設ける
            Vector3 right = transform.right;
            transform.RotateAround(target.position, right, -mouseY);
            
            // カメラが下向きになりすぎないように制限
            Vector3 directionToTarget = target.position - transform.position;
            float angle = Vector3.Angle(transform.forward, Vector3.up);
            if (angle < 10f || angle > 170f)
            {
                transform.RotateAround(target.position, right, mouseY);
            }
        }
    }
    
    // ターゲットを動的に変更するメソッド
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    // オフセットを動的に変更するメソッド
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    // スムース速度を動的に変更するメソッド
    public void SetSmoothSpeed(float newSpeed)
    {
        smoothSpeed = newSpeed;
    }
    
    // デバッグ用：Gizmosで追跡範囲を表示
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, 0.5f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, target.position);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(desiredPosition, 0.3f);
        }
    }
} 