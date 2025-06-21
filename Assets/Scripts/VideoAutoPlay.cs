using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VideoAutoPlay : MonoBehaviour
{
    [Header("视频配置")]
    public string videoFileName = "StoryVideo_v2.mp4";
    
    [Header("渲染配置")]
    public VideoRenderMode renderMode = VideoRenderMode.CameraFarPlane;
    public bool useMainCamera = true;
    
    private VideoPlayer videoPlayer;
    private bool tryAlternativeSetup = false;

    void Start()
    {
        // 首先修复Canvas缩放问题 - 这是关键问题！
        FixCanvasScale();
        
        // 获取VideoPlayer组件
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            Debug.LogError("[VideoAutoPlay] 未找到VideoPlayer组件！");
            return;
        }

        // 配置视频播放器
        SetupVideoPlayer();
        
        // 播放视频
        PlayVideo();
    }

    private void FixCanvasScale()
    {
        // 修复Canvas缩放问题 - Video场景中Canvas的localScale是(0,0,0)
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.transform.localScale == Vector3.zero)
            {
                canvas.transform.localScale = Vector3.one;
                Debug.Log($"[VideoAutoPlay] 修复了Canvas '{canvas.name}' 的缩放问题: 从(0,0,0)改为(1,1,1)");
                
                // 同时修复Canvas的RectTransform设置
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    canvasRect.anchorMin = Vector2.zero;
                    canvasRect.anchorMax = Vector2.one;
                    canvasRect.offsetMin = Vector2.zero;
                    canvasRect.offsetMax = Vector2.zero;
                    Debug.Log($"[VideoAutoPlay] 修复了Canvas RectTransform设置");
                }
            }
        }
    }

    private void SetupVideoPlayer()
    {
        // 设置视频源为URL
        videoPlayer.source = VideoSource.Url;
        
        // 构建视频文件路径
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = videoPath;
        
        // 首先尝试基本设置
        if (!tryAlternativeSetup)
        {
            // 第一次尝试：使用CameraFarPlane模式
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                videoPlayer.targetCamera = mainCamera;
                Debug.Log($"[VideoAutoPlay] 使用CameraFarPlane模式，目标摄像机: {mainCamera.name}");
            }
            else
            {
                Debug.LogWarning("[VideoAutoPlay] 未找到主摄像机，尝试使用RenderTexture模式");
                tryAlternativeSetup = true;
            }
        }
        
        if (tryAlternativeSetup)
        {
            // 备用方案：创建一个RawImage来显示视频
            Debug.Log("[VideoAutoPlay] 使用RawImage显示视频");
            SetupRawImageDisplay();
        }
        
        // 设置其他播放选项
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.isLooping = false;
        
        // 音频设置
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        
        // 注册事件
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.started += OnVideoStarted;
        
        Debug.Log($"[VideoAutoPlay] 视频路径: {videoPath}");
        Debug.Log($"[VideoAutoPlay] 文件是否存在: {System.IO.File.Exists(videoPath)}");
        Debug.Log($"[VideoAutoPlay] 渲染模式: {videoPlayer.renderMode}");
    }

    private void SetupRawImageDisplay()
    {
        // 创建RenderTexture
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 16);
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        
        // 查找Canvas中的RawImage，如果没有就创建一个
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            // 确保Canvas缩放正确
            if (canvas.transform.localScale == Vector3.zero)
            {
                canvas.transform.localScale = Vector3.one;
                Debug.Log("[VideoAutoPlay] 修复了Canvas缩放");
            }
            
            RawImage rawImage = canvas.GetComponentInChildren<RawImage>();
            if (rawImage == null)
            {
                // 创建新的RawImage
                GameObject rawImageObj = new GameObject("VideoDisplay");
                rawImageObj.transform.SetParent(canvas.transform, false);
                rawImage = rawImageObj.AddComponent<RawImage>();
                
                // 设置RawImage占满整个Canvas
                RectTransform rectTransform = rawImage.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                
                Debug.Log("[VideoAutoPlay] 创建了新的RawImage用于显示视频");
            }
            
            // 设置RawImage的纹理
            rawImage.texture = renderTexture;
            Debug.Log("[VideoAutoPlay] 设置RawImage纹理完成");
        }
    }

    private void PlayVideo()
    {
        if (videoPlayer != null)
        {
            Debug.Log("[VideoAutoPlay] 开始准备视频...");
            videoPlayer.Prepare();
        }
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("[VideoAutoPlay] 视频准备完成，开始播放");
        Debug.Log($"[VideoAutoPlay] 视频尺寸: {vp.width} x {vp.height}");
        Debug.Log($"[VideoAutoPlay] 视频帧率: {vp.frameRate}");
        Debug.Log($"[VideoAutoPlay] 视频时长: {vp.length} 秒");
        Debug.Log($"[VideoAutoPlay] 视频轨道数: {vp.controlledAudioTrackCount}");
        
        vp.Play();
        
        // 如果3秒后视频还没有开始播放，尝试备用方案
        if (!tryAlternativeSetup)
        {
            Invoke("CheckVideoPlayback", 3.0f);
        }
    }

    void CheckVideoPlayback()
    {
        if (videoPlayer != null && videoPlayer.isPrepared && (!videoPlayer.isPlaying || videoPlayer.frame <= 1))
        {
            Debug.LogWarning("[VideoAutoPlay] 视频似乎没有正确播放，尝试备用方案...");
            tryAlternativeSetup = true;
            
            // 停止当前播放
            videoPlayer.Stop();
            
            // 重新设置
            SetupVideoPlayer();
            PlayVideo();
        }
    }

    void OnVideoStarted(VideoPlayer vp)
    {
        Debug.Log("[VideoAutoPlay] 视频开始播放");
        Debug.Log($"[VideoAutoPlay] 当前时间: {vp.time}");
        Debug.Log($"[VideoAutoPlay] 是否正在播放: {vp.isPlaying}");
        Debug.Log($"[VideoAutoPlay] 帧计数: {vp.frame}");
        
        // 如果帧计数为-1，说明视频没有正确播放，尝试备用方案
        if (vp.frame <= 0 && !tryAlternativeSetup)
        {
            Debug.LogWarning("[VideoAutoPlay] 检测到帧计数异常，立即切换到RenderTexture模式");
            tryAlternativeSetup = true;
            vp.Stop();
            SetupVideoPlayer();
            PlayVideo();
            return;
        }
        
        // 取消检查
        CancelInvoke("CheckVideoPlayback");
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[VideoAutoPlay] 视频播放错误: {message}");
        
        // 如果出错且还没尝试过备用方案，则尝试
        if (!tryAlternativeSetup)
        {
            Debug.Log("[VideoAutoPlay] 尝试备用显示方案...");
            tryAlternativeSetup = true;
            SetupVideoPlayer();
            PlayVideo();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[VideoAutoPlay] 视频播放完成，切换到挑战场景");
        GameManager.Instance.SwitchScene(GameState.Challenge);
    }

    // 添加调试方法
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && videoPlayer != null)
        {
            DebugVideoStatus();
        }
        
        // 按T键切换显示方案
        if (Input.GetKeyDown(KeyCode.T) && videoPlayer != null)
        {
            Debug.Log("[VideoAutoPlay] 手动切换显示方案");
            tryAlternativeSetup = !tryAlternativeSetup;
            videoPlayer.Stop();
            SetupVideoPlayer();
            PlayVideo();
        }
        
        // 按F键强制修复Canvas
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("[VideoAutoPlay] 手动修复Canvas缩放");
            FixCanvasScale();
        }
    }

    private void DebugVideoStatus()
    {
        Debug.Log("=== 视频状态调试信息 ===");
        Debug.Log($"是否准备就绪: {videoPlayer.isPrepared}");
        Debug.Log($"是否正在播放: {videoPlayer.isPlaying}");
        Debug.Log($"当前时间: {videoPlayer.time}");
        Debug.Log($"当前帧: {videoPlayer.frame}");
        Debug.Log($"总帧数: {videoPlayer.frameCount}");
        Debug.Log($"渲染模式: {videoPlayer.renderMode}");
        Debug.Log($"目标摄像机: {(videoPlayer.targetCamera != null ? videoPlayer.targetCamera.name : "无")}");
        Debug.Log($"目标纹理: {(videoPlayer.targetTexture != null ? "已设置" : "无")}");
        Debug.Log($"备用方案: {tryAlternativeSetup}");
        
        // 调试Canvas状态
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            Debug.Log($"Canvas '{canvas.name}' 缩放: {canvas.transform.localScale}");
        }
        Debug.Log("=========================");
    }

    void OnDestroy()
    {
        // 清理事件订阅
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.started -= OnVideoStarted;
        }
        
        // 清理调用
        CancelInvoke();
    }
}
