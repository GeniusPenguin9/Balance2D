using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchScene : MonoBehaviour
{
    public GameState targetState_Enter;
    public GameState targetState_Esc;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 检测回车键输入，从Start场景切换到Video场景
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) )
        {
            SwitchToNextScene(targetState_Enter);
        }else if (Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchToNextScene(targetState_Esc);
        }
    }

    // 检查当前场景，根据当前场景切换到下一个场景
    public void SwitchToNextScene(GameState targetState)
    {
        Debug.Log("回车键被按下，切换到下一个场景");
        GameManager.Instance.SwitchScene(targetState);
    }
}
