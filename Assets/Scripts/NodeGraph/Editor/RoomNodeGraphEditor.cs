using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;  // UnityEditor 回调
using System.Collections.Generic;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;

    private Vector2 graphOffset;
    private Vector2 graphDrag;

    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;

    // 节点布局值
    private const float nodeWidth = 160.0f;
    private const float nodeHeight = 75.0f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    // 连接线值
    private const float connectingLineWidth = 3.0f;
    private const float connectingLineArrawSize = 6.0f;

    // 网格间距
    private const float gridLarge = 100.0f;
    private const float gridSmall = 25.0f;

    [MenuItem("Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        // 订阅检查器选择更改事件
        Selection.selectionChanged += InspectorSelectionChanged;

        // 定义节点布局样式
        roomNodeStyle = new GUIStyle();
        // node1(蓝) node2(深绿色) node3(绿) node4(黄) node5(橙) node6(红)
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // 定义选中节点样式
        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // 加载 Room node types
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        // 取消订阅检查器选择更改事件
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    /// <summary>
    /// 如果在检查器中双击了房间节点图可脚本对象资源，则打开房间节点图编辑器窗口
    /// </summary>
    [OnOpenAsset(0)]  // 需要引入命名空间 UnityEditor.Callbacks
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            // 如果是房间节点图对象，就打开编辑器
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }

    /// <summary>
    /// 绘制编辑器 GUI
    /// </summary>
    private void OnGUI()
    {
        // 如果选择了类型为 RoomNodeGraphSO 的可脚本对象，则处理
        if (currentRoomNodeGraph != null)
        {
            // 绘制网格
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            // 被拖拽时划一条线
            DrawDraggedLine();

            // 处理事件
            ProcessEvents(Event.current);

            // 绘制房间节点之间的连接
            DrawRoomConnections();

            // 绘制房间节点
            DrawRoomNodes();
        }

        if (GUI.changed)
            Repaint();
    }

    /// <summary>
    /// 为房间节点图编辑器绘制背景网格
    /// </summary>
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0.0f);

        // 垂直线
        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0.0f) + gridOffset,
                new Vector3(gridSize * i, position.height + gridSize, 0.0f) + gridOffset);
        }

        // 水平线
        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0.0f) + gridOffset,
                new Vector3(position.width + gridSize, gridSize * j, 0.0f) + gridOffset);
        }

        // 重置颜色
        Handles.color = Color.white;
    }

    /// <summary>
    /// 被拖拽时划一条线
    /// </summary>
    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            // 从节点到线位置绘制线
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                Color.white, null, connectingLineWidth);
        }
    }
    private void ProcessEvents(Event currentEvent)
    {
        // 重置图拖动变量
        graphDrag = Vector2.zero;

        // 获取鼠标所在的房间节点，如果它为空或当前未被拖动
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // 如果鼠标不在房间节点上 或者 我们目前正在从房间节点拖动一条线，然后处理图形事件
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            // 处理房间节点事件
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    /// <summary>
    /// 检查鼠标时候在节点上，如果是就返回该节点，否则返回 null
    /// </summary>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }

        return null;
    }

    /// <summary>
    /// 处理房间节点图事件
    /// </summary>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
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
    /// 处理房间节点图中的鼠标按下事件（不是在一个节点上）
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // 处理房间节点图中的鼠标右键按下事件（显示上下文菜单）
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// 显示上下文菜单
    /// </summary>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);

        menu.ShowAsContext();
    }

    /// <summary>
    /// 在鼠标位置创建房间节点
    /// </summary>
    private void CreateRoomNode(object mousePositionObject)
    {
        // 如果当前节点图为空那就先添加入口房间节点
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200.0f, 200.0f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    /// <summary>
    /// 在鼠标位置创建房间节点 - 重载并传递 RoomNodeType
    /// </summary>
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // 创建房间节点可编程的对象资源
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // 添加房间节点到当前房间节点图的房间节点列表
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // 设置房间节点值
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        // 添加房间节点到房间节点图可编程对象资源数据库（做子对象）
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        currentRoomNodeGraph.OnValidate();
    }

    /// <summary>
    /// 删除选中的房间节点
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        // 在迭代过程中删除节点会有问题，所以使用一个队列缓存将要删除的节点
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);

                // 遍历子房间节点 ID
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // 检索子房间节点
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if (childRoomNode != null)
                    {
                        // 删除父ID从子房间节点
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                // 遍历父房间节点 ID
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    // 检索子房间节点
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        // 删除父ID从子房间节点
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // 删除队列缓存的房间节点
        while (roomNodeDeletionQueue.Count > 0)
        {
            // 从队列中获取房间节点
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            // 删除字典中的节点
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            // 删除列表中的节点
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            // 删除资产数据库中的节点
            DestroyImmediate(roomNodeToDelete, true);

            // 保存资产数据库
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// 删除选中的房间节点之间的连接
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        // 遍历所有房间节点
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    // 获取子房间节点
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    // 如果子房间节点被选中
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        // 删除子ID 从 父房间节点
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                        // 删除父ID 从 子房间节点
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // 清除所有选中的房间节点
        ClearAllSelectedRoomNodes();
    }

    /// <summary>
    /// 清除所有房间节点的选择
    /// </summary>
    private void ClearAllSelectedRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;
                GUI.changed = true;
            }
        }
    }

    /// <summary>
    /// 选择所有房间节点
    /// </summary>
    private void SelectAllRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    /// <summary>
    /// 处理鼠标松开事件
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // 如果松开鼠标右键，当前正在拖动一条线
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            // 检查是否在房间节点上
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if (roomNode != null)
            {
                // 如果可以的话将其设置为父房间节点的子节点（如果可以添加）
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    // 在子房间节点中设置父id
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    /// <summary>
    /// 处理鼠标拖动事件
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // 处理右键点击拖动事件 —— 画线
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        // 处理左键点击拖动事件 —— 拖动节点图
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    /// <summary>
    /// 处理右键点击拖动事件 —— 画线
    /// </summary>
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    /// <summary>
    /// 处理左键点击拖动事件 —— 拖动节点图
    /// </summary>
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        graphDrag = dragDelta;

        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(dragDelta);
        }

        GUI.changed = true;
    }


    /// <summary>
    /// 从房间节点拖动连接线
    /// </summary>
    public void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// 清除从房间节点拖动的线
    /// </summary>
    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// 在图形窗口中绘制房间节点之间的连接
    /// </summary>
    private void DrawRoomConnections()
    {
        // 遍历所有房间节点
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                // 遍历孩子房间节点
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // 从字典中获取子房间节点
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);
                        GUI.changed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 在父房间节点和子房间节点之间绘制连接线
    /// </summary>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        // 获取线条的 开始位置 和 结束位置
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        // 计算重点位置
        Vector2 midPosition = (endPosition + startPosition) / 2.0f;

        // 向量从起点到终点的位置
        Vector2 direction = endPosition - startPosition;

        // 从中点计算归一化的垂直位置
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrawSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrawSize;

        // 计算从中点偏移的箭头位置
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrawSize;

        // 画箭头
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);
        // 绘制垂直线（调试用）
        //Handles.DrawBezier(midPosition, arrowTailPoint1, midPosition, arrowTailPoint1, Color.white, null, connectingLineWidth);

        // 画线
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    /// <summary>
    /// 在节点图编辑器窗口中绘制房间节点
    /// </summary>
    private void DrawRoomNodes()
    {
        // 循环遍历所有房间节点并绘制它们
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
            {
                roomNode.Draw(roomNodeStyle);
            }
        }

        GUI.changed = true;
    }

    /// <summary>
    /// 检查器中的选择更改
    /// </summary>
    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}
