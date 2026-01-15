using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public Image progressBarFill;
    public float loadingDuration = 3f;

    public void StartLoading(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, GameSession.Instance.levelMusic));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, List<AudioClip> levelMusicClips)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            yield return null; // espera hasta que la escena esté lista
        }
       
        MusicManager.Instance.FadeOut(0.8f * loadingDuration);

        yield return AnimateLoadingBar();
       
        operation.allowSceneActivation = true;        
    }
    private IEnumerator AnimateLoadingBar()
    {
        progressBarFill.fillAmount = 0f;

        bool done = false;

        LeanTween.value(progressBarFill.gameObject, 0f, 1f, loadingDuration)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnUpdate((float val) =>
            {
                progressBarFill.fillAmount = val;
            })
            .setOnComplete(() => done = true);

        while (!done)
            yield return null;
    }

}
