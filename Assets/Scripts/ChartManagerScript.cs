using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChartManagerScript : MonoBehaviour {

    //public WMG_Axis_Graph lineGraph1;
    public WMG_Axis_Graph lineGraph2;
    //public WMG_Axis_Graph lineGraph3;
    public int accLogMemory;
    public int numberOfPoints;

    //private WMG_Series series1;
    //private WMG_Series series12;
    private WMG_Series series2;
    //private WMG_Series series3;
    //private WMG_Series series32;

    private List<Vector2> accLog;
    //private List<Vector2> velXLog;
    //private List<Vector2> velYLog;
    private List<Vector2> compactAccLog;
    //private List<Vector2> userAccXLog;
    //private List<Vector2> userAccYLog;

    private float accumulator;
    private int valuesPerPoint;

	// Use this for initialization
	void Awake () {
        //series1 = lineGraph1.lineSeries[0].GetComponent<WMG_Series>();
        //series12 = lineGraph1.lineSeries[1].GetComponent<WMG_Series>();
        series2 = lineGraph2.lineSeries[0].GetComponent<WMG_Series>();
        //series3 = lineGraph3.lineSeries[0].GetComponent<WMG_Series>();
        //series32 = lineGraph3.lineSeries[1].GetComponent<WMG_Series>();

        //series1.pointValues = new List<Vector2>();
        //series12.pointValues = new List<Vector2>(); 
        series2.pointValues = new List<Vector2>();
        //series3.pointValues = new List<Vector2>();
        //series32.pointValues = new List<Vector2>();

        //lineGraph1.yAxisMaxValue = 15;
        //lineGraph1.yAxisMinValue = 0;

        lineGraph2.yAxisMaxValue = 3;
        lineGraph2.yAxisMinValue = -3;

        //lineGraph3.yAxisMaxValue = 5;
        //lineGraph3.yAxisMinValue = -5;

        lineGraph2.axesType = WMG_Axis_Graph.axesTypes.I_IV;
        //lineGraph3.axesType = WMG_Axis_Graph.axesTypes.I_IV;

        accLog = new List <Vector2>();
        //velXLog = new List <Vector2>();
        //velYLog = new List<Vector2>();
        compactAccLog = new List<Vector2>();
        //userAccXLog = new List<Vector2>();
        //userAccYLog = new List<Vector2>();

        valuesPerPoint = accLogMemory / numberOfPoints;
	}
	
	// Update is called once per frame
	void Update () {
        //series1.pointValues = velXLog;
        //series12.pointValues = velYLog;
        compactAccLog.Clear();

        if (accLog.Count < accLogMemory)            //Ignore the graph calculations if we don't have enough calculated values yet
            return;

        for (int i = 0; i < numberOfPoints; i++ )
        {
            accumulator = 0;
            for (int j = 0; j < valuesPerPoint; j++)
            {
                accumulator += accLog[i * valuesPerPoint + j].y;
            }
            compactAccLog.Add(new Vector2(i, accumulator / valuesPerPoint));
        }
        series2.pointValues = compactAccLog;

        //series3.pointValues = userAccXLog;
        //series32.pointValues = userAccYLog;
	}

    void UpdateAccLog(Vector2 acc)
    {
        accLog.Add(acc);
        if (accLog.Count > accLogMemory)
        {
            accLog.RemoveAt(0);
        }
    }

    void NewVelXLog(List<Vector2> newVelLog)
    {
        //velXLog = newVelLog;
    }

    void NewVelYLog(List<Vector2> newVelLog)
    {
        //velYLog = newVelLog;
    }

    void NewUserAccXLog(List<Vector2> newUserAccLog)
    {
        //userAccXLog = newUserAccLog;
    }

    void NewUserAccYLog(List<Vector2> newUserAccLog)
    {
        //userAccYLog = newUserAccLog;
    }
}
