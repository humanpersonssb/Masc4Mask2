using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MasqueradeGame.UI
{
    public class InteractionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<InteractionButton> OnHover;
        public event Action<InteractionButton> OnUnhover;
        
        public Button Button;
        public TextMeshProUGUI InstructionTextField;
        public StatementGenerator Generator;
        public Role AssociatedRole;
        public CanvasGroup Selector;
        public float SelectorAlphaSpeed;
        public bool SelectorIsOn;

        public void Init(string instruction, StatementGenerator generator, Role associatedRole = default)
        {
            InstructionTextField.text = instruction;
            Generator = generator;
            AssociatedRole = associatedRole;
            SelectorIsOn = false;
            Selector.alpha = 0;
        }

        public GameStatement Generate(GameManager game, Character speaker)
        {
            return Generator.GenerateStatement(game, speaker, speaker.CurrentRole != Role.Pope);
        }

        private void Update()
        {
            Selector.alpha = Mathf.Lerp(Selector.alpha, SelectorIsOn ? 1 : 0, Time.deltaTime * SelectorAlphaSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SelectorIsOn = true;
            AudioManager.Instance.audioPlayer.PlayOneShot(AudioManager.Instance.hoverSomething);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SelectorIsOn = false;
        }
    }
}