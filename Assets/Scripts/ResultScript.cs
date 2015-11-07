using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;

public class ResultScript : MonoBehaviour {

    private List<Vector2> velXLog;
    private List<Vector2> velYLog;
	private List<Vector2> velLog;
    private List<Vector2> usrAccXLog;
    private List<Vector2> usrAccYLog;
	private List<Vector2> posHistoryLog;
	private List<Vector2> timeLog;
    private GameObject chartManager;
	private GameObject regularClock;
	private GameObject warpedClock;
	private GameObject posHistoryObject;

	private float regClockSecs;
	private float regClockMilliSecs;
	private float warpClockSecs;
	private float warpClockMilliSecs;

	// Use this for initialization
	void Start () {
        velXLog = new List<Vector2>();
        velYLog = new List<Vector2>();
		velLog = new List<Vector2> ();
        usrAccXLog = new List<Vector2>();
        usrAccYLog = new List<Vector2>();
		posHistoryLog = new List<Vector2>();
		timeLog = new List<Vector2> ();
        chartManager = GameObject.Find("Chart Manager");
		regularClock = GameObject.Find ("Regular Clock");
		warpedClock = GameObject.Find("Warped Clock");
		posHistoryObject = GameObject.Find ("Position History");
        FileExistanceChecks();
        ReadVals();
        DisplayGraph();
	}

    void FileExistanceChecks()
    {
        if (File.Exists("/sdcard/velXLog.csv") == false)
            Debug.Log("Velocity X Log not found!");
        if (File.Exists("/sdcard/velYLog.csv") == false)
            Debug.Log("Velocity Y Log not found!");
        if (File.Exists("/sdcard/accXLog.csv") == false)
            Debug.Log("User Acceleration X Log not found!");
        if (File.Exists("/sdcard/accYLog.csv") == false)
            Debug.Log("User Acceleration Y Log not found!");
		if (File.Exists ("/sdcard/clockTimes.txt") == false)
			Debug.Log ("Clock times not found!");
		if (File.Exists("/sdcard/positionHistoryLog.csv") == false)
			Debug.Log("Position History Log not found!");
    }

    void ReadVals()
    {
        string[] lines = System.IO.File.ReadAllLines(@"/sdcard/velXLog.csv");
        string[] xVals = lines[0].Split(',');
        string[] yVals = lines[1].Split(',');
        //Debug.Assert(xVals.Length == yVals.Length, "The Vel X log seems to have an error");
        for (int i=0; i<xVals.Length; i++)
        {
            if (xVals[i].Length == 0 || yVals[i].Length == 0) continue;     //Don't copy over empty strings to the array
            Vector2 vel = new Vector2(Convert.ToSingle(xVals[i]), Convert.ToSingle(yVals[i]));
            velXLog.Add(vel);
        }

        lines = System.IO.File.ReadAllLines(@"/sdcard/velYLog.csv");
        xVals = lines[0].Split(',');
        yVals = lines[1].Split(',');
        //Debug.Assert(xVals.Length == yVals.Length, "The Vel Y log seems to have an error");
        for (int i = 0; i < xVals.Length; i++)
        {
            if (xVals[i].Length == 0 || yVals[i].Length == 0) continue;     //Don't copy over empty strings to the array
            Vector2 vel = new Vector2(Convert.ToSingle(xVals[i]), Convert.ToSingle(yVals[i]));
            velYLog.Add(vel);
        }

		Debug.Log ("Starting Vel Calcs");
		Debug.Log (velXLog.Count);
        Debug.Log(velYLog.Count);
        //TODO: Figure out why velXLog is bigger than velYLog!
        //This is seriously weird
        int smallerLog;
        if (velXLog.Count < velYLog.Count) smallerLog = velXLog.Count;
        else smallerLog = velYLog.Count;
        for (int i=0; i<smallerLog; i++)
        {
            Vector2 vel = new Vector2(velXLog[i].x, Mathf.Sqrt(velXLog[i].y*velXLog[i].y + velYLog[i].y*velYLog[i].y));
            velLog.Add(vel);
        }
		Debug.Log ("Vel Calcs Completed");

        lines = System.IO.File.ReadAllLines(@"/sdcard/accXLog.csv");
        xVals = lines[0].Split(',');
        yVals = lines[1].Split(',');
        //Debug.Assert(xVals.Length == yVals.Length, "The User Acc X log seems to have an error");
        for (int i = 0; i < xVals.Length; i++)
        {
            if (xVals[i].Length == 0 || yVals[i].Length == 0) continue;     //Don't copy over empty strings to the array
            Vector2 acc = new Vector2(Convert.ToSingle(xVals[i]), Convert.ToSingle(yVals[i]));
            usrAccXLog.Add(acc);
        }

        lines = System.IO.File.ReadAllLines(@"/sdcard/accYLog.csv");
        xVals = lines[0].Split(',');
        yVals = lines[1].Split(',');
        //Debug.Assert(xVals.Length == yVals.Length, "The User Acc Y log seems to have an error");
        for (int i = 0; i < xVals.Length; i++)
        {
            if (xVals[i].Length == 0 || yVals[i].Length == 0) continue;     //Don't copy over empty strings to the array
            Vector2 acc = new Vector2(Convert.ToSingle(xVals[i]), Convert.ToSingle(yVals[i]));
            usrAccYLog.Add(acc);
		}

		lines = System.IO.File.ReadAllLines(@"/sdcard/clockTimes.txt");
		float time1 = Convert.ToSingle(lines[0]);
		float time2 = Convert.ToSingle(lines[1]);
		regularClock.SendMessage ("ChangeTime", time1);
		warpedClock.SendMessage ("ChangeTime", time2);

		lines = System.IO.File.ReadAllLines(@"/sdcard/positionHistoryLog.csv");
		xVals = lines[0].Split(',');
		yVals = lines[1].Split(',');
		for (int i = 0; i < xVals.Length; i++)
		{
			if (xVals[i].Length == 0 || yVals[i].Length == 0) continue;     //Don't copy over empty strings to the array
			Vector2 pos = new Vector2(Convert.ToSingle(xVals[i]), Convert.ToSingle(yVals[i]));
			posHistoryLog.Add(pos);
		}

		lines = System.IO.File.ReadAllLines(@"/sdcard/WarpedClockTimes.csv");
		xVals = lines[0].Split (',');
		for (int i = 0; i < xVals.Length; i++)
		{
			if (xVals[i].Length == 0) continue;     //Don't copy over empty strings to the array
			Vector2 time = new Vector2(i*0.02f, Convert.ToSingle(xVals[i]));
			timeLog.Add (time);
		}

		posHistoryObject.SendMessage ("DrawPositionHistory", posHistoryLog);
	
    }

    void DisplayGraph()
    {
        chartManager.SendMessage("NewVelXLog", velXLog);
        chartManager.SendMessage("NewVelYLog", velYLog);
		chartManager.SendMessage ("NewVelLog", velLog);
		chartManager.SendMessage("NewUserAccXLog", usrAccXLog);
        chartManager.SendMessage("NewUserAccYLog", usrAccYLog);
		chartManager.SendMessage ("NewTimeLog", timeLog);
    }

	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.LoadLevel(1);
		}
	}
}
