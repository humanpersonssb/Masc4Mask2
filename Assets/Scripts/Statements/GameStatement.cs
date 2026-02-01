using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace MasqueradeGame
{
    public class GameStatement
    {
        public int ReputationChange;
        public string Statement;

        public GameStatement(string s, int repChange = 0)
        {
            Statement = s;
            ReputationChange = repChange;
        }

        public static GameStatement DummyStatement()
        {
            return new GameStatement("You look very pretty today!");
        }
    }

    public enum StatementType
    {
        MyNumberOperation, WhoDidntShowUp, HaveTalkedTo, WentToLocation, Fluff,
        OneNonTarget, RespondToGuess
    }

    // Instantiate these at the beginning of each game, and call their generate function.
    // They may hold state (e.g. the player has already learned a piece of information, and you don't want to
    // give it to them again.)
    public abstract class StatementGenerator
    {
        public abstract bool CanUse(GameManager game, Character speaker);
        public abstract GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue);
    }

    public class SG_MyInfluenceIsBetween : StatementGenerator
    {
        const int MAX_INFLUENCE = 9;
        private int _rangeMin;
        private int _rangeMax;
        public HashSet<Role> RolesUsed;

        public SG_MyInfluenceIsBetween(int rangeMin, int rangeMax)
        {
            _rangeMin = rangeMin;
            _rangeMax = rangeMax;
            RolesUsed = new();
        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            return !RolesUsed.Contains(speaker.trueRole.roleType);
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            int myInfluence = speaker.TrueInfluence;
            int min = 0;
            int max = 0;
            const int MAX_ATTEMPTS = 100;
            int currentAttempts = 0;

            do
            {
                int range = Random.Range(_rangeMin, _rangeMax);
                min = Random.Range(0, MAX_INFLUENCE - range + 2);
                max = min + range;
                if(isTrue == IsInRange(myInfluence, min, max)) // true and in range, or false and not in range
                {
                    break;
                }
            } while(currentAttempts++ < MAX_ATTEMPTS);

            if(currentAttempts >= MAX_ATTEMPTS)
            {
                return GameStatement.DummyStatement();
            }

            RolesUsed.Add(speaker.trueRole.roleType);
            return new GameStatement($"My influence is between {min} and {max} (inclusive).");
        }

        private bool IsInRange(int num, int min, int max)
        {
            return num >= min && num <= max;
        }
    }

    public class SG_MyInfluenceIsEven : StatementGenerator
    {
        public HashSet<Role> RolesUsed;
        public SG_MyInfluenceIsEven()
        {
            RolesUsed = new();
        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            return !RolesUsed.Contains(speaker.trueRole.roleType);
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            List<int> primeNumbers = new List<int>{2,3,5,7};
            int myInfluence = speaker.TrueInfluence;
            bool isEven = myInfluence % 2 == 0;
            bool isPrime = primeNumbers.Contains(myInfluence);

            float rng = Random.value;
            RolesUsed.Add(speaker.trueRole.roleType);
            
            if(isTrue == isEven)
            {
                return new("My influence is an even number.");
            }
            else
            {
                return new("My influence is an odd number.");
            }
           
        }
    }

    public class SG_MyInfluenceIsPrime : StatementGenerator
    {
        public HashSet<Role> RolesUsed;
        public SG_MyInfluenceIsPrime()
        {
            RolesUsed = new();
        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            return !RolesUsed.Contains(speaker.trueRole.roleType);
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            List<int> primeNumbers = new List<int>{2,3,5,7};
            int myInfluence = speaker.TrueInfluence;
            bool isEven = myInfluence % 2 == 0;
            bool isPrime = primeNumbers.Contains(myInfluence);

            float rng = Random.value;
            RolesUsed.Add(speaker.trueRole.roleType);
            
            if(isTrue == isPrime)
            {
                return new("My influence is a prime number.");
            }
            else
            {
                return new("My influence is not a prime number.");
            }
        
        }
    }

    public class SG_WhoDidntShowUp : StatementGenerator
    {
        public HashSet<Role> LearnedRoles;

        public SG_WhoDidntShowUp()
        {
            LearnedRoles = new();
        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            return LearnedRoles.Count < 2;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            HashSet<Role> didntShowUp = isTrue ? GetWhoDidntShowUp(game) : GetWhoDidShowUp(game);
            if(didntShowUp.Count == LearnedRoles.Count)
            {
                return GameStatement.DummyStatement();
            }

            Role role;
            do
            {
                List<Role> rolesList = didntShowUp.ToList();
                role = rolesList[Random.Range(0, rolesList.Count)];
            } while (LearnedRoles.Contains(role));

            if(isTrue)
            {
                LearnedRoles.Add(role);
            }
            return new($"I overheard that the {role} didn't make it to the party.");
        }

        private HashSet<Role> GetWhoDidntShowUp(GameManager game)
        {
            return game.GetUnusedRoles().Select(x => x.roleType).ToHashSet();
        }

        private HashSet<Role> GetWhoDidShowUp(GameManager game)
        {
            return game.GetUsedRoles().Select(x => x.roleType).ToHashSet();
        }
    }

    public class SG_IHaveTalkedTo : StatementGenerator
    {
        public Dictionary<Role, HashSet<Role>> LearnedDict;

        public SG_IHaveTalkedTo()
        {
            LearnedDict = new();
        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            if(game.currentRound < 3) return false;
            return !LearnedDict.ContainsKey(speaker.trueRole.roleType) || LearnedDict[speaker.trueRole.roleType].Count < 2;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            if(!LearnedDict.ContainsKey(speaker.trueRole.roleType))
            {
                LearnedDict[speaker.trueRole.roleType] = new();
            }
            float rng = Random.value;
            List<Role> rolesToLearn = isTrue ? GetTalkHistory(speaker).ToList() : GetUntalkHistory(game, speaker).ToList();
            if(rolesToLearn.Count == 0)
            {
                return new($"I haven't been in the same room as anyone today.");
            }
            if(rolesToLearn.Count == 1)
            {
                Role roleToLearn2 = rolesToLearn[0];
                Role anotherRandomRole = game.AllRoles.Where(x => x != roleToLearn2).ToList()[Random.Range(0, game.AllRoles.Count)];
                return new($"I've only gotten to one person so far today. It was either the {roleToLearn2} or the {anotherRandomRole}. I seem to have forgotten.");
            }
            Role roleToLearn;
            const int MAX_ATTEMPTS = 30;
            int currentAttempts = 0;
            do
            {
                roleToLearn = rolesToLearn[Random.Range(0, rolesToLearn.Count)];
            } while(LearnedDict[speaker.trueRole.roleType].Contains(roleToLearn) && currentAttempts++ < MAX_ATTEMPTS);
          
        
            LearnedDict[speaker.trueRole.roleType].Add(roleToLearn);
            return new($"I have met up with {roleToLearn} today.");
        }

        private HashSet<Role> GetTalkHistory(Character speaker)
        {
            return speaker.RolesIveContacted.Where(x => x != speaker.trueRole.roleType).ToHashSet();
        }

        private HashSet<Role> GetUntalkHistory(GameManager game, Character speaker)
        {
            return game.AllRoles.Except(speaker.RolesIveContacted.ToHashSet()).Where(x => x != speaker.trueRole.roleType).ToHashSet();
        }
    }

    public class SG_IHaventTalkedTo : StatementGenerator
    {
        public Dictionary<Role, HashSet<Role>> LearnedDict;

        public SG_IHaventTalkedTo()
        {
            LearnedDict = new();
        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            if(game.currentRound < 3) return false;
            return !LearnedDict.ContainsKey(speaker.trueRole.roleType) || LearnedDict[speaker.trueRole.roleType].Count < 2;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            if(!LearnedDict.ContainsKey(speaker.trueRole.roleType))
            {
                LearnedDict[speaker.trueRole.roleType] = new();
            }
            float rng = Random.value;
            List<Role> rolesToLearn = !isTrue ? GetTalkHistory(speaker).ToList() : GetUntalkHistory(game, speaker).ToList();
            if(rolesToLearn.Count == 0)
            {
                return new($"I have introduced myself to everyone at least once today.");
            }
            Role roleToLearn;
            const int MAX_ATTEMPTS = 30;
            int currentAttempts = 0;
            do
            {
                roleToLearn = rolesToLearn[Random.Range(0, rolesToLearn.Count)];
            } while(LearnedDict[speaker.trueRole.roleType].Contains(roleToLearn) && currentAttempts++ < MAX_ATTEMPTS);
          
        
            LearnedDict[speaker.trueRole.roleType].Add(roleToLearn);
            return new($"Have you seen {roleToLearn}? I have yet to find them today.");
        }

        private HashSet<Role> GetTalkHistory(Character speaker)
        {
            return speaker.RolesIveContacted.Where(x => x != speaker.trueRole.roleType).ToHashSet();
        }

        private HashSet<Role> GetUntalkHistory(GameManager game, Character speaker)
        {
            return game.AllRoles.Except(speaker.RolesIveContacted).Where(x => x != speaker.trueRole.roleType).ToHashSet();
        }
    }

    public class SG_IAmOneOfThree : StatementGenerator
    {
        public Dictionary<Role, HashSet<Role>> LearnedRoles;
        public bool RoleHasUsed(Role role) => LearnedRoles.ContainsKey(role);

        public SG_IAmOneOfThree()
        {
            LearnedRoles = new();
        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            return !RoleHasUsed(speaker.trueRole.roleType);
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            if(!LearnedRoles.ContainsKey(speaker.trueRole.roleType))
            {
                LearnedRoles[speaker.trueRole.roleType] = new();
            }

            var allRoles = game.AllRoles.ToList();
            var myRole = speaker.trueRole.roleType;
            List<Role> rolesIGive = isTrue ? new List<Role>(){myRole} : new List<Role>();
            while(true)
            {
                if(rolesIGive.Count >= 3) break;
                Role notMeRole;
                do
                {
                    notMeRole = allRoles[Random.Range(0, allRoles.Count)];
                    if(notMeRole != myRole && !rolesIGive.Contains(notMeRole)) break;
                }
                while(true);
                rolesIGive.Add(notMeRole);
            }

            foreach(var role in rolesIGive)
            {
                LearnedRoles[speaker.trueRole.roleType].Add(role);
            }

            return new($"I am one of the following roles: {rolesIGive[0]}, {rolesIGive[1]}, or {rolesIGive[2]}");
        }
    }

    public class SG_Fluff : StatementGenerator
    {
        private readonly List<string> Fluffs = new()
        {
            "I love Eric Zimmerman!",
            "James is my favourite!",
            "This party is awesome!"     
        };

        public SG_Fluff()
        {

        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            return true;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            return new(Fluffs[Random.Range(0, Fluffs.Count)], 1);
        }
    }

    public class SG_StoppedByGuard : StatementGenerator
    {
        private string _trueStatement = "I can't do that right now. There's a guard in this room (it may or may not be me).";
        private string _falseStatement = "Of course. There definitely isn't a guard present."; // confirmed pope

        public SG_StoppedByGuard()
        {

        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            return false;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            return new(isTrue ? _trueStatement : _falseStatement);
        }
    }

    public class SG_TargetIsOneOfTwo : StatementGenerator
    {
        public int UsageCount;

        public SG_TargetIsOneOfTwo()
        {
            UsageCount = 0;
        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            return UsageCount < 1;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            Character targetCharacter = game.GetTargetCharacter();
            List<MaskType> maskList = game.AllMasks.ToList();
            MaskType targetMask = targetCharacter.currentMask;
            List<MaskType> masksToLearn = isTrue ? new(){targetMask} : new();

            while(true)
            {
                if(masksToLearn.Count >= 2) break;
                MaskType mask;
                do
                {
                    mask = maskList[Random.Range(0, maskList.Count)];
                    if(mask != targetMask && !masksToLearn.Contains(mask)) 
                    {
                        break;
                    }
                } while (true);
                masksToLearn.Add(mask);
            }

            UsageCount++;
            return new($"The target is wearing either the {masksToLearn[0]} mask or the {masksToLearn[1]} mask.");
        }
    }

    public class SG_ILastTalkedTo : StatementGenerator
    {
        public SG_ILastTalkedTo()
        {

        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            return true;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            List<Role> allRoles = game.AllRoles.ToList();

            if(isTrue)
            {
                if(speaker.lastContactedCharacter == null)
                {
                    return new("I haven't spoken with anyone today.");
                }
                Role lastTalkedTo = speaker.lastContactedCharacter.trueRole.roleType;
                return new($"I just talked to the {lastTalkedTo}. They were quite agreeable.");
            }
            else
            {
                Role lastTalkedTo;
                if(speaker.lastContactedCharacter == null)
                {
                    lastTalkedTo = allRoles[Random.Range(0, allRoles.Count)];
                }
                else
                {
                    lastTalkedTo = speaker.lastContactedCharacter.trueRole.roleType;
                }
                Role lie;
                do
                {
                    lie = allRoles[Random.Range(0, allRoles.Count)];
                    if(lie != lastTalkedTo) break;
                } while(true);

                return new($"I just talked to the {lie}. They were quite agreeable.");
            }
        }
    }

    public class SG_GuessMyRole : StatementGenerator
    {
        public Role CachedGuess; // Set this variable before generating the statement!

        public SG_GuessMyRole()
        {

        }

        
        public override bool CanUse(GameManager game, Character speaker)
        {
            return true;
        }


        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            Role myRole = speaker.trueRole.roleType;
            Debug.Log($"Is true: {isTrue} :: myRole: {myRole} :: CachedGuess: {CachedGuess}");
        
            if(isTrue == (myRole == CachedGuess))
            {
                return new($"Well guessed! I am indeed the {CachedGuess}", 1);
            }
            else
            {
                return new($"What!? How dare you insult me, I am NOT the {CachedGuess}!" , -1);
            }
        }
    }

    public class SG_IdentityOfANonTarget : StatementGenerator
    {
        public HashSet<Role> RolesGiven;

        public SG_IdentityOfANonTarget()
        {
            RolesGiven = new();
        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            return RolesGiven.Count < 4;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            Character targetCharacter = game.GetTargetCharacter();
            const int MAX_ATTEMPTS = 40;
            int attempts = 0;
            Character infoCharacter;
            do
            {
                infoCharacter = game.AllCharacters[Random.Range(0, game.AllCharacters.Count)];
                if(infoCharacter != targetCharacter && !RolesGiven.Contains(infoCharacter.trueRole.roleType)) break;
            } while(attempts++ < MAX_ATTEMPTS);
            if(attempts >= MAX_ATTEMPTS)
            {
                return GameStatement.DummyStatement();
            }

            return new($"The {infoCharacter.trueRole.roleType} is the face behind the {infoCharacter.currentMask} mask!");
        }
    }

    public class SG_InterrogateFail : StatementGenerator
    {
        public SG_InterrogateFail()
        {

        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            return false;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            return new($"(This person refuses to be interrogated... they must have higher reputation than you.)" , -1);
        }
    }

    public class SG_BefriendFail : StatementGenerator
    {
        public SG_BefriendFail()
        {

        }

        public override bool CanUse(GameManager game, Character speaker)
        {
            return false;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            return new($"(This person doesn't want to be your friend... their reputation is too far from yours.)" , -1);
        }
    }

    public class SG_SumOfRanks : StatementGenerator
    {
        public override bool CanUse(GameManager game, Character speaker)
        {
            return false;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            int myRank = speaker.TrueInfluence;
            List<Character> twoOthers = game.AllUnmaskedCharacters.OrderBy(_ => Guid.NewGuid()).Take(2).ToList();
            int sumRank = twoOthers.Sum(x => x.TrueInfluence);
            int total = sumRank + myRank;
            if (!isTrue)
            {
                float rng = Random.value;
                if (rng < 0.5f)
                {
                    total += Random.Range(1, 6);
                }
                else
                {
                    total -= Random.Range(1, 6);
                }
            }
            return new($"The sum of my rank, {twoOthers[0].currentMask}'s rank, and {twoOthers[1].currentMask}'s rank is {total}.");
        }
    }

    public class SG_IsRoyalty : StatementGenerator
    {
        public override bool CanUse(GameManager game, Character speaker)
        {
            return false;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            bool sayYes = isTrue == (speaker.TrueInfluence >= 5);
            if (sayYes)
            {
                return new("Yes, I am royalty (my rank is 5 or above).");
            }

            return new("No, I am not royalty (my rank is lower than 5).");
        }
    }

    public class SG_WasInLastSwap : StatementGenerator
    {
        public override bool CanUse(GameManager game, Character speaker)
        {
            return false;
        }

        public override GameStatement GenerateStatement(GameManager game, Character speaker, bool isTrue)
        {
            bool sayYes = isTrue == speaker.WasInLastSwap;
            if (sayYes)
            {
                return new($"Yes, I was in the last mask swap.");
            }
            else
            {
                return new($"No, I was not in the last mask swap.");
            }
        }
    }
}