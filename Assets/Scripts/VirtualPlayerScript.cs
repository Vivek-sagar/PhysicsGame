using UnityEngine;
using System.Collections;

public class VirtualPlayerScript : MonoBehaviour {

    float vel;
	// Use this for initialization
	void Start () {
        vel = 0;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position += new Vector3(vel*Time.deltaTime, 0, 0);
	}

    void ChangeVel(float newVel)
    {
        vel = newVel/10;
        transform.Find("Warped Light Clock").gameObject.SendMessage("ChangeVel", vel);
    }


}
