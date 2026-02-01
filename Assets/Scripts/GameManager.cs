using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using MasqueradeGame.UI;
using MasqueradeGame.VFX;
using TMPro;

namespace MasqueradeGame
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        public Difficulty currentDifficulty;
        public int currentRound = 0;

        [Header("Role Configuration")]
        public List<RoleData> allRoles; 
        public int numberOfCharactersInPlay = 7; 

        [Header("Mask Configuration")]
        public List<MaskVisuals> allMaskVisuals;

        [Header("Room References")]
        public Room ballroom;
        public Room bathroom;
        public Room balcony;
        public Room study;
        public Room wineCellar;
        public Room courtyard;

        [Header("Character Prefab")]
        public GameObject characterPrefab;
        public RectTransform charactersRoot; 

        [Header("Player Resources")]
        public int playerInfluence;
        public Role targetRole;

        [Header("Clock")]
        public RectTransform clockHand;
        public float degreesPerRound = 30f; 

        [Header("Event Panel")]
        public GameObject eventPanel;
        public TMPro.TextMeshProUGUI eventText;
        public UnityEngine.UI.Button eventContinueButton;

        private List<Character> charactersInPlay = new List<Character>();
        private List<RoleData> rolesInPlay = new List<RoleData>();
        private List<RoleData> rolesNotInPlay = new List<RoleData>();
        private Dictionary<RoomType, Room> roomDictionary = new Dictionary<RoomType, Room>();

        private bool isGameActive = false;
        private Character selectedCharacter;
        public ScreenFaderVFX ScreenFader;

        public event Action<int> OnRoundChanged;
        public event Action<Character> OnCharacterSelected;
        public event Action<Character> OnCharacterSelectedForGuessing;
        public event Action<bool> OnGameEnded;

        public CharactersPanelUI CharactersPanel;


        public readonly HashSet<Role> AllRoles = new()
        {
            Role.Spy, Role.Guard, Role.Tailor, Role.Baron, Role.Countess, Role.Prince, Role.Duke, 
            Role.Princess, Role.Pope, Role.King
        };

        public readonly HashSet<MaskType> AllMasks = new()
        {
            MaskType.Peacock,
            MaskType.Rabbit,
            MaskType.Porcupine,
            MaskType.Weasel,
            MaskType.Mouse,
            MaskType.Deer,
            MaskType.Fox,
            MaskType.Wolf,
        };

        private void Awake()
        {
            InitializeRoomDictionary();
            SetupRoomConnections();
            
            if (eventPanel != null)
            {
                eventPanel.SetActive(false);
            }
            
            if (eventContinueButton != null)
            {
                Debug.Log("Adding listener to event continue button");
                eventContinueButton.onClick.RemoveAllListeners();
                eventContinueButton.onClick.AddListener(CloseEventPanel);
            }
        }

        public CurtainTransition CurtainVfx;
        public void StartNewGame(Difficulty difficulty)
        {
            CurtainVfx.DoFade(() =>
            {
                currentDifficulty = difficulty;
                currentRound = 0;
                isGameActive = true;

                SetDifficultyParameters();
                SetupRoles();
                SetupCharacters();

                Debug.Log($"Game started! Difficulty: {difficulty}, Target: {targetRole}, Influence: {playerInfluence}");
            });
            AudioManager.Instance.audioPlayer.PlayOneShot(AudioManager.Instance.startGame);
        }

        private void SetDifficultyParameters()
        {
            switch (currentDifficulty)
            {
                case Difficulty.Easy:
                    playerInfluence = 5;
                    numberOfCharactersInPlay = 4;
                    break;
                case Difficulty.Medium:
                    playerInfluence = 3;
                    numberOfCharactersInPlay = 6;
                    break;
                case Difficulty.Hard:
                    playerInfluence = 1;
                    numberOfCharactersInPlay = 7;
                    break;
            }
        }

        private void SetupRoles()
        {
            rolesInPlay.Clear();
            rolesNotInPlay.Clear();

            List<RoleData> shuffledRoles = allRoles.OrderBy(x => UnityEngine.Random.value).ToList();

            if (currentDifficulty == Difficulty.Easy)
            {
                shuffledRoles = shuffledRoles.Where(r => r.roleType != Role.Princess).ToList();
            }

            for (int i = 0; i < numberOfCharactersInPlay; i++)
            {
                rolesInPlay.Add(shuffledRoles[i]);
            }

            for (int i = numberOfCharactersInPlay; i < shuffledRoles.Count; i++)
            {
                rolesNotInPlay.Add(shuffledRoles[i]);
            }
            
            List<RoleData> eligibleTargets = rolesInPlay.Where(r => r.influenceValue >= 5).ToList();
            
            if (eligibleTargets.Count > 0)
            {
                targetRole = eligibleTargets[UnityEngine.Random.Range(0, eligibleTargets.Count)].roleType;
            }
            else
            {
                Debug.LogWarning("No roles with influence >= 5 found, picking random target");
                targetRole = rolesInPlay[UnityEngine.Random.Range(0, rolesInPlay.Count)].roleType;
            }
        }

        private void SetupCharacters()
        {
            charactersInPlay.Clear();

            List<MaskVisuals> shuffledMasks = allMaskVisuals.OrderBy(x => UnityEngine.Random.value).ToList();

            bool tailorInPlay = rolesInPlay.Any(r => r.roleType == Role.Tailor);
            MaskVisuals rabbitMask = shuffledMasks.FirstOrDefault(m => m.maskType == MaskType.Rabbit);
            
            if (tailorInPlay && rabbitMask != null)
            {
                shuffledMasks.Remove(rabbitMask);
            }

            List<Room> allRooms = new List<Room> { ballroom, bathroom, balcony, study, wineCellar, courtyard };

            int maskIndex = 0;
            
            for (int i = 0; i < rolesInPlay.Count; i++)
            {
                Transform parent = charactersRoot != null ? charactersRoot.transform : transform;
                GameObject charObj = Instantiate(characterPrefab, parent);
                Character character = charObj.GetComponent<Character>();

                character.characterID = i;
                character.characterName = $"Guest {i + 1}";

                Room startingRoom = allRooms[UnityEngine.Random.Range(0, allRooms.Count)];

                MaskVisuals assignedMask;
                
                if (rolesInPlay[i].roleType == Role.Tailor)
                {
                    assignedMask = rabbitMask;
                }
                else
                {
                    assignedMask = shuffledMasks[maskIndex % shuffledMasks.Count];
                    maskIndex++;
                }

                character.Initialize(rolesInPlay[i], assignedMask, startingRoom);
                character.OnCharacterClicked += OnCharacterClickedHandler;

                charactersInPlay.Add(character);
            }
        }

        private void Update()
        {
            ClockSupportText.text = $"{3 - (currentRound % 3)} hours left before you MUST guess someone's identity!";
        }

        public void AdvanceTurn()
        {
            CurtainVfx.DoFade(() =>
            {
                currentRound++;
                OnRoundChanged?.Invoke(currentRound);
                
                // Update clock hand rotation
                UpdateClockHand();

                MoveAllCharacters();

                if (currentRound >= 15)
                {
                    Win();
                    return;
                }
            
                if (currentRound % 3 == 0)
                {
                    ForceGuess();
                    //EventPhase();
                }

                //// hii olin
            });
        }

        public TextMeshProUGUI ClockSupportText;
        private void UpdateClockHand()
        {
            if (clockHand != null)
            {
                float targetRotation = -currentRound * degreesPerRound; // Negative to rotate clockwise
                clockHand.rotation = Quaternion.Euler(0, 0, targetRotation);
            }

        }

        private void ForceGuess()
        {
            var maskedCharacters = AllMaskedCharacters;
            CharactersPanel.Open(maskedCharacters, c => OnCharacterSelectedForGuessing?.Invoke(c));
        }

        public void EventPhase()
        {
            Character char1 = null;
            Character char2 = null;
            bool foundValidPair = false;
            int attempts = 0;
            int maxAttempts = 50;

            // After round 10, we can swap masks between any two characters
            if (currentRound > 10)
            {
                List<Character> eligibleCharacters = charactersInPlay.Where(c => !c.demasked).ToList();
                
                if (eligibleCharacters.Count < 2)
                {
                    Debug.Log("Not enough eligible characters for event phase");
                    return;
                }

                while (!foundValidPair && attempts < maxAttempts)
                {
                    char1 = eligibleCharacters[UnityEngine.Random.Range(0, eligibleCharacters.Count)];
                    char2 = eligibleCharacters[UnityEngine.Random.Range(0, eligibleCharacters.Count)];
                    
                    if (char1 != char2 && !char1.demasked && !char2.demasked)
                    {
                        foundValidPair = true;
                    }
                    attempts++;
                }

                if (foundValidPair)
                {
                    char1.SwapMasks(char2);

                    if (eventPanel != null && eventText != null)
                    {
                        string roleName = UnityEngine.Random.value > 0.5f ? char1.trueRole.roleName : char2.trueRole.roleName;
                        
                        eventText.text = $"Chaos at the masquerade!\n\nThe {roleName} has swapped masks with someone!";
                        
                        eventPanel.SetActive(true);
                    }

                    Debug.Log($"Event (Late Game): {char1.characterName} ({char1.trueRole.roleName}) swapped masks with {char2.characterName} ({char2.trueRole.roleName})");
                }
            }
            else
            {
                // Before round 10, must be in the same room
                List<Room> allRooms = new List<Room> { ballroom, bathroom, balcony, study, wineCellar, courtyard };
                List<Room> roomsWithMultipleCharacters = allRooms.Where(r => r.OccupantCount >= 2).ToList();

                if (roomsWithMultipleCharacters.Count == 0)
                {
                    Debug.Log("No rooms with multiple characters for event phase");
                    return;
                }

                while (!foundValidPair && attempts < maxAttempts)
                {
                    Room selectedRoom = roomsWithMultipleCharacters[UnityEngine.Random.Range(0, roomsWithMultipleCharacters.Count)];
                    List<Character> occupants = selectedRoom.Occupants.Where(c => !c.demasked).ToList();

                    if (occupants.Count < 2)
                    {
                        attempts++;
                        continue;
                    }

                    char1 = occupants[UnityEngine.Random.Range(0, occupants.Count)];
                    
                    do
                    {
                        char2 = occupants[UnityEngine.Random.Range(0, occupants.Count)];
                    } while (char2 == char1 && occupants.Count > 1);

                    if (char1 != char2 && !char1.demasked && !char2.demasked)
                    {
                        foundValidPair = true;
                    }
                    attempts++;
                }

                if (foundValidPair)
                {
                    char1.SwapMasks(char2);

                    if (eventPanel != null && eventText != null)
                    {
                        string roleName = UnityEngine.Random.value > 0.5f ? char1.trueRole.roleName : char2.trueRole.roleName;
                        
                        eventText.text = $"A commotion in the {char1.currentRoom.displayName}!\n\nThe {roleName} has swapped masks with someone in their room!";
                        
                        eventPanel.SetActive(true);
                    }

                    Debug.Log($"Event: {char1.characterName} ({char1.trueRole.roleName}) swapped masks with {char2.characterName} ({char2.trueRole.roleName}) in {char1.currentRoom.displayName}");
                }
            }

            if (!foundValidPair)
            {
                Debug.Log("Could not find valid pair for event phase after max attempts");
            }
        }

        private void CloseEventPanel()
        {
            if (eventPanel != null)
            {
                eventPanel.SetActive(false);
            }
            Debug.Log("we closin the panel");
        }

        private void MoveAllCharacters()
        {
            Dictionary<Character, Room> nextRoomIntentions = new();
            foreach (Character character in charactersInPlay)
            {
                Room nextRoom = character.GetNextRoom(targetRole);
                if (nextRoom != null)
                {
                    nextRoomIntentions[character] = nextRoom;
                }
            }

            if(nextRoomIntentions.Values.All(x => x.isBlindSpot))
            {
                var charList = nextRoomIntentions.Keys.ToList();
                Character randomChar = charList[UnityEngine.Random.Range(0, charList.Count)];
                nextRoomIntentions[randomChar] = ballroom;
            }

            foreach((Character c, Room r) in nextRoomIntentions)
            {
                c.MoveToRoom(r);
            }
        }

        private void OnCharacterClickedHandler(Character character)
        {
            Debug.Log($"GameManager received character click: {character.characterName}");
            
            selectedCharacter = character;
            
            Debug.Log($"About to invoke OnCharacterSelected event. Subscribers: {OnCharacterSelected?.GetInvocationList().Length ?? 0}");
            OnCharacterSelected?.Invoke(character);
            
            Debug.Log($"Selected: {character.characterName} wearing {character.currentMask} mask");
        }

        public void MakeFinalGuess(Character guessedCharacter)
        {
            bool won = guessedCharacter.trueRole.roleType == targetRole;
            isGameActive = false;

            Debug.Log($"Final guess: {guessedCharacter.characterName}");
            Debug.Log($"Target was: {targetRole}");
            Debug.Log($"Result: {(won ? "WIN" : "LOSE")}");

            OnGameEnded?.Invoke(won);
        }

        public RectTransform LoseGraphicsRoot;
        public TextMeshProUGUI LoseText;
        public void Lose()
        {
            EndGame();
            LoseGraphicsRoot.gameObject.SetActive(true);
            LoseText.text = Score.ToString();
            AudioManager.Instance.audioPlayer.PlayOneShot(AudioManager.Instance.endGame);
        }
        
        public RectTransform WinGraphicsRoot;
        public void Win()
        {
            EndGame();
            WinGraphicsRoot.gameObject.SetActive(true);
            AudioManager.Instance.audioPlayer.PlayOneShot(AudioManager.Instance.endGame);
        }

        private void EndGame()
        {
            isGameActive = false;
            Debug.Log("Game ended - Time to make your guess!");
        }

        public List<Character> GetCharactersInRoom(RoomType roomType)
        {
            if (roomDictionary.TryGetValue(roomType, out Room room))
            {
                return room.Occupants;
            }
            return new List<Character>();
        }

        public Character GetCharacterByID(int id)
        {
            return charactersInPlay.FirstOrDefault(c => c.characterID == id);
        }

        public Character GetCharacterByRole(Role role)
        {
            foreach (var c in AllCharacters)
            {
                if (c.trueRole.roleType == role) return c;
            }

            return null;
        }

        public bool IsCharacterGuessed(Role role)
        {
            Character cbr = GetCharacterByRole(role);
            if (cbr == null) return false;
            return cbr.demasked;
        }

        private void InitializeRoomDictionary()
        {
            roomDictionary[RoomType.Ballroom] = ballroom;
            roomDictionary[RoomType.Bathroom] = bathroom;
            roomDictionary[RoomType.Balcony] = balcony;
            roomDictionary[RoomType.Study] = study;
            roomDictionary[RoomType.WineCellar] = wineCellar;
            roomDictionary[RoomType.Courtyard] = courtyard;
        }

        private void SetupRoomConnections()
        {
            foreach (var room in roomDictionary.Values)
            {
                room.connectedRooms.Clear();
            }

            AddBidirectionalConnection(ballroom, study);
            AddBidirectionalConnection(study, bathroom);
            AddBidirectionalConnection(ballroom, wineCellar);
            AddBidirectionalConnection(ballroom, bathroom);
            AddBidirectionalConnection(ballroom, balcony);
            AddBidirectionalConnection(balcony, study);

            AddBidirectionalConnection(balcony, courtyard);

            bathroom.isBlindSpot = true;
            wineCellar.isBlindSpot = true;
        }

        private void AddBidirectionalConnection(Room room1, Room room2)
        {
            if (!room1.connectedRooms.Contains(room2))
                room1.connectedRooms.Add(room2);

            if (!room2.connectedRooms.Contains(room1))
                room2.connectedRooms.Add(room1);
        }
        
        public List<RoleData> GetUsedRoles()
        {
            return new List<RoleData>(rolesInPlay);
        }

        public List<RoleData> GetUnusedRoles()
        {
            return new List<RoleData>(rolesNotInPlay);
        }

        public Character GetTargetCharacter()
        {
            foreach(Character c in charactersInPlay)
            {
                if(c.trueRole.roleType == targetRole)
                {
                    return c;
                }
            }
            Debug.LogError("Was unable to find target character :(");
            return null;
        }

        public bool IsGameActive => isGameActive;
        public int CurrentRound => currentRound;
        public List<Character> AllCharacters => charactersInPlay;
        
        public List<Character> AllMaskedCharacters => AllCharacters.Where(x => x.currentMask != MaskType.None).ToList();
        public List<Character> AllCharactersWithoutMask => AllCharacters.Where(x => x.currentMask == MaskType.None).ToList();
        public int Score => AllCharactersWithoutMask.Count();
    }
}