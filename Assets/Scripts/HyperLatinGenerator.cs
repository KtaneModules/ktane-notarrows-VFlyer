using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HyperLatinGenerator : MonoBehaviour {

	public string firstGrid, prefilledSecond;

	// Use this for initialization
	void Start () {
		StartCoroutine(HandleGenerateGraceoLatin());
	}

	IEnumerator HandleGenerateGraceoLatin()
	{
		if (firstGrid.Length != 100) yield break;
		var allPossiblities = new List<int>[100];
	retryGen:
		for (var x = 0; x < 100; x++)
		{
			allPossiblities[x] = Enumerable.Range(0, 10).ToList();
			allPossiblities[x].Shuffle();
		}
		var priorityIdxPlaces = Enumerable.Range(0, 100).ToArray().Shuffle();
		if (prefilledSecond == null || prefilledSecond.Length != 100)
		{
			var firstComboSets = Enumerable.Range(0, 10).ToArray().Shuffle();
			var idxesMatchFirstIdx = Enumerable.Range(0, 100).Where(a => firstGrid[a] == firstGrid[priorityIdxPlaces.First()]).ToArray().Shuffle();
			for (int i = 0; i < idxesMatchFirstIdx.Length; i++)
			{
				int idx = idxesMatchFirstIdx[i];
				allPossiblities[idx].RemoveAll(a => firstComboSets[i] != a);
			}
		}
		else
        {
			for (var x = 0; x < 100; x++)
			{
				int curVal;
				if (int.TryParse(prefilledSecond[x].ToString(), out curVal))
					allPossiblities[x].RemoveAll(a => a != curVal);
			}
		}
		do
		{
			var newAllPossibilities = allPossiblities.Select(a => a.ToList()).ToArray();
			for (var x = 0; x < 100; x++)
			{
				var curX = x % 10;
				var curY = x / 10;
				if (allPossiblities[x].Count == 1)
				{
					
					var remainingVal = allPossiblities[x].Single();
					foreach (var idxNotX in Enumerable.Range(0, 10).Where(a => a != curX))
						newAllPossibilities[curX + 10 * idxNotX].Remove(remainingVal);
					foreach (var idxNotY in Enumerable.Range(0, 10).Where(a => a != curY))
						newAllPossibilities[idxNotY + 10 * curY].Remove(remainingVal);
					foreach (var idxMatchFirstDigit in Enumerable.Range(0, 100).Where(a => a != x && firstGrid[x] == firstGrid[a]))
						newAllPossibilities[idxMatchFirstDigit].Remove(remainingVal);
				}
			}
			if (Enumerable.Range(0, 100).All(a => newAllPossibilities[a].SequenceEqual(allPossiblities[a])))
			{
				var nextPriorityIdx = priorityIdxPlaces.First(a => allPossiblities[a].Count > 1);
				var selectedIdxFilter = allPossiblities[nextPriorityIdx].PickRandom();
				allPossiblities[nextPriorityIdx].RemoveAll(a => a != selectedIdxFilter);
			}
			else
				allPossiblities = newAllPossibilities;
			yield return null;
		}
		while (allPossiblities.Any(a => a.Count > 1) && !allPossiblities.Any(a => a.Count <= 0));
		if (allPossiblities.Any(a => a.Count == 0)) goto retryGen;
		Debug.Log(Enumerable.Range(0, 100).Select(a => string.Format("{0}{1}", firstGrid[a], allPossiblities[a].Single())).Join());

	}

}
