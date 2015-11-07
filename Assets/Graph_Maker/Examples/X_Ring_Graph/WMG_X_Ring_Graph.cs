using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WMG_X_Ring_Graph : MonoBehaviour {

	public List<WMG_Ring_Graph> ringGraphs;

	public bool onlyRandomizeData;

	void Start() {
		for (int i = 0; i < ringGraphs.Count; i++) {
			ringGraphs[i].Refresh();
		}
	}

	public void randomize() {
		for (int i = 0; i < ringGraphs.Count; i++) {
			int numRings = ringGraphs[i].values.Count;
			if (!onlyRandomizeData) numRings = Random.Range(1,6);
			ringGraphs[i].values = ringGraphs[i].GenRandomList(numRings, ringGraphs[i].minValue, ringGraphs[i].maxValue);
			if (!onlyRandomizeData) {
				ringGraphs[i].bandMode = (1 == Random.Range(0,2));
				if (i == 3) {
					ringGraphs[i].degrees = Random.Range(0, 180);
					ringGraphs[i].topBotPadding.y = -ringGraphs[i].outerRadius * (1 - Mathf.Cos(ringGraphs[i].degrees/2 * Mathf.Deg2Rad)) + 50;
				}
			}
			ringGraphs[i].Refresh();
		}
	}

	public void dataOnlyChanged() {
		onlyRandomizeData = !onlyRandomizeData;
	}
	
}
