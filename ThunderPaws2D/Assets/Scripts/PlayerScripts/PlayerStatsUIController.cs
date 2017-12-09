using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsUIController : MonoBehaviour {

    /// <summary>
    /// Rectangle that indicates health level
    /// </summary>
    [SerializeField]
    private RectTransform healthBarRect;
    /// <summary>
    /// Text within rectangle indicating health level
    /// </summary>
    [SerializeField]
    private RectTransform ultimateBarRect;

    private void Start() {
        if (healthBarRect == null) {
            Debug.LogError("No HealthBarRect found");
            throw new UnassignedReferenceException();
        }
        if (ultimateBarRect == null) {
            Debug.LogError("No ultimateBarRect found");
            throw new UnassignedReferenceException();
        }
    }

    /// <summary>
    /// Update health references and visual indicator
    /// </summary>
    /// <param name="_cur"></param>
    /// <param name="_max"></param>
    public void SetHealthStatus(int _cur, int _max) {
        //Calculate percentage of max health
        float value = (float)_cur / _max;
        //TODO: Change color of bar over time
        healthBarRect.localScale = new Vector3(value, healthBarRect.localScale.y, healthBarRect.localScale.z);
    }

    /// <summary>
    /// Update ultimate references and visual indicator
    /// </summary>
    /// <param name="_cur"></param>
    /// <param name="_max"></param>
    public void SetUltimateStatus(int _cur, int _max) {
        //Calculate percentage of max health
        float value = (float)_cur / _max;
        //TODO: Change color of bar over time
        ultimateBarRect.localScale = new Vector3(value, ultimateBarRect.localScale.y, ultimateBarRect.localScale.z);
    }
}
