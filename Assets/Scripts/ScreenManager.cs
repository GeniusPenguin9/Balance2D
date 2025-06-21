using UnityEngine;

/// <summary>
/// 屏幕管理器 - 负责设置游戏分辨率和显示模式
/// Screen Manager - Handles game resolution and display mode settings
/// </summary>
public class ScreenManager : MonoBehaviour
{
    [Header("Screen Settings")]
    [SerializeField] private int targetWidth = 1920;
    [SerializeField] private int targetHeight = 1080;
    [SerializeField] private bool fullScreen = true;
    
    void Start()
    {
        SetScreenResolution();
        Debug.Log($"Screen resolution set to: {targetWidth}x{targetHeight}, Fullscreen: {fullScreen}");
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
        Screen.fullScreen = fullScreen;
        Debug.Log($"Fullscreen toggled to: {fullScreen}");
    }
} 