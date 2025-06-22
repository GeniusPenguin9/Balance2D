# code
GameManager (Scene Swith and singleton resource)
ChallengeManager - main game logic
UIManager - 视觉效果管理(坐标轴、旋转、定位)
AudioManager

# 主要逻辑
- 游戏是倒计时回合制
- 游戏有两个结束条件
  - currentRound归零，此时触发UnknownEnd
  - currentRound未归零，其中一个player和chest发生位置的重合，让用户进行选择
    - 选择"share", 此时触发WinWinEnd
    - 选择"not share", 此时触发FailEnd
  - currentRound未归零，player的位置超过maxPosition，此时触发FailEnd。等于maxPosition时，视为安全。
- 游戏设计为两名玩家使用同一台电脑，假设两位分别为A和B
- 一个回合的流程为
  - 界面显示'回合开始'
  - actionContainer显示
  - UI上A玩家的名字高亮
  - 数字0-5会隐射到ActionType
  - A玩家通过键盘点击数字0-5，点击的内容不在界面上高亮
  - B玩家通过键盘点击数字0-5，点击的内容不在界面上高亮
  - 界面显示'结算中'
  - 根据两者选择的ActionType进行结算，移动改变A和B的位置
  - 检查游戏结束条件，如果满足则跳转到相应结束场景，否则继续下一回合

# 效果逻辑
- ✅ 一个GameObject, 挂载一个水平的矩形image。把这个矩形考虑成坐标轴，-10~10。
- ✅ 提供一个方法，使得image会绕着中心点旋转。
- ✅ 提供一个方法，使得其他的gameObject位于矩形的特定坐标

## UIManager功能说明
- 单例模式管理，提供全局访问接口
- 管理水平矩形坐标轴(-10到10的坐标范围)
- 可以在unity中设定unitDegree，默认为3°
- 暴露一个接口，输入参数为A,B，宝箱三者的位置
- 接口效果应为：根据A/B的位置，旋转矩形。比如A在-5,B在4,此时旋转角度应为 unitDegree*(A current postion + B current position)， 即逆时针旋转3°
- A/B/宝箱，三者可以认为站在矩形上，因此：
  - 绝对位置应跟着矩形的旋转而变化
  - 与矩形的相对位置，由函数输入参数决定


# 已实现功能
- ✅ 游戏状态管理（ChallengeGameState枚举）
- ✅ 回合制流程控制（GameLoop协程）
- ✅ 玩家输入处理（数字键0-5映射到ActionType）
- ✅ 行动结算逻辑（ProcessActions方法）
- ✅ 游戏结束条件检查（位置重合检测和回合数检查）
- ✅ UI更新系统（玩家高亮、状态显示、位置显示）
- ✅ 选择系统（分享/不分享的UI面板）
- ✅ 场景切换集成（通过GameManager切换到结束场景）

# TODO
- [x] 宝箱位置更新逻辑
- [x] 移动动画
- [ ] 木板音效
- [x] 三个结局，图1 -> 等待2秒 -> 图2
- [x] Before Challenge, 依次显示

# ActionType说明
- 0: Self_Add_1 - 自己位置+1
- 1: Self_Minus_1 - 自己位置-1  
- 2: Enemy_Add_1 - 敌人位置+1
- 3: Enemy_Minus_1 - 敌人位置-1
- 4: Enemy_Reverse - 敌人位置取反
- 5: Nothing - 什么都不做

# UI组件需求
ChallengeManager需要在Inspector中设置以下UI组件：
- roundText: 显示当前回合数
- gameStateText: 显示游戏状态信息
- playerANameText: 玩家A名字（会高亮显示轮到的玩家）
- playerBNameText: 玩家B名字
- playerAPositionText: 玩家A位置显示
- playerBPositionText: 玩家B位置显示  
- chestPositionText: 宝箱位置显示
- choicePanel: 选择分享的UI面板
- shareButton: 选择分享的按钮
- notShareButton: 选择不分享的按钮
- actionContainer: 行动选择容器（在玩家输入阶段显示）