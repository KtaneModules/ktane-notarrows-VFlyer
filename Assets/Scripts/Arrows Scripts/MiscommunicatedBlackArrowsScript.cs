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
	const string digits = "0123456789";
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
	List<int> trapTileIdxes;
	Dictionary<int, int[]> distanceAllNavigatableTiles = new Dictionary<int, int[]>();

	static int modIDCnt;
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
		QuickLogFormat("Pressed {0} to move to idx {1}...", arrowDirectionNames[idx], curCoordIdx);
		var requireRecalc = false;
		if (trapTileIdxes.Contains(curCoordIdx))
        {
			MAudio.PlaySoundAtTransform("CyanArrowsFall", transform);
			QuickLog("The tile in that index is not safe! Resetting.");
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
			QuickLog("Deviated from the optimal press. At least you are safe but some recalculations are needed.");
			requireRecalc = true;
		}
		else
        {
			var radarRow = radarCoordIdx / 10;
			var radarCol = radarCoordIdx % 10;
			requireRecalc = radarRow == curRow || radarCol == curCol || AreIdxesDiagonal(radarCoordIdx, curCoordIdx);
		}
		if (requireRecalc)
			HandleRadar();
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

	IEnumerator ReplaceText(string newText = "")
    {
		var lastText = textDisplay.text;
		if (lastText == newText) yield break;
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
		QuickLogDebugFormat("Idx navigatable tiles: {0}", navigatableTiles.Join());
		QuickLogDebugFormat("Max distance possible: {0}", distanceAllNavigatableTiles.Keys.Max());
		QuickLog("Trapped Tiles:");
		for (var x = 0; x < 10; x++)
			QuickLogFormat(Enumerable.Range(0, 10).Select(a => trapTileIdxes.Contains(10 * x + a) ? 'X' : '-').Join());
		QuickLogFormat("Port count, modulo 6: {0}", countedCoordIdxes);
		QuickLogFormat("Starting on idx {0}, value {1} on the table.", curCoordIdx, grid[curCoordIdx / 10][curCoordIdx % 10]);
		escapeTileIdx = distanceAllNavigatableTiles.Last().Value.PickRandom();
		QuickLogFormat("Escape Tile Idx: {0}", escapeTileIdx);
	}
	bool AreIdxesDiagonal(int oneIdx, int secondIdx)
    {
		var stIdxRow = oneIdx / 10;
		var stIdxCol = oneIdx % 10;
		var ndIdxRow = secondIdx / 10;
		var ndIdxCol = secondIdx % 10;

		for (var x = 1; x < 5; x++)
		{
			if (((stIdxRow + x) % 10 == ndIdxRow || (ndIdxRow + x) % 10 == stIdxRow) &&
				((stIdxCol + x) % 10 == stIdxCol || (ndIdxCol + x) % 10 == ndIdxCol))
				return true;
		}
		return false;
    }

	void HandleRadar()
	{
		var idxRow = curCoordIdx / 10;
		var idxCol = curCoordIdx % 10;
		var forbiddenRadarIdxes = Enumerable.Range(0, 100).Where(a =>
			(a / 10 + 5) % 10 == idxRow ||
			(a / 10 + 5) % 10 == idxCol || a == curCoordIdx || AreIdxesDiagonal(a, curCoordIdx)).ToList();
		countedCoordIdxes = (countedCoordIdxes + 1) % 6;
	retryRadar:
		var radarSafe = true;
		var traversableTileIdxes = new List<int>();
		var deltasRow = new[] { 0, 1, 9, 0 };
		var deltasCol = new[] { 1, 0, 0, 9 };
		var safeIdxDirTravel = Enumerable.Range(0, 4).ToList();
		var offsettingRow = curCoordIdx / 10;
		var offsettingCol = curCoordIdx % 10;
		for (var x = 1; x < 5 && safeIdxDirTravel.Any(); x++)
        {
			var lastSafeIdxDirTravel = safeIdxDirTravel.ToList();
			foreach (var safeDir in lastSafeIdxDirTravel)
            {
				var endingTile = (deltasRow[safeDir] * x + offsettingRow) % 10 * 10 + (deltasCol[safeDir] * x + offsettingCol) % 10;
				if (trapTileIdxes.Contains(endingTile))
					safeIdxDirTravel.Remove(safeDir);
				else
					traversableTileIdxes.Add(endingTile);
			}				
        }
		QuickLogDebugFormat("Safe tiles idx respecting only moving in one direction: {0}", traversableTileIdxes.Join());
		radarCoordIdx = traversableTileIdxes.PickRandom();
		radarSafe &= radarCoordIdx != curCoordIdx;
		if (!radarSafe) goto retryRadar;
		QuickLogFormat("Selected new radar idx: {0}", radarCoordIdx);
		QuickLogFormat("Current conversion rule: {0}", numReadTypes[countedCoordIdxes].ToString());
		var convertedToDisplay = ConvertToDisplay(radarCoordIdx, numReadTypes[countedCoordIdxes]).ToString("00");
		QuickLogFormat("Its position will be displayed as the following on the module: {0}", convertedToDisplay);
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
		for (int i = 0; i < 25; i++)
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
