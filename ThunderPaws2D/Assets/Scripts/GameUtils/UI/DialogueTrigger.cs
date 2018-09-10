using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour {
    public Dialogue Dialog;
    public float DialogueDelay;

    private void Start() {
        Invoke("StartDialog", DialogueDelay);
    }

    private void StartDialog() {
        var box = transform.Find("DialogueBox");
        if (box != null && !box.gameObject.activeSelf) {
            box.gameObject.SetActive(true);
        }
        DialogueManager.Instance.StartDialogue(Dialog);
    }

}
