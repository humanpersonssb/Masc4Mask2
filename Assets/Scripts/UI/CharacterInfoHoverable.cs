using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MasqueradeGame.UI
{
    public class CharacterInfoHoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private InfobooxHover _hoverbox;
        [SerializeField] private RoleData _roleData;
        
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