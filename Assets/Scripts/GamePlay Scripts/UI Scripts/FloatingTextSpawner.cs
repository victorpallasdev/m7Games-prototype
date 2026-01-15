using System.Collections.Generic;
using UnityEngine;
#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

public class FloatingTextSpawner : MonoBehaviour
{
    public static FloatingTextSpawner Instance { get; private set; }
    [SerializeField] private GameObject floatingTextPrefab;


    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Spawnea todos los números de daño a la vez (físico + elementales), sin solaparse (slots).
    /// parent: normalmente tu healthBarCanvas (Transform/RectTransform).
    /// worldPos: punto base donde quieres que salgan (mismo que usabas).
    /// </summary>
public void SpawnDamageBatch(
    Vector3 worldPos,
    Transform parent,
    int physicalDamage,
    List<(Element element, int dmg)> elements
)
    {
    worldPos.y += 150f;
    int total = (physicalDamage > 0 ? 1 : 0) + elements.Count;
    if (total <= 0) return;

    int nextSlot = 0;

    // Slot 0: físico (si existe)
    if (physicalDamage > 0)
    {
        SpawnOne(worldPos, parent, slotIndex: nextSlot, slotCount: total, ft =>
        {
            ft.TakeDamage(physicalDamage);
        });
        nextSlot++; // a partir de aquí, elementales
    }

    // Elementales: 1,2,3,...
    foreach (var e in elements)
    {
        if (e.dmg <= 0) continue;

        SpawnOne(worldPos, parent, slotIndex: nextSlot, slotCount: total, ft =>
        {
            ft.TakeDamage(e.element, e.dmg);
        });

        nextSlot++;
    }
}

    /// <summary>
    /// Curación en lote (por si quieres spawnear varios ticks). Si es uno, puedes llamar a SpawnHealOne.
    /// </summary>
    public void SpawnHealBatch(Vector3 worldPos, Transform parent, List<int> heals)
    {
        int total = heals.Count;
        if (total == 0) return;
        int center = (total - 1) / 2;
        int nextLeft = center - 1;
        int nextRight = center + 1;
        bool goLeft = true;
        worldPos.y += 150f;
        for (int i = 0; i < heals.Count; i++)
        {
            int slot = (i == 0) ? center : (goLeft ? nextLeft-- : nextRight++);
            goLeft = !goLeft;

            int healVal = heals[i];
            SpawnOne(worldPos, parent, Mathf.Clamp(slot, 0, total - 1), total, (ft) =>
            {
                ft.TakeHeal(healVal);
            });
        }
    }

    public void SpawnHealOne(Vector3 worldPos, Transform parent, int heal)
    {
        worldPos.y += 150f;
        SpawnOne(worldPos, parent, 0, 1, (ft) => ft.TakeHeal(heal));
    }
    public void SpawnAbsorbOne(Vector3 worldPos, Transform parent, int abosrb)
    {
        worldPos.y += 40f;
        SpawnOne(worldPos, parent, 0, 1, (ft) => ft.AbsorbPhysical(abosrb));
    }

    // ===== Helpers =====

    private void SpawnOne(
    Vector3 worldPos,
    Transform parent,
    int slotIndex,
    int slotCount,
    System.Action<FloatingText> setup)
{
    var inst = Instantiate(floatingTextPrefab, worldPos, Quaternion.identity, parent);
    var ft = inst.GetComponent<FloatingText>();
    if (ft != null)
    {
        ft.SetSlot(slotIndex, slotCount); // usa el spacing explícito si lo implementaste
        setup(ft);
    }
}
}
