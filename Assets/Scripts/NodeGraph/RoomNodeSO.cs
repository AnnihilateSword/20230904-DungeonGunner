using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;  // GUID
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();  // ����Ŀ��ÿ���ڵ�ֻ��һ�����ڵ�
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
    /// ��ʼ���ڵ�
    /// </summary>
    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        // ���ط���ڵ������б�
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// ʹ��ָ����ʽ���ƽڵ�
    /// </summary>
    public void Draw(GUIStyle nodeStyle)
    {
        // ���ƽڵ����ʹ�� Begin Area
        GUILayout.BeginArea(rect, nodeStyle);

        // ����һ���µĴ����ȥ��� GUI ����
        EditorGUI.BeginChangeCheck();

        // �������ڵ��и��ڵ� ���� ����Ϊ��ڣ��Ǿ���ʾ��ǩ������ʾ��������
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            // ��ʾ��ǩ�Ҳ��ܸ���
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // ʹ�ÿ�ѡ��� RoomNodeType ����ֵ��ʾһ���������ڣ�Ĭ��Ϊ��ǰ���õ� roomNodeType��
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            // �����������ѡ���Ѹ��ģ���ʹ�����ӿ�����Ч
            // ��������ȼ��ο��ĵ���https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/operators/#operator-precedence
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isBoosRoom && roomNodeTypeList.list[selection].isBoosRoom ||
                roomNodeTypeList.list[selected].isNone)
            {
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        // ��ȡ�ӷ���ڵ�
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        // ����ӷ���ڵ㲻�� null
                        if (childRoomNode != null)
                        {
                            // ɾ����ID �� ������ڵ�
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                            // ɾ����ID �� �ӷ���ڵ�
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }
                    }
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            // ���Ŀ����󣨱��棩���޸Ķ����������������Ŀ������ɾ����
            // EditorUtility.SetDirty �������ǽ���ǰĿ�������Ϊ���޸ģ���������Ϊ��Ҫ���档
            // ����ζ���� Unity �༭�������û�б����Ŀ�������޸ģ���Щ�޸�Ҳ���ᶪʧ��
            // ��ʹ�� EditorUtility.SetDirty ʱ��Unity �༭�������˳��༭ģʽ�򱣴泡��ʱ�Զ������Ŀ�������޸ġ�
            // �������ͨ�����Զ���༭���ű���ʹ�ã���ȷ����Ŀ�������޸ı���ȷ���档
            EditorUtility.SetDirty(this);
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// �ÿ�ѡ���Ҫ��ʾ�ķ���ڵ���������ַ�������
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
    /// ����ڵ��¼�
    /// </summary>
    public void ProcessEvents(Event currentEvent)
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
    /// ������갴���¼�
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // �������
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        // �Ҽ�����
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    /// <summary>
    /// ������������¼�
    /// </summary>
    private void ProcessLeftClickDownEvent()
    {
        // ����ʵ�ʶ���ѡ��
        Selection.activeObject = this;

        // �л��ڵ�ѡ��
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
    /// �����Ҽ�����
    /// </summary>
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    /// <summary>
    /// ��������ɿ��¼�
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // ����ɿ�
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    /// <summary>
    /// �����������ɿ��¼�
    /// </summary>
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    /// <summary>
    /// ��������϶��¼�
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // ����϶�
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    /// <summary>
    /// �����������϶��¼�
    /// </summary>
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// �϶��ڵ�
    /// </summary>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Ϊ�ڵ������id������ڵ�������򷵻�true�����򷵻�false��
    /// </summary>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        // ����ӽڵ��Ƿ������Ч����ӵ����ڵ�
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// ����ӽڵ��Ƿ������Ч����ӵ����ڵ� ���� ������Է��� true�����򷵻� false
    /// </summary>
    public bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossNodeAlready = false;
        // ���ڵ�ͼ���Ƿ��Ѿ���һ�����ӵ�boss����
        foreach (var roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBoosRoom && roomNode.parentRoomNodeIDList.Count > 0)
                isConnectedBossNodeAlready = true;
        }

        // ����ӽڵ������� Boss ���䣬���Ҵ���һ���Ѿ����ӵ� Boss ���䣬���� false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBoosRoom && isConnectedBossNodeAlready)
            return false;

        // ����ӽڵ������� None������ false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;

        // ����ýڵ��Ѿ��������ýڵ��ˣ����� false
        if (childRoomNodeIDList.Contains(childID))
            return false;

        // ����ýڵ�ID �� �ӽڵ�ID������ false
        if (id == childID)
            return false;

        // ����ӽڵ�ID�ڸ��ڵ��б��У����� false
        if (parentRoomNodeIDList.Contains(childID))
            return false;

        // ����ӽڵ��Ѿ���һ�����ڵ��ˣ����� false
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
            return false;

        // ����ӽڵ���һ�����ȣ�corridor�����Ҹýڵ�Ҳ��һ�����ȣ�corridor�������� false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        // ����ӽڵ�͸ýڵ㶼�������ȣ����� false
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        // ���������ȣ������ӽڵ��Ƿ� С�� ��������������
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
            return false;

        // ����ӽڵ�����ڣ����� false����ڲ������ӽڵ㣩
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;

        // ������������һ�����䣬�������Ƚڵ��Ƿ��Ѿ������һ�����䣨����ֻ������һ�����䣩
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        return true;
    }

    /// <summary>
    /// Ϊ�ڵ���Ӹ�id������ڵ�������򷵻�true�����򷵻�false��
    /// </summary>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// �ӽڵ���ɾ����id������ڵ��ѱ�ɾ���򷵻� true�����򷵻� false��
    /// </summary>
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        // ������и��ӽڵ㣬��ɾ��
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }

    /// <summary>
    /// �ӽڵ���ɾ����id������ڵ��ѱ�ɾ���򷵻� true�����򷵻� false��
    /// </summary>
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        // ������иø��ڵ㣬��ɾ��
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
