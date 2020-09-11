using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Entities;

public class ResourcesHUD : MonoBehaviour
{
    [Range(0,7)]
    public int team;

    public TMP_Text foodText;
    public TMP_Text woodText;
    public TMP_Text goldText;
    public TMP_Text stoneText;

    private void Update()
    {
        var resourceBankOfTeam = ResourceSystem.GetResourceBankOfTeam(team);
        if (resourceBankOfTeam == null)
        {
            return;
        }

        if (foodText != null)
        {
            foodText.text = resourceBankOfTeam.foodCount.ToString();
        }
        if (woodText != null)
        {
            woodText.text = resourceBankOfTeam.woodCount.ToString();
        }
        if (goldText != null)
        {
            goldText.text = resourceBankOfTeam.goldCount.ToString();
        }
        if (stoneText != null)
        {
            stoneText.text = resourceBankOfTeam.stoneCount.ToString();
        }
    }
    //public 
}
