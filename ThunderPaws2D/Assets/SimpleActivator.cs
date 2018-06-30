using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleActivator : MonoBehaviour {

	public void Deactivate() {
        gameObject.SetActive(false);
    }
}
