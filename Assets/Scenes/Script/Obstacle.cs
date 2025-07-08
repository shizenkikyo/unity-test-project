using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("障害物設定")]
    [SerializeField] private bool _isSolid = true;
    [SerializeField] private bool _isVisible = true;
    [SerializeField] private Color _obstacleColor = Color.red;
    
    [Header("物理設定")]
    [SerializeField] private bool _useRigidbody = false;
    [SerializeField] private bool _isKinematic = true;
    
    [Header("視覚効果")]
    [SerializeField] private bool _showWarning = true;
    [SerializeField] private Color _warningColor = Color.yellow;
    [SerializeField] private float _pulseSpeed = 2f;
    
    private Renderer _obstacleRenderer;
    private Material _originalMaterial;
    private bool _isPulsing = false;
    
    private void Start()
    {
        SetupObstacle();
    }
    
    private void SetupObstacle()
    {
        // レンダラーの設定
        _obstacleRenderer = GetComponent<Renderer>();
        if (_obstacleRenderer != null)
        {
            _originalMaterial = _obstacleRenderer.material;
            if (_isVisible)
            {
                SetupMaterial();
            }
        }
        
        // 物理設定
        SetupPhysics();
        
        // コライダーの確認
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            // コライダーがない場合は追加
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = !_isSolid;
        }
        else
        {
            collider.isTrigger = !_isSolid;
        }
    }
    
    private void SetupMaterial()
    {
        if (_obstacleRenderer == null) return;
        
        // 新しいマテリアルを作成
        Material material = new Material(Shader.Find("Standard"));
        material.color = _obstacleColor;
        
        // グリッドテクスチャを追加
        material.mainTexture = CreateGridTexture();
        material.mainTextureScale = new Vector2(2f, 2f);
        
        _obstacleRenderer.material = material;
    }
    
    private Texture2D CreateGridTexture()
    {
        int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        Color gridColor = Color.white;
        Color backgroundColor = _obstacleColor;
        
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                bool isGridLine = (x % (textureSize / 8) == 0) || (y % (textureSize / 8) == 0);
                texture.SetPixel(x, y, isGridLine ? gridColor : backgroundColor);
            }
        }
        
        texture.Apply();
        return texture;
    }
    
    private void SetupPhysics()
    {
        if (_useRigidbody)
        {
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody>();
            }
            rigidbody.isKinematic = _isKinematic;
            rigidbody.useGravity = !_isKinematic;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Cube"))
        {
            // Cubeとの衝突を検出
            Debug.Log($"Obstacle collision detected with: {other.name}");
            
            if (_showWarning)
            {
                StartPulse();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Cube"))
        {
            // 衝突が終了
            StopPulse();
        }
    }
    
    private void StartPulse()
    {
        if (!_isPulsing)
        {
            _isPulsing = true;
            StartCoroutine(PulseEffect());
        }
    }
    
    private void StopPulse()
    {
        _isPulsing = false;
        if (_obstacleRenderer != null && _originalMaterial != null)
        {
            _obstacleRenderer.material = _originalMaterial;
        }
    }
    
    private System.Collections.IEnumerator PulseEffect()
    {
        while (_isPulsing)
        {
            float pulse = Mathf.Sin(Time.time * _pulseSpeed) * 0.5f + 0.5f;
            Color pulseColor = Color.Lerp(_obstacleColor, _warningColor, pulse);
            
            if (_obstacleRenderer != null)
            {
                _obstacleRenderer.material.color = pulseColor;
            }
            
            yield return null;
        }
    }
    
    // 障害物の有効/無効を切り替え
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
    
    // 障害物の色を変更
    public void SetColor(Color color)
    {
        _obstacleColor = color;
        if (_obstacleRenderer != null)
        {
            _obstacleRenderer.material.color = color;
        }
    }
    
    // デバッグ用：Gizmosで障害物の範囲を表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _isSolid ? Color.red : Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // 衝突範囲を表示
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale * 1.1f);
    }
} 