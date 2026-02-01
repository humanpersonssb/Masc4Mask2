using UnityEngine;
using UnityEngine.UI;
using MasqueradeGame;
using System.Text;
using TMPro;

public class DevEnd : MonoBehaviour
{
    public GameObject CheatPanel;
    public GameManager gameManager;
    public TextMeshProUGUI cheatText; 
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheatPanel.SetActive(true);
            DisplayAllCharacterRoles();
        }
    }
    
    private void DisplayAllCharacterRoles()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager reference not set in DevEnd!");
            return;
        }
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>=== CHARACTER ROLES ===</b>\n");
        
        foreach (Character character in gameManager.AllCharacters)
        {
            sb.AppendLine($"<b>{character.characterName}</b> ({character.currentMask} mask)");
            sb.AppendLine($"  → {character.trueRole.roleName} (Influence: {character.trueRole.influenceValue})\n");
        }
        
        sb.AppendLine($"<color=yellow><b>TARGET ROLE: {gameManager.targetRole}</b></color>");
        
        Character targetChar = gameManager.GetTargetCharacter();
        if (targetChar != null)
        {
            sb.AppendLine($"<color=green><b>TARGET: {targetChar.characterName} with {targetChar.currentMask} mask</b></color>");
        }
        
        if (cheatText != null)
        {
            cheatText.text = sb.ToString();
        }
        else
        {
            Debug.LogWarning("Cheat Text reference not set!");
        }
        
        Debug.Log(sb.ToString());
    }
}