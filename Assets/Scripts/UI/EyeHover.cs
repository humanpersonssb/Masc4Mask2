using UnityEngine;
using UnityEngine.EventSystems;

namespace MasqueradeGame.UI
{
    public class EyeHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform _infoHover;
        public void OnPointerEnter(PointerEventData eventData)
        {
            _infoHover.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _infoHover.gameObject.SetActive(false);
        }
    }
}