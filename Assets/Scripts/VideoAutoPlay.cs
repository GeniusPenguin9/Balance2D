using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoAutoPlay : MonoBehaviour
{

    void Start()
    {
        VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "StoryVideo_v1.mp4");

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
       GameManager.Instance.SwitchScene(GameState.Challenge);
    }
}
