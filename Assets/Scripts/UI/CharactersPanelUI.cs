using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MasqueradeGame.UI
{
    public class CharactersPanelUI : MonoBehaviour
    {
        [SerializeField] private RectTransform _graphicsRoot;
        [SerializeField] private GridLayoutGroup _gridLayout;
        [SerializeField] private CharacterCardRenderer _cardPrefab;

        public void Open(List<Character> characters, Action<Character> onClick)
        {
            foreach (Transform t in _gridLayout.transform)
            {
                Destroy(t.gameObject);
            }

            foreach (var character in characters)
            {
                CharacterCardRenderer card = Instantiate(_cardPrefab, _gridLayout.transform);
                card.Init(character);
                card.OnClick += c => HandleClickCharacter(c, onClick);
            }
        }

        private void HandleClickCharacter(Character c, Action<Character> onClick)
        {
            _graphicsRoot.gameObject.SetActive(false);
            onClick?.Invoke(c);
        }
    }
}