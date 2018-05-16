/*******************************************************************************************************
* ReadOnlyAttribute.cs
* script does not need to be attached to any gamobject
* This script makes [ReadOnly] properties possible.
* There´s one more Editor script required: ReadOnlyDrawer.cs in Assets/Editor
* Instructions: https://disybell.com/229
* Created by Jan Fiess in May 2018
*******************************************************************************************************/


using UnityEngine;
 
public class ReadOnlyAttribute : PropertyAttribute
{
    public bool Condition { get; protected set; }
    public ReadOnlyAttribute()
    {
        Condition = true;
    }
    public ReadOnlyAttribute(bool condition)
    {
        Condition = condition;
    }
}