using UnityEngine;
using System.Collections;

public class LightClockScript : MonoBehaviour {

    private GameObject topMirror, bottomMirror;
    private GameObject photon;
    private int direction=1;
    private float t=0;
    public float photon_speed;
    public float speed = 0;

	// Use this for initialization
	void Start () {
        topMirror = transform.Find("Top Mirror").gameObject;
        bottomMirror = transform.Find("Bottom Mirror").gameObject;
        photon = transform.Find("Photon").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
        if (speed > photon_speed) speed = photon_speed;     //TODO: Remove this shit and put in the proper code which takes speed of light
        t += direction * Mathf.Sqrt((photon_speed*photon_speed ) - (speed * speed)) * Time.deltaTime;
        if (t > 1) direction = -1;
        if (t < 0) direction = 1;
        photon.transform.position = Vector3.Lerp(topMirror.transform.position, bottomMirror.transform.position, t);
	}

    void ChangeVel(float newVel)
    {
        speed = newVel;
        
    }

    void ChangePhotonVel(float newVel)
    {
        photon_speed = newVel;
    }
}
