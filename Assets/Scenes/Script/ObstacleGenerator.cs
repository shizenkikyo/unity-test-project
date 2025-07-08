using UnityEngine;
using System.Linq;

public class ObstacleGenerator : MonoBehaviour
{
    [Header("障害物生成設定")]
    [SerializeField] private Vector3[] _obstaclePositions = {
        new Vector3(3f, 0.5f, 0f),
        new Vector3(-3f, 0.5f, 0f),
        new Vector3(0f, 0.5f, 3f),
        new Vector3(0f, 0.5f, -3f)
    };
    
    [SerializeField] private Vector3 _obstacleSize = new Vector3(1f, 1f, 1f);
    [SerializeField] private Color[] _obstacleColors = {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };
    
    [Header("生成オプション")]
    [SerializeField] private bool _generateOnStart = true;
    [SerializeField] private bool _randomizeColors = false;
    [SerializeField] private bool _randomizePositions = false;
    [SerializeField] private float _randomRange = 5f;
    
    private GameObject[] _generatedObstacles;
    
    private void Start()
    {
        if (_generateOnStart)
        {
            GenerateObstacles();
        }
    }
    
    private void GenerateObstacles()
    {
        _generatedObstacles = new GameObject[_obstaclePositions.Length];
        
        for (int i = 0; i < _obstaclePositions.Length; i++)
        {
            Vector3 position = _obstaclePositions[i];
            
            // ランダム位置の適用
            if (_randomizePositions)
            {
                position += new Vector3(
                    Random.Range(-_randomRange, _randomRange),
                    0f,
                    Random.Range(-_randomRange, _randomRange)
                );
            }
            
            // 障害物を作成
            GameObject obstacle = CreateObstacle(position, i);
            _generatedObstacles[i] = obstacle;
        }
        
        Debug.Log($"Generated {_obstaclePositions.Length} obstacles");
    }
    
    private GameObject CreateObstacle(Vector3 position, int index)
    {
        // 障害物オブジェクトを作成
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = $"Obstacle_{index}";
        obstacle.transform.position = position;
        obstacle.transform.localScale = _obstacleSize;
        
        // Obstacleスクリプトを追加
        Obstacle obstacleScript = obstacle.AddComponent<Obstacle>();
        
        // 色を設定
        Color color = _obstacleColors[index % _obstacleColors.Length];
        if (_randomizeColors)
        {
            color = new Color(
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f)
            );
        }
        
        // Layerを設定（Layer 8をObstacle用に使用）
        obstacle.layer = 8; // Layer 8をObstacle用に設定
        
        // タグ設定は削除（LayerMaskのみを使用）
        
        return obstacle;
    }
    
    // エディタでの障害物再生成
    [ContextMenu("障害物を再生成")]
    private void RegenerateObstacles()
    {
        // 既存の障害物を削除
        if (_generatedObstacles != null)
        {
            foreach (GameObject obstacle in _generatedObstacles)
            {
                if (obstacle != null)
                {
                    DestroyImmediate(obstacle);
                }
            }
        }
        
        // 新しい障害物を生成
        GenerateObstacles();
    }
    
    // 障害物を削除
    [ContextMenu("障害物を削除")]
    private void ClearObstacles()
    {
        if (_generatedObstacles != null)
        {
            foreach (GameObject obstacle in _generatedObstacles)
            {
                if (obstacle != null)
                {
                    DestroyImmediate(obstacle);
                }
            }
        }
        
        Debug.Log("All obstacles cleared");
    }
    
    // 障害物の情報を取得
    public GameObject[] GetObstacles()
    {
        return _generatedObstacles;
    }
    
    // デバッグ用：Gizmosで障害物の配置を表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
        foreach (Vector3 position in _obstaclePositions)
        {
            Gizmos.DrawWireCube(position, _obstacleSize);
        }
    }
} 