using UnityEngine;

/// <summary>
/// 屏幕管理器 - 负责设置游戏分辨率和显示模式（单例模式）
/// Screen Manager - Handles game resolution and display mode settings (Singleton Pattern)
/// </summary>
public class ScreenManager : MonoBehaviour
{
    [Header("Screen Settings")]
    [SerializeField] private int targetWidth = 1920;
    [SerializeField] private int targetHeight = 1080;
    [SerializeField] private bool fullScreen = true;
    
    /// <summary>
    /// 单例实例
    /// Singleton instance
    /// </summary>
    public static ScreenManager Instance { get; private set; }
    
    void Awake()
    {
        // 单例模式实现
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ScreenManager singleton instance created and set to persist across scenes");
        }
        else
        {
            Debug.Log("ScreenManager instance already exists, destroying duplicate");
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        SetScreenResolution();
        Debug.Log($"Screen resolution set to: {targetWidth}x{targetHeight}, Fullscreen: {fullScreen}");
    }
    
    void Update()
    {
        // F12键切换全屏/窗口模式
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ToggleFullscreen();
        }
    }
    
    /// <summary>
    /// 设置屏幕分辨率
    /// Set screen resolution
    /// </summary>
    void SetScreenResolution()
    {
        Screen.SetResolution(targetWidth, targetHeight, fullScreen);
    }
    
    /// <summary>
    /// 切换全屏模式
    /// Toggle fullscreen mode
    /// </summary>
    public void ToggleFullscreen()
    {
        fullScreen = !fullScreen;
        // 切换时固定使用1920x1080分辨率
        Screen.SetResolution(1920, 1080, fullScreen);
        Debug.Log($"Fullscreen toggled to: {fullScreen}, Resolution: 1920x1080");
    }
} 