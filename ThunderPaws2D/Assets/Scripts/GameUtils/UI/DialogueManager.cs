using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {
    public static DialogueManager Instance;

    public Animator Animator;

    public Text NameText;
    public Text DialogueText;
    public Transform DialogUi;
    public string SceneToLoadAfter;

    private Queue<string> _sentences;

    private void Awake() {
        // Ensure the object persists through the lifetime of the game
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }

        _sentences = new Queue<string>();
    }

    // Use this for initialization
    void Start () {
	}

    public void StartDialogue(Dialogue dialogue) {
        print("Starting dialogue!");
        // Freeze everything in the scene
        if(Animator != null) {
            Animator.SetBool("IsOpen", true);
        }

        NameText.text = dialogue.Name;

        _sentences.Clear();
        foreach(var sentence in dialogue.Sentences) {
            _sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
        Invoke("StopTime", 1f);
    }

    private void StopTime() {
        foreach (var player in GameObject.FindGameObjectsWithTag(GameConstants.Tag_Player)) {
            player.GetComponent<PlayerInputController>().enabled = false;
        }
        Time.timeScale = 0f;
    }

    public void DisplayNextSentence() {
        if(_sentences.Count == 0) {
            EndDialogue();
            return;
        }
        var curSentence = _sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(curSentence));

    }

    IEnumerator TypeSentence(string sentence) {
        DialogueText.text = "";
        foreach(var letter in sentence.ToCharArray()) {
            DialogueText.text += letter;
            yield return null;
        }
    }

    public void EndDialogue() {
        DialogueText.text = "...";
        // Freeze everything in the scene
        Time.timeScale = 1f;
        if (Animator != null) {
            Animator.SetBool("IsOpen", false);
        }
        if (!string.IsNullOrEmpty(SceneToLoadAfter)) {
            AudioManager.Instance.stopSound(GameConstants.Audio_BackstoryMusic);
            SceneManager.LoadScene(GameConstants.Scene_LevelName_1);
        }
        foreach (var player in GameObject.FindGameObjectsWithTag(GameConstants.Tag_Player)) {
            player.GetComponent<PlayerInputController>().enabled = true;
        }
        print("End of conversation");
        if(DialogUi != null) {
            DialogUi.gameObject.SetActive(false);
        }
    }
}
