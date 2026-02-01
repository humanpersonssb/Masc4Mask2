using System;
using System.Collections.Generic;
using System.Linq;
using MasqueradeGame.UI;
using MasqueradeGame.VFX;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MasqueradeGame
{
    public class TalkInteractionManager : MonoBehaviour
    {
        [Header("UI Elements")]
        public RectTransform GraphicsRoot;
        [FormerlySerializedAs("SilhouetteImage")] public Image FullBodyImage;
        public TextMeshProUGUI SpeakerText;
        public TextMeshProUGUI SpeakerName;
        public Button ContinueButton;
        public GameStatementGeneratorManager StatementGeneratorManager;
        public VerticalLayoutGroup OptionsRoot;
        public GridLayoutGroup GuessesRoot;

        [Header("Config")]
        public GameManager GameManager;
        public InteractionButton OptionPrefab;
        public int OptionCount = 4;
        public List<string> IntroductionTexts;

        [Header("State")]
        public Character ActiveCharacter;
        public List<string> Log;

        public ScreenFaderVFX Fader;

        private void Awake()
        {
            Log = new List<string>();
        }

        private void OnEnable()
        {
            GameManager.OnCharacterSelected += HandleSelectCharacter;
            GameManager.OnCharacterSelectedForGuessing += HandleSelectCharacterGuessing;
        }
        
        private void OnDisable()
        {
            GameManager.OnCharacterSelected -= HandleSelectCharacter;
            GameManager.OnCharacterSelectedForGuessing -= HandleSelectCharacterGuessing;
        }

        private void HandleSelectCharacter(Character character)
        {
            Open(character);
        }
        
        private void HandleSelectCharacterGuessing(Character character)
        {
            OpenForGuessing(character);
        }

        public void Open(Character character)
        {
            ActiveCharacter = character;
            GraphicsRoot.gameObject.SetActive(true);
            ContinueButton.gameObject.SetActive(false);
            OptionsRoot.gameObject.SetActive(true);
            GuessesRoot.gameObject.SetActive(false);
            PopulateOptions();
            SpeakerText.text = IntroductionTexts[Random.Range(0, IntroductionTexts.Count)];
            FullBodyImage.sprite = ActiveCharacter.GetCharacterSprite();
        }
        
        public void OpenForGuessing(Character character)
        {
            ActiveCharacter = character;
            GraphicsRoot.gameObject.SetActive(true);
            ContinueButton.gameObject.SetActive(false);
            OptionsRoot.gameObject.SetActive(false);
            GuessesRoot.gameObject.SetActive(true);
            PopulateGuessingOptions();
            SpeakerText.text = "Who am I?";
            FullBodyImage.sprite = ActiveCharacter.GetCharacterSprite();
        }

        private void Update()
        {
            if (ActiveCharacter == null) return;
            FullBodyImage.sprite = ActiveCharacter.GetCharacterSprite();
            SpeakerName.text = ActiveCharacter.GetName();
        }

        public void PopulateOptions()
        {
            foreach (Transform existing in OptionsRoot.transform)
            {
                Destroy(existing.gameObject);
            }
            
            var options = StatementGeneratorManager
                .ManualOptions
                .Where(x => x.CanUse(GameManager, ActiveCharacter))
                .OrderBy(_ => Guid.NewGuid())
                .Take(OptionCount);
            foreach (var option in options)
            {
                var optionInstance = Instantiate(OptionPrefab, OptionsRoot.transform);
                optionInstance.Init(option.GetOptionText(), option);
                optionInstance.Button.onClick.AddListener(() => HandleClickOption(optionInstance));
            }
        }
        
        public void PopulateGuessingOptions()
        {
            foreach (Transform existing in GuessesRoot.transform)
            {
                Destroy(existing.gameObject);
            }

            var allRoles = GameManager.allRoles;
            foreach (RoleData role in allRoles)
            {
                var optionInstance = Instantiate(OptionPrefab, GuessesRoot.transform);
                optionInstance.Init($"{role.roleType.ToString()}", StatementGeneratorManager.DirectGuess, role.roleType);
                optionInstance.Button.onClick.AddListener(() => HandleClickGuess(optionInstance));
            }
        }

        public void DepopulateOptions()
        {
            foreach (Transform existing in OptionsRoot.transform)
            {
                Destroy(existing.gameObject);
            }
            foreach (Transform existing in GuessesRoot.transform)
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

        private void HandleClickGuess(InteractionButton button)
        {
            ContinueButton.gameObject.SetActive(true);
            ContinueButton.onClick.RemoveAllListeners();
            ContinueButton.onClick.AddListener(HandleClickContinueFromGuess);
            DepopulateOptions();
            (button.Generator as SG_GuessMyRole)!.CachedGuess = button.AssociatedRole;
            Fader.DoFade((() =>
            {
                GameStatement statement = button.Generate(GameManager, ActiveCharacter);
                if (statement.Success)
                {
                    ActiveCharacter.SetMask(MaskType.None);
                }
                else
                {
                    GameManager.Lose();
                }
                SpeakerText.text = statement.Statement;
                Log.Add(statement.ToString());
            }));
        }

        private void HandleClickContinue()
        {
            GraphicsRoot.gameObject.SetActive(false);
            GameManager.AdvanceTurn();
        }

        private void HandleClickContinueFromGuess()
        {
            GraphicsRoot.gameObject.SetActive(false);
            GameManager.EventPhase();
        }
    }
}