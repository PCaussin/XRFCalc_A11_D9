using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace XRFCalc.XRF;

public static class Chemistry
{
    /// <summary>
    /// Maximum atomic number; constant = 104
    /// </summary>
    public const int MaxElementsD = 105;
    private const int numLn = 14;
    private const int LnZ = 104;

 
    private static int TableSize { get { return MaxElementsD + 1; } }

    /// <summary>
    /// Number of Lanthanides in series, constant = 14
    /// </summary>
    public static int NumberOfLanthanides { get { return numLn; } }
    private const int MAXPARTS = 100;

    public static int DecodeFormula(string csForm, int[] pnZ, float[] pnAtom, float[] pfFraction, out float pfW)
    {

        // allow for a maximum of 100 parts in the mixture
        float[] xt = new float[TableSize];
        float[] rt = new float[TableSize];
        float[] wt = new float[TableSize];
        int[] pPartialNbAtom = new int[MAXPARTS];
        int[][] pPartialZ = new int[MAXPARTS][];
        float[][] pPartialWeightFract = new float[MAXPARTS][];

        pfW = 0.0f;

        string csF = csForm;
        int nn = csF.IndexOf(';');
        if (nn > 0) csF = csF[..nn];
        if (csF.Length == 0) return 0;

        //bool lnPresent = false;

        if (!csF.Contains('%') && !csF.Contains('+'))
            return DecodeFormula1(csF, pnZ, pnAtom, pfFraction, out pfW, false, out _);

        // else, scan the mixture formula
        int lf = csF.Length;
        char[] csTemp = new char[lf + 1]; // initialize length
        int cc = 0;
        int nc = 0;
        int st = 0;

        int j = 0;
        for (; j < lf; j++)
        {
            char ca = csF[j];
            if (ca == '%' || ca <= ' ') continue;
            if (st == 0)
            {
                if (char.IsDigit(ca) || ca == System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator[0])
                {
                    csTemp[nc++] = ca;
                    csTemp[nc] = '\0';
                }
                else
                {
                    if (nc > 0 && float.TryParse(new string(csTemp, 0, nc), out rt[cc])) { }
                    else
                        rt[cc] = 0.0f;
                    csTemp[0] = ca;
                    csTemp[1] = '\0';
                    nc = 1;
                    st = 1;
                }
            }
            else
            {
                if (ca != '+')
                {
                    csTemp[nc] = ca;
                    nc++;
                    csTemp[nc] = '\0';
                }
                else
                {
                    pPartialZ[cc] = new int[TableSize];
                    pPartialWeightFract[cc] = new float[TableSize];
                    pPartialNbAtom[cc] = DecodeFormula1(new string(csTemp, 0, nc), pPartialZ[cc], new float[MaxElementsD], pPartialWeightFract[cc], out wt[cc], false, out _);
                    //lnPresent |= ln1;
                    if (pPartialNbAtom[cc] < 0)
                        return -1;
                    if (cc < MAXPARTS - 1) cc++;
                    nc = 0;
                    st = 0;
                }
            }
        }
        if (nc > 0 && st == 0)
            return -1;
        if (nc > 0) // last compound in mixture
        {
            pPartialZ[cc] = new int[TableSize];
            pPartialWeightFract[cc] = new float[TableSize];
            pPartialNbAtom[cc] = DecodeFormula1(new string(csTemp, 0, nc), pPartialZ[cc], new float[MaxElementsD], pPartialWeightFract[cc], out wt[cc], false, out _);
            //lnPresent |= ln1;
            if (pPartialNbAtom[cc] < 0)
                return -1;
            if (cc < MAXPARTS) cc++;
        }
        for (j = 0; j <= MaxElementsD; j++)
            xt[j] = 0.0f;
        float w = 0.0f;
        for (j = 0; j < cc; j++) w += rt[j];
        if (w > 105)
            return -1;
        for (j = 0; j < cc; j++)
        {
            if (rt[j] == 0.0f)
            {
                rt[j] = 100.0f - w;
                break;
            }
        }
        float fWW = 0.0f;
        for (j = 0; j < cc; j++)
        {
            fWW += wt[j] * rt[j] / 100.0f;
            for (int k = 0; k < pPartialNbAtom[j]; k++)
                xt[pPartialZ[j][k]] += pPartialWeightFract[j][k] * rt[j] / 100.0f;
        }
        pfW = fWW;
        int nTotal = 0;

        for (j = 1; j <= MaxElementsD; j++)
        {
            if (xt[j] > 0.0f)
            {
                if (pnZ != null) pnZ[nTotal] = j;
                if (pfFraction != null) pfFraction[nTotal] = xt[j];
                nTotal++;
            }
        }
        return nTotal;
    } // end the function DecodeFormula

    /// <summary>
    /// Calculate the element concentration of a mixture
    /// </summary>
    /// <param name="Formulas">Formulas of compounds</param>
    /// <param name="Concentrations">Concentrations of compounds</param>
    /// <param name="lnPresent">"Ln" present in one or more compounds</param>
    /// <returns>table of element concentrations starting at atomic number 0</returns>
    public static float[] ComputeElements(string[] Formulas, float[] Concentrations, out bool lnPresent)
    {
        float[] fraction = new float[TableSize];
        float[] nbAtom = new float[TableSize];
        int[] zcomp = new int[TableSize];
        float[] result = new float[TableSize];
        lnPresent = false;
        for (int form = 0; form < Formulas.Length; form++)
        {
            int nbEl = DecodeFormula1(Formulas[form], zcomp, nbAtom, fraction, out _, false, out bool ln);
            lnPresent |= ln;
            for (int i = 0; i < nbEl; i++)
                result[zcomp[i]] += fraction[i] * Concentrations[form];
        }
        return result;
    }

    static void RoundTableSQD(float[] TableSQD)
    {
        for (int i = 0; i <= MaxElementsD; i++)
        {
            if (TableSQD[i] == 0.0F) continue;
            TableSQD[i] = fRoundTableSQD * ((int)((TableSQD[i] / fRoundTableSQD) + 0.5f));
        }
    }

    public static int DecodeFormula1(string inp, int[] zcomp, float[] nbAtom, float[] zfrac, out float pfW, bool simple, out bool lnPresent)
    {
        int state = 0;
        int nbel = 0;
        int jscan = 0;
        float fg = 1;
        float fgg = 1;
        float f = 0;
        float val;
        pfW = 0.0f;
        char ca;
        char caprev;
        char[] el = new char[2];
        char[] post = new char[50]; // buffer for atom count
        char[] pre = new char[50];  // buffer for pre-element count
        bool bInd = false;
        float[] x = new float[TableSize];
        int[] zc = new int[TableSize];
        lnPresent = false;
        string csForm = simple ? Clean(inp) : PreTranslate(Clean(inp), out _, out lnPresent);

        if (csForm == null)
            return -1;
        int ppr = 0;    // index in pre table
        int ppo = 0;    // index in post table

        int z = 0;
        for (; z <= MaxElementsD; z++) x[z] = 0.0f;

        bool hydratation = false;
        bool afterCenteredDot = false;

        for (; jscan < csForm.Length; jscan++)
        {
            ca = csForm[jscan];
            caprev = jscan > 0 ? csForm[jscan - 1] : '\u0000';
            if (ca == '\u0000' || ca == '/') break;
            if (ca >= 'A' && ca <= 'Z')
            {
                afterCenteredDot = false;
                if (ppr != 0)
                {
                    pre[ppr] = '\u0000';
                    if (CheckNumber(pre, hydratation, out _, out val, out _))
                        fg = val;
                }
                if (ppo != 0)
                {
                    post[ppo] = '\u0000';
                    if (CheckNumber(post, hydratation, out _, out val, out _))
                        f = val;
                }
                if (bInd)
                {
                    for (z = 1; z <= MaxElementsD; z++)
                        if (el[0] == gacElements[z][0] && (el[1] == 0 && gacElements[z].Length == 1 || gacElements[z].Length >= 2 && el[1] == gacElements[z][1])) break;
                    if (z > MaxElementsD) return -1;
                    x[z] += f * fgg;
                }
                bInd = true;
                el[0] = ca;
                el[1] = '\u0000';
                state = 1;
                fgg = fg;
                f = 1;
                ppo = 0;
                continue;
            }
            if ((ca == centeredDot) || (ca == centeredDot2) || (ca == centeredDot3) || (ca == centeredDot4))
            {
                afterCenteredDot = true;
                fg = 1;
                ppr = 0;
                hydratation = false;
                state = 0;
                continue;
            }
            if ((ca == delta) || (ca == delta2) || (ca == delta3) || (ca == delta4))        // PC 1-Aug-2014 ignore a delta and following numbers //DM more deltas...
            {
                state = 5;
                continue;
            }
            bool isNum = false;
            switch (ca)
            {
                case '(':
                case ')':
                case '+':
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '.':
                case 'x':
                case 'z':
                case 'j':
                    isNum = true;
                    break;
                case 'X':
                    if (jscan >= csForm.Length - 1 || csForm[jscan + 1] != 'e')
                        isNum = true;
                    break;
                case 'y':
                    if (caprev != 'D')
                        isNum = true;
                    break;
                case 'n':
                    if (state != 1)
                        isNum = true;
                    break;
            }
            switch (state)
            {
                default:    // should not happen
                    //Debug.Assert(false);
                    return -1;
                case 0:
                    if (isNum && ppr < 50)
                    {
                        if (afterCenteredDot)
                            hydratation = true;
                        pre[ppr++] = ca;
                        state = 4;
                        continue;
                    }
                    return -1;
                case 1:
                    if (ca >= 'a' && ca <= 'i' || ca >= 'k' && ca <= 'w' || ca == 'y' && caprev == 'D')
                    {
                        el[1] = ca;
                        state = 2;
                        continue;
                    }
                    else if (isNum && ppo < 50)
                    {
                        post[ppo++] = ca;
                        state = 3;
                        continue;
                    }
                    else if (el[1] == 'X')              // PC 8-Aug-2014 allow "X" as a number
                    {
                        if (afterCenteredDot)
                            hydratation = true;
                        pre[ppr++] = 'n';
                        state = 0;
                        continue;
                    }
                    return -1;
                case 2:
                    if (isNum && ppo < 50)
                    {
                        post[ppo++] = ca;
                        state = 3;
                        continue;
                    }
                    return -1;
                case 3:
                    if (isNum && ppo < 50)
                    {
                        post[ppo++] = ca;
                        continue;
                    }
                    return -1;
                case 4:
                    if (isNum && ppr < 50)
                    {
                        pre[ppr++] = ca;
                        continue;
                    }
                    return -1;
                case 5:                     // PC 1-Aug-2014: ignore numbers that follow a delta
                    if (isNum)
                        continue;
                    return -1;

            } // end the switch (state)
        } // end the main loop on characters

        if (state != 0 && bInd)
        {
            if (ppo != 0)
            {
                post[ppo] = '\u0000';
                if (CheckNumber(post, false, out _, out val, out _))
                    f = val;
            }
            for (z = 1; z <= MaxElementsD; z++)
                if (el[0] == gacElements[z][0] && (el[1] == 0 && gacElements[z].Length == 1 || gacElements[z].Length >= 2 && el[1] == gacElements[z][1]))
                    break;
            if (z > MaxElementsD)
                return -1;
            x[z] += f * fgg;
        }
        float w = 0;
        for (z = 1; z <= MaxElementsD; z++)
        {
            if (x[z] > 0.0f)
            {
                if (z != LnZ)	// not special "Ln" case
                {
                    zc[nbel] = z;
                    zcomp[nbel] = z;
                    nbAtom[nbel] = x[z];
                    w += x[z] * gafAtomWeight[z];
                    nbel++;
                }
                else                    // "Ln": eventy split the amount of Ln among all lanthanides
                {
                    lnPresent = true;
                    for (int k = 0; k < NumberOfLanthanides; k++)
                    {
                        zc[nbel] = lanthanides[k];
                        zcomp[nbel] = zc[nbel];
                        nbAtom[nbel] = x[z] / NumberOfLanthanides;
                        w += x[z] / NumberOfLanthanides * gafAtomWeight[zc[nbel]];
                        nbel++;
                    }
                }
            }
        }
        for (int kel = 0; kel < nbel; kel++)
        {
            zfrac[kel] = nbAtom[kel] * gafAtomWeight[zc[kel]] / w;
        }
        pfW = w;
        return nbel;
    }

    /// <summary>
    /// Check for a floating point number in text[], starting at index, stopping at end or before
    /// </summary>
    /// <param name="text">input string</param>
    /// <param name="index">input and output: starting index; returns the index of the character that follows the decoded value when return = true, unchanged when false</param>
    /// <param name="end">maximum index in input string</param>
    /// <param name="value">returns the decoded value or 0 if return = false</param>
    /// <returns>true if a value has been decoded</returns>

    private static bool CheckFloat(char[] text, ref int index, int end, out float value)
    {
        char t;
        bool hasDigit = false;
        bool hasDot = false;    // check for multiple dots
        int i = index;
        value = 0.0f;
        for (; i < end; i++)
        {
            bool done = false;
            t = text[i];
            switch (t)
            {
                case '+':
                case '-':
                    if (i != index)
                        done = true; // handle sign as part of the number if it is first character only
                    break;
                // numeric characters are inserted, we stop at the first non numeric character
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    hasDigit = true;
                    break;
                case '.':
                    if (!hasDot)
                    {
                        hasDot = true;
                        break;
                    }
                    else
                        return false;     // second dot is a syntax error
                default: // stop assembling the number string
                    done = true;
                    break;
            }
            if (done) break;
        }
        // at this point i points to the first character following the tentative number
        if (!hasDigit) // a string without any digit is not valid
            return false;
        if (i > index)
        {
            string strNum = new string(text, index, i - index);
            if (System.Single.TryParse(strNum, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out value))
            {
                index = i;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Decode a number in the format e.g. 2 or 3.5 or 1.34+x or 1.34+2x or x or 2x or 3-2x;
    /// PC 26-Feb-2009 change the decoding to allow e.g. 3-0.5x
    /// z could be used instead of x, the number can be enclosed in () or [].
    /// </summary>
    /// <param name="text">input string as a zero terminated Unicode character array</param>
    /// <param name="hydrat">true if number follows a centered dot, used to replace x or z</param>
    /// <param name="approx">returns true if x or z approximation is used</param>
    /// <param name="value">returns decoded value</param>
    /// <param name="nextchar">index of next character in string</param>
    /// <returns>result of syntax check</returns>
    private static bool CheckNumber(char[] text, bool hydrat, out bool approx, out float value, out int nextchar)
    {
        value = 0;
        approx = false;
        nextchar = 0;
        if (text.Length <= 0)
            return false;
        bool paren = false;     // enclosed in parenthesis
        int index = 0;     // scanned character index
        int i = 0;         // auxilliary index
        for (; i < text.Length; i++)
        {
            if (text[i] == 0)
                break;
        }
        int last = i;           // actual limit of zero terminated string
        int end = last;         // limit of scanning
        bool addDelta = false;  // approximation is added to primary result, multiplies the result otherwise
        float qDelta = 1.0f;    // approximation multiplier (2x, -3x etc.)

        if (text[0] == '(' || text[0] == ']')
        {
            index++;
            char mtc = text[0] == '(' ? ')' : ']';
            for (i = index; i < last; i++)
            {
                if (text[i] == mtc)
                {
                    end = i;
                    paren = true;
                    break;
                }
            }
            if (!paren)         // need a matching right parenthesis or bracket
                return false;
        }
        CheckFloat(text, ref index, end, out value);    // discard the result: a "number" like 'z' is OK
        i = index;                                      // index is not modified if CheckFloat fails, or points to the fisrt character after the number if it succeeds
                                                        // handle approximation case if there are additional characters after the decoded number
        if (i < end)
        {
            char t = text[i];
            switch (t)
            {
                case '-':
                    qDelta = -1.0f;
                    i++;
                    addDelta = true;
                    break;
                case '+':
                    addDelta = true;
                    i++;
                    break;
            }
            if (addDelta && i < end)    // for additive approximation only, we check a multiplier
            {
                if (CheckFloat(text, ref i, end, out float fct))
                    qDelta *= fct;      // no valid number is just an implied 1.0 at this point
            }
            if (i < end)
            {
                t = text[i];
                if (t == 'x' || t == 'y' || t == 'z' || t == 'n' || t == 'j' || t == 'X' && (i == end - 1 || text[i + 1] != 'e'))
                {
                    i++;
                    float xz = t == 'x' ? XApproximationParameter : t == 'y' ? YApproximationParameter : t == 'z' ? ZApproximationParameter : t == 'j' ? JApproximationParameter : NApproximationParameter;
                    approx = true;
                    if (addDelta)
                        value += qDelta * xz;
                    else if (value != 0.0f)
                        value *= xz;
                    else
                        value = hydrat ? ApproximativeHydratation : xz;
                }
            }
        }

        if (paren)
            nextchar = end + 1;
        else
            nextchar = i;
        return value != 0.0f;
    }

    /// <summary>
    /// ignore anything before the first meaningful character in the string, removes all codes below space
    /// </summary>
    /// <param name="inp">input text</param>
    /// <returns>resulting string</returns>
    private static string Clean(string inp) // ignore anything up to the leading uppercase letter or ( or [, remove spaces and control chars
    {
        int len = inp.Length;
        char[] temp = new char[len];
        bool ready = false;
        int ks = 0;
        bool blankLast = false;
        for (int k = 0; k < len; k++)
        {
            char c = inp[k];
            if (!ready && c > ' ')
                ready = true;
            bool blank = c == ' ';
            if (ready && c > ' ' /*&& c != delta*/)
            {
                if (blankLast && k + 2 < len && (c == '+' || c == '-') && inp[k + 1] > '0' && inp[k + 1] <= '6' && inp[k + 2] == ' ')
                    k += 2;
                else
                    temp[ks++] = c;
            }
            blankLast = blank;
        }
        return new string(temp, 0, ks);
    }

    private class StoreString
    {
        private int dataSize = 0;

        public int DataSize
        {
            get { return dataSize; }
            set { dataSize = value; }
        }

        public char[] Data { get; set; }
        public void Add(char c) { Data[DataSize++] = c; }
        public void Add(char[] a) { for (int i = 0; i < a.Length && a[i] != 0; i++) Data[DataSize++] = a[i]; }
        public StoreString(char[] d) { Data = d; }
    }

    /// <summary>
    /// Handles Parentheses
    /// </summary>
    /// <param name="Inp">input string</param>
    /// <param name="nextchar">index of next character to process in input</param>
    /// <param name="lnPresent">lnPresent set if Ln found</param>
    /// <returns>result with parentheses removed</returns>
    private static string PreTranslate(string Inp, out int nextchar, out bool lnPresent)
    {
        // add missing right brackets
        int nb = 0;
        for (int i = 0; i < Inp.Length; i++)
        {
            switch (Inp[i])
            {
                case '(':
                case '[':
                    nb++;
                    break;
                case ')':
                case ']':
                    nb--;
                    break;
            }
        }
        for (int i = 0; i < nb; i++) Inp += ")";
        //
        float nMult;
        Stack<char[]> pTemp = new Stack<char[]>();
        Stack<StoreString> pStore = new Stack<StoreString>();
        Stack<int> commas = new Stack<int>();
        //int next;

        pTemp.Push(new char[16384]);
        pStore.Push(new StoreString(pTemp.Peek()));
        commas.Push(0);
        bool errorSyntax = false;
        lnPresent = false;

        int jScan = 0;
        while (!errorSyntax && jScan < Inp.Length && Inp[jScan] != '/')
        {
            char c = Inp[jScan++];
            switch (c)
            {
                case '[':
                case '(':
                    pTemp.Push(new char[16384]);
                    pStore.Push(new StoreString(pTemp.Peek()));
                    commas.Push(0);
                    break;
                case ']':
                case ')':
                    if (pTemp.Count <= 1)
                    {
                        errorSyntax = true;
                        continue;
                    }
                    pStore.Peek().Add('\u0000');
                    if (!CheckNumber(Inp.ToCharArray(jScan, Inp.Length - jScan), false, out _, out nMult, out int next))
                        nMult = 1.0f;
                    else
                        jScan += next;
                    pStore.Pop();
                    if (!Multiply(pStore.Peek(), nMult / (1 + commas.Pop()), pTemp.Pop(), out bool ln))
                    {
                        errorSyntax = true;
                        break;
                    }
                    if (ln)
                        lnPresent = true;
                    break;
                case ',':
                    if (jScan == 1 || jScan > 1 && !char.IsLetter(Inp[jScan - 2]) || jScan < Inp.Length - 1 && !char.IsAsciiLetterUpper(Inp[jScan]))
                    {
                        errorSyntax = true;
                        break;
                    }
                    commas.Push(commas.Pop() + 1);
                    break;
                default:
                    pStore.Peek().Add(c);
                    break;
            }
        }
        if (!errorSyntax)
        {
            nextchar = jScan;
            if (pTemp.Count == 1)
            {
                char[] b1 = pTemp.Pop();
                Array.Resize<char>(ref b1, pStore.Pop().DataSize);
                return new string(b1);
            }
        }
        nextchar = 0;
        return null;
    }

    private static bool Multiply(StoreString output, float factor, char[] inp, out bool ln)
    {
        if (factor == 1.0f)
        {
            output.Add(inp);
            ln = false;
        }
        else
        {
            int len = 0;
            while (inp[len++] != 0) ;
            int[] pZ = new int[TableSize];
            float[] pnA = new float[TableSize];
            float[] zfr = new float[TableSize];
            int n = DecodeFormula1(new string(inp, 0, len), pZ, pnA, zfr, out _, true, out ln);
            if (n <= 0) return false;
            string nf;
            string result = "";
            for (int k = 0; k < n; k++)
            {
                nf = (pnA[k] * factor).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                result += gacElements[pZ[k]] + nf;
            }
            output.Add(result.ToCharArray());
            return true;
        }
        return true;
    }

    /// <summary>
    /// controls the rounding of concentrations
    /// </summary>
    public static bool isRoundTableSQD = true;

    /// <summary>
    /// epsilon value for concentration rounding
    /// </summary>
    public static float fRoundTableSQD = 1.192092896e-07F;

    /// <summary>
    /// table of element symbols starting at 0
    /// </summary>
    public static string[] gacElements =
        { ""
              , "H", "He", "Li", "Be", "B", "C", "N", "O", "F", "Ne"
              , "Na", "Mg", "Al", "Si", "P", "S", "Cl", "Ar", "K", "Ca"
              , "Sc", "Ti", "V", "Cr", "Mn", "Fe", "Co", "Ni", "Cu", "Zn"
              , "Ga", "Ge", "As", "Se", "Br", "Kr", "Rb", "Sr", "Y", "Zr"
              , "Nb", "Mo", "Tc", "Ru", "Rh", "Pd", "Ag", "Cd", "In", "Sn"
              , "Sb", "Te", "I", "Xe", "Cs", "Ba", "La", "Ce", "Pr", "Nd"
              , "Pm", "Sm", "Eu", "Gd", "Tb", "Dy", "Ho", "Er", "Tm", "Yb"
              , "Lu", "Hf", "Ta", "W", "Re", "Os", "Ir", "Pt", "Au", "Hg"
              , "Tl", "Pb", "Bi", "Po", "At", "Rn", "Fr", "Ra", "Ac", "Th"
              , "Pa", "U", "Np", "Pu", "Am", "Cm", "Bk", "Cf", "Es", "Fm"
              , "Md","No","D", "Ln", "T"
        };

    public static int AtomicNumber(string? element)
    {
        if (string.IsNullOrEmpty(element))
            return 0;
        for (int i = 0; i < gacElements.Length; i++)
            if (element == gacElements[i]) return i;
        return 0;
    }

    public static int AtomicNumber(char[] element, int start, int length)
    {
        char[] temp = new char[length];
        for (int i = 0; i < length && start < element.Length; i++)
            temp[i]  = i == 0 ? char.ToUpper((element[start++])) : element[start++];
        return AtomicNumber(new string(temp));
    }
    public static int AtomicNumber(char element) => AtomicNumber(new string(char.ToUpper(element), 1));

    // mendeleev table coloration

    private static readonly Color cA = Color.FromRgb(235, 193, 55);     // alkaline
    private static readonly Color aT = Color.FromRgb(231, 193, 149);    // alkaline-earth
    private static readonly Color tM = Color.FromRgb(246, 233, 218);    // transition metals
    private static readonly Color lN = Color.FromRgb(240, 207, 253);    // lanthanides
    private static readonly Color oM = Color.FromRgb(197, 220, 231);    // other metals
    private static readonly Color mO = Color.FromRgb(157, 179, 223);    // metalloids
    private static readonly Color nM = Color.FromRgb(218, 226, 190);    // non-metals
    private static readonly Color hA = Color.FromRgb(253, 253, 69);     // halogens
    private static readonly Color nG = Color.FromRgb(193, 250, 80);     // noble gases
    private static readonly Color aC = Color.FromRgb(191, 159, 251);    // actinides
    private static readonly Color rD = Colors.Red;
    private static readonly Color bK = Colors.Black;


    public static readonly Color[] elementsColor =
    {
        nM, // 0 unused
        nM, nG, cA, aT, mO, nM, nM, nM, hA, nG, // 1-10 etc.
        cA, aT, oM, mO, nM, nM, hA, nG, cA, aT,
        tM, tM, tM, tM, tM, tM, tM, tM, tM, tM,
        oM, mO, mO, nM, hA, nG, cA, aT, tM, tM,
        tM, tM, tM, tM, tM, tM, tM, tM, oM, oM,
        mO, mO, hA, nG, cA, aT, lN, lN, lN, lN,
        lN, lN, lN, lN, lN, lN, lN, lN, lN, lN,
        tM, tM, tM, tM, tM, tM, tM, tM, tM, tM,
        oM, oM, oM, mO, hA, nG, cA, aT, aC, aC,
        aC, aC, aC, aC, aC, aC, aC, aC, aC, aC,
        aC, aC, nM, lN, nM
    };
    public static readonly Color[] elementsForeColor =
    {
        nM, // 0 unused
        rD, rD, bK, bK, bK, bK, rD, rD, rD, rD, // 1-10 etc.
        bK, bK, bK, bK, bK, bK, rD, rD, bK, bK,
        bK, bK, bK, bK, bK, bK, bK, bK, bK, bK,
        bK, bK, bK, bK, bK, rD, bK, bK, bK, bK,
        bK, bK, bK, bK, bK, bK, bK, bK, bK, bK,
        bK, bK, bK, rD, bK, bK, bK, bK, bK, bK,
        bK, bK, bK, bK, bK, bK, bK, bK, bK, bK,
        bK, bK, bK, bK, bK, bK, bK, bK, bK, bK,
        bK, bK, bK, bK, bK, rD, bK, bK, bK, bK,
        bK, bK, bK, bK, bK, bK, bK, bK, bK, bK,
        bK, bK, bK, bK, bK
    };

    /// <summary>
    /// Table of atomic weights of elements starting at 0.
    /// </summary>
    /// <remarks>
    /// Table updated on 13.03.2012 by DD. Current IUPAC and NIST recommended data are included.
    /// </remarks>
    public static float[] gafAtomWeight =
        { 0.0f
              ,   1.0079f,   4.0026f,   6.9410f,   9.0122f,  10.8110f,  12.0107f,  14.0067f
              ,  15.9994f,  18.9984f,  20.1797f,  22.9898f,  24.3050f,  26.9815f,  28.0855f
              ,  30.9738f,  32.0650f,  35.4530f,  39.9480f,  39.0983f,  40.0780f,  44.9559f
              ,  47.8670f,  50.9415f,  51.9961f,  54.9380f,  55.8450f,  58.9332f,  58.6934f
              ,  63.5460f,  65.3800f,  69.7230f,  72.6400f,  74.9216f,  78.9600f,  79.9040f
              ,  83.7980f,  85.4678f,  87.6200f,  88.9059f,  91.2240f,  92.9064f,  95.9600f
              ,  98.0000f, 101.0700f, 102.9055f, 106.4200f, 107.8682f, 112.4110f, 114.8180f
              , 118.7100f, 121.7600f, 127.6000f, 126.9045f, 131.2930f, 132.9055f, 137.3270f
              , 138.9055f, 140.1160f, 140.9077f, 144.2420f, 145.0000f, 150.3600f, 151.9640f
              , 157.2500f, 158.9254f, 162.5000f, 164.9303f, 167.2590f, 168.9342f, 173.0540f
              , 174.9668f, 178.4900f, 180.9479f, 183.8400f, 186.2070f, 190.2300f, 192.2170f
              , 195.0840f, 196.9666f, 200.5900f, 204.3833f, 207.2000f, 208.9804f, 208.9824f
              , 209.9871f, 222.0176f, 223.0197f, 226.0254f, 227.0278f, 232.0381f, 231.0359f
              , 238.0289f, 237.0482f, 244.0496f, 243.0614f, 247.0704f, 247.0703f, 251.0796f
              ,  252.000f,  257.000f,  258.000f,  259.000f,    2.000f, 156.700f,  3.000f
            };

    /// <summary>
    /// Container for some useful element information
    /// </summary>
    public class ELEM
    {
        /// <summary>
        /// Oxydation number
        /// </summary>
        public int Oxydation
        {
            get { return elementData & 0xF; }
            private set { elementData &= ~0xF; elementData |= value & 0xF; }
        }
        /// <summary>
        /// usual valency except for oxygen
        /// </summary>
        public int Valency
        {
            get { return (elementData & 0xF0) >> 4; }
            private set { elementData &= ~0xF0; elementData |= (value & 0xF) << 4; }
        }
        /// <summary>
        /// element is electronegative
        /// </summary>
        public bool ElectroNegative
        {
            get { return (elementData & 0x100) != 0; }
            private set { if (value) elementData |= 0x100; else elementData &= ~0x100; }
        }
        /// <summary>
        /// element is halide
        /// </summary>
        public bool Halide
        {
            get { return (elementData & 0x200) != 0; }
            private set { if (value) elementData |= 0x200; else elementData &= ~0x200; }
        }
        /// <summary>
        /// element is uncommon in natural compounds
        /// </summary>
        public bool Rare
        {
            get { return (elementData & 0x400) != 0; }
            private set { if (value) elementData |= 0x400; else elementData &= ~0x400; }
        }
        /// <summary>
        /// valency when combined to oxygen
        /// </summary>
        public int OxygenBalance
        {
            get { return (elementData & 0x7800) >> 11; }
            private set { elementData &= ~0x7800; elementData |= (value & 0xF) << 11; }
        }

        internal ELEM(int oxidation, int valency, bool electroNegative, bool halide, bool rare, int oxygenBalance)
        {
            Oxydation = oxidation;
            Valency = valency;
            ElectroNegative = electroNegative;
            Halide = halide;
            Rare = rare;
            OxygenBalance = oxygenBalance;
        }

        int elementData = 0;
    }
    public static bool HasNoOxidation(int z)
    {
        return z == 8 || sData[z].Oxydation == 0;
    }

    /// <summary>
    /// table of element data for Z = 0 to 105
    /// </summary>
    static public ELEM[] sData = { new ELEM(0, 0, false, false, true, 2) // 0 = dummy
                                       , new ELEM(1, 1, false, false, false, 2) // 1 = H
                                       , new ELEM(0 , 0, false, false, true, 2), new ELEM(1, 1, false, false, false, 2), new ELEM(2, 2, false, false, false, 2) // 2-4
                                       , new ELEM(3, 0, false, false, false, 2), new ELEM(4, 0, false, false, false, 2), new ELEM(0, 2, true, false, false, 2) // 5-7
                                       , new ELEM(0, 0, false, false, false, 2), new ELEM(0, 1, true, true, false, 2), new ELEM(0, 0, false, false, true, 2) // 8-10
                                       , new ELEM(1, 1, false, false, false, 2), new ELEM(2, 2, false, false, false, 2), new ELEM(3, 3, false, false, false, 2) // 11-13
                                       , new ELEM(4, 4, false, false, false, 2), new ELEM(5, 0, false, false, false, 2), new ELEM(6, 0, false, false, false, 2) // 14-16
                                       , new ELEM(0, 1, true, true, false, 2), new ELEM(0, 0, false, false, true, 2), new ELEM(1, 1, false, false, false, 2) // 17-19
                                       , new ELEM(2, 2, false, false, false, 2), new ELEM(3, 3, false, false, false, 2), new ELEM(4, 4, false, false, false, 2) // 20-22
                                       , new ELEM(5, 5, false, false, false, 2), new ELEM(3, 6, false, false, false, 2), new ELEM(2, 4, false, false, false, 2) // 23-25
                                       , new ELEM(3, 3, false, false, false, 2), new ELEM(2, 2, false, false, false, 2), new ELEM(2, 2, false, false, false, 2) // 26-28
                                       , new ELEM(2, 2, false, false, false, 2), new ELEM(2, 2, false, false, false, 2), new ELEM(3, 3, false, false, false, 2) // 29-31
                                       , new ELEM(4, 4, false, false, false, 2), new ELEM(3, 0, false, false, false, 2), new ELEM(4, 0, false, false, false, 2) // 32-34
                                       , new ELEM(0, 1, true, true, false, 2), new ELEM(0, 0, false, false, true, 2), new ELEM(1, 1, false, false, false, 2) // 35-37
                                       , new ELEM(2, 2, false, false, false, 2), new ELEM(3, 3, false, false, false, 2), new ELEM(4, 4, false, false, false, 2) // 38-40
                                       , new ELEM(5, 5, false, false, false, 2), new ELEM(6, 6, false, false, false, 2), new ELEM(4, 0, false, false, true, 2) // 41-43
                                       , new ELEM(0, 0, false, false, false, 2), new ELEM(0, 0, false, false, false, 2), new ELEM(0, 0, false, false, false, 2) // 44-46
                                       , new ELEM(0, 2, false, false, false, 2), new ELEM(2, 2, false, false, false, 2), new ELEM(3, 3, false, false, false, 2) // 47-49
                                       , new ELEM(4, 4, false, false, false, 2), new ELEM(3, 5, false, false, false, 2), new ELEM(4, 6, false, false, false, 2) // 50-52
                                       , new ELEM(0, 1, true, true, false, 2), new ELEM(0, 0, false, false, true, 2), new ELEM(1, 1, false, false, false, 2) // 53-55
                                       , new ELEM(2, 2, false, false, false, 2), new ELEM(3, 3, false, false, false, 2), new ELEM(4, 4, false, false, false, 2) // 56-58
                                       , new ELEM(11, 4, false, false, false, 6), new ELEM(3, 3, false, false, false, 2), new ELEM(3, 3, false, false, true, 2) // 59-61
                                       , new ELEM(3, 3, false, false, false, 2), new ELEM(3, 3, false, false, false, 2), new ELEM(3, 3, false, false, false, 2) // 62-64
                                       , new ELEM(7, 3, false, false, false, 4), new ELEM(3, 3, false, false, false, 2), new ELEM(3, 3, false, false, false, 2) // 65-67
                                       , new ELEM(3, 3, false, false, false, 2), new ELEM(3, 3, false, false, false, 2), new ELEM(3, 3, false, false, false, 2) // 68-70
                                       , new ELEM(3, 3, false, false, false, 2), new ELEM(4, 4, false, false, false, 2), new ELEM(5, 5, false, false, false, 2) // 71-73
                                       , new ELEM(6, 6, false, false, false, 2), new ELEM(0, 3, false, false, false, 2), new ELEM(0, 4, false, false, false, 2) // 74-76
                                       , new ELEM(0, 0, false, false, false, 2), new ELEM(0, 0, false, false, false, 2), new ELEM(0, 0, false, false, false, 2) // 77-79
                                       , new ELEM(0, 4, false, false, false, 2), new ELEM(0, 3, false, false, false, 2), new ELEM(2, 4, false, false, false, 2) // 80-82
                                       , new ELEM(3, 4, false, false, false, 2), new ELEM(0, 0, false, false, true, 2), new ELEM(0, 1, true, true, true, 2) // 83-85
                                       , new ELEM(0, 0, false, false, true, 2), new ELEM(1, 1, false, false, true, 2), new ELEM(2, 2, false, false, true, 2) // 86-88
                                       , new ELEM(3, 3, false, false, true, 2), new ELEM(4, 4, false, false, false, 2), new ELEM(2, 2, false, false, true, 2) // 89-91
                                       , new ELEM(4, 6, false, false, false, 2), new ELEM(5, 0, false, false, true, 2), new ELEM(3, 0, false, false, true, 2) // 92-94
                                       , new ELEM(2, 0, false, false, true, 2), new ELEM(3, 0, false, false, true, 2), new ELEM(3, 0, false, false, true, 2) // 95-97
                                       , new ELEM(3, 0, false, false, true, 2), new ELEM(3, 0, false, false, true, 2), new ELEM(3, 0, false, false, true, 2) // 98-100
                                       , new ELEM(3, 0, false, false, true, 2), new ELEM(3, 0, false, false, true, 2) // 101 102
                                       , new ELEM(1, 1, false, false, false, 2), new ELEM(3, 3, false, false, false, 2), new ELEM(1, 1, false, false, false, 2) // 103 = D 104 =  Ln Don't use! , 105 = T(T added 6-Aug-2014 PC)
                                     };

    public static int[] normalGas =         // number of molecule in gazeous state IF element is gazeous in normal conditions
        {
            0
            , 2, 1, 0, 0, 0, 0, 2, 2, 1, 1
            , 0, 0, 0, 0, 0, 0, 1, 1, 0, 0
            , 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            , 0, 0, 0, 0, 0, 1, 0, 0, 0, 0
            , 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            , 0, 0, 0, 1, 0, 0, 0, 0, 0, 0
            , 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            , 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            , 0, 0, 0, 0, 0, 1, 0, 0, 0, 0
            , 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            , 0, 0, 2, 0, 2
        };

    private static readonly int[] lanthanides = { 57, 58, 59, 60, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71 };
    private static readonly char centeredDot = '\u00B7';
    private static readonly char centeredDot2 = '*';        //As an alternative for the centered dot
    private static readonly char centeredDot3 = '!';        //As an alternative for the centered dot
    private static readonly char centeredDot4 = '\u2022';   //As an alternative for the centered dot (DM 080409)
    private static readonly char delta = '\u0394';          // unicode Delta
    private static readonly char delta2 = '\u00C4';         // Delta
    private static readonly char delta3 = '\u25A1';         // Delta displayed as white square in PLU2019 (DM 131218)
    private static readonly char delta4 = '?';              // Delta displayed as ? in PLU2019 (DM 131218)

    /// <summary>
    /// replaces "x" in approximative concentration expression, default = 0.01
    /// </summary>
    public static float XApproximationParameter { get; set; } = 0.01f;

    /// <summary>
    /// replaces "y" in approximative concentration expression, default = 0.01
    /// </summary>
    public static float YApproximationParameter { get; set; } = 0.01f;

    /// <summary>
    /// replaces "z" in approximative concentration expression, default = 0.01
    /// </summary>
    public static float ZApproximationParameter { get; set; } = 0.01f;

    /// <summary>
    /// replaces "j" in approximative concentration expression, default = 0.01
    /// </summary>
    public static float JApproximationParameter { get; set; } = 0.01f;

    /// <summary>
    /// replaces "n" in approximative concentration expression, default = 10
    /// </summary>
    public static float NApproximationParameter { get; set; } = 10.0f;

    /// <summary>
    /// replaces "x" or "z" when they immediately follow a centered dot (generally indicating an unknown state of hydratation)
    /// </summary>
    public static float ApproximativeHydratation { get; set; } = 5.0f;
    public static void NormalOxide(int z, out string _oxide, out float factor)
    {
        int n1 = sData[z].Oxydation;
        int n2 = sData[z].OxygenBalance;
        string oxide = gacElements[z];
        if (n1 == 0)
        {
            factor = 1;
        }
        else
        {
            while (n1 % 2 == 0 && n2 % 2 == 0) { n1 /= 2; n2 /= 2; }
            while (n1 % 3 == 0 && n2 % 3 == 0) { n1 /= 3; n2 /= 3; }
            string num;
            if (n2 != 1) { num = n2.ToString(); oxide += num; }
            oxide += 'O';
            if (n1 != 1) { num = n1.ToString(); oxide += num; }
            factor = 1 + n1 * gafAtomWeight[8] / n2 / gafAtomWeight[z];
        }
        _oxide = oxide;
    }

    public enum OxidMode { elementOrOxide = 0, element = 1, oxide = 2 };

    public static bool IsElementOrNormalOxide(string formula, OxidMode mode)
    {
        int[] nZ = new int[MaxElementsD];
        float[] nA = new float[MaxElementsD];
        float[] zfrac = new float[MaxElementsD];
        int nEl = DecodeFormula1(formula, nZ, nA, zfrac, out _, false, out _);
        if (nEl > 2) return false;
        if (nEl <= 0 || sData[nZ[nEl - 1]].Rare || nEl != 1 && mode == OxidMode.element) return false;
        if (nEl == 1)
            return mode != OxidMode.oxide || sData[nZ[nEl - 1]].Oxydation == 0;
        if (nZ[0] != 8 && nZ[1] != 8) return false;
        return nZ[0] == 8 ? sData[nZ[1]].Oxydation * nA[1] == sData[nZ[1]].OxygenBalance * nA[0] :
            sData[nZ[0]].Oxydation * nA[0] == sData[nZ[0]].OxygenBalance * nA[1];
    }
}