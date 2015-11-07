using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class BaseScriptFE : MonoBehaviour {

	public float speed = 10.0F;
	public Queue<Vector3> acc = new Queue<Vector3> ();
	public Vector3 accNew;
    public GUIStyle style;
	public Vector3 pos;
	public Vector3 vel;
	public Vector3 avgAcc;
    public int neighbourhoodSize = 10;
    public int stopTimeThreshold = 100;
    public float neighbourhoodThreshold = 0.007f;
    public int maximaSpacing = 10;
    public int smoothingWindowSize = 20;
    public float assumedMax;
    public Transform flashScreen;
    public GameObject curve;
    public GameObject compassNeedle;
    public GameObject chartManager;
    public GameObject simulator;
	public GameObject regularClock;
    public GameObject warpedClock;
    public GameObject virtualPlayer;
    public GameObject regularLightClock;

    //public GameObject lineGraph;

    private float calcVel;
    private float globalCalcVelX;
    private float globalCalcVelY;
	private Vector3 currVel;
	private Vector3 maxVel;
	private Vector3 dispVel;
	private float nextTime;
    private float max;
    private float frameTime;
    private float speedOfLight;

    private List<Vector3> accLog = new List<Vector3>();
    private List<Vector3> velLog = new List<Vector3>();
    private List<int> maximaLog = new List<int>();
    private List<float> rotationLog = new List<float>();
	private List<float> warpedTimeLog = new List<float>();

	private GraphScript graphXScript;
	private GraphScript graphYScript;
	private GraphScript graphVelScript;

    void Start()
    {
        frameTime = 0;

		Input.gyro.enabled = true;
        avgAcc = Vector3.zero;
		vel = Vector3.zero;
		pos = transform.position;

        chartManager = GameObject.Find("Chart Manager");
        simulator = GameObject.Find("Simulator");
		regularClock = GameObject.Find ("Regular Clock");
        regularLightClock = GameObject.Find("Regular Light Clock");
        warpedClock = GameObject.Find("Warped Clock");
        virtualPlayer = GameObject.Find("Virtual Player");
        simulator.SendMessage("initializeSimulation");



		nextTime = 10;
        speedOfLight = 15;

        
        maximaLog.Add(0);
    }

    void OnGUI()
    {
        //GUI.skin.label.fontSize = 40;
        //GUI.Label(new Rect(10, 10, 1000, 100), "Acceleration is: " + accNew.y);
        //GUI.Label(new Rect(10, 100, 1000, 100), "Time between frames is" + frameTime + "");
        //GUI.Label(new Rect(10, 200, 1000, 100), "Gyro enabled? " + Input.gyro.enabled);
        //GUI.Label(new Rect(10, 200, 1000, 100), "Velocity is: " + globalCalcVelX + "," + globalCalcVelY);

        //speedOfLight = GUI.HorizontalSlider(new Rect(550, 30, 300, 50), speedOfLight, 1.0f, 20.0f);
    }

	public void SliderChanged(float newVal)
	{
		speedOfLight = Mathf.Exp(newVal);
        Debug.Log(newVal + " " + speedOfLight);
        virtualPlayer.transform.FindChild("Warped Light Clock").SendMessage("ChangePhotonVel", speedOfLight / Mathf.Exp(3));
        regularLightClock.SendMessage("ChangePhotonVel", speedOfLight / Mathf.Exp(3));
	}

    public void OnBack()
    {
        Application.LoadLevel(0);
    }

    void Update()
    {
        //Flashes screen and calls the OnTouch function when a double finger tap is detected
        if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            StartCoroutine("FlashScreen");
        }

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.LoadLevel(0);
		}
    }

    IEnumerator CaptureScreenshotAfterDelay(Texture2D tex)
    {
        yield return new WaitForSeconds(0.5f);
        
    }

    IEnumerator FlashScreen()
    {
        for (float i = 0; i < 1; i += 0.1f)
        {
            flashScreen.GetComponent<Renderer>().material.color = Color.Lerp(new Color(255.0f / 255.0f, 96.0f / 255.0f, 96.0f / 255.0f), new Color(96.0f / 255.0f, 182.0f / 255.0f, 255.0f / 255.0f), i);
            yield return new WaitForSeconds(0.05f);
        }
    }

    //Function to check if a given point in a list of points is a local maxima
    bool CheckIfMaxima(int i, List<float> list, int neighbourhoodSize, float neighbourhoodThreshold)
    {
        //Disregard the first few and last few points (because they don't have enough neighbours)
        if (i < neighbourhoodSize || i > list.Count - neighbourhoodSize)
            return false;
        
        //Ensure that slope of line from previous point to next point is within 10% of Max acceleration. 
        //Mathematically it should be zero, but that is far from realistic.
        if (Mathf.Abs(Mathf.Abs(list[i - 1]) - Mathf.Abs(list[i + 1])) > 0.1f * assumedMax)
            return false;

        //Take average of a fixed number of points on the left and right of the maxima point
        float leftNeighbourhood = 0;
        float rightNeighbourhood = 0;
        for (int j=0; j<neighbourhoodSize; j++)
        {
            leftNeighbourhood += Mathf.Abs(list[i - j]);
            rightNeighbourhood += Mathf.Abs(list[i + j]);
        }
        leftNeighbourhood /= neighbourhoodSize;
        rightNeighbourhood /= neighbourhoodSize;

        //Ensure that the average of the left and right neighbourhoods are lower than the maxima point by a threshold
        //if (leftNeighbourhood > Mathf.Abs(list[i]) - neighbourhoodThreshold * assumedMax || rightNeighbourhood > Mathf.Abs(list[i]) - neighbourhoodThreshold * assumedMax)
        if (leftNeighbourhood > list[i] - neighbourhoodThreshold * assumedMax || rightNeighbourhood > list[i] - neighbourhoodThreshold * assumedMax)
            return false;

        if (list[i - neighbourhoodSize+1] > list[i] - 2 * neighbourhoodThreshold || list[i + neighbourhoodSize-1] > list[i] - 2 * neighbourhoodThreshold)
            return false;

        //Disregard Minima (This eliminates all extrema below 0. TODO: Needs to allow Maxima below 0)
        if (list[i] < 0)
        {
            return false;
        }

        return true;
    }
    

    //Gets called after every few milliseconds
	void FixedUpdate()
	{
        frameTime = Time.deltaTime;

        //Get newest acceleration value from accelerometer
		accNew = new Vector3(Input.gyro.userAcceleration.x, Input.gyro.userAcceleration.y, 0);
        
        //Add new value to queue of acceleration values, get avg of the queue, and use that as the new acceleration value
		acc.Enqueue (accNew);
		if (acc.Count > smoothingWindowSize)
			acc.Dequeue ();
        avgAcc = Vector3.zero;
		foreach (Vector3 tempAcc in acc) {
			avgAcc += tempAcc;
		}
		avgAcc /= acc.Count;

        accLog.Add(avgAcc); 


        
        //Check if the point from a few iterations back is a maxima (The first point that has enough right neighbours)
        if (accLog.Count > 2 + 2 * neighbourhoodSize)
        {
            List<Vector3> v3List = new List<Vector3>();
            v3List = accLog.GetRange(accLog.Count - 2 - 2 * neighbourhoodSize, 1 + 2 * neighbourhoodSize);
            List<float> list = new List<float>();
            foreach (Vector3 val in v3List)
            {
                list.Add(val.y);
            }

            //Search for maxima and replace bands of maxima with a single (the first) one
            if (CheckIfMaxima(1 + neighbourhoodSize, list, neighbourhoodSize, neighbourhoodThreshold))
            {
                if (maximaLog.Count == 0)
                    maximaLog.Add(accLog.Count - 1 - neighbourhoodSize);
                else
                    if (accLog.Count - 1 - neighbourhoodSize > maximaLog[maximaLog.Count - 1] + maximaSpacing)
                    {
                        maximaLog.Add(accLog.Count - 1 - neighbourhoodSize);

                        simulator.SendMessage("simulationSetSpeed", (50.0f / ((accLog.Count - 1 - neighbourhoodSize) - maximaLog[maximaLog.Count - 2])));
                    }
            }
        }
        //Send relevant information to the simulator
        simulator.SendMessage("simulationChangeDir", compassNeedle.transform.rotation.eulerAngles.z);
        simulator.SendMessage("simulationStep", Time.deltaTime);

        //Calculate an approximate current velocity using previous 2 maxima values
        if (maximaLog.Count > 2)
        {
            // If it has been a considerable time since the last step, the user has probably stopped
            if ((accLog.Count - 1 - neighbourhoodSize) > maximaLog[maximaLog.Count - 1] + stopTimeThreshold)
                calcVel = 0;
            else
                calcVel = (50.0f / ((maximaLog[maximaLog.Count - 1] - maximaLog[maximaLog.Count - 2])));
        }
        else
            calcVel = 0;

        if (calcVel > speedOfLight) calcVel = speedOfLight;

		float totalTime = warpedClock.GetComponent<ClockScript>().totalTime;
		warpedTimeLog.Add (totalTime);
        warpedClock.SendMessage("ChangeSpeed", Mathf.Sqrt(1 - Mathf.Pow(calcVel / speedOfLight, 2)));
        Vector2 avgAcc2v = new Vector2(avgAcc.x, avgAcc.y);
        chartManager.SendMessage("UpdateAccLog", avgAcc2v);

        //Debug.Log(calcVel);

        float dir = compassNeedle.transform.rotation.eulerAngles.z*(Mathf.PI / 180.0f);
        float calcVelX = Mathf.Abs(Mathf.Sin(dir)*calcVel);
        float calcVelY = Mathf.Abs(Mathf.Cos(dir)*calcVel);

        globalCalcVelX = calcVelX;
        globalCalcVelY = calcVelY;

        virtualPlayer.SendMessage("ChangeVel", calcVel);

        currVel += avgAcc;

        compassNeedle.transform.Rotate(new Vector3(0, 0, Input.gyro.rotationRateUnbiased.z*Time.deltaTime*(180.0f/Mathf.PI)));
        rotationLog.Add(compassNeedle.transform.rotation.eulerAngles.z);
	}
}
