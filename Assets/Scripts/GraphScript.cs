using UnityEngine;
using System.Collections;

public class GraphScript : MonoBehaviour {

	public LineRenderer lineRenderer;
	public Color c1 = Color.yellow;

	public int maxLengthOfLineRenderer = 200;

	private int lengthOfLineRenderer = 0;

	private Vector3 startPos;

	private float[] arr;

	// Use this for initialization
	void Start () {
		lineRenderer = gameObject.GetComponent<LineRenderer> ();
		//lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		//lineRenderer.SetColors(c1, c1);
		lineRenderer.SetWidth(0.02F, 0.02F);
		lineRenderer.SetVertexCount(maxLengthOfLineRenderer);

		arr = new float[maxLengthOfLineRenderer];
		startPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		int i = 0;
		while (i < lengthOfLineRenderer) {
			Vector3 pos = new Vector3(startPos.x +i * 0.05F*transform.parent.localScale.x, startPos.y+arr[i]*transform.parent.localScale.y, startPos.z);
			lineRenderer.SetPosition(i, pos);
			i++;
		}
	}

	public void AddArrVal(float a)
	{
		if (lengthOfLineRenderer < maxLengthOfLineRenderer)
		{
			arr[lengthOfLineRenderer] = a;
			lengthOfLineRenderer++;
		}
		else
		{
			for (int i=0; i<maxLengthOfLineRenderer-1; i++)
			{
				arr[i] = arr[i+1];
			}
			arr[maxLengthOfLineRenderer-1] = a;
		}
	}
}
