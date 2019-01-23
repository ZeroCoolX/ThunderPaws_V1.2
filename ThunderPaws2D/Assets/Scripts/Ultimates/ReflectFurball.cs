
public class ReflectFurball : Ultimate {
    public override void Activate() {
        print("ReflectFurball activated!");
        DeactivateDelegate.Invoke();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
