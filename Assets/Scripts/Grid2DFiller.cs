using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid2DFiller : MonoBehaviour {

	public string gridA, gridB;
	public bool stitchGrids;
	public int squareLength = 10;
	// Use this for initialization
	void Start () {
		if (!stitchGrids)
		{
			var resulingGrid = Enumerable.Range(0, squareLength * squareLength).ToArray();
			do
				resulingGrid.Shuffle();
			while (Enumerable.Range(0, squareLength).Any(a =>
			 resulingGrid.Skip(squareLength * a).Take(squareLength).Select(b => b % squareLength).Distinct().Count() < squareLength ||
			 resulingGrid.Skip(squareLength * a).Take(squareLength).Select(b => b / squareLength).Distinct().Count() < squareLength ||
			 Enumerable.Range(0, squareLength).Select(b => resulingGrid[squareLength * b + a]).Select(b => b / squareLength).Distinct().Count() < squareLength ||
			 Enumerable.Range(0, squareLength).Select(b => resulingGrid[squareLength * b + a]).Select(b => b % squareLength).Distinct().Count() < squareLength
			));
			Debug.Log(resulingGrid.Join(","));
		}
	}
}
