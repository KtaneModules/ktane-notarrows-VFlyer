using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class NotBlueArrowsScript : BaseArrowsScript {

	public KMSelectable screenSelectable;
	public PatternGenerator[] bars;
	public KMBombInfo bombInfo;
	const string digits = "0123456789";
	readonly static string[] arrowDirectionNames = new[] { "Up", "Right", "Down", "Left", };
	List<List<int>> patternRefs = new List<List<int>>
	{
		new List<int> { 27 },
		new List<int> { 1,1,1,1,1,1,1,1,1,1,1,1,1,1 },
		new List<int> { 2,1,2,1,2,1,2,1,2,1,2 },
		new List<int> { 2,1,3,1,5,1,3,1,2 },
		new List<int> { 2,3,1,3,1,3,1,3,2 },
		new List<int> { 4,1,2,1,3,1,2,1,4 },
		new List<int> { 1,3,2,3,1,3,2,3,1 },
		new List<int> { 3,3,3,3,3,3,3 },
		new List<int> { 1,1,2,2,2,1,2,2,2,1,1 },
		new List<int> { 2,1,1,3,1,1,1,3,1,1,2 },
		new List<int> { 1,2,1,1,2,3,2,1,1,2,1 },
		new List<int> { 3,5,1,3,1,5,3 },
		new List<int> { 1,1,1,3,1,1,1,1,3,1,1,1 },
		new List<int> { 1,3,2,1,2,2,1,2,3,1 },
		new List<int> { 3,1,2,4,4,2,1,3 },
		new List<int> { 2,1,1,4,1,1,4,1,1,2 },
	};

	int[] displayedValues;
	int curValue, goalValue;
	static int modIDCnt;
	protected override void QuickLogFormat(string toLog = "", params object[] misc)
	{
		QuickLog(string.Format(toLog, misc));
	}
	protected override void QuickLog(string toLog = "")
	{
		Debug.LogFormat("[Not Blue Arrows #{0}] {1}", moduleId, toLog);
	}
	protected override void QuickLogDebugFormat(string toLog = "", params object[] misc)
	{
		QuickLogDebug(string.Format(toLog, misc));
	}
	protected override void QuickLogDebug(string toLog = "")
	{
		Debug.LogFormat("<Not Blue Arrows #{0}> {1}", moduleId, toLog);
	}
	// Use this for initialization
	void Start () {
		moduleId = ++modIDCnt;
		for (var x = 0; x < bars.Length; x++)
			bars[x].DuplicateBaseObjectXTimes();
		displayedValues = new int[2];
		textDisplay.text = "";
		ResetModule();
		modSelf.OnActivate += delegate { StartCoroutine(HandleAnimRenderBars()); StartCoroutine(TypeText(curValue.ToString())); };
        for (var x = 0; x < arrowButtons.Length; x++)
        {
			var y = x;
			arrowButtons[x].OnInteract += delegate { if (!(moduleSolved || isanimating)) HandleArrowPress(y); return false; };
        }
		screenSelectable.OnInteract += delegate { if (!(moduleSolved || isanimating)) HandleSubmit(); return false; };
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
	void ResetModule()
    {
		for (var x = 0; x < 2; x++)
			displayedValues[x] = Random.Range(0, 16);
		for (var x = 0; x < bars.Length; x++)
			foreach (GameObject pxl in bars[x].storedObjects)
				pxl.SetActive(false);
		goalValue = displayedValues[0] * displayedValues[1] + displayedValues[0] * 16 + displayedValues[1];
		curValue = Random.Range(100, 1000);

		QuickLogFormat("The top line's representation in decimal is {0}.", displayedValues[0]);
		QuickLogFormat("The bottom line's representation in decimal is {0}.", displayedValues[1]);
		QuickLogFormat("The value to reach should be {0}.", goalValue);
		QuickLogFormat("Starting on {0}.", curValue);

	}
	IEnumerator TypeText(string text)
    {
		var allowedCharSizes = new[] { 100, 75, 50 };
		textDisplay.characterSize = text.Length < 2 ? allowedCharSizes.First() : text.Length - 2 >= allowedCharSizes.Length ? allowedCharSizes.Last() : allowedCharSizes[text.Length - 2];
		for (var x = 0; x < text.Length; x++)
		{
			textDisplay.text = text.Substring(0, x);
			yield return new WaitForSeconds(0.2f);
		}
		textDisplay.text = text;
		isanimating = false;
	}
	IEnumerator HandleAnimRenderBars(bool enablePressesOnFinish = true)
    {
		isanimating = true;
		var topBoolSet = new bool[27];
		var bottomBoolSet = new bool[27];
		var selectedGroupingTop = patternRefs[displayedValues[0]];
		var selectedGroupingBot = patternRefs[displayedValues[1]];
		for (var pointer = 0; pointer < 27;)
		{
			for (var x = 0; x < selectedGroupingTop.Count; x++)
			{
				if (pointer >= 27) break;
				for (var p = 0; p < selectedGroupingTop[x]; p++)
					topBoolSet[pointer + p] = true;
				pointer += selectedGroupingTop[x] + 1;
			}
		}
		for (var pointer = 0; pointer < 27;)
		{
			for (var x = 0; x < selectedGroupingBot.Count; x++)
			{
				if (pointer >= 27) break;
				for (var p = 0; p < selectedGroupingBot[x]; p++)
					bottomBoolSet[pointer + p] = true;
				pointer += selectedGroupingBot[x] + 1;
			}
		}
		for (var x = 0; x < 14; x++)
        {
			yield return new WaitForSeconds(0.1f);
			bars[0].storedObjects[x].SetActive(topBoolSet[x]);
			bars[1].storedObjects[x].SetActive(bottomBoolSet[x]);
			bars[0].storedObjects[26 - x].SetActive(topBoolSet[x]);
			bars[1].storedObjects[26 - x].SetActive(bottomBoolSet[x]);
		}
		isanimating &= !enablePressesOnFinish;
	}
	void HandleArrowPress(int idx)
    {
		MAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, arrowButtons[idx].transform);
		arrowButtons[idx].AddInteractionPunch(0.25f);
		var lastSecondsDigit = (int)(bombInfo.GetTime() % 10);
		var lastSNDigit = bombInfo.GetSerialNumberNumbers().LastOrDefault();
		var lastCurValue = curValue;
		var valueInString = curValue.ToString();
		switch (idx)
        {
			case 0: // Up
				switch(lastSecondsDigit)
                {
					case 0:
						curValue /= 2; break;
					case 1:
						curValue += 10; break;
					case 2:
						if (valueInString.Count(a => char.IsDigit(a)) > 1)
							curValue = int.Parse(valueInString.Substring(0, valueInString.Length - 1));
						break;
					case 3:
						break;
					case 4:
						if (!int.TryParse(valueInString.Select(a => digits.IndexOf(a) != -1 ? digits[(digits.IndexOf(a) + 1) % 10] : a).Join(""), out curValue))
							QuickLog("Value was invalid after shifting digits up by 1.");
						break;
					case 5:
						curValue += lastSNDigit; break;
					case 6:
						curValue -= 100; break;
					case 7:
						var convertedChrArray = valueInString.ToCharArray();
						for (var x = 0; x < Mathf.Min(2, convertedChrArray.Count(a => char.IsDigit(a))); x++)
							convertedChrArray[convertedChrArray.Length - 1 - x] = '0';
						if (!int.TryParse(convertedChrArray.Join(""), out curValue))
							QuickLog("Value is invalid after setting up to two right-most digits to 0.");
						break;
					case 8:
						curValue = curValue * 17 / 20; break;
					case 9:
						curValue += 11; break;
				}
				break;
			case 1: // Right
				switch (lastSecondsDigit)
				{
					case 0:
						curValue -= bombInfo.GetSerialNumberNumbers().LastOrDefault(); break;
					case 1:
						break;
					case 2:
						curValue += 6; break;
					case 3:
						curValue *= 2; break;
					case 4:
						curValue += displayedValues[0]; break;
					case 5:
						curValue -= 38; break;
					case 6:
						if (!int.TryParse(valueInString.Select(a => digits.IndexOf(a) != -1 ? digits[(digits.IndexOf(a) + 2) % 10] : a).Join(""), out curValue))
							QuickLog("Value was invalid after shifting digits up by 2.");
						break;
					case 7:
						curValue -= displayedValues[1]; break;
					case 8:
						curValue -= 10; break;
					case 9:
						var convertedCharList = valueInString.ToList();
						if (convertedCharList.Count(a => char.IsDigit(a)) > 1)
                        {
							var lastChr = convertedCharList.Last();
							convertedCharList.RemoveAt(convertedCharList.Count - 1);
							convertedCharList.Insert(char.IsDigit(convertedCharList.First()) ? 0 : 1, lastChr);
							if (!int.TryParse(convertedCharList.Join(""), out curValue))
								QuickLog("Value was invalid after moving the last digit to the front.");
						}
						break;
				}
				break;
			case 2: // Down
				switch (lastSecondsDigit)
				{
					case 0:
						var convertedChrArray = valueInString.ToCharArray();
						for (var x = 0; x < Mathf.Min(1, convertedChrArray.Count(a => char.IsDigit(a))); x++)
							convertedChrArray[convertedChrArray.Length - 1 - x] = '0';
						if (!int.TryParse(convertedChrArray.Join(""), out curValue))
							QuickLog("Value is invalid after setting a right-most digit to 0.");
						break;
					case 1:
						if (valueInString.ToList().Count(a => char.IsDigit(a)) > 1)
						{
							var sgnValue = Mathf.Sign(lastCurValue);
							var absVal = Mathf.Abs(lastCurValue).ToString();
							int tryConvertedVal;
							if (int.TryParse(absVal.Reverse().Join(""), out tryConvertedVal))
								curValue = (int)sgnValue * tryConvertedVal;
							else
								QuickLog("Value is invalid after reversing digits.");
						}
						break;
					case 2:
						curValue -= 17; break;
					case 3:
						var convertedCharList = valueInString.ToList();
						if (convertedCharList.Count(a => char.IsDigit(a)) > 1)
						{
							var startIdx = char.IsDigit(convertedCharList.First()) ? 0 : 1;
							var firstChr = convertedCharList[startIdx];
							convertedCharList.RemoveAt(startIdx);
							convertedCharList.Add(firstChr);
							if (!int.TryParse(convertedCharList.Join(""), out curValue))
								QuickLog("Value was invalid after moving the first digit to the back.");
						}
						break;
					case 4:
						curValue *= 2; break;
					case 5:
						curValue += 27; break;
					case 6:
						break;
					case 7:
						curValue -= displayedValues[0]; break;
					case 8:
						if (!int.TryParse(valueInString.Select(a => digits.IndexOf(a) != -1 ? digits[(digits.IndexOf(a) + 9) % 10] : a).Join(""), out curValue))
							QuickLog("Value was invalid after shifting digits down by 1.");
						break;
					case 9:
						curValue += 1;break;
				}
				break;
			case 3: // Left
				switch (lastSecondsDigit)
				{
					case 0:
						curValue += 100; break;
					case 1:
						curValue -= 1; break;
					case 2:
						if (!int.TryParse(valueInString.Select(a => digits.IndexOf(a) != -1 ? digits[(digits.IndexOf(a) + 8) % 10] : a).Join(""), out curValue))
							QuickLog("Value was invalid after shifting digits down by 2.");
						break;
					case 3:
						curValue += displayedValues[1]; break;
					case 4:
						curValue *= 5; break;
					case 5:
						curValue += 42; break;
					case 6:
						if (valueInString.ToList().Count(a => char.IsDigit(a)) > 1)
						{
							var sgnValue = Mathf.Sign(lastCurValue);
							var absVal = Mathf.Abs(lastCurValue).ToString();
							int tryConvertedVal;
							if (int.TryParse(absVal.Reverse().Join(""), out tryConvertedVal))
								curValue = (int)sgnValue * tryConvertedVal;
							else
								QuickLog("Value is invalid after reversing digits.");
						}
						break;
					case 7:
						curValue -= 9; break;
					case 8:
						break;
					case 9:
						curValue *= lastSNDigit; break;
				}
				break;
		}
		if (lastCurValue == curValue)
			QuickLogFormat("Current value did not change after pressing {0} when the last seconds digit of the countdown timer is {1}.", arrowDirectionNames[idx], lastSecondsDigit);
		else
			QuickLogFormat("Current value changed to {2} after pressing {0} when the last seconds digit of the countdown timer is {1}.", arrowDirectionNames[idx], lastSecondsDigit, curValue);
		var allowedCharSizes = new[] { 100, 75, 50 };
		var curText = curValue.ToString();
		textDisplay.characterSize = curText.Length < 2 ? allowedCharSizes.First() : curText.Length - 2 >= allowedCharSizes.Length ? allowedCharSizes.Last() : allowedCharSizes[curText.Length - 2];
		textDisplay.text = curText;
	}
	void HandleSubmit()
    {
		MAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, screenSelectable.transform);
		screenSelectable.AddInteractionPunch(0.25f);
		if (curValue == goalValue)
        {
			QuickLogFormat("Submitted correct value: {0}. Module disarmed.", curValue);
			moduleSolved = true;
			StartCoroutine(victory());
        }
		else
        {
			QuickLogFormat("Submitted incorrect value: {0}. Strike! Resetting...", curValue);
			modSelf.HandleStrike();
			ResetModule();
			StartCoroutine(HandleAnimRenderBars());
			StartCoroutine(TypeText(curValue.ToString()));
			isanimating = true;
		}
    }
	IEnumerator HideBars()
    {
		for (var x = 0; x < 14; x++)
		{
			yield return new WaitForSeconds(0.1f);
			bars[0].storedObjects[x].SetActive(false);
			bars[1].storedObjects[x].SetActive(false);
			bars[0].storedObjects[26 - x].SetActive(false);
			bars[1].storedObjects[26 - x].SetActive(false);
		}
	}
    protected override IEnumerator victory()
    {
		StartCoroutine(HideBars());
		isanimating = true;
		for (int i = 0; i < 25; i++)
		{
			textDisplay.text = textDisplay.text.Select(a => digits.PickRandom()).Join("");
			yield return new WaitForSeconds(0.025f);
		}
		textDisplay.characterSize = 100;
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
	void HandleColorblindToggle()
	{
		colorblindArrowDisplay.gameObject.SetActive(colorblindActive);
		textDisplay.color = colorblindActive ? Color.white : new Color(.03529412f, .043137256f, 1);
		foreach (var bar in bars)
        {
			for (var x = 0; x < bar.storedObjects.Length; x++)
            {
				var renderer = bar.storedObjects[x].GetComponent<MeshRenderer>();
				if (renderer != null)
					renderer.material.color = colorblindActive ? Color.white : new Color(.03529412f, .043137256f, 1);
			}
        }
	}
#pragma warning disable 414
	private readonly string TwitchHelpMessage = "Press the specified arrow button with \"!{0} up/right/down/left\" Words can be substituted as one letter (Ex. right as r) To time a specific press, append digits to press when the last seconds digit of the countdown timer is any of those values, I.E. \"!{0} u 5 6 3 1\" Arrow presses can be chained and timed together via spaces, I.E. \"!{0} u 6 L 2 R 8\" Toggle colorblind mode with \"!{0} colorblind\" Submit the displayed value with \"!{0} submit\"";
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
		else if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			screenSelectable.OnInteract();
			if (moduleSolved) { yield return "solve"; }
			yield break;
		}
		var splittedParts = command.Trim().Split();
		var btnsAllToPress = new List<KMSelectable>();
		var timeConstraints = new List<List<int>>();
		List<int> curTimeList = null;
		foreach (var aPart in splittedParts)
        {
			var rgxMatchU = Regex.Match(aPart, @"^\s*u(p)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var rgxMatchD = Regex.Match(aPart, @"^\s*d(own)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var rgxMatchL = Regex.Match(aPart, @"^\s*l(eft)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var rgxMatchR = Regex.Match(aPart, @"^\s*r(ight)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var rgxMatchDigit = Regex.Match(aPart, @"^\s*[0-9]\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var requireNewList = false;

			if (rgxMatchU.Success)
            {
				requireNewList = true;
				btnsAllToPress.Add(arrowButtons[0]);
            }
			else if (rgxMatchR.Success)
            {
				requireNewList = true;
				btnsAllToPress.Add(arrowButtons[1]);
            }
			else if (rgxMatchD.Success)
            {
				requireNewList = true;
				btnsAllToPress.Add(arrowButtons[2]);
            }
			else if (rgxMatchL.Success)
            {
				requireNewList = true;
				btnsAllToPress.Add(arrowButtons[3]);
            }
			else if (rgxMatchDigit.Success)
			{
				var idxDigitMatch = digits.IndexOf(rgxMatchDigit.Value.First());
				if (curTimeList == null)
				{
					yield return "sendtochaterror You cannot specify when to press without specifying the direction to press first!";
					yield break;
				}
				else if (idxDigitMatch == -1)
                {
					yield return string.Format("sendtochaterror The specified command part \"{0}\" does not work when timing presses!", rgxMatchDigit.Value);
					yield break;
				}
				curTimeList.Add(idxDigitMatch);
			}
			else
            {
				yield return string.Format("sendtochaterror I do not know what \"{0}\" means. Check your command for typos.", aPart);
				yield break;
            }
			if (requireNewList)
            {
				if (curTimeList != null)
					timeConstraints.Add(curTimeList);
				curTimeList = new List<int>();
			}
		}
		if (curTimeList != null)
			timeConstraints.Add(curTimeList);
		if (btnsAllToPress.Any())
		{
			yield return null;
			for (var x = 0; x < btnsAllToPress.Count; x++)
			{
				var curTimeConstraints = timeConstraints[x];
				while (curTimeConstraints.Any() && !curTimeConstraints.Contains((int)bombInfo.GetTime() % 10))
					yield return string.Format("trycancel A timed button press was canceled after {0} press(es)!", x);
				btnsAllToPress[x].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
		}
		yield break;
	}
    protected override IEnumerator TwitchHandleForcedSolve()
    {
        while (curValue != goalValue)
        {
			if (curValue < 0)
            {
				while ((int)bombInfo.GetTime() % 10 != 5)
					yield return true;
				arrowButtons[0].OnInteract();
				yield return new WaitForSeconds(0.1f);
				while ((int)bombInfo.GetTime() % 10 != 7)
					yield return true;
				arrowButtons[0].OnInteract();
			}
			else if (curValue > 999)
            {
				while ((int)bombInfo.GetTime() % 10 != 0)
					yield return true;
				arrowButtons[0].OnInteract();
			}
			else if (curValue / 100 < goalValue / 100)
            {
				while ((int)bombInfo.GetTime() % 10 != 0)
					yield return true;
				arrowButtons[3].OnInteract();
			}
			else if (curValue / 100 > goalValue / 100)
            {
				while ((int)bombInfo.GetTime() % 10 != 6)
					yield return true;
				arrowButtons[0].OnInteract();
			}
			else if (curValue / 10 % 10 < goalValue / 10 % 10)
            {
				while ((int)bombInfo.GetTime() % 10 != 1)
					yield return true;
				arrowButtons[0].OnInteract();
			}
			else if (curValue / 10 % 10 > goalValue / 10 % 10)
            {
				while ((int)bombInfo.GetTime() % 10 != 8)
					yield return true;
				arrowButtons[1].OnInteract();
			}
			else if (curValue % 10 < goalValue % 10)
            {
				while ((int)bombInfo.GetTime() % 10 != 9)
					yield return true;
				arrowButtons[2].OnInteract();
			}
			else if (curValue % 10 > goalValue % 10)
            {
				while ((int)bombInfo.GetTime() % 10 != 1)
					yield return true;
				arrowButtons[3].OnInteract();
			}
			yield return new WaitForSeconds(0.1f);
		}
		screenSelectable.OnInteract();
		while (isanimating)
			yield return true;
    }
}
