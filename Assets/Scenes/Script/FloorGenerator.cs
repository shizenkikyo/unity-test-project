using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
    [Header("床の設定")]
    [SerializeField] private Vector3 floorSize = new Vector3(20f, 1f, 20f);
    [SerializeField] private Vector3 floorPosition = new Vector3(0f, 0f, 0f);
    [SerializeField] private Material floorMaterial;
    
    [Header("床の見た目")]
    [SerializeField] private Color floorColor = Color.gray;
    [SerializeField] private bool addGridTexture = true;
    [SerializeField] private float gridSize = 1f;
    
    [Header("物理設定")]
    [SerializeField] private bool addRigidbody = false;
    [SerializeField] private bool isKinematic = true;
    
    private GameObject floorObject;
    
    void Start()
    {
        CreateFloor();
    }
    
    void CreateFloor()
    {
        // 床オブジェクトの作成
        floorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floorObject.name = "Floor";
        
        // 位置とサイズの設定
        floorObject.transform.position = floorPosition;
        floorObject.transform.localScale = floorSize;
        
        // マテリアルの設定
        SetupMaterial();
        
        // 物理設定
        SetupPhysics();
        
        // レイヤー設定（地面チェック用）
        floorObject.layer = LayerMask.NameToLayer("Default");
        
        Debug.Log("床を作成しました: " + floorObject.name);
    }
    
    void SetupMaterial()
    {
        Renderer renderer = floorObject.GetComponent<Renderer>();
        
        if (floorMaterial != null)
        {
            renderer.material = floorMaterial;
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
            material.color = floorColor;
            
            if (addGridTexture)
            {
                material.mainTexture = CreateGridTexture();
                material.mainTextureScale = new Vector2(floorSize.x / gridSize, floorSize.z / gridSize);
            }
            
            renderer.material = material;
        }
    }
    
    Texture2D CreateGridTexture()
    {
        int textureSize = 256;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        Color gridColor = Color.white;
        Color backgroundColor = floorColor;
        
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
        Rigidbody existingRigidbody = floorObject.GetComponent<Rigidbody>();
        if (existingRigidbody != null)
        {
            DestroyImmediate(existingRigidbody);
        }
        
        if (addRigidbody)
        {
            Rigidbody rigidbody = floorObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = isKinematic;
            rigidbody.useGravity = !isKinematic;
        }
        
        // Colliderの設定（既にCubeプリミティブに含まれている）
        Collider collider = floorObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }
    }
    
    // エディタでの床の再生成
    [ContextMenu("床を再生成")]
    void RegenerateFloor()
    {
        if (floorObject != null)
        {
            DestroyImmediate(floorObject);
        }
        CreateFloor();
    }
    
    // 床の情報を取得
    public GameObject GetFloorObject()
    {
        return floorObject;
    }
    
    // 床の位置を取得
    public Vector3 GetFloorPosition()
    {
        return floorObject != null ? floorObject.transform.position : Vector3.zero;
    }
    
    // 床のサイズを取得
    public Vector3 GetFloorSize()
    {
        return floorObject != null ? floorObject.transform.localScale : Vector3.zero;
    }
    
    // デバッグ用：Gizmosで床の範囲を表示
    void OnDrawGizmosSelected()
    {
        if (floorObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(floorObject.transform.position, floorObject.transform.localScale);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(floorPosition, floorSize);
        }
    }
} 