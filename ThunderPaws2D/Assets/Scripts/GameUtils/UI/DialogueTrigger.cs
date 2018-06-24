using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour {
    public Dialogue Dialog;

    public void TriggerDialogue() {
        DialogueManager.Instance.StartDialogue(Dialog);
    }
}
