using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KModkit;
using System.Text.RegularExpressions;

public class NotGreenArrowsScript : BaseArrowsScript {

	public KMRuleSeedable ruleSeed;
	public KMBombInfo bombInfo;
	public TextMesh bigTxtMesh;
	string displayedLetters;
	static int modIDCnt;

	readonly static int[] primeNumbers = new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };
	readonly static int[][] pairGroupIdxes0 = new[] { new[] { 0, 1 }, new[] { 0, 2 }, new[] { 0, 3 } };
	readonly static string[] arrowDirectionNames = new[] { "Up", "Right", "Down", "Left", };
	const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", vowels = "AEIOU";
	Condition[][] allConditionsUsed;
	bool[][] conditionInverted;
	Dictionary<string, int[]> referenceValues = new Dictionary<string, int[]> {
		{ "prime", primeNumbers },
		{ "square", new[] { 0, 1, 4, 9, 16, 25, 36, 49, 64, 81, 100 } },
	};
	int[] trueCountsAll;
	IEnumerable<bool> isArrowSafe;

	public enum ConditionType
    {
		None,
		DisplayedLettersAny,
		DisplayedLettersRegex,
		DisplayedLetterAlphaSumAny,
		EdgeworkSerialNumberAny,
		EdgeworkIndicatorsAny,
		EdgeworkIndicatorsRegex,
		GenericAny,
		Custom
	}
	public class Condition
	{
		public ConditionType conditionStored;
		public string paramStored;
			
		public Condition(ConditionType newType, string param)
		{
			conditionStored = newType;
			paramStored = param;
		}
		public Condition()
		{
			conditionStored = ConditionType.None;
			paramStored = "";
		}
	};
	protected override void QuickLogFormat(string toLog = "", params object[] misc)
	{
		QuickLog(string.Format(toLog, misc));
	}
	protected override void QuickLog(string toLog = "")
	{
		Debug.LogFormat("[Not Green Arrows #{0}] {1}", moduleId, toLog);
	}
    protected override void QuickLogDebugFormat(string toLog = "", params object[] misc)
    {
		QuickLogDebug(string.Format(toLog, misc));
	}
    protected override void QuickLogDebug(string toLog = "")
    {
		Debug.LogFormat("<Not Green Arrows #{0}> {1}", moduleId, toLog);
	}

    bool CheckCondition(ConditionType condition, params string[] args)
    {
		switch (condition)
        {
			default:
			case ConditionType.None:
				return false;
			case ConditionType.DisplayedLettersAny:
				return displayedLetters.Any(a => args.First().Contains(a));
			case ConditionType.DisplayedLettersRegex:
				return Regex.IsMatch(displayedLetters, args.First());
			case ConditionType.DisplayedLetterAlphaSumAny:
				return args.Contains(displayedLetters.Select(a => 1 + alphabet.IndexOf(a)).Sum().ToString());
			case ConditionType.EdgeworkIndicatorsAny:
				return bombInfo.GetIndicators().Any(a => a.Any(b => args.First().Contains(a)));
			case ConditionType.EdgeworkIndicatorsRegex:
				return bombInfo.GetIndicators().Any(a => Regex.IsMatch(a, args.First()));
			case ConditionType.GenericAny:
				return args.Skip(1).Contains(args.First());
		}
    }
	void HandleRuleSeed()
	{
		var DRDivisOrigItems = new List<int>();
		for (var x = 3; x <= 78; x++)
			if (x % ((x - 1) % 9 + 1) == 0)
				DRDivisOrigItems.Add(x * 1);
		referenceValues.Add("DRDivisOrig", DRDivisOrigItems.ToArray());
		allConditionsUsed = new Condition[4][];
		conditionInverted = new bool[4][];
		var rng = ruleSeed == null ? new MonoRandom(1) : ruleSeed.GetRNG();
		if (rng.Seed == 1)
        {
			// Conditions for the Up arrow
			allConditionsUsed[0] = new Condition[] {
				new Condition(ConditionType.DisplayedLettersRegex, string.Format(@"[{0}][{1}][{2}]", vowels, alphabet, alphabet)), // First letter is a vowel
				new Condition(ConditionType.DisplayedLettersRegex, string.Format(@"[{0}][{1}]{2}", vowels, alphabet, "{2}")),
				new Condition(ConditionType.DisplayedLettersRegex, string.Format(@"[{0}][{1}]{2}", vowels, alphabet, "{2}")),
				new Condition(ConditionType.DisplayedLettersRegex, string.Format(@"[{0}][{1}]{2}", vowels, alphabet, "{2}")),
				new Condition(ConditionType.DisplayedLettersAny, "U"),
				new Condition(ConditionType.DisplayedLettersAny, "P"),
				new Condition(ConditionType.DisplayedLettersRegex, string.Format(@"[{0}][{1}]{2}", vowels, alphabet, "{2}")),
				new Condition(ConditionType.DisplayedLettersRegex, string.Format(@"[{0}][{1}][{2}]", "ABCDEFGHIJKLMNOPQSTUVWXYZ", alphabet, alphabet)),
				new Condition(ConditionType.EdgeworkIndicatorsRegex, string.Format(@"[{0}][{1}][{2}]", 'S', alphabet, alphabet)),
			};
			conditionInverted[0] = new[] { false, false, false, false, false, true, false, false, false };
			return;
        }
		if (ruleSeed != null)
			QuickLogFormat("Module generated using rule seed {0}.", rng.Seed);
		else
			QuickLogFormat("Rule Seed Handler does not exist. Default rules required.");
	}

	// Use this for initialization
	void Start () {
		moduleId = ++modIDCnt;
		HandleRuleSeed();
		ResetModule();
		modSelf.OnActivate += delegate { StartCoroutine(TypeText(displayedLetters)); };
		textDisplay.text = "";
		bigTxtMesh.text = "";

		for (var x = 0; x < 4; x++)
        {
			var y = x;
			arrowButtons[x].OnInteract += delegate { if (!(moduleSolved || isanimating)) HandleArrowIdxPress(y); return false; };
        }
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
	IEnumerator TypeText(string txt)
    {
		for (var x = 0; x < txt.Length; x++)
        {
			textDisplay.text = txt.Substring(0, x);
			yield return new WaitForSeconds(0.2f);
        }
		textDisplay.text = txt;
		isanimating = false;
    }
	void HandleDefaultRules()
    {
		// I am sorry.
		var sumAlphaPosDisplayLetters = displayedLetters.Select(a => 1 + alphabet.IndexOf(a)).Sum();
		var trueConditionsUpArrow = new[] {
			vowels.Contains(displayedLetters[0]),
			primeNumbers.Contains(sumAlphaPosDisplayLetters),
			displayedLetters.Distinct().Count() == 3,
			bombInfo.GetSerialNumberLetters().Contains(displayedLetters[1]),
			displayedLetters.Contains('U'),
			!displayedLetters.Contains('P'),
			displayedLetters[2] < displayedLetters[1],
			displayedLetters[0] != 'R',
			bombInfo.GetIndicators().Any(a => a.StartsWith("S"))
		};
		var trueConditionsDownArrow = new[] {
			vowels.Contains(displayedLetters[1]),
			sumAlphaPosDisplayLetters % ((sumAlphaPosDisplayLetters - 1) % 9 + 1) == 0,
			displayedLetters[0] == displayedLetters[1],
			!bombInfo.GetSerialNumberLetters().Any(a => displayedLetters.Contains(a)),
			displayedLetters.Contains('D'),
			!displayedLetters.Contains('W'),
			displayedLetters[1] < displayedLetters[0],
			displayedLetters[0] != 'V',
			bombInfo.GetIndicators().Any(a => a.StartsWith("F"))
		};
		var trueConditionsLeftArrow = new[] {
			!displayedLetters.Any(a => vowels.Contains(a)),
			sumAlphaPosDisplayLetters % 2 == 1,
			displayedLetters[0] == displayedLetters[2],
			bombInfo.GetSerialNumberLetters().Contains(displayedLetters[2]),
			displayedLetters.Contains('L'),
			!displayedLetters.Contains('F'),
			displayedLetters[0] < displayedLetters[1],
			displayedLetters[0] != 'O',
			bombInfo.GetIndicators().Any(a => a.StartsWith("C"))
		};
		var trueConditionsRightArrow = new[] {
			vowels.Contains(displayedLetters[2]),
			sumAlphaPosDisplayLetters % 7 == 0,
			displayedLetters[1] == displayedLetters[2],
			bombInfo.GetSerialNumberLetters().Contains(displayedLetters[0]),
			displayedLetters.Contains('R'),
			!displayedLetters.Contains('G'),
			displayedLetters[0] < displayedLetters[2],
			displayedLetters[0] != 'Z',
			bombInfo.GetIndicators().Any(a => a.StartsWith("N"))
		};
		trueCountsAll = new[] {
			trueConditionsUpArrow.Count(a => a),
			trueConditionsRightArrow.Count(a => a),
			trueConditionsDownArrow.Count(a => a),
			trueConditionsLeftArrow.Count(a => a),
		};
		QuickLogFormat("True conditions for the up arrow: {0}", Enumerable.Range(0, trueConditionsUpArrow.Length).Where(a => trueConditionsUpArrow[a]).Select(a => a + 1).Join());
		QuickLogFormat("True conditions for the down arrow: {0}", Enumerable.Range(0, trueConditionsDownArrow.Length).Where(a => trueConditionsDownArrow[a]).Select(a => a + 1).Join());
		QuickLogFormat("True conditions for the right arrow: {0}", Enumerable.Range(0, trueConditionsRightArrow.Length).Where(a => trueConditionsRightArrow[a]).Select(a => a + 1).Join());
		QuickLogFormat("True conditions for the left arrow: {0}", Enumerable.Range(0, trueConditionsLeftArrow.Length).Where(a => trueConditionsLeftArrow[a]).Select(a => a + 1).Join());
    }
    void ResetModule()
    {
		displayedLetters = "";
		for (var x = 0; x < 3; x++)
			displayedLetters += alphabet.PickRandom();
		QuickLogFormat("Displayed letters: {0}", displayedLetters);
		var lettersSerialNo = bombInfo.GetSerialNumberLetters().Join("");
		if (ruleSeed == null || ruleSeed.GetRNG().Seed == 1)
			HandleDefaultRules();
		else
        {
			// RS section begins here. WIP.
        }
		var sortedTrueConditionsCnt = trueCountsAll.OrderBy(a => a);
		QuickLogDebugFormat("Sorted true condition counts: {0}", sortedTrueConditionsCnt.Join());
		if (trueCountsAll.Distinct().Count() == 1)
		{
			QuickLog("All four arrows have the same number of true conditions.");
			isArrowSafe = Enumerable.Repeat(true, 4);
		}
		else if (Enumerable.Range(0, 4).Any(a => Enumerable.Range(0, 4).Where(b => b != a).Select(b => trueCountsAll[b]).Distinct().Count() == 1))
		{
			QuickLog("Three arrows have the same number of true conditions.");
			var idxNotSame = Enumerable.Range(0, 4).Single(a => Enumerable.Range(0, 4).Where(b => b != a).Select(b => trueCountsAll[b]).Distinct().Count() == 1);
			isArrowSafe = Enumerable.Range(0, 4).Select(a => a == idxNotSame).ToArray();
		}
		else if (pairGroupIdxes0.Any(a => a.Select(b => trueCountsAll[b]).Distinct().Count() == 1 && Enumerable.Range(0, 4).Except(a).Select(b => trueCountsAll[b]).Distinct().Count() == 1))
		{
			QuickLog("There are two pairs of arrows that have the same number of true conditions.");
			isArrowSafe = Enumerable.Range(0, 4).Select(a => trueCountsAll[a] >= trueCountsAll.Max()).ToArray();
		}
		else if (pairGroupIdxes0.Any(a => a.Select(b => trueCountsAll[b]).Distinct().Count() == 1 || Enumerable.Range(0, 4).Except(a).Select(b => trueCountsAll[b]).Distinct().Count() == 1))
        {
			QuickLog("There is a pair of arrows that have the same number of true conditions.");
			var pairIdxes = pairGroupIdxes0.Concat(pairGroupIdxes0.Select(a => Enumerable.Range(0, 4).Except(a))).Single(a => a.Select(b => trueCountsAll[b]).Distinct().Count() == 1);
			var nonPairedIdxes = Enumerable.Range(0, 4).Except(pairIdxes);
			var selectedIdxSafe = trueCountsAll[nonPairedIdxes.First()] < trueCountsAll[nonPairedIdxes.Last()] ? nonPairedIdxes.First() : nonPairedIdxes.Last();
			isArrowSafe = Enumerable.Range(0, 4).Select(a => a == selectedIdxSafe).ToArray();
		}
		else if (Enumerable.Range(0, 3).All(a => sortedTrueConditionsCnt.ElementAt(a) + 1 == sortedTrueConditionsCnt.ElementAt(a + 1)))
		{
			QuickLog("All four arrows have a consectutive number of true conditions.");
			isArrowSafe = trueCountsAll.Select(a => a <= sortedTrueConditionsCnt.Min()).ToArray();
		}
		else if (Enumerable.Range(sortedTrueConditionsCnt.ElementAt(1), 3).SequenceEqual(sortedTrueConditionsCnt.Skip(1)) || Enumerable.Range(sortedTrueConditionsCnt.First(), 3).SequenceEqual(sortedTrueConditionsCnt.Take(3)))
		{
			QuickLog("Three arrows have a consectutive number of true conditions.");
			var idxNotConsectutive = -1;
			for (var x = 0; x < 4; x++)
            {
				var ommitedSortedSequence = Enumerable.Range(0, 4).Where(a => a != x).Select(a => trueCountsAll[a]).OrderBy(a => a);
				if (Enumerable.Range(0, 2).All(a => ommitedSortedSequence.ElementAt(a) + 1 == ommitedSortedSequence.ElementAt(a + 1)))
					idxNotConsectutive = x;
            }
			isArrowSafe = Enumerable.Range(0, 4).Select(a => a == idxNotConsectutive).ToArray();
		}
		else
        {
			QuickLog("No other conditions applied.");
			isArrowSafe = Enumerable.Range(0, 4).Select(a => trueCountsAll[a] >= trueCountsAll.Max()).ToArray();
		}
		QuickLogFormat("Applicable arrows to press: {0}", !Enumerable.Range(0, 4).Any(a => isArrowSafe.ElementAt(a)) ? "<none>" : Enumerable.Range(0, 4).Where(a => isArrowSafe.ElementAt(a)).Select(a => "URDL"[a]).Join(", "));
	}
	void HandleArrowIdxPress(int idx)
	{
		MAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, arrowButtons[idx].transform);
		arrowButtons[idx].AddInteractionPunch(0.25f);
		if (isArrowSafe.ElementAt(idx) || isArrowSafe.All(a => !a))
		{
			if (isArrowSafe.All(a => !a))
				QuickLogFormat("There were no safe arrows to press, but I'll accept {0} for now. Just be sure to report an issue in case this happens again.", arrowDirectionNames[idx]);
			else
				QuickLogFormat("{0} was safe to press. Module disarmed.", arrowDirectionNames[idx]);
			moduleSolved = true;
			isanimating = true;
			StartCoroutine(victory());
		}
		else
        {
			
			QuickLogFormat("{0} was not safe to press! Resetting.", arrowDirectionNames[idx]);
			isanimating = true;
			modSelf.HandleStrike();
			ResetModule();
			StartCoroutine(TypeText(displayedLetters));
		}
    }
	void HandleColorblindToggle()
	{
		colorblindArrowDisplay.gameObject.SetActive(colorblindActive);
	}
	protected override IEnumerator victory()
	{
		isanimating = true;
		for (int i = 0; i < 100; i++)
		{
			var rand2 = alphabet.PickRandom();	
			if (i < 25)
			{
				var rand1 = alphabet.PickRandom();
				var rand3 = alphabet.PickRandom();
				textDisplay.text = rand1 + "" + rand2 + "" + rand3;
			}
			else if (i < 50)
			{
				textDisplay.text = "";
				var rand1 = alphabet.PickRandom();
				bigTxtMesh.text = rand1 + "" + rand2;
			}
			else
			{
				bigTxtMesh.text = "G" + rand2;
			}
			yield return new WaitForSeconds(0.025f);
		}
		bigTxtMesh.text = "GG";
		isanimating = false;
		modSelf.HandlePass();
	}
    protected override IEnumerator TwitchHandleForcedSolve()
    {
		if (isArrowSafe.Any(a => a))
			arrowButtons[Enumerable.Range(0, 4).Where(a => isArrowSafe.ElementAt(a)).PickRandom()].OnInteract();
		else
			arrowButtons.PickRandom().OnInteract();
		while (isanimating) yield return true;
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
