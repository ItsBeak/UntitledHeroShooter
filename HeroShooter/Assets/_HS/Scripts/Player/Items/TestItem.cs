using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestItem : ItemBase
{
    public override void UseItem()
    {
        Debug.Log("Used Test Item");
    }
}
