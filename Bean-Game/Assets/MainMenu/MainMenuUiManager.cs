using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class StartMenuUIManager : MonoBehaviour
{
    public static StartMenuUIManager Instance; // Singleton pattern

    [Header("UI Elements")]
    public GameObject startMenuUI; // Reference to Start Menu UI
    public GameObject settingsMenuUI; // Reference to Settings Menu UI
    public GameObject creditsMenuUI; // Reference to Credits Menu UI

    [Header("Buttons")]
    public Button playButton; // Button to play the game
    public Button settingsButton; // Button to open the settings menu
    public Button creditsButton; // Button to open the credits menu
    public Button quitButton; // Button to quit the game


    [Header("Settings Panels")]
    public GameObject videoPanel;
    public GameObject audioPanel;
    public GameObject controlsPanel;

    [Header("Video Settings")]
    public TMP_Dropdown resolutionDropdown;
    public Button textureLowButton, textureMediumButton, textureHighButton;
    public Button modelLowButton, modelMediumButton, modelHighButton;
    public Button frame30Button, frame60Button, frameUncappedButton;

    [Header("Audio Settings")]
    public Slider masterVolumeSlider;

    [Header("Controls Settings")]
    public TMP_Text interactKeyText;
    public TMP_Text dropKeyText;

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
    }

    private void Start()
    {
        // Set up button listeners
        playButton.onClick.AddListener(PlayGame);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(QuitGame);

        // Initialize UI screens
        startMenuUI.SetActive(true);
        settingsMenuUI.SetActive(false);
        creditsMenuUI.SetActive(false);
    }

    public void PlayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1); // Replace with your actual game scene name
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        startMenuUI.SetActive(true);
        //AudioManager.instance.PlayOneShot(menuSoundA, this.transform.position); //Might be needed later
    }

    public void OpenSettings()
    {
        startMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
    }



    public void OpenCredits()
    {
        startMenuUI.SetActive(false);
        creditsMenuUI.SetActive(true); // Display credits menu
    }

    public void CloseCredits()
    {
        creditsMenuUI.SetActive(false);
        startMenuUI.SetActive(true);
    }


    public void ShowVideoSettings()
    {
        videoPanel.SetActive(true);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(false);
       
    }

    public void ShowAudioSettings()
    {
        videoPanel.SetActive(false);
        audioPanel.SetActive(true);
        controlsPanel.SetActive(false);
       
    }

    public void ShowControlsSettings()
    {
        videoPanel.SetActive(false);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(true);
        
    }


    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit(); // Quit the application
    }
}