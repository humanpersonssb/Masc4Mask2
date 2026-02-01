using System;
using System.Collections.Generic;
using System.Linq;
using MasqueradeGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MasqueradeGame
{
    public class TalkInteractionManager : MonoBehaviour
    {
        [Header("UI Elements")]
        public RectTransform GraphicsRoot;
        public Image SilhouetteImage;
        public TextMeshProUGUI SpeakerText;
        public Button ContinueButton;
        public GameStatementGeneratorManager StatementGeneratorManager;
        public VerticalLayoutGroup OptionsRoot;

        [Header("Config")]
        public GameManager GameManager;
        public InteractionButton OptionPrefab;
        public int OptionCount = 4;
        public List<string> IntroductionTexts;

        [Header("State")]
        public Character ActiveCharacter;
        public List<string> Log;

        private void Awake()
        {
            Log = new List<string>();
        }

        private void OnEnable()
        {
            GameManager.OnCharacterSelected += HandleSelectCharacter;
        }
        
        private void OnDisable()
        {
            GameManager.OnCharacterSelected -= HandleSelectCharacter;
        }

        private void HandleSelectCharacter(Character character)
        {
            Open(character);
        }

        public void Open(Character character)
        {
            ActiveCharacter = character;
            GraphicsRoot.gameObject.SetActive(true);
            ContinueButton.gameObject.SetActive(false);
            PopulateOptions();
            SpeakerText.text = IntroductionTexts[Random.Range(0, IntroductionTexts.Count)];
            if (ActiveCharacter.currentMask == MaskType.None)
            {
                SilhouetteImage.gameObject.SetActive(false);
            }
            else
            {
                SilhouetteImage.gameObject.SetActive(true);
                SilhouetteImage.sprite = ActiveCharacter.silhouetteSprite;
            }
        }

        public void PopulateOptions()
        {
            foreach (Transform existing in OptionsRoot.transform)
            {
                Destroy(existing.gameObject);
            }
            
            var options = StatementGeneratorManager.ManualOptions.OrderBy(_ => Guid.NewGuid()).Take(OptionCount);
            foreach (var option in options)
            {
                var optionInstance = Instantiate(OptionPrefab, OptionsRoot.transform);
                optionInstance.Init(option.GetOptionText(), option);
                optionInstance.Button.onClick.AddListener(() => HandleClickOption(optionInstance));
            }
        }

        public void DepopulateOptions()
        {
            foreach (Transform existing in OptionsRoot.transform)
            {
                Destroy(existing.gameObject);
            }
        }

        private void HandleClickOption(InteractionButton button)
        {
            ContinueButton.gameObject.SetActive(true);
            ContinueButton.onClick.RemoveAllListeners();
            ContinueButton.onClick.AddListener(HandleClickContinue);
            DepopulateOptions();
            var statement = button.Generate(GameManager, ActiveCharacter);
            SpeakerText.text = statement.Statement;
            Log.Add(statement.ToString());
        }

        private void HandleClickContinue()
        {
            GraphicsRoot.gameObject.SetActive(false);
            GameManager.AdvanceTurn();
        }
    }
}