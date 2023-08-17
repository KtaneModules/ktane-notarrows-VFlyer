using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid2DFiller : MonoBehaviour {

	public int[] grid;
	public int squareLength = 10;
	// Use this for initialization
	void Start () {
		retryGen:
		var endingGrid = Enumerable.Repeat(-1, squareLength * squareLength).ToArray();
		var remainingPossibilities = new List<int>[squareLength * (squareLength - 1)];
		for (var x = 0; x < squareLength * squareLength - 1; x++)
			remainingPossibilities[x] = Enumerable.Range(0, squareLength).ToList();
		var firstRowSets = Enumerable.Range(0, squareLength).ToArray().Shuffle();
		for (var x = 0; x < squareLength; x++)
			endingGrid[x] = firstRowSets[x];
		for (var x = 0; x < remainingPossibilities.Length; x++)
        {

        }
	}
}
