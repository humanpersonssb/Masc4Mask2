using UnityEngine;

namespace MasqueradeGame
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance => FindAnyObjectByType<AudioManager>();
        public AudioSource audioPlayer;
        public AudioClip startGame;
        public AudioClip endGame;
        public AudioClip hoverSomething;
        public AudioClip clickSomething;
    }
}