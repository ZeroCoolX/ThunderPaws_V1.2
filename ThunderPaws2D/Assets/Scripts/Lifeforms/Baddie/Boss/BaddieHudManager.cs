using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieHudManager : MonoBehaviour {
    public static BaddieHudManager Instance;

    [SerializeField]
    private RectTransform[] _healthBarRects;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }

        if (_healthBarRects == null || _healthBarRects.Length == 0) {
            Debug.LogError("No HealthBarRects found");
            throw new UnassignedReferenceException();
        }
    }

    public void HideHealthBar(int index) {
        _healthBarRects[index].gameObject.SetActive(false);
    }

    public void SetHealthStatus(int healthbarIndex, float currentHealth, float maxHealth) {
        float healthVal;
        if (currentHealth == 0) {
            healthVal = 0f;
        } else {
            // Calculate percentage of max health
            healthVal = (float)currentHealth / maxHealth;
        }
        _healthBarRects[healthbarIndex].localScale = new Vector3(healthVal, _healthBarRects[healthbarIndex].localScale.y, _healthBarRects[healthbarIndex].localScale.z);
    }
}
