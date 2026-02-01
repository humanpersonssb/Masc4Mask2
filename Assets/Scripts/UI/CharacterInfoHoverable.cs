using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MasqueradeGame.UI
{
    public class CharacterInfoHoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private InfobooxHover _hoverbox;
        [SerializeField] private RoleData _roleData;
        [SerializeField] private Image _guessedImage;
        
        public GameManager gameManager;

        private void Update()
        {
            bool isGuessed = gameManager.IsCharacterGuessed(_roleData.roleType);
            _guessedImage.gameObject.SetActive(isGuessed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hoverbox.Open(_roleData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hoverbox.Close();
        }
    }
}