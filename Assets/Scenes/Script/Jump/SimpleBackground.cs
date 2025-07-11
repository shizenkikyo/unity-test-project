using UnityEngine;

public class SimpleBackground : MonoBehaviour
{
    [Header("背景設定")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private bool _enableDynamicBackground = true;
    
    [Header("色設定")]
    [SerializeField] private Color _groundColor = new Color(0.6f, 0.4f, 0.2f); // 地面色
    [SerializeField] private Color _skyColor = new Color(0.3f, 0.6f, 1.0f); // 空色
    [SerializeField] private Color _highSkyColor = new Color(0.1f, 0.2f, 0.5f); // 高高度の空色
    
    [Header("高さ設定")]
    [SerializeField] private float _colorChangeHeight = 30f; // 色が変わる高さ
    [SerializeField] private float _maxHeight = 100f; // 最大高さ
    
    [Header("プレイヤー設定")]
    [SerializeField] private Transform _playerTransform; // プレイヤーのTransform
    
    [Header("雲の設定")]
    [SerializeField] private bool _enableClouds = true;
    [SerializeField] private int _cloudCount = 20;
    [SerializeField] private float _cloudSpeed = 0.5f;
    [SerializeField] private float _cloudSize = 2f;
    [SerializeField] private Color _cloudColor = Color.white; // 雲の色
    
    private float _initialPlayerHeight;
    private GameObject[] _clouds;
    
    void Start()
    {
        // メインカメラを自動取得
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
        
        // プレイヤーが設定されていない場合はエラーを出力
        if (_playerTransform == null)
        {
            Debug.LogError("SimpleBackground: _playerTransformが設定されていません。InspectorでPlayerを設定してください。");
            return;
        }
        
        // カメラの設定を変更してSkyBoxを無効化
        SetupCamera();
        
        _initialPlayerHeight = _playerTransform.position.y;
        
        // 雲を初期化
        if (_enableClouds)
        {
            InitializeClouds();
        }
        
        // 初期背景色を設定
        UpdateBackgroundColor();
    }
    
    void Update()
    {
        if (!_enableDynamicBackground) return;
        
        UpdateBackgroundColor();
        UpdateClouds();
    }
    
    private void SetupCamera()
    {
        if (_mainCamera == null) return;
        
        // カメラのClear FlagsをSolid Colorに変更
        _mainCamera.clearFlags = CameraClearFlags.SolidColor;
        
        // 初期背景色を設定
        _mainCamera.backgroundColor = _groundColor;
        
        Debug.Log("カメラの設定を変更しました: SkyBox → Solid Color");
    }
    
    private void UpdateBackgroundColor()
    {
        if (_mainCamera == null || _playerTransform == null) return;
        
        float playerHeight = _playerTransform.position.y - _initialPlayerHeight;
        float heightRatio = Mathf.Clamp01(playerHeight / _colorChangeHeight);
        
        // 高さに応じて色を変更
        Color currentColor;
        if (playerHeight > _colorChangeHeight)
        {
            // 高高度では高高度の空色に
            float highRatio = Mathf.Clamp01((playerHeight - _colorChangeHeight) / (_maxHeight - _colorChangeHeight));
            currentColor = Color.Lerp(_skyColor, _highSkyColor, highRatio);
        }
        else
        {
            // 低高度では地面色から空色に
            currentColor = Color.Lerp(_groundColor, _skyColor, heightRatio);
        }
        
        _mainCamera.backgroundColor = currentColor;
    }
    
    private void InitializeClouds()
    {
        _clouds = new GameObject[_cloudCount];
        
        for (int i = 0; i < _cloudCount; i++)
        {
            _clouds[i] = CreateCloud();
        }
    }
    
    private GameObject CreateCloud()
    {
        // 雲のオブジェクトを作成
        GameObject cloud = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cloud.name = "BackgroundCloud";
        
        // 新しいマテリアルを作成（Unlit/Colorシェーダーを使用）
        Material cloudMaterial = new Material(Shader.Find("Unlit/Color"));
        if (cloudMaterial == null)
        {
            // Unlit/Colorが見つからない場合は、シンプルなマテリアルを作成
            cloudMaterial = new Material(Shader.Find("Standard"));
            cloudMaterial.color = _cloudColor;
        }
        else
        {
            cloudMaterial.color = _cloudColor;
        }
        
        // マテリアルを適用
        cloud.GetComponent<Renderer>().material = cloudMaterial;
        
        // コライダーを削除
        DestroyImmediate(cloud.GetComponent<Collider>());
        
        // ランダムな位置に配置
        Vector3 randomPosition = new Vector3(
            Random.Range(-30f, 30f),
            Random.Range(5f, 25f),
            Random.Range(-30f, 30f)
        );
        cloud.transform.position = randomPosition;
        
        // ランダムなサイズ
        float randomSize = Random.Range(_cloudSize * 0.5f, _cloudSize * 1.5f);
        cloud.transform.localScale = Vector3.one * randomSize;
        
        return cloud;
    }
    
    private void UpdateClouds()
    {
        if (_clouds == null) return;
        
        for (int i = 0; i < _clouds.Length; i++)
        {
            if (_clouds[i] != null)
            {
                // 雲を上に移動
                _clouds[i].transform.Translate(Vector3.up * _cloudSpeed * Time.deltaTime);
                
                // 一定の高さに達したら下に戻す
                if (_clouds[i].transform.position.y > 30f)
                {
                    Vector3 newPosition = _clouds[i].transform.position;
                    newPosition.y = -5f;
                    newPosition.x = Random.Range(-30f, 30f);
                    newPosition.z = Random.Range(-30f, 30f);
                    _clouds[i].transform.position = newPosition;
                }
            }
        }
    }
    
    // 背景色を動的に変更するメソッド
    public void SetBackgroundColors(Color groundColor, Color skyColor, Color highSkyColor)
    {
        _groundColor = groundColor;
        _skyColor = skyColor;
        _highSkyColor = highSkyColor;
        UpdateBackgroundColor();
    }
    
    // 雲の有効/無効を切り替えるメソッド
    public void SetClouds(bool enable)
    {
        _enableClouds = enable;
        
        if (enable && _clouds == null)
        {
            InitializeClouds();
        }
        else if (!enable && _clouds != null)
        {
            // 既存の雲を削除
            for (int i = 0; i < _clouds.Length; i++)
            {
                if (_clouds[i] != null)
                {
                    Destroy(_clouds[i]);
                }
            }
            _clouds = null;
        }
    }
    
    // 雲の速度を変更するメソッド
    public void SetCloudSpeed(float speed)
    {
        _cloudSpeed = speed;
    }
    
    // 雲のサイズを変更するメソッド
    public void SetCloudSize(float size)
    {
        _cloudSize = size;
        if (_clouds != null)
        {
            for (int i = 0; i < _clouds.Length; i++)
            {
                if (_clouds[i] != null)
                {
                    float randomSize = Random.Range(_cloudSize * 0.5f, _cloudSize * 1.5f);
                    _clouds[i].transform.localScale = Vector3.one * randomSize;
                }
            }
        }
    }
} 