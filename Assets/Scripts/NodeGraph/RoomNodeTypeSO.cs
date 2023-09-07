using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;  // �ڵ���������

    #region Header
    [Header("�Ƿ��ڱ༭���пɼ��ķ���ڵ����ͣ����Ҽ��ܷ񴴽���")]
    #endregion Header
    public bool displayInNodeGraphEditor = true;
    #region Header
    [Header("�Ƿ�����������")]
    #endregion Header
    public bool isCorridor;
    #region Header
    [Header("�Ƿ����ϱ����������")]
    #endregion Header
    public bool isCorridorNS;
    #region Header
    [Header("�Ƿ��Ƕ������������")]
    #endregion Header
    public bool isCorridorEW;
    #region Header
    [Header("�Ƿ������")]
    #endregion Header
    public bool isEntrance;
    #region Header
    [Header("�Ƿ��� Boos ����")]
    #endregion Header
    public bool isBoosRoom;
    #region Header
    [Header("�Ƿ��ǿգ�δ����ģ�")]
    #endregion Header
    public bool isNone;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
    #endregion
}
