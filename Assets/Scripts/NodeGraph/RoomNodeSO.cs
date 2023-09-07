using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;  // GUID
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();  // 本项目中每个节点只有一个父节点
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code
#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    /// <summary>
    /// 初始化节点
    /// </summary>
    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        // 加载房间节点类型列表
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// 使用指定样式绘制节点
    /// </summary>
    public void Draw(GUIStyle nodeStyle)
    {
        // 绘制节点盒子使用 Begin Area
        GUILayout.BeginArea(rect, nodeStyle);

        // 开启一个新的代码块去检查 GUI 更新
        EditorGUI.BeginChangeCheck();

        // 如果房间节点有父节点 或者 类型为入口，那就显示标签而不显示弹出窗口
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            // 显示标签且不能更改
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // 使用可选择的 RoomNodeType 名称值显示一个弹出窗口（默认为当前设置的 roomNodeType）
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            // 如果房间类型选择已更改，则使子连接可能无效
            // 运算符优先级参考文档：https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/operators/#operator-precedence
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isBoosRoom && roomNodeTypeList.list[selection].isBoosRoom ||
                roomNodeTypeList.list[selected].isNone)
            {
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        // 获取子房间节点
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        // 如果子房间节点不是 null
                        if (childRoomNode != null)
                        {
                            // 删除子ID 从 父房间节点
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                            // 删除父ID 从 子房间节点
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }
                    }
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            // 标记目标对象（保存），修改对象而不创建撤消条目（不能删除）
            // EditorUtility.SetDirty 的作用是将当前目标对象标记为已修改，并将其标记为需要保存。
            // 这意味着在 Unity 编辑器中如果没有保存对目标对象的修改，这些修改也不会丢失。
            // 当使用 EditorUtility.SetDirty 时，Unity 编辑器将在退出编辑模式或保存场景时自动保存对目标对象的修改。
            // 这个方法通常在自定义编辑器脚本中使用，以确保对目标对象的修改被正确保存。
            EditorUtility.SetDirty(this);
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// 用可选择的要显示的房间节点类型填充字符串数组
    /// </summary>
    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }

        return roomArray;
    }

    /// <summary>
    /// 处理节点事件
    /// </summary>
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            // 处理鼠标按下事件
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            // 处理鼠标松开事件
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            // 处理鼠标拖动事件
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 处理鼠标按下事件
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // 左键按下
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        // 右键按下
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    /// <summary>
    /// 处理左键按下事件
    /// </summary>
    private void ProcessLeftClickDownEvent()
    {
        // 设置实际对象选择
        Selection.activeObject = this;

        // 切换节点选择
        if (isSelected == true)
        {
            isSelected = false;
        }
        else
        {
            isSelected = true;
        }
    }

    /// <summary>
    /// 处理右键按下
    /// </summary>
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    /// <summary>
    /// 处理鼠标松开事件
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // 左键松开
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    /// <summary>
    /// 处理鼠标左键松开事件
    /// </summary>
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    /// <summary>
    /// 处理鼠标拖动事件
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // 左键拖动
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    /// <summary>
    /// 处理鼠标左键拖动事件
    /// </summary>
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// 拖动节点
    /// </summary>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// 为节点添加子id（如果节点已添加则返回true，否则返回false）
    /// </summary>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        // 检查子节点是否可以有效地添加到父节点
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检查子节点是否可以有效地添加到父节点 —— 如果可以返回 true，否则返回 false
    /// </summary>
    public bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossNodeAlready = false;
        // 检查节点图中是否已经有一个连接的boss房间
        foreach (var roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBoosRoom && roomNode.parentRoomNodeIDList.Count > 0)
                isConnectedBossNodeAlready = true;
        }

        // 如果子节点类型是 Boss 房间，并且存在一个已经连接的 Boss 房间，返回 false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBoosRoom && isConnectedBossNodeAlready)
            return false;

        // 如果子节点类型是 None，返回 false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;

        // 如果该节点已经包含过该节点了，返回 false
        if (childRoomNodeIDList.Contains(childID))
            return false;

        // 如果该节点ID 是 子节点ID，返回 false
        if (id == childID)
            return false;

        // 如果子节点ID在父节点列表中，返回 false
        if (parentRoomNodeIDList.Contains(childID))
            return false;

        // 如果子节点已经有一个父节点了，返回 false
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
            return false;

        // 如果子节点是一个走廊（corridor）并且该节点也是一个走廊（corridor），返回 false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        // 如果子节点和该节点都不是走廊，返回 false
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        // 如果添加走廊，则检查子节点是否 小于 允许的最大子走廊
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
            return false;

        // 如果子节点是入口，返回 false（入口不能是子节点）
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;

        // 如果向走廊添加一个房间，检查该走廊节点是否已经添加了一个房间（走廊只能连接一个房间）
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        return true;
    }

    /// <summary>
    /// 为节点添加父id（如果节点已添加则返回true，否则返回false）
    /// </summary>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// 从节点中删除子id（如果节点已被删除则返回 true，否则返回 false）
    /// </summary>
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        // 如果含有该子节点，则删除
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 从节点中删除父id（如果节点已被删除则返回 true，否则返回 false）
    /// </summary>
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        // 如果含有该父节点，则删除
        if (parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }
        return false;
    }

#endif
    #endregion
}
