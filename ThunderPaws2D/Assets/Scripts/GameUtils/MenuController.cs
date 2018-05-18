using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

    /// <summary>
    /// Necessary for collisions
    /// </summary>
    private SimpleCollider Collider;

    // Use this for initialization
    void Start() {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << 8, 3);
    }

    //void OnDrawGizmosSelected() {
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(transform.position, 3);
    //}


    private void Apply(Vector3 v, Collider2D c) {
        SceneManager.LoadScene(GameConstants.Scene_LevelName_1);
        GameMaster.Instance.AudioManager.stopSound(GameConstants.Audio_BossMusic);
        GameMaster.Instance.AudioManager.playSound(GameConstants.Audio_MenuMusic);
    }
}
