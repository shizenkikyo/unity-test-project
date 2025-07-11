using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
    [Header("床の設定")]
    [SerializeField] private Vector3 _floorSize = new Vector3(20f, 1f, 20f);
    [SerializeField] private Vector3 _floorPosition = new Vector3(0f, 0f, 0f);
    [SerializeField] private Material _floorMaterial;
    
    [Header("床の見た目")]
    [SerializeField] private Color _floorColor = Color.gray;
    [SerializeField] private bool _addGridTexture = true;
    [SerializeField] private float _gridSize = 1f;
    
    [Header("物理設定")]
    [SerializeField] private bool _addRigidbody = false;
    [SerializeField] private bool _isKinematic = true;
    
    private GameObject _floorObject;
    
    void Start()
    {
        CreateFloor();
    }
    
    void CreateFloor()
    {
        // 床オブジェクトの作成
        _floorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _floorObject.name = "Floor";
        
        // 位置とサイズの設定
        _floorObject.transform.position = _floorPosition;
        _floorObject.transform.localScale = _floorSize;
        
        // マテリアルの設定
        SetupMaterial();
        
        // 物理設定
        SetupPhysics();
        
        // レイヤー設定（地面チェック用）
        _floorObject.layer = LayerMask.NameToLayer("Default");
        
        Debug.Log("床を作成しました: " + _floorObject.name);
    }
    
    void SetupMaterial()
    {
        Renderer renderer = _floorObject.GetComponent<Renderer>();
        
        if (_floorMaterial != null)
        {
            renderer.material = _floorMaterial;
        }
        else
        {
            // デフォルトマテリアルの作成
            Shader standardShader = Shader.Find("Standard");
            if (standardShader == null)
            {
                // Standardシェーダーが見つからない場合は、Built-inシェーダーを使用
                standardShader = Shader.Find("Legacy Shaders/Diffuse");
            }
            
            if (standardShader == null)
            {
                // それでも見つからない場合は、デフォルトのシェーダーを使用
                standardShader = Shader.Find("Sprites/Default");
            }
            
            Material material = new Material(standardShader);
            material.color = _floorColor;
            
            if (_addGridTexture)
            {
                material.mainTexture = CreateGridTexture();
                material.mainTextureScale = new Vector2(_floorSize.x / _gridSize, _floorSize.z / _gridSize);
            }
            
            renderer.material = material;
        }
    }
    
    Texture2D CreateGridTexture()
    {
        int textureSize = 256;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        Color gridColor = Color.white;
        Color backgroundColor = _floorColor;
        
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                // グリッドラインの作成
                bool isGridLine = (x % (textureSize / 16) == 0) || (y % (textureSize / 16) == 0);
                texture.SetPixel(x, y, isGridLine ? gridColor : backgroundColor);
            }
        }
        
        texture.Apply();
        return texture;
    }
    
    void SetupPhysics()
    {
        // 既存のRigidbodyを削除
        Rigidbody existingRigidbody = _floorObject.GetComponent<Rigidbody>();
        if (existingRigidbody != null)
        {
            DestroyImmediate(existingRigidbody);
        }
        
        if (_addRigidbody)
        {
            Rigidbody rigidbody = _floorObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = _isKinematic;
            rigidbody.useGravity = !_isKinematic;
        }
        
        // Colliderの設定（既にCubeプリミティブに含まれている）
        Collider collider = _floorObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }
    }
    
    // エディタでの床の再生成
    [ContextMenu("床を再生成")]
    void RegenerateFloor()
    {
        if (_floorObject != null)
        {
            DestroyImmediate(_floorObject);
        }
        CreateFloor();
    }
    
    // 床の情報を取得
    public GameObject GetFloorObject()
    {
        return _floorObject;
    }
    
    // 床の位置を取得
    public Vector3 GetFloorPosition()
    {
        return _floorObject != null ? _floorObject.transform.position : Vector3.zero;
    }
    
    // 床のサイズを取得
    public Vector3 GetFloorSize()
    {
        return _floorObject != null ? _floorObject.transform.localScale : Vector3.zero;
    }
    
    // デバッグ用：Gizmosで床の範囲を表示
    void OnDrawGizmosSelected()
    {
        if (_floorObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_floorObject.transform.position, _floorObject.transform.localScale);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_floorPosition, _floorSize);
        }
    }
} 