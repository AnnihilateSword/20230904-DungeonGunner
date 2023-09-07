using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{
    /// <summary>
    /// ����Ƿ�Ϊ���ַ���
    /// </summary>
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (stringToCheck == "")
        {
            Debug.Log(fieldName + " �ǿյģ����ұ����� object" + thisObject.name.ToString() + " �а���һ��ֵ");
            return true;
        }
        return false;
    }

    /// <summary>
    /// �б�Ϊ�ջ������ֵ��� - ������ִ����򷵻�true
    /// </summary>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null)
            {
                Debug.Log(fieldName + " �� object " + thisObject.name.ToString() + " ���� null ֵ");
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0)
        {
            Debug.Log(fieldName + " �� object " + thisObject.name.ToString() + " ��û��ֵ");
            error = true;
        }

        return error;
    }
}
