using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchScene : MonoBehaviour
{
    public GameState targetState_Enter = GameState.Nothing;
    public GameState targetState_Esc = GameState.Nothing;
    public GameState targetState_Y = GameState.Nothing;
    public GameState targetState_N = GameState.Nothing;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (GameManager.Instance.CurrentState)
        {
            case GameState.Start:
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    GameManager.Instance.SwitchScene(targetState_Enter);
                }
                break;
            case GameState.Video:
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    GameManager.Instance.SwitchScene(targetState_Enter);
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    GameManager.Instance.SwitchScene(targetState_Esc);
                }
                break;
            case GameState.Challenge:
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    GameManager.Instance.SwitchScene(targetState_Y);
                }
                else if (Input.GetKeyDown(KeyCode.N))
                {
                    GameManager.Instance.SwitchScene(targetState_N);
                }
                break;
            case GameState.UnknownEnd:
            case GameState.WinWinEnd:
            case GameState.FailEnd:
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    Debug.Log("检测到回车键，从结束场景跳转回Start场景 - Enter key detected, switching from end scene back to Start scene");
                    GameManager.Instance.SwitchScene(GameState.Start);
                }
                break;
            default:
                break;
        }
    }
}
