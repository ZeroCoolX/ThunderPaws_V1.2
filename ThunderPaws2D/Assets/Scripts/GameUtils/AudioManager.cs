using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound {

    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 0.7f;
    [Range(0.5f, 1.5f)]
    public float pitch = 1f;

    [Range(0f, 0.5f)]
    public float randomVolume = 0.1f;
    [Range(0f, 0.5f)]
    public float randomPitch = 0.1f;

    public bool loop = false;

    private AudioSource source;

    public void setSource(AudioSource _source) {
        source = _source;
        source.clip = clip;
        source.loop = loop;
    }

    //randomize for variance
    public void play() {
        source.volume = volume * 1 + Random.Range(-randomVolume / 2f, randomVolume / 2);
        source.pitch = pitch * 1 + Random.Range(-randomPitch / 2f, randomPitch / 2);
        source.Play();
    }

    public void stop() {
        source.Stop();
    }
}

public class AudioManager : MonoBehaviour {

    public static AudioManager Instance;

    [SerializeField]
    Sound[] sounds;

    void Awake() {
        if (Instance != null) {
            if (Instance != this) {
                Destroy(this.gameObject);
            }
        } else {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    void Start() {
        for (int i = 0; i < sounds.Length; ++i) {
            GameObject _go = new GameObject("Sound_" + i + "_" + sounds[i].name);
            _go.transform.SetParent(this.transform);
            sounds[i].setSource(_go.AddComponent<AudioSource>());
        }
    }

    public void playSound(string _name) {
        for (int i = 0; i < sounds.Length; ++i) {
            if (sounds[i].name == _name) {
                //found the sound, now play it!
                sounds[i].play();
                return;
            }
        }

        //no sounds with _name
        Debug.LogWarning("AudioManager: Sound " + _name + " not found in sounds array");
    }

    public void stopSound(string _name) {
        for (int i = 0; i < sounds.Length; ++i) {
            if (sounds[i].name == _name) {
                //found the sound, now play it!
                sounds[i].stop();
                return;
            }
        }
    }

}
