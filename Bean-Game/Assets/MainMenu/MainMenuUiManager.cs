using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class StartMenuUIManager : MonoBehaviour
{
    public static StartMenuUIManager Instance; // Singleton pattern

    [Header("UI Elements")]
    public GameObject startMenuUI; // Reference to Start Menu UI
    //public GameObject settingsMenuUI; // Reference to Settings Menu UI
    //public GameObject creditsMenuUI; // Reference to Credits Menu UI

    [Header("Buttons")]
    public Button playButton; // Button to play the game
    //public Button settingsButton; // Button to open the settings menu
    //public Button creditsButton; // Button to open the credits menu
    public Button quitButton; // Button to quit the game
    //public Button backButtonSettings;
    //public Button backButtonCredits;


    //[Header("Settings Panels")]
    //public GameObject videoPanel;
    //public GameObject audioPanel;
    //public GameObject controlsPanel;

    //[Header("Video Settings")]
    //public TMP_Dropdown resolutionDropdown;
    //public Button textureLowButton, textureMediumButton, textureHighButton;
    //public Button modelLowButton, modelMediumButton, modelHighButton;
    //public Button frame30Button, frame60Button, frameUncappedButton;

    //[Header("Audio Settings")]
    //public Slider masterVolumeSlider;

    //[Header("Controls Settings")]
    //public TMP_Text interactKeyText;
    //public TMP_Text dropKeyText;


    [Header("Sound Effects")]
    public AudioClip buttonClickSound;
    private AudioSource audioSource;

    [Header("Upgrade Data")]
    [SerializeField] private UpgradeData[] upgrades;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Set singleton instance
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // Set up button listeners
        if (playButton) playButton.onClick.AddListener(() => ButtonClicked(PlayGame));
        //if (settingsButton) settingsButton.onClick.AddListener(() => ButtonClicked(OpenSettings));
        //if (creditsButton) creditsButton.onClick.AddListener(() => ButtonClicked(OpenCredits));
        if (quitButton) quitButton.onClick.AddListener(() => ButtonClicked(QuitGame));
        //backButtonSettings.onClick.AddListener(() => ButtonClicked(CloseSettings));
        //backButtonCredits.onClick.AddListener(() => ButtonClicked(CloseCredits));


        audioSource = GetComponent<AudioSource>(); // Try getting an existing one

        // Initialize UI screens
        startMenuUI.SetActive(true);
        //settingsMenuUI.SetActive(false);
        //creditsMenuUI.SetActive(false);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = buttonClickSound;
        audioSource.volume = 1f;

        audioSource.spatialBlend = 0f; 
            
    }

    private void ButtonClicked(System.Action action)
    {
        if (buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        else
        {
            Debug.LogError("buttonClickSound is NULL! Assign an audio clip in the Inspector.");
        }

        action.Invoke(); // Call the original function
    }

    public void PlayGame()
    {
        // Reset upgrades when starting new game
        //foreach (UpgradeData upgrade in upgrades)
        //{
        //    upgrade.ResetUpgrade();
        //    Debug.Log(upgrade.upgradeEnabled);

        //}

        Debug.Log("Upgrades reset for new game!");

        UnityEngine.SceneManagement.SceneManager.LoadScene(1); // Replace with your actual game scene name
    }

    //public void CloseSettings()
    //{
    //    settingsMenuUI.SetActive(false);
    //    startMenuUI.SetActive(true);
    //    //AudioManager.instance.PlayOneShot(menuSoundA, this.transform.position); //Might be needed later
    //}

    //public void OpenSettings()
    //{
    //    startMenuUI.SetActive(false);
    //    settingsMenuUI.SetActive(true);
    //}



    //public void OpenCredits()
    //{
    //    startMenuUI.SetActive(false);
    //    creditsMenuUI.SetActive(true); // Display credits menu
    //}

    //public void CloseCredits()
    //{
    //    creditsMenuUI.SetActive(false);
    //    startMenuUI.SetActive(true);
    //}


    //public void ShowVideoSettings()
    //{
    //    videoPanel.SetActive(true);
    //    audioPanel.SetActive(false);
    //    controlsPanel.SetActive(false);
       
    //}

    //public void ShowAudioSettings()
    //{
    //    videoPanel.SetActive(false);
    //    audioPanel.SetActive(true);
    //    controlsPanel.SetActive(false);
       
    //}

    //public void ShowControlsSettings()
    //{
    //    videoPanel.SetActive(false);
    //    audioPanel.SetActive(false);
    //    controlsPanel.SetActive(true);
        
    //}


    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit(); // Quit the application
    }
}