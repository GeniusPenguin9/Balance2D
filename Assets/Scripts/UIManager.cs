using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI管理器，负责处理游戏的视觉效果逻辑
/// 管理水平矩形坐标轴，提供旋转和GameObject定位功能
/// 根据玩家A和B的位置计算旋转角度，使游戏对象跟随矩形旋转
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("坐标轴设置")]
    public Image coordinateAxisImage; // 水平的矩形image，作为坐标轴
    public float unitDegree = 3f; // 单位旋转角度，默认3°
    
    [Header("GameObject定位设置")]
    public float playerAHeightOffset = 1.0f; // 玩家A底部相对于矩形上表面的偏移量
    public float playerBHeightOffset = 1.0f; // 玩家B底部相对于矩形上表面的偏移量  
    public float chestHeightOffset = 0.5f; // 宝箱底部相对于矩形上表面的偏移量
    
    [Header("游戏对象引用")]
    public GameObject playerA; // 玩家A的游戏对象
    public GameObject playerB; // 玩家B的游戏对象
    public GameObject chest; // 宝箱游戏对象
    
    // 单例模式
    public static UIManager Instance { get; private set; }
    
    // 私有变量
    private RectTransform axisRectTransform;
    private float axisWidth; // 坐标轴原始宽度
    private float axisActualWidth; // 坐标轴实际显示宽度（考虑缩放）
    private Vector3 axisScale; // 坐标轴的缩放比例
    private int coordinateMin; // 从ChallengeManager获取的坐标轴最小值
    private int coordinateMax; // 从ChallengeManager获取的坐标轴最大值
    
    // 当前位置记录
    private int currentPlayerAPosition = 0;
    private int currentPlayerBPosition = 0;
    private int currentChestPosition = 0;
    private float currentRotationAngle = 0f;
    
    void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeComponents();
    }
    
    void Start()
    {
        Debug.Log("UIManager 初始化完成");
        Debug.Log($"unitDegree设置为: {unitDegree}°");
    }
    
    /// <summary>
    /// 初始化组件
    /// </summary>
    void InitializeComponents()
    {
        if (coordinateAxisImage != null)
        {
            axisRectTransform = coordinateAxisImage.GetComponent<RectTransform>();
            if (axisRectTransform == null)
            {
                Debug.LogError("坐标轴Image没有RectTransform组件！");
            }
            else
            {
                // 获取坐标轴原始宽度和缩放比例
                axisWidth = axisRectTransform.rect.width;
                axisScale = axisRectTransform.localScale;
                
                // 计算实际显示宽度（考虑缩放）
                axisActualWidth = axisWidth * axisScale.x;
                
                Debug.Log($"坐标轴原始宽度: {axisWidth}");
                Debug.Log($"坐标轴缩放比例: {axisScale}");
                Debug.Log($"坐标轴实际显示宽度: {axisActualWidth}");
            }
        }
        else
        {
            Debug.LogWarning("未设置坐标轴Image！");
        }
        
        // 从ChallengeManager获取坐标范围
        ChallengeManager challengeManager = FindObjectOfType<ChallengeManager>();
        if (challengeManager != null)
        {
            coordinateMin = challengeManager.minPosition;
            coordinateMax = challengeManager.maxPosition;
            Debug.Log($"从ChallengeManager获取坐标范围: [{coordinateMin}, {coordinateMax}]");
        }
        else
        {
            Debug.LogError("未找到ChallengeManager，UIManager无法正常工作！");
        }
    }
    
    /// <summary>
    /// 核心接口：更新A、B、宝箱三者的位置
    /// 根据A+B的位置计算旋转角度，并更新所有游戏对象位置
    /// </summary>
    /// <param name="playerAPosition">玩家A的坐标位置</param>
    /// <param name="playerBPosition">玩家B的坐标位置</param>
    /// <param name="chestPosition">宝箱的坐标位置</param>
    public void UpdatePositions(int playerAPosition, int playerBPosition, int chestPosition)
    {
        // 更新当前位置记录
        currentPlayerAPosition = playerAPosition;
        currentPlayerBPosition = playerBPosition;
        currentChestPosition = chestPosition;
        
        // 根据A+B位置计算旋转角度（正值重则顺时针旋转，使用负角度）
        float rotationAngle = -unitDegree * (playerAPosition + playerBPosition);
        currentRotationAngle = rotationAngle;
        
        Debug.Log($"更新位置 - 玩家A: {playerAPosition}, 玩家B: {playerBPosition}, 宝箱: {chestPosition}");
        Debug.Log($"计算旋转角度: -{unitDegree} * ({playerAPosition} + {playerBPosition}) = {rotationAngle}° (正值重->顺时针)");
        
        // 设置坐标轴旋转
        SetAxisRotation(rotationAngle);
        
        // 更新所有游戏对象位置（跟随矩形旋转）
        UpdateGameObjectPositions();
    }
    
    /// <summary>
    /// 设置坐标轴的旋转角度
    /// </summary>
    /// <param name="angle">旋转角度（负角度为顺时针）</param>
    private void SetAxisRotation(float angle)
    {
        if (axisRectTransform != null)
        {
            axisRectTransform.rotation = Quaternion.Euler(0, 0, angle);
            string direction = angle > 0 ? "逆时针" : "顺时针";
            Debug.Log($"设置坐标轴旋转角度: {angle}° ({direction})");
        }
    }
    
    /// <summary>
    /// 更新所有游戏对象位置，使其跟随矩形旋转
    /// </summary>
    private void UpdateGameObjectPositions()
    {
        if (playerA != null)
        {
            PositionGameObjectOnRotatedAxis(playerA, currentPlayerAPosition);
        }
        
        if (playerB != null)
        {
            PositionGameObjectOnRotatedAxis(playerB, currentPlayerBPosition);
        }
        
        if (chest != null)
        {
            PositionGameObjectOnRotatedAxis(chest, currentChestPosition);
        }
    }
    
    /// <summary>
    /// 将GameObject定位到旋转后坐标轴的特定坐标位置
    /// GameObject会跟随矩形旋转，并始终垂直于矩形表面，底面贴着矩形上表面
    /// 考虑坐标轴的缩放因子进行正确的位置计算
    /// </summary>
    /// <param name="gameObject">要定位的GameObject</param>
    /// <param name="coordinate">坐标值（在coordinateMin到coordinateMax范围内）</param>
    private void PositionGameObjectOnRotatedAxis(GameObject gameObject, int coordinate)
    {
        if (gameObject == null)
        {
            Debug.LogError("要定位的GameObject为空！");
            return;
        }
        
        if (axisRectTransform == null)
        {
            Debug.LogError("无法定位GameObject：坐标轴RectTransform为空！");
            return;
        }
        
        // 限制坐标在有效范围内
        coordinate = Mathf.Clamp(coordinate, coordinateMin, coordinateMax);
        
        // 计算坐标在轴上的相对位置（0到1之间）
        float normalizedPosition = (float)(coordinate - coordinateMin) / (coordinateMax - coordinateMin);
        
        // 计算在未旋转坐标轴上的像素偏移（相对于坐标轴中心）
        // 使用实际显示宽度（考虑缩放因子）
        float pixelOffset = (normalizedPosition - 0.5f) * axisActualWidth;
        
        // 计算旋转角度（转换为弧度）
        float rotationRadians = currentRotationAngle * Mathf.Deg2Rad;
        
        // 应用旋转变换到位置偏移（这是在矩形中心线上的位置）
        float rotatedX = pixelOffset * Mathf.Cos(rotationRadians);
        float rotatedY = pixelOffset * Mathf.Sin(rotationRadians);
        
        // 获取坐标轴的世界位置（矩形中心）
        Vector3 axisWorldPosition = axisRectTransform.position;
        
        // 计算GameObject在矩形中心线上的世界位置
        Vector3 centerLinePosition = axisWorldPosition + new Vector3(rotatedX, rotatedY, 0);
        
        // 计算垂直于矩形表面的方向向量（向上）
        // 矩形的法向量随着旋转而改变
        Vector3 surfaceNormal = new Vector3(-Mathf.Sin(rotationRadians), Mathf.Cos(rotationRadians), 0);
        
        // 计算矩形的厚度（高度）的一半，用于找到上表面
        float axisHeight = axisRectTransform.rect.height * axisScale.y;
        float halfHeight = axisHeight * 0.5f;
        
        // 计算矩形上表面的位置
        Vector3 upperSurfacePosition = centerLinePosition + surfaceNormal * halfHeight;
        
        // 根据GameObject类型选择对应的高度偏移量
        float heightOffset = GetHeightOffsetForGameObject(gameObject);
        
        // 计算GameObject的最终位置（在矩形上表面之上）
        // heightOffset代表GameObject底部到中心的距离
        Vector3 targetPosition = upperSurfacePosition + surfaceNormal * heightOffset;
        
        // 设置GameObject位置
        gameObject.transform.position = targetPosition;
        
        // 关键：设置GameObject的旋转，使其垂直于矩形表面
        // 让GameObject跟随矩形旋转，但保持垂直于矩形表面
        gameObject.transform.rotation = Quaternion.Euler(0, 0, currentRotationAngle);
        
        Debug.Log($"GameObject {gameObject.name} 定位到坐标 {coordinate}，" +
                 $"矩形厚度: {axisHeight}，上表面偏移: {halfHeight}，" +
                 $"GameObject偏移: {heightOffset}，" +
                 $"旋转角度 {currentRotationAngle}°，最终位置: {targetPosition}");
    }
    
    /// <summary>
    /// 根据GameObject获取对应的高度偏移量
    /// </summary>
    /// <param name="gameObject">目标GameObject</param>
    /// <returns>对应的高度偏移量</returns>
    private float GetHeightOffsetForGameObject(GameObject gameObject)
    {
        if (gameObject == playerA)
        {
            return playerAHeightOffset;
        }
        else if (gameObject == playerB)
        {
            return playerBHeightOffset;
        }
        else if (gameObject == chest)
        {
            return chestHeightOffset;
        }
        else
        {
            // 默认使用玩家A的偏移量
            Debug.LogWarning($"未知的GameObject: {gameObject.name}，使用默认偏移量");
            return playerAHeightOffset;
        }
    }
    
    /// <summary>
    /// 平滑更新位置的版本，带动画效果
    /// </summary>
    /// <param name="playerAPosition">玩家A的坐标位置</param>
    /// <param name="playerBPosition">玩家B的坐标位置</param>
    /// <param name="chestPosition">宝箱的坐标位置</param>
    /// <param name="duration">动画持续时间</param>
    public void UpdatePositionsSmooth(int playerAPosition, int playerBPosition, int chestPosition, float duration = 0.5f)
    {
        StartCoroutine(SmoothUpdateCoroutine(playerAPosition, playerBPosition, chestPosition, duration));
    }
    
    /// <summary>
    /// 平滑更新位置的协程
    /// </summary>
    private IEnumerator SmoothUpdateCoroutine(int targetPlayerAPos, int targetPlayerBPos, int targetChestPos, float duration)
    {
        // 记录起始状态
        int startPlayerAPos = currentPlayerAPosition;
        int startPlayerBPos = currentPlayerBPosition;
        int startChestPos = currentChestPosition;
        float startRotation = currentRotationAngle;
        
        // 计算目标旋转角度（正值重则顺时针旋转，使用负角度）
        float targetRotation = -unitDegree * (targetPlayerAPos + targetPlayerBPos);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // 使用平滑插值计算当前位置和旋转
            int currentPlayerAPos = Mathf.RoundToInt(Mathf.Lerp(startPlayerAPos, targetPlayerAPos, progress));
            int currentPlayerBPos = Mathf.RoundToInt(Mathf.Lerp(startPlayerBPos, targetPlayerBPos, progress));
            int currentChestPos = Mathf.RoundToInt(Mathf.Lerp(startChestPos, targetChestPos, progress));
            float currentRotation = Mathf.Lerp(startRotation, targetRotation, progress);
            
            // 更新状态（不触发重新计算）
            currentPlayerAPosition = currentPlayerAPos;
            currentPlayerBPosition = currentPlayerBPos;
            currentChestPosition = currentChestPos;
            currentRotationAngle = currentRotation;
            
            // 应用旋转和位置
            SetAxisRotation(currentRotation);
            UpdateGameObjectPositions();
            
            yield return null;
        }
        
        // 确保最终状态精确
        UpdatePositions(targetPlayerAPos, targetPlayerBPos, targetChestPos);
        
        Debug.Log($"平滑更新位置完成 - 玩家A: {targetPlayerAPos}, 玩家B: {targetPlayerBPos}, 宝箱: {targetChestPos}");
    }
    
    /// <summary>
    /// 重置坐标轴和所有游戏对象到初始状态
    /// </summary>
    public void ResetToInitialState()
    {
        UpdatePositions(0, 0, 0);
        Debug.Log("重置到初始状态");
    }
    
    /// <summary>
    /// 获取当前坐标轴的旋转角度
    /// </summary>
    /// <returns>当前旋转角度</returns>
    public float GetCurrentAxisRotation()
    {
        return currentRotationAngle;
    }
    
    /// <summary>
    /// 获取当前玩家A位置
    /// </summary>
    public int GetCurrentPlayerAPosition()
    {
        return currentPlayerAPosition;
    }
    
    /// <summary>
    /// 获取当前玩家B位置
    /// </summary>
    public int GetCurrentPlayerBPosition()
    {
        return currentPlayerBPosition;
    }
    
    /// <summary>
    /// 获取当前宝箱位置
    /// </summary>
    public int GetCurrentChestPosition()
    {
        return currentChestPosition;
    }
    
    /// <summary>
    /// 获取坐标轴的实际显示宽度（考虑缩放）
    /// </summary>
    /// <returns>实际显示宽度</returns>
    public float GetAxisActualWidth()
    {
        return axisActualWidth;
    }
    
    /// <summary>
    /// 获取坐标轴的缩放比例
    /// </summary>
    /// <returns>缩放比例</returns>
    public Vector3 GetAxisScale()
    {
        return axisScale;
    }
    
    /// <summary>
    /// 设置所有GameObject相对于矩形表面的高度偏移量（统一设置）
    /// </summary>
    /// <param name="offset">高度偏移量</param>
    public void SetGameObjectHeightOffset(float offset)
    {
        playerAHeightOffset = offset;
        playerBHeightOffset = offset;
        chestHeightOffset = offset;
        // 重新更新所有GameObject位置以应用新的偏移量
        UpdateGameObjectPositions();
        Debug.Log($"设置所有GameObject高度偏移量为: {offset}");
    }
    
    /// <summary>
    /// 设置玩家A的高度偏移量
    /// </summary>
    /// <param name="offset">高度偏移量</param>
    public void SetPlayerAHeightOffset(float offset)
    {
        playerAHeightOffset = offset;
        UpdateGameObjectPositions();
        Debug.Log($"设置玩家A高度偏移量为: {offset}");
    }
    
    /// <summary>
    /// 设置玩家B的高度偏移量
    /// </summary>
    /// <param name="offset">高度偏移量</param>
    public void SetPlayerBHeightOffset(float offset)
    {
        playerBHeightOffset = offset;
        UpdateGameObjectPositions();
        Debug.Log($"设置玩家B高度偏移量为: {offset}");
    }
    
    /// <summary>
    /// 设置宝箱的高度偏移量
    /// </summary>
    /// <param name="offset">高度偏移量</param>
    public void SetChestHeightOffset(float offset)
    {
        chestHeightOffset = offset;
        UpdateGameObjectPositions();
        Debug.Log($"设置宝箱高度偏移量为: {offset}");
    }
    
    /// <summary>
    /// 获取玩家A的高度偏移量
    /// </summary>
    /// <returns>玩家A的高度偏移量</returns>
    public float GetPlayerAHeightOffset()
    {
        return playerAHeightOffset;
    }
    
    /// <summary>
    /// 获取玩家B的高度偏移量
    /// </summary>
    /// <returns>玩家B的高度偏移量</returns>
    public float GetPlayerBHeightOffset()
    {
        return playerBHeightOffset;
    }
    
    /// <summary>
    /// 获取宝箱的高度偏移量
    /// </summary>
    /// <returns>宝箱的高度偏移量</returns>
    public float GetChestHeightOffset()
    {
        return chestHeightOffset;
    }
    
    /// <summary>
    /// 获取当前GameObject高度偏移量（为了向后兼容，返回玩家A的偏移量）
    /// </summary>
    /// <returns>当前高度偏移量</returns>
    public float GetGameObjectHeightOffset()
    {
        return playerAHeightOffset;
    }
    
    void OnDestroy()
    {
        // 清理单例引用
        if (Instance == this)
        {
            Instance = null;
        }
        
        // 停止所有协程
        StopAllCoroutines();
    }
} 