using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour {
    public Dialogue Dialog;

    private void Start() {
        DialogueManager.Instance.StartDialogue(Dialog);
    }

}
