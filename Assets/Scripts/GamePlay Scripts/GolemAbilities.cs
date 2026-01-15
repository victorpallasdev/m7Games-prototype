using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GolemAbilities
{
        public IEnumerator RootSmash(GolemController golem, PlayerCharacterController dwarf)
        {
            Debug.Log("El Golem usa Root Smash!");

            yield return new WaitForSeconds(0.5f); // Animación

            // Enraíza 2 turnos
            StatusEffect rootEffect = StatusEffectCatalog.CreateEffect("Root", 0, 3);
            dwarf.AddStatusEffect(rootEffect);

            // Golpe de 40 de naturaleza
            Dictionary<Element, int> elementalDamage = new Dictionary<Element, int> { { Element.Nature, 40 } };
            dwarf.StartCoroutine(dwarf.TakeDamage(0, elementalDamage));

            yield return new WaitForSeconds(0.5f);

            Debug.Log("Root Smash finalizado.");
            golem.IsAtacking = false;
        }
}
