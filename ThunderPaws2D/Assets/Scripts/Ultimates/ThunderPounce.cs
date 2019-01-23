

public class ThunderPounce : Ultimate {
    public override void Activate() {
        print("ThunderPounce activated!");
        DeactivateDelegate.Invoke();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
