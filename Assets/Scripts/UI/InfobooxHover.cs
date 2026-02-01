using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MasqueradeGame.UI
{
    public class InfobooxHover : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private TextMeshProUGUI _nameField;
        [SerializeField] private TextMeshProUGUI _influenceField;
        [SerializeField] private TextMeshProUGUI _abilityField;
        [SerializeField] private Image _portraitField;

        public void Open(RoleData role)
        {
            _root.gameObject.SetActive(true);
            Render(role);
        }

        public void Close()
        {
            _root.gameObject.SetActive(false);
        }

        public void Render(RoleData role)
        {
            _nameField.text = role.roleType.ToString();
            _influenceField.text = role.influenceValue.ToString();
            _abilityField.text = role.description;
            _portraitField.sprite = role.sprite;
        }
    }
}