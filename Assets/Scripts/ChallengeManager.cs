using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChallengeManager : MonoBehaviour
{
    // round - 可在Unity编辑器中修改
    [Header("游戏参数设置")]
    public int maxRound = 10;
    private int currentRound;

    // position limits - 可在Unity编辑器中修改
    public int maxPosition = 10;
    public int minPosition = -10;

    // initial position - 可在Unity编辑器中修改
    public int playerAInitialPosition = -5;
    public int playerBInitialPosition = 5;
    public int chestInitialPosition = 0;

    // dynamic position - 运行时动态计算
    private int playerACurrentPosition;
    private int playerBCurrentPosition;
    private int chestCurrentPosition;

    // action container
    public GameObject actionContainer;

    // Game state
    // 游戏结束条件：
    // 1. currentRound归零 -> UnknownEnd
    // 2. player与chest位置重合 -> 让用户选择分享与否 -> WinWinEnd或FailEnd
    // 3. player位置越界（<minPosition或>maxPosition） -> FailEnd (在范围内时安全)
    private ChallengeGameState currentGameState = ChallengeGameState.RoundStart;
    
    // Player actions
    private ActionType playerAAction = ActionType.Nothing;
    private ActionType playerBAction = ActionType.Nothing;
    private bool playerAHasChosen = false;
    private bool playerBHasChosen = false;
    private bool isPlayerATurn = true; // 当前轮到哪个玩家输入

    // UI references (需要在Inspector中设置)
    [Header("UI References")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI gameStateText;
    public TextMeshProUGUI playerANameText;
    public TextMeshProUGUI playerBNameText;

    [Header("Color Settings")]
    [Tooltip("玩家A正常状态颜色（十六进制格式，如: #FFFFFF）")]
    public string playerANormalColorHex = "#FFFFFF";
    [Tooltip("玩家A高亮状态颜色（十六进制格式，如: #FFFF00）")]
    public string playerAHighlightColorHex = "#FFFF00";
    [Tooltip("玩家B正常状态颜色（十六进制格式，如: #FFFFFF）")]
    public string playerBNormalColorHex = "#FFFFFF";
    [Tooltip("玩家B高亮状态颜色（十六进制格式，如: #FF0000）")]
    public string playerBHighlightColorHex = "#FF0000";

    public GameObject choicePanel; // 选择分享的面板
    public Button shareButton;
    public Button notShareButton;

    // Story UI references (需要在Inspector中设置)
    [Header("Story UI References")]
    public GameObject storyPanel; // 故事面板
    public TextMeshProUGUI storyText; // 故事文本

    // Story content (可在Unity编辑器中修改)
    [Header("Story Content")]
    public List<string> storyContents = new List<string>(); // 故事内容列表
    public float storyDisplayInterval = 1f; // 故事内容显示间隔时间（秒）

    // 用于存储原始字体大小和颜色
    private float playerANameOriginalFontSize;
    private float playerBNameOriginalFontSize;
    private Color playerANormalColor;
    private Color playerAHighlightColor;
    private Color playerBNormalColor;
    private Color playerBHighlightColor;
    
    // 记录遇到宝箱的玩家（用于高亮显示）
    private bool playerAReachedChest = false;
    private bool playerBReachedChest = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("ChallengeManager 开始初始化");
        InitializeGame();
        StartCoroutine(GameLoop());
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// 初始化游戏
    /// </summary>
    void InitializeGame()
    {
        Debug.Log("初始化游戏状态");
        currentRound = maxRound;
        playerACurrentPosition = playerAInitialPosition;
        playerBCurrentPosition = playerBInitialPosition;
        chestCurrentPosition = chestInitialPosition;
        
        // 初始化UI按钮事件
        if (shareButton != null)
            shareButton.onClick.AddListener(() => OnChoiceSelected(true));
        if (notShareButton != null)
            notShareButton.onClick.AddListener(() => OnChoiceSelected(false));
        
        // 初始化ChoicePanel状态 - 默认不显示且不可交互
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
            Debug.Log("ChoicePanel已设置为初始不显示状态");
        }

        if (actionContainer != null)
        {
            actionContainer.SetActive(false);
            Debug.Log("ActionContainer已设置为初始不显示状态");
        }
        
        // 初始化StoryPanel状态 - 默认不显示
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
            Debug.Log("StoryPanel已设置为初始不显示状态");
        }
        
        // 记录原始字体大小和初始化颜色
        if (playerANameText != null)
        {
            playerANameOriginalFontSize = playerANameText.fontSize;
        }
        if (playerBNameText != null)
        {
            playerBNameOriginalFontSize = playerBNameText.fontSize;
        }
        
        // 初始化颜色（从十六进制字符串转换）
        playerANormalColor = HexToColor(playerANormalColorHex);
        playerAHighlightColor = HexToColor(playerAHighlightColorHex);
        playerBNormalColor = HexToColor(playerBNormalColorHex);
        playerBHighlightColor = HexToColor(playerBHighlightColorHex);
        
        Debug.Log($"颜色初始化完成 - A正常:{playerANormalColorHex}, A高亮:{playerAHighlightColorHex}, B正常:{playerBNormalColorHex}, B高亮:{playerBHighlightColorHex}");
        
        // 明确设置初始颜色状态
        if (playerANameText != null)
        {
            playerANameText.color = playerANormalColor;
            Debug.Log($"玩家A名字初始颜色设置为: {playerANormalColor}");
        }
        if (playerBNameText != null)
        {
            playerBNameText.color = playerBNormalColor;
            Debug.Log($"玩家B名字初始颜色设置为: {playerBNormalColor}");
        }
        
        UpdateUI();
    }

    /// <summary>
    /// 游戏主循环
    /// </summary>
    IEnumerator GameLoop()
    {
        // 初始更新一下，防止手动放置元素导致的UI偏差
        UIManager uiManager = FindObjectOfType<UIManager>();
        uiManager.UpdatePositions(playerACurrentPosition, playerBCurrentPosition, chestCurrentPosition);
        
        // 1.显示StoryPanel
        // 2.让显示StoryContent(TextAsset)显示指定的语句，每句话持续1秒，直到所有语句显示完毕
        // 3.隐藏StoryPanel
        
        // 显示故事内容
        if (storyContents != null && storyContents.Count > 0)
        {
            Debug.Log("开始显示故事内容");
            
            // 显示StoryPanel
            if (storyPanel != null)
            {
                storyPanel.SetActive(true);
                Debug.Log("StoryPanel已显示");
            }
            
            // 逐句显示故事内容，每句话持续1秒
            for (int i = 0; i < storyContents.Count; i++)
            {
                if (storyText != null)
                {
                    storyText.text = storyContents[i];
                    Debug.Log($"显示故事内容 [{i + 1}/{storyContents.Count}]: {storyContents[i]}");
                }
                yield return new WaitForSeconds(storyDisplayInterval); // 等待指定的间隔时间
            }
            
            // 隐藏StoryPanel
            if (storyPanel != null)
            {
                storyPanel.SetActive(false);
                Debug.Log("StoryPanel已隐藏，故事显示完毕");
            }
        }
        else
        {
            Debug.Log("没有故事内容需要显示，直接开始游戏");
        }

        while (currentRound > 0)
        {
            // 开始新回合
            yield return StartCoroutine(PlayRound());
            
            // 检查位置是否越界（在ProcessActions中已经检查过，但如果那里没有触发场景切换，这里再次确认）
            if (playerACurrentPosition < minPosition || playerACurrentPosition > maxPosition ||
                playerBCurrentPosition < minPosition || playerBCurrentPosition > maxPosition)
            {
                Debug.Log("回合结束后检测到位置越界 - FailEnd");
                // TODO: Animation
                GameManager.Instance.SwitchScene(GameState.FailEnd);
                yield break; // 游戏结束
            }
            
            // 检查游戏是否结束（玩家与宝箱重合）
            // 注意：在这里我们需要在ProcessActions中保存旧的宝箱位置，所以这个检查需要在ProcessActions中进行
            // 这里暂时保留原有逻辑作为备用检查
            if (playerACurrentPosition == chestCurrentPosition || playerBCurrentPosition == chestCurrentPosition)
            {
                Debug.Log("检测到玩家与宝箱位置重合，进入选择阶段");
                yield return StartCoroutine(HandleChoicePhase());
                yield break; // 游戏结束
            }
            
            currentRound--;
            UpdateUI();
        }
        
        // 回合数用完，触发UnknownEnd
        Debug.Log("回合数用完，游戏结束 - UnknownEnd");
        GameManager.Instance.SwitchScene(GameState.UnknownEnd);
    }

    /// <summary>
    /// 执行一个回合
    /// </summary>
    IEnumerator PlayRound()
    {
        Debug.Log($"第 {currentRound} 回合开始");
        
        // 重置回合状态
        ResetRoundState();
        
        // 显示"回合开始"
        currentGameState = ChallengeGameState.RoundStart;
        UpdateUI();
        yield return new WaitForSeconds(1f);
        
        // 显示actionContainer，开始玩家输入阶段
        currentGameState = ChallengeGameState.PlayerInput;
        if (actionContainer != null)
            actionContainer.SetActive(true);
        UpdateUI();
        
        // 等待两个玩家都选择完毕
        yield return StartCoroutine(WaitForPlayerChoices());
        
        // 隐藏actionContainer
        if (actionContainer != null)
            actionContainer.SetActive(false);
        
        // 显示"结算中"
        currentGameState = ChallengeGameState.Calculating;
        UpdateUI();
        yield return new WaitForSeconds(1f);
        
        // 执行行动结算
        bool needChoicePhase = ProcessActions();
        
        // 如果需要进入选择阶段，启动选择协程并结束当前回合
        if (needChoicePhase)
        {
            Debug.Log("开始选择阶段，游戏循环将被中断");
            yield return StartCoroutine(HandleChoicePhase());
            yield break; // 结束当前回合和游戏循环
        }
        
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// 重置回合状态
    /// </summary>
    void ResetRoundState()
    {
        playerAAction = ActionType.Nothing;
        playerBAction = ActionType.Nothing;
        playerAHasChosen = false;
        playerBHasChosen = false;
        isPlayerATurn = true;
        
        // 重置宝箱遇到状态
        playerAReachedChest = false;
        playerBReachedChest = false;
    }

    /// <summary>
    /// 等待玩家选择
    /// </summary>
    IEnumerator WaitForPlayerChoices()
    {
        Debug.Log("等待玩家A和玩家B做出选择");
        
        while (!playerAHasChosen || !playerBHasChosen)
        {
            yield return null;
        }
        
        Debug.Log($"玩家选择完毕 - A: {playerAAction}, B: {playerBAction}");
    }

    /// <summary>
    /// 处理输入
    /// </summary>
    void HandleInput()
    {
        if (currentGameState != ChallengeGameState.PlayerInput)
            return;

        // 检测数字键1-6
        for (int i = 1; i <= 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                HandleActionInput((ActionType)(i - 1)); // 将键盘1-6映射到ActionType 0-5
                break;
            }
        }
    }

    /// <summary>
    /// 处理行动输入
    /// </summary>
    void HandleActionInput(ActionType action)
    {
        if (isPlayerATurn && !playerAHasChosen)
        {
            playerAAction = action;
            playerAHasChosen = true;
            isPlayerATurn = false;
            Debug.Log($"玩家A选择了行动: {action}");
        }
        else if (!isPlayerATurn && !playerBHasChosen)
        {
            playerBAction = action;
            playerBHasChosen = true;
            Debug.Log($"玩家B选择了行动: {action}");
        }
        
        UpdateUI();
    }

    /// <summary>
    /// 处理行动结算
    /// </summary>
    /// <returns>如果需要进入选择阶段则返回true</returns>
    bool ProcessActions()
    {
        Debug.Log("开始处理行动结算");
        Debug.Log($"结算前位置 - A: {playerACurrentPosition}, B: {playerBCurrentPosition}, 宝箱: {chestCurrentPosition}");
        Debug.Log($"玩家行动 - A: {playerAAction}, B: {playerBAction}");
        
        // 第一步：处理Enemy_Reverse效果，修改对方的行动
        ActionType finalPlayerAAction = playerAAction;
        ActionType finalPlayerBAction = playerBAction;
        
        // 检查玩家A是否选择了Enemy_Reverse
        if (playerAAction == ActionType.Enemy_Reverse)
        {
            if (playerBAction == ActionType.Self_Add_1)
            {
                finalPlayerBAction = ActionType.Self_Minus_1;
                Debug.Log("玩家A的Enemy_Reverse生效：玩家B的Self_Add_1变成Self_Minus_1");
            }
            else if (playerBAction == ActionType.Self_Minus_1)
            {
                finalPlayerBAction = ActionType.Self_Add_1;
                Debug.Log("玩家A的Enemy_Reverse生效：玩家B的Self_Minus_1变成Self_Add_1");
            }
            else
            {
                Debug.Log("玩家A的Enemy_Reverse无效：玩家B没有选择Self_Add_1或Self_Minus_1");
            }
            finalPlayerAAction = ActionType.Nothing; // Enemy_Reverse本身不产生位置效果
        }
        
        // 检查玩家B是否选择了Enemy_Reverse
        if (playerBAction == ActionType.Enemy_Reverse)
        {
            if (playerAAction == ActionType.Self_Add_1)
            {
                finalPlayerAAction = ActionType.Self_Minus_1;
                Debug.Log("玩家B的Enemy_Reverse生效：玩家A的Self_Add_1变成Self_Minus_1");
            }
            else if (playerAAction == ActionType.Self_Minus_1)
            {
                finalPlayerAAction = ActionType.Self_Add_1;
                Debug.Log("玩家B的Enemy_Reverse生效：玩家A的Self_Minus_1变成Self_Add_1");
            }
            else
            {
                Debug.Log("玩家B的Enemy_Reverse无效：玩家A没有选择Self_Add_1或Self_Minus_1");
            }
            finalPlayerBAction = ActionType.Nothing; // Enemy_Reverse本身不产生位置效果
        }
        
        Debug.Log($"Enemy_Reverse处理后的最终行动 - A: {finalPlayerAAction}, B: {finalPlayerBAction}");
        
        // 第二步：结算两名玩家的位置
        // 记录行动前的位置，用于同时计算两个行动的效果
        int initialPlayerAPosition = playerACurrentPosition;
        int initialPlayerBPosition = playerBCurrentPosition;
        
        // 计算玩家A的行动对两个玩家位置的影响
        int playerAActionEffectOnA = 0;
        int playerAActionEffectOnB = 0;
        CalculateActionEffect(finalPlayerAAction, true, initialPlayerAPosition, initialPlayerBPosition, 
                            out playerAActionEffectOnA, out playerAActionEffectOnB);
        
        // 计算玩家B的行动对两个玩家位置的影响
        int playerBActionEffectOnA = 0;
        int playerBActionEffectOnB = 0;
        CalculateActionEffect(finalPlayerBAction, false, initialPlayerAPosition, initialPlayerBPosition, 
                            out playerBActionEffectOnA, out playerBActionEffectOnB);
        
        // 同时应用所有效果到玩家位置
        playerACurrentPosition = initialPlayerAPosition + playerAActionEffectOnA + playerBActionEffectOnA;
        playerBCurrentPosition = initialPlayerBPosition + playerAActionEffectOnB + playerBActionEffectOnB;
        
        Debug.Log($"玩家位置结算后 - A: {playerACurrentPosition}, B: {playerBCurrentPosition}");
        
        // 第三步：结算宝箱位置（重力效果）
        int oldChestPosition = chestCurrentPosition;
        chestCurrentPosition = chestCurrentPosition + (playerACurrentPosition + playerBCurrentPosition);
        Debug.Log($"宝箱位置结算：{oldChestPosition} + ({playerACurrentPosition} + {playerBCurrentPosition}) = {chestCurrentPosition}");
        
        Debug.Log($"最终位置 - A: {playerACurrentPosition}, B: {playerBCurrentPosition}, 宝箱: {chestCurrentPosition}");

        // 让UI更新相关位置
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.UpdatePositions(playerACurrentPosition, playerBCurrentPosition, chestCurrentPosition);
        }
        else
        {
            Debug.LogError("未找到UIManager组件");
        }
        
        // 第四步：判断是否触发结局
        // 首先检查宝箱移动过程中是否经过玩家位置（触发选择阶段）
        if (CheckPositionOverlap(oldChestPosition, chestCurrentPosition))
        {
            Debug.Log("宝箱移动过程中经过了玩家位置，触发选择阶段");
            return true; // 需要进入选择阶段
        }
        
        // 然后检查位置是否越界（触发FailEnd条件）
        if (playerACurrentPosition < minPosition || playerACurrentPosition > maxPosition ||
            playerBCurrentPosition < minPosition || playerBCurrentPosition > maxPosition)
        {
            Debug.Log($"玩家位置越界 - A: {playerACurrentPosition}, B: {playerBCurrentPosition}, 范围: [{minPosition}, {maxPosition}]");
            Debug.Log("触发FailEnd");
            GameManager.Instance.SwitchScene(GameState.FailEnd);
            return false; // 游戏直接结束，不需要选择阶段
        }
        
        UpdateUI();
        return false; // 正常继续游戏
    }

    /// <summary>
    /// 计算单个行动对两个玩家位置的影响
    /// </summary>
    /// <param name="action">行动类型</param>
    /// <param name="isPlayerA">是否为玩家A的行动</param>
    /// <param name="initialPlayerAPosition">行动前玩家A的位置</param>
    /// <param name="initialPlayerBPosition">行动前玩家B的位置</param>
    /// <param name="effectOnA">对玩家A位置的影响</param>
    /// <param name="effectOnB">对玩家B位置的影响</param>
    void CalculateActionEffect(ActionType action, bool isPlayerA, int initialPlayerAPosition, int initialPlayerBPosition,
                              out int effectOnA, out int effectOnB)
    {
        effectOnA = 0;
        effectOnB = 0;
        
        switch (action)
        {
            case ActionType.Self_Add_1:
                if (isPlayerA)
                    effectOnA = 1;
                else
                    effectOnB = 1;
                break;
                
            case ActionType.Self_Minus_1:
                if (isPlayerA)
                    effectOnA = -1;
                else
                    effectOnB = -1;
                break;
                
            case ActionType.Enemy_Add_1:
                if (isPlayerA)
                    effectOnB = 1;
                else
                    effectOnA = 1;
                break;
                
            case ActionType.Enemy_Minus_1:
                if (isPlayerA)
                    effectOnB = -1;
                else
                    effectOnA = -1;
                break;
                
            case ActionType.Enemy_Reverse:
                // Enemy_Reverse不在这里处理位置效果，在ProcessActions中已经处理了行动反转
                // 这里应该被替换为ActionType.Nothing
                break;
                
            case ActionType.Nothing:
                // 不做任何操作，效果为0
                break;
        }
        
        Debug.Log($"行动效果计算 - {(isPlayerA ? "玩家A" : "玩家B")}的{action}: 对A的影响={effectOnA}, 对B的影响={effectOnB}");
    }

    /// <summary>
    /// 检查位置重合 - 检查宝箱移动路径是否经过玩家位置
    /// </summary>
    /// <param name="oldChestPosition">宝箱移动前的位置</param>
    /// <param name="newChestPosition">宝箱移动后的位置</param>
    /// <returns>如果宝箱移动路径经过任何玩家位置则返回true</returns>
    bool CheckPositionOverlap(int oldChestPosition, int newChestPosition)
    {
        // 检查宝箱移动路径上是否经过玩家位置
        int start = Mathf.Min(oldChestPosition, newChestPosition);
        int end = Mathf.Max(oldChestPosition, newChestPosition);
        
        // 重置遇到宝箱的状态
        playerAReachedChest = false;
        playerBReachedChest = false;
        
        // 检查路径上的每个位置是否与玩家重合
        for (int pos = start; pos <= end; pos++)
        {
            if (pos == playerACurrentPosition)
            {
                Debug.Log($"宝箱从位置{oldChestPosition}移动到{newChestPosition}的过程中经过了玩家A位置{pos}");
                playerAReachedChest = true;
            }
            if (pos == playerBCurrentPosition)
            {
                Debug.Log($"宝箱从位置{oldChestPosition}移动到{newChestPosition}的过程中经过了玩家B位置{pos}");
                playerBReachedChest = true;
            }
        }
        
        return playerAReachedChest || playerBReachedChest;
    }

    /// <summary>
    /// 处理选择阶段
    /// </summary>
    IEnumerator HandleChoicePhase()
    {
        currentGameState = ChallengeGameState.Choice;
        if (choicePanel != null)
            choicePanel.SetActive(true);
        UpdateUI(); // 更新UI以高亮遇到宝箱的玩家
        
        // 等待玩家选择
        bool hasChosen = false;
        while (!hasChosen)
        {
            yield return null;
            hasChosen = (currentGameState != ChallengeGameState.Choice);
        }
    }

    /// <summary>
    /// 处理选择结果
    /// </summary>
    void OnChoiceSelected(bool share)
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);
            
        if (share)
        {
            Debug.Log("玩家选择分享 - WinWinEnd");
            GameManager.Instance.SwitchScene(GameState.WinWinEnd);
        }
        else
        {
            Debug.Log("玩家选择不分享 - FailEnd");
            GameManager.Instance.SwitchScene(GameState.FailEnd);
        }
    }

    /// <summary>
    /// 将十六进制颜色字符串转换为Color
    /// </summary>
    /// <param name="hexColor">十六进制颜色字符串（如#FFFFFF）</param>
    /// <returns>对应的Color对象</returns>
    Color HexToColor(string hexColor)
    {
        Color color = Color.white; // 默认白色
        
        try
        {
            // 确保十六进制字符串以#开头
            if (!hexColor.StartsWith("#"))
            {
                hexColor = "#" + hexColor;
            }
            
            // 使用Unity的ColorUtility.TryParseHtmlString方法解析
            if (ColorUtility.TryParseHtmlString(hexColor, out color))
            {
                Debug.Log($"成功解析颜色: {hexColor} -> {color}");
            }
            else
            {
                Debug.LogError($"无法解析颜色字符串: {hexColor}，使用默认白色");
                color = Color.white;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"解析颜色时发生异常: {hexColor}, 错误: {e.Message}，使用默认白色");
            color = Color.white;
        }
        
        return color;
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    void UpdateUI()
    {
        if (roundText != null)
            roundText.text = $"{currentRound}";

        if (gameStateText != null)
        {
            switch (currentGameState)
            {
                case ChallengeGameState.RoundStart:
                    gameStateText.text = "Round Start";
                    break;
                case ChallengeGameState.PlayerInput:
                    if (isPlayerATurn && !playerAHasChosen)
                        gameStateText.text = "Waiting for Player A (Press 1-6)";
                    else if (!isPlayerATurn && !playerBHasChosen)
                        gameStateText.text = "Waiting for Player B (Press 1-6)";
                    else if (playerAHasChosen && playerBHasChosen)
                        gameStateText.text = "Both players have chosen";
                    else
                        gameStateText.text = "Waiting for player input";
                    break;
                case ChallengeGameState.Calculating:
                    gameStateText.text = "Calculating...";
                    break;
                case ChallengeGameState.Choice:
                    if (playerAReachedChest && playerBReachedChest)
                        gameStateText.text = "Both players reached the chest! Choose whether to share";
                    else if (playerAReachedChest)
                        gameStateText.text = "Player A reached the chest! Choose whether to share";
                    else if (playerBReachedChest)
                        gameStateText.text = "Player B reached the chest! Choose whether to share";
                    else
                        gameStateText.text = "Player reached the chest! Choose whether to share";
                    break;
            }
        }

        // 高亮当前轮到的玩家或遇到宝箱的玩家 - 通过字体大小和颜色变化
        if (playerANameText != null)
        {
            bool shouldHighlightA = false;
            
            // 在选择阶段，高亮遇到宝箱的玩家
            if (currentGameState == ChallengeGameState.Choice && playerAReachedChest)
            {
                shouldHighlightA = true;
            }
            // 在输入阶段，高亮当前轮到的玩家
            else if (currentGameState == ChallengeGameState.PlayerInput && isPlayerATurn && !playerAHasChosen)
            {
                shouldHighlightA = true;
            }
            
            // 应用字体大小和颜色变化
            playerANameText.fontSize = shouldHighlightA ? playerANameOriginalFontSize * 2f : playerANameOriginalFontSize;
            playerANameText.color = shouldHighlightA ? playerAHighlightColor : playerANormalColor;
        }
        
        if (playerBNameText != null)
        {
            bool shouldHighlightB = false;
            
            // 在选择阶段，高亮遇到宝箱的玩家
            if (currentGameState == ChallengeGameState.Choice && playerBReachedChest)
            {
                shouldHighlightB = true;
            }
            // 在输入阶段，高亮当前轮到的玩家
            else if (currentGameState == ChallengeGameState.PlayerInput && !isPlayerATurn && !playerBHasChosen)
            {
                shouldHighlightB = true;
            }
            
            // 应用字体大小和颜色变化
            playerBNameText.fontSize = shouldHighlightB ? playerBNameOriginalFontSize * 2f : playerBNameOriginalFontSize;
            playerBNameText.color = shouldHighlightB ? playerBHighlightColor : playerBNormalColor;
        }
    }
}

public enum ActionType
{
    Self_Add_1,      // 1: 自己位置+1
    Self_Minus_1,    // 2: 自己位置-1
    Enemy_Add_1,     // 3: 敌人位置+1
    Enemy_Minus_1,   // 4: 敌人位置-1
    Enemy_Reverse,   // 5: 敌人原本若选择Self_Add_1，则强制敌人选择Self_Minus_1；敌人原本若选择Self_Minus_1，则强制敌人选择Self_Add_1
    Nothing          // 6: 什么都不做
}

public enum ChallengeGameState
{
    RoundStart,      // 回合开始
    PlayerInput,     // 玩家输入阶段
    Calculating,     // 结算中
    Choice           // 选择阶段（分享与否）
}