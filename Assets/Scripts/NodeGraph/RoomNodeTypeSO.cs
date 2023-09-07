using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;  // 节点类型名称

    #region Header
    [Header("是否在编辑器中可见的房间节点类型（即右键能否创建）")]
    #endregion Header
    public bool displayInNodeGraphEditor = true;
    #region Header
    [Header("是否是走廊类型")]
    #endregion Header
    public bool isCorridor;
    #region Header
    [Header("是否是南北方向的走廊")]
    #endregion Header
    public bool isCorridorNS;
    #region Header
    [Header("是否是东西方向的走廊")]
    #endregion Header
    public bool isCorridorEW;
    #region Header
    [Header("是否是入口")]
    #endregion Header
    public bool isEntrance;
    #region Header
    [Header("是否是 Boos 房间")]
    #endregion Header
    public bool isBoosRoom;
    #region Header
    [Header("是否是空（未分配的）")]
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
