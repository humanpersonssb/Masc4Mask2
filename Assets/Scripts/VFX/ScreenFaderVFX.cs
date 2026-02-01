using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MasqueradeGame.VFX
{
    public class ScreenFaderVFX : MonoBehaviour
    {
        public Image fader;
        public float fadeDuration = 0.5f;
        public float fadeStay = 0.2f;

        public void DoFade(Action apexCallback)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(fader.DOFade(1, fadeDuration/2));
            sequence.AppendInterval(fadeStay);
            sequence.JoinCallback(() => apexCallback());
            sequence.Append(fader.DOFade(0, fadeDuration/2));
        }
    }
}