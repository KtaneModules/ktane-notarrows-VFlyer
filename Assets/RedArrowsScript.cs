﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System;
using System.Text.RegularExpressions;

public class RedArrowsScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;

    public KMSelectable[] buttons;
    public GameObject numDisplay;

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

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        firstMove = false;
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        foreach(KMSelectable obj in buttons){
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
    }

    void Start () {
        numDisplay.GetComponent<TextMesh>().text = " ";
        StartCoroutine(generateNewNum());
    }

    void PressButton(KMSelectable pressed)
    {
        if(moduleSolved != true)
        {
            pressed.AddInteractionPunch(0.25f);
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            if(pressed == buttons[0] && nextPlaceUnsafe("UP"))
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Red Arrows #{0}] A barrier was hit! Module Resetting!", moduleId);
                firstMove = false;
                Start();
            }
            else if (pressed == buttons[1] && nextPlaceUnsafe("DOWN"))
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Red Arrows #{0}] A barrier was hit! Module Resetting!", moduleId);
                firstMove = false;
                Start();
            }
            else if (pressed == buttons[2] && nextPlaceUnsafe("LEFT"))
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Red Arrows #{0}] A barrier was hit! Module Resetting!", moduleId);
                firstMove = false;
                Start();
            }
            else if (pressed == buttons[3] && nextPlaceUnsafe("RIGHT"))
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
                }else if (pressed == buttons[1])
                {
                    current += 42;
                }else if (pressed == buttons[2])
                {
                    current -= 2;
                }else if (pressed == buttons[3])
                {
                    current += 2;
                }
                if ((""+maze[current]).Equals(finish+""))
                {
                    StartCoroutine(victory());
                    Debug.LogFormat("[Red Arrows #{0}] Successfully reached the end of the maze! Module Disarmed!", moduleId);
                }
                if(firstMove == false)
                {
                    firstMove = true;
                    numDisplay.GetComponent<TextMesh>().text = " ";
                }
            }
        }
    }

    private IEnumerator generateNewNum()
    {
        yield return null;
        int check = 0;
        int rando = 0;
        while (rando == check)
        {
            rando = UnityEngine.Random.RandomRange(0, 10);
            int.TryParse(bomb.GetSerialNumber().Substring(5, 1), out check);
        }
        yield return new WaitForSeconds(0.5f);
        numDisplay.GetComponent<TextMesh>().text = "" + rando;
        StopCoroutine("generateNewNum");
        start = rando;
        current = maze.IndexOf(""+start);
        int.TryParse(bomb.GetSerialNumber().Substring(5, 1), out finish);
        Debug.LogFormat("[Red Arrows #{0}] The start has been set to point '{1}'! The finish has been set to point '{2}'!", moduleId, start, finish);
    }

    private bool nextPlaceUnsafe(string check)
    {
        if(check.Equals("UP"))
        {
            char imp = maze[current - 21];
            if(imp.Equals('-') || imp.Equals('|'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }else if (check.Equals("DOWN"))
        {
            char imp = maze[current + 21];
            if (imp.Equals('-') || imp.Equals('|'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }else if (check.Equals("LEFT"))
        {
            char imp = maze[current - 1];
            if (imp.Equals('-') || imp.Equals('|'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }else if (check.Equals("RIGHT"))
        {
            char imp = maze[current + 1];
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
        yield return null;
        for(int i = 0; i < 100; i++)
        {
            int rand1 = UnityEngine.Random.RandomRange(0, 10);
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
        StopCoroutine("victory");
        GetComponent<KMBombModule>().HandlePass();
        moduleSolved = true;
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} up [Presses the up arrow button] | !{0} right [Presses the right arrow button] | !{0} down [Presses the down arrow button once] | !{0} left [Presses the left arrow button once] | !{0} left right down up [Chain button presses] | !{0} reset [Resets the module back to the start] | Movement words can be substituted as one letter (Ex. right as r)";
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
            Debug.LogFormat("[Red Arrows #{0}] Module Reset back to starting position!", moduleId);
            yield break;
        }

        string[] parameters = command.Split(' ');
        var buttonsToPress = new List<KMSelectable>();
        foreach (string param in parameters)
        {
            if (param.EqualsIgnoreCase("up") || param.EqualsIgnoreCase("u"))
                buttonsToPress.Add(buttons[0]);
            else if (param.EqualsIgnoreCase("down") || param.EqualsIgnoreCase("d"))
                buttonsToPress.Add(buttons[1]);
            else if (param.EqualsIgnoreCase("left") || param.EqualsIgnoreCase("l"))
                buttonsToPress.Add(buttons[2]);
            else if (param.EqualsIgnoreCase("right") || param.EqualsIgnoreCase("r"))
                buttonsToPress.Add(buttons[3]);
            else
                yield break;
        }

        yield return null;
        yield return buttonsToPress;
    }
}
