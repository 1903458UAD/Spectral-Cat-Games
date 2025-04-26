using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using FMODUnity;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.SocialPlatforms;

using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; //Instance of UIManager

    public GameObject mainMenuUI; // Reference to the Main Menu UI
    public Button playButton; // Reference to the Play Button
    public Button quitButton; // Reference to the quit Button
    public IntroScript introScript;

    private DayManager dayManager;

    [Header("UI Elements")] //Headers that show up in inspector
    public GameObject gameOverScreen; //Reference to game over ui
    public Image crosshair; //Refeerence to Crosshair UI
    public TMP_Text incomeText;//Reference to the income UI
    public GameObject pauseMenuUI; // Reference to Pause Menu UI
    public GameObject settingsMenuUI;
    public GameObject upgradeMenu;
    public GameObject CreditsMenuUI;

    public Toggle developerCheatsToggle;
    private DeveloperCheats developerCheats;
    public GameObject endOfDayScreen;

    public GameObject newDayPanel; // The GameObject for the "New Day" panel
    public CanvasGroup nextDayPanelGroup; // CanvasGroup attached to the "New Day" panel
    public TextMeshProUGUI dayText; // Text component for displaying the current day


    [Header("Customer Order UI")]
    public TMP_Text customerOrderText; // New UI element to display coffee order
    public GameObject reciept;
    public GameObject recieptOverlay;

    public Sprite overlay1;
    public Sprite overlay2;
    public Sprite overlay3;
    public Sprite overlay4;

    public TMP_Text coffeePrice; 
    public UpgradeData coffeePriceData;

    public TMP_Text orderNo;
    public int orderNoCount;

    [Header("Player Lives UI")]
    public GameObject warningSlip;
    public TMP_Text warningDescription;
    public TMP_Text livesLeft;

    public Image[] lifeIcons;

    [Header("Camera Script")]
    public MonoBehaviour cameraScript;

    [Header("Cridits buttons")]
    public Button Credits;
    public Button CreditsBackButton;


    [Header("Controls Settings")]
    public TMP_Text interactKeyText; 
    public TMP_Text dropKeyText;

    [Header("End of Day Screen")]
    public Button progressButton; 

    private bool currentlyRebinding = false;  // To track if we’re waiting for input
    private string stringOfBinding = ""; // Stores which action is being changed

    private Color defaultCrosshairColor = Color.white; //Default colouyr of crosshair
    private Color interactableCrosshairColor = Color.red;//Colour of the crosshair when looking at an interactable object

    private bool isPaused = false; // Pause state

    //[SerializeField] private Transform playerSpawnPoint; 
    [SerializeField] private GameObject player;

    [SerializeField] private EventReference menuSoundA;
    [SerializeField] private EventReference menuSoundB;
    [SerializeField] private EventReference startFX;
    [SerializeField] private EventReference exitFX;

    [Header("Customer Patience Bar")]
    public Slider slider;

    public Sprite happy;
    public Sprite bored;
    public Sprite angry;

    public GameObject customerMoodlet;
    public GameObject customerPatienceBar;

    public GameObject IncomeIcon;

    public UpgradeDataManager upgradeDataManager;

    private void Awake() // When instance is being loaded
    {
        if (Instance == null) //If no instance of the UIManager exists
        {
            Instance = this; //Set this instance as UIManager
        }
        else
        {
            Destroy(gameObject); //Destory duplicates
        }
    }

    private void Update()
    {
        if (mainMenuUI.activeSelf || pauseMenuUI.activeSelf || settingsMenuUI.activeSelf || CreditsMenuUI.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        orderNoCount = GameObject.Find("CustomerWindow").GetComponent<CustomerWindow>().customerOrderNo;
    }

    private void Start() 
    {
        HideGameOverScreen(); //Set the game over screen is hidden
        SetCrosshairDefault(); //Set the crosshair to default
        HideDayEndScreen();
        HidePatienceBar();
        HideReciept();
        HideGameplayUI();
        reciept.SetActive(false);

        warningSlip.SetActive(false);

        interactKeyText.text = PlayerPrefs.GetString("InteractKey", "Mouse0");
        dropKeyText.text = PlayerPrefs.GetString("DropKey", "Mouse1");

        developerCheats = FindObjectOfType<DeveloperCheats>();

        IncomeIcon.SetActive(false);

        // Show main menu at the start
        dayManager = DayManager.Instance;
        


        if (dayManager != null && dayManager.currentDay == 1)
        {
            // Show menu + intro
            mainMenuUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Skip menu + cutscene
            introScript.PlayIntro();
            mainMenuUI.SetActive(false);
            //SpawnPlayerAtStart();
            ShowGameplayUI();

            // Lock the cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Find the IntroScript in the scene
        introScript = FindObjectOfType<IntroScript>();

        if (introScript == null)
        {
            Debug.LogError("IntroScript not found in the scene!");
        }


        // Assign the Play button function
        playButton.onClick.AddListener(StartGame);
        if (developerCheats == null)
        {
            Debug.LogError("DeveloperCheats script not found in the scene!");
            return;
        }


        // Initialize toggle state based on current cheatsEnabled value
        developerCheatsToggle.isOn = developerCheats.cheatsEnabled;

        // Add listener to toggle button
        developerCheatsToggle.onValueChanged.AddListener(ToggleDeveloperCheats);
        

        if (customerOrderText == null)
        {
            Debug.LogError("[UIManager] Customer Order Text is not assigned in the Inspector!");
        }

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false); 
        } 
    }

    public void StartGame()
    {
        AudioManager.instance.PlayOneShot(startFX, this.transform.position);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //IncomeIcon.SetActive(true);
        upgradeDataManager.ResetUpgrades();
        HideGameplayUI();

  

        if (introScript != null)
        {
            mainMenuUI.SetActive(false); // Hide the main menu
            introScript.PlayIntro(); // Start the cutscene
        }
        else
        {
            Debug.LogError("Introscript is null");
        }
    }


    public void ToggleDeveloperCheats(bool isEnabled)
    {
        developerCheats.cheatsEnabled = isEnabled;
        Debug.Log($"[UIManager] Developer Cheats state updated: {developerCheats.cheatsEnabled}");
    }

    public void Rebinding(string action)
    {
        if (currentlyRebinding) return; // Prevent multiple rebinding actions

        currentlyRebinding = true;
        stringOfBinding = action;
        AudioManager.instance.PlayOneShot(menuSoundB, this.transform.position);
        if (action == "Interact")
            interactKeyText.text = "Press any key...";
        else if (action == "Drop")
            dropKeyText.text = "Press any key...";

        StartCoroutine(WaitForKeyPress());
    }

    private IEnumerator WaitForKeyPress()
    {
        while (!Input.anyKeyDown)  // Wait until a key is pressed
            yield return null;

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                AssignNewKey(key);
                AudioManager.instance.PlayOneShot(menuSoundB, this.transform.position);
                break;
            }
        }
    }

    private void AssignNewKey(KeyCode newKey)
    {
        if (stringOfBinding == "Interact")
        {
            PlayerPrefs.SetString("InteractKey", newKey.ToString()); // Save new key
            interactKeyText.text = newKey.ToString(); // Update UI
        }
        else if (stringOfBinding == "Drop")
        {
            PlayerPrefs.SetString("DropKey", newKey.ToString()); // Save new key
            dropKeyText.text = newKey.ToString(); // Update UI
        }

        currentlyRebinding = false;
        stringOfBinding = "";

        FindObjectOfType<PlayerInteraction>().UpdateKeybindings();
    }

    public void ShowGameOverScreen() // Show death screen when player dies
    {
        gameOverScreen.SetActive(true); //Activate the game over UI (Which shows it to player)
        HideGameplayUI();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void HideGameOverScreen() //Hide Death screen, To be called when reset
    {
        gameOverScreen.SetActive(false); //De-Activate the game over UI (Which hides it from the player)
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowDayEndScreen() //Hide Death screen, To be called when reset
    {
        endOfDayScreen.SetActive(true); //De-Activate the game over UI (Which hides it from the player)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cameraScript.enabled = false;
        Time.timeScale = 0f;

    }

    public void HideDayEndScreen() //Hide Death screen, To be called when reset
    {
        endOfDayScreen.SetActive(false); //De-Activate the game over UI (Which hides it from the player)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraScript.enabled = true;
        Time.timeScale = 1f;
    }

    public void NextDayButton()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        DayManager.Instance.NextDay(); 
        SceneManager.LoadScene(sceneName);
       
        GameManager.Instance.SetIncome(StaticData.incomePassed);
    }

    public void RestartButton()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        DayManager.Instance.ResetDay();
        SceneManager.LoadScene(sceneName);
    }


    public void OnNewGame()
    {
        DayManager.Instance.ResetDay();
        SceneManager.LoadScene("GameScene"); // Starts Day 1 again
    }

    public void SetCrosshairInteractable() //Set the colour of crosshair when not targeting an interactable object
    {
        crosshair.color = interactableCrosshairColor; //change to the crosshair colour (Initally or currently: Red)
    }

    public void SetCrosshairDefault() //Set the colour of the crosshair when not targeting an interactable object
    {
        crosshair.color = defaultCrosshairColor; //change to the crosshair colour (Initally or currently: Black)
    }

    public void UpdateIncomeDisplay(float income)
    {
        incomeText.text = string.Format("{0}", income);

        Debug.Log(StaticData.incomePassed);
    }

    public void UpdateCustomerOrder(int requiredBeans, string syrup)
    {
        if (requiredBeans == 1) //Correct wording when using 1 "bean" instead of multiple "beans"
        {
            if(syrup == "None")
            customerOrderText.text = $"- {requiredBeans} Shot Espresso";

            else
            customerOrderText.text = $"- {requiredBeans} Shot Espresso \n\n - Add {syrup}";
        }
        else
        {
            if (syrup == "None")
            customerOrderText.text = $"- {requiredBeans} Shots Espresso";

            else
            customerOrderText.text = $"- {requiredBeans} Shots Espresso \n\n - Add {syrup}";
        }

        //if (requiredBeans == 1) //Correct wording when using 1 "bean" instead of multiple "beans"
        //{
        //    customerOrderText.text = $"Order Coffee Strength: {requiredBeans} Bean with Syrup: {syrup}";
        //}
        //else
        //{
        //    customerOrderText.text = $"Order Coffee Strength: {requiredBeans} Beans with Syrup: {syrup}";
        //}
    }

    public void TogglePause()
    {

        if (mainMenuUI.activeSelf)
        {
           
            return;
        }

        if (upgradeMenu.activeSelf)
        {

            return;
        }

        if (gameOverScreen.activeSelf)
        {

            return;
        }
        if (newDayPanel.activeSelf)
        {

            return;
        }
        if (endOfDayScreen.activeSelf)
        {

            return;
        }

        // If the settings menu is open, go back to the pause menu instead of resuming
        if (settingsMenuUI.activeSelf)
        {
            ShowPauseMenu();
            return; // Prevent the game from unpausing
        }

        if (CreditsMenuUI.activeSelf)
        {
            ShowPauseMenu();
            return; // Prevent the game from unpausing
        }

        isPaused = !isPaused;

        if(!upgradeMenu.activeSelf)
        {
        if (isPaused)
        {
            Time.timeScale = 0f;
            pauseMenuUI.SetActive(true);

            HideGameplayUI();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Disable camera movement
            if (cameraScript != null)
            {
                cameraScript.enabled = false;
            }


            // Ensure UI buttons work properly
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenuUI.SetActive(false);

            ShowGameplayUI();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        
            if (cameraScript != null)
            {
                cameraScript.enabled = true;
            }
        }

        }
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }

    public void ResumeGame()
    {
        TogglePause(); // Unpause the game
        AudioManager.instance.PlayOneShot(menuSoundA, this.transform.position);
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
        AudioManager.instance.PlayOneShot(menuSoundA, this.transform.position);
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
        AudioManager.instance.PlayOneShot(menuSoundA, this.transform.position);
    }

    public void OpenCredits()
    {
        pauseMenuUI.SetActive(false);
        CreditsMenuUI.SetActive(true);
        AudioManager.instance.PlayOneShot(menuSoundA, this.transform.position);
    }

    public void CloseCredits()
    {
        CreditsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
        AudioManager.instance.PlayOneShot(menuSoundA, this.transform.position);
    }

    public void Settings()
    {
        settingsMenuUI.SetActive(true); //Hide the Pause menu UI
        pauseMenuUI.SetActive(false); //Hide the Pause menu UI

        AudioManager.instance.PlayOneShot(menuSoundA, this.transform.position);
    }

    public void UpdateLifeUI(int currentLives, string waringDesc)
    {
        warningSlip.SetActive(true);
        warningDescription.text = $"Description of Infraction: {waringDesc}";
        livesLeft.text = $"{currentLives} more chances!";
        Invoke(nameof(HideLifeUI), 3f);

        //for (int i = 0; i < lifeIcons.Length; i++)
        //{
        //    if (i < currentLives)
        //    {
        //        lifeIcons[i].color = Color.white; // Represents remaining lives
        //    }
        //    else
        //    {
        //        lifeIcons[i].color = Color.red; // Represents lost lives
        //    }
        //}
    }

    public void HideLifeUI()
    {
        warningSlip.SetActive(false);
    }

    public void ShowPauseMenu()
    {
        settingsMenuUI.SetActive(false); // Hide the settings menu
        CreditsMenuUI.SetActive(false); // Hide the credits menu
        pauseMenuUI.SetActive(true); // Show the pause menu
        AudioManager.instance.PlayOneShot(menuSoundA, this.transform.position);
    }

    public void MainMenu()
    {
        //Time.timeScale = 1f; // Time is resumed when switching scenes
        //pauseMenuUI.SetActive(false); //Hide the Pause menu UI

        //UnityEngine.SceneManagement.SceneManager.LoadScene(0); //Need to change 0 to whatever the mainmenu scene will be
    }

    public void ShowGameplayUI()
    {
        if (crosshair != null) crosshair.gameObject.SetActive(true);
        if (incomeText != null) incomeText.gameObject.SetActive(true);
        if (customerOrderText != null) customerOrderText.gameObject.SetActive(true);
        IncomeIcon.SetActive(true);
        ShowReciept();
        ShowPatienceBar();

        foreach (Image life in lifeIcons)
        {
            life.gameObject.SetActive(true);
        }
    }

    public void HideGameplayUI()
    {
        if (crosshair != null) crosshair.gameObject.SetActive(false);
        if (incomeText != null) incomeText.gameObject.SetActive(false);
        if (customerOrderText != null) customerOrderText.gameObject.SetActive(false);
        IncomeIcon.SetActive(false);
        HideReciept();
        HidePatienceBar();

        foreach (Image life in lifeIcons)
        {
            life.gameObject.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game button clicked!");
        AudioManager.instance.PlayOneShot(exitFX, this.transform.position);
        Application.Quit(); // Quits the game to desktop 
    }

    public void GetStartTime(float startTime)
    {
        slider.maxValue = startTime;
        slider.value = startTime;

        customerMoodlet.GetComponent<Image>().sprite = happy;
    }

    public void GetCurrentTime(float currentTime)
    {
        slider.value = currentTime;
    }

    public void setMoodlet(string mood)
    {
        switch (mood)
        {
            case "happy":
                customerMoodlet.GetComponent<Image>().sprite = happy;
                customerMoodlet.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, 5f), 1.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                break;

            case "bored":
                customerMoodlet.GetComponent<Image>().sprite = bored;
                customerMoodlet.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, 10f), 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                break;

            case "angry":
                customerMoodlet.GetComponent<Image>().sprite = angry;
                customerMoodlet.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, 20f), 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                break;

            default:
                customerMoodlet.GetComponent<Image>().sprite = happy;
                customerMoodlet.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, 5f), 1.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                break;
        }
    }

    public void ShowPatienceBar()
    {
        customerPatienceBar.SetActive(true);
    }

    public void HidePatienceBar()
    {
        if (customerOrderText != null) customerOrderText.gameObject.SetActive(false);
        customerPatienceBar.SetActive(false);
    }

    public void ShowReciept()
    {
        int randomNumber = Random.Range(0, 3);

        switch (randomNumber)
        {
            case 0:
                recieptOverlay.GetComponent<Image>().sprite = overlay1;
                break;

            case 1:
                recieptOverlay.GetComponent<Image>().sprite = overlay2;
                break;

            case 2:
                recieptOverlay.GetComponent<Image>().sprite = overlay3;
                break;

            case 3:
                recieptOverlay.GetComponent<Image>().sprite = overlay4;
                break;

            default:
                recieptOverlay.GetComponent<Image>().sprite = null;
                break;
        }

        reciept.SetActive(true);
        if (customerOrderText != null) customerOrderText.gameObject.SetActive(true);
        coffeePrice.text = string.Format("{0}", coffeePriceData.internalBaseValue);
        orderNo.text = string.Format($"Order #{orderNoCount}");
    }

    public void HideReciept()
    {
        reciept.SetActive(false);
    }

    public void ShowNextDayPanel(int currentDay)
    {
        if (newDayPanel == null || nextDayPanelGroup == null || dayText == null)
        {
            Debug.LogError("UIManager: New Day Panel references are missing.");
            return;
        }

        if (currentDay != 1)
        {
            newDayPanel.SetActive(true);
            dayText.text = "Day " + currentDay;
            StartCoroutine(FadeSequence());
        }

    }

    private IEnumerator FadeSequence()
    {
        yield return StartCoroutine(FadeCanvasGroup(nextDayPanelGroup, 0f, 1f, 0.01f)); // Fade in
        yield return new WaitForSeconds(8.7f); // Pause
        yield return StartCoroutine(FadeCanvasGroup(nextDayPanelGroup, 1f, 0f, 1f)); // Fade out
        newDayPanel.SetActive(false); // Hide the panel after fade
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null || group.gameObject == null)
        {
            Debug.LogError("CanvasGroup or its GameObject is null! Cannot fade.");
            yield break;
        }

        float time = 0f;
        while (time < duration)
        {
            group.alpha = Mathf.Lerp(from, to, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        group.alpha = to;
    }
}
