using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    private static GameManager _instance;
    private GameState currentState = GameState.Start;
    public GameState CurrentState
    {
        get { return currentState; }
        set { currentState = value; }
    }
    
    // Public property to access the singleton instance
    public static GameManager Instance
    {
        get
        {
            // If instance doesn't exist, try to find it in the scene
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                
                // If still null, create a new GameObject with GameManager
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                    Debug.Log("GameManager instance created automatically");
                }
            }
            return _instance;
        }
    }

    // Ensure only one instance exists
    void Awake()
    {
        // If instance already exists and it's not this one, destroy this one
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple GameManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // Set this as the instance and make it persistent
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("GameManager singleton initialized");
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GameManager started");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    

    // TODO-TEST: build scene name as enum. 
     public void SwitchScene(GameState state)
    {
        Debug.Log($"Switching to {state}");
        this.currentState = state;

        SceneManager.LoadSceneAsync(Enum.GetName(typeof(GameState), state)).completed += (AsyncOperation operation) =>
        {
            switch (state)
            {
                case GameState.Video:
                    LoadVideoResources();
                    break;
                case GameState.Challenge:
                    LoadChallengeResources();
                    break;
                case GameState.UnknownEnd:
                    LoadUnknownEndResources();
                    break;
                case GameState.WinWinEnd:
                    LoadWinWinEndResources();
                    break;
                case GameState.FailEnd:
                    LoadFailEndResources();
                    break;
            }
        };
    }

    public void LoadMenuResources(){
        Debug.Log("Loading menu resources");
    }
    public void LoadVideoResources(){
        Debug.Log("Loading video resources");
    }
    public void LoadChallengeResources(){
        Debug.Log("Loading challenge resources");
    }
    public void LoadUnknownEndResources(){
        Debug.Log("Loading unknown end resources");
        // 尝试启动对话显示
        TryStartDialogue();
    }
    public void LoadWinWinEndResources(){
        Debug.Log("Loading win win end resources");
        // 尝试启动对话显示
        TryStartDialogue();
    }
    public void LoadFailEndResources(){
        Debug.Log("Loading fail end resources");
        // 尝试启动对话显示
        TryStartDialogue();
    }
    
    /// <summary>
    /// 尝试找到并启动对话显示组件
    /// </summary>
    private void TryStartDialogue()
    {
        // 使用协程延迟调用，确保场景完全加载
        StartCoroutine(DelayedStartDialogue());
    }
    
    /// <summary>
    /// 延迟启动对话，确保场景完全加载
    /// </summary>
    private IEnumerator DelayedStartDialogue()
    {
        // 等待一帧，确保场景组件完全初始化
        yield return new WaitForEndOfFrame();
        
        DialogueDisplay dialogueDisplay = FindObjectOfType<DialogueDisplay>();
        if (dialogueDisplay != null)
        {
            Debug.Log("找到对话显示组件: " + dialogueDisplay.gameObject.name);
            dialogueDisplay.StartDialogue();
        }
        else
        {
            Debug.LogWarning("未找到对话显示组件！请确保场景中有DialogueDisplay脚本");
        }
    }
}
public enum GameState
{
    Start,
    Video,
    Challenge,
    UnknownEnd,
    WinWinEnd,
    FailEnd,
    Nothing
}