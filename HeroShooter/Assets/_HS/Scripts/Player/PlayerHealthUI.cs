using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup healthCanvas;
    public TextMeshProUGUI healthText;
    public Image healthFill;
    public CanvasGroup hitEffect;
    public Transform statusEffectUIContainer;

    private void Update()
    {
        hitEffect.alpha -= 2 * Time.deltaTime;
    }

    public void PlayDamageEffect()
    {
        hitEffect.alpha = 1;

        // play hit sound effect
    }
}
