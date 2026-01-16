using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace XRFCalc.XRF;

public static class AbsorptionCalc
{
    static readonly string accented = "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";
    static readonly string translat = "AAAAAAECEEEEIIIIDNOOOOOx0UUUUYPAAAAAAECEEEEIIIIONOOOOO/0UUUUYPY";
    static readonly string latin = "ABGDZzLlMNn";
    static readonly string greek = "αβγδζζλλμνν";

    internal static char TranslateAccented(char c)
    {
        int i = accented.IndexOf(c);
        return i >= 0 ? translat[i] : c;
    }

    private static char ToGreek(char c)
    {
        int i = latin.IndexOf(c);
        return i >= 0 ? greek[i] : c;
    }
    private static char ToLatin(char c)
    {
        int i = greek.IndexOf(c);
        return i >= 0 ? latin[i] : c;
    }
    public struct LineData
    {
        internal string iupac;          // IUPAC conforming name of the line e.g. K-L3
        internal string siegbahn;        // Siegbahn's notation e.g. KA1
        internal float energy;          // line energy in eV
        internal float transitionProb;	// transition probability (0->1)
        internal string SiegbahnGr
        {
            get
            {
                StringBuilder sb = new StringBuilder(siegbahn.Length);
                for (int i = 0; i < siegbahn.Length; i++)
                {
                    switch (i)
                    {
                        case 1:
                            sb.Append(ToGreek(siegbahn[i]));
                            break;
                        default:
                            sb.Append(siegbahn[i]);
                            break;
                    }
                }
                return sb.ToString();
            }
        }
    };

    public struct CKData
    {
        internal char id;           // id of final state e.g. L1->L2, id=2
        internal float fct;         // probability of Coster-Kronig transition
                                    //	        internal float	fctTotal;		// total probability including intermediate paths
    };

    public class EdgeData
    {
        internal char sp = ' ';             // shell letter (K,L,M,N,O,P)
        internal int id = 0;                // subshell number
        internal int z = 0;                 // atomic number for back reference, added PC 25-Nov-2004
        internal float energy = 0.0f;       // edge energy in eV
        internal float yield = 0.0f;        // fluorescence yield of shell
        internal float jumpFactor = 0.0f;	// jump factor of edge = mu(energy+1)/mu(energy-1)
        internal CKData[] ckData = null;
        internal LineData[] lineData = null;
        internal string EdgeName => sp == 'K' ? "K" : $"{sp}{id}";
    };

    public class PhotoData
    {
        internal float energyLog = 0.0f;    // ln(eV)
        internal float absorbLog = 0.0f;    // ln(cm2/g)
        internal float d2absorb = 0.0f;		// ln(d2mu/deV2)
    };

    public class ScatterData
    {
        internal float energyLog = 0.0f;		// ln(eV)
        internal float rayleighLog = 0.0f;
        internal float d2rayleigh = 0.0f;
        internal float comptonLog = 0.0f;
        internal float d2compton = 0.0f;
    };

    public class ElementData
    {
        internal int z = 0;                     // atomic number
        internal float A = 0.0f;                // atomic mass
        internal float density = 0.0f;			// density (solid state)
        internal EdgeData[] edges = null;
        internal PhotoData[] photo = null;
        internal ScatterData[] scatter = null;
    };

    public static float HC => 12.39852f;

    static bool initialized = false;

    private static readonly ElementData[] elements = new ElementData[Chemistry.MaxElementsD + 1];

    public static bool IsInitialized { get { return initialized; } }
    private static readonly char[] spaces = new char[] { ' ' };
    private static readonly char[] spacesOrNull = new char[] { '\0', ' ' };

    public static bool Initialize(string[] text)
    {
        if (initialized) return true;

        ElementData pE = null;
        bool bReadPhoto = false;
        bool bReadScatter = false;
        bool bReadLines = false;
        foreach (string str in text)
        {
            if (str == null) break;                         // the array might be padded with null strings at the end, so just quit.
            string strt = str.Trim();
            int nl = strt.Length;
            if (nl < 2 || strt[0] == '/' && strt[1] == '/')
                continue;
            string[] tokens = strt.Split(spaces, StringSplitOptions.RemoveEmptyEntries);
            if (nl > 9 && strt[..7].ToLower() == "element" && tokens.Length >= 5)
            {
                pE = null;
                bReadPhoto = false;
                bReadScatter = false;
                bReadLines = false;
                int.TryParse(tokens[2], out int nZ);
                if (nZ <= 0) continue;
                else if (nZ > Chemistry.MaxElementsD) break;
                pE = elements[nZ] = new ElementData();
                pE.z = nZ;
                float.TryParse(tokens[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out float x);
                pE.A = x;
                float.TryParse(tokens[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out x);
                pE.density = x;
                continue;
            }
            if (pE == null)
                continue;
            if (strt.ToLower() == "endelement")
            {
                pE = null;
                continue;
            }
            if (strt.Length > 4 && strt[..4].ToLower() == "edge" && tokens.Length >= 5)
            {
                bReadLines = false;
                // shell designation = tokens[1]
                EdgeData edge = new EdgeData
                {
                    sp = tokens[1][0]
                };
                if (edge.sp > 'K')
                {
                    int.TryParse(tokens[1][1..], out edge.id);
                    if (edge.id < 1 || edge.id > 11) continue;
                }
                float.TryParse(tokens[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out edge.energy);
                float.TryParse(tokens[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out edge.yield);
                float.TryParse(tokens[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out edge.jumpFactor);
                edge.z = pE.z;
                Array.Resize(ref pE.edges, pE.edges == null ? 1 : pE.edges.Length + 1);
                pE.edges[^1] = edge;
                continue;
            }
            if (strt.Length > 2 && strt[..2].ToLower() == "ck")
            {
                if (tokens[0].ToLower() == "cktotal")
                    continue;
                for (int i = 1; i < tokens.Length;)
                {
                    CKData ck = new CKData
                    {
                        id = tokens[i++][1]
                    };
                    float.TryParse(tokens[i++], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out ck.fct);
                    EdgeData ed = pE.edges[^1];
                    Array.Resize(ref ed.ckData, ed.ckData == null ? 1 : ed.ckData.Length + 1);
                    ed.ckData[^1] = ck;
                }
                continue;
            }
            else if (strt.ToLower() == "lines" && pE.edges != null)
            {
                bReadLines = true;
                bReadPhoto = false;
                bReadScatter = false;
            }
            else if (strt.ToLower() == "photo")
            {
                bReadLines = false;
                bReadPhoto = true;
                bReadScatter = false;
            }
            else if (strt.ToLower() == "scatter")
            {
                bReadLines = false;
                bReadPhoto = false;
                bReadScatter = true;
            }
            else if (bReadLines && tokens.Length >= 4)
            {
                LineData line = new LineData
                {
                    iupac = tokens[0],
                    siegbahn = tokens[1]
                };
                if (line.siegbahn[1] >= 'a' && line.siegbahn[1] <= 'g')
                {
                    char[] s = line.siegbahn.ToCharArray();
                    s[1] = char.ToUpperInvariant(s[1]); // upper case LA, LB, LG lower case Ll, etc.
                    line.siegbahn = new string(s);
                }
                float.TryParse(tokens[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out line.energy);
                float.TryParse(tokens[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out line.transitionProb);
                EdgeData ed = pE.edges[^1];
                Array.Resize(ref ed.lineData, ed.lineData == null ? 1 : ed.lineData.Length + 1);
                ed.lineData[^1] = line;
            }
            else if (bReadPhoto && tokens.Length >= 3)
            {
                PhotoData ph = new PhotoData();
                float.TryParse(tokens[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out ph.energyLog);
                float.TryParse(tokens[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out ph.absorbLog);
                float.TryParse(tokens[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out ph.d2absorb);
                Array.Resize(ref pE.photo, pE.photo == null ? 1 : pE.photo.Length + 1);
                pE.photo[^1] = ph;
            }
            else if (bReadScatter && tokens.Length >= 4)
            {
                ScatterData sc = new ScatterData();
                float.TryParse(tokens[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out sc.energyLog);
                float.TryParse(tokens[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out sc.rayleighLog);
                float.TryParse(tokens[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out sc.d2rayleigh);
                float.TryParse(tokens[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out sc.comptonLog);
                float.TryParse(tokens[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out sc.d2compton);
                Array.Resize(ref pE.scatter, pE.scatter == null ? 1 : pE.scatter.Length + 1);
                pE.scatter[^1] = sc;
            }
        }
        initialized = true;
        return true;
    }

    public static float Absorption(int z, float en)
    {
        if (!initialized)
            return 0.0f;
        if (z <= 0 || z > Chemistry.MaxElementsD) return 0.0f;
        if (en <= 0 || en > 100) return 0.0f;
        ElementData elem = elements[z];
        double x = Math.Log(en * 1000.0);
        int k = 0;
        float xl = elem.photo[0].energyLog;
        float xh = 0;
        for (; k < elem.photo.Length - 1; k++)
        {
            xh = elem.photo[k + 1].energyLog;
            if (x <= xh && (x > xl || k == 0))
                break;
            xl = xh;
        }
        PhotoData pd1 = elem.photo[k];
        PhotoData pd2 = elem.photo[k + 1];
        float h = xh - xl;
        double a = (xh - x) / h;
        double b = (x - xl) / h;
        return (float)Math.Exp(a * pd1.absorbLog + b * pd2.absorbLog + ((a * a * a - a) * pd1.d2absorb + (b * b * b - b) * pd2.d2absorb) * h * h / 6.0);
    }

    public static float Compton(int z, float en)
    {
        if (!initialized)
            return 0.0f;
        if (z <= 0 || z > Chemistry.MaxElementsD) return 0.0f;
        if (en <= 0 || en > 100) return 0.0f;
        ElementData elem = elements[z];
        double x = Math.Log(en * 1000.0);
        if (x < elem.scatter[0].energyLog || x > elem.scatter[^1].energyLog)
            return 0.0f;
        int k = 0;
        float xl = elem.scatter[0].energyLog;
        float xh = 0;
        for (; k < elem.scatter.Length - 1; k++)
        {
            xh = elem.scatter[k + 1].energyLog;
            if (x <= xh)
                break;
            xl = xh;
        }
        float h = xh - xl;
        ScatterData sd1 = elem.scatter[k];
        ScatterData sd2 = elem.scatter[k + 1];
        double a = (xh - x) / h;
        double b = (x - xl) / h;
        return (float)Math.Exp(a * sd1.comptonLog + b * sd2.comptonLog + ((a * a * a - a) * sd1.d2compton + (b * b * b - b) * sd2.d2compton) * h * h / 6.0);
    }

    public static float Rayleigh(int z, float en)
    {
        if (!initialized)
            return 0.0f;
        if (z <= 0 || z > Chemistry.MaxElementsD) return 0.0f;
        if (en <= 0 || en > 100) return 0.0f;
        ElementData elem = elements[z];
        double x = Math.Log(en * 1000.0);
        if (x < elem.scatter[0].energyLog || x > elem.scatter[^1].energyLog)
            return 0.0f;
        int k = 0;
        float xl = elem.scatter[0].energyLog;
        float xh = 0;
        for (; k < elem.scatter.Length - 1; k++)
        {
            xh = elem.scatter[k + 1].energyLog;
            if (x <= xh)
                break;
            xl = xh;
        }
        float h = xh - xl;
        ScatterData sd1 = elem.scatter[k];
        ScatterData sd2 = elem.scatter[k + 1];
        double a = (xh - x) / h;
        double b = (x - xl) / h;
        return (float)Math.Exp(a * sd1.rayleighLog + b * sd2.rayleighLog + ((a * a * a - a) * sd1.d2rayleigh + (b * b * b - b) * sd2.d2rayleigh) * h * h / 6.0);
    }

    private static readonly string[] li1c = new string[] { "BK", "CK", "NK", "OK", "FK", "KK", "PK", "SK", "VK", "YK", "WK", "IK", "UK", "WL", "IL", "UL", "UM", "WM", "IK", "VL", "YL", "WN", "UN" };

    internal static int VerifyLineName(ref string csL)
    {
        int l = csL.Length;
        char[] t = new char[l + 4];
        bool bSlash = false;
        bool spaces = false;
        char c;
        int k = 0;
        int i = 0;
        int n = 0;
        for (; n < l; n++)
        {
            c = ToLatin(csL[n]);
            if (c < ' ') continue;
            if (c == ' ')
            {
                if (spaces) continue;	// ignore multiple spaces
                spaces = true;
                if (i == 0 && !bSlash)	// PC 2-Dec-2009 would remove the first space in line comments!
                {
                    i++;
                    // PC 26-Apr-2002 : case e.g. Ca Ka1 did not work!
                    continue;
                }
                else if (k < 2) continue;
            }
            else
                spaces = false;
            if (c != ',' && c != '-' && !char.IsLetterOrDigit(c))
                bSlash = true;

            if (!bSlash && c >= 'a' && c <= 'z')
                c = char.ToUpperInvariant(c);
            t[k++] = c;
        }

        if (t[1] >= 'K' && t[1] <= 'O')
        {
            for (n = 0; n < li1c.Length; n++)
                if (t[0] == li1c[n][0] && t[1] == li1c[n][1]) break;
            if (n < li1c.Length)
            {
                for (i = k; i >= 1; i--) t[i + 2] = t[i];
                t[1] = t[2] = ' ';
            }
        }
        if (t[1] == ' ' && t[2] != ' ')
        {
            for (i = k; i >= 1; i--) t[i + 1] = t[i];
        }
        else
        {
            t[1] = char.ToLowerInvariant(t[1]);
            if (t[2] != ' ')
                for (i = k; i >= 2; i--) t[i + 1] = t[i];
            t[2] = ' ';
        }
        csL = new string(t).TrimEnd(spacesOrNull);
        if (csL.Length >= 2)
            return Chemistry.AtomicNumber(csL[..2].Trim());
        else
            return 0;
    }

    public static float FindEnergy(ref string lineOrEdge, out int isEdge, out string alternateDesignation)
    {
        isEdge = 0;
        alternateDesignation = "";
        if (!AbsorptionCalc.IsInitialized || lineOrEdge.Length < 2) return 0.0f;
        int z = VerifyLineName(ref lineOrEdge);
        if (z == 0 || lineOrEdge.Length < 4) return 0.0f;
        ElementData elem = elements[z];
        char edge = TranslateAccented(lineOrEdge[3]);
        int edgeNum = 0;
        float x = 0;
        if (lineOrEdge.Length == 4 && edge == 'K' || lineOrEdge.Length == 5 && char.IsDigit(lineOrEdge[4]) && int.TryParse(lineOrEdge.AsSpan(4, 1), out edgeNum))
        {
            for (int i = 0; i < elem.edges.Length; i++)
                if (elem.edges[i].sp == edge && elem.edges[i].id == edgeNum)
                {
                    isEdge = z;
                    x = elem.edges[i].energy / 1000.0f;
                }
            alternateDesignation = Chemistry.gacElements[z] + " " + (edge == 'K' ? "K" : $"{edge}{edgeNum}");
        }
        else if (lineOrEdge.Length >= 5)
        {
            StringBuilder sb = new StringBuilder(lineOrEdge.Length);
            for (int i = 3; i < lineOrEdge.Length; i++)
                sb.Append(TranslateAccented(lineOrEdge[i]));
            string temp = sb.ToString();
            string strElem = Chemistry.gacElements[z] + " ";
            for (int i = 0; i < elem.edges.Length; i++)
            {
                EdgeData ed = elem.edges[i];
                if (ed.sp == lineOrEdge[3] && ed.lineData != null)
                {
                    for (int l = 0; l < ed.lineData.Length; l++)
                    {
                        if (string.Compare(ed.lineData[l].siegbahn, temp, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            x = ed.lineData[l].energy / 1000.0f;
                            alternateDesignation = strElem + ed.lineData[l].iupac;
                            lineOrEdge = strElem + ed.lineData[l].SiegbahnGr;
                        }
                        else if (string.Compare(ed.lineData[l].iupac, temp, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            x = ed.lineData[l].energy / 1000.0f;
                            alternateDesignation = strElem + ed.lineData[l].SiegbahnGr;
                        }
                    }
                }
            }
        }
        return x;
    }

    public static float ElementDensity(int z)   // modified for perfect gases @ 0°C, 101325Pa
    {
        if (!initialized)
            return 0.0f;
        return Chemistry.normalGas[z] == 0 ? elements[z].density : Chemistry.normalGas[z] * Chemistry.gafAtomWeight[z] / 22414.0f;
    }

    public class LineInfo
    {
        public string Name;
        public bool IsEdge;
    }

    public static List<LineInfo> AllLines(int z, bool iupac)
    {
        List<LineInfo> lines = new List<LineInfo>();
        ElementData elem = elements[z];
        for (int i = 0; i < elem.edges.Length; i++)
        {
            EdgeData ed = elem.edges[i];
            if (ed.lineData != null)
            {
                lines.Add(new LineInfo() { Name = ed.EdgeName, IsEdge = true });
                for (int l = 0; l < ed.lineData.Length; l++)
                    lines.Add(new LineInfo() { Name = iupac ? ed.lineData[l].iupac : ed.lineData[l].SiegbahnGr });
            }
        }
        return lines.OrderBy(x => x.Name).ToList();
    }


}