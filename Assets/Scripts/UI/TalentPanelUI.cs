using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentPanelUI : MonoBehaviour
{
    [SerializeField] Button talentBtn;
    [SerializeField] Button backBtn;
    [SerializeField] GameObject talentPanel;
    [SerializeField] int baseCost = 5;
    [SerializeField] float costMultipler = 0f; //Use this and numClicks to determine each upgrade cost
    [SerializeField] int deltaHealth = 5;
    [SerializeField] int deltaDamage = 1;
    [SerializeField] float deltaAtkSpeed = 0.01f;
    [SerializeField] int deltaCoinDrop = 1;
    [SerializeField] int deltaExpGain = 1;
    [SerializeField] int deltaMoveSpeed = 10;

    int numClicksHealth = 0;
    int numClicksDamage = 0;
    int numClicksAtkSpeed = 0;
    int numClicksCoinDrop = 0;
    int numClicksExpGain = 0;
    int numClicksMoveSpeed = 0;

    [SerializeField] Button upgradeHealthBtn;
    [SerializeField] Button upgradeDamageBtn;
    [SerializeField] Button upgradeAtkSpeedBtn;
    [SerializeField] Button upgradeCoinDropBtn;
    [SerializeField] Button upgradeExpGainBtn;
    [SerializeField] Button upgradeMoveSpeedBtn;

    void Start()
    {
        HidePanel();
        talentBtn.onClick.AddListener(ShowPanel);
        backBtn.onClick.AddListener(HidePanel);

        upgradeHealthBtn.GetComponentInChildren<TMP_Text>().text = baseCost.ToString();
        upgradeDamageBtn.GetComponentInChildren<TMP_Text>().text = baseCost.ToString();
        upgradeAtkSpeedBtn.GetComponentInChildren<TMP_Text>().text = baseCost.ToString();
        upgradeCoinDropBtn.GetComponentInChildren<TMP_Text>().text = baseCost.ToString();
        upgradeExpGainBtn.GetComponentInChildren<TMP_Text>().text = baseCost.ToString();
        upgradeMoveSpeedBtn.GetComponentInChildren<TMP_Text>().text = baseCost.ToString();
    }

    void ShowPanel()
    {
        talentPanel.SetActive(true);
    }

    void HidePanel()
    {
        talentPanel.SetActive(false);
    }
}
