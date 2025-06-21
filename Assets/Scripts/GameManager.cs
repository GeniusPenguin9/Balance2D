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
    public void LoadChallengeResources(){
        Debug.Log("Loading challenge resources");
    }
    public void LoadUnknownEndResources(){
        Debug.Log("Loading unknown end resources");
    }
    public void LoadWinWinEndResources(){
        Debug.Log("Loading win win end resources");
    }
    public void LoadFailEndResources(){
        Debug.Log("Loading fail end resources");
    }
}
public enum GameState
{
    Start,
    Challenge,
    UnknownEnd,
    WinWinEnd,
    FailEnd,
}