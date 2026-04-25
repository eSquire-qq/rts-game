using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI lumberText;
    public TextMeshProUGUI supplyText;

    public int myPlayerId;

    public void UpdateFromState(StateMsg state)
    {
        Debug.Log("UpdateFromState CALLED");

        if (state == null)
        {
            Debug.Log("STATE NULL");
            return;
        }

        if (state.players == null)
        {
            Debug.Log("PLAYERS NULL");
            return;
        }

        foreach (var p in state.players)
        {
            Debug.Log("PLAYER: " + p.playerId + " GOLD: " + p.gold);

            if (p.playerId != myPlayerId) continue;

            goldText.text = "" + p.gold;
            lumberText.text = "" + p.lumber;
            supplyText.text = "" + p.usedSupply + " / " + p.maxSupply;

            return;
        }
    }
}