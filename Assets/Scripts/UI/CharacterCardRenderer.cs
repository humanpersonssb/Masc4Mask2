using System;
using UnityEngine;
using UnityEngine.UI;

namespace MasqueradeGame.UI
{
    public class CharacterCardRenderer : MonoBehaviour
    {
        public event Action<Character> OnClick;
        
        [SerializeField] private Image _maskField;
        [SerializeField] private Button _button;

        private Character _activeCharacter;

        private void OnEnable()
        {
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
    }
}