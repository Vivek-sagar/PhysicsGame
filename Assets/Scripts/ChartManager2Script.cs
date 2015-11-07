using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChartManager2Script : MonoBehaviour
{

    public WMG_Axis_Graph lineGraph1;
	public WMG_Axis_Graph lineGraph2;
    public WMG_Axis_Graph lineGraph3;
	public WMG_Axis_Graph lineGraph4;

    public WMG_Series series1;
    public WMG_Series series12;
	public WMG_Series series2;
    public WMG_Series series3;
    public WMG_Series series32;
	public WMG_Series series4;
	public WMG_Series series42;
	
    private List<Vector2> velXLog;
    private List<Vector2> velYLog;
	private List<Vector2> velLog;
    private List<Vector2> userAccXLog;
    private List<Vector2> userAccYLog;
	private List<Vector2> warpedTimeLog;
	private List<Vector2> realTimeLog;

    private List<Vector2> temp;
    private int tempCount;

    // Awake instead of Start to ensure that initialization is done before Remote function calls are made
    void Awake()
    {
        series1 = lineGraph1.lineSeries[0].GetComponent<WMG_Series>();
        series12 = lineGraph1.lineSeries[1].GetComponent<WMG_Series>();
		series2 = lineGraph2.lineSeries[0].GetComponent<WMG_Series>();
        series3 = lineGraph3.lineSeries[0].GetComponent<WMG_Series>();
        series32 = lineGraph3.lineSeries[1].GetComponent<WMG_Series>();
		series4 = lineGraph4.lineSeries[0].GetComponent<WMG_Series>();
		series42 = lineGraph4.lineSeries[1].GetComponent<WMG_Series>();

        series1.pointValues = new List<Vector2>();
        series12.pointValues = new List<Vector2>(); 
		series2.pointValues = new List<Vector2> ();
        series3.pointValues = new List<Vector2>();
        series32.pointValues = new List<Vector2>();
		series4.pointValues = new List<Vector2> ();
		series42.pointValues = new List<Vector2> ();

        lineGraph1.yAxisMaxValue = 15;
        lineGraph1.yAxisMinValue = -15;
		lineGraph2.yAxisMaxValue = 15;
		lineGraph2.yAxisMinValue = -15;
        lineGraph3.yAxisMaxValue = 5;
        lineGraph3.yAxisMinValue = -5;
		lineGraph4.yAxisMaxValue = 15;
		lineGraph4.yAxisMinValue = -15;

		lineGraph1.axesType = WMG_Axis_Graph.axesTypes.I_IV;
        lineGraph2.axesType = WMG_Axis_Graph.axesTypes.I_IV;
		lineGraph3.axesType = WMG_Axis_Graph.axesTypes.I_IV;
		lineGraph4.axesType = WMG_Axis_Graph.axesTypes.I_IV;

        velXLog = new List <Vector2>();
        velYLog = new List<Vector2>();
		velLog = new List<Vector2> ();
        userAccXLog = new List<Vector2>();
        userAccYLog = new List<Vector2>();
		warpedTimeLog = new List<Vector2> ();
		realTimeLog = new List<Vector2> ();
    }

    // Update is called once per frame
    void Update()
    {
        series1.pointValues = velXLog;
        series12.pointValues = velYLog;
		series2.pointValues = velLog;
        series3.pointValues = userAccXLog;
        series32.pointValues = userAccYLog;
		series4.pointValues = warpedTimeLog;
		series42.pointValues = realTimeLog;
    }

    void NewVelXLog(List<Vector2> newVelLog)
    {
        velXLog = newVelLog;
    }

    void NewVelYLog(List<Vector2> newVelLog)
    {
        velYLog = newVelLog;
    }

    void NewUserAccXLog(List<Vector2> newUserAccLog)
    {
        userAccXLog = newUserAccLog;
    }

    void NewUserAccYLog(List<Vector2> newUserAccLog)
    {
        userAccYLog = newUserAccLog;
    }

	void NewVelLog(List<Vector2> newVelLog)
	{
		velLog = newVelLog;
	}

	void NewTimeLog(List<Vector2> newTimeLog)
	{
		if (newTimeLog.Count > 100) {
			int multiple = newTimeLog.Count / 100;
			for (int i=multiple; i<newTimeLog.Count; i+=multiple) {
				float acc = 0;
				for (int j = 0; j< multiple; j++) {
					acc += newTimeLog [i - j].y;
				}
				acc /= multiple;
				warpedTimeLog.Add (new Vector2 (newTimeLog [i].x, acc));
				Debug.Log (newTimeLog[i].x);
				realTimeLog.Add(new Vector2(newTimeLog[i].x, newTimeLog[i].x)); 
			}
		} else
		{
			warpedTimeLog = newTimeLog;
			for (int i=0; i<newTimeLog.Count; i++)
				realTimeLog.Add (new Vector2(i, i));
		}
	}
}
