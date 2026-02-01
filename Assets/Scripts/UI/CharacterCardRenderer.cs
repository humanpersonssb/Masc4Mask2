using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MasqueradeGame.UI
{
    public class CharacterCardRenderer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<Character> OnClick;
        
        [SerializeField] private Image _maskField;
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _iAmSelected;

        private Character _activeCharacter;

        private void OnEnable()
        {
            _iAmSelected.gameObject.SetActive(false);
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        public void Init(Character character)
        {
            _activeCharacter = character;
            _maskField.sprite = character.maskIcon;
        }

        private void HandleClick()
        {
            OnClick?.Invoke(_activeCharacter);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _iAmSelected.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _iAmSelected.gameObject.SetActive(false);
        }
    }
}