using System.Collections.Generic;
using UnityEngine;

namespace MasqueradeGame
{

    public class Room : MonoBehaviour
    {
        [Header("Room Configuration")]
        public RoomType roomType;
        public string displayName;
        public bool isBlindSpot; 

        [Header("Connections")]
        public List<Room> connectedRooms = new List<Room>();

        [Header("Visual")]
        public Transform[] characterPositions;
        public SpriteRenderer roomSprite;

        public List<Character> currentOccupants = new List<Character>();

        public List<Character> Occupants => currentOccupants;
        public int OccupantCount => currentOccupants.Count;
        
        private Dictionary<Character, int> characterSlots = new Dictionary<Character, int>();
        private void Awake()
        {
            if (connectedRooms == null)
                connectedRooms = new List<Room>();
        }


public void AddCharacter(Character character)
{
    if (currentOccupants.Contains(character))
        return;

    currentOccupants.Add(character);

    int slotIndex = GetNextAvailableSlot();
    characterSlots[character] = slotIndex;

    Debug.Log(
        $"Adding {character.characterName} to {roomType}. " +
        $"Assigned slot {slotIndex}. " +
        $"Total occupants: {currentOccupants.Count}, Positions: {characterPositions.Length}"
    );

    PositionCharacter(character, slotIndex);
}


        public void RemoveCharacter(Character character)
        {
            currentOccupants.Remove(character);
            characterSlots.Remove(character);
        }


        private void PositionCharacter(Character character, int slotIndex)
{
    RectTransform rectTransform = character.GetComponent<RectTransform>();

    if (rectTransform != null)
    {
        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError($"{character.characterName} has no Canvas parent!");
            return;
        }

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector3 worldPos;

            if (characterPositions.Length > 0 && slotIndex < characterPositions.Length)
            {
                Transform positionTransform = characterPositions[slotIndex];

                if (positionTransform == null)
                {
                    Debug.LogError(
                        $"{character.characterName} - Position slot {slotIndex} in {roomType} is NULL!"
                    );
                    worldPos = transform.position;
                }
                else
                {
                    worldPos = positionTransform.position;
                }
            }
            else if (characterPositions.Length > 0)
            {
                int posIndex = slotIndex % characterPositions.Length;
                worldPos = characterPositions[posIndex].position +
                           new Vector3(
                               Random.Range(-0.2f, 0.2f),
                               Random.Range(-0.2f, 0.2f),
                               0
                           );
            }
            else
            {
                worldPos = transform.position;
            }

            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            rectTransform.position = screenPos;
        }
        else
        {
            Debug.LogWarning(
                $"{character.characterName} Canvas is not ScreenSpaceOverlay: {canvas.renderMode}"
            );
        }
    }
    else
    {
        if (characterPositions.Length == 0)
        {
            character.transform.position = transform.position;
            return;
        }

        if (slotIndex < characterPositions.Length)
        {
            character.transform.position = characterPositions[slotIndex].position;
        }
        else
        {
            int posIndex = slotIndex % characterPositions.Length;
            character.transform.position =
                characterPositions[posIndex].position +
                new Vector3(
                    Random.Range(-0.2f, 0.2f),
                    Random.Range(-0.2f, 0.2f),
                    0
                );
        }
    }
}



        public Room GetRandomConnection()
        {
            if (connectedRooms.Count == 0)
                return null;

            return connectedRooms[Random.Range(0, connectedRooms.Count)];
        }


        public bool IsConnectedTo(Room other)
        {
            return connectedRooms.Contains(other);
        }

        public List<Character> GetVisibleCharacters(bool playerHasVision)
        {
            if (isBlindSpot && !playerHasVision)
                return new List<Character>(); 

            return new List<Character>(currentOccupants);
        }


        public Room GetMostCrowdedConnection()
        {
            Room mostCrowded = null;
            int maxCount = -1;

            foreach (Room room in connectedRooms)
            {
                if (room.OccupantCount > maxCount)
                {
                    maxCount = room.OccupantCount;
                    mostCrowded = room;
                }
            }

            return mostCrowded;
        }


        public Room GetRoomWithHighestInfluence()
        {
            Room bestRoom = null;
            int highestInfluence = -1;

            foreach (Room room in connectedRooms)
            {
                foreach (Character character in room.Occupants)
                {
                    if (character.TrueInfluence > highestInfluence)
                    {
                        highestInfluence = character.TrueInfluence;
                        bestRoom = room;
                    }
                }
            }

            return bestRoom;
        }
        
        private int GetNextAvailableSlot()
        {
            for (int i = 0; i < characterPositions.Length; i++)
            {
                if (!characterSlots.ContainsValue(i))
                    return i;
            }

            return characterSlots.Count;
        }

    }
}