using UnityEngine;

namespace MasqueradeGame
{

    [CreateAssetMenu(fileName = "New Role", menuName = "Masquerade/Role Data")]
    public class RoleData : ScriptableObject
    {
        [Header("Basic Info")] 
        public Sprite sprite;
        public Role roleType;
        public int influenceValue;
        public string roleName;
        [TextArea(3, 6)]
        public string description;

        [Header("Starting Conditions")]
        public bool hasFixedStartingMask;
        public MaskType fixedMaskType; // For Tailor (always brown)

        [Header("Movement Behavior")]
        public bool hasCustomMovement;
        public MovementPriority movementPriority;

        [Header("Special Abilities")]
        public bool canSwapMasks; // Princess
        public bool copiesTraits; // Spy
        public bool alwaysLies; // Pope
        public bool blocksInteraction; // Guard


        public static int GetInfluenceValue(Role role)
        {
            return (int)role;
        }
    }

    public enum MovementPriority
    {
        Random,              // Default 
        AvoidTarget,         // Baron 
        PreferSpecificRooms, // Countess 
        SeekCrowds,          // Prince 
        SeekHighInfluence,   // Prime Minister
        CopyLastContact      // Spy 
    }
}