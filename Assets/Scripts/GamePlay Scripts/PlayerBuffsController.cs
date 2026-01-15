using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBuffsController : MonoBehaviour
{
    public static PlayerBuffsController Instance;
    PlayerCharacterController dwarfController;
    List<PlayerBuff> playerActiveBuffs;
    public GameObject buffFramePrefab;
    public Transform buffsPanel;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        dwarfController = FindFirstObjectByType<PlayerCharacterController>();        
    }

    public void UpdatePlayerBuffs()
    {
        foreach (Transform buff in buffsPanel)
        {
            Destroy(buff.gameObject);
        }
        playerActiveBuffs = dwarfController.GetActiveBuffs();
        foreach (PlayerBuff buff in playerActiveBuffs)
        {
            GameObject newBuffIcon = Instantiate(buffFramePrefab, buffsPanel);
            Image icon = newBuffIcon.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI durationText = newBuffIcon.GetComponentInChildren<TextMeshProUGUI>();
            icon.sprite = buff.BuffIcon;
            durationText.text = buff.roundDuration.ToString();
        }

    }
}
