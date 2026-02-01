using System;
using UnityEngine;

namespace MasqueradeGame
{

    public enum Role
    {
        Spy = 0,
        Guard = 1,
        Tailor = 2,
        Baron = 3,
        Countess = 4,
        Prince = 5,
        Duke = 6,
        Princess = 7,
        Pope = 8,
        King = 9
    }

    public enum MaskType
    {
        Peacock,
        Rabbit,
        Porcupine,
        Weasel,
        Mouse,
        Deer,
        Fox,
        Wolf,
        None
    }          

    public enum RoomType
    {
        Ballroom,
        Bathroom,
        Balcony,
        Study,
        WineCellar,
        Courtyard
    }


    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    [Serializable]
    public enum InteractionOption
    {
        DirectGuess, Interrogate, Befriend, Talk
    }

    [Serializable]
    public enum InterrogateOption
    {
        Number, DidntShow, HasTalked, OneOfThree

    }
     [System.Serializable]
    public class MaskVisuals
    {
        public MaskType maskType;
        public Sprite maskIcon;
        public Sprite silhouette;
    }
}
