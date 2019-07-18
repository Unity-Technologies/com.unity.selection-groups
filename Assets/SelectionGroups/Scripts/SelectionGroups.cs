using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct SelectionGroup
{
    public string groupName;
    public List<Object> objects;


}

public class SelectionGroups : MonoBehaviour
{
    public List<SelectionGroup> groups = new List<SelectionGroup>();

}
