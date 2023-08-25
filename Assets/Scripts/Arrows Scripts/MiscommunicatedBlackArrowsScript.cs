using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MiscommunicatedBlackArrowsScript : BaseArrowsScript {

	enum DisConType
    {
		Invalid = -1,
		ARowBCol,
		AColBRow,
		ValueAB,
		ValueBA,
		BFirstARow,
		BLastARow,
		AFirstBRow,
		ALastBRow,
		BFirstACol,
		BLastACol,
		AFirstBCol,
		ALastBCol,

    }
	readonly static string[] arrowDirectionNames = new[] { "Up", "Right", "Down", "Left", };
	readonly static int[][] grid = new int[][] {

		new[] { 30, 68, 03, 59, 95, 42, 21, 14, 86, 77, },
		new[] { 75, 04, 69, 96, 87, 53, 32, 25, 11, 40, },
		new[] { 23, 12, 71, 67, 56, 98, 89, 00, 45, 34, },
		new[] { 57, 46, 35, 24, 13, 09, 90, 88, 72, 61, },
		new[] { 41, 37, 26, 15, 74, 80, 08, 99, 63, 52, },
		new[] { 05, 79, 97, 81, 22, 64, 43, 36, 50, 18, },
		new[] { 19, 91, 82, 33, 60, 75, 54, 47, 28, 06, },
		new[] { 92, 83, 44, 70, 38, 16, 65, 51, 07, 29, },
		new[] { 84, 55, 10, 48, 01, 27, 76, 62, 39, 93, },
		new[] { 66, 20, 58, 02, 49, 31, 17, 73, 94, 85, },
		};
	public KMBombInfo bombInfo;
	public MeshRenderer[] arrowRenderers;
	DisConType[] numReadTypes = new DisConType[] {
		DisConType.ARowBCol, DisConType.ValueAB,
		DisConType.BLastACol, DisConType.AColBRow,
		DisConType.ValueBA, DisConType.BFirstARow };

	int escapeTileIdx, countedCoordIdxes, curCoordIdx, radarCoordIdx, curOptimalDirIdx;
	List<int> trapTileIdxes, recommendRadarIdxes;
	Dictionary<int, int[]> distanceAllNavigatableTiles = new Dictionary<int, int[]>();

	static int modIDCnt = 0;
	protected override void QuickLogFormat(string toLog = "", params object[] misc)
	{
		QuickLog(string.Format(toLog, misc));
	}
	protected override void QuickLog(string toLog = "")
	{
		Debug.LogFormat("[Miscommunicated Black Arrows #{0}] {1}", moduleId, toLog);
	}
	protected override void QuickLogDebugFormat(string toLog = "", params object[] misc)
	{
		QuickLogDebug(string.Format(toLog, misc));
	}
	protected override void QuickLogDebug(string toLog = "")
	{
		Debug.LogFormat("<Miscommunicated Black Arrows #{0}> {1}", moduleId, toLog);
	}
	string QuickCoord(int idx)
    {
		return string.Format("{0}{1}", "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[idx % 10], idx / 10 + 1);
    }
	// Use this for initialization
	void Start () {
		try
		{
			colorblindActive = Colorblind.ColorblindModeActive;
		}
		catch
		{
			colorblindActive = false;
		}
		HandleColorblindToggle();
		moduleId = ++modIDCnt;
		trapTileIdxes = new List<int>();
		ResetModule();

        for (var x = 0; x < arrowButtons.Length; x++)
        {
			var y = x;
			arrowButtons[x].OnInteract += delegate {
				if (!(isanimating || moduleSolved))
                {
					arrowButtons[y].AddInteractionPunch(0.25f);
					MAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, arrowButtons[y].transform);
					HandleArrowPress(y);
                }
				return false;
			};
        }
		modSelf.OnActivate += delegate { HandleRadar(); };
	}
	void HandleArrowPress(int idx)
    {
		var curRow = curCoordIdx / 10;
		var curCol = curCoordIdx % 10;
		switch(idx)
        {
			case 0:
				curRow = (curRow + 9) % 10;
				break;
			case 1:
				curCol = (curCol + 1) % 10;
				break;
			case 2:
				curRow = (curRow + 1) % 10;
				break;
			case 3:
				curCol = (curCol + 9) % 10;
				break;
        }
		curCoordIdx = curRow * 10 + curCol;
		QuickLogFormat("Pressed {0} to move to {1}...", arrowDirectionNames[idx], QuickCoord(curCoordIdx));
		var requireRecalc = false;
		var requireFullRecheck = false;
		if (trapTileIdxes.Contains(curCoordIdx))
        {
			MAudio.PlaySoundAtTransform("CyanArrowsFall", transform);
			QuickLogFormat("{0} is trapped! Resetting.", QuickCoord(curCoordIdx));
			modSelf.HandleStrike();
			StartCoroutine(AnimateReset(idx));
        }
		else if (escapeTileIdx == curCoordIdx)
        {
			QuickLog("You have escaped. For now.");
			moduleSolved = true;
			StartCoroutine(victory());
        }
		else if (curOptimalDirIdx != idx)
        {
			QuickLog("Deviated from the recommended press. At least you are safe but some recalculations are needed.");
			requireRecalc = true;
			requireFullRecheck = true;
		}
		else
			requireRecalc = AreIdxesDiagonal(radarCoordIdx, curCoordIdx);
		if (requireRecalc)
			HandleRadar(requireFullRecheck);
    }

	int ConvertToDisplay(int idxPos, DisConType convertType = DisConType.ARowBCol)
    {
		var idxRow = idxPos / 10;
		var idxCol = idxPos % 10;
		var valueOnTable = grid[idxRow][idxCol];
		// Normal: AB = idxPos.
		switch (convertType)
        {
			case DisConType.ARowBCol:
				return idxPos;
			case DisConType.AColBRow:
				return idxRow + idxCol * 10;
			case DisConType.AFirstBRow:
				return idxRow + 10 * (valueOnTable / 10);
			case DisConType.ALastBRow:
				return idxRow + 10 * (valueOnTable % 10);
			case DisConType.ValueAB:
				return valueOnTable;
			case DisConType.ValueBA:
				return valueOnTable / 10 + 10 * (valueOnTable % 10);
			case DisConType.BFirstARow:
				return idxRow * 10 + valueOnTable / 10;
			case DisConType.BLastARow:
				return idxRow * 10 + valueOnTable % 10;
			case DisConType.BFirstACol:
				return idxCol * 10 + valueOnTable / 10;
			case DisConType.BLastACol:
				return idxCol * 10 + valueOnTable % 10;
		}
		return idxPos;
    }
	int ConvertToIdxPos(int display, DisConType convertType = DisConType.ARowBCol)
    {
		var firstDigit = display / 10;
		var secondDigit = display % 10;

		// Normal: AB = idxPos
		switch (convertType)
        {
			case DisConType.ARowBCol:
				return display;
			case DisConType.AColBRow:
				return firstDigit + secondDigit * 10;
			case DisConType.ValueAB:
                {
					var idxRowAB = Enumerable.Range(0, 10).Single(a => grid[a].Contains(display));
					var idxColAB = Array.IndexOf(grid[idxRowAB], display);
					return 10 * idxRowAB + idxColAB;
                }
			case DisConType.ValueBA:
                {
					var BAVal = 10 * secondDigit + firstDigit;
					var idxRowBA = Enumerable.Range(0, 10).Single(a => grid[a].Contains(BAVal));
					var idxColBA = grid[idxRowBA].ToList().IndexOf(BAVal);
					return 10 * idxRowBA + idxColBA;
                }
			case DisConType.AFirstBRow:
                {
					var curRow1stDigits = grid[secondDigit].Select(a => a / 10).ToList();
					return secondDigit + curRow1stDigits.IndexOf(firstDigit) * 10;
                }
			case DisConType.ALastBRow:
                {
					var curRowLstDigits = grid[secondDigit].Select(a => a % 10).ToList();
					return secondDigit + curRowLstDigits.IndexOf(firstDigit) * 10;
                }
			case DisConType.BFirstARow:
                {
					var curRow1stDigits = grid[firstDigit].Select(a => a / 10).ToList();
					return 10 * firstDigit + curRow1stDigits.IndexOf(secondDigit);
				}
			case DisConType.BLastARow:
                {
					var curRowLstDigits = grid[firstDigit].Select(a => a % 10).ToList();
					return 10 * firstDigit + curRowLstDigits.IndexOf(secondDigit);
				}
			case DisConType.BLastACol:
                {
					var curColLstDigits = grid.Select(a => a[firstDigit] % 10).ToList();
					return firstDigit + 10 * curColLstDigits.IndexOf(secondDigit);
				}
			case DisConType.BFirstACol:
                {
					var curCol1stDigits = grid.Select(a => a[firstDigit] / 10).ToList();
					return firstDigit + 10 * curCol1stDigits.IndexOf(secondDigit);
				}
			case DisConType.AFirstBCol:
                {
					var curCol1stDigits = grid.Select(a => a[secondDigit] / 10).ToList();
					return secondDigit + 10 * curCol1stDigits.IndexOf(firstDigit);
				}
			case DisConType.ALastBCol:
                {
					var curColLstDigits = grid.Select(a => a[secondDigit] % 10).ToList();
					return secondDigit + 10 * curColLstDigits.IndexOf(firstDigit);
				}
		}
		return display;
    }
	IEnumerator AnimateReset(int idx)
    {
		var lastColor = textDisplay.color;
		isanimating = true;
		for (float x = 0; x < 1f; x += Time.deltaTime / 3f)
        {
			arrowRenderers[idx].material.color = Color.red * (1f - x) + Color.black * x;
			textDisplay.color = lastColor * (1f - x);
			yield return null;
        }
		textDisplay.text = "";
		HandleColorblindToggle();
		for (var x = 0; x < arrowRenderers.Length; x++)
			arrowRenderers[idx].material.color = Color.black;
		ResetModule();
		HandleRadar();
	}

	IEnumerator ReplaceText(string newText = "", bool animateAnyway = true)
    {
		var lastText = textDisplay.text;
		if (lastText == newText && !animateAnyway) { isanimating = false; yield break; }
        for (var x = 0; x < lastText.Length; x++)
        {
			textDisplay.text = lastText.Substring(0, lastText.Length - x);
			yield return new WaitForSeconds(0.2f);
		}
		for (var x = 0; x < newText.Length; x++)
		{
			textDisplay.text = newText.Substring(0, x);
			yield return new WaitForSeconds(0.2f);
		}
		textDisplay.text = newText;
		isanimating = false;
	}

	void ResetModule()
    {
		var serialNo = bombInfo.GetSerialNumber();
		var startingValue = int.Parse(Enumerable.Range(0, 2).Select(a => serialNo[3 * a + 2]).Join(""));
		countedCoordIdxes = bombInfo.GetPortCount() % 6;
		curCoordIdx = ConvertToIdxPos(startingValue, numReadTypes[countedCoordIdxes]);

		var attemptCount = 0;
	retryGen:
		attemptCount++;
		trapTileIdxes.Clear();
		trapTileIdxes.AddRange(Enumerable.Range(0, 100).Where(a => a != curCoordIdx).ToArray().Shuffle().Take(40));
		var navigatableTiles = new List<int>();
		distanceAllNavigatableTiles.Clear();
		var curNavigatableTiles = new List<int> { curCoordIdx };
        for (var d = 0; curNavigatableTiles.Any(); d++)
        {
			var nextNavigatableTiles = new List<int>();
			foreach (var idxTile in curNavigatableTiles)
            {
				navigatableTiles.Add(idxTile);
				var deltasRow = new[] { 0, 1, 9, 0 };
				var deltasCol = new[] { 1, 0, 0, 9 };
				var curRow = idxTile / 10;
				var curCol = idxTile % 10;
				for (var x = 0; x < 4; x++)
                {
					var checkRow = (curRow + deltasRow[x]) % 10;
					var checkCol = (curCol + deltasCol[x]) % 10;
					var newTileIdx = checkRow * 10 + checkCol;
					if (!(trapTileIdxes.Contains(newTileIdx) ||
						navigatableTiles.Contains(newTileIdx) ||
						nextNavigatableTiles.Contains(newTileIdx)))
						nextNavigatableTiles.Add(newTileIdx);
				}
			}
			distanceAllNavigatableTiles.Add(d, curNavigatableTiles.ToArray());
			curNavigatableTiles = nextNavigatableTiles;
        }

		if (navigatableTiles.Count < 20 && attemptCount < 100) goto retryGen;
		QuickLogFormat("Generated a navigatable board after {0} attempt(s).", attemptCount);
		QuickLogDebugFormat("Navigatable tiles: {0}", navigatableTiles.Select(a => QuickCoord(a)).Join());
		QuickLogDebugFormat("Max distance possible: {0}", distanceAllNavigatableTiles.Keys.Max());
		QuickLog("Trapped Tiles:");
		for (var x = 0; x < 10; x++)
			QuickLogFormat(Enumerable.Range(0, 10).Select(a => trapTileIdxes.Contains(10 * x + a) ? 'X' : '-').Join());
		Debug.Log(Enumerable.Range(0, 10).Select(x => Enumerable.Range(0, 10).Select(a => trapTileIdxes.Contains(10 * x + a) ? 'X' : '-').Join("")).Join("\n"));
		QuickLogFormat("Port count, modulo 6: {0}", countedCoordIdxes);
		QuickLogFormat("Starting on {0}, value {1} on the table.", QuickCoord(curCoordIdx), grid[curCoordIdx / 10][curCoordIdx % 10]);
		escapeTileIdx = distanceAllNavigatableTiles.Last().Value.PickRandom();
		QuickLogFormat("Escape Tile: {0}", QuickCoord(escapeTileIdx));
	}
	bool AreIdxesDiagonal(int oneIdx, int secondIdx)
    {
		var stIdxRow = oneIdx / 10;
		var stIdxCol = oneIdx % 10;
		var ndIdxRow = secondIdx / 10;
		var ndIdxCol = secondIdx % 10;

		for (var x = 0; x < 5; x++)
		{
			if (((stIdxRow + x) % 10 == ndIdxRow || (ndIdxRow + x) % 10 == stIdxRow) &&
				((stIdxCol + x) % 10 == ndIdxCol || (ndIdxCol + x) % 10 == stIdxCol))
				return true;
		}
		return false;
    }
	IEnumerable<List<int>> GetPathToDestination()
    {
		var curNavigatableTiles = new List<int> { escapeTileIdx };
		var navigatabledTiles = new List<int>();
		var distanceEndingTiles = new Dictionary<int, IEnumerable<int>>();
		for (var d = 0; curNavigatableTiles.Any(); d++)
		{
			var nextNavigatableTiles = new List<int>();
			foreach (var idxTile in curNavigatableTiles)
			{
				navigatabledTiles.Add(idxTile);
				var deltasRow = new[] { 0, 1, 9, 0 };
				var deltasCol = new[] { 1, 0, 0, 9 };
				var curRow = idxTile / 10;
				var curCol = idxTile % 10;
				for (var x = 0; x < 4; x++)
				{
					var checkRow = (curRow + deltasRow[x]) % 10;
					var checkCol = (curCol + deltasCol[x]) % 10;
					var newTileIdx = checkRow * 10 + checkCol;
					if (!(trapTileIdxes.Contains(newTileIdx) ||
						navigatabledTiles.Contains(newTileIdx) ||
						nextNavigatableTiles.Contains(newTileIdx)))
						nextNavigatableTiles.Add(newTileIdx);
				}
			}
			distanceEndingTiles.Add(d, curNavigatableTiles.ToArray());
			curNavigatableTiles = nextNavigatableTiles;
			if (nextNavigatableTiles.Contains(curCoordIdx))
			{
				distanceEndingTiles.Add(d + 1, curNavigatableTiles.ToArray());
				break;
			}
		}
		//QuickLogDebugFormat("Idxes arranged by distances: [{0}]", distanceEndingTiles.Select(a => a.Value.Join(",")).Join("];["));
		var allPathIdxes = new List<List<int>>();
		var allCurPathIdxes = new List<List<int>> { new List<int> { curCoordIdx } };
		while (allCurPathIdxes.Any())
        {
			var nextPathIdxes = new List<List<int>>();
			foreach (var curPath in allCurPathIdxes)
            {
				var lastIdxInPath = curPath.Last();
				var nextCurDistanceFromLast = distanceEndingTiles.Keys.Last(a => distanceEndingTiles[a].Contains(lastIdxInPath)) - 1;
				var deltasRow = new[] { 0, 1, 9, 0 };
				var deltasCol = new[] { 1, 0, 0, 9 };
				var lastRow = lastIdxInPath / 10;
				var lastCol = lastIdxInPath % 10;
				for (var x = 0; x < 4; x++)
				{
					var checkRow = (lastRow + deltasRow[x]) % 10;
					var checkCol = (lastCol + deltasCol[x]) % 10;
					var newTileIdx = checkRow * 10 + checkCol;
					if (distanceEndingTiles.ContainsKey(nextCurDistanceFromLast) && distanceEndingTiles[nextCurDistanceFromLast].Contains(newTileIdx))
					{
						var resultingNewPath = curPath.Concat(new[] { newTileIdx }).ToList();
						if (nextCurDistanceFromLast > 0)
							nextPathIdxes.Add(resultingNewPath);
						else
							allPathIdxes.Add(resultingNewPath);
					}
				}
			}
			allCurPathIdxes = nextPathIdxes;
		}

		return allPathIdxes;
	}


	void HandleRadar(bool getNewPath = true)
	{
		countedCoordIdxes = (countedCoordIdxes + 1) % 6;
		if (getNewPath)
        {
			var allPossiblePaths = GetPathToDestination();
			QuickLogDebugFormat("Safe paths to escape: [{0}]", allPossiblePaths.Select(a => a.Join(",")).Join("];["));
			recommendRadarIdxes = allPossiblePaths.PickRandom();
			QuickLogDebugFormat("Selected recommend radar coordinates: {0}", recommendRadarIdxes.Select(a => QuickCoord(a)).Join(","));
		}
		var curRow = curCoordIdx / 10;
		var curCol = curCoordIdx % 10;
		var allowedRadars = new List<int>();
		var allowedDirs = Enumerable.Range(0, 4).ToList();
		var maxDistancesRadarAll = Enumerable.Repeat(0, 4).ToArray();
		var dirRadarRefs = new Dictionary<int, List<int>>();
        for (int i = 0; i < 4; i++)
			dirRadarRefs.Add(i, new List<int>());
		for (var x = 1; x <= 4; x++)
        {
			var deltasRow = new[] { 9, 0, 1, 0 }; // Arranged to URDL for deltas
			var deltasCol = new[] { 0, 1, 0, 9 }; // Arranged to URDL for deltas
            foreach (var n in allowedDirs.ToArray())
            {
				var checkRow = (curRow + deltasRow[n] * x) % 10;
				var checkCol = (curCol + deltasCol[n] * x) % 10;
				var newTileIdx = checkRow * 10 + checkCol;
				if (trapTileIdxes.Contains(newTileIdx))
					allowedDirs.Remove(n);
				else
                {
					maxDistancesRadarAll[n]++;
					dirRadarRefs[n].Add(newTileIdx);
					allowedRadars.Add(newTileIdx);
				}
			}
		}
		QuickLogDebugFormat("Allowed non-deviating radars from {0}: {1}", QuickCoord(curCoordIdx), allowedRadars.Select(a => QuickCoord(a)).Join(", "));
		QuickLogDebugFormat("Max distances for moving U,R,D,L: {0}", maxDistancesRadarAll.Join(", "));
		QuickLogDebugFormat("Idxes in recommended radars: {0}", allowedRadars.Select(a => recommendRadarIdxes.IndexOf(a)).Join(", "));
		var idxInRecommended = recommendRadarIdxes.IndexOf(curCoordIdx);
		var dirRef = allowedRadars.ToDictionary(a => a, a => recommendRadarIdxes.IndexOf(a)).Where(a => a.Value > idxInRecommended);
		var idxMax = dirRef.Max(a => a.Value);
		var firstSafeRadar = dirRef.Single(a => a.Value >= idxMax).Key;
		
		curOptimalDirIdx = dirRadarRefs.Single(a => a.Value.Contains(firstSafeRadar)).Key;

		var deviatedCurCoordIdxes = new List<int>() { firstSafeRadar };
		var _stSRRowIdx = firstSafeRadar / 10;
		var _stSRColIdx = firstSafeRadar % 10;
		for (var x = 1; x < 5 - maxDistancesRadarAll[curOptimalDirIdx]; x++)
        {
			var deltasRow = new[] { 9, 9, 1, 1 };
			var deltasCol = new[] { 9, 1, 1, 9 };
			// UL, UR, DR, DL
			var offset1RowIdx = (_stSRRowIdx + deltasRow[curOptimalDirIdx] * x) % 10;
			var offset1ColIdx = (_stSRColIdx + deltasCol[curOptimalDirIdx] * x) % 10;
			var offset2RowIdx = (_stSRRowIdx + deltasRow[(curOptimalDirIdx + 1) % 4] * x) % 10;
			var offset2ColIdx = (_stSRColIdx + deltasCol[(curOptimalDirIdx + 1) % 4] * x) % 10;
			var hasValueAdded = false;

			if ((offset1RowIdx + 5) % 10 != curRow && (curCol + 5) % 10 != offset1ColIdx)
			{
				hasValueAdded = true;
				deviatedCurCoordIdxes.Add(10 * offset1RowIdx + offset1ColIdx);
			}
			if ((offset2RowIdx + 5) % 10 != curRow && (curCol + 5) % 10 != offset2ColIdx)
			{
				hasValueAdded = true;
				deviatedCurCoordIdxes.Add(10 * offset2RowIdx + offset2ColIdx);
			}
			if (!hasValueAdded)
				break;
		}

		radarCoordIdx = deviatedCurCoordIdxes.PickRandom();

		QuickLogFormat("New position radared: {0}", QuickCoord(radarCoordIdx));
		QuickLogFormat("Current conversion rule: {0}", numReadTypes[countedCoordIdxes].ToString());
		var convertedToDisplay = ConvertToDisplay(radarCoordIdx, numReadTypes[countedCoordIdxes]).ToString("00");
		QuickLogFormat("Its position will be displayed as the following on the module: {0}", convertedToDisplay);
		QuickLogFormat("Recommended Direction: {0}", arrowDirectionNames[curOptimalDirIdx]);
		isanimating = true;
		StartCoroutine(ReplaceText(convertedToDisplay));
    }
	void HandleColorblindToggle()
	{
		colorblindArrowDisplay.gameObject.SetActive(colorblindActive);
		textDisplay.color = colorblindActive ? new Color32(255, 255, 255, 255) : new Color32(51, 51, 51, 255);
	}
	protected override IEnumerator victory()
	{
		isanimating = true;
		for (int i = 0; i < 50; i++)
		{
			int rand2 = Random.Range(0, 10);
			int rand1 = Random.Range(0, 10);
			textDisplay.text = rand2.ToString() + rand1.ToString();
			yield return new WaitForSeconds(0.025f);
		}
		for (int i = 0; i < 50; i++)
		{
			int rand2 = Random.Range(0, 10);
			textDisplay.text = "G" + rand2;
			yield return new WaitForSeconds(0.025f);
		}
		textDisplay.text = "GG";
		modSelf.HandlePass();
	}
}
