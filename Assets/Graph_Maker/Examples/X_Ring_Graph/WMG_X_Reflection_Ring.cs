using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WMG_X_Reflection_Ring : MonoBehaviour {

	public WMG_Ring_Graph graph;
	public List<WMG_X_Data_Provider> dataProviders;
	public bool highlightRandomRing;
	public bool clearHighlights;

	void Start() {
		graph.valuesDataSource.setDataProviders(dataProviders);
		graph.labelsDataSource.setDataProviders(dataProviders);
		graph.ringIDsDataSource.setDataProviders(dataProviders);
		foreach (WMG_X_Data_Provider dp in dataProviders) {
			dp.TestProperty = Random.Range(graph.minValue, graph.maxValue);
			dp.TestPropertyWithFields = new Vector2(Random.Range(graph.minValue, graph.maxValue),Random.Range(graph.minValue, graph.maxValue));
		}
	}

	void Update() {
		if (highlightRandomRing) {
			highlightRandomRing = false;
			graph.HighlightRing(dataProviders[Random.Range(0, dataProviders.Count)].idField);
		}
		if (clearHighlights) {
			clearHighlights = false;
			graph.RemoveHighlights();
		}
	}
}
