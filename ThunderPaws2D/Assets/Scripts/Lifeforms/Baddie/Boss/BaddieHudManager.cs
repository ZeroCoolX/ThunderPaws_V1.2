using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieHudManager : MonoBehaviour {
    public static BaddieHudManager Instance;

    [SerializeField]
    private RectTransform _healthBarRect;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }

        if (_healthBarRect == null) {
            Debug.LogError("No HealthBarRect found");
            throw new UnassignedReferenceException();
        }
    }

    public void SetHealthStatus(float currentHealth, float maxHealth) {
        float healthVal;
        if (currentHealth == 0 && maxHealth == 0) {
            healthVal = 1f;
        } else {
            // Calculate percentage of max health
            healthVal = (float)currentHealth / maxHealth;
        }
        _healthBarRect.localScale = new Vector3(healthVal, _healthBarRect.localScale.y, _healthBarRect.localScale.z);
        print("localScale = " + _healthBarRect.localScale);
    }
}
