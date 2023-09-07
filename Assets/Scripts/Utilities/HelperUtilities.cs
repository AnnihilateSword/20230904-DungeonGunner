using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{
    /// <summary>
    /// 检查是否为空字符串
    /// </summary>
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (stringToCheck == "")
        {
            Debug.Log(fieldName + " 是空的，并且必须在 object" + thisObject.name.ToString() + " 中包含一个值");
            return true;
        }
        return false;
    }

    /// <summary>
    /// 列表为空或包含空值检查 - 如果出现错误，则返回true
    /// </summary>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null)
            {
                Debug.Log(fieldName + " 在 object " + thisObject.name.ToString() + " 中有 null 值");
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0)
        {
            Debug.Log(fieldName + " 在 object " + thisObject.name.ToString() + " 中没有值");
            error = true;
        }

        return error;
    }
}
