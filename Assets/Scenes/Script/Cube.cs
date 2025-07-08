using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cube : MonoBehaviour
{
    [Header("回転設定")]
    [SerializeField] private float rotatingSpeed = 0.8f;
    [SerializeField] private float rotatingAngle = 90f;
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useSmoothEasing = true;
    [SerializeField] private float easingStrength = 0.3f;
    
    [Header("音効果設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rollSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private float rollVolume = 0.5f;
    [SerializeField] private float landVolume = 0.7f;
    [SerializeField] private bool enableSound = true;
    
    private Vector3 halfSize;
    private float time = 0f;
    private Vector3 axis = Vector3.zero;
    private Vector3 point = Vector3.zero;
    private bool isRotating = false;
    
    private void Awake()
    {
        halfSize = transform.localScale / 2f;
        
        // AudioSourceが設定されていない場合は自動で追加
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // AudioSourceの初期設定
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // 回転中は入力を無視
        if (isRotating) return;
        
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // 入力に基づいて回転軸と回転中心を設定
        if (keyboard.upArrowKey.isPressed)
        {
            axis = Vector3.right;
            point = transform.position + new Vector3(0f, -halfSize.y, halfSize.z);
        }
        else if (keyboard.downArrowKey.isPressed)
        {
            axis = Vector3.left;
            point = transform.position + new Vector3(0f, -halfSize.y, -halfSize.z);
        }
        else if (keyboard.leftArrowKey.isPressed)
        {
            axis = Vector3.forward;
            point = transform.position + new Vector3(-halfSize.x, -halfSize.y, 0f);
        }
        else if (keyboard.rightArrowKey.isPressed)
        {
            axis = Vector3.back;
            point = transform.position + new Vector3(halfSize.x, -halfSize.y, 0f);
        }
        
        // 回転開始
        if (point != Vector3.zero)
        {
            PlayRollSound();
            StartCoroutine(StartRotate());
        }
    }
    
    private IEnumerator StartRotate()
    {
        isRotating = true;
        float elapsedTime = 0f;
        float totalAngle = 0f;
        
        Debug.Log($"Starting rotation - Axis: {axis}, Point: {point}, Speed: {rotatingSpeed}");
        
        while (elapsedTime < rotatingSpeed)
        {
            // アニメーションカーブを使用して滑らかな回転
            float progress = elapsedTime / rotatingSpeed;
            float curveValue = rotationCurve.Evaluate(progress);
            
            // イージングを適用
            if (useSmoothEasing)
            {
                curveValue = ApplyEasing(curveValue, easingStrength);
            }
            
            // このフレームで回転する角度を計算
            float targetAngle = rotatingAngle * curveValue;
            float angleThisFrame = targetAngle - totalAngle;
            
            if (angleThisFrame > 0)
            {
                transform.RotateAround(point, axis, angleThisFrame);
                totalAngle = targetAngle;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 最終的な角度調整（精度を保つため）
        float finalAngle = rotatingAngle - totalAngle;
        if (finalAngle > 0)
        {
            transform.RotateAround(point, axis, finalAngle);
        }
        
        // 回転完了時の処理
        time = 0f;
        axis = Vector3.zero;
        point = Vector3.zero;
        isRotating = false;
        
        // 着地音を再生
        PlayLandSound();
        
        Debug.Log("Rotation completed");
    }
    
    // イージング関数
    private float ApplyEasing(float t, float strength)
    {
        // 滑らかなイージング（SmoothStepの改良版）
        float eased = t * t * (3f - 2f * t);
        return Mathf.Lerp(t, eased, strength);
    }
    
    // 転がり音を再生
    private void PlayRollSound()
    {
        if (!enableSound || audioSource == null || rollSound == null) return;
        
        audioSource.clip = rollSound;
        audioSource.volume = rollVolume;
        audioSource.Play();
        
        Debug.Log("Playing roll sound");
    }
    
    // 着地音を再生
    private void PlayLandSound()
    {
        if (!enableSound || audioSource == null || landSound == null) return;
        
        audioSource.clip = landSound;
        audioSource.volume = landVolume;
        audioSource.Play();
        
        Debug.Log("Playing land sound");
    }
    
    // 音効果の設定をリセット
    [ContextMenu("Reset Audio Settings")]
    private void ResetAudioSettings()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
        Debug.Log("Audio settings reset");
    }
    
    // デバッグ用：Gizmosで回転軸と回転中心を表示
    private void OnDrawGizmosSelected()
    {
        if (point != Vector3.zero)
        {
            // 回転中心を表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(point, 0.1f);
            
            // 回転軸を表示
            Gizmos.color = Color.red;
            Gizmos.DrawRay(point, axis * 2f);
            
            // Cubeの中心から回転中心への線を表示
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, point);
        }
        
        // Cubeのサイズを表示
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}