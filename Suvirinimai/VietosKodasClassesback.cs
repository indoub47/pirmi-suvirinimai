using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace DefektoKodas
{
    enum KelioElementoTipas { joks = 0,
                                tarpstočio_lyginis_kelias,
                                tarpstočio_nelyginis_kelias,
                                stoties_pagrindinis_kelias,
                                atvykimo_išvykimo_kelias,
                                stoties_kelias,
                                trumpasis_kelias_nuo_iešmo_kairėn,
                                trumpasis_kelias_nuo_iešmo_dešinėn,
                                sankirta,
                                iešmas };
    enum SiūlėEnum {kairioji = 0, dešinioji = 9}


    #region Naudojamos klasės

    class VietosKodas
    {
        // aa.bccc.dd.ee.f
        public int aa { get; private set; }
        public int b { get; private set; }
        public int ccc { get; private set; }
        public int dd { get; private set; }
        public int ee { get; private set; }
        public int f { get; private set; }

        public VietosKodas(int AA, int BBBB, int CC, int DD, int E)
        {
            string invalidMessage = invalidKodasMessage(AA, BBBB, CC, DD, E);
            if (invalidMessage != string.Empty)
                throw new InvalidKodasException(invalidMessage);
            aa = AA;
            b = BBBB / 1000;
            ccc = BBBB % 1000;
            dd = CC;
            ee = DD;
            f = E;
        }

        public VietosKodas(DataRow row)
        {
            int k11; int k12; int k21; int k22; int k23; int k24; int k31; int k32; int k41; int k42; int k51;
            try
            {
                k11 = Convert.ToInt32(row["k11"]);
                k12 = Convert.ToInt32(row["k12"]);
                k21 = Convert.ToInt32(row["k21"]);
                k22 = Convert.ToInt32(row["k22"]);
                k23 = Convert.ToInt32(row["k23"]);
                k24 = Convert.ToInt32(row["k24"]);
                k31 = Convert.ToInt32(row["k31"]);
                k32 = Convert.ToInt32(row["k32"]);
                k41 = Convert.ToInt32(row["k41"]);
                k42 = Convert.ToInt32(row["k42"]);
                k51 = Convert.ToInt32(row["k51"]);
            }
            catch
            {
                throw new InvalidDuomenysException();
            }

            if (k11 < 0 || k11 > 9) throw new InvalidDuomenysException("k11 netinka");
            if (k12 < 0 || k12 > 9) throw new InvalidDuomenysException("k12 netinka");
            if (k21 < 0 || k21 > 9) throw new InvalidDuomenysException("k21 netinka");
            if (k22 < 0 || k22 > 9) throw new InvalidDuomenysException("k22 netinka");
            if (k23 < 0 || k23 > 9) throw new InvalidDuomenysException("k23 netinka");
            if (k24 < 0 || k24 > 9) throw new InvalidDuomenysException("k24 netinka");
            if (k31 < 0 || k31 > 9) throw new InvalidDuomenysException("k31 netinka");
            if (k32 < 0 || k32 > 9) throw new InvalidDuomenysException("k32 netinka");
            if (k41 < 0 || k41 > 9) throw new InvalidDuomenysException("k41 netinka");
            if (k42 < 0 || k42 > 9) throw new InvalidDuomenysException("k42 netinka");
            if (k51 < 0 || k51 > 9) throw new InvalidDuomenysException("k51 netinka");

            aa = k11 * 10 + k12;
            b = k21;
            ccc = k22 * 100 + k23 * 10 + k24;
            dd = k31 * 10 + k32;
            ee = k41 * 10 + k42;
            f = k51;

            string invalidMessage = invalidKodasMessage(aa, b*1000+ccc, dd, ee, f);
            if (invalidMessage != string.Empty)
                throw new InvalidKodasException(invalidMessage);
        }

        private string invalidKodasMessage(int aa, int bbbb, int cc, int dd, int e)
        {
            if (aa <= 0 || aa > 99)
                return "Kodo pirmosios grupės (AA.bbbb.cc.dd.e) skaičiai gali būti nuo 1 iki 99.";
            if (bbbb <= 0 || bbbb > 9999)
                return "Kodo antrosios grupės (aa.BBBB.cc.dd.e) skaičiai gali būti nuo 1 iki 9999.";
            if (bbbb / 1000 == 0)
                return "Kodo antrosios grupės pirmasis skaičius (aa.Bbbb.cc.dd.e) negali būti lygus 0.";
            if (bbbb % 1000 == 0)
                return "Kodo antrosios trys paskutinieji skaičiai (aa.bBBB.cc.dd.e) negali būti visi nuliai.";
            if (cc < 0 || cc > 99)
                return "Kodo trečiosios grupės (aa.bbbb.CC.dd.e) skaičiai gali būti nuo 0 iki 99.";
            if (dd < 0 || dd > 99)
                return "Kodo ketvirtosios grupės (aa.bbbb.cc.DD.e) skaičiai gali būti nuo 0 iki 99.";
            if (e < 0 || e > 9)
                return "Kodo paskutinis skaičius (aa.bbbb.cc.dd.E) gali būti nuo 0 iki 9.";
            return string.Empty;
        }

        public VietosKodas(string AABBBBCCDDE)
        {
            Regex rgx;
            string strKodas = AABBBBCCDDE.Trim();

            if (Regex.Match(strKodas, @"\d{11}").Success) //jei vieni skaičiai
            {
                rgx = new Regex(@"(\d{2})(\d)(\d{3})(\d{2})(\d{2})(\d)");
            }
            else if (strKodas.IndexOf(".") > -1)
            {
                rgx = new Regex(@"(\d{2})\.(\d)(\d{3})\.(\d{2})\.(\d{2})\.(\d)");
            }
            else
            {
                throw new InvalidKodasException("Neteisinga kodo struktūra. Formatas turi būti: xx.xxxx.xx.xx.x");
            }

            Match rgxMatch = rgx.Match(strKodas);
            if (!rgxMatch.Success || strKodas!=rgxMatch.Groups[0].Value)
                throw new InvalidKodasException("Neteisinga kodo struktūra. Formatas turi būti: xx.xxxx.xx.xx.x");

            aa = Convert.ToInt32(rgxMatch.Groups[1].Value);
            b = Convert.ToInt32(rgxMatch.Groups[2].Value);
            ccc = Convert.ToInt32(rgxMatch.Groups[3].Value);
            dd = Convert.ToInt32(rgxMatch.Groups[4].Value);
            ee = Convert.ToInt32(rgxMatch.Groups[5].Value);
            f = Convert.ToInt32(rgxMatch.Groups[6].Value);

            string invalidMessage = invalidKodasMessage(aa, b*1000+ccc, dd, ee, f);
            if (invalidMessage != string.Empty)
                throw new InvalidKodasException(invalidMessage);
        }

        public VietosKodas(int[] kodas)
        {
            if (kodas.Length < 5)
                throw new InvalidKodasException("Kodas turi būti iš 5 grupių (xx.xxxx.xx.xx.x)");
            string invalidMessage = invalidKodasMessage(kodas[0], kodas[1], kodas[2], kodas[3], kodas[4]);
            if (invalidMessage != string.Empty)
                throw new InvalidKodasException(invalidMessage);

            aa = kodas[0];
            b = kodas[1] / 1000;
            ccc = kodas[1] % 1000;
            dd = kodas[2];
            ee = kodas[3];
            f = kodas[4];
        }

        public VietosKodas(string[] kodas)
        {
            if (kodas.Length < 5) 
                throw new InvalidKodasException();
            try
            {
                aa = Convert.ToInt32(kodas[0]);
                b = Convert.ToInt32(kodas[1]) / 1000;
                ccc = Convert.ToInt32(kodas[1]) % 1000;
                dd = Convert.ToInt32(kodas[2]);
                ee = Convert.ToInt32(kodas[3]);
                f = Convert.ToInt32(kodas[4]);
            }
            catch
            {
                throw new InvalidKodasException("Kodą turi sudaryti skaitmenys.");
            }

            string invalidMessage = invalidKodasMessage(aa, b * 1000 + ccc, dd, ee, f);
            if (invalidMessage != string.Empty)
                throw new InvalidKodasException(invalidMessage);
        }
    }
    

    class Siūlė
    {
        public SiūlėEnum kodas { get; private set; }
        public string pavadinimas
        {
            get
            {
                return kodas.ToString("G");
            }
        }

        public Siūlė(SiūlėEnum sl)
        {
            kodas = sl;
        }
        
        public Siūlė(int sKodas)
        {
            try
            {
                kodas = (SiūlėEnum)sKodas;
            }
            catch
            {
                throw;
            }
        }
    }


    class Stotis : IComparable<Stotis>
    {
        public int kodas { get; private set; }
        public string pavadinimas { get; private set; }
        public int nuo { get; private set; }
        public int iki { get; private set; }
        public Linija linija { get; private set; }

        public Stotis(int pKodas, string pPavadinimas, int pNuo_m, int pIki_m, Linija pLinija)
        {
            kodas = pKodas;
            pavadinimas = pPavadinimas;
            nuo = pNuo_m;
            iki = pIki_m;
            linija = pLinija;            
        }

        public int CompareTo(Stotis other)
        {
            return Math.Max(nuo, iki).CompareTo(Math.Max(other.nuo, other.iki));
        }

        public bool yraDidelė
        {
            get { return kodas != 0; }
        }
    }


    class Linija
    {
        public int kodas { get; private set; }
        public string pavadinimas { get; private set; }
        //public int pradzia { get; private set; }
        //public int pabaiga { get; private set; }

        public Linija (int lin_kodas, string lin_pavadinimas/*, int pradziosM, int  pabaigosM*/)
        {
            kodas = lin_kodas;
            pavadinimas = lin_pavadinimas;
            //pradzia = pradziosM;
            //pabaiga = pabaigosM;
        }
    }


    class Tarpstotis
    {
        public Stotis stotis1 { get; private set; }
        public Stotis stotis2 { get; private set; }

        public Tarpstotis (Stotis stotis_1, Stotis stotis_2)
        {
            if (stotis_1.iki < stotis_2.nuo)
            {
                stotis1 = stotis_1;
                stotis2 = stotis_2;
            }
            else
            {
                stotis1 = stotis_2;
                stotis2 = stotis_1;
            }
        }

        public int ilgis_m
        {
            get
            {
                return stotis2.nuo - stotis1.iki;
            }
        }
    }

    
    class KelioElementas
    {
        public KelioElementoTipas tipas { get; private set; }
        public object nr;
        public int numeris
        {
            get {return (int)nr; }
            private set { nr = value; }
        }
        public KelioElementas(KelioElementoTipas tp, int numero)
        {
            tipas = tp;
            nr = numero;
        }
        public KelioElementas(KelioElementoTipas tp)
        {
            tipas = tp;
        }
    }


    class KelioKoordinatė
    {
        // skaičiai saugomi object, tam, kad neinicializuotų nuliais,
        // kai būna nenurodytas piketas arba metras
        private object okm;
        private object opk;
        private object om;

        public int km
        {
            get
            {
                return (int)okm;
            }
            private set
            {
                okm = (object)value;
            }
        }
        public object pk
        {
            get
            {
                return (int)opk;
            }
            private set
            {
                opk = (object)value;
            }
        }
        public object m
        {
            get
            {
                return (int)om;
            }
            private set
            {
                om = (object)value;
            }
        }
        public KelioKoordinatė(int kkKm, int kkPk, int kkM)
        {
            km = kkKm;
            pk = kkPk;
            m = kkM;
        }
        public KelioKoordinatė(int kkKm)
        {
            km = kkKm;
        }
    }

    class Atstumas
    {
        public int m { set; get; }
        public float km
        {
            set { m = (int)Math.Round(value * 1000); }
            get { return Convert.ToSingle(m / 1000.0); }
        }
        public Atstumas(int metrai)
        {
            m = metrai;
        }
    }

    class Sandūra
    {
        private object oNr;
        public int Nr
        {
            get
            {
                return (int)oNr;
            }
            private set
            {
                oNr = (object)value;
            }
        }

        public Sandūra(int num)
        {
            oNr = num;
        }
    }
   

    struct Vieta
    {
        public string kodas;
        public List<Linija> galimosLinijos;
        public Stotis stotis;
        public Tarpstotis tarpstotis;
        public KelioElementas kelioElementas;
        public KelioKoordinatė koordinatė;
        public Atstumas atstumasNuoIešmo;
        public Siūlė siūlė;
        public Sandūra iešmoSandūra;
    }

    #endregion


    static class LGIF
    {
        public static Dictionary<int, Linija> linijos = new Dictionary<int, Linija>();
        public static Dictionary<int, List<Stotis>> infrastruktūra = new Dictionary<int, List<Stotis>>();
        private static string dataFileName = "lgif.xml";
        private static Dictionary<string, string> ExceptMsgs;

        static LGIF()
        {
            ExceptMsgs = new Dictionary<string, string>();
            ExceptMsgs.Add("Be_duomenų_failo",
                "Be duomenų failo programos darbas neįmanomas.");
            ExceptMsgs.Add("Nepavyksta_perskaityti_failo",
                "Nepavyksta perskaityti duomenų failo {0}. Įsitikinkite, kad failas {0} nebuvo ištrintas, pervardintas arba perkeltas į kitą vietą kompiuterio diske.");
            ExceptMsgs.Add("Klaida_nuskaitant_duomenis",
                "Klaida nuskaitant duomenis iš failo {0}. Gali būti, kad redaguojant failą {0}, neteisingai įrašyti duomenys.");
        }

        private static int xmlToM(XmlNode kKoord)
        {
            try
            {
                return (Convert.ToInt32(kKoord["km"].InnerText) - 1) * 1000 + (Convert.ToInt32(kKoord["pk"].InnerText) - 1) * 100 + Convert.ToInt32(kKoord["m"].InnerText);
            }
            catch
            {
                return -1;
            }
        }       

        public static void loadData()
        {
            
            if (!File.Exists(dataFileName))
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Pasirinkite duomenų failą";
                    ofd.Filter = "*.xml|*.xml|Visi|*.*";
                    ofd.FileName = dataFileName;
                    if (ofd.ShowDialog() == DialogResult.OK)
                     dataFileName = ofd.FileName;
                    else
                        throw new ReadFileFailureException(ExceptMsgs["Be_duomenų_failo"]);
                }
            }

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(dataFileName);
            }
            catch
            {
                throw new ReadFileFailureException(
                    string.Format(ExceptMsgs["Nepavyksta_perskaityti_failo"],
                    Path.GetFileName(dataFileName)));
            }

            int linijosKodas, stotiesKodas;
            string linijosPavadinimas, stotiesPavadinimas;
            int stotisNuo, stotisIki;

            try
            {
                Linija currentLinija;
                List<Stotis> linijosStotys;

                XmlNodeList ndlLinijos = doc.SelectNodes("/infrastruktura/linijos/linija");
                foreach (XmlNode ndLinija in ndlLinijos)
                {
                    linijosKodas = Convert.ToInt32(ndLinija["kodas"].InnerText);
                    linijosPavadinimas = ndLinija["pavadinimas"].InnerText;
                    currentLinija = new Linija(linijosKodas, linijosPavadinimas);

                    LGIF.linijos.Add(linijosKodas, currentLinija);
                    linijosStotys = new List<Stotis>();                    

                    XmlNodeList ndlStotys = ndLinija.SelectNodes("stotys/stotis");
                    foreach (XmlNode ndStotis in ndlStotys)
                    {
                        stotiesKodas = Convert.ToInt32(ndStotis["kodas"].InnerText);
                        stotiesPavadinimas = ndStotis["pavadinimas"].InnerText;
                        stotisNuo = xmlToM(ndStotis["nuo"]);
                        stotisIki = xmlToM(ndStotis["iki"]);
                        linijosStotys.Add(new Stotis(stotiesKodas, stotiesPavadinimas, stotisNuo, stotisIki, currentLinija));
                    }
                    // surūšiuoti linijos stotis
                    linijosStotys.Sort();
                    LGIF.infrastruktūra.Add(linijosKodas, linijosStotys);                    
                }

            }
            catch
            {
                throw new ReadFileFailureException(
                    string.Format(ExceptMsgs["Klaida_nuskaitant_duomenis"],
                    Path.GetFileName(dataFileName)));
            }
        }
    }
    
    
    static class VietosKodasParser
    {
        private static Dictionary<string, string> ExceptMsgs;
        const int  MAX_SANDURU_IESME = 12;

        static VietosKodasParser()
        {
            ExceptMsgs = new Dictionary<string, string>();
            ExceptMsgs.Add("dd_nuo_1_iki_10",
                "Pirmieji du kodo skaičiai (XX.xxxx.xx.xx.x) rodo, kad defektas yra arba tarpstotyje, arba mažoje stotyje, todėl kodo trečios grupės skaičiai (xx.xxxx.XX.xx.x) žymi kelio koodinatės piketą ir turi būti nuo 1 iki 10.");
            ExceptMsgs.Add("Koordinatė_netinka_linijai",
                "Kode nurodyta kelio koordinatė xx.xXXX.XX.XX.x netinka linijai XX.xxxx.xx.xx.x");
            ExceptMsgs.Add("Koordinatė_tinka_stočiai",
                "Kode nurodyta linijos XX.xxxx.xx.xx.x kelio koordinatė xx.xXXX.XX.XX.x yra didelėje stotyje (\"{0}\"). XX.xxxx.xx.xx.x turėtų būti tos stoties kodas ({1}).");
            ExceptMsgs.Add("Siūlė_0_arba_9",
                "Keliuose paskutinis kodo skaičius (xx.xxxx.xx.xx.X) reiškia siūlę ir gali būti tiktai 0 arba 9.");
            ExceptMsgs.Add("Mažos_iešmas_ne_0_ne_9",
                "Kai mažoje stotyje antros grupės pirmasis skaičius (xx.Xxxx.xx.xx.x) yra 6-9, paskutinis kodo skaičius (xx.xxxx.xx.xx.X) reiškia mažos stoties iešmo numerį ir negali būti 0 ir 9.");
            ExceptMsgs.Add("Tarpstočio_kelias_1_arba_2",
                "Tarpstotyje kodo antrosios grupės pirmasis skaičius (xx.Xxxx.xx.xx.x) žymi lyginį arba nelyginį kelią ir gali būti tiktai 1 arba 2.");
            ExceptMsgs.Add("Nėra_stoties_su_koordinate",
                "Nėra stoties XX.xxxx.xx.xx.x su kelio koordinate xx.xXXX.XX.XX.x, kaip nurodyta šitame kode.");
            ExceptMsgs.Add("Nėra_tokios_linijos_stoties",
                "Nėra nei linijos, nei stoties XX.xxxx.xx.xx.x, kokia nurodyta šitame kode.");
            ExceptMsgs.Add("Atstumas_nuo_iešmo_iešme",
                "Kai defektas yra didelės stoties iešme (trečias skaičius - 8 arba 9), neturi prasmės didesnis už nulį atstumas nuo iešmo - trečios ir ketvirtos grupės skaičiai xx.8xxx.XX.XX.x turi būti nuliai.");

            ExceptMsgs.Add("xx.Xxxx.xx.xx.x negali būti 8",
                "Linijoje arba mažoje stotyje antros grupės pirmasis skaičius (xx.Xxxx.xx.xx.x) negali būti 8.");
            ExceptMsgs.Add("xx.xxxx.xx.xx.X turi būti 0",
                "Kai suvirinimas yra iešme, kodo paskutinis skaičius (xx.xxxx.xx.xx.X) turi būti 0.");
            ExceptMsgs.Add("xx.xXXX.xx.xx.x netinka mažai stočiai",
                "Trečiasis kodo skaičius 9 rodo, kad suvirinimas yra mažos stoties iešme.Kodo antrosios grupės trimis paskutiniaisiais skaičiais (xx.xXXX.xx.xx.x) išreikštas kelio koordinatės kilometras netinka nė vienai tos linijos mažai stočiai.");
            ExceptMsgs.Add("xx.xxxx.XX.xx.x gali būti nuo 1 iki 8",
                "Kai suvirinimas yra mažosios stoties iešme, kodo trečiosios grupės skaičiai (xx.xxxx.XX.xx.x) žymi iešmo numerį, kuris mažoje stotyje gali būti nuo 1 iki 8.");
            ExceptMsgs.Add("xx.xxxx.xx.XX.x gali būti nuo 1 iki MAX_SANDŪRŲ",
                "Kai suvirinimas yra iešme, kodo ketvirtosios grupės skaičiai (xx.xxxx.xx.XX.x) žymi iešmo sandūros numerį, kuris gali būti nuo 1 iki {0}.");
            ExceptMsgs.Add("xx.Xxxx.xx.xx.x gali būti 1 arba 2",
                "Tarpstotyje antrosios grupės pirmasis skaičius (xx.Xxxx.xx.xx.x) žymi tarpstočio kelio numerį ir gali būti tiktai 1 arba 2.");
            ExceptMsgs.Add("xx.xxxx.XX.xx.x gali būti nuo 1 iki 10",
                "Kodo trečiosios grupės skaičiais (xx.xxxx.XX.xx.x) žymimas kelio koordinatės piketas gali būti nuo 1 iki 10.");
            ExceptMsgs.Add("xx.xxxx.xx.xx.X gali būti 0 arba 9",
                "Jeigu suvirinimas kelyje, paskutinis kodo skaičius (xx.xxxx.xx.xx.X) reiškia kelio siūlę ir gali būti tiktai 0 arba 9.");
            ExceptMsgs.Add("xx.xXXX.XX.XX.x netinka linijai",
                "Kode nurodyta kelio koordinatė xx.xXXX.XX.XX.x netinka linijai XX.xxxx.xx.xx.x");
            ExceptMsgs.Add("xx.xXXX.XX.XX.x netinka stočiai",
                "Kode nurodyta kelio koordinatė xx.xXXX.XX.XX.x netinka stočiai XX.xxxx.xx.xx.x");
            ExceptMsgs.Add("Mažoje stotyje trumpajame kelyje xx.xxxx.xx.xx.X gali būti 1-8",
                "Jeigu suvirinimas yra mažos stoties trumpajame kelyje, paskutinis kodo skaičius xx.xxxx.xx.xx.X reiškia iešmo, nuo kurio prasideda trumpasis kelias, numerį ir gali būti nuo 1 iki 8.");
            ExceptMsgs.Add("Mažoje stotyje netrumpajame kelyje xx.xxxx.xx.xx.X gali būti 0 arba 9",
                "Jeigu suvirinimas yra mažos stoties kelyje (ne trumpajame), paskutinis kodo skaičius xx.xxxx.xx.xx.X reiškia siūlę ir gali būti tiktai 0 arba 9.");

            // dar nesutvarkyti
            ExceptMsgs.Add("Tokios stoties nėra", "Tokios stoties nėra");

        }

        public static Vieta parseDefektas(VietosKodas kodas)
        {
            // AA.BCCC.DD.EE.F

            int aa = kodas.aa;
            int b = kodas.b;
            int ccc = kodas.ccc;
            int dd = kodas.dd;
            int ee = kodas.ee;
            int f = kodas.f;


            Vieta vieta = new Vieta();
            vieta.galimosLinijos = new List<Linija>();

            vieta.kodas = aa.ToString("00") + "." + b.ToString() + ccc.ToString("000") + "." + dd.ToString("00") + "." + ee.ToString("00") + "." + f.ToString();
            
            // ar linija
            List<Stotis> lin = null;
            Stotis stt = null;
            int dist;

            if (LGIF.infrastruktūra.ContainsKey(aa)) // jeigu yra tokia linija, kokia nurodyta AA
            {
                if (dd > 10 || dd < 1)
                    throw new InvalidKodasException(ExceptMsgs["dd_nuo_1_iki_10"]);
                lin = LGIF.infrastruktūra[aa];
                vieta.galimosLinijos.Add(LGIF.linijos[aa]);

                // tikrinti, ar kode nurodyta kelio koordinate linijai tinka  
                dist = (ccc - 1) * 1000 + (dd - 1) * 100 + ee; 
                int start, end;

                start = lin.First().nuo != -1 ? lin.First().nuo : lin.First().iki;
                end = lin.Last().iki != -1 ? lin.Last().iki : lin.Last().nuo;

                if (dist < start || dist > end)
                    throw new InvalidKodasException(ExceptMsgs["Koordinatė_netinka_linijai"]);

                // tikrinti, ar nurodyta kelio koordinatė nėra didelėje stotyje (nes tada AA vietoje turėtų būti didelės stoties kodas)
                foreach (Stotis st in lin)
                {
                    if (st.kodas != 0 && st.nuo != -1 && st.iki != -1)
                        if (dist >= st.nuo && dist <= st.iki)
                            throw new InvalidKodasException(string.Format(ExceptMsgs["Koordinatė_tinka_stočiai"], st.pavadinimas, st.kodas));
                }

                // ieškoti - stoties arba tarpstočio
                for (int i=0; i < lin.Count; i++)
                {
                    if (dist >= lin[i].nuo && dist <= lin[i].iki) // maža stotis
                    {
                        vieta.stotis = lin[i];
                        switch (b)
                        {
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                if (f != 0 && f != 9) throw new InvalidKodasException(ExceptMsgs["Siūlė_0_arba_9"]);
                                vieta.kelioElementas = new KelioElementas(KelioElementoTipas.stoties_kelias, b);
                                vieta.siūlė = new Siūlė(f);
                                break;
                            case 6:
                                if (f == 0 || f == 9) throw new InvalidKodasException(ExceptMsgs["Mažos_iešmas_ne_0_ne_9"]);
                                vieta.kelioElementas = new KelioElementas(KelioElementoTipas.trumpasis_kelias_nuo_iešmo_kairėn, f);
                                break;
                            case 7:
                                if (f == 0 || f == 9) throw new InvalidKodasException(ExceptMsgs["Mažos_iešmas_ne_0_ne_9"]);
                                vieta.kelioElementas = new KelioElementas(KelioElementoTipas.trumpasis_kelias_nuo_iešmo_dešinėn, f);
                                break;
                            case 8:
                            case 9:
                                if (f == 0 || f == 9) throw new InvalidKodasException(ExceptMsgs["Mažos_iešmas_ne_0_ne_9"]);
                                vieta.kelioElementas = new KelioElementas(KelioElementoTipas.iešmas, f);
                                break;
                        }
                        vieta.koordinatė = new KelioKoordinatė(ccc, dd, ee);
                        break;
                    }
                    else if (dist < lin[i + 1].nuo)
                    {
                        vieta.tarpstotis = new Tarpstotis(lin[i], lin[i + 1]);
                        if (f != 0 && f != 9)
                            throw new InvalidKodasException(ExceptMsgs["Siūlė_0_arba_9"]);

                        switch (b)
                        {
                            case 1:
                                vieta.kelioElementas = new KelioElementas(KelioElementoTipas.tarpstočio_nelyginis_kelias, b);
                                // Čia - klausimas. Formaliai šnekant, b nuorodo tik, kelias lyginis, ar nelyginis, bet nieko nesako apie jo numerį
                                // Žinoma, galima tikėtis, kad nelyginio kelio numeris bus 1, o lyginio - 2. Bet ar yra tokia taisyklė, tai nežinau.
                                break;
                            case 2:
                                vieta.kelioElementas = new KelioElementas(KelioElementoTipas.tarpstočio_lyginis_kelias, b);
                                break;
                            default:
                                // b t.b. 1 arba 2
                                throw new InvalidKodasException(ExceptMsgs["Tarpstočio_kelias_1_arba_2"]);
                        }

                        vieta.koordinatė = new KelioKoordinatė(ccc, dd, ee);
                        vieta.siūlė = new Siūlė(f);
                        break;
                    }
                }
            }                
            else // ieško didelės stoties
            {
                if (b == 1) // jeigu nurodyta kelio koordinatė, ta pati stotis (tas pats AA) gali būti keliose skirtingose linijose.
                    // Todėl reikia rasti tą liniją, kurioje tai stočiai tinka kelio koordinatė, nurodyta BBB.CC.DD
                {
                    if (dd > 10 || dd < 1)
                        throw new InvalidKodasException(ExceptMsgs["dd_nuo_1_iki_10"]);
                    dist = (ccc-1)*1000 + (dd-1)*100 + ee;
                    foreach (List<Stotis> ln in LGIF.infrastruktūra.Values)
                    {
                        stt = ln.Find(delegate(Stotis stotis) { return (stotis.kodas == aa && dist <= stotis.iki && dist >= stotis.nuo); });
                        if (stt != null)
                        {
                            vieta.galimosLinijos.Add(stt.linija);
                            vieta.stotis = stt;
                        }
                    }
                    if (vieta.stotis == null)
                        throw new InvalidKodasException(ExceptMsgs["Nėra_stoties_su_koordinate"]);
                }
                else // jeigu kelio koordinatė nenurodyta, užtenka rasti didelę stotį.
                    // Čia - nevienareikšmiškumas. Kode nesimato, kokiai linijai priklauso konkretus iešmas ar stoties kelias
                {
                    List<Stotis> rastosStotys = new List<Stotis>();
                    foreach (List<Stotis> ln in LGIF.infrastruktūra.Values)
                    {
                        stt = ln.Find(delegate(Stotis stotis) { return (stotis.kodas == aa); });
                        if (stt != null)
                        {
                            rastosStotys.Add(stt);
                            vieta.galimosLinijos.Add(stt.linija);
                        }
                    }

                    if (rastosStotys.Count == 0)
                        throw new InvalidKodasException(ExceptMsgs["Nėra_tokios_linijos_stoties"]);
                    else
                        vieta.stotis = rastosStotys[0];
                }

                if (f != 0 && f != 9)
                    throw new InvalidKodasException(ExceptMsgs["Siūlė_0_arba_9"]);
                else
                    vieta.siūlė = new Siūlė(f);

                switch (b)
                {
                    case 1:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.stoties_pagrindinis_kelias, b);
                        vieta.koordinatė = new KelioKoordinatė(ccc, dd, ee);
                        break;
                    case 2:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.stoties_pagrindinis_kelias, ccc);
                        vieta.atstumasNuoIešmo = new Atstumas(dd * 100 + ee);
                        break;
                    case 3:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.atvykimo_išvykimo_kelias, ccc);
                        vieta.atstumasNuoIešmo = new Atstumas(dd * 100 + ee);
                        break;
                    case 4:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.stoties_kelias, ccc);
                        vieta.atstumasNuoIešmo = new Atstumas(dd * 100 + ee);
                        break;
                    case 5:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.sankirta, ccc);
                        vieta.atstumasNuoIešmo = new Atstumas(dd * 100 + ee);
                        break;
                    case 6:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.trumpasis_kelias_nuo_iešmo_kairėn, ccc);
                        vieta.atstumasNuoIešmo = new Atstumas(dd * 100 + ee);
                        break;
                    case 7:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.trumpasis_kelias_nuo_iešmo_dešinėn, ccc);
                        vieta.atstumasNuoIešmo = new Atstumas(dd * 100 + ee);
                        break;
                    case 8:
                    case 9:
                        if (dd != 0 || ee != 0)
                            throw new InvalidKodasException(ExceptMsgs["Atstumas_nuo_iešmo_iešme"]);
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.iešmas, ccc);
                        vieta.atstumasNuoIešmo = new Atstumas(0);
                        break;
                }                
            }
            return vieta;
        }

        public static Vieta parseSuvirinimas(VietosKodas kodas)
        {
            // AA.BCCC.DD.EE.F

            int aa = kodas.aa;
            int b = kodas.b;
            int ccc = kodas.ccc;
            int dd = kodas.dd;
            int ee = kodas.ee;
            int f = kodas.f;


            Vieta vieta = new Vieta();
            vieta.galimosLinijos = new List<Linija>();

            vieta.kodas = aa.ToString("00") + "." + b.ToString() + ccc.ToString("000") + "." + dd.ToString("00") + "." + ee.ToString("00") + "." + f.ToString();

            // ar linija
            List<Stotis> lin = null;
            Stotis stt = null;
            int dist;

            if (LGIF.infrastruktūra.ContainsKey(aa)) // jeigu yra tokia linija, kokia nurodyta AA
            {
                lin = LGIF.infrastruktūra[aa];
                if (b == 8) throw new InvalidKodasException(ExceptMsgs["xx.Xxxx.xx.xx.x negali būti 8"]);
                vieta.galimosLinijos.Add(LGIF.linijos[aa]);

                if (b == 9) //  jeigu iešmas
                {
                    if (f != 0) throw new InvalidKodasException(ExceptMsgs["xx.xxxx.xx.xx.X turi būti 0"]);

                    // ieškoma mažos stoties, kurios ribose būtų ccc nurodytas kilometras
                    {
                        int cccPradzia = (ccc - 1) * 1000;   // ccc kilometro pradžia
                        int cccPabaiga = cccPradzia + 1000;    // ccc kilometro pabaiga
                        stt = lin.Find(delegate(Stotis stotis)
                        {
                            return (!((stotis.iki == -1 ? true : stotis.iki <= cccPradzia) || stotis.nuo >= cccPabaiga));
                        });
                    }
                    if (stt == null) throw new InvalidKodasException(ExceptMsgs["xx.xXXX.xx.xx.x netinka mažai stočiai"]);
                    vieta.stotis = stt;
                    if (dd < 1 || dd > 8) throw new InvalidKodasException(ExceptMsgs["xx.xxxx.XX.xx.x gali būti nuo 1 iki 8"]);
                    if (ee < 1 || ee > MAX_SANDURU_IESME) throw new InvalidKodasException(string.Format(ExceptMsgs["xx.xxxx.xx.XX.x gali būti nuo 1 iki MAX_SANDŪRŲ"], MAX_SANDURU_IESME));
                    // suvirinimas mažos stoties iešme - baigta
                    vieta.stotis = stt;
                    vieta.koordinatė = new KelioKoordinatė(ccc);
                    vieta.kelioElementas = new KelioElementas(KelioElementoTipas.iešmas, dd);
                    vieta.iešmoSandūra = new Sandūra(ee);
                    return vieta;
                }

                else // jeigu ne iešmas
                {
                    if (dd < 1 || dd > 10) throw new InvalidKodasException(ExceptMsgs["xx.xxxx.XX.xx.x gali būti nuo 1 iki 10"]);
                    if ((b != 6 && b != 7) && (f != 0 && f != 9)) throw new InvalidKodasException(ExceptMsgs["Mažoje stotyje netrumpajame kelyje xx.xxxx.xx.xx.X gali būti 0 arba 9"]);
                    if ((b == 6 || b == 7) && (f == 0 || f == 9)) throw new InvalidKodasException(ExceptMsgs["Mažoje stotyje trumpajame kelyje xx.xxxx.xx.xx.X gali būti 1-8"]);
                    vieta.koordinatė = new KelioKoordinatė(ccc, dd, ee);

                    // ar ccc.dd.ee tinka mažai stočiai
                    dist = (ccc - 1) * 1000 + (dd - 1) * 100 + ee;
                    stt = lin.Find(delegate(Stotis stotis) { return ((stotis.nuo == -1 ? false : dist >= stotis.nuo) && dist <= stotis.iki); });
                    if (stt != null) // ccc.ee.dd mažai stočiai tinka
                    {
                        vieta.stotis = stt;
                        switch (b)
                        {
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                vieta.kelioElementas = new KelioElementas(KelioElementoTipas.stoties_kelias, b);
                                vieta.siūlė = new Siūlė(f);
                                break;
                            case 6:
                                vieta.kelioElementas = new KelioElementas(KelioElementoTipas.trumpasis_kelias_nuo_iešmo_kairėn, f);
                                break;
                            case 7:
                                vieta.kelioElementas = new KelioElementas(KelioElementoTipas.trumpasis_kelias_nuo_iešmo_dešinėn, f);
                                break;
                        }
                        return vieta;
                    }
                    else // ccc.dd.ee mažai stočiai netinka
                    {
                        if (b != 1 && b != 2) throw new InvalidKodasException(ExceptMsgs["xx.Xxxx.xx.xx.x gali būti 1 arba 2"]);
                        // rasti tarpstotį
                        {
                            Stotis stotis1 = lin.FindLast(delegate(Stotis stotis) { return (stotis.iki == -1 ? false : dist > stotis.iki); });
                            Stotis stotis2 = lin.Find(delegate(Stotis stotis) { return (dist < stotis.nuo); });
                            if (stotis1 == null || stotis2 == null) throw new InvalidKodasException(ExceptMsgs["xx.xXXX.XX.XX.x netinka linijai"]);
                            vieta.tarpstotis = new Tarpstotis(stotis1, stotis2);
                        }
                        vieta.siūlė = new Siūlė(f);
                        switch (b)
                        {
                            case 1:
                                vieta.kelioElementas = new KelioElementas(KelioElementoTipas.tarpstočio_nelyginis_kelias, b);
                                break;
                            case 2:
                                vieta.kelioElementas = new KelioElementas(KelioElementoTipas.tarpstočio_lyginis_kelias, b);
                                break;
                        }
                        return vieta;
                    }
                }
            }
            else // jeigu tokios linijos nėra
            {
                List<Stotis> galimosStotys = new List<Stotis>();
                // ieškoma stoties aa;
                foreach (List<Stotis> linijosStotys in LGIF.infrastruktūra.Values)
                {
                    stt = linijosStotys.Find(delegate(Stotis stotis) { return stotis.kodas == aa; });
                    if (stt != null) galimosStotys.Add(stt);
                }
                if (galimosStotys.Count == 0) throw new InvalidKodasException(ExceptMsgs["Tokios stoties nėra"]);
                if (b == 9) throw new InvalidKodasException(ExceptMsgs["DS b negali būti 9"]);
                if (b == 8)
                {
                    if (dd != 0 || f != 0) throw new InvalidKodasException(ExceptMsgs["Kai suvirinimas DSI, xx.xxxx.XX.xx.X turi būti 0."]);
                    if (ee < 1 || ee > MAX_SANDURU_IESME) throw new InvalidKodasException(string.Format(ExceptMsgs["DSI xx.xxxx.xx.XX.x gali būti nuo 1 iki {0}."], MAX_SANDURU_IESME));
                    
                    foreach (Stotis st in galimosStotys) vieta.galimosLinijos.Add(st.linija);
                    vieta.stotis = stt; // įdedama paskutinė rastoji
                    vieta.kelioElementas = new KelioElementas(KelioElementoTipas.iešmas, ccc);
                    vieta.iešmoSandūra = new Sandūra(ee);
                    return vieta;
                }
                if (f != 0 && f != 9) throw new InvalidKodasException(ExceptMsgs["xx.xxxx.xx.xx.X gali būti 0 arba 9"]);
                if (b == 1)
                {
                    if (dd == 0) throw new InvalidKodasException(ExceptMsgs["xx.xxxx.XX.xx.x gali būti nuo 1 iki 10"]);
                    // tikrinti, ar koordinate ccc.dd.ee tinka didelei stočiai aa
                    dist = (ccc - 1) * 1000 + (dd - 1) * 100 + ee;
                    stt = null;
                    stt = galimosStotys.Find(delegate(Stotis stotis) { return (stotis.nuo == -1 ? false : stotis.nuo <= dist) && (stotis.iki >= dist); });
                    if (stt == null) throw new InvalidKodasException(ExceptMsgs["xx.xXXX.XX.XX.x netinka stočiai"]);
                    vieta.galimosLinijos.Add(stt.linija);
                    vieta.stotis = stt;
                    vieta.kelioElementas = new KelioElementas(KelioElementoTipas.stoties_pagrindinis_kelias);
                    vieta.koordinatė = new KelioKoordinatė(ccc,dd,ee);
                    vieta.siūlė = new Siūlė(f);
                    return vieta;
                }
                foreach (Stotis st in galimosStotys) vieta.galimosLinijos.Add(st.linija);
                vieta.stotis = stt; // įdedama paskutinė rastoji
                vieta.atstumasNuoIešmo = new Atstumas(dd * 100 + ee);
                vieta.siūlė = new Siūlė(f);
                switch(b)
                {
                    case 2:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.stoties_pagrindinis_kelias, ccc);
                        break;
                    case 3:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.atvykimo_išvykimo_kelias, ccc);
                        break;
                    case 4:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.stoties_kelias, ccc);
                        break;
                    case 5:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.sankirta, ccc);
                        break;
                    case 6:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.trumpasis_kelias_nuo_iešmo_kairėn, ccc);
                        break;
                    case 7:
                        vieta.kelioElementas = new KelioElementas(KelioElementoTipas.trumpasis_kelias_nuo_iešmo_dešinėn, ccc);
                        break;
                }
                return vieta;
            }
        }

        public static Dictionary<string, string> IšimtiesPranešimai { get; set; }
    }

    #region custom exceptions
    class InvalidKodasException : Exception
    {
        public InvalidKodasException()
        {
        }
        public InvalidKodasException(string message)
            : base(message)
        {
        }
    }

    class InvalidDuomenysException : Exception
    {
        public InvalidDuomenysException()
        {
        }
        public InvalidDuomenysException(string message)
            : base(message)
        {
        }
    }

    class InvalidNumerisException : Exception
    {
        public InvalidNumerisException()
        {
        }
        public InvalidNumerisException(string message)
            : base(message)
        {
        }
    }

    class ReadFileFailureException : Exception
    {
        public ReadFileFailureException()
        {
        }
        public ReadFileFailureException(string message)
            : base(message)
        {
        }
    }
#endregion

}
