using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(
    fileName = "GemLibrary",
    menuName = "Gem Library",
    order = 100)]
public class GemLibrary : ScriptableObject
{
    [Tooltip("Arrastra aquí todas las CardData de tipo Gem y DefensiveGem")]
    public List<CardData> gemCards = new List<CardData>();

    /// <summary>
    /// Devuelve una lista con todas las CardData cuyo 'Cardtype' sea Gem o DefensiveGem.
    /// </summary>
    public List<CardData> GetGems()
    {
        return gemCards
            .Where(cd => cd != null &&
                         (cd.cardType == CardType.Gem || cd.cardType == CardType.DefensiveGem))
            .ToList();
    }
}
