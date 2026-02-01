using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MasqueradeGame.UI
{
    public class InteractionButton : MonoBehaviour
    {
        public Button Button;
        public TextMeshProUGUI InstructionTextField;
        public StatementGenerator Generator;

        public void Init(string instruction, StatementGenerator generator)
        {
            InstructionTextField.text = instruction;
            Generator = generator;
        }

        public GameStatement Generate(GameManager game, Character speaker)
        {
            return Generator.GenerateStatement(game, speaker, speaker.CurrentRole == Role.Pope);
        }
    }
}