using UnityEngine;
using System.Collections;

public class PosHistoryScaleScript : MonoBehaviour {

	LineRenderer lineRenderer;
	GameObject background;
	Color c1 = Color.red;

	Vector3 startPos;
	Vector3 endPos;

	void Start () {
		lineRenderer = gameObject.GetComponent<LineRenderer> ();
		lineRenderer.SetWidth(0.1F, 0.1F);
		//lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer.SetColors (c1, c1);
		background = GameObject.Find ("Pos History Background");
		lineRenderer.SetVertexCount (2);
		startPos = background.transform.position + new Vector3 (background.transform.lossyScale.x*0.6f, -background.transform.lossyScale.y*0.5f, 0);
		endPos = startPos + new Vector3 (0, 1, 0);

		lineRenderer.SetPosition (0, startPos);
		lineRenderer.SetPosition (1, endPos);

	}
	
	public void SetScale(float scale)
	{
		endPos = startPos + new Vector3 (0, scale*10, 0);
		lineRenderer.SetPosition (0, startPos);
		lineRenderer.SetPosition (1, endPos);
	}
}
