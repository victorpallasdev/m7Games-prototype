using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SummonableUnitController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private PlayerCharacterController playerDwarfController;
    private CombatManager combatManager;
    public int duration;
    private string unitName;
    private GameObject projectilePrefab;


    public void Initialize(SummonableUnitData summonableData)
    {
        unitName = summonableData.unitName;
        spriteRenderer.sprite = summonableData.unitImage;
        animator.runtimeAnimatorController = summonableData.animationController;
        duration = summonableData.duration;
        playerDwarfController = PlayerCharacterController.Instance;
        combatManager = CombatManager.Instance;
        playerDwarfController.AddSummonableUnit(this);
    }

    public IEnumerator UnitAttack(Action onComplete)
    {
        if (unitName == "Turret")
        {
            // Precargamos los assets
            Sprite projectileSprite = null;
            projectilePrefab = null;
            LoadProjectilePrefab(prefab =>
            {
                projectilePrefab = prefab;
            });
            LoadSprite("BulletSprite", sprite =>
            {
                projectileSprite = sprite;
            });
            yield return new WaitUntil(() => projectilePrefab != null && projectileSprite != null);

            Vector3 bulletPosition = new Vector3(-69f, -71f, 0f);

            for (int i = 0; i < 3; i++)
            {
                CameraController cameraController = CameraController.Instance;
                GolemController golem = GolemController.Instance;
                if (golem.IsKnockedOut)
                {
                    break;
                }
                SoundsFXManager soundsFXManager = SoundsFXManager.Instance;
                List<Transform> allEnemies = combatManager.GetFieldEntities();
                allEnemies.Add(golem.transform);
                int randomIndex = UnityEngine.Random.Range(0, allEnemies.Count);
                Transform target = allEnemies[randomIndex];
                Vector3 targetPosition = new Vector3();
                Debug.Log("TARGET = " + target.name);
                animator.SetTrigger("Shot");

                if (target.CompareTag("Golem"))
                {

                    cameraController.SetCameraGolemSpriteHeight();
                    yield return new WaitUntil(() => cameraController.spriteGolemHeight > 0f);
                    targetPosition = golem.transform.localPosition;
                    targetPosition = new Vector3(targetPosition.x, targetPosition.y + cameraController.spriteGolemHeight / 2, 0);
                    soundsFXManager.PlayShotSound();
                    GameObject bullet = Instantiate(projectilePrefab, bulletPosition, Quaternion.identity, transform.parent);
                    ProjectileEntityController entityController = bullet.GetComponent<ProjectileEntityController>();
                    entityController.Initialize(projectileSprite);
                    //entityController.projectileLight.enabled = false;
                    Dictionary<Element, int> elementalPower = new Dictionary<Element, int>();
                    LeanTween.move(bullet, targetPosition, 0.05f).setOnComplete(() =>
                    {
                        StartCoroutine(golem.TakeDamage(10, elementalPower, null, null));
                        Destroy(bullet);
                    });
                    
                }
                else
                {
                    targetPosition = target.localPosition;
                    FieldEntityController entityController = target.GetComponent<FieldEntityController>();
                    GameObject bullet = Instantiate(projectilePrefab, bulletPosition, Quaternion.identity, transform.parent);
                    ProjectileEntityController entityController2 = bullet.GetComponent<ProjectileEntityController>();
                    entityController2.Initialize(projectileSprite);
                    Dictionary<string, int> elementalPower = new Dictionary<string, int>();
                    LeanTween.move(bullet, targetPosition, 0.05f).setOnComplete(() =>
                    {
                        entityController.TakeHit();
                        Destroy(bullet);
                    });
                }
                yield return new WaitForSeconds(0.2f);
            }
            onComplete?.Invoke();
        }    
    }


    public void reduceTurnDurationUnit()
    {
        duration -= 1;
        if (duration <= 0)
        {
            playerDwarfController.RemoveSummonableUnit(this);
            Destroy(gameObject);
        }
    }


    public void LoadProjectilePrefab(Action<GameObject> onLoaded)
    {
        string projectilePrefabKey = "ProjectilePrefab";
        AssetReference reference = new AssetReference(projectilePrefabKey);

        reference.LoadAssetAsync<GameObject>().Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                onLoaded?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogError($"No se pudo cargar el prefab Addressable con el nombre '{projectilePrefabKey}'.");
                onLoaded?.Invoke(null);
            }
        };
    }

    public void LoadSprite(string address, Action<Sprite> onLoaded)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(address);
        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                onLoaded?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"Error al cargar sprite '{address}'");
                onLoaded?.Invoke(null);
            }
        };
    }
    








}