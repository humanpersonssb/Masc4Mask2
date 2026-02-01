using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasqueradeGame
{
    public class InteractionManager : MonoBehaviour
    {
        public GameStatementGeneratorManager statementGenerator;

        [Header("UI References")]
        public GameObject interactionPanel;
        public Image characterMaskImage;
        public Image characterSilhouetteImage;
        public TextMeshProUGUI characterNameText;
        public TextMeshProUGUI dialogueText;

        [Header("Interaction Buttons")]
        public Button option1Button;
        public Button option2Button;
        public Button option3Button;
        public Button option4Button;
        public TextMeshProUGUI option1Text;
        public TextMeshProUGUI option2Text;
        public TextMeshProUGUI option3Text;
        public TextMeshProUGUI option4Text;
        public Button option5Button;
        public Button option6Button;
        public Button option7Button;
        public Button option8Button;
        public Button option9Button;
        public Button option10Button;
        
        [Header("Close Button")]
        public Button closeButton;
        
        [Header("Continue Button")]
        public Button continueButton;

        private Character currentCharacter;
        private GameManager gameManager;

        public event Action OnInteractionComplete;
        public int state = 0;
        private List<Button> optionButtons;
        private List<string> responses;

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            responses = new();

            option1Button.onClick.AddListener(() => OnOptionSelected(1));
            option2Button.onClick.AddListener(() => OnOptionSelected(2));
            option3Button.onClick.AddListener(() => OnOptionSelected(3));
            option4Button.onClick.AddListener(() => OnOptionSelected(4));
            option5Button.onClick.AddListener(() => OnOptionSelected(5));
            option6Button.onClick.AddListener(() => OnOptionSelected(6));
            option7Button.onClick.AddListener(() => OnOptionSelected(7));
            option8Button.onClick.AddListener(() => OnOptionSelected(8));
            option9Button.onClick.AddListener(() => OnOptionSelected(9));
            option10Button.onClick.AddListener(() => OnOptionSelected(10));
            
            optionButtons = new()
            {
                option1Button, option2Button, option3Button, option4Button, option5Button, option6Button,
                option7Button, option8Button, option9Button, option10Button
            };
            
            closeButton.onClick.AddListener(CloseInteraction);
            continueButton.onClick.AddListener(OnContinueClicked);

            interactionPanel.SetActive(false);
            if (continueButton != null)
                continueButton.gameObject.SetActive(false);

            if (gameManager != null)
            {
                gameManager.OnCharacterSelectedForGuessing += ShowInteraction;
            }
        }
        
        public void ShowInteraction(Character character)
        {
            Debug.Log($"InteractionManager.ShowInteraction called for {character.characterName}");
            
            currentCharacter = character;
            interactionPanel.SetActive(true);
            option1Button.gameObject.SetActive(true);
            option2Button.gameObject.SetActive(true);
            option3Button.gameObject.SetActive(true);
            option4Button.gameObject.SetActive(true);
            option5Button.gameObject.SetActive(false);
            option6Button.gameObject.SetActive(false);
            option7Button.gameObject.SetActive(false);
            option8Button.gameObject.SetActive(false);
            option9Button.gameObject.SetActive(false);
            option10Button.gameObject.SetActive(false);
            
            if (continueButton != null)
                continueButton.gameObject.SetActive(false);
            
            Debug.Log($"Interaction panel activated: {interactionPanel.activeSelf}");

            characterMaskImage.sprite = character.maskIcon;
            characterSilhouetteImage.sprite = character.silhouetteSprite;
            characterNameText.text = $"{character.currentMask} Mask";
            option1Text.text = "Guess";
            option2Text.text = "Interrogate";
            option3Text.text = "Befriend";
            option4Text.text = "Talk";
            dialogueText.text = "How has your evening been going?";
            state = 0;

            //this is the rebellion
                                dialogueText.text = "Well then, who am I?";
                    option1Text.text = "You are the Spy!";
                    option2Text.text = "You are the Guard!";
                    option3Text.text = "You are the Tailor!";
                    option4Text.text = "You are the Baron!";
                    option5Button.gameObject.SetActive(true);
                    option6Button.gameObject.SetActive(true);
                    option7Button.gameObject.SetActive(true);
                    option8Button.gameObject.SetActive(true);
                    option9Button.gameObject.SetActive(true);
                    option10Button.gameObject.SetActive(true);
            EnableOptions();
        }

        private void OnOptionSelected(int optionNumber)
        {
            if (currentCharacter == null)
                return;
                /*
            if(state == 0)
            {
                if(optionNumber > 2)
                {
                    //da magic for olin
                    string response = GetStatement(
                        gameManager, 
                        currentCharacter,
                        optionNumber == 3 ? InteractionOption.Befriend : InteractionOption.Talk
                    ).Statement;
                    responses.Add(response);
                
                    dialogueText.text = response;

                    DisableOptions();
                    ShowContinueButton();
                    
                } else if(optionNumber == 2)
                {
                    state = 1;
                    //logic to deduce if we were wrong and wrong prompt.
                    dialogueText.text = "What... would you like to know?";
                    option1Text.text = "What's your reputation?";
                    option2Text.text = "Who didn't show up?";
                    option3Text.text = "Who are you looking for?";
                    option4Text.text = "Who are you?";
                } else{
                    state = 2;
                    dialogueText.text = "Well then, who am I?";
                    option1Text.text = "You are the Spy!";
                    option2Text.text = "You are the Guard!";
                    option3Text.text = "You are the Tailor!";
                    option4Text.text = "You are the Baron!";
                    option5Button.gameObject.SetActive(true);
                    option6Button.gameObject.SetActive(true);
                    option7Button.gameObject.SetActive(true);
                    option8Button.gameObject.SetActive(true);
                    option9Button.gameObject.SetActive(true);
                    option10Button.gameObject.SetActive(true);

                    Character character = gameManager.GetTargetCharacter();
                    int targetInfluence = character.trueRole.influenceValue;
                    //optionButtons[targetInfluence].gameObject.SetActive(false);
                    optionButtons[targetInfluence].interactable = false;
                }
            } 
            else if (state == 1)
            {
                //da number will be second question
                InterrogateOption interrogationOption = optionNumber switch
                {
                    1 => InterrogateOption.Number,
                    2 => InterrogateOption.DidntShow,
                    3 => InterrogateOption.HasTalked,
                    _ => InterrogateOption.OneOfThree
                };
                string response = GetStatement(
                    gameManager, 
                    currentCharacter,
                    InteractionOption.Interrogate,
                    interrogateOption: interrogationOption).Statement;
                responses.Add(response);

                dialogueText.text = response;

                DisableOptions();
                ShowContinueButton();
                state = 0;
            } 
            else if (state == 2)
            {
                //da number will be second question
                string response = GetStatement(
                    gameManager, 
                    currentCharacter,
                    InteractionOption.DirectGuess,
                    directRoleGuess: (Role)(optionNumber - 1)).Statement;
                responses.Add(response);

                dialogueText.text = response;

                DisableOptions();
                ShowContinueButton();
                state = 0;
            }*/



                    Character character = gameManager.GetTargetCharacter();
                    int targetInfluence = character.trueRole.influenceValue;
                    //optionButtons[targetInfluence].gameObject.SetActive(false);
                    optionButtons[targetInfluence].interactable = false;
        }

        private void ShowContinueButton()
        {
            if (continueButton != null)
                continueButton.gameObject.SetActive(true);
        }

        private void HideContinueButton()
        {
            if (continueButton != null)
                continueButton.gameObject.SetActive(false);
        }

        private void OnContinueClicked()
        {
            HideContinueButton();
            CompleteInteraction();
            AudioManager.Instance.audioPlayer.PlayOneShot(AudioManager.Instance.endGame);
        }

        private string GetPlaceholderResponse(int option)
        {
            switch (option)
            {
                case 1:
                    return $"The {currentCharacter.currentMask} mask says: \"My role? That's for me to know...\"";
                case 2:
                    return $"The {currentCharacter.currentMask} mask says: \"Did you hear the princess had to stay home?\"";
                case 3:
                    return $"The {currentCharacter.currentMask} mask says: \"Oh could you be a dear and direct me to the King? I have yet to see him.\"";
                case 4:
                    return $"The {currentCharacter.currentMask} mask says: \"Eric Zimmerman once said, this is a positive feedback loop.\"";
                default:
                    return "...";
            }
        }

        private GameStatement GetStatement(
            GameManager game,
            Character speaker,
            InteractionOption interactionOption, 
            InterrogateOption interrogateOption = default,
            Role directRoleGuess = default)
        {
            bool isTruthful = speaker.CurrentRole != Role.Pope;
            Room myRoom = speaker.currentRoom;
            List<Character> everyoneInMyRoom = myRoom.Occupants;
            List<Role> rolesInMyRoom = everyoneInMyRoom.Select(x => x.CurrentRole).ToList();
            bool guardIsInRoom = rolesInMyRoom.Contains(Role.Guard);

            GameStatement result;

            if(interactionOption != InteractionOption.Talk && guardIsInRoom)
            {
                result = statementGenerator.StoppedByGuard.GenerateStatement(game, speaker, isTruthful);
            }
            else
            {
                switch(interactionOption)
                {
                    case InteractionOption.DirectGuess:
                        statementGenerator.DirectGuess.CachedGuess = directRoleGuess;
                        result = statementGenerator.DirectGuess.GenerateStatement(game, speaker, isTruthful);
                        break;
                    case InteractionOption.Interrogate:
                        StatementGenerator generator = statementGenerator.GetInterrogateGenerator(speaker, interrogateOption);
                        result = generator.GenerateStatement(game, speaker, isTruthful);
                        break;
                    case InteractionOption.Befriend:
                        result = statementGenerator.GetBefriendGenerator(speaker).GenerateStatement(game, speaker, isTruthful);
                        break;
                    default:
                        result = statementGenerator.GetTalkGenerator(speaker).GenerateStatement(game, speaker, isTruthful);
                        break;
                }
            }

            gameManager.playerInfluence += result.ReputationChange;
            return result;
        }

        private void CompleteInteraction()
        {
            CloseInteraction();
            OnInteractionComplete?.Invoke();

            if (gameManager != null)
            {
                gameManager.AdvanceTurn();
            }
        }

        public void CloseInteraction()
        {
            interactionPanel.SetActive(false);
            currentCharacter = null;
            HideContinueButton();
            EnableOptions();
        }

        private void DisableOptions()
        {
            option1Button.interactable = false;
            option2Button.interactable = false;
            option3Button.interactable = false;
            option4Button.interactable = false;
            option5Button.interactable = false;
            option6Button.interactable = false;
            option7Button.interactable = false;
            option8Button.interactable = false;
            option9Button.interactable = false;
            option10Button.interactable = false;
        }

        private void EnableOptions()
        {
            option1Button.interactable = true;
            option2Button.interactable = true;
            option3Button.interactable = true;
            option4Button.interactable = true;
            option5Button.interactable = true;
            option6Button.interactable = true;
            option7Button.interactable = true;
            option8Button.interactable = true;
            option9Button.interactable = true;
            option10Button.interactable = true;
        }

        private void OnDestroy()
        {
            option1Button.onClick.RemoveAllListeners();
            option2Button.onClick.RemoveAllListeners();
            option3Button.onClick.RemoveAllListeners();
            option4Button.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
            
            if (continueButton != null)
                continueButton.onClick.RemoveAllListeners();

            if (gameManager != null)
            {
                gameManager.OnCharacterSelected -= ShowInteraction;
            }
        }
    }
}