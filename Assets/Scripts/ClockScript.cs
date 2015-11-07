using UnityEngine;
using System.Collections;
using Cyberconian.Unity;
public class ClockScript : MonoBehaviour
{

    private int seconds;
    private int milliseconds;

    public float totalTime;
    public float multiplier;

	private bool running;

    void Update()
    {
		if (running) 
		{
			totalTime += Time.deltaTime * multiplier;
			milliseconds = (int)(totalTime * 10) % 10;
			seconds = (int)(totalTime);
			gameObject.GetComponent<SevenSegmentDriver> ().Data = (seconds * 10 + milliseconds).ToString ();
		}
    }

    void Start()
    {
        seconds = 0;
        milliseconds = 0;
		running = true;
        gameObject.GetComponent<SevenSegmentDriver>().Data = "000";
    }

    void ChangeSpeed(float mul)
    {
        multiplier = mul;
    }

	void ChangeTime(float time)
	{
		totalTime = time;
		milliseconds = (int)(totalTime * 10) % 10;
		seconds = (int)(totalTime);
		gameObject.GetComponent<SevenSegmentDriver> ().Data = (seconds * 10 + milliseconds).ToString ();
	}

	void Pause()
	{
		running = false;
	}

	void UnPause()
	{
		running = true;
	}

}