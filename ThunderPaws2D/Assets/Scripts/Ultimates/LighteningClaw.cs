
public class LighteningClaw : Ultimate {
    public override void Activate() {
        print("LighteningClaw activated!");
        DeactivateDelegate.Invoke();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
