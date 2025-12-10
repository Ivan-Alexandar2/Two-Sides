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

    // new
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TextMeshProUGUI endGameText;

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
                // Ако лодката е на десния бряг, пътниците се броят към десния бряг за условията на играта
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

    // EndGame Logic

    private void CheckGameStatus()
    {
        // Условие за победа: Всички са на левия бряг
        if (PeopleOnLeftBank == 3 && KillersOnLeftBank == 3)
        {
            EndGame("You win");
            return; // Излизаме, за да не провери и за загуба
        }

        // Условие за загуба: Повече убийци от хора на някой бряг (ако има хора)
        bool rightBankLoss = (PeopleOnRightBank < KillersOnRightBank) && (PeopleOnRightBank > 0);
        bool leftBankLoss = (PeopleOnLeftBank < KillersOnLeftBank) && (PeopleOnLeftBank > 0);

        if (rightBankLoss || leftBankLoss)
        {
            EndGame("You Lose!");
        }
    }

    private void EndGame(string message)
    {
        if (endGamePanel.activeSelf) return; // За да не се вика многократно

        endGamePanel.SetActive(true);
        endGameText.text = message;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
