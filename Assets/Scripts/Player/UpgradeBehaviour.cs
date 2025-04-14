using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBehaviour : MonoBehaviour
{
    public enum UpgradeType
    {
        Weapon,
        Utility,
        Passive
    }

    [System.Serializable]
    public class UpgradeStore
    {
        public List<Upgrade> weaponUpgrades;
        public List<Upgrade> utilityUpgrades;
        public List<Upgrade> passiveUpgrades;
    }

    public UpgradeStore upgradeStore;
    public List<Upgrade> availableUpgrades;

    public TextMeshProUGUI[] upgradeNameTexts; // Array to hold the Text components for upgrade names
    public Image[] upgradeIcons; // Array to hold the Image components for upgrade icons
    public GameObject upgradePanel; // Reference to the upgrade panel UI    
    public Button[] upgradeButtons; // Array to hold the Button components for upgrade buttons

    public PlayerShoot playerShoot; // Reference to the PlayerShoot script
    public PlayerHealth playerHealth; // Reference to the PlayerHealth script

    public static UpgradeBehaviour instance;

    private bool isWavePaused = false;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Initially hide the upgrade panel
        upgradePanel.SetActive(false);
    }

    Upgrade GetRandomUpgrade(List<Upgrade> upgrades)
    {
        if (upgrades == null || upgrades.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, upgrades.Count);
        return upgrades[randomIndex];
    }

    // UI
    void DisplayUpgrades()
    {
        upgradePanel.SetActive(true); // Show the upgrade panel
        for (int i = 0; i < availableUpgrades.Count; i++)
        {
            if (availableUpgrades[i] != null)
            {
                upgradeNameTexts[i].text = availableUpgrades[i].upgradeName;
                upgradeIcons[i].sprite = availableUpgrades[i].icon;
                int index = i; // Capture the current index
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => OnUpgradeButtonClicked(index));
            }
        }
    }

    public void ShowUpgradesAfterBossDefeat()
    {
        // Randomly pick one upgrade from each type
        availableUpgrades = new List<Upgrade>
        {
            GetRandomUpgrade(upgradeStore.weaponUpgrades),
            GetRandomUpgrade(upgradeStore.utilityUpgrades),
            GetRandomUpgrade(upgradeStore.passiveUpgrades)
        };

        // Pause the wave
        PauseWave();

        // Display the upgrades
        DisplayUpgrades();
    }

    public void OnUpgradeButtonClicked(int index)
    {
        if (index >= 0 && index < availableUpgrades.Count)
        {
            ApplyUpgrade(availableUpgrades[index]);
        }
    }

    public void ApplyUpgrade(Upgrade upgrade)
    {
        if (upgrade is IUpgradeEffect upgradeEffect)
        {
            upgradeEffect.ApplyEffect(playerShoot);
            upgradeEffect.ApplyEffect(playerHealth);
        }

        // Log the applied upgrade for verification
        Debug.Log($"Applied Upgrade: {upgrade.upgradeName}, Type: {upgrade.upgradeType}");

        // Resume the wave after applying the upgrade
        ResumeWave();

        // Hide the upgrade panel
        upgradePanel.SetActive(false);
    }

    void PauseWave()
    {
        isWavePaused = true;
        Time.timeScale = 0f; // Pause the game
    }

    void ResumeWave()
    {
        isWavePaused = false;
        Time.timeScale = 1f; // Resume the game
    }

    // Update is called once per frame
    void Update()
    {
        // You can add any additional logic here if needed
    }
}
