using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;
using System.Linq;
using System.Text.RegularExpressions;

public class NotRedArrowsScript : BaseArrowsScript {
	public KMSelectable screenSelectable;
	public KMBombInfo bombInfo;

	static int modIDCnt;

	int expectedNumber, currentNumber;
	bool leftIsAdd, downIsAdd;
    protected override void QuickLogFormat(string toLog = "", params object[] misc)
    {
		QuickLog(string.Format(toLog, misc));
    }
    protected override void QuickLog(string toLog = "")
    {
		Debug.LogFormat("[Not Red Arrows #{0}] {1}", moduleId, toLog);
    }
	protected override void QuickLogDebugFormat(string toLog = "", params object[] misc)
	{
		QuickLogDebug(string.Format(toLog, misc));
	}
	protected override void QuickLogDebug(string toLog = "")
	{
		Debug.LogFormat("<Not Red Arrows #{0}> {1}", moduleId, toLog);
	}
	// Use this for initialization
	void Start()
	{
		moduleId = ++modIDCnt;
		QuickLog("The module will only log JMP instructions, the initial value displayed, and the expected value on this module.");
		modSelf.OnActivate += delegate { StartCoroutine(TypeText(currentNumber.ToString("00"))); };
		//leftIsAdd = Random.value < 0.5f;
		//downIsAdd = Random.value < 0.5f;
		ResetModule();
		screenSelectable.OnInteract += delegate {
			if (!(moduleSolved || isanimating))
				HandleSubmit();
			return false;
		};
		for (var x = 0; x < arrowButtons.Length; x++)
        {
			var y = x;
			arrowButtons[x].OnInteract += delegate {
				if (!(moduleSolved || isanimating))
					HandleArrowPress(y);
				return false;
			};
        }
		try
		{
			colorblindActive = Colorblind.ColorblindModeActive;
		}
		catch
        {
			colorblindActive = false;
        }
		HandleColorblindModeToggle();
		textDisplay.text = "";
	}
	void ResetModule()
    {
		var startingValue = Random.Range(0, 100);
		QuickLogFormat("Initial Value: {0}", startingValue);
		var key = startingValue;
		var lastDigit = bombInfo.GetSerialNumberNumbers().LastOrDefault();
		var correctNumber = 11;
		var tick = 0;
		var trigger = true;
		var totalCount = 0;
		if (startingValue % 2 == 0)
        {
			QuickLog("JMP S_E");
			if (key % 10 == lastDigit)
			{
				trigger = false;
				key -= lastDigit;
			}
			else if (key % 10 > lastDigit)
			{
				key = (lastDigit + 2) * 7;
			}
			else
				key += lastDigit;
        }
		else
        {
			QuickLog("JMP S_O");
			if (key % 10 == lastDigit)
			{
				key = (2 * key) % 100;
			}
			else if (key % 10 > lastDigit)
			{
				key = (lastDigit - 2) * 7;
				trigger = false;
			}
			else
				key = 50 - key;
		}
		QuickLog("JMP CNT_OB");
		var n = Mathf.Abs(key);
		var cycleCount = 0;
		var count = 0;
		while (tick <= 3)
		{
			while (n > 0)
			{
				var a = n % 2;
				if (a == 1)
					count++;
				n /= 2;
				cycleCount++;
			}
			totalCount += count;
			if (tick > 2) break;
			if (cycleCount % 2 == 0)
            {
				QuickLog("JMP O_V");
				if (trigger)
				{
					trigger = false;
					key = key * (cycleCount + 1);
				}
				else
					key += cycleCount;
            }
			else
            {
				QuickLog("JMP O_D");
				if (trigger)
				{
					key = key + (lastDigit * cycleCount);
				}
				else
				{
					trigger = false;
					key++;
				}
            }
			tick++;
			QuickLog("JMP CNT_OB");
		}
		QuickLog("JMP FORK_A");
		var x = 32;
		x -= totalCount;
		if (trigger)
		{
			key++;
			QuickLog("JMP A_TL");
			var str = (cycleCount * lastDigit) + 3;
			for (var i = 0; i < x; i++)
            {
				if (startingValue % str == 0)
                {
					key += str;
					continue;
                }
				str++;
				key += str - 1;
            }
		}
		else
		{
			key -= 10;
			QuickLog("JMP A_FL");
			var afl = (totalCount - lastDigit) + 4;
			for (var i=0;i<x;i++)
            {
				if (startingValue % afl == 0)
                {
					key += afl;
					continue;
                }
				afl++;
				key += afl - i;
            }
		}
		QuickLog("JMP PR_CH");
		var r = Mathf.Abs(key) / 2;
		var flag = false;
		var isPrime = true;
		if (Mathf.Abs(key) == 0 || Mathf.Abs(key) == 1)
        {
			isPrime = false;
        }
		else
        {
			for (var q = 2; q <= r; q++)
            {
				if (Mathf.Abs(key) % q == 0)
				{
					isPrime = false;
					flag = true;
					break;
				}
            }
			if (!flag) isPrime = true;
        }
		if (!isPrime)
        {
			QuickLog("JMP KEY_CMP");
			var b = 1;
			var v = x % (1 + lastDigit);
			v += 5;
			for (var i = 0; i < v; i++)
            {
				b += i;
				key += totalCount % b;
            }
		}
		else
        {
			var b = 2;
			var v = totalCount % (lastDigit + 1);
			v += 2;
			for (var i = 0; i < v; i++)
            {
				b += i;
				key += (x % b);
            }
        }
		QuickLog("JMP FORK_B");
		if (trigger && flag)
        {
			QuickLog("JMP ANS_A");
			if (key % 10 == lastDigit)
				correctNumber = correctNumber * (key - totalCount);
			else if (key % 10 > lastDigit)
				correctNumber = (2 * key) - cycleCount;
			else
				correctNumber = key;
        }
		else if (trigger)
        {
			QuickLog("JMP ANS_B");
			if (key % 10 == lastDigit)
				correctNumber = (key + 11) - totalCount;
			else if (key % 10 > lastDigit)
				correctNumber = key + 2;
			else
				correctNumber = correctNumber + key;
		}
		else if (flag)
        {
			QuickLog("JMP ANS_C");
			if (key % 10 == lastDigit)
				correctNumber = key - correctNumber;
			else if (key % 10 > lastDigit)
				correctNumber = x + lastDigit;
			else
				correctNumber = correctNumber + totalCount;
		}
		else
        {
			QuickLog("JMP ANS_D");
			if (key % 10 == lastDigit)
				correctNumber = correctNumber + totalCount;
			else if (key % 10 > lastDigit)
				correctNumber = key - 21;
			else
				correctNumber = correctNumber * lastDigit;
		}

		QuickLog("JMP LAS");
		correctNumber = Mathf.Abs(correctNumber) % 100;

		expectedNumber = correctNumber;
		currentNumber = startingValue;
		QuickLogFormat("Expected Value: {0}", expectedNumber);
	}
	IEnumerator TypeText(string text)
	{
		for (var x = 0; x < text.Length; x++)
		{
			textDisplay.text = text.Substring(0, x);
			yield return new WaitForSeconds(0.2f);
		}
		textDisplay.text = text;
		isanimating = false;
	}
	void HandleArrowPress(int idx)
    {
		MAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, arrowButtons[idx].transform);
		arrowButtons[idx].AddInteractionPunch(0.25f);
		switch (idx)
        {
			case 0:
			case 2:
                {
					currentNumber += 10 * (downIsAdd ^ idx == 0 ? 1 : -1);
                }
				break;
			case 1:
			case 3:
				{
					var isIncreasing = leftIsAdd ^ idx == 1;
					currentNumber += isIncreasing ? 1 : -1;
					if (isIncreasing && currentNumber % 10 == 0)
						currentNumber -= 10;
					else if (!isIncreasing && currentNumber % 10 == 9)
						currentNumber += 10;
				}
				break;
        }
		currentNumber = ((currentNumber % 100) + 100) % 100;
		textDisplay.text = currentNumber.ToString("00");
	}
	void HandleSubmit()
    {
		MAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, screenSelectable.transform);
		screenSelectable.AddInteractionPunch(0.25f);
		if (currentNumber == expectedNumber)
        {
			moduleSolved = true;
			QuickLogFormat("Submitted correct value. Module disarmed.");
			StartCoroutine(victory());
        }
		else
        {
			QuickLogFormat("{0} was incorrectly submitted. Resetting...", currentNumber);
			modSelf.HandleStrike();
			ResetModule();
			isanimating = true;
			StartCoroutine(TypeText(currentNumber.ToString("00")));
		}
    }
	void HandleColorblindModeToggle()
	{
		colorblindArrowDisplay.gameObject.SetActive(colorblindActive);
	}
	protected override IEnumerator ProcessTwitchCommand(string command)
	{
		if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			screenSelectable.OnInteract();
			if (moduleSolved) yield return "solve"; 
			yield break;
		}

		string[] parameters = command.Split(' ');
		string checks = "";
		for (int j = 0; j < parameters.Length; j++)
		{
			checks += parameters[j];
		}
		var buttonsToPress = new List<KMSelectable>();
		for (int i = 0; i < checks.Length; i++)
		{
			if (checks.ElementAt(i).Equals('u') || checks.ElementAt(i).Equals('U'))
				buttonsToPress.Add(arrowButtons[0]);
			else if (checks.ElementAt(i).Equals('d') || checks.ElementAt(i).Equals('D'))
				buttonsToPress.Add(arrowButtons[2]);
			else if (checks.ElementAt(i).Equals('l') || checks.ElementAt(i).Equals('L'))
				buttonsToPress.Add(arrowButtons[3]);
			else if (checks.ElementAt(i).Equals('r') || checks.ElementAt(i).Equals('R'))
				buttonsToPress.Add(arrowButtons[1]);
			else
				yield break;
		}

		yield return null;
		yield return buttonsToPress;
	}

    protected override IEnumerator TwitchHandleForcedSolve()
    {
		while (currentNumber != expectedNumber)
		{
			yield return null;
			while (currentNumber % 10 != expectedNumber % 10)
			{
				arrowButtons[leftIsAdd ? 3 : 1].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
			while (currentNumber / 10 != expectedNumber / 10)
			{
				arrowButtons[downIsAdd ? 2 : 0].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
		}
		screenSelectable.OnInteract();
		while (isanimating) yield return true;
	}
}
