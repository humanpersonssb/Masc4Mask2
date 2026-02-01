using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MasqueradeGame.VFX
{
    public class CurtainTransition : MonoBehaviour
    {
        public Image mover;
        public float moveDuration = 0.5f;
        public float moveStay = 0.2f;
        public Transform up;
        public Transform down;

        public void DoFade(Action apexCallback)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(mover.transform.DOMove(down.position, moveDuration));
            sequence.AppendInterval(moveStay);
            sequence.JoinCallback(() => apexCallback());
            sequence.Append(mover.transform.DOMove(up.position, moveDuration));
        }
    }
}