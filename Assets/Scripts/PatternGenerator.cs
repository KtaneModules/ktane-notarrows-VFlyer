using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternGenerator : MonoBehaviour {
	public GameObject baseObject;
	public int amountToDuplicate = 26;
	public GameObject[] storedObjects;
	public Vector3 offsetPerDupe;
	// Use this for initialization
	void Start () {
		DuplicateBaseObjectXTimes();
	}
	public void DuplicateBaseObjectXTimes()
    {
		storedObjects = new GameObject[amountToDuplicate + 1];
		for (var p = 0; p < amountToDuplicate; p++)
		{
			var nextObject = Instantiate(baseObject, transform, false);
			storedObjects[p] = nextObject;
			baseObject.transform.localPosition += offsetPerDupe;
		}
		storedObjects[amountToDuplicate] = baseObject;
    }
}
