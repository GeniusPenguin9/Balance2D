using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 音频管理器 - 负责管理游戏音频播放（单例模式）
/// Audio Manager - Handles game audio playback (Singleton Pattern)
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("音频设置 Audio Settings")]
    [SerializeField] private AudioClip backgroundMusic; // 背景音乐音频剪辑
    [SerializeField] private bool enableMusicInVideoScene = false; // 视频场景是否播放音乐
    [SerializeField] [Range(0f, 1f)] private float bgmVolumePercentage = 0.5f; // BGM音量百分比 (0-1)
    
    /// <summary>
    /// 单例实例
    /// Singleton instance
    /// </summary>
    public static AudioManager Instance { get; private set; }
    
    private AudioSource musicAudioSource; // 音乐音频源
    private bool isMusicPlaying = false; // 音乐是否正在播放
    

    
    void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AudioManager singleton instance created and set to persist across scenes");
            
            // 初始化音频组件
            InitializeAudioComponents();
        }
        else
        {
            Debug.Log("AudioManager instance already exists, destroying duplicate");
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // 注册场景加载事件监听器
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // 检查当前场景并开始播放音乐（如果需要）
        CheckAndPlayMusic();
        
        Debug.Log($"AudioManager initialized - EnableMusicInVideoScene: {enableMusicInVideoScene}");
    }
    
    /// <summary>
    /// 初始化音频组件
    /// Initialize audio components
    /// </summary>
    void InitializeAudioComponents()
    {
        // 创建音乐音频源
        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.loop = true; // 循环播放
        musicAudioSource.playOnAwake = false; // 不自动播放
        
        if (backgroundMusic != null)
        {
            musicAudioSource.clip = backgroundMusic;
            Debug.Log($"Background music loaded: {backgroundMusic.name}");
        }
        else
        {
            Debug.LogWarning("Background music clip is not assigned. Please assign it in the inspector.");
        }
        
        // 初始化音量设置
        UpdateMusicVolume();
    }
    
    /// <summary>
    /// 场景加载完成时的回调函数
    /// Callback function when scene is loaded
    /// </summary>
    /// <param name="scene">加载的场景</param>
    /// <param name="mode">加载模式</param>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        CheckAndPlayMusic();
    }
    
    /// <summary>
    /// 检查当前场景并播放音乐（如果需要）
    /// Check current scene and play music if needed
    /// </summary>
    void CheckAndPlayMusic()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"Checking music for scene: {currentSceneName}");
        
        // 判断是否应该播放音乐
        bool shouldPlayMusic = ShouldPlayMusicInScene(currentSceneName);
        
        if (shouldPlayMusic && !isMusicPlaying)
        {
            PlayBackgroundMusic();
        }
        else if (!shouldPlayMusic && isMusicPlaying)
        {
            StopBackgroundMusic();
        }
    }
    
    /// <summary>
    /// 判断在指定场景是否应该播放音乐
    /// Determine if music should be played in the specified scene
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    /// <returns>是否应该播放音乐</returns>
    bool ShouldPlayMusicInScene(string sceneName)
    {
        // Video场景默认不播放音乐（除非特别启用）
        if (sceneName.Equals("Video", System.StringComparison.OrdinalIgnoreCase) && !enableMusicInVideoScene)
        {
            Debug.Log("Video scene detected - music disabled");
            return false;
        }
        
        // 其他所有场景都播放音乐
        return true;
    }
    
    /// <summary>
    /// 播放背景音乐
    /// Play background music
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (musicAudioSource != null && backgroundMusic != null && !isMusicPlaying)
        {
            // 确保音量设置正确
            UpdateMusicVolume();
            musicAudioSource.Play();
            isMusicPlaying = true;
            Debug.Log("Background music started playing");
        }
        else if (backgroundMusic == null)
        {
            Debug.LogWarning("Cannot play background music: audio clip is null");
        }
    }
    
    /// <summary>
    /// 停止背景音乐
    /// Stop background music
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (musicAudioSource != null && isMusicPlaying)
        {
            musicAudioSource.Stop();
            isMusicPlaying = false;
            Debug.Log("Background music stopped");
        }
    }
    

    
    /// <summary>
    /// 更新音乐音量 - BGM音量 = 系统音量 * 百分比
    /// Update music volume - BGM volume = System volume * Percentage
    /// </summary>
    void UpdateMusicVolume()
    {
        if (musicAudioSource != null)
        {
            // BGM音量 = 系统音量 * 百分比
            musicAudioSource.volume = AudioListener.volume * bgmVolumePercentage;
            Debug.Log($"Background music volume updated: System({AudioListener.volume:F2}) * Percentage({bgmVolumePercentage:F2}) = {musicAudioSource.volume:F2}");
        }
    }
    
    void OnDestroy()
    {
        // 取消注册场景加载事件监听器
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("AudioManager destroyed and scene event listener removed");
    }
} 