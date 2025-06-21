#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 视频场景修复工具
/// 用于修复Video场景中的常见配置问题
/// </summary>
public class VideoSceneFixer : EditorWindow
{
    [MenuItem("Tools/修复视频场景")]
    public static void ShowWindow()
    {
        GetWindow<VideoSceneFixer>("视频场景修复工具");
    }

    void OnGUI()
    {
        GUILayout.Label("视频场景修复工具", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("修复当前场景的视频配置", GUILayout.Height(30)))
        {
            FixVideoScene();
        }

        GUILayout.Space(10);
        GUILayout.Label("修复内容：", EditorStyles.boldLabel);
        GUILayout.Label("• 修复Canvas缩放问题");
        GUILayout.Label("• 配置VideoPlayer组件");
        GUILayout.Label("• 设置摄像机目标");
        GUILayout.Label("• 验证视频文件路径");
    }

    private void FixVideoScene()
    {
        bool hasChanges = false;

        // 修复Canvas缩放问题
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.transform.localScale == Vector3.zero)
            {
                canvas.transform.localScale = Vector3.one;
                Debug.Log($"[VideoSceneFixer] 修复了Canvas '{canvas.name}' 的缩放问题");
                hasChanges = true;
            }
        }

        // 修复VideoPlayer配置
        VideoPlayer[] videoPlayers = FindObjectsOfType<VideoPlayer>();
        foreach (VideoPlayer vp in videoPlayers)
        {
            bool vpChanged = false;

            // 设置渲染模式
            if (vp.renderMode == VideoRenderMode.CameraNearPlane || vp.renderMode == VideoRenderMode.CameraFarPlane)
            {
                if (vp.targetCamera == null)
                {
                    Camera mainCamera = Camera.main;
                    if (mainCamera != null)
                    {
                        vp.targetCamera = mainCamera;
                        Debug.Log($"[VideoSceneFixer] 为VideoPlayer '{vp.name}' 设置了目标摄像机");
                        vpChanged = true;
                    }
                }
            }

            // 设置数据源
            if (vp.source != VideoSource.Url)
            {
                vp.source = VideoSource.Url;
                vpChanged = true;
            }

            // 设置播放选项
            if (vp.playOnAwake)
            {
                vp.playOnAwake = false;
                vpChanged = true;
            }

            if (!vp.waitForFirstFrame)
            {
                vp.waitForFirstFrame = true;
                vpChanged = true;
            }

            if (vpChanged)
            {
                EditorUtility.SetDirty(vp);
                hasChanges = true;
                Debug.Log($"[VideoSceneFixer] 修复了VideoPlayer '{vp.name}' 的配置");
            }
        }

        // 验证视频文件
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "StoryVideo_v2.mp4");
        if (!System.IO.File.Exists(videoPath))
        {
            Debug.LogWarning($"[VideoSceneFixer] 视频文件不存在: {videoPath}");
        }
        else
        {
            Debug.Log($"[VideoSceneFixer] 视频文件验证通过: {videoPath}");
        }

        if (hasChanges)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[VideoSceneFixer] 场景修复完成，请保存场景");
            EditorUtility.DisplayDialog("修复完成", "视频场景配置已修复，请保存场景！", "确定");
        }
        else
        {
            Debug.Log("[VideoSceneFixer] 场景配置正常，无需修复");
            EditorUtility.DisplayDialog("检查完成", "场景配置正常，无需修复", "确定");
        }
    }
}
#endif 