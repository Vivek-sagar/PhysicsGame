using UnityEngine;
using System.Collections;

public class StartScreenScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void StartGame()
	{
		Application.LoadLevel (1);
	}

	public void HelpScreen()
	{
		Application.LoadLevel (2);
	}

    public void FeynmansExperimentScreen()
    {
        Application.LoadLevel(4);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
