using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class StatusEffectUI : MonoBehaviour
{
    public Image statusIcon;
    public TextMeshProUGUI statusName;
    public Image statusDuration;

    public void SetStatusData(Sprite icon, string name)
    {
        if (icon)
        {
            statusIcon.sprite = icon;
        }

        statusName.text = name;
    }
}
