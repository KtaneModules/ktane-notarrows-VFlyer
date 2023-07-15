using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class RedArrowsScript : MonoBehaviour
{

    public KMAudio audio;
    public KMBombInfo bomb;
    public KMColorblindMode Colorblind;

    public KMSelectable[] buttons;
    public GameObject numDisplay;
    public GameObject colorblindText;

    private string maze = "---------------------" +
                          "|o+o+o+o|o|o+o+o+o+0|" +
                          "|+---+-+-+-+---+----|" +
                          "|o|o+o|o+o|o+o|o+o+o|" +
                          "|+-+-----+-+-+-----+|" +
                          "|1|o|o+4|o+o|o|o+o|6|" +
                          "|--+-+---+-----+-+--|" +
                          "|o+o|o+o+o+o+o+o|o+o|" +
                          "|+---------+-------+|" +
                          "|o+9|o+o+o|o|o+o+3|o|" +
                          "|----+---+-+-+-----+|" +
                          "|o+o|o|5|o+o|o+o+o+o|" +
                          "|+-+-+-+-----------+|" +
                          "|o|o+o|o+o+o+o|7+o+o|" +
                          "|+-----------+-----+|" +
                          "|o|o+o+o|o+8|o+o+o|o|" +
                          "|+-+---+-+-------+-+|" +
                          "|o|o|o+o|o+o+o+o|o+o|" +
                          "|+-+-+---------+---+|" +
                          "|o+o|o+o+o+2|o+o+o+o|" +
                          "---------------------";
    private int start;
    private int finish;
    private int current;

    private bool firstMove;
    private bool colorblindActive = false;
    private bool isanimating = true;
    private bool activated = false;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        firstMove = false;
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        colorblindActive = Colorblind.ColorblindModeActive;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void Start()
    {
        numDisplay.GetComponent<TextMesh>().text = " ";
        if (activated)
            StartCoroutine(generateNewNum());
    }

    void OnActivate()
    {
        StartCoroutine(generateNewNum());
        if (colorblindActive)
            colorblindText.SetActive(true);
        activated = true;
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true && isanimating != true)
        {
            pressed.AddInteractionPunch(0.25f);
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
            if (pressed == buttons[0] && nextPlaceUnsafe("UP", current))
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Red Arrows #{0}] A barrier was hit! Module Resetting!", moduleId);
                firstMove = false;
                Start();
            }
            else if (pressed == buttons[1] && nextPlaceUnsafe("DOWN", current))
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Red Arrows #{0}] A barrier was hit! Module Resetting!", moduleId);
                firstMove = false;
                Start();
            }
            else if (pressed == buttons[2] && nextPlaceUnsafe("LEFT", current))
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Red Arrows #{0}] A barrier was hit! Module Resetting!", moduleId);
                firstMove = false;
                Start();
            }
            else if (pressed == buttons[3] && nextPlaceUnsafe("RIGHT", current))
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Red Arrows #{0}] A barrier was hit! Module Resetting!", moduleId);
                firstMove = false;
                Start();
            }
            else
            {
                if (pressed == buttons[0])
                {
                    current -= 42;
                }
                else if (pressed == buttons[1])
                {
                    current += 42;
                }
                else if (pressed == buttons[2])
                {
                    current -= 2;
                }
                else if (pressed == buttons[3])
                {
                    current += 2;
                }
                if (("" + maze[current]).Equals(finish + ""))
                {
                    moduleSolved = true;
                    StartCoroutine(victory());
                    Debug.LogFormat("[Red Arrows #{0}] Successfully reached the end of the maze! Module Disarmed!", moduleId);
                }
                if (firstMove == false)
                {
                    firstMove = true;
                    numDisplay.GetComponent<TextMesh>().text = " ";
                }
            }
        }
    }

    private IEnumerator generateNewNum()
    {
        isanimating = true;
        int check = 0;
        int rando = 0;
        while (rando == check)
        {
            rando = Random.Range(0, 10);
            int.TryParse(bomb.GetSerialNumber().Substring(5, 1), out check);
        }
        yield return new WaitForSeconds(0.5f);
        numDisplay.GetComponent<TextMesh>().text = "" + rando;
        start = rando;
        current = maze.IndexOf("" + start);
        int.TryParse(bomb.GetSerialNumber().Substring(5, 1), out finish);
        Debug.LogFormat("[Red Arrows #{0}] The start has been set to point '{1}'! The finish has been set to point '{2}'!", moduleId, start, finish);
        isanimating = false;
    }

    private bool nextPlaceUnsafe(string check, int pos)
    {
        if (check.Equals("UP"))
        {
            char imp = maze[pos - 21];
            if (imp.Equals('-') || imp.Equals('|'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (check.Equals("DOWN"))
        {
            char imp = maze[pos + 21];
            if (imp.Equals('-') || imp.Equals('|'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (check.Equals("LEFT"))
        {
            char imp = maze[pos - 1];
            if (imp.Equals('-') || imp.Equals('|'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (check.Equals("RIGHT"))
        {
            char imp = maze[pos + 1];
            if (imp.Equals('-') || imp.Equals('|'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private IEnumerator victory()
    {
        isanimating = true;
        for (int i = 0; i < 100; i++)
        {
            int rand1 = Random.Range(0, 10);
            if (i < 50)
            {
                numDisplay.GetComponent<TextMesh>().text = rand1 + "";
            }
            else
            {
                numDisplay.GetComponent<TextMesh>().text = "G" + rand1;
            }
            yield return new WaitForSeconds(0.025f);
        }
        numDisplay.GetComponent<TextMesh>().text = "GG";
        isanimating = false;
        GetComponent<KMBombModule>().HandlePass();
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} u/d/l/r [Presses the specified arrow button] | !{0} reset [Resets the module back to the start] | Presses can be chained, for example '!{0} uuddlrl'";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*reset\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            numDisplay.GetComponent<TextMesh>().text = " ";
            yield return new WaitForSeconds(0.5f);
            current = maze.IndexOf("" + start);
            numDisplay.GetComponent<TextMesh>().text = "" + start;
            Debug.LogFormat("[Red Arrows #{0}] Module Reset back to starting position! (TP)", moduleId);
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
                buttonsToPress.Add(buttons[0]);
            else if (checks.ElementAt(i).Equals('d') || checks.ElementAt(i).Equals('D'))
                buttonsToPress.Add(buttons[1]);
            else if (checks.ElementAt(i).Equals('l') || checks.ElementAt(i).Equals('L'))
                buttonsToPress.Add(buttons[2]);
            else if (checks.ElementAt(i).Equals('r') || checks.ElementAt(i).Equals('R'))
                buttonsToPress.Add(buttons[3]);
            else
                yield break;
        }

        yield return null;
        yield return buttonsToPress;
        if (moduleSolved) { yield return "solve"; }
    }

    struct QueueItem
    {
        public int Cell;
        public int Parent;
        public int Direction;
        public QueueItem(int cell, int parent, int dir)
        {
            Cell = cell;
            Parent = parent;
            Direction = dir;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (!moduleSolved)
        {
            while (isanimating)
                yield return true;
            var visited = new Dictionary<int, QueueItem>();
            var q = new Queue<QueueItem>();
            var sol = maze.IndexOf(finish.ToString()[0]);
            q.Enqueue(new QueueItem(current, -1, 0));
            while (q.Count > 0)
            {
                var qi = q.Dequeue();
                if (visited.ContainsKey(qi.Cell))
                    continue;
                visited[qi.Cell] = qi;
                if (qi.Cell == sol)
                    break;
                if (!nextPlaceUnsafe("UP", qi.Cell))
                    q.Enqueue(new QueueItem(qi.Cell - 42, qi.Cell, 0));
                if (!nextPlaceUnsafe("DOWN", qi.Cell))
                    q.Enqueue(new QueueItem(qi.Cell + 42, qi.Cell, 1));
                if (!nextPlaceUnsafe("LEFT", qi.Cell))
                    q.Enqueue(new QueueItem(qi.Cell - 2, qi.Cell, 2));
                if (!nextPlaceUnsafe("RIGHT", qi.Cell))
                    q.Enqueue(new QueueItem(qi.Cell + 2, qi.Cell, 3));
            }
            var r = sol;
            var path = new List<int>();
            while (true)
            {
                var nr = visited[r];
                if (nr.Parent == -1)
                    break;
                path.Add(nr.Direction);
                r = nr.Parent;
            }
            for (int i = path.Count - 1; i >= 0; i--)
            {
                buttons[path[i]].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
        while (isanimating)
            yield return true;
    }
}