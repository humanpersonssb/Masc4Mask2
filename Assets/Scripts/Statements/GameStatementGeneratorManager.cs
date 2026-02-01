using UnityEngine;
using System;
using System.Collections.Generic;

namespace MasqueradeGame
{
    public class GameStatementGeneratorManager : MonoBehaviour
    {
        [SerializeField] private GameManager _game;

        public List<StatementGenerator> LiveGenerators;
        
        public List<StatementGenerator> TalkGenerators;
        public List<StatementGenerator> InterrogateGeneratorsNumberOption;
        public StatementGenerator InterrogateGeneratorDidntShowOption;
        public StatementGenerator InterrogateGeneratorHasTalkedOption;
        public StatementGenerator InterrogateGeneratorOneOfThreeOption;
        public List<StatementGenerator> BefriendGenerators;
        public StatementGenerator StoppedByGuard;
        public SG_GuessMyRole DirectGuess;

        public StatementGenerator InterrogateFail;
        public StatementGenerator BefriendFail;


        private void Awake()
        {
            Init();
        }

        private void Update() // Debug controls
        {
            if(Input.GetKeyDown(KeyCode.LeftBracket))
            {
                Character randomCharacter = _game.AllCharacters[UnityEngine.Random.Range(0, _game.AllCharacters.Count)];
                var talkGenerator = GetTalkGenerator(randomCharacter);
                Debug.Log($"{randomCharacter.trueRole.roleType}: {talkGenerator.GenerateStatement(_game, randomCharacter, randomCharacter.CurrentRole != Role.Pope).Statement}");
            }
            if(Input.GetKeyDown(KeyCode.RightBracket))
            {
                Character randomCharacter = _game.AllCharacters[UnityEngine.Random.Range(0, _game.AllCharacters.Count)];
                var talkGenerator = GetBefriendGenerator(randomCharacter);
                Debug.Log($"{randomCharacter.trueRole.roleType}: {talkGenerator.GenerateStatement(_game, randomCharacter, randomCharacter.CurrentRole != Role.Pope).Statement}");
            }
        }

        private void Init()
        {
            var guessRole = new SG_GuessMyRole();
            var oneOfThree = new SG_IAmOneOfThree();
            var haventTalked = new SG_IHaventTalkedTo();
            var haveTalked = new SG_IHaveTalkedTo();
            var lastTalked = new SG_ILastTalkedTo();
            var influenceEven = new SG_MyInfluenceIsEven();
            var influenceBetween = new SG_MyInfluenceIsBetween(4, 5);
            var stoppedByGuard = new SG_StoppedByGuard();
            var didntShow = new SG_WhoDidntShowUp();
            var interrogateFail = new SG_InterrogateFail();
            var befriendFail = new SG_BefriendFail();
            var identityOfOne = new SG_IdentityOfANonTarget();

            LiveGenerators = new List<StatementGenerator>()
            {
                guessRole, oneOfThree, haventTalked, haveTalked, lastTalked, influenceEven, 
                influenceBetween, stoppedByGuard, didntShow, identityOfOne
            };

            TalkGenerators = new List<StatementGenerator>()
            {
                influenceEven, influenceBetween,
                didntShow, haventTalked, oneOfThree
            };

            InterrogateGeneratorsNumberOption = new List<StatementGenerator>()
            {
                influenceEven, influenceBetween
            };
            InterrogateGeneratorDidntShowOption = didntShow;
            InterrogateGeneratorHasTalkedOption = haveTalked;
            InterrogateGeneratorOneOfThreeOption = oneOfThree;

            BefriendGenerators = new List<StatementGenerator>()
            {
                identityOfOne, lastTalked
            };

            StoppedByGuard = stoppedByGuard;
            DirectGuess = guessRole;

            InterrogateFail = interrogateFail;
            BefriendFail = befriendFail;
        }

        public StatementGenerator GetTalkGenerator(Character character)
        {
            return GetGeneratorFromList(character, TalkGenerators);
        }

        public StatementGenerator GetInterrogateGenerator(Character character, InterrogateOption option)
        {
            if(_game.playerInfluence <= character.TrueInfluence)
            {
                return InterrogateFail;
            }
            switch(option)
            {
                case InterrogateOption.Number:
                    return GetGeneratorFromList(character, InterrogateGeneratorsNumberOption);
                case InterrogateOption.DidntShow:
                    return InterrogateGeneratorDidntShowOption;
                case InterrogateOption.HasTalked:
                    return InterrogateGeneratorHasTalkedOption;
                case InterrogateOption.OneOfThree:
                    return InterrogateGeneratorOneOfThreeOption;
                default:
                    throw new ArgumentOutOfRangeException($"Unimplemented interrogate option: {option}.");
            }
        }

        public StatementGenerator GetBefriendGenerator(Character character)
        {
            if(Math.Abs(_game.playerInfluence - character.TrueInfluence) > 1)
            {
                return BefriendFail;
            }
            return GetGeneratorFromList(character, BefriendGenerators);
        }

        private StatementGenerator GetGeneratorFromList(Character character, List<StatementGenerator> gens)
        {
            const int MAX_ATTEMPTS = 20;
            int attempts = 0;
            StatementGenerator generator;
            do
            {
                generator = gens[UnityEngine.Random.Range(0, gens.Count)];
                if(generator.CanUse(_game, character)) break;
            } while(attempts++ < MAX_ATTEMPTS);

            return generator;
        }
    }
}
