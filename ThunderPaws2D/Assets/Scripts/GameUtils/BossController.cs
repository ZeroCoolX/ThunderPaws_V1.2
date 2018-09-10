using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour {
    private Transform _target;
    private float nextTimeToSearch;
    private float searchDelay = 0.25f;

    protected void Start() {
        if (_target == null) {
            FindPlayer();
            return;
        }
    }

    protected void FindPlayer() {
        GameObject searchResult = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);

        if (nextTimeToSearch <= Time.time) {
            if (searchResult != null) {
                _target = searchResult.transform;
            }
            nextTimeToSearch = Time.time + searchDelay;
        }
    }

}
