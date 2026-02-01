using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace MasqueradeGame
{
    public class UIManager : MonoBehaviour
    {
        [Header("Game Info Display")]
        public TextMeshProUGUI roundText;
        public TextMeshProUGUI scoreText;

        [Header("Difficulty Selection")]
        public GameObject difficultyPanel;
        public Button easyButton;
        public Button mediumButton;
        public Button hardButton;

        [Header("Room Displays")]
        public RoomDisplay ballroomDisplay;
        public RoomDisplay bathroomDisplay;
        public RoomDisplay balconyDisplay;
        public RoomDisplay studyDisplay;
        public RoomDisplay wineCellarDisplay;
        public RoomDisplay courtyardDisplay;

        [Header("End Game")]
        public GameObject endGamePanel;
        public TextMeshProUGUI endGameText;
        public Button restartButton;

        [Header("Unused Roles Display")]
        public GameObject unusedRolesPanel;
        public Transform unusedRolesContainer;
        public GameObject roleItemPrefab;

        //here is the event panel stuff
        [Header("Event Selection")]
        public GameObject eventPanel;
        public Button continueButton;
        public TextMeshProUGUI eventText;

        private GameManager gameManager;
        private Dictionary<RoomType, RoomDisplay> roomDisplays;

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();

            easyButton.onClick.AddListener(() => StartGame(Difficulty.Easy));
            mediumButton.onClick.AddListener(() => StartGame(Difficulty.Medium));
            hardButton.onClick.AddListener(() => StartGame(Difficulty.Hard));

            restartButton.onClick.AddListener(ShowDifficultySelection);

            roomDisplays = new Dictionary<RoomType, RoomDisplay>
            {
                { RoomType.Ballroom, ballroomDisplay },
                { RoomType.Bathroom, bathroomDisplay },
                { RoomType.Balcony, balconyDisplay },
                { RoomType.Study, studyDisplay },
                { RoomType.WineCellar, wineCellarDisplay },
                { RoomType.Courtyard, courtyardDisplay }
            };

            foreach (var display in roomDisplays.Values)
            {
                display.Initialize(gameManager);
            }

            if (gameManager != null)
            {
                gameManager.OnRoundChanged += UpdateRoundDisplay;
                gameManager.OnGameEnded += ShowEndGame;
            }

            ShowDifficultySelection();
        }

        private void Update()
        {
            scoreText.text = gameManager.Score.ToString();
        }

        private void ShowDifficultySelection()
        {
            difficultyPanel.SetActive(true);
            endGamePanel.SetActive(false);
            if (unusedRolesPanel != null)
                unusedRolesPanel.SetActive(false);
        }

        private void StartGame(Difficulty difficulty)
        {
            difficultyPanel.SetActive(false);
            gameManager.StartNewGame(difficulty);

            UpdateGameInfoDisplay();
            UpdateAllRoomDisplays();
            DisplayUnusedRoles();
        }

        private void UpdateRoundDisplay(int round)
        {
            roundText.text = $"Round: {round}";
            UpdateAllRoomDisplays();
        }

        private void UpdateGameInfoDisplay()
        {
            roundText.text = $"Round: {gameManager.CurrentRound}/{gameManager}";
        }

        private void UpdateAllRoomDisplays()
        {
            foreach (var kvp in roomDisplays)
            {
                RoomType roomType = kvp.Key;
                RoomDisplay display = kvp.Value;

                List<Character> charactersInRoom = gameManager.GetCharactersInRoom(roomType);
                display.UpdateDisplay(charactersInRoom);
            }
        }

        private void DisplayUnusedRoles()
        {
            if (unusedRolesPanel == null || unusedRolesContainer == null)
                return;

            foreach (Transform child in unusedRolesContainer)
            {
                Destroy(child.gameObject);
            }

            List<RoleData> unusedRoles = gameManager.GetUnusedRoles();

            foreach (RoleData role in unusedRoles)
            {
                if (roleItemPrefab != null)
                {
                    GameObject item = Instantiate(roleItemPrefab, unusedRolesContainer);
                    TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = $"{role.roleName} ({role.influenceValue})";
                    }
                }
            }

            unusedRolesPanel.SetActive(true);
        }

        private void ShowEndGame(bool won)
        {
            endGamePanel.SetActive(true);
            endGameText.text = won ? "YOU WIN!" : "YOU LOSE!";
            endGameText.color = won ? Color.green : Color.red;
        }

        private void OnDestroy()
        {
            easyButton.onClick.RemoveAllListeners();
            mediumButton.onClick.RemoveAllListeners();
            hardButton.onClick.RemoveAllListeners();
            restartButton.onClick.RemoveAllListeners();

            if (gameManager != null)
            {
                gameManager.OnRoundChanged -= UpdateRoundDisplay;
                gameManager.OnGameEnded -= ShowEndGame;
            }
        }
    }

    [System.Serializable]
    public class RoomDisplay
    {
        public RoomType roomType;
        public TextMeshProUGUI roomNameText;
        public Transform characterIconContainer;
        public GameObject characterIconPrefab;
        public GameObject blindSpotOverlay; 

        private GameManager gameManager;

        public void Initialize(GameManager manager)
        {
            gameManager = manager;
        }

        private void OnCharacterIconClicked(Character character)
        {
            if (character.characterButton != null)
            {
                character.characterButton.onClick.Invoke();
            }
        }

        public void UpdateDisplay(List<Character> characters)
        {
            if (characterIconContainer != null)
            {
                foreach (Transform child in characterIconContainer)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            bool isBlindSpot = (roomType == RoomType.Bathroom || roomType == RoomType.WineCellar);

            if (blindSpotOverlay != null)
            {
                blindSpotOverlay.SetActive(isBlindSpot);
            }

            if (isBlindSpot)
            {
                return;
            }

            foreach (Character character in characters)
            {
                if (characterIconPrefab != null && characterIconContainer != null)
                {
                    GameObject icon = GameObject.Instantiate(characterIconPrefab, characterIconContainer);
                    Image iconImage = icon.GetComponent<Image>();
                    if (iconImage != null && character.maskIcon != null)
                    {
                        iconImage.sprite = character.maskIcon;
                    }

                    Button iconButton = icon.GetComponent<Button>();
                    if (iconButton != null)
                    {
                        Character charRef = character; 
                        iconButton.onClick.AddListener(() => OnCharacterIconClicked(charRef));
                    }
                }
            }
        }
    }
}