using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueDisplay : MonoBehaviour
{
    [Header("对话设置")]
    [TextArea(3, 10)]
    public string[] dialogueTexts = new string[]
    {
        "欢迎来到平衡2D世界！",
        "请保持左右平衡...",
        "准备好开始挑战了吗？"
    };
    
    [Header("UI组件")]
    public TextMeshProUGUI dialogueText; // 对话文本组件
    public GameObject dialoguePanel; // 对话面板
    public GameObject[] elementsToShowAfterDialogue; // 对话结束后要显示的元素
    
    [Header("时间设置")]
    public float displayDuration = 2f; // 每句话显示时长（秒）
    
    // 私有变量
    private int currentDialogueIndex = 0; // 当前对话索引
    private float timer = 0f; // 计时器
    private bool dialogueActive = true; // 对话是否激活
    private bool dialogueStarted = false; // 对话是否已开始
    
    void Start()
    {
        // 只做基础初始化，不自动开始对话
        foreach (GameObject element in elementsToShowAfterDialogue)
        {
            if (element != null)
            {
                element.SetActive(false);
            }
        }
        
        // 初始化时隐藏对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        Debug.Log("对话显示脚本已初始化，等待外部调用开始对话");
    }
    
    /// <summary>
    /// 开始显示对话（供外部调用）
    /// </summary>
    public void StartDialogue()
    {
        Debug.Log("StartDialogue 被调用");
        
        // 重置状态
        currentDialogueIndex = 0;
        timer = 0f;
        dialogueActive = true;
        dialogueStarted = false;
        
        // 检查组件引用
        if (dialoguePanel == null)
        {
            Debug.LogError("DialoguePanel 引用为空！请在Inspector中设置对话面板");
            return;
        }
        
        if (dialogueText == null)
        {
            Debug.LogError("DialogueText 引用为空！请在Inspector中设置对话文本组件");
            return;
        }
        
        Debug.Log("DialoguePanel 状态: " + dialoguePanel.name + " - Active: " + dialoguePanel.activeSelf);
        
        // 隐藏其他元素
        foreach (GameObject element in elementsToShowAfterDialogue)
        {
            if (element != null)
            {
                element.SetActive(false);
                Debug.Log("隐藏元素: " + element.name);
            }
        }
        
        // 显示对话面板
        dialoguePanel.SetActive(true);
        Debug.Log("设置 DialoguePanel 为激活状态: " + dialoguePanel.activeSelf);
        
        // 显示第一句对话
        ShowCurrentDialogue();
        dialogueStarted = true;
        
        Debug.Log("对话已开始，共有 " + dialogueTexts.Length + " 句对话");
    }
    
    void Update()
    {
        // 如果对话还在进行中
        if (dialogueActive && dialogueStarted)
        {
            timer += Time.deltaTime;
            
            // 如果达到显示时长
            if (timer >= displayDuration)
            {
                timer = 0f;
                currentDialogueIndex++;
                
                // 检查是否还有对话要显示
                if (currentDialogueIndex < dialogueTexts.Length)
                {
                    ShowCurrentDialogue();
                }
                else
                {
                    // 所有对话显示完毕，结束对话
                    EndDialogue();
                }
            }
        }
    }
    
    /// <summary>
    /// 显示当前对话
    /// </summary>
    void ShowCurrentDialogue()
    {
        if (dialogueText != null && currentDialogueIndex < dialogueTexts.Length)
        {
            dialogueText.text = dialogueTexts[currentDialogueIndex];
            Debug.Log("显示对话 " + (currentDialogueIndex + 1) + ": " + dialogueTexts[currentDialogueIndex]);
        }
    }
    
    /// <summary>
    /// 结束对话，隐藏对话面板并显示其他元素
    /// </summary>
    void EndDialogue()
    {
        dialogueActive = false;
        
        // 隐藏对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        // 显示对话结束后要显示的元素
        foreach (GameObject element in elementsToShowAfterDialogue)
        {
            if (element != null)
            {
                element.SetActive(true);
            }
        }
        
        Debug.Log("对话显示完毕，已隐藏对话面板并显示其他元素");
    }
    
    /// <summary>
    /// 手动跳过对话（可选功能）
    /// </summary>
    public void SkipDialogue()
    {
        if (dialogueActive)
        {
            EndDialogue();
            Debug.Log("对话已被手动跳过");
        }
    }
    
    /// <summary>
    /// 重置对话（可选功能）
    /// </summary>
    public void ResetDialogue()
    {
        currentDialogueIndex = 0;
        timer = 0f;
        dialogueActive = true;
        dialogueStarted = false;
        
        // 重新显示对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        // 隐藏其他元素
        foreach (GameObject element in elementsToShowAfterDialogue)
        {
            if (element != null)
            {
                element.SetActive(false);
            }
        }
        
        Start();
        Debug.Log("对话已重置");
    }
} 