using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SimulatorScript : MonoBehaviour {

    public GameObject turtle;

    private Vector2 simulationTurtlePos;
    private Vector2 simulationTurtleDir;
    private float simulationTurtleSpeed;
    private List<Vector3> simulationTurtlePosLog;
    private float turtleZPos;
    private int simulationEnd;

	// Use this for initialization
	void Start () {

        turtle = GameObject.Find("Turtle");
        turtleZPos = turtle.transform.position.z;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void initializeSimulation()
    {
        simulationTurtlePosLog = new List<Vector3>();
        simulationTurtlePos = new Vector2(500 / 2, 500 / 2);
        simulationTurtleDir = new Vector2(0, 1);
        simulationTurtleSpeed = 0;

        simulationTurtlePosLog.Add(new Vector3(simulationTurtlePos.x, simulationTurtlePos.y, 0));
    }

    void simulationStep(float delta)
    {
        simulationTurtlePos += simulationTurtleDir * simulationTurtleSpeed * delta;
        simulationTurtlePosLog.Add(new Vector3(simulationTurtlePos.x, simulationTurtlePos.y, 0));
    }

    void simulationChangeDir(float dir)
    {
        simulationTurtleDir = new Vector2(Mathf.Cos(dir * (Mathf.PI / 180.0f)), Mathf.Sin(dir * (Mathf.PI / 180.0f)));
    }

    void simulationSetSpeed(float speed)
    {
        simulationTurtleSpeed = speed;
    }

    IEnumerator simulate()
    {
        simulationEnd = simulationTurtlePosLog.Count;
        turtle.transform.position = new Vector3(0, 0, turtle.transform.position.z);
        for (int i=0; i<simulationEnd-1; i++)
        {
            Vector3 newPos = new Vector3(simulationTurtlePosLog[i].x-250, simulationTurtlePosLog[i].y-250, 0);
            newPos /= 20;
            newPos += new Vector3(0f, 0f, turtleZPos);
            newPos = Quaternion.Euler(0, 0, 90) * newPos;
            turtle.transform.position = newPos;

            //for (float j=0; j<1; j+= 0.2f)
            //{
            //    Debug.Log(turtle.transform.position);
            //    turtle.transform.position = Vector3.Lerp(initPos, finalPos, j);
            //    yield return new WaitForSeconds(0.1f);
            //}
            yield return new WaitForSeconds(0.01f);
        }
    }

    void simulationDraw()
    {
        Texture2D simulationTex = new Texture2D(500, 500);
        for (int i = 0; i < simulationTex.width; i++)
        {
            for (int j = 0; j < simulationTex.height; j++)
            {
                simulationTex.SetPixel(i, j, Color.white);
            }
        }
        DrawGraph(simulationTex, simulationTurtlePosLog, Color.black, 4);
        simulationTex.Apply();
        SaveTextureToFile(simulationTex, "PositionHistory");
		GenerateLog ();
        StartCoroutine(simulate());
    }

	void GenerateLog ()
	{
		WriteLog (simulationTurtlePosLog, "positionHistoryLog");
	}

	void WriteLog(List<Vector3> log, string fileName)
	{
		string text = "";
		foreach (Vector3 val in log)
		{
			text += val.x + ",";
		}
		text += "\r\n";
		foreach (Vector3 val in log)
		{
			text += val.y + ",";
		}
		text += "\r\n";
		StreamWriter sw = System.IO.File.CreateText("/sdcard/" + fileName + ".csv");
		sw.Close();
		System.IO.File.WriteAllText("/sdcard/" + fileName + ".csv", text);
	}

    //TODO:Redundant functions redefined from BaseScript. Consider a better implementation
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
        File.WriteAllBytes("/sdcard/" + fileName + ".png", bytes);

    }
}
