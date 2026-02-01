using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace MasqueradeGame
{
    public class Character : MonoBehaviour
    {
        [Header("Identity")]
        public int characterID;
        public string characterName;

        [Header("Role (Hidden)")]
        public RoleData trueRole;
        private Role copiedRole; 
        private bool isUsingCopiedRole;

        [Header("Mask (Visible)")]
        public MaskType currentMask;
        public Sprite maskIcon;
        public Sprite silhouetteSprite;

        [Header("Location")]
        public Room currentRoom;

        [Header("Visual Representation")]
        public Image maskImage; 
        
        [Header("Visual Novel Display")]
        public Image maskIconUI; 
        public Image silhouetteUI; 

        [Header("Interaction")]
        public Button characterButton;
        public bool canBeInteractedWith = true;

        public event Action<Character> OnCharacterClicked;
        public event Action<Room, Room> OnRoomChanged;

        public HashSet<Role> RolesIveContacted;
        public Character lastContactedCharacter;
        private Room previousRoom;

        public int TrueInfluence => trueRole.influenceValue;
        public Role CurrentRole => isUsingCopiedRole ? copiedRole : trueRole.roleType;

        public bool WasInLastSwap = false;

        private void Awake()
        {
            RolesIveContacted = new();
            Debug.Log($"Character Awake called for {gameObject.name}");
            
            if (characterButton == null)
            {
                Debug.LogError($"Character {gameObject.name} has no Button assigned! Trying to find one...");
                characterButton = GetComponent<Button>();
            }
            
            if (characterButton != null)
            {
                Debug.Log($"Button found for {gameObject.name}, adding listener");
                characterButton.onClick.AddListener(OnClicked);
            }
            else
            {
                Debug.LogError($"Character {gameObject.name} still has no Button component!");
            }
        }

        private void Start()
        {
            Debug.Log($"Character Start called for {gameObject.name}");
        }

        private void Update()
        {
            maskIconUI.sprite = maskIcon;
    
            // Hide sprites
            if (currentRoom != null && currentRoom.isBlindSpot)
            {
                if (maskIconUI != null)
                    maskIconUI.enabled = false;
        
                if (silhouetteUI != null)
                    silhouetteUI.enabled = false;
            }
            else
            {
                if (maskIconUI != null)
                    maskIconUI.enabled = true;
        
                if (silhouetteUI != null)
                    silhouetteUI.enabled = true;
            }
        }

        public Sprite GetCharacterSprite()
        {
            if (currentMask == MaskType.None)
            {
                return trueRole.sprite;
            }
            return maskIcon;
        }

        public void OnClicked()
        {
            Debug.Log($"Character button clicked: {characterName}, Can Interact: {canBeInteractedWith}");
            
            if (canBeInteractedWith)
            {
                Debug.Log($"Invoking OnCharacterClicked event for {characterName}");
                OnCharacterClicked?.Invoke(this);
            }
        }

public void Initialize(RoleData role, MaskVisuals maskVisuals, Room startingRoom)
{
    trueRole = role;
    currentMask = maskVisuals.maskType;
    maskIcon = maskVisuals.maskIcon;
    silhouetteSprite = maskVisuals.silhouette;
    
    currentRoom = startingRoom;
    previousRoom = startingRoom;

    MoveToRoom(startingRoom, true);
    UpdateVisuals();
}

        public void MoveToRoom(Room newRoom, bool isInitialPlacement = false)
        {
            if (currentRoom != null && !isInitialPlacement)
            {
                currentRoom.RemoveCharacter(this);
                previousRoom = currentRoom;
            }

            currentRoom = newRoom;
            newRoom.AddCharacter(this);

            OnRoomChanged?.Invoke(previousRoom, currentRoom);

            if (trueRole.roleType == Role.Princess && !isInitialPlacement)
            {
                CheckPrincessMaskSwap();
            }
        }

        private void CheckPrincessMaskSwap()
        {
            if (currentRoom.OccupantCount == 2)
            {
                foreach (Character other in currentRoom.Occupants)
                {
                    if (other != this)
                    {
                        SwapMasks(other);
                        Debug.Log($"Princess {characterName} swapped masks with {other.characterName}");
                        break;
                    }
                }
            }
        }

        public void SetMask(MaskType mask)
        {
            currentMask = mask;
        }

        public void SwapMasks(Character other)
        {
            MaskType temp = currentMask;
            currentMask = other.currentMask;
            other.currentMask = temp;

            Sprite tempS = maskIcon;
            maskIcon = other.maskIcon;
            other.maskIcon = tempS;
            UpdateVisuals();
            other.UpdateVisuals();

            WasInLastSwap = true;
            other.WasInLastSwap = true;
        }

        public void CopyTraits(Character other)
        {
            if (trueRole.roleType == Role.Spy)
            {
                copiedRole = other.CurrentRole;
                isUsingCopiedRole = true;
                lastContactedCharacter = other;
                Debug.Log($"Spy {characterName} copied role from {other.characterName}: {copiedRole}");
            }
        }

        public void MakeContact(Character other)
        {
            lastContactedCharacter = other;
            RolesIveContacted.Add(other.trueRole.roleType);

            if (trueRole.roleType == Role.Spy)
            {
                CopyTraits(other);
            }
        }

        private void UpdateVisuals()
        {
            if (maskImage != null && maskIcon != null)
            {
                maskImage.sprite = maskIcon;
            }

            if (maskIconUI != null && maskIcon != null)
            {
                maskIconUI.sprite = maskIcon;
            }

            if (silhouetteUI != null && silhouetteSprite != null)
            {
                silhouetteUI.sprite = silhouetteSprite;
            }
        }

        private void OnDestroy()
        {
            if (characterButton != null)
            {
                characterButton.onClick.RemoveListener(OnClicked);
            }
        }


        public Room GetNextRoom(Role targetRole)
        {
            if (!trueRole.hasCustomMovement)
            {
                return currentRoom.GetRandomConnection();
            }

            Room nextRoom = null;

            switch (trueRole.movementPriority)
            {
                case MovementPriority.AvoidTarget:
                    nextRoom = GetRoomAvoidingTarget(targetRole);
                    break;

                case MovementPriority.PreferSpecificRooms:
                    nextRoom = GetPreferredRoom(RoomType.Bathroom, RoomType.WineCellar);
                    break;

                case MovementPriority.SeekCrowds:
                    nextRoom = currentRoom.GetMostCrowdedConnection();
                    break;

                case MovementPriority.SeekHighInfluence:
                    nextRoom = currentRoom.GetRoomWithHighestInfluence();
                    break;

                case MovementPriority.CopyLastContact:
                    if (lastContactedCharacter != null && isUsingCopiedRole)
                    {
                        nextRoom = lastContactedCharacter.GetNextRoom(targetRole);
                    }
                    break;
            }

            return nextRoom ?? currentRoom.GetRandomConnection();
        }


        private Room GetRoomAvoidingTarget(Role targetRole)
        {
            foreach (Room room in currentRoom.connectedRooms)
            {
                bool hasTarget = false;
                foreach (Character character in room.Occupants)
                {
                    if (character.trueRole.roleType == targetRole)
                    {
                        hasTarget = true;
                        break;
                    }
                }

                if (!hasTarget)
                {
                    return room;
                }
            }


            return currentRoom.GetRandomConnection();
        }


        private Room GetPreferredRoom(params RoomType[] preferredTypes)
        {
            foreach (Room room in currentRoom.connectedRooms)
            {
                foreach (RoomType type in preferredTypes)
                {
                    if (room.roomType == type)
                    {
                        return room;
                    }
                }
            }

            return currentRoom.GetRandomConnection();
        }


    }
}