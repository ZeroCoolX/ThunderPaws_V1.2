using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {
    public static DialogueManager Instance;

    public Animator Animator;

    public Text NameText;
    public Text DialogueText;

    private Queue<string> _sentences;

    private void Awake() {
        // Ensure the object persists through the lifetime of the game
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start () {
        _sentences = new Queue<string>();
	}

    public void StartDialogue(Dialogue dialogue) {
        print("Starting dialogue!");

        Animator.SetBool("IsOpen", true);

        NameText.text = dialogue.Name;

        _sentences.Clear();
        foreach(var sentence in dialogue.Sentences) {
            _sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
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
        Animator.SetBool("IsOpen", false);
        print("End of conversation");
    }
}
