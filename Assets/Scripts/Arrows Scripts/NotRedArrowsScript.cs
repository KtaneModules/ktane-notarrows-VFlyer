using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;
using System.Linq;

public class NotRedArrowsScript : BaseArrowsScript {
	public KMSelectable screenSelectable;
	public KMBombInfo bombInfo;

	static int modIDCnt;

	byte expectedNumber, currentNumber;
	// Use this for initialization
	void Start () {
		moduleId = ++modIDCnt;
	}
    protected override void QuickLogFormat(string toLog = "", params object[] misc)
    {
		Debug.LogFormat("[Not Red Arrows #{0}] {1}", moduleId, string.Format(toLog, misc));
    }
    protected override void QuickLog(string toLog = "")
    {
		Debug.LogFormat("[Not Red Arrows #{0}] {1}", moduleId, toLog);
    }

    void ResetModule()
    {
		var startingValue = Random.Range(0, 100);
		var key = startingValue;
		var lastDigit = bombInfo.GetSerialNumberNumbers().LastOrDefault();
		var correctNumber = 11;
		var tick = 0;
		var trigger = true;
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
		var n = key;
		var cycleCount = 0;
		while (n > 0)
        {
			var a = n % 2;
			if (a == 1)
				cycleCount++;
			n /= 2;
        }
    }
	void HandleColorblindModeToggle()
	{
		colorblindArrowDisplay.gameObject.SetActive(colorblindActive);
	}
}
