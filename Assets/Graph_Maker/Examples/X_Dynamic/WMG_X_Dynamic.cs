using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class WMG_X_Dynamic : MonoBehaviour {
	public GameObject graphPrefab;
	public WMG_Axis_Graph graph;
	public WMG_X_Data_Provider dataProvider;

    private int counter;

	public bool performTests;
	public bool noTestDelay;
	
	public float testInterval;
	public float testGroupInterval = 0.1f;
	float animDuration;

    List<Vector2> newSeries;

	WaitForSeconds waitTime;

	void Start() {
		GameObject graphGO = GameObject.Instantiate(graphPrefab) as GameObject;
		graph = graphGO.GetComponent<WMG_Axis_Graph>();
		graph.changeSpriteParent(graphGO, this.gameObject);
		graph.changeSpritePositionTo(graphGO, Vector3.zero);
		graph.graphTitleOffset = new Vector2(0, 60);
		graph.autoAnimationsDuration = testInterval - 0.1f;

        newSeries = new List<Vector2>();
        counter = 0;

		waitTime = new WaitForSeconds(testInterval);
		animDuration = testInterval - 0.1f; // have animations slightly faster than the test interval
		if (animDuration < 0) animDuration = 0;

		WMG_Data_Source dataSource = graph.lineSeries[0].GetComponent<WMG_Series>().pointValuesDataSource;
		if (dataSource != null) {
			dataSource.setDataProvider(dataProvider);
		}
        
		if (performTests) {
			StartCoroutine(startTests());
		}
        //gameObject.
	}

    IEnumerator gridsTicksTests()
    {
        List<string> xLabels = new List<string>(graph.xAxisLabels);

        WMG_Anim.animInt(() => graph.yAxisNumTicks, x => graph.yAxisNumTicks = x, animDuration, 11);

        if (!noTestDelay) yield return waitTime;
        graph.xLabelType = WMG_Axis_Graph.labelTypes.ticks;
        graph.SetXLabelsUsingMaxMin = true;
        WMG_Anim.animInt(() => graph.xAxisNumTicks, x => graph.xAxisNumTicks = x, animDuration, 11);

        if (!noTestDelay) yield return waitTime;
        //graph.xLabelType = WMG_Axis_Graph.labelTypes.ticks_center;
        //graph.xAxisLabels = xLabels;
    }

	IEnumerator startTests() {
		yield return new WaitForSeconds(testGroupInterval);

        graph.axesType = WMG_Axis_Graph.axesTypes.I_IV;
		// add delete tests
		graph.graphTitleString = "User Acceleration";


       

        StartCoroutine(gridsTicksTests());
		StartCoroutine(addDeleteTests());
		if (!noTestDelay) yield return new WaitForSeconds(testInterval * 11);
		yield return new WaitForSeconds(testGroupInterval);

		graph.graphTitleString = "";
	}
	IEnumerator addDeleteTests() {

        graph.deleteSeries();
        graph.deleteSeries();
		addSeriesWithNewData();

        int count = 0;
        while (count < 10)
        {
            if (!noTestDelay) yield return waitTime;
            count++;

            graph.deleteSeries();
            addSeriesWithNewData();
        }
        yield return null;
		
	}

	void addSeriesWithNewData() {
		WMG_Series series = graph.addSeries();
		series.UseXDistBetweenToSpace = true;
		series.AutoUpdateXDistBetween = true;
		series.lineScale = 0.5f;
        series.pointColor = Color.blue;
		series.seriesName = "Series " + graph.lineSeries.Count;
        
        series.pointValues = newSeries;//graph.GenRandomY(graph.groups.Count, 1, graph.groups.Count, graph.yAxisMinValue, graph.yAxisMaxValue);
	}

    void addNewData(Vector2 data)
    {
        newSeries.Add(data);
        float max = 0;
        foreach (Vector2 val in newSeries)
        {
            if (max < Mathf.Abs(val.y))
                max = Mathf.Abs(val.y);
        }

        graph.yAxisMaxValue = max;
        graph.yAxisMinValue = -max;

        graph.xAxisMaxValue = newSeries.Count;
        //if (newSeries.Count > 100)
        //    newSeries.RemoveAt(0);
    }

    void removeData()
    {
        newSeries.Clear();
    }

}
