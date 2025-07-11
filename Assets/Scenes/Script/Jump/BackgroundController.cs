using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    [Header("背景設定")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private bool _enableDynamicBackground = true;
    [SerializeField] private Color _topColor = new Color(0.2f, 0.4f, 0.8f); // 上部の色（空色）
    [SerializeField] private Color _bottomColor = new Color(0.8f, 0.6f, 0.4f); // 下部の色（地面色）
    [SerializeField] private float _gradientHeight = 10f; // グラデーションの高さ
    
    [Header("動的背景設定")]
    [SerializeField] private bool _enableHeightBasedColor = true; // 高さに基づく色変更
    [SerializeField] private Color _highAltitudeColor = new Color(0.1f, 0.2f, 0.5f); // 高高度の色
    [SerializeField] private float _highAltitudeThreshold = 50f; // 高高度の閾値
    
    [Header("プレイヤー設定")]
    [SerializeField] private Transform _playerTransform; // プレイヤーのTransform
    
    [Header("パーティクル背景")]
    [SerializeField] private bool _enableParticleBackground = true;
    [SerializeField] private GameObject _particlePrefab;
    [SerializeField] private int _particleCount = 50;
    [SerializeField] private float _particleSpeed = 1f;
    [SerializeField] private float _particleSize = 0.1f;
    
    private Material _backgroundMaterial;
    private float _initialPlayerHeight;
    
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
            Debug.LogError("BackgroundController: _playerTransformが設定されていません。InspectorでPlayerを設定してください。");
            return;
        }
        
        // カメラの設定を変更してSkyBoxを無効化
        SetupCamera();
        
        _initialPlayerHeight = _playerTransform.position.y;
        
        // 背景マテリアルを作成
        CreateBackgroundMaterial();
        
        // パーティクル背景を初期化
        if (_enableParticleBackground)
        {
            InitializeParticleBackground();
        }
    }
    
    void Update()
    {
        if (!_enableDynamicBackground || _mainCamera == null) return;
        
        UpdateBackgroundColor();
    }
    
    private void CreateBackgroundMaterial()
    {
        // 外部シェーダーファイルを使用
        Shader shader = Shader.Find("Custom/BackgroundGradient");
        if (shader == null)
        {
            // シェーダーが見つからない場合は、シンプルな背景色を使用
            Debug.LogWarning("BackgroundGradient.shaderが見つかりません。シンプルな背景色を使用します。");
            UpdateSimpleBackground();
            return;
        }
        
        _backgroundMaterial = new Material(shader);
        _backgroundMaterial.SetColor("_TopColor", _topColor);
        _backgroundMaterial.SetColor("_BottomColor", _bottomColor);
        _backgroundMaterial.SetFloat("_GradientHeight", _gradientHeight);
    }
    
    private void SetupCamera()
    {
        if (_mainCamera == null) return;
        
        // カメラのClear FlagsをSolid Colorに変更
        _mainCamera.clearFlags = CameraClearFlags.SolidColor;
        
        // 初期背景色を設定
        _mainCamera.backgroundColor = _bottomColor;
        
        Debug.Log("カメラの設定を変更しました: SkyBox → Solid Color");
    }
    
    private void UpdateSimpleBackground()
    {
        // シンプルな背景色の更新
        if (_enableHeightBasedColor && _playerTransform != null)
        {
            float playerHeight = _playerTransform.position.y - _initialPlayerHeight;
            float heightRatio = Mathf.Clamp01(playerHeight / _highAltitudeThreshold);
            
            Color currentColor = Color.Lerp(_bottomColor, _highAltitudeColor, heightRatio);
            _mainCamera.backgroundColor = currentColor;
        }
        else
        {
            // グラデーション風の背景色
            Color gradientColor = Color.Lerp(_bottomColor, _topColor, 0.5f);
            _mainCamera.backgroundColor = gradientColor;
        }
    }
    
    private void UpdateBackgroundColor()
    {
        if (_backgroundMaterial != null)
        {
            // 高さに基づく色変更
            if (_enableHeightBasedColor && _playerTransform != null)
            {
                float playerHeight = _playerTransform.position.y - _initialPlayerHeight;
                float heightRatio = Mathf.Clamp01(playerHeight / _highAltitudeThreshold);
                
                Color currentTopColor = Color.Lerp(_topColor, _highAltitudeColor, heightRatio);
                _backgroundMaterial.SetColor("_TopColor", currentTopColor);
            }
        }
        else
        {
            UpdateSimpleBackground();
        }
    }
    
    private void InitializeParticleBackground()
    {
        if (_particlePrefab == null)
        {
            // パーティクルプレハブがない場合は、シンプルな球体を作成
            CreateParticlePrefab();
        }
        
        // パーティクルを生成
        for (int i = 0; i < _particleCount; i++)
        {
            CreateParticle();
        }
    }
    
    private void CreateParticlePrefab()
    {
        _particlePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _particlePrefab.name = "BackgroundParticle";
        
        // マテリアルを設定
        Material particleMaterial = new Material(Shader.Find("Standard"));
        particleMaterial.color = new Color(1f, 1f, 1f, 0.3f);
        _particlePrefab.GetComponent<Renderer>().material = particleMaterial;
        
        // コライダーを削除
        DestroyImmediate(_particlePrefab.GetComponent<Collider>());
        
        // プレハブとして保存
        _particlePrefab.SetActive(false);
    }
    
    private void CreateParticle()
    {
        if (_particlePrefab == null) return;
        
        GameObject particle = Instantiate(_particlePrefab, transform);
        particle.SetActive(true);
        
        // ランダムな位置に配置
        Vector3 randomPosition = new Vector3(
            Random.Range(-20f, 20f),
            Random.Range(0f, 50f),
            Random.Range(-20f, 20f)
        );
        particle.transform.position = randomPosition;
        
        // ランダムなサイズ
        float randomSize = Random.Range(_particleSize * 0.5f, _particleSize * 1.5f);
        particle.transform.localScale = Vector3.one * randomSize;
        
        // パーティクルアニメーション
        StartCoroutine(AnimateParticle(particle));
    }
    
    private System.Collections.IEnumerator AnimateParticle(GameObject particle)
    {
        Vector3 startPosition = particle.transform.position;
        Vector3 endPosition = startPosition + Vector3.up * 30f;
        
        float duration = Random.Range(5f, 15f);
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (particle == null) yield break;
            
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            particle.transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            
            // 透明度を徐々に下げる
            Renderer renderer = particle.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = Mathf.Lerp(0.3f, 0f, progress);
                renderer.material.color = color;
            }
            
            yield return null;
        }
        
        // パーティクルを削除して新しいパーティクルを作成
        if (particle != null)
        {
            Destroy(particle);
            CreateParticle();
        }
    }
    
    // 背景色を動的に変更するメソッド
    public void SetBackgroundColors(Color topColor, Color bottomColor)
    {
        _topColor = topColor;
        _bottomColor = bottomColor;
        
        if (_backgroundMaterial != null)
        {
            _backgroundMaterial.SetColor("_TopColor", _topColor);
            _backgroundMaterial.SetColor("_BottomColor", _bottomColor);
        }
    }
    
    // 高高度色を設定するメソッド
    public void SetHighAltitudeColor(Color highAltitudeColor)
    {
        _highAltitudeColor = highAltitudeColor;
    }
    
    // パーティクル背景の有効/無効を切り替えるメソッド
    public void SetParticleBackground(bool enable)
    {
        _enableParticleBackground = enable;
        
        if (enable)
        {
            InitializeParticleBackground();
        }
        else
        {
            // 既存のパーティクルを削除
            foreach (Transform child in transform)
            {
                if (child.name.Contains("BackgroundParticle"))
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
} 