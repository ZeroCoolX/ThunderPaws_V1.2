using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour {

    public GameObject LoadingScreenVisual;
    public Slider LoadingSlider;
    public Text LoadingText;

    private string[] _loadingTexts = {
        "Waking up Thunderpaws from cryo-sleep.",
        "Handing out standard issue FuzzBusters.",
        "Using nip for maximum alertness.",
        "Deploying drop shock paw pods.",
        "Thunderpaws incoming..."
    };
    private float _progressLastFrame;
    private int index = 0;

    public void LoadScene(string sceneName, string audio) {
        LoadingScreenVisual.SetActive(true);
        StartCoroutine(LoadNewScene(sceneName, audio));
    }

    // The coroutine runs on its own at the same time as Update() and takes an integer indicating which scene to load.
    IEnumerator LoadNewScene(string scene, string audio) {
        // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
        AsyncOperation async = SceneManager.LoadSceneAsync(scene);
        LoadingText.text = _loadingTexts[index++];
        async.allowSceneActivation = false;

        // While the asynchronous operation to load the new scene is not yet complete, continue waiting until it's done.
        while (!async.isDone) {
             LoadingSlider.value = async.progress;

            if (async.progress >= 0.9f) {
                var fabricatedProgress = async.progress + 0.023f;
                while(index < _loadingTexts.Length) {
                    LoadingSlider.value = Mathf.Clamp(fabricatedProgress, 0.9f, 1f);
                    LoadingText.text = _loadingTexts[index];
                    ++index;
                    var randomWait = Random.Range(1, 3);
                    fabricatedProgress += 0.023f;
                    yield return new WaitForSeconds(randomWait);
                }
                AudioManager.Instance.StopSound(audio);
                async.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
