using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;  // UnityEditor �ص�
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

    // �ڵ㲼��ֵ
    private const float nodeWidth = 160.0f;
    private const float nodeHeight = 75.0f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    // ������ֵ
    private const float connectingLineWidth = 3.0f;
    private const float connectingLineArrawSize = 6.0f;

    // ������
    private const float gridLarge = 100.0f;
    private const float gridSmall = 25.0f;

    [MenuItem("Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        // ���ļ����ѡ������¼�
        Selection.selectionChanged += InspectorSelectionChanged;

        // ����ڵ㲼����ʽ
        roomNodeStyle = new GUIStyle();
        // node1(��) node2(����ɫ) node3(��) node4(��) node5(��) node6(��)
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // ����ѡ�нڵ���ʽ
        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // ���� Room node types
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        // ȡ�����ļ����ѡ������¼�
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    /// <summary>
    /// ����ڼ������˫���˷���ڵ�ͼ�ɽű�������Դ����򿪷���ڵ�ͼ�༭������
    /// </summary>
    [OnOpenAsset(0)]  // ��Ҫ���������ռ� UnityEditor.Callbacks
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            // ����Ƿ���ڵ�ͼ���󣬾ʹ򿪱༭��
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }

    /// <summary>
    /// ���Ʊ༭�� GUI
    /// </summary>
    private void OnGUI()
    {
        // ���ѡ��������Ϊ RoomNodeGraphSO �Ŀɽű���������
        if (currentRoomNodeGraph != null)
        {
            // ��������
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            // ����קʱ��һ����
            DrawDraggedLine();

            // �����¼�
            ProcessEvents(Event.current);

            // ���Ʒ���ڵ�֮�������
            DrawRoomConnections();

            // ���Ʒ���ڵ�
            DrawRoomNodes();
        }

        if (GUI.changed)
            Repaint();
    }

    /// <summary>
    /// Ϊ����ڵ�ͼ�༭�����Ʊ�������
    /// </summary>
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0.0f);

        // ��ֱ��
        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0.0f) + gridOffset,
                new Vector3(gridSize * i, position.height + gridSize, 0.0f) + gridOffset);
        }

        // ˮƽ��
        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0.0f) + gridOffset,
                new Vector3(position.width + gridSize, gridSize * j, 0.0f) + gridOffset);
        }

        // ������ɫ
        Handles.color = Color.white;
    }

    /// <summary>
    /// ����קʱ��һ����
    /// </summary>
    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            // �ӽڵ㵽��λ�û�����
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                Color.white, null, connectingLineWidth);
        }
    }
    private void ProcessEvents(Event currentEvent)
    {
        // ����ͼ�϶�����
        graphDrag = Vector2.zero;

        // ��ȡ������ڵķ���ڵ㣬�����Ϊ�ջ�ǰδ���϶�
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // �����겻�ڷ���ڵ��� ���� ����Ŀǰ���ڴӷ���ڵ��϶�һ���ߣ�Ȼ����ͼ���¼�
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            // ������ڵ��¼�
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    /// <summary>
    /// ������ʱ���ڽڵ��ϣ�����Ǿͷ��ظýڵ㣬���򷵻� null
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
    /// ������ڵ�ͼ�¼�
    /// </summary>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            // ������갴���¼�
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            // ��������ɿ��¼�
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            // ��������϶��¼�
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// ������ڵ�ͼ�е���갴���¼���������һ���ڵ��ϣ�
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // ������ڵ�ͼ�е�����Ҽ������¼�����ʾ�����Ĳ˵���
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
    /// ��ʾ�����Ĳ˵�
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
    /// �����λ�ô�������ڵ�
    /// </summary>
    private void CreateRoomNode(object mousePositionObject)
    {
        // �����ǰ�ڵ�ͼΪ���Ǿ��������ڷ���ڵ�
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200.0f, 200.0f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    /// <summary>
    /// �����λ�ô�������ڵ� - ���ز����� RoomNodeType
    /// </summary>
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // ��������ڵ�ɱ�̵Ķ�����Դ
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // ��ӷ���ڵ㵽��ǰ����ڵ�ͼ�ķ���ڵ��б�
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // ���÷���ڵ�ֵ
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        // ��ӷ���ڵ㵽����ڵ�ͼ�ɱ�̶�����Դ���ݿ⣨���Ӷ���
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        currentRoomNodeGraph.OnValidate();
    }

    /// <summary>
    /// ɾ��ѡ�еķ���ڵ�
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        // �ڵ���������ɾ���ڵ�������⣬����ʹ��һ�����л��潫Ҫɾ���Ľڵ�
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);

                // �����ӷ���ڵ� ID
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // �����ӷ���ڵ�
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if (childRoomNode != null)
                    {
                        // ɾ����ID���ӷ���ڵ�
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                // ����������ڵ� ID
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    // �����ӷ���ڵ�
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        // ɾ����ID���ӷ���ڵ�
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // ɾ�����л���ķ���ڵ�
        while (roomNodeDeletionQueue.Count > 0)
        {
            // �Ӷ����л�ȡ����ڵ�
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            // ɾ���ֵ��еĽڵ�
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            // ɾ���б��еĽڵ�
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            // ɾ���ʲ����ݿ��еĽڵ�
            DestroyImmediate(roomNodeToDelete, true);

            // �����ʲ����ݿ�
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// ɾ��ѡ�еķ���ڵ�֮�������
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        // �������з���ڵ�
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    // ��ȡ�ӷ���ڵ�
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    // ����ӷ���ڵ㱻ѡ��
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        // ɾ����ID �� ������ڵ�
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                        // ɾ����ID �� �ӷ���ڵ�
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // �������ѡ�еķ���ڵ�
        ClearAllSelectedRoomNodes();
    }

    /// <summary>
    /// ������з���ڵ��ѡ��
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
    /// ѡ�����з���ڵ�
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
    /// ��������ɿ��¼�
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // ����ɿ�����Ҽ�����ǰ�����϶�һ����
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            // ����Ƿ��ڷ���ڵ���
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if (roomNode != null)
            {
                // ������ԵĻ���������Ϊ������ڵ���ӽڵ㣨���������ӣ�
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    // ���ӷ���ڵ������ø�id
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    /// <summary>
    /// ��������϶��¼�
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // �����Ҽ�����϶��¼� ���� ����
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        // �����������϶��¼� ���� �϶��ڵ�ͼ
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    /// <summary>
    /// �����Ҽ�����϶��¼� ���� ����
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
    /// �����������϶��¼� ���� �϶��ڵ�ͼ
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
    /// �ӷ���ڵ��϶�������
    /// </summary>
    public void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// ����ӷ���ڵ��϶�����
    /// </summary>
    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// ��ͼ�δ����л��Ʒ���ڵ�֮�������
    /// </summary>
    private void DrawRoomConnections()
    {
        // �������з���ڵ�
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                // �������ӷ���ڵ�
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // ���ֵ��л�ȡ�ӷ���ڵ�
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
    /// �ڸ�����ڵ���ӷ���ڵ�֮�����������
    /// </summary>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        // ��ȡ������ ��ʼλ�� �� ����λ��
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        // �����ص�λ��
        Vector2 midPosition = (endPosition + startPosition) / 2.0f;

        // ��������㵽�յ��λ��
        Vector2 direction = endPosition - startPosition;

        // ���е�����һ���Ĵ�ֱλ��
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrawSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrawSize;

        // ������е�ƫ�Ƶļ�ͷλ��
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrawSize;

        // ����ͷ
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);
        // ���ƴ�ֱ�ߣ������ã�
        //Handles.DrawBezier(midPosition, arrowTailPoint1, midPosition, arrowTailPoint1, Color.white, null, connectingLineWidth);

        // ����
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    /// <summary>
    /// �ڽڵ�ͼ�༭�������л��Ʒ���ڵ�
    /// </summary>
    private void DrawRoomNodes()
    {
        // ѭ���������з���ڵ㲢��������
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
    /// ������е�ѡ�����
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
