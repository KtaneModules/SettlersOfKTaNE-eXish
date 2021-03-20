using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using KModkit;

public class SettlersOfKTaNEScript : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;

    public KMSelectable BrickDisp, LumberDisp, OreDisp, GrainDisp, WoolDisp;
    public KMSelectable LeftDice, RightDice;
    public KMSelectable Anchor;
    public KMSelectable[] cylinders;
    public KMSelectable[] Hexas = new KMSelectable[7];
    public KMSelectable[] Streets;
    public KMSelectable NosolveMode;

    public GameObject Se1, Se2, Se3, Se4, Se5, Ci1, Ci2, Ci3, Ci4;
    public GameObject BrickNo, LumberNo, OreNo, GrainNo, WoolNo;
    public GameObject[] PosSaves = new GameObject[5];
    public GameObject Statuslight;

    public int[] diceRolls = { 2, 3, 3, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 9, 9, 9, 9, 10, 10, 10, 11, 11, 12 };
    public int diceroll;
    public int counter = 0;
    public int discardableRes = 0;

    public Material GrainHex, OreHex, WoodHex, WoolHex, BrickHex;
    public Material[] figurColors = new Material[4];
    public String[] figurColorNames = { "white", "red", "orange", "blue" };

    //logging
    static int moduleIdCounter = 1;
    int moduleId = 0;
    private bool isSolved = false;

    //Debug.LogFormat("[Settlers Of KTaNE #{0}] text", moduleId, );

    //Game variables
    int brick = 0;
    int wood = 0;
    int ore = 0;
    int grain = 0;
    int wool = 0;

    int points = 0;

    bool firstHouse = true;
    bool firstPath = true;
    bool trade = false;
    bool thiefMode = false;
    bool longStreetDetected = false;
    bool[] pressedStreets = new bool[30];
    Settlement[] settlements = new Settlement[75];
    int[] settlementNo = { 2, 4, 11, 13, 15, 21, 23, 25, 30, 32, 34, 36, 40, 42, 44, 46, 51, 53, 55, 61, 63, 65, 72, 74 };
    GameObject[] houses = new GameObject[5];
    GameObject[] cities = new GameObject[4];
    int[][] streetConnections = new int[30][];
    Hex[] theHexes = new Hex[7];
    bool IsNoSolveMode = false;
    String CommandQueue = "";

    // Use this for initializations


    void Start()
    {
        brick = UnityEngine.Random.Range(0, 5);
        wood = UnityEngine.Random.Range(0, 5);
        ore = UnityEngine.Random.Range(0, 5);
        grain = UnityEngine.Random.Range(0, 5);
        wool = UnityEngine.Random.Range(0, 5);
        for (int i = 0; i < settlementNo.Length; i++)
        {
            settlements[settlementNo[i]] = new Settlement(settlementNo[i], cylinders[i]);
        }
        houses[0] = Se1;
        houses[1] = Se2;
        houses[2] = Se3;
        houses[3] = Se4;
        houses[4] = Se5;
        cities[0] = Ci1;
        cities[1] = Ci2;
        cities[2] = Ci3;
        cities[3] = Ci4;
        UpdateMats();
        shuffleArray();
        rotateLeftDice(UnityEngine.Random.Range(1, 7));
        rotateRightDice(UnityEngine.Random.Range(1, 7));
        streetConnections[0] = new int[] { 2, 11 };
        streetConnections[1] = new int[] { 2, 13 };
        streetConnections[2] = new int[] { 13, 4 };
        streetConnections[3] = new int[] { 4, 15 };
        streetConnections[4] = new int[] { 11, 21 };
        streetConnections[5] = new int[] { 13, 23 };
        streetConnections[6] = new int[] { 15, 25 };
        streetConnections[7] = new int[] { 30, 21 };
        streetConnections[8] = new int[] { 21, 32 };
        streetConnections[9] = new int[] { 32, 23 };
        streetConnections[10] = new int[] { 23, 34 };
        streetConnections[11] = new int[] { 25, 34 };
        streetConnections[12] = new int[] { 25, 36 };
        streetConnections[13] = new int[] { 40, 30 };
        streetConnections[14] = new int[] { 32, 42 };
        streetConnections[15] = new int[] { 34, 44 };
        streetConnections[16] = new int[] { 36, 46 };
        streetConnections[17] = new int[] { 40, 51 };
        streetConnections[18] = new int[] { 51, 42 };
        streetConnections[19] = new int[] { 42, 53 };
        streetConnections[20] = new int[] { 53, 44 };
        streetConnections[21] = new int[] { 44, 55 };
        streetConnections[22] = new int[] { 55, 46 };
        streetConnections[23] = new int[] { 51, 61 };
        streetConnections[24] = new int[] { 53, 63 };
        streetConnections[25] = new int[] { 55, 65 };
        streetConnections[26] = new int[] { 61, 72 };
        streetConnections[27] = new int[] { 72, 63 };
        streetConnections[28] = new int[] { 63, 74 };
        streetConnections[29] = new int[] { 74, 65 };
        SetfigurColor();
        Init();
    }

    void Awake()
    {
        foreach (KMSelectable street in Streets)
        {
            street.OnInteract += delegate () { PressStreet(street); return false; };
        }

        Hexas[0].OnInteract += delegate () { PressHexa(0); return false; };
        Hexas[1].OnInteract += delegate () { PressHexa(1); return false; };
        Hexas[2].OnInteract += delegate () { PressHexa(2); return false; };
        Hexas[3].OnInteract += delegate () { PressHexa(3); return false; };
        Hexas[4].OnInteract += delegate () { PressHexa(4); return false; };
        Hexas[5].OnInteract += delegate () { PressHexa(5); return false; };
        Hexas[6].OnInteract += delegate () { PressHexa(6); return false; };
        moduleId = moduleIdCounter++;
        BrickDisp.OnInteract += delegate () { PressBrickDisp(); return false; };
        LumberDisp.OnInteract += delegate () { PressLumberDisp(); return false; };
        OreDisp.OnInteract += delegate () { PressOreDisp(); return false; };
        GrainDisp.OnInteract += delegate () { PressGrainDisp(); return false; };
        WoolDisp.OnInteract += delegate () { PressWoolDisp(); return false; };
        Anchor.OnInteract += delegate () { PressAnchor(); return false; };
        LeftDice.OnInteract += delegate () { PressDice(); return false; };
        RightDice.OnInteract += delegate () { PressDice(); return false; };
        foreach (KMSelectable cylinder in cylinders)
        {
            cylinder.OnInteract += delegate () { PressCylinder(cylinder); return false; };
        }
        NosolveMode.OnInteract += delegate () { PressNoSolveMode(NosolveMode); return false; };

    }

    void SetfigurColor()
    {
        int color = UnityEngine.Random.Range(0, 3);
        Debug.LogFormat("[Settlers Of KTaNE #{0}] Chosen color is {1}", moduleId, figurColorNames[color]);
        for (int i = 0; i < 5; i++)
        {
            houses[i].GetComponent<MeshRenderer>().material = figurColors[color];
        }
        for (int i = 0; i < 4; i++)
        {
            cities[i].GetComponent<MeshRenderer>().material = figurColors[color];
        }
        for (int i = 0; i < Streets.Length; i++)
        {
            Streets[i].GetComponent<MeshRenderer>().material = figurColors[color];
        }
    }

    void PressNoSolveMode(KMSelectable press)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, press.transform);
        press.AddInteractionPunch();
        IsNoSolveMode = !IsNoSolveMode;
        Finished();
    }

    void PressHexa(int z)
    {
        Hex temp = theHexes[z];
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, temp.Position.transform);
        temp.Position.AddInteractionPunch();
        if (temp.claimable > 0)
        {
            giveMats(temp.material);
            temp.claimable--;
            UpdateMats();
            Debug.LogFormat("[Settlers Of KTaNE #{0}] {1} claimed.", moduleId, getFullName(temp.material));
        }
        else
        {
            Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: No claimable resource.", moduleId);
            Strike(Hexas[z]);
        }
    }

    void PressAnchor()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Anchor.transform);
        Anchor.AddInteractionPunch();
        trade = true;
    }

    int getSettNobyKMSelectable(KMSelectable x)
    {
        for (int i = 0; i < settlements.Length; i++)
        {
            if (settlements[i] != null)
            {
                if (settlements[i].SelectObject == x)
                {
                    return i;
                }
            }
        }
        return 0;
    }

    void PressCylinder(KMSelectable cylinder)
    {
        int x = getSettNobyKMSelectable(cylinder);

        cylinder.AddInteractionPunch();
        if (firstHouse)
        {
            getNextHouseObject().transform.position = cylinder.transform.position;
            getSettlementById(settlements, x).House = getNextHouseObject();
            Debug.LogFormat("[Settlers Of KTaNE #{0}] You placed your first settlement at {1}. (in reading order)", moduleId, getSettlementPos(x) + 1);
            points++;
            Debug.LogFormat("[Settlers Of KTaNE #{0}] You now have {1} points!", moduleId, points);
            for (int i = 0; i < houses.Length; i++)
            {
                if (houses[i] != null)
                {
                    houses[i] = null;
                    break;
                }
            }
            firstHouse = false;
            getSettlementById(settlements, x).isSettlementSet = true;
            audio.PlaySoundAtTransform("HammerSoundEffect", transform);
        }
        else
        {
            if (!getSettlementById(settlements, x).isSettlementSet)
            {
                if (checkValiditySettlements(getSettlementById(settlements, x)))
                {
                    if (!isEmpty(houses))
                    {
                        wood--;
                        grain--;
                        wool--;
                        brick--;
                        UpdateMats();
                        getNextHouseObject().transform.position = cylinder.transform.position;
                        getSettlementById(settlements, x).House = getNextHouseObject();
                        Debug.LogFormat("[Settlers Of KTaNE #{0}] You placed a settlement at {1}. (in reading order)", moduleId, getSettlementPos(x) + 1);
                        points++;
                        Debug.LogFormat("[Settlers Of KTaNE #{0}] You now have {1} points!", moduleId, points);
                        Finished();
                        for (int i = 0; i < houses.Length; i++)
                        {
                            if (houses[i] != null)
                            {
                                houses[i] = null;
                                break;
                            }
                        }
                        firstHouse = false;
                        getSettlementById(settlements, x).isSettlementSet = true;
                        audio.PlaySoundAtTransform("HammerSoundEffect", transform);

                    }
                    else
                    {
                        Strike(cylinder);
                        Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: No settlements left. You only have 5.", moduleId);
                    }

                }
                else
                {
                    Strike(cylinder);
                    Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: Not a valid position for a new settlement.", moduleId);
                }
            }
            else
            {
                if (ore >= 3 && grain >= 2)
                {

                    ore = ore - 3;
                    grain = grain - 2;
                    UpdateMats();
                    getSettlementById(settlements, x).House.transform.position = PosSaves[getNextEmptySettlementPos()].transform.position;
                    houses[getNextEmptySettlementPos()] = getSettlementById(settlements, x).House;
                    getSettlementById(settlements, x).House = null;
                    getNextCityObject().transform.position = cylinder.transform.position;
                    cylinder.GetComponent<KMSelectable>().Highlight = null;
                    cylinder.GetComponent<KMSelectable>().Parent = null;
                    GetComponent<KMSelectable>().Children[getSettlementPos(x) + 8] = null;
                    GetComponent<KMSelectable>().UpdateChildren();
                    getSettlementById(settlements, x).isCitySet = true;
                    Debug.LogFormat("[Settlers Of KTaNE #{0}] You upgraded your settlement to a city at {1}. (in reading order)", moduleId, getSettlementPos(x) + 1);
                    points++;
                    Debug.LogFormat("[Settlers Of KTaNE #{0}] You now have {1} points!", moduleId, points);
                    audio.PlaySoundAtTransform("HammerSoundEffect", transform);
                    Finished();
                }
                else
                {
                    Strike(cylinder);
                    Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: Not enough resource for a city. You had {1} ore and {2} grain.", moduleId, ore, grain);
                }
            }
        }
    }


    void PressStreet(KMSelectable street)
    {
        street.AddInteractionPunch();
        if (pressedStreets[Array.IndexOf(Streets, street)])
            return;
        pressedStreets[Array.IndexOf(Streets, street)] = true;
        if (firstPath)
        {
            if (getSettlementById(settlements, streetConnections[Streetpos(street)][0]).isSettlementSet || getSettlementById(settlements, streetConnections[Streetpos(street)][1]).isSettlementSet)
            {
                firstPath = false;
                getSettlementById(settlements, streetConnections[Streetpos(street)][0]).path = true;
                getSettlementById(settlements, streetConnections[Streetpos(street)][1]).path = true;
                street.GetComponent<MeshRenderer>().enabled = true;
                GetComponent<KMSelectable>().Children[Streetpos(street) + 32] = null;
                street.GetComponent<KMSelectable>().Highlight = null;
                street.GetComponent<KMSelectable>().Parent = null;
                GetComponent<KMSelectable>().UpdateChildren();
                Debug.LogFormat("[Settlers Of KTaNE #{0}] You placed your first street between settlement positions {1} and {2}. (in reading order)", moduleId, getSettlementPos(streetConnections[Streetpos(street)][0]) + 1, getSettlementPos(streetConnections[Streetpos(street)][1]));
                audio.PlaySoundAtTransform("HammerSoundEffect", transform);

            }
            else
            {
                Strike(street);
                Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: No neaby settlement.", moduleId);
            }
        }
        else
        {

            if (wood > 0 && brick > 0)
            {
                if (getSettlementById(settlements, streetConnections[Streetpos(street)][0]).path || getSettlementById(settlements, streetConnections[Streetpos(street)][1]).path)
                {
                    wood--;
                    brick--;
                    UpdateMats();
                    firstPath = false;
                    if (getSettlementById(settlements, streetConnections[Streetpos(street)][0]).path)
                    {
                        getSettlementById(settlements, streetConnections[Streetpos(street)][0]).hasDuplicatePath = true;
                    }
                    else
                    {
                        getSettlementById(settlements, streetConnections[Streetpos(street)][0]).path = true;
                    }

                    if (getSettlementById(settlements, streetConnections[Streetpos(street)][1]).path)
                    {
                        getSettlementById(settlements, streetConnections[Streetpos(street)][1]).hasDuplicatePath = true;
                    }
                    else
                    {
                        getSettlementById(settlements, streetConnections[Streetpos(street)][1]).path = true;
                    }

                    street.GetComponent<MeshRenderer>().enabled = true;
                    GetComponent<KMSelectable>().Children[Streetpos(street) + 32] = null;
                    street.GetComponent<KMSelectable>().Highlight = null;
                    street.GetComponent<KMSelectable>().Parent = null;
                    GetComponent<KMSelectable>().UpdateChildren();
                    audio.PlaySoundAtTransform("HammerSoundEffect", transform);
                    Debug.LogFormat("[Settlers Of KTaNE #{0}] You placed a street between settlement positions {1} and {2}. (in reading order)", moduleId, getSettlementPos(streetConnections[Streetpos(street)][0]) + 1, getSettlementPos(streetConnections[Streetpos(street)][1]));
                    if (!longStreetDetected)
                    {
                        longStreet(getSettlementById(settlements, streetConnections[Streetpos(street)][0]), 1, 10);
                        if (!longStreetDetected)
                        {
                            longStreet(getSettlementById(settlements, streetConnections[Streetpos(street)][1]), 1, 10);
                        }

                    }
                    Finished();

                }
                else
                {
                    Strike(street);
                    Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: No neaby street.", moduleId);
                }
            }
            else
            {
                Strike(street);
                Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: Not enough resource for a street. You had {1} wood and {2} brick.", moduleId, wood, brick);
            }
        }
    }



    void longStreet(Settlement x, int iteration, int prev)
    {                      //0 = LEFT; 1 == MIDDLE; 2 == RIGHT               //Exeption at getLeft/getRight
        if (iteration == 6)                                                       //done
        {
            points = points + 2;
            Debug.LogFormat("[Settlers Of KTaNE #{0}] You have built a street with the length of 5! You have {1} points", moduleId, points);
            longStreetDetected = true;
        }
        else
        {
            if (iteration == 1)
            {
                if (x.getLeft(settlements) != null)
                {
                    if (x.getLeft(settlements).hasDuplicatePath)
                    {
                        longStreet(x.getLeft(settlements), iteration + 1, 0);
                    }
                }
                if (x.getRight(settlements) != null)
                {
                    if (x.getRight(settlements).hasDuplicatePath)
                    {
                        longStreet(x.getRight(settlements), iteration + 1, 2);
                    }
                }
                if (x.getMiddle(settlements) != null)
                {
                    if (x.getMiddle(settlements).hasDuplicatePath)
                    {
                        longStreet(x.getMiddle(settlements), iteration + 1, 1);
                    }
                }
            }
            else if (prev == 0)
            {
                if (x.getLeft(settlements) != null)
                {
                    if (x.getLeft(settlements).hasDuplicatePath || (iteration == 5 && x.getLeft(settlements).path))
                    {
                        longStreet(x.getLeft(settlements), iteration + 1, 0);
                    }
                }
                if (x.getMiddle(settlements) != null)
                {
                    if (x.getMiddle(settlements).hasDuplicatePath || (iteration == 5 && x.getMiddle(settlements).path))
                    {
                        longStreet(x.getMiddle(settlements), iteration + 1, 1);
                    }
                }
            }
            if (prev == 1)
            {

                if (x.getLeft(settlements) != null)
                {
                    if (x.getLeft(settlements).hasDuplicatePath || (iteration == 5 && x.getLeft(settlements).path))
                    {
                        longStreet(x.getLeft(settlements), iteration + 1, 0);
                    }
                }
                if (x.getRight(settlements) != null)
                {
                    if (x.getRight(settlements).hasDuplicatePath || (iteration == 5 && x.getRight(settlements).path))
                    {
                        longStreet(x.getRight(settlements), iteration + 1, 2);
                    }
                }
            }
            else if (prev == 2)
            {
                if (x.getMiddle(settlements) != null)
                {
                    if (x.getMiddle(settlements).hasDuplicatePath || (iteration == 5 && x.getMiddle(settlements).path))
                    {
                        longStreet(x.getMiddle(settlements), iteration + 1, 1);
                    }
                }
                if (x.getRight(settlements) != null)
                {
                    if (x.getRight(settlements).hasDuplicatePath || (iteration == 5 && x.getRight(settlements).path))
                    {
                        longStreet(x.getRight(settlements), iteration + 1, 2);
                    }
                }
            }
        }
    }

    bool allClaimed()
    {
        for (int i = 0; i < 7; i++)
        {
            if (theHexes[i].claimable != 0)
            {
                return false;
            }
        }
        return true;
    }

    void Finished()
    {
        if (!IsNoSolveMode)
        {
            if (!isSolved)
            {
                if (points >= 5)
                {
                    isSolved = true;
                    Debug.LogFormat("[Settlers Of KTaNE #{0}] Module SOLVED! You built up a good township. Well done.", moduleId);
                    Debug.LogFormat("[Settlers Of KTaNE #{0}] If you think there is a bug anywhere please let me know. Message me via Discord GeekYiwen#7561 or via e-Mail: yiwenmc@gmail.com. Thank you very much!", moduleId);
                    GetComponent<KMBombModule>().HandlePass();

                }

            }

        }
    }

    int Streetpos(KMSelectable street)
    {
        for (int i = 0; i < Streets.Length; i++)
        {
            if (Streets[i] == street)
            {
                return i;
            }
        }
        return 0;
    }

    bool isEmpty(GameObject[] x)
    {
        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != null)
            {
                return false;
            }
        }
        return true;
    }

    GameObject getNextHouseObject()
    {

        for (int i = 0; i < houses.Length; i++)
        {
            if (houses[i] != null)
            {
                return houses[i];
            }
        }
        return null;
    }

    int getSettlementPos(int x)
    {
        int j = 0;
        for (int i = 0; i < settlements.Length; i++)
        {
            if (settlements[i] != null)
            {
                if (settlements[i].id == x)
                {

                    return j;
                }
                j++;
            }

        }
        return j;
    }

    int getNextEmptySettlementPos()
    {
        for (int i = 0; i < houses.Length; i++)
        {
            if (houses[i] == null)
            {
                return i;
            }
        }
        return 0;
    }

    GameObject getNextCityObject()
    {
        for (int i = 0; i < cities.Length; i++)
        {
            if (cities[i] != null)
            {
                GameObject x = cities[i];
                cities[i] = null;
                return x;
            }
        }

        return null;
    }

    Settlement getSettlementById(Settlement[] s, int id)
    {

        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] != null)
            {
                if (s[i].id == id)
                {
                    return s[i];
                }
            }
        }
        return null;
    }

    void shuffleArray()
    {
        diceRolls.Shuffle();
        Debug.LogFormat("[Settlers Of KTaNE #{0}] New dicerolls are {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29} ,{30} ({31}, {32}, {33}, {34}, {35}, {36})", moduleId, diceRolls[0], diceRolls[1], diceRolls[2], diceRolls[3], diceRolls[4], diceRolls[5], diceRolls[6], diceRolls[7], diceRolls[8], diceRolls[9], diceRolls[10], diceRolls[11], diceRolls[12], diceRolls[13], diceRolls[14], diceRolls[15], diceRolls[16], diceRolls[17], diceRolls[18], diceRolls[19], diceRolls[20], diceRolls[21], diceRolls[22], diceRolls[23], diceRolls[24], diceRolls[25], diceRolls[26], diceRolls[27], diceRolls[28], diceRolls[29], diceRolls[30], diceRolls[31], diceRolls[32], diceRolls[33], diceRolls[34], diceRolls[35]);
    }

    bool checkValiditySettlements(Settlement s)
    {
        if (wool > 0 && wood > 0 && grain > 0 && brick > 0)
        {       //Look for enough resources
            if (checkNearbyHouse(s))
            {                              //look for nearby Houses
                if (s.path)
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool checkNearbyHouse(Settlement s)
    {

        if (s.getLeft(settlements) != null)
        {                       //is there Left?
            if (s.getLeft(settlements).isSettlementSet)
            {           //is there a Settlement on the Left?
                return false;
            }
        }
        if (s.getMiddle(settlements) != null)
        {                                                          //is there Middle?
            if (s.getMiddle(settlements).isSettlementSet)
            {                                                      //is there a Settlement on the Middle?
                return false;
            }
        }
        if (s.getRight(settlements) != null)
        {                                                            //is there Right?
            if (s.getRight(settlements).isSettlementSet)
            {                                                        //is there a Settlement on the Right?
                return false;
            }
        }
        return true;
    }

    String getFullName(String x)
    {
        switch (x)
        {
            case "B": return "brick";
            case "G": return "grain";
            case "O": return "ore";
            case "W": return "wood";
            case "S": return "wool";


            default: return null;
        }
    }

    void PressBrickDisp()
    {

        Debug.LogFormat("[Settlers Of KTaNE #{0}] Brick pressed.", moduleId);
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BrickDisp.transform);
        BrickDisp.AddInteractionPunch();
        if (thiefMode)
        {
            if (brick > 0)
            {
                brick--;
                UpdateMats();
                discardableRes--;
                if (discardableRes == 0)
                {
                    thiefMode = false;
                }
            }
        }
        else
        {
            if (CommandQueue.Length == 1 && tradeEnough(CommandQueue) && trade)
            {
                SubtractMats(CommandQueue);
                giveMats("B");
                Debug.LogFormat("[Settlers Of KTaNE #{0}] You traded 3 {1} for 1 {2}.", moduleId, getFullName(CommandQueue), getFullName("B"));
                CommandQueue = "";
                UpdateMats();
                trade = false;


                return;
            }
            else
            {
                CommandQueue = CommandQueue + "B";
            }
            if (CommandQueue.Length > 1)
            {
                Strike(BrickDisp);
                Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: You didn't trade correctly. Trade transaction resetted.", moduleId);
                CommandQueue = "";
                trade = false;
            }
        }

    }

    void PressLumberDisp()
    {

        Debug.LogFormat("[Settlers Of KTaNE #{0}] Wood pressed.", moduleId);
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, LumberDisp.transform);
        if (thiefMode)
        {
            if (wood > 0)
            {
                wood--;
                UpdateMats();
                discardableRes--;
                if (discardableRes == 0)
                {
                    thiefMode = false;
                }
            }
        }
        else
        {
            if (CommandQueue.Length == 1 && tradeEnough(CommandQueue) && trade)
            {
                SubtractMats(CommandQueue);
                giveMats("W");
                CommandQueue = "";
                Debug.LogFormat("[Settlers Of KTaNE #{0}] You traded 3 {1} for 1 {2}.", moduleId, getFullName(CommandQueue), getFullName("W"));
                UpdateMats();
                trade = false;
                return;
            }
            else
            {
                CommandQueue = CommandQueue + "W";
            }
            if (CommandQueue.Length > 1)
            {
                Strike(LumberDisp);
                Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: You didn't trade correctly. Trade transaction resetted.", moduleId);
                CommandQueue = "";
                trade = false;
            }
        }

    }

    void PressOreDisp()
    {

        Debug.LogFormat("[Settlers Of KTaNE #{0}] Ore pressed.", moduleId);
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, OreDisp.transform);
        if (thiefMode)
        {
            if (ore > 0)
            {
                ore--;
                UpdateMats();
                discardableRes--;
                if (discardableRes == 0)
                {
                    thiefMode = false;
                }
            }
        }
        else
        {
            if (CommandQueue.Length == 1 && tradeEnough(CommandQueue) && trade)
            {
                SubtractMats(CommandQueue);
                giveMats("O");
                Debug.LogFormat("[Settlers Of KTaNE #{0}] You traded 3 {1} for 1 {2}.", moduleId, getFullName(CommandQueue), getFullName("O"));
                CommandQueue = "";
                UpdateMats();
                trade = false;
                return;
            }
            else
            {
                CommandQueue = CommandQueue + "O";
            }
            if (CommandQueue.Length > 1)
            {
                Strike(OreDisp);
                Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: You didn't trade correctly. Trade transaction resetted.", moduleId);
                CommandQueue = "";
                trade = false;
            }
        }

    }

    void PressGrainDisp()
    {

        Debug.LogFormat("[Settlers Of KTaNE #{0}] Grain pressed.", moduleId);
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, GrainDisp.transform);
        GrainDisp.AddInteractionPunch();
        if (thiefMode)
        {
            if (grain > 0)
            {
                grain--;
                UpdateMats();
                discardableRes--;
                if (discardableRes == 0)
                {
                    thiefMode = false;
                }
            }
        }
        else
        {
            if (CommandQueue.Length == 1 && tradeEnough(CommandQueue) && trade)
            {
                SubtractMats(CommandQueue);
                giveMats("G");
                Debug.LogFormat("[Settlers Of KTaNE #{0}] You traded 3 {1} for 1 {2}.", moduleId, getFullName(CommandQueue), getFullName("G"));
                CommandQueue = "";
                UpdateMats();
                trade = false;
                return;
            }
            else
            {
                CommandQueue = CommandQueue + "G";
            }
            if (CommandQueue.Length > 1)
            {
                Strike(GrainDisp);
                Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: You didn't trade correctly. Trade transaction resetted.", moduleId);
                CommandQueue = "";
                trade = false;
            }
        }

    }

    void PressWoolDisp()
    {

        Debug.LogFormat("[Settlers Of KTaNE #{0}] Wood pressed.", moduleId);
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, WoolDisp.transform);
        WoolDisp.AddInteractionPunch();
        if (thiefMode)
        {
            if (wool > 0)
            {
                wool--;
                UpdateMats();
                discardableRes--;
                if (discardableRes == 0)
                {
                    thiefMode = false;
                }
            }
        }
        else
        {
            if (CommandQueue.Length == 1 && tradeEnough(CommandQueue) && trade)
            {
                SubtractMats(CommandQueue);
                giveMats("S");
                Debug.LogFormat("[Settlers Of KTaNE #{0}] You traded 3 {1} for 1 {2}.", moduleId, getFullName(CommandQueue), getFullName("S"));
                CommandQueue = "";
                UpdateMats();
                trade = false;
                return;
            }
            else
            {
                CommandQueue = CommandQueue + "S";
            }
            if (CommandQueue.Length > 1)
            {
                Strike(WoolDisp);
                Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: You didn't trade correctly. Trade transaction resetted.", moduleId);
                CommandQueue = "";
                trade = false;
            }
        }

    }

    void UpdateMats()
    {
        GrainNo.GetComponentInChildren<TextMesh>().text = "" + grain;
        OreNo.GetComponentInChildren<TextMesh>().text = "" + ore;
        LumberNo.GetComponentInChildren<TextMesh>().text = "" + wood;
        WoolNo.GetComponentInChildren<TextMesh>().text = "" + wool;
        BrickNo.GetComponentInChildren<TextMesh>().text = "" + brick;
        Debug.LogFormat("[Settlers Of KTaNE #{0}] You now have {1} brick, {2} wood, {3} ore, {4} grain and {5} wool.", moduleId, brick, wood, ore, grain, wool);
    }

    void PressDice()
    {
        audio.PlaySoundAtTransform("DiceRoll", transform);
        LeftDice.AddInteractionPunch();
        RightDice.AddInteractionPunch();
        if (trade || CommandQueue != "")
        {
            CommandQueue = "";
            trade = false;
            Debug.LogFormat("[Settlers Of KTaNE #{0}] Trade transaction not finished. Trade transaction resetted.", moduleId);
        }
        if (discardableRes == 0 && !firstHouse && !firstPath)
        {
            if (discardableRes == 0)
            {
                if (allClaimed())
                {
                    if (counter >= 30)
                    {
                        shuffleArray();
                        counter = 0;
                    }
                    diceroll = diceRolls[counter];
                    while (true)
                    {
                        int temp = UnityEngine.Random.Range(1, 7);
                        if (diceroll - temp >= 1 && diceroll - temp <= 6)
                        {
                            rotateLeftDice(temp);
                            rotateRightDice(diceroll - temp);
                            break;
                        }
                    }
                    if (diceroll == 7)
                    {
                        if (wood + grain + wool + ore + brick > 7)
                        {
                            discardableRes = (wood + grain + wool + ore + brick) / 2;
                            thiefMode = true;
                            Debug.LogFormat("[Settlers Of KTaNE #{0}] 7 Rolled. You have {1} resource. You have to discard {2} resource.", moduleId, wood + grain + wool + ore + brick, discardableRes);
                        }
                        counter++;
                    }
                    else
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            theHexes[i].claimable = givenRessources(theHexes[i]);
                        }

                        counter++;
                    }
                }
                else
                {
                    Strike(LeftDice);
                    Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: Not all resource have been claimed yet. Don't waste!", moduleId);

                }
            }
            else
            {
                Strike(LeftDice);
                Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: Not enough resource have been discarded yet!", moduleId);
            }
        }
        else
        {
            Strike(LeftDice);
            Debug.LogFormat("[Settlers Of KTaNE #{0}] STRIKE due to: Not all resource have been discarded yet OR haven't build first street or settlement.", moduleId);
        }


    }

    void Strike(KMSelectable pressed)           //strike shortcut
    {
        if (!isSolved || points < 5)
        {
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {

            Debug.LogFormat("[Settlers Of KTaNE #{0}] This STRIKE would have been a strike, but you have finished the module already.", moduleId);
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, pressed.transform);
        }
    }



    void SubtractMats(String x)
    {
        switch (x)
        {
            case "B":
                brick = brick - 3;
                break;
            case "W":
                wood = wood - 3;
                break;
            case "O":
                ore = ore - 3;
                break;
            case "G":
                grain = grain - 3;
                break;
            case "S":
                wool = wool - 3;
                break;
            default: return;
        }

    }

    bool tradeEnough(String x)
    {
        switch (x)
        {
            case "B":
                if (brick >= 3)
                {
                    return true;
                }
                else return false;
            case "W":
                if (wood >= 3)
                {
                    return true;
                }
                else return false;
            case "O":
                if (ore >= 3)
                {
                    return true;
                }
                else return false;
            case "G":
                if (grain >= 3)
                {
                    return true;
                }
                else return false;
            case "S":
                if (wool >= 3)
                {
                    return true;
                }
                else return false;
            default: return false;
        }
    }

    void giveMats(String x)
    {
        switch (x)
        {
            case "B":
                brick++;
                break;
            case "W":
                wood++;
                break;
            case "O":
                ore++;
                break;
            case "G":
                grain++;
                break;
            case "S":
                wool++;
                break;
            default: return;
        }
    }

    void Init()
    {
        RandomiseHex();             //Makes random resource Hexes
    }

    void RandomiseHex()
    {
        int HexNumber0 = (bomb.GetPortCount() + bomb.GetIndicators().Count()) % 10 + 2;
        if (HexNumber0 == 7)
        {
            HexNumber0 = getRidofSeven(6);
        }
        int HexNumber1 = getHexNumberbaseSix(0);
        int HexNumber2 = getHexNumberbaseSix(1);
        int HexNumber3 = getHexNumberbaseSix(2);
        int HexNumber4 = getHexNumberbaseSix(3);
        int HexNumber5 = getHexNumberbaseSix(4);
        int HexNumber6 = getHexNumberbaseSix(5);

        theHexes[0] = new Hex(Hexas[0], HexNumber0, new Settlement[] { getSettlementById(settlements, 23), getSettlementById(settlements, 34), getSettlementById(settlements, 44), getSettlementById(settlements, 53), getSettlementById(settlements, 42), getSettlementById(settlements, 32) }, new int[] { 11, 16, 21, 20, 15, 10 });                            //goes through all Hexes and changes their colors
        DrawHexField(theHexes[0]);

        theHexes[1] = new Hex(Hexas[1], HexNumber1, new Settlement[] { getSettlementById(settlements, 4), getSettlementById(settlements, 15), getSettlementById(settlements, 25), getSettlementById(settlements, 34), getSettlementById(settlements, 23), getSettlementById(settlements, 13) }, new int[] { 4, 7, 12, 11, 6, 3 });
        DrawHexField(theHexes[1]);
        theHexes[2] = new Hex(Hexas[2], HexNumber2, new Settlement[] { getSettlementById(settlements, 25), getSettlementById(settlements, 36), getSettlementById(settlements, 46), getSettlementById(settlements, 55), getSettlementById(settlements, 44), getSettlementById(settlements, 34) }, new int[] { 13, 17, 23, 22, 16, 12 });
        DrawHexField(theHexes[2]);
        theHexes[3] = new Hex(Hexas[3], HexNumber3, new Settlement[] { getSettlementById(settlements, 44), getSettlementById(settlements, 55), getSettlementById(settlements, 65), getSettlementById(settlements, 74), getSettlementById(settlements, 63), getSettlementById(settlements, 53) }, new int[] { 22, 26, 30, 29, 25, 21 });
        DrawHexField(theHexes[3]);
        theHexes[4] = new Hex(Hexas[4], HexNumber4, new Settlement[] { getSettlementById(settlements, 42), getSettlementById(settlements, 53), getSettlementById(settlements, 63), getSettlementById(settlements, 72), getSettlementById(settlements, 61), getSettlementById(settlements, 51) }, new int[] { 20, 25, 28, 27, 24, 19 });
        DrawHexField(theHexes[4]);
        theHexes[5] = new Hex(Hexas[5], HexNumber5, new Settlement[] { getSettlementById(settlements, 21), getSettlementById(settlements, 32), getSettlementById(settlements, 42), getSettlementById(settlements, 51), getSettlementById(settlements, 40), getSettlementById(settlements, 30) }, new int[] { 9, 15, 19, 18, 14, 8 });
        DrawHexField(theHexes[5]);
        theHexes[6] = new Hex(Hexas[6], HexNumber6, new Settlement[] { getSettlementById(settlements, 2), getSettlementById(settlements, 13), getSettlementById(settlements, 23), getSettlementById(settlements, 32), getSettlementById(settlements, 21), getSettlementById(settlements, 11) }, new int[] { 2, 6, 10, 9, 5, 1 });
        DrawHexField(theHexes[6]);

        Debug.LogFormat("[Settlers Of KTaNE #{0}] The centre Hex will produce {2} at {1}.", moduleId, HexNumber0, getFullName(theHexes[0].material));
        Debug.LogFormat("[Settlers Of KTaNE #{0}] The north-east Hex will produce {2} at {1}.", moduleId, HexNumber1, getFullName(theHexes[1].material));
        Debug.LogFormat("[Settlers Of KTaNE #{0}] The east Hex will produce {2} at {1}.", moduleId, HexNumber2, getFullName(theHexes[2].material));
        Debug.LogFormat("[Settlers Of KTaNE #{0}] The south-east Hex will produce {2} at {1}.", moduleId, HexNumber3, getFullName(theHexes[3].material));
        Debug.LogFormat("[Settlers Of KTaNE #{0}] The south-west Hex will produce {2} at {1}.", moduleId, HexNumber4, getFullName(theHexes[4].material));
        Debug.LogFormat("[Settlers Of KTaNE #{0}] The west Hex will produce {2} at {1}.", moduleId, HexNumber5, getFullName(theHexes[5].material));
        Debug.LogFormat("[Settlers Of KTaNE #{0}] The north-west Hex will produce {2} at {1}.", moduleId, HexNumber6, getFullName(theHexes[6].material));

    }

    int getHexNumberbaseSix(int x)
    {
        int a, b, c;
        c = LettertoInt(bomb.GetSerialNumber().Substring(x, 1));
        b = c / 6;
        a = c % 6;
        if (a + b + 2 == 7)
        {
            return getRidofSeven(x);
        }
        return a + b + 2;
    }

    int getRidofSeven(int x)
    {
        switch (x)
        {
            case 0:
                x = correctNumber(7 + bomb.GetBatteryCount());                              //tr add Batteries
                if (x == 7)
                {
                    return 10;
                }
                else return x;
            case 1:                                                                                //r add portplates
                x = correctNumber(7 + bomb.GetPortPlateCount());
                if (x == 7)
                {
                    return 5;
                }
                else return x;
            case 2:
                x = correctNumber(7 - bomb.CountUniquePorts());                                 //br subtract uniquePorts/distinct port types
                if (x == 7)
                {
                    return 2;
                }
                else return x;
            case 3:
                x = correctNumber(7 + bomb.GetBatteryHolderCount() - bomb.GetIndicators().Count()); //add holders subtract Indicators
                if (x == 7)
                {
                    return 12;
                }
                else return x;
            case 4:
                x = correctNumber(7 * int.Parse(bomb.GetSerialNumber().Substring(5)));          //multiply by last digit
                if (x == 7)
                {
                    return 6;
                }
                else return x;
            case 5:
                x = correctNumber(7 + bomb.GetPortPlateCount() + bomb.GetBatteryHolderCount() + bomb.GetIndicators().Count());  //add all widgets(big size)
                if (x == 7)
                {
                    return 9;
                }
                else return x;
            case 6:
                return 8;                                                                       //centre give 8
            default: return 0;
        }
    }

    int correctNumber(int x)
    {
        while (true)
        {
            if (x >= 2 && x <= 12)
            {
                return x;
            }
            else if (x < 2)
            {
                x = x + 11;
            }
            else if (x > 12)
            {
                x = x - 11;
            }
        }
    }

    void DrawHexField(Hex Hex)
    {     //makes Random number between 1-5 and then chooses the HexColor from that number
        switch (UnityEngine.Random.Range(1, 6))
        {
            case 1:
                Hex.Position.GetComponent<MeshRenderer>().material = GrainHex;
                Hex.material = "G";
                break;
            case 2:
                Hex.Position.GetComponent<MeshRenderer>().material = BrickHex;
                Hex.material = "B";
                break;
            case 3:
                Hex.Position.GetComponent<MeshRenderer>().material = WoolHex;
                Hex.material = "S";
                break;
            case 4:
                Hex.Position.GetComponent<MeshRenderer>().material = OreHex;
                Hex.material = "O";
                break;
            case 5:
                Hex.Position.GetComponent<MeshRenderer>().material = WoodHex;
                Hex.material = "W";
                break;
            default: return;

        }
    }

    int LettertoInt(String x)
    {
        switch (x)
        {
            case "A": return 1 + 9;
            case "B": return 2 + 9;
            case "C": return 3 + 9;
            case "D": return 4 + 9;
            case "E": return 5 + 9;
            case "F": return 6 + 9;
            case "G": return 7 + 9;
            case "H": return 8 + 9;
            case "I": return 9 + 9;
            case "J": return 10 + 9;
            case "K": return 11 + 9;
            case "L": return 12 + 9;
            case "M": return 13 + 9;
            case "N": return 14 + 9;
            case "O": return 15 + 9;
            case "P": return 16 + 9;
            case "Q": return 17 + 9;
            case "R": return 18 + 9;
            case "S": return 19 + 9;
            case "T": return 20 + 9;
            case "U": return 21 + 9;
            case "V": return 22 + 9;
            case "W": return 23 + 9;
            case "X": return 24 + 9;
            case "Y": return 25 + 9;
            case "Z": return 26 + 9;
            case "1": return 1;
            case "2": return 2;
            case "3": return 3;
            case "4": return 4;
            case "5": return 5;
            case "6": return 6;
            case "7": return 7;
            case "8": return 8;
            case "9": return 9;
            case "0": return 0;

            default: return 0;
        }
    }

    void rotateLeftDice(int x)
    {//changes rotation of left dice so the x face is in top.
        switch (x)
        {
            case 1:
                LeftDice.GetComponent<Transform>().transform.localEulerAngles = new Vector3(-90, 0, 0);
                break;
            case 2:
                LeftDice.GetComponent<Transform>().transform.localEulerAngles = new Vector3(180, 0, 0);
                break;
            case 3:
                LeftDice.GetComponent<Transform>().transform.localEulerAngles = new Vector3(180, 0, 90);
                break;
            case 4:
                LeftDice.GetComponent<Transform>().transform.localEulerAngles = new Vector3(0, 0, 90);
                break;
            case 5:
                LeftDice.GetComponent<Transform>().transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case 6:
                LeftDice.GetComponent<Transform>().transform.localEulerAngles = new Vector3(90, 0, 90);
                break;
            default: return;
        }
    }

    void rotateRightDice(int x)
    {//changes rotation of right dice so the x face is in top.
        switch (x)
        {
            case 1:
                RightDice.GetComponent<Transform>().transform.localEulerAngles = new Vector3(-90, 0, 0);
                break;
            case 2:
                RightDice.GetComponent<Transform>().transform.localEulerAngles = new Vector3(180, 0, 0);
                break;
            case 3:
                RightDice.GetComponent<Transform>().transform.localEulerAngles = new Vector3(180, 0, 90);
                break;
            case 4:
                RightDice.GetComponent<Transform>().transform.localEulerAngles = new Vector3(0, 0, 90);
                break;
            case 5:
                RightDice.GetComponent<Transform>().transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case 6:
                RightDice.GetComponent<Transform>().transform.localEulerAngles = new Vector3(90, 0, 90);
                break;
            default: return;
        }
    }

    public int givenRessources(Hex h)
    {
        if (h.HexNo == diceroll)
        {
            int x = 0;
            for (int i = 0; i < h.nearby.Length; i++)
            {
                if (h.nearby[i].isCitySet)
                {
                    x = x + 2;
                }
                else if (h.nearby[i].isSettlementSet)
                {
                    x++;
                }
            }
            if (x != 0)
            {
                Debug.LogFormat("[Settlers Of KTaNE #{0}] You rolled {1}. You must collect {2} {3}.", moduleId, diceroll, x, getFullName(h.material));
            }
            return x;
        }
        else return 0;
    }
    // Update is called once per frame
    void Update()
    {

    }
    //____________________________________________________________________________________________________________________________________________________________________________________________________________
    //TP Implementation
    public string command;
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "Roll the dice with !{0} roll or !{0} roll until <no1> <no2> <...>. |" +
        " Build a settlement/city with !{0} build house/city/settlement <hexCardinal> <CardinalOfThatHex>. |" +
        " Build a street with !{0} build street <hexCardinal> <CardinalOfThatHex>. |" +
        " Claim one or multiple resource with !{0} hex <hexCardinal> <hexCardinal> <...>. |" +
        " Trade resources using !{0} trade <res> <res>. e.g. !{0} trade brick wood: trades 4 brick for 1 wood. Valid resource are BRICK, WOOD, ORE, GRAIN, WOOL. |" +
        " Discard resources during thiefmode with !{0} discard <res> <number>. e.g. !{0} discard brick 4, discards 4 brick. |" +
        " Valid Cardinals are N,NE,SE... & T,TR,BR,M,C..." +
        " !{0} toggleNoSolveMode <-- its self-explanatory @L.W. :P";
#pragma warning restore 414


    Hex TPStringtoHex(String partCommand)
    {
        switch (partCommand)
        {
            case "c":
                return theHexes[0];
            case "ne":
                return theHexes[1];
            case "e":
                return theHexes[2];
            case "se":
                return theHexes[3];
            case "sw":
                return theHexes[4];
            case "w":
                return theHexes[5];
            case "nw":
                return theHexes[6];
            case "m":
                return theHexes[0];
            case "tr":
                return theHexes[1];
            case "r":
                return theHexes[2];
            case "br":
                return theHexes[3];
            case "bl":
                return theHexes[4];
            case "l":
                return theHexes[5];
            case "tl":
                return theHexes[6];
            default: return null;
        }
    }

    KMSelectable StringToRessourseKMSe(String x)
    {
        if (command.StartsWith("brick"))
        {
            command = command.Substring(5).Trim();
            return BrickDisp;
        }
        else if (command.StartsWith("wood"))
        {
            command = command.Substring(4).Trim();
            return LumberDisp;
        }
        else if (command.StartsWith("wool"))
        {
            command = command.Substring(4).Trim();
            return WoolDisp;
        }
        else if (command.StartsWith("ore"))
        {
            command = command.Substring(3).Trim();
            return OreDisp;
        }
        else if (command.StartsWith("grain"))
        {
            command = command.Substring(5).Trim();
            return GrainDisp;
        }
        else return null;
    }

    int TPStringtoIntStreet(string command)
    {
        Hex temp;
        if (command.Substring(1, 1) == " ")
        {
            temp = TPStringtoHex(command.Substring(0, 1));
            command = command.Substring(1).Trim();
        }
        else
        {
            temp = TPStringtoHex(command.Substring(0, 2));
            command = command.Substring(2).Trim();
        }
        int loc;
        switch (command)
        {
            case "tr":
                loc = 0;
                break;
            case "r":
                loc = 1;
                break;
            case "br":
                loc = 2;
                break;
            case "bl":
                loc = 3;
                break;
            case "l":
                loc = 4;
                break;
            case "tl":
                loc = 5;
                break;
            case "ne":
                loc = 0;
                break;
            case "e":
                loc = 1;
                break;
            case "se":
                loc = 2;
                break;
            case "sw":
                loc = 3;
                break;
            case "w":
                loc = 4;
                break;
            case "nw":
                loc = 5;
                break;
            default: return -1;

        }
        return temp.streets[loc] - 1;

    }

    int TPStringtoInt(String partCommand)
    {
        switch (partCommand)
        {
            case "n": return 0;
            case "ne": return 1;
            case "se": return 2;
            case "s": return 3;
            case "sw": return 4;
            case "nw": return 5;
            case "t": return 0;
            case "tr": return 1;
            case "br": return 2;
            case "b": return 3;
            case "bl": return 4;
            case "tl": return 5;
            default: return 6;
        }
    }

    bool ArrayHasDiceNumber(List<int> list)
    {
        for (int i = 0; i < list.Count(); i++)
        {
            if (list.ElementAt(i) >= 2 && list.ElementAt(i) <= 13)
            {
                return true;
            }
        }
        return false;
    }

    string[] validStreetCoords = { "ne", "e", "se", "sw", "w", "nw", "tr", "r", "br", "bl", "l", "tl" };
    string[] validHexCoords = { "ne", "e", "se", "sw", "w", "nw", "c", "tr", "r", "br", "bl", "l", "tl", "m" };
    string[] validHouseCoords = { "n", "ne", "se", "s", "sw", "nw", "t", "tr", "br", "b", "bl", "tl" };

    IEnumerator ProcessTwitchCommand(string TPcommand)
    {
        command = TPcommand;
        command = command.ToLowerInvariant().Trim();
        RegexOptions options = RegexOptions.None;
        Regex regex = new Regex("[ ]{2,}", options);
        command = regex.Replace(command, " ");
        if (command.Equals("roll"))                                                             //roll the dice
        {
            yield return null;
            LeftDice.OnInteract();
            yield break;
        }
        else if (command.Equals("togglenosolvemode"))
        {
            yield return null;
            NosolveMode.OnInteract();
            yield break;
        }
        else if (Regex.IsMatch(command, @"^roll +until +[\d+ +]+$"))                            //roll until {0} {1} ...
        {
            List<int> myArray = new List<int>();
            command = command.Substring(5).Trim();
            command = command.Substring(6).Trim();
            while (command != "")
            {
                if (command.Length == 1)
                {
                    myArray.Add(int.Parse(command.Substring(0, 1)));
                    command = "";
                }
                else if (command.Length == 2)
                {
                    myArray.Add(int.Parse(command.Substring(0, 2)));
                    command = "";
                }
                else if (command.Substring(1, 1) == " ")
                {
                    myArray.Add(int.Parse(command.Substring(0, 1)));
                    command = command.Substring(2).Trim();
                }
                else if (command.Substring(2, 1) == " ")
                {
                    myArray.Add(int.Parse(command.Substring(0, 2)));
                    command = command.Substring(3).Trim();
                }
                else
                {
                    yield return "sendtochaterror Invalid command.";
                    yield break;
                }
            }
            if (ArrayHasDiceNumber(myArray))
            {
                while (true)
                {
                    for (int i = 0; i < myArray.Count(); i++)
                    {
                        if (diceRolls[counter] == myArray.ElementAt(i))
                        {
                            yield return null;
                            LeftDice.OnInteract();
                            yield break;
                        }
                    }
                    yield return null;
                    LeftDice.OnInteract();
                }
            }
            else
            {
                yield return "sendtochaterror Dice rolls have to be in range of 2 - 12.";
                yield break;
            }
        }
        else if (Regex.IsMatch(command, @"^hex +[a-z+ +]+$"))                           //collect resource !# hex [cardinal]
        {
            command = command.Substring(4).Trim();
            string[] hexas = command.Split(' ');
            for (int i = 0; i < hexas.Length; i++)
            {
                if (TPStringtoHex(hexas[i]) == null)
                {
                    yield return "sendtochaterror Invalid hex coordinates.";
                    yield break;
                }
            }
            yield return null;
            for (int i = 0; i < hexas.Length; i++)
            {
                TPStringtoHex(hexas[i]).Position.OnInteract();
                yield return new WaitForSeconds(0.05f);
            }
        }
        else if (Regex.IsMatch(command, @"^build +[a-z| ]*$"))
        {                                                                                       //upgrade or build settlement !# build house/city/settlement ([cardinal] of Hex) ([cardinal] of that Hex)
            command = command.Substring(6).Trim();
            if (Regex.IsMatch(command, @"^house +[a-z][a-z]? +[a-z][a-z]?$"))
            {
                command = command.Substring(6).Trim();
                if (validHexCoords.Contains(command.Substring(0, 2).Replace(" ", "")))
                {
                    if (validHouseCoords.Contains(command.Substring(command.Length - 2, 2).Replace(" ", "")))
                    {
                        yield return null;

                        TPStringtoHex(command.Substring(0, 2).Trim()).nearby[TPStringtoInt(command.Substring(command.Length - 2, 2).Trim())].SelectObject.OnInteract();
                        yield break;
                    }
                    else
                    {
                        yield return "sendtochaterror Invalid house coordinates.";
                    }
                }
                else
                {
                    yield return "sendtochaterror Invalid hex coordinates.";
                }
            }
            else if (Regex.IsMatch(command, @"^city +[a-z][a-z]? +[a-z][a-z]?$"))
            {
                command = command.Substring(5).Trim();
                if (validHexCoords.Contains(command.Substring(0, 2).Replace(" ", "")))
                {
                    if (validHouseCoords.Contains(command.Substring(command.Length - 2, 2).Replace(" ", "")))
                    {
                        yield return null;

                        TPStringtoHex(command.Substring(0, 2).Trim()).nearby[TPStringtoInt(command.Substring(command.Length - 2, 2).Trim())].SelectObject.OnInteract();
                        yield break;
                    }
                    else
                    {
                        yield return "sendtochaterror Invalid house coordinates.";
                    }
                }
                else
                {
                    yield return "sendtochaterror Invalid hex coordinates.";
                }
            }
            else if (Regex.IsMatch(command, @"^settlement +[a-z][a-z]? +[a-z][a-z]?$"))
            {
                command = command.Substring(11).Trim();
                if (validHexCoords.Contains(command.Substring(0, 2).Replace(" ", "")))
                {
                    if (validHouseCoords.Contains(command.Substring(command.Length - 2, 2).Replace(" ", "")))
                    {
                        yield return null;

                        TPStringtoHex(command.Substring(0, 2).Trim()).nearby[TPStringtoInt(command.Substring(command.Length - 2, 2).Trim())].SelectObject.OnInteract();
                        yield break;
                    }
                    else
                    {
                        yield return "sendtochaterror Invalid house coordinates.";
                    }
                }
                else
                {
                    yield return "sendtochaterror Invalid hex coordinates.";
                }
            }
            else if (Regex.IsMatch(command, @"^street +[a-z][a-z]? +[a-z][a-z]?$"))
            {
                command = command.Substring(7).Trim();
                if (validHexCoords.Contains(command.Substring(0, 2).Replace(" ", "")))
                {
                    if (validStreetCoords.Contains(command.Substring(command.Length - 2, 2).Replace(" ", "")))
                    {
                        yield return null;
                        int temp = TPStringtoIntStreet(command);
                        Streets[temp].OnInteract();
                        yield break;
                    }
                    else
                    {
                        yield return "sendtochaterror Invalid street coordinates.";
                    }
                }
                else
                {
                    yield return "sendtochaterror Invalid hex coordinates.";
                }

            }
        }
        else if (Regex.IsMatch(command, @"^trade +[a-z]+ +[a-z]+$"))                                                                     //trade 4 for 1 !# trade [brick/ore/wool/wood/grain] [brick/ore/wool/wood/grain]
        {
            command = command.Substring(6).Trim();

            KMSelectable x = StringToRessourseKMSe(command);
            KMSelectable y = StringToRessourseKMSe(command);
            if (command == "")
            {
                yield return null;
                x.OnInteract();
                yield return new WaitForSeconds(0.1f);
                Anchor.OnInteract();
                yield return new WaitForSeconds(0.1f);
                y.OnInteract();
                yield break;
            }
        }
        else if (Regex.IsMatch(command, @"^discard +[a-z]+ +\d*$"))                                                                      //discard resources
        {
            command = command.Substring(8).Trim();
            KMSelectable x = StringToRessourseKMSe(command);
            if (command != "")
            {
                for (int i = 0; i < int.Parse(command); i++)
                {
                    yield return null;
                    x.OnInteract();
                    yield return new WaitForSeconds(0.05f);
                }
                yield break;
            }
            else
            {
                yield return null;
                x.OnInteract();
                yield break;
            }
        }
        yield return "sendtochaterror Invalid command.";
        yield break;
    }
}

//_______________________________________  CLASSES  __________________________________________________
public class Settlement
{
    public int id;
    public bool path;
    public bool hasDuplicatePath;
    public bool isSettlementSet;
    public bool isCitySet;
    public GameObject House;
    public KMSelectable SelectObject;


    public Settlement(int id, KMSelectable selectObject)
    {     //Constructor
        this.id = id;
        path = false;
        hasDuplicatePath = false;
        this.isSettlementSet = false;
        this.isCitySet = false;
        this.SelectObject = selectObject;
    }

    public Settlement getMiddle(Settlement[] s)
    {
        if (s[id].id != 2 && s[id].id != 4 && s[id].id != 72 && s[id].id != 74)
        {
            if (((int)(id / 10)) % 2 == 0)
            {
                if (id - 10 < 0)
                {
                    return null;
                }
                else return s[id - 10];
            }
            else
            {
                return s[id + 10];
            }
        }
        return null;
    }

    public Settlement getLeft(Settlement[] s)
    {
        if (s[id].id != 11 && s[id].id != 30 && s[id].id != 40 && s[id].id != 61)
        {
            if (((int)(id / 10)) % 2 == 0)
            {
                return s[id + 9];
            }
            else
            {
                if (id - 11 < 0)
                {
                    return null;
                }
                else return s[id - 11];
            }

        }
        return null;

    }

    public Settlement getRight(Settlement[] s)
    {
        if (s[id].id != 15 && s[id].id != 36 && s[id].id != 46 && s[id].id != 65)
        {

            if (((int)(id / 10)) % 2 == 0)
            {
                return s[id + 11];
            }
            else
            {
                if (id - 9 < 0)
                {
                    return null;
                }
                else return s[id - 9];
            }
        }
        return null;
    }

}

public class Hex
{
    public int HexNo;
    public Settlement[] nearby;
    public int[] streets;
    public KMSelectable Position;
    public String material;
    public int claimable;

    public Hex(KMSelectable pos, int HexNo, Settlement[] nearbyNo, int[] streets)
    {
        nearby = new Settlement[6];
        this.Position = pos;
        claimable = 0;
        this.HexNo = HexNo;
        for (int i = 0; i < 6; i++)
        {
            nearby[i] = nearbyNo[i];
        }
        this.streets = streets;
    }
}