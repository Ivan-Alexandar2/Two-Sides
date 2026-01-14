using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("NPC Counter")]
    public int PeopleOnRightBank; // the start bank
    public int KillersOnRightBank;

    public int PeopleInBoat;
    public int KillersInBoat;
    public int TotalBoatPopulation;

    public int PeopleOnLeftBank;
    public int KillersOnLeftBank;

    [Header("Beacons")]
    public TeleportBeacon[] RightBankBeacons;
    public TeleportBeacon[] BoatBeacons;
    public TeleportBeacon[] LeftBankBeacons;

    public BoatController boat;

    [Header("End Screens")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private TextMeshProUGUI loseText;
    [SerializeField] private TextMeshProUGUI winText;

    [Header("Timer")]
    public TextMeshProUGUI timerText;
    private float elapsedTime;
    private bool isGameActive = false;

    // new
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TextMeshProUGUI endGameText;

    [SerializeField] private GameButtons gameButtons;

    void Start()
    {
        PeopleOnRightBank = 3;
        KillersOnRightBank = 3;

        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    void Update()
    {
        CalculatePopulation();
        CheckGameStatus();

        if (isGameActive)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Z))
        {
            // Flip the active state of the menu (if on -> off, if off -> on)
            bool isMenuOpen = !gameButtons.pauseMenu.activeSelf;
            gameButtons.pauseMenu.SetActive(isMenuOpen);

            Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isMenuOpen;

            Time.timeScale = isMenuOpen ? 0f : 1f;
        }
    }

    private void CalculatePopulation()
    {
        ResetCounters();

        CountBank(RightBankBeacons, ref PeopleOnRightBank, ref KillersOnRightBank);
        CountBank(LeftBankBeacons, ref PeopleOnLeftBank, ref KillersOnLeftBank);

        //counts people on boat
        CountBoat();
    }

    private void ResetCounters()
    {
        PeopleOnRightBank = 0;
        KillersOnRightBank = 0;
        PeopleInBoat = 0;
        KillersInBoat = 0;
        PeopleOnLeftBank = 0;
        KillersOnLeftBank = 0;
    }

    private void CountBank(TeleportBeacon[] beacons, ref int peopleCount, ref int killerCount)
    {
        foreach (var beacon in beacons)
        {
            if (beacon.inhabitant == null) continue;

            if (beacon.inhabitant.GetComponent<Killer>())
                killerCount++;
            else if (beacon.inhabitant.GetComponent<Person>())
                peopleCount++;
        }
    }

    private void CountBoat()
    {
        foreach (var beacon in BoatBeacons)
        {
            if (beacon.inhabitant == null) continue;

            bool isKiller = beacon.inhabitant.GetComponent<Killer>() != null;
            bool isPerson = beacon.inhabitant.GetComponent<Person>() != null;

            if (isKiller)
            {
                KillersInBoat++;
                // If the boat is on the right or left bank, boat passengers are counted towards that bank (game rules)
                if (boat.isOnRightBank) KillersOnRightBank++;
                else KillersOnLeftBank++;
            }
            else if (isPerson)
            {
                PeopleInBoat++;
                if (boat.isOnRightBank) PeopleOnRightBank++;
                else PeopleOnLeftBank++;
            }
        }
    }

    void UpdateTimerUI()
    {
        // Math to turn seconds into "00:00" format
        int minutes = Mathf.FloorToInt(elapsedTime / 60F);
        int seconds = Mathf.FloorToInt(elapsedTime % 60F);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Call this function from your PLAY BUTTON
    public void StartGame()
    {
        elapsedTime = 0;
        isGameActive = true;
        timerText.color = Color.white; // Reset color just in case
    }

    // EndGame Logic

    private void CheckGameStatus()
    {
        // Win condition
        if (PeopleOnLeftBank == 3 && KillersOnLeftBank == 3)
        {
            EndGame("You Win!");
            return;
        }

        // Lose conditions
        bool rightBankLoss = (PeopleOnRightBank < KillersOnRightBank) && (PeopleOnRightBank > 0);
        bool leftBankLoss = (PeopleOnLeftBank < KillersOnLeftBank) && (PeopleOnLeftBank > 0);

        if (rightBankLoss || leftBankLoss)
        {
            EndGame("You Lose!");
        }
    }

    private void EndGame(string message)
    {
        if (endGamePanel.activeSelf) return;

        if (!isGameActive) return; // Prevent double winning

        isGameActive = false;
        

        endGamePanel.SetActive(true);
        endGameText.text = message;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if(message == "You Win!") timerText.color = Color.cyan;
        if(message == "You Lose!") timerText.color = Color.red;
    }
}
