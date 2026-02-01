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

        public void Init(Character character)
        {
            _activeCharacter = character;
            _maskField.sprite = character.maskIcon;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => OnClick?.Invoke(character));
        }
    }
}