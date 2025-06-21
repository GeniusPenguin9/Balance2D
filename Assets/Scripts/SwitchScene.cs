using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

        public void SwitchChallengeScene(){
        GameManager.Instance.SwitchScene(GameState.Challenge);
    }
    public void SwitchUnknownEndScene(){
        GameManager.Instance.SwitchScene(GameState.UnknownEnd);
    }
}
