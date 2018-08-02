using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PRETTY SURE THIS IS ALSO DEPRECATED
/// </summary>
public class MenuController : MonoBehaviour {

    private SimpleCollider Collider;

    private const int PLAYER_LAYER = 8;

    void Start() {
        // Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << PLAYER_LAYER, 3);
    }

    private void Apply(Vector3 v, Collider2D c) {
        SceneManager.LoadScene(GameConstants.Scene_LevelName_1);
        AudioManager.Instance.StopSound(GameConstants.Audio_MenuMusic);
        AudioManager.Instance.PlaySound(GameConstants.Audio_MainMusic);
    }
}
