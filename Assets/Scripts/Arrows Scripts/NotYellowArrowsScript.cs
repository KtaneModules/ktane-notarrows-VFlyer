using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArrowsSpecialHandlers;
using System.Linq;
using System.Text.RegularExpressions;

public class NotYellowArrowsScript : BaseArrowsScript {
	public TextMesh colorblindDisplayTxt;
	const byte width = 9, height = 9;
	WallColor[] allPossibleColors = new WallColor[] { WallColor.Red, WallColor.Yellow, WallColor.Green, WallColor.Blue };
	IEnumerable<WallColor>[][] hWalls = new[] {
		new[] {
		new[] { WallColor.Blue }, new[] { WallColor.Red, WallColor.Yellow, WallColor.Green }, new[] { WallColor.Green },
		new[] { WallColor.Yellow }, new[] { WallColor.Blue }, new[] { WallColor.Red },
		new[] { WallColor.Red, WallColor.Green, WallColor.Blue }, new[] { WallColor.Yellow },
		}, new[] {
		new[] { WallColor.Yellow }, new[] { WallColor.Red, WallColor.Yellow }, new WallColor[0],
		new[] { WallColor.Blue, WallColor.Red, WallColor.Yellow, WallColor.Green }, new[] { WallColor.Blue, WallColor.Red, WallColor.Yellow, WallColor.Green }, new WallColor[0],
		new WallColor[0], new WallColor[0],
		}, new[] {
		new[] { WallColor.Blue }, new[] { WallColor.Red, WallColor.Yellow }, new[] { WallColor.Red, WallColor.Green, WallColor.Blue },
		new[] { WallColor.Blue, WallColor.Yellow }, new[] { WallColor.Red }, new[] { WallColor.Yellow, WallColor.Green },
		new[] { WallColor.Blue }, new[] { WallColor.Yellow },
		}, new[] {
		new[] { WallColor.Yellow }, new WallColor[0], new[] { WallColor.Blue, WallColor.Yellow, WallColor.Red },
		new[] { WallColor.Green }, new[] { WallColor.Red, WallColor.Green, WallColor.Yellow }, new[] { WallColor.Blue, WallColor.Yellow },
		new WallColor[0], new[] { WallColor.Blue },
		}, new[] {
		new WallColor[0], new WallColor[0], new[] { WallColor.Green },
		new[] { WallColor.Red, WallColor.Yellow }, new[] { WallColor.Blue, WallColor.Green }, new[] { WallColor.Red },
		new WallColor[0], new WallColor[0],
		}, new[] {
		new[] { WallColor.Green }, new[] { WallColor.Green, WallColor.Red }, new[] { WallColor.Yellow, WallColor.Green },
		new[] { WallColor.Red, WallColor.Green }, new[] { WallColor.Blue }, new[] { WallColor.Red, WallColor.Green, WallColor.Blue },
		new WallColor[0], new[] { WallColor.Red },
		}, new[] {
		new[] { WallColor.Red }, new[] { WallColor.Red, WallColor.Green }, new[] { WallColor.Blue, WallColor.Yellow, WallColor.Green },
		new[] { WallColor.Red, WallColor.Green }, new[] { WallColor.Green }, new[] { WallColor.Blue, WallColor.Green, WallColor.Red },
		new[] { WallColor.Yellow }, new[] { WallColor.Green },
		}, new[] {
		new[] { WallColor.Green, WallColor.Blue }, new WallColor[0], new WallColor[0],
		new[] { WallColor.Blue, WallColor.Red, WallColor.Yellow, WallColor.Green }, new[] { WallColor.Blue, WallColor.Red, WallColor.Yellow, WallColor.Green }, new WallColor[0],
		new WallColor[0], new WallColor[0],
		},new[] {
		new[] { WallColor.Red }, new[] { WallColor.Blue, WallColor.Yellow, WallColor.Green }, new[] { WallColor.Yellow },
		new[] { WallColor.Green }, new[] { WallColor.Red }, new[] { WallColor.Blue },
		new[] { WallColor.Blue, WallColor.Red, WallColor.Yellow }, new[] { WallColor.Green } },
	}, vWalls = new[] {
		new[] {
		new[] { WallColor.Blue }, new[] { WallColor.Red, WallColor.Green }, new[] { WallColor.Blue },
		new[] { WallColor.Red }, new WallColor[0], new[] { WallColor.Green },
		new[] { WallColor.Yellow }, new[] { WallColor.Red, WallColor.Green }, new[] { WallColor.Yellow },
		}, new[] {
		new[] { WallColor.Red, WallColor.Green, WallColor.Yellow }, new[] { WallColor.Blue, WallColor.Green }, new[] { WallColor.Green },
		new WallColor[0], new WallColor[0], new WallColor[0],
		new[] { WallColor.Red, WallColor.Blue }, new WallColor[0], new[] { WallColor.Red, WallColor.Green, WallColor.Blue },
		}, new[] {
		new[] { WallColor.Green }, new WallColor[0], new[] { WallColor.Blue, WallColor.Green, WallColor.Red },
		new[] { WallColor.Red }, new[] { WallColor.Blue }, new[] { WallColor.Green },
		new[] { WallColor.Red, WallColor.Green, WallColor.Yellow }, new WallColor[0], new[] { WallColor.Red },
		}, new[] {
		new[] { WallColor.Red }, new[] { WallColor.Red, WallColor.Green, WallColor.Yellow, WallColor.Blue }, new[] { WallColor.Blue, WallColor.Red },
		new[] { WallColor.Yellow, WallColor.Green, WallColor.Blue }, new[] { WallColor.Blue, WallColor.Red }, new[] { WallColor.Yellow },
		new[] { WallColor.Green }, new[] { WallColor.Yellow, WallColor.Red, WallColor.Green, WallColor.Blue }, new[] { WallColor.Green },
		}, new[] {
		new[] { WallColor.Blue }, new[] { WallColor.Red, WallColor.Green, WallColor.Yellow, WallColor.Blue }, new[] { WallColor.Yellow },
		new[] { WallColor.Red }, new[] { WallColor.Green, WallColor.Yellow }, new[] { WallColor.Green, WallColor.Red },
		new[] { WallColor.Yellow, WallColor.Blue }, new[] { WallColor.Yellow, WallColor.Red, WallColor.Green, WallColor.Blue }, new[] { WallColor.Yellow },
		}, new[] {
		new[] { WallColor.Yellow }, new WallColor[0], new[] { WallColor.Blue, WallColor.Yellow },
		new[] { WallColor.Red, WallColor.Blue }, new[] { WallColor.Blue, WallColor.Yellow, WallColor.Red }, new[] { WallColor.Yellow },
		new[] { WallColor.Red }, new WallColor[0], new[] { WallColor.Blue },
		}, new[] {
		new[] { WallColor.Blue, WallColor.Green, WallColor.Yellow }, new WallColor[0], new[] { WallColor.Red, WallColor.Yellow },
		new WallColor[0], new WallColor[0], new WallColor[0],
		new[] { WallColor.Red, WallColor.Blue }, new WallColor[0], new[] { WallColor.Yellow, WallColor.Red, WallColor.Blue },
		}, new[] {
		new[] { WallColor.Red }, new[] { WallColor.Yellow }, new[] { WallColor.Red },
		new[] { WallColor.Blue }, new WallColor[0], new[] { WallColor.Yellow },
		new[] { WallColor.Green }, new WallColor[0], new[] { WallColor.Green },
		}
	};
	Dictionary<string, Color> colorRefs = new Dictionary<string, Color> {
		{ "Red", Color.red },
		{ "Yellow", new Color(1, .841f, 0f) },
		{ "Green", new Color(0.18393165f, 0.5955882f, 0.22083879f) },
		{ "Blue", new Color(.03529412f, .043137256f, 1) },
		};
	readonly static string[] arrowDirectionNames = new[] { "Up", "Right", "Down", "Left", },
		cardinalNames = new[] { "North", "East", "South", "West", };
	readonly string displayDirections = "\u25B4\u25B8\u25BE\u25C2";
	byte curRow = 4, curCol = 4, idxDirectionOffset = 0;
	static int modIDCnt;
	WallColor curForbiddenColor = WallColor.Invalid;
	protected override void QuickLogFormat(string toLog = "", params object[] misc)
	{
		QuickLog(string.Format(toLog, misc));
	}
	protected override void QuickLog(string toLog = "")
	{
		Debug.LogFormat("[Not Yellow Arrows #{0}] {1}", moduleId, toLog);
	}
	protected override void QuickLogDebugFormat(string toLog = "", params object[] misc)
	{
		QuickLogDebug(string.Format(toLog, misc));
	}
	protected override void QuickLogDebug(string toLog = "")
	{
		Debug.LogFormat("<Not Yellow Arrows #{0}> {1}", moduleId, toLog);
	}
	// Use this for initialization
	void Start () {
		moduleId = ++modIDCnt;
		ChangeForbiddenArrow(true);
		modSelf.OnActivate += delegate { StartCoroutine(HandleTypeText(displayDirections[idxDirectionOffset].ToString())); };
        for (var x = 0; x < arrowButtons.Length; x++)
        {
			var y = x;
			arrowButtons[x].OnInteract += delegate {
				if (!(moduleSolved || isanimating))
					HandleArrowPress(y);
				return false;
			};
        }
		textDisplay.text = "";
		textDisplay.characterSize = 150;
		textDisplay.transform.localPosition += Vector3.left * 0.04f;
		try
        {
			colorblindActive = Colorblind.ColorblindModeActive;
        }
		catch
        {
			colorblindActive = false;
        }
		HandleColorblindToggle();
	}

	IEnumerator HandleTypeText(string toType)
	{
		colorblindDisplayTxt.text = "";
		for (var x = 0; x < toType.Length; x++)
		{
			textDisplay.text = toType.Substring(0, x);
			yield return new WaitForSeconds(0.2f);
		}
		textDisplay.text = toType;
		colorblindDisplayTxt.text = colorblindActive ? (curForbiddenColor == WallColor.Invalid ? "W" : curForbiddenColor.ToString().Substring(0, 1)) : "";
		isanimating = false;
	}
	void ChangeForbiddenArrow(bool initialState = false)
    {
		curForbiddenColor = initialState ? WallColor.Invalid : allPossibleColors.PickRandom();
		idxDirectionOffset = (byte)Random.Range(0, 4);
		QuickLogFormat("The arrow is now {1} {0}.", arrowDirectionNames[idxDirectionOffset], initialState ? "White" : curForbiddenColor.ToString());
		textDisplay.color = initialState ? Color.white : colorRefs[curForbiddenColor.ToString()];
		colorblindDisplayTxt.text = colorblindActive ? (initialState ? "W" : curForbiddenColor.ToString().Substring(0, 1)) : "";
	}
	void HandleColorblindToggle()
    {
		colorblindArrowDisplay.gameObject.SetActive(colorblindActive);
		colorblindDisplayTxt.text = colorblindActive ? (curForbiddenColor == WallColor.Invalid ? "W" : curForbiddenColor.ToString().Substring(0, 1)) : "";
	}
	void HandleArrowPress(int idx)
    {
		MAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, arrowButtons[idx].transform);
		arrowButtons[idx].AddInteractionPunch(0.25f);
		var actualDirectionIdx = (idx - idxDirectionOffset + 4) % 4;
		QuickLogFormat("Pressing {0} moved you {1}.", arrowDirectionNames[idx], cardinalNames[actualDirectionIdx]);
		var remainingPossibleColors = allPossibleColors.Where(a => a != curForbiddenColor);
		var isMoveSafe = true;
		switch (actualDirectionIdx)
        {
			case 0: // North
                {
					isMoveSafe = curRow > 0 && vWalls[curRow - 1][curCol].Intersect(remainingPossibleColors).Any();
					//Debug.Log(vWalls[curRow - 1][curCol].Intersect(remainingPossibleColors).Select(a => a.ToString()).Join());
					if (isMoveSafe)
					{
						curRow--;
						QuickLogFormat("Successfully moved to {0}{1}.", "ABCDEFGHI"[curCol], curRow + 1);
					}
					else if (vWalls[curRow - 1][curCol].Any())
						QuickLogFormat("Out of the bridges [{0}] when moving, none were safe to travel north.", vWalls[curRow - 1][curCol].Select(a => a.ToString()).Join(", "));
					else
						QuickLogFormat("There are no bridges connected to from the current location to the location directly north.");
				}
				break;
			case 1: // East
                {
					isMoveSafe = curCol < 8 && hWalls[curRow][curCol].Intersect(remainingPossibleColors).Any();
					//Debug.Log(hWalls[curRow][curCol].Intersect(remainingPossibleColors).Select(a => a.ToString()).Join());
					if (isMoveSafe)
					{
						curCol++;
						QuickLogFormat("Successfully moved to {0}{1}.", "ABCDEFGHI"[curCol], curRow + 1);
					}
					else if (hWalls[curRow][curCol].Any())
						QuickLogFormat("Out of the bridges [{0}] when moving, none were safe to travel east.", hWalls[curRow][curCol].Select(a => a.ToString()).Join(", "));
					else
						QuickLogFormat("There are no bridges connected to from the current location to the location directly east.");
				}
				break;
			case 2: // South
                {
					isMoveSafe = curRow < 8 && vWalls[curRow][curCol].Intersect(remainingPossibleColors).Any();
					//Debug.Log(vWalls[curRow][curCol].Intersect(remainingPossibleColors).Select(a => a.ToString()).Join());
					if (isMoveSafe)
					{
						curRow++;
						QuickLogFormat("Successfully moved to {0}{1}.", "ABCDEFGHI"[curCol], curRow + 1);
					}
					else if (vWalls[curRow][curCol].Any())
						QuickLogFormat("Out of the bridges [{0}] when moving, none were safe to travel south.", vWalls[curRow + 1][curCol].Select(a => a.ToString()).Join(", "));
					else
						QuickLogFormat("There are no bridges connected to from the current location to the location directly south.");
				}
				break;
			case 3: // West
                {
					isMoveSafe = curCol > 0 && hWalls[curRow][curCol - 1].Intersect(remainingPossibleColors).Any();
					//Debug.Log(hWalls[curRow][curCol - 1].Intersect(remainingPossibleColors).Select(a => a.ToString()).Join());
					if (isMoveSafe)
					{
						curCol--;
						QuickLogFormat("Successfully moved to {0}{1}.", "ABCDEFGHI"[curCol], curRow + 1);
					}
					else if (hWalls[curRow][curCol - 1].Any())
						QuickLogFormat("Out of the bridges [{0}] when moving, none were safe to travel west.", hWalls[curRow][curCol - 1].Select(a => a.ToString()).Join(", "));
					else
						QuickLogFormat("There are no bridges connected to from the current location to the location directly west.");
				}
				break;
        }
		var requireReset = false;
		if (isMoveSafe)
		{
			var curIdxPos = curCol * 9 + curRow;
			var idxesEnd = new[] { 0, 8, 72, 80 };
			if (idxesEnd.Contains(curIdxPos))
            {
				moduleSolved = true;
				QuickLogFormat("Safely moved to the destination. Module solved.");
				StartCoroutine(victory());
				return;
			}
		}
		else
        {
			modSelf.HandleStrike();
			requireReset = true;
			curCol = 4;
			curRow = 4;
        }
		ChangeForbiddenArrow(requireReset);
		StartCoroutine(HandleTypeText(displayDirections[idxDirectionOffset].ToString()));
	}
	IEnumerator HandleRainbowTextAnim()
	{
		Color lastColor = textDisplay.color;
		for (int i = 0; i < 25; i++)
		{
			textDisplay.color = lastColor * (1.0f - i / 25f) + Color.red * (i / 25f);
			yield return new WaitForSeconds(0.025f);
		}
		for (int i = 0; i < 25; i++)
		{
			textDisplay.color = Color.red * (1.0f - i / 25f) + colorRefs["Green"] * (i / 25f);
			yield return new WaitForSeconds(0.025f);
		}
		for (int i = 0; i < 25; i++)
		{
			textDisplay.color = colorRefs["Green"] * (1.0f - i / 25f) + colorRefs["Blue"] * (i / 25f);
			yield return new WaitForSeconds(0.025f);
		}
		for (int i = 0; i < 25; i++)
		{
			textDisplay.color = colorRefs["Blue"] * (1.0f - i / 25f) + colorRefs["Yellow"] * (i / 25f);
			yield return new WaitForSeconds(0.025f);
		}
		textDisplay.color = colorRefs["Yellow"];
	}

	protected override IEnumerator victory()
	{
		isanimating = true;
		Color lastColor = textDisplay.color;
		colorblindArrowDisplay.text = "";
		StartCoroutine(HandleRainbowTextAnim());
		for (int i = 0; i < 25; i++)
		{
			int rand1 = Random.Range(0, 4);
			textDisplay.text = displayDirections[rand1].ToString();
			yield return new WaitForSeconds(0.025f);
		}
		textDisplay.transform.localPosition += Vector3.right * .04f;
		textDisplay.characterSize = 100;
		for (int i = 0; i < 25; i++)
		{
			int rand2 = Random.Range(0, 10);
			textDisplay.text = rand2.ToString();
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
		isanimating = false;
	}
#pragma warning disable 414
	private readonly string TwitchHelpMessage = "Press the specified arrow button with \"!{0} up/right/down/left\" Words can be substituted as one letter (Ex. right as r) Toggle colorblind mode with \"!{0} colorblind\"";
#pragma warning restore 414
	protected override IEnumerator ProcessTwitchCommand(string command)
	{
		if (moduleSolved || isanimating)
		{
			yield return "sendtochaterror The module is not accepting any commands at this moment.";
			yield break;
		}
        if (Regex.IsMatch(command, @"^\s*colou?rblind\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            colorblindActive = !colorblindActive;
            HandleColorblindToggle();
            yield break;
        }
		if (Regex.IsMatch(command, @"^\s*u(p)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			arrowButtons[0].OnInteract();
		}
		else if (Regex.IsMatch(command, @"^\s*d(own)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			arrowButtons[2].OnInteract();
		}
		else if (Regex.IsMatch(command, @"^\s*l(eft)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			arrowButtons[3].OnInteract();
		}
		else if (Regex.IsMatch(command, @"^\s*r(ight)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			arrowButtons[1].OnInteract();
		}
		if (moduleSolved) { yield return "solve"; }
		yield break;
	}
}
