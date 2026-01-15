using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System;

/// <summary>
/// Permite cargar un CardData desde Addressables de forma síncrona (bloqueante).
/// Si prefieres no bloquear, conviértelo en IEnumerator y haz yield return en el gestor de coroutines.
/// </summary>
public static class CardDataLoader
{
    public static CardData LoadCardDataSync(string address)
    {
        // Inicia la carga
        AsyncOperationHandle<CardData> handle = Addressables.LoadAssetAsync<CardData>(address);
        // Espera hasta que termine
        handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;

        Debug.LogError($"No se pudo cargar CardData en la dirección '{address}'.");
        return null;
    }

    /// <summary>
    /// Ejemplo de carga asíncrona si quisieras no bloquear el hilo principal:
    /// StartCoroutine(CardLoader.LoadCardDataAsync("Cards/Sapphire", card => { … usar card … }));
    /// </summary>
    public static IEnumerator LoadCardDataAsync(string address, Action<CardData> callback)
    {
        var handle = Addressables.LoadAssetAsync<CardData>(address);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
            callback?.Invoke(handle.Result);
        else
        {
            Debug.LogError($"Error cargando CardData (async) desde '{address}'.");
            callback?.Invoke(null);
        }
    }
}
