using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PosHistoryScript : MonoBehaviour {

	public LineRenderer lineRenderer;
	public Color c1 = Color.red;
	
	private int maxLengthOfLineRenderer;
	private GameObject background;
	private GameObject scale;
	
	private int lengthOfLineRenderer = 0;
	
	private Vector3 startPos;
	
	private float[] arr;

	// Use this for initialization
	void Start () {
		lineRenderer = gameObject.GetComponent<LineRenderer> ();
		lineRenderer.SetWidth(0.02F, 0.02F);
		//lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer.SetColors (c1, c1);
		background = GameObject.Find ("Pos History Background");
		scale = GameObject.Find ("Scale");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void DrawPositionHistory(List<Vector2> points)
	{
		maxLengthOfLineRenderer = points.Count;
		lineRenderer.SetVertexCount(maxLengthOfLineRenderer);	
		arr = new float[maxLengthOfLineRenderer];
		startPos = transform.position; 

		Vector2 trailMin = new Vector2 (0, 0);
		Vector2 trailMax = new Vector2 (0, 0);

		for (int i=0; i<points.Count; i++)
		{
			points[i] = Quaternion.Euler(0, 0, 90) * points[i];
			points[i] += new Vector2(250, -250);
			if (points[i].x < trailMin.x) trailMin.x = points[i].x;
			if (points[i].x > trailMax.x) trailMax.x = points[i].x;
			if (points[i].y < trailMin.y) trailMin.y = points[i].y;
			if (points[i].y > trailMax.y) trailMax.y = points[i].y;
		}

		float posHistoryScale = 250.0f/Mathf.Max(trailMax.x - trailMin.x+10, trailMax.y - trailMin.y+10);
		scale.GetComponent<PosHistoryScaleScript>().SendMessage("SetScale", background.transform.lossyScale.x/Mathf.Max(trailMax.x - trailMin.x+10, trailMax.y - trailMin.y+10));

		for (int i=0; i<points.Count; i++)
		{
			Vector3 newPos = startPos + new Vector3(points[i].x*background.transform.lossyScale.x*posHistoryScale/500, points[i].y*background.transform.lossyScale.y*posHistoryScale/500, -0.1f);
			lineRenderer.SetPosition(i, newPos);
		}
	}
}
