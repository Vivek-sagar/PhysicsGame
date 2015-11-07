using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class BaseScript : MonoBehaviour {

	public float speed = 10.0F;
	public Queue<Vector3> acc = new Queue<Vector3> ();
	public Vector3 accNew;
    public GUIStyle style;
	public Vector3 pos;
	public Vector3 vel;
	public Vector3 avgAcc;
    public Vector3 hRulerDefaultScale;
    public Vector3 vRulerDefaultScale;
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
    public GameObject horizontalRuler;
    public GameObject verticalRuler;

    //public GameObject lineGraph;

    private float calcVel;
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
        warpedClock = GameObject.Find("Warped Clock");
        simulator.SendMessage("initializeSimulation");
        horizontalRuler = GameObject.Find("Horizontal Ruler");
        verticalRuler = GameObject.Find("Vertical Ruler");

		nextTime = 10;
        speedOfLight = 15;

        hRulerDefaultScale = horizontalRuler.transform.localScale;
        vRulerDefaultScale = verticalRuler.transform.localScale;
        
        maximaLog.Add(0);
    }

    void OnGUI()
    {
        //GUI.skin.label.fontSize = 40;
        //GUI.Label(new Rect(10, 10, 1000, 100), "Acceleration is: " + accNew.y);
        //GUI.Label(new Rect(10, 100, 1000, 100), "Time between frames is" + frameTime + "");
		//GUI.Label(new Rect(10, 200, 1000, 100), "Gyro enabled? " + Input.gyro.enabled);
        //GUI.Label(new Rect(10, 200, 1000, 100), "Compass Heading " + compassNeedle.transform.rotation.eulerAngles.z);

        //speedOfLight = GUI.HorizontalSlider(new Rect(550, 30, 300, 50), speedOfLight, 1.0f, 20.0f);
    }

	public void SliderChanged(float newVal)
	{
		speedOfLight = Mathf.Exp(newVal);
	}

    void Update()
    {
        //Flashes screen and calls the OnTouch function when a double finger tap is detected
        if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            StartCoroutine("FlashScreen");
            OnTouch();   
        }

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.LoadLevel(0);
		}
    }

    //Function that is executed at the end of a trial.
    //it draws the required graphs, clears the logs and sets up for the next trial
    void OnTouch()
    {
        currVel = Vector3.zero;
        compassNeedle.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

        Texture2D tex;
        List<float> temp;
        float maxVel;
        
        //TODO: Acceleration X


        //Acceleration Y
        tex = new Texture2D(accLog.Count, 100, TextureFormat.ARGB32, false);

        //Add normalized Acceleration values to temp list
        temp = new List<float>();
        
        foreach (Vector3 acc in accLog)
        {
            temp.Add((acc.y / (float)assumedMax));
        }
        WriteLog(accLog, "accLog");
        //Draw acceleration graph
        DrawGraph(tex, temp, Color.black, 1);

        //Draw lines at maxima points
        DrawGraph(tex, maximaLog, Color.red, 2);
        WriteLog(maximaLog, "maximaLog");

        //Populate velList with velocity values calculated from distances between Maxima points
        List<float> velList = new List<float>();
        List<float> velXList = new List<float>();
        List<float> velYList = new List<float>();
        float dir;
        //velList.Add(0);                             //Append 0 at the beginning of velList to make sure trial starts at 0
        for (int i = 1; i < maximaLog.Count; i++)
        {
            velList.Add((1.0f*3.6f / ((maximaLog[i] - maximaLog[i - 1])*0.02f)));
            dir = rotationLog[maximaLog[i]] *(Mathf.PI/180.0f);
            velXList.Add(-(Mathf.Sin(dir)*(1.0f * 3.6f / ((maximaLog[i] - maximaLog[i - 1]) * 0.02f))));
            velYList.Add((Mathf.Cos(dir)*(1.0f * 3.6f / ((maximaLog[i] - maximaLog[i - 1]) * 0.02f))));
        }
        velList.Add(0);                             //Append 0 at the end of velList to make sure trial ends at 0
        velList.Add(0);                             //Twice

        velXList.Add(0);
        velXList.Add(0);
        velYList.Add(0);
        velYList.Add(0);

        maximaLog.Add(accLog.Count);  

		BezierPath bezierPath;
		List<Vector3> drawingPoints;

        maxVel = Mathf.Max(velXList.ToArray());
        List<Vector3> points = new List<Vector3>();
        List<Vector2> points2v = new List<Vector2>();

        for (int i = 0; i < velXList.Count; i++)
        {
            points.Add(new Vector3(maximaLog[i], velXList[i], 0));
        }
		bezierPath = new BezierPath();
		bezierPath.Interpolate(points, .5f);
		drawingPoints = bezierPath.GetDrawingPoints2();
		foreach (Vector3 point in drawingPoints)
		{
			points2v.Add (new Vector2(point.x, point.y));
		}
		WriteLog(points2v, "velXLog");

        maxVel = Mathf.Max(velYList.ToArray());
        points = new List<Vector3>();
        points2v = new List<Vector2>();
        for (int i = 0; i < velYList.Count; i++)
        {
            points.Add(new Vector3(maximaLog[i], velYList[i], 0));
        }
		bezierPath = new BezierPath();
		bezierPath.Interpolate(points, .5f);
		drawingPoints = bezierPath.GetDrawingPoints2();
		foreach (Vector3 point in drawingPoints)
		{
			points2v.Add (new Vector2(point.x, point.y));
		}
		WriteLog(points2v, "velYLog");
		
		DrawGraph(tex, points, Color.blue, 3);

        //User Acceleration Calculation
        points2v = new List<Vector2>();
		points = new List<Vector3>();
        for (int i = 0; i < velList.Count-1; i++)
        {
            dir = rotationLog[maximaLog[i]] * (Mathf.PI / 180.0f);
			points.Add(new Vector3(maximaLog[i], (velXList[i + 1] - velXList[i]), 0));
        }
		bezierPath = new BezierPath();
		bezierPath.Interpolate(points, .5f);
		drawingPoints = bezierPath.GetDrawingPoints2();
		foreach (Vector3 point in drawingPoints)
		{
			points2v.Add (new Vector2(point.x, point.y));
		}
        WriteLog(points2v, "accXLog");

        points2v = new List<Vector2>();
		points = new List<Vector3>();
        for (int i = 0; i < velList.Count - 1; i++)
        {
            dir = rotationLog[maximaLog[i]] * (Mathf.PI / 180.0f);
            points.Add(new Vector3(maximaLog[i], (velYList[i + 1] - velYList[i]), 0));
        }
		bezierPath = new BezierPath();
		bezierPath.Interpolate(points, .5f);
		drawingPoints = bezierPath.GetDrawingPoints2();
		foreach (Vector3 point in drawingPoints)
		{
			points2v.Add (new Vector2(point.x, point.y));
		}
		WriteLog(points2v, "accYLog");

		
		//Draw all graphs onto the texture and convert to image
        tex.Apply();
        SaveTextureToFile(tex, "accelerationY");

        //Basic Graph to show rotation changes
        temp.Clear();
        tex = new Texture2D(accLog.Count, 100, TextureFormat.ARGB32, false);
        foreach (float rot in rotationLog)
        {
            if (rot > Mathf.PI/2)
                temp.Add((rot - Mathf.PI) / (Mathf.PI / 2));
            else
                temp.Add(rot / (Mathf.PI / 2));
        }
        DrawGraph(tex, temp, Color.black, 1);
        tex.Apply();
        SaveTextureToFile(tex, "RotationZ");

        //Clear logs
        accLog.Clear();
        velLog.Clear();
        maximaLog.Clear();
        maximaLog.Add(0);

        rotationLog.Clear();

		float time1 = regularClock.GetComponent<ClockScript>().totalTime;
		float time2 = warpedClock.GetComponent<ClockScript>().totalTime;
		WriteValues (time1, time2, "clockTimes");

		WriteLog (warpedTimeLog, "WarpedClockTimes");


        //Draw the simulation results
        simulator.SendMessage("simulationDraw");

        //Capture Screenshot
        Texture2D tex2 = new Texture2D(Screen.width, Screen.height);
        //StartCoroutine(CaptureScreenshotAfterDelay(tex2));
        tex2.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex2.Apply();

        SaveTextureToFile(tex2, "GraphMakerAccY");

        //Another way to capture screenshot that doesn't seem to be working
        Application.CaptureScreenshot(Application.persistentDataPath+ "Screenshot.png");



        Application.LoadLevel(3);
    }

    IEnumerator CaptureScreenshotAfterDelay(Texture2D tex)
    {
        yield return new WaitForSeconds(0.5f);
        
    }

    void WriteLog(List<Vector3> log, string fileName)
    {
        string text = "";
        foreach (Vector3 val in log)
        {
            text += val.y + ",";
        }
        text += "\r\n";
        StreamWriter sw = System.IO.File.CreateText("/sdcard/" + fileName + ".csv");
        sw.Close();
        System.IO.File.WriteAllText("/sdcard/" + fileName + ".csv", text);
    }

    void WriteLog(List<Vector2> log, string fileName)
    {
        string text = "";
        foreach (Vector2 val in log)
        {
            text += val.x + ",";
        }
        text += "\r\n";
        foreach (Vector2 val in log)
        {
            text += val.y + ",";
        }
        text += "\r\n";
        StreamWriter sw = System.IO.File.CreateText("/sdcard/" + fileName + ".csv");
        sw.Close();
        System.IO.File.WriteAllText("/sdcard/" + fileName + ".csv", text);
    }

    void WriteLog(List<int> log, string fileName)
    {
        string text = "";
        foreach (int val in log)
        {
            text += val + ",";
        }
        text += "\r\n";
        StreamWriter sw = System.IO.File.CreateText("/sdcard/" + fileName + ".csv");
        sw.Close();
        System.IO.File.WriteAllText("/sdcard/" + fileName + ".csv", text);
    }

	void WriteLog(List<float> log, string fileName)
	{
		string text = "";
		foreach (float val in log)
		{
			text += val + ",";
		}
		text += "\r\n";
		StreamWriter sw = System.IO.File.CreateText("/sdcard/" + fileName + ".csv");
		sw.Close();
		System.IO.File.WriteAllText("/sdcard/" + fileName + ".csv", text);
	}

	void WriteValues(float T1, float T2, string fileName)
	{
		string text = "";
		text = T1 + "\n" + T2;
		StreamWriter sw = System.IO.File.CreateText ("/sdcard/" + fileName + ".txt");
		sw.Close ();
		System.IO.File.WriteAllText ("/sdcard/" + fileName + ".txt", text);
	}

    //Draws one line passing through all points (x=index in list, y=point[i]) in 'points'
    void DrawGraph(Texture2D tex, List<float> points, Color color, int style)
    {
        for (int i = 0; i < tex.width; i++ )
        {
            for (int j=0; j<tex.height; j++)
            {
                tex.SetPixel(i, j, Color.white);
            }
        }
        for (int i = 0; i < points.Count - 1; i++)
        {
            DrawLine(tex, i, (int)(points[i] * 50) + 50, i + 1, (int)(points[i + 1] * 50) + 50, color);
        }
    }
    // Draws a vertical line through x=point for every point in 'points'
    void DrawGraph(Texture2D tex, List<int> points, Color color, int style)
    {
        foreach (int val in points)
        {
            DrawLine(tex, val, 0, val, 100, color);
        }
    }
    //Plots each point specified in 'points' in 2D space and connects them
    void DrawGraph(Texture2D tex, List<Vector3> points, Color color, int style)
    {
        if (style == 3)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 newPoint1 = points[i];
                Vector3 newPoint2 = points[i + 1];
                DrawLine(tex, (int)newPoint1.x, (int)(newPoint1.y * 80) + 10, (int)newPoint2.x, (int)(newPoint2.y * 80) + 10, color);
            }
        }
        if (style == 4)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 newPoint1 = points[i];
                Vector3 newPoint2 = points[i + 1];
                DrawLine(tex, (int)newPoint1.x, (int)(newPoint1.y), (int)newPoint2.x, (int)(newPoint2.y), color);
            }
        }
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

    //Function to draw a line. Can be treated as a blackbox
    void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
    {
        int dy = (int)(y1 - y0);
        int dx = (int)(x1 - x0);
        int stepx, stepy;

        if (dy < 0) { dy = -dy; stepy = -1; }
        else { stepy = 1; }
        if (dx < 0) { dx = -dx; stepx = -1; }
        else { stepx = 1; }
        dy <<= 1;
        dx <<= 1;

        float fraction = 0;

        tex.SetPixel(x0, y0, col);
        if (dx > dy)
        {
            fraction = dy - (dx >> 1);
            while (Mathf.Abs(x0 - x1) > 1)
            {
                if (fraction >= 0)
                {
                    y0 += stepy;
                    fraction -= dx;
                }
                x0 += stepx;
                fraction += dy;
                tex.SetPixel(x0, y0, col);
            }
        }
        else
        {
            fraction = dx - (dy >> 1);
            while (Mathf.Abs(y0 - y1) > 1)
            {
                if (fraction >= 0)
                {
                    x0 += stepx;
                    fraction -= dy;
                }
                y0 += stepy;
                fraction += dx;
                tex.SetPixel(x0, y0, col);
            }
        }
    }

    void SaveTextureToFile(Texture2D tex, string fileName)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes("/sdcard/" + fileName + ".png", bytes );
        
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
        chartManager.SendMessage("UpdateAccLog", new Vector2(0, calcVel));

        float dir = compassNeedle.transform.rotation.eulerAngles.z*(Mathf.PI / 180.0f);
        float calcVelX = Mathf.Abs(Mathf.Sin(dir)*calcVel);
        float calcVelY = Mathf.Abs(Mathf.Cos(dir)*calcVel);

        StartCoroutine("ChangeHorizontalRulerScale", new Vector3(hRulerDefaultScale.x * Mathf.Sqrt(1 - Mathf.Pow(calcVelX / speedOfLight, 2)), hRulerDefaultScale.y, hRulerDefaultScale.z));
        StartCoroutine("ChangeVerticalRulerScale", new Vector3(vRulerDefaultScale.x, vRulerDefaultScale.y * Mathf.Sqrt(1 - Mathf.Pow(calcVelY / speedOfLight, 2)), vRulerDefaultScale.z));
	    currVel += avgAcc;

        compassNeedle.transform.Rotate(new Vector3(0, 0, Input.gyro.rotationRateUnbiased.z*Time.deltaTime*(180.0f/Mathf.PI)));
        rotationLog.Add(compassNeedle.transform.rotation.eulerAngles.z);
	}

    private IEnumerator ChangeHorizontalRulerScale(Vector3 newScale)
    {
        float t = 0.0f;

        Vector3 start = horizontalRuler.transform.localScale;
        Vector3 end = newScale;

        while (t < 1.0f)
        {
            //Scale over .2 seconds
            t += (Time.deltaTime / 0.2f);
            horizontalRuler.transform.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }

    private IEnumerator ChangeVerticalRulerScale(Vector3 newScale)
    {
        float t = 0.0f;

        Vector3 start = verticalRuler.transform.localScale;
        Vector3 end = newScale;

        while (t < 1.0f)
        {
            //Scale over .2 seconds
            t += (Time.deltaTime / 0.2f);
            verticalRuler.transform.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }
}
