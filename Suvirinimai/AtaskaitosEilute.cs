using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Nbb;
using ewal.Msg;

namespace SuvirinimaiApp
{
    class AtaskaitosEilute
    {
        internal DateTime data;
        internal int suvirinimuSkaicius;
        internal string vieta;
        internal string aktuNumeriai;
        internal string suvirintojai;
        internal string operatoriai;
        internal string kelioMeistrai;
        internal string begiuTipai;
        internal string begiuTemperaturos;
        internal string protarpiai;

        public AtaskaitosEilute(List<Suvirinimas> panasusSuvirinimai)
        {
            this.data = panasusSuvirinimai[0].data;
            this.suvirinimuSkaicius = panasusSuvirinimai.Count;
            this.vieta = composeVieta(panasusSuvirinimai);
            this.aktuNumeriai = composeAktuNumeriai(panasusSuvirinimai);
            this.suvirintojai = composeSuvirintojai(panasusSuvirinimai);
            this.operatoriai = composeOperatoriai(panasusSuvirinimai);
            this.kelioMeistrai = composeKelioMeistrai(panasusSuvirinimai);
            this.begiuTipai = composeBegiuTipai(panasusSuvirinimai);
            this.begiuTemperaturos = composeBegiuTemperaturos(panasusSuvirinimai);
            this.protarpiai = composeProtarpiai(panasusSuvirinimai);
        }

        private string composeKelioMeistrai(List<Suvirinimas> suvirinimai)
        {
            Regex rgxFind = new Regex(@"\w+? ");
            string delim = ", ";
            List<string> rastiUnikalūs = new List<string>();
            StringBuilder sb = new StringBuilder();
            bool toksJauYra;
            string tmpString;
            foreach (Suvirinimas suv in suvirinimai)
            {
                tmpString = string.Format("{0} ({1})", suv.kelioMeistras.vardas, suv.kelioMeistras.meistrija);
                toksJauYra = false;
                foreach (string rastasUnikalus in rastiUnikalūs)
                {
                    if (rastasUnikalus == tmpString)
                    {
                        toksJauYra = true;
                        break;
                    }
                }
                if (!toksJauYra) rastiUnikalūs.Add(tmpString);
            }
            string tikVardas;
            foreach (string meistras in rastiUnikalūs)
            {
                // Čia reikalinga, kad vardų rašytų tik pirmąsias raides
                tikVardas = rgxFind.Matches(meistras)[0].ToString();
                sb.Append(meistras.Replace(tikVardas, tikVardas[0] + "."));
                sb.Append(delim);
            }
            sb.Remove(sb.Length - delim.Length, delim.Length);
            return sb.ToString();
        }

        private string composeSuvirintojai(List<Suvirinimas> suvirinimai)
        {
            Type tSuvirinimas = typeof(Suvirinimas);
            Type tSuvirintojas = typeof(Suvirintojas);

            string suvirintojuVardai = composeComplexPropertyVardas(tSuvirinimas.GetField("suvirintojas"), tSuvirintojas.GetField("vardas"), suvirinimai);
            string suvirintojuImones = composeComplexProperty(tSuvirinimas.GetField("suvirintojas"), tSuvirintojas.GetField("imone"), suvirinimai);
            return string.Format("{0} ({1})", suvirintojuVardai, suvirintojuImones);
        }

        private string composeOperatoriai(List<Suvirinimas> suvirinimai)
        {
            Type tSuvirinimas = typeof(Suvirinimas);
            Type tOperatorius = typeof(Operatorius);

            return composeComplexPropertyVardas(tSuvirinimas.GetField("operatorius"), tOperatorius.GetField("vardas"), suvirinimai);
        }

        private string composeAktuNumeriai(List<Suvirinimas> suvirinimai)
        {
            Type suvirinimasType = typeof(Suvirinimas);
            return composeSimpleProperty(suvirinimasType.GetField("aktoNr"), suvirinimai);
        }

        private string composeBegiuTipai(List<Suvirinimas> suvirinimai)
        {
            Type suvirinimasType = typeof(Suvirinimas);
            return composeSimplePropertyDistinct(suvirinimasType.GetField("begioTipas"), suvirinimai);
        }

        private string composeBegiuTemperaturos(List<Suvirinimas> suvirinimai)
        {
            Type suvirinimasType = typeof(Suvirinimas);
            return composeSimplePropertyDistinct(suvirinimasType.GetField("begioTemperatura"), suvirinimai);
        }

        private string composeProtarpiai(List<Suvirinimas> suvirinimai)
        {
            Type suvirinimasType = typeof(Suvirinimas);
            return composeSimplePropertyDistinct(suvirinimasType.GetField("protarpis"), suvirinimai);
        }

        private string kElementoPavad(KelioElementas kElementas)
        {
            string pavadinimas;
            if (kElementas.tipas == KelioElementoTipas.stoties_kelias ||
                kElementas.tipas == KelioElementoTipas.atvykimo_išvykimo_kelias ||
                kElementas.tipas == KelioElementoTipas.tarpstočio_lyginis_kelias ||
                kElementas.tipas == KelioElementoTipas.tarpstočio_nelyginis_kelias)
                pavadinimas = "kelias";
            else if (kElementas.tipas == KelioElementoTipas.trumpasis_kelias_nuo_iešmo_dešinėn ||
                kElementas.tipas == KelioElementoTipas.trumpasis_kelias_nuo_iešmo_kairėn)
                pavadinimas = "trump. kel. nuo iešmo";
            else pavadinimas = kElementas.tipas.ToString("G");

            return pavadinimas.Replace("_", " ");
        }

        struct SuvirinimuDuom {
            public long id;
            public string vietosKodas;
            public string klaida;
            public SuvirinimuDuom(long aidy, string vk, string kl)
            {
                id = aidy;
                vietosKodas = vk;
                klaida = kl;
            }
        }

        private string composeVieta(List<Suvirinimas> suvirinimai)
        {
            // renkami duomenys tam atvejui, jeigu tarp suvirinimo kodų pasitaikytų klaidingų
            List<SuvirinimuDuom> suvDuomenys = new List<SuvirinimuDuom>();
            bool yraKlaidinguKodu = false;
            string klaida;
            foreach (Suvirinimas suvirinimas in suvirinimai)
            {
                if (!string.IsNullOrEmpty(suvirinimas.errMsg)) 
                {
                    klaida = suvirinimas.errMsg;
                    yraKlaidinguKodu = true;
                }
                else 
                {
                    klaida = string.Empty;
                }
                suvDuomenys.Add(
                    new SuvirinimuDuom(suvirinimas.id, suvirinimas.strVietosKodas, klaida)
                    );
            }

            StringBuilder sb = new StringBuilder();

            // jei buvo klaidingų duomenų
            if (yraKlaidinguKodu)
            {
                sb.Append("Yra klaidingų kodų.");
                foreach (SuvirinimuDuom sd in suvDuomenys)
                {
                    sb.Append(" id");
                    sb.Append(sd.id);
                    sb.Append(" - ");
                    sb.Append(sd.vietosKodas);
                    if (sd.klaida != string.Empty)
                    {
                        sb.Append(" - ");
                        sb.Append(sd.klaida);
                    }
                    sb.Append(";");
                }
                return sb.ToString();
            }
            
            //else, i.e. klaidingų kodų nerasta
            Vieta vt = suvirinimai[0].vieta;
            if (vt.tarpstotis != null)
            {
                sb.Append(vt.tarpstotis.pavadinimas);
                sb.Append(", ");
                sb.Append(kElementoPavad(vt.kelioElementas));
                sb.Append(" Nr. ");
                sb.Append(vt.kelioElementas.numeris);
                sb.Append(", km ");
                sb.Append(vt.koordinate.km);
            }
            else
            {
                sb.Append("st. ");
                sb.Append(vt.galimosStotys[0].pavadinimas);
                sb.Append(", ");
                sb.Append(kElementoPavad(vt.kelioElementas));
                sb.Append(" Nr. ");
                sb.Append(vt.kelioElementas.numeris);
                /*
                if (vt.kelioElementas.tipas != KelioElementoTipas.stoties_pagrindinis_kelias)
                {
                    sb.Append(" Nr. ");
                    sb.Append(vt.kelioElementas.numeris);
                }
                */
                if (vt.kelioElementas.tipas == KelioElementoTipas.iešmas)
                {
                    sb.Append(" (");
                    string delim = ", ";
                    foreach (Suvirinimas suv in suvirinimai)
                    {
                        sb.Append(suv.vieta.iesmoSandura.Nr);
                        sb.Append(delim);
                    }
                    sb.Remove(sb.Length - delim.Length, delim.Length);
                    sb.Append(")");
                }
            }

            return sb.ToString();
        }

        private string composeComplexProperty(FieldInfo parentField, FieldInfo childField, List<Suvirinimas> suvirinimai)
        {
            string delim = ", ";
            ArrayList tmpArray = new ArrayList();
            StringBuilder sb = new StringBuilder();
            bool jauYra;
            object tmpParentObject;
            string tmpChildString;
            foreach (Suvirinimas suv in suvirinimai)
            {
                tmpParentObject = parentField.GetValue(suv);
                tmpChildString = (string)childField.GetValue(tmpParentObject);
                jauYra = false;
                foreach (string sąvybė in tmpArray)
                {
                    if (sąvybė == tmpChildString)
                    {
                        jauYra = true;
                        break;
                    }
                }
                if (!jauYra) tmpArray.Add(tmpChildString);
            }
            foreach (string sąvybė in tmpArray)
            {
                sb.Append(sąvybė);
                sb.Append(delim);
            }
            sb.Remove(sb.Length - delim.Length, delim.Length);
            return sb.ToString();
        }

        private string composeComplexPropertyVardas(FieldInfo parentField, FieldInfo childField, List<Suvirinimas> suvirinimai)
        {
            Regex rgxFind = new Regex(".* ");
            string delim = ", ";
            ArrayList tmpArray = new ArrayList();
            StringBuilder sb = new StringBuilder();
            bool jauYra;
            object tmpParentObject;
            string tmpChildString;
            foreach (Suvirinimas suv in suvirinimai)
            {
                tmpParentObject = parentField.GetValue(suv);
                tmpChildString = (string)childField.GetValue(tmpParentObject);
                jauYra = false;
                foreach (string sąvybė in tmpArray)
                {
                    if (sąvybė == tmpChildString)
                    {
                        jauYra = true;
                        break;
                    }
                }
                if (!jauYra) tmpArray.Add(tmpChildString);
            }
            foreach (string sąvybė in tmpArray)
            {
                // Čia reikalinga, kad vardų rašytų tik pirmąsias raides
                sb.Append(rgxFind.Replace(sąvybė, new MatchEvaluator(FirstLetter)));
                sb.Append(delim);
            }
            sb.Remove(sb.Length - delim.Length, delim.Length);
            return sb.ToString();
        }

        private string FirstLetter(Match m)
        {
            return m.ToString()[0] + ".";
        }

        private string composeSimplePropertyDistinct(FieldInfo fieldToCompose, List<Suvirinimas> suvirinimai)
        {
            string delim = ", ";
            List<string> listOfDistincts = new List<string>();

            bool jauYra;
            foreach (Suvirinimas suv in suvirinimai)
            {
                jauYra = false;
                foreach (string savybe in listOfDistincts)
                {
                    if (savybe.ToString() == (fieldToCompose.GetValue(suv)).ToString())
                    {
                        jauYra = true;
                        break;
                    }
                }
                if (!jauYra) listOfDistincts.Add(fieldToCompose.GetValue(suv).ToString());
            }
            listOfDistincts.Sort();
            return string.Join(delim, listOfDistincts.ToArray());
        }

        private string composeSimplePropertyDistinctVardas(FieldInfo fieldToCompose, List<Suvirinimas> suvirinimai)
        {
            Regex rgxFind = new Regex(".* ");
            string delim = ", ";
            List<string> listOfDistincts = new List<string>();

            bool jauYra;
            foreach (Suvirinimas suv in suvirinimai)
            {
                jauYra = false;
                foreach (string savybe in listOfDistincts)
                {
                    if (savybe.ToString() == (fieldToCompose.GetValue(suv)).ToString())
                    {
                        jauYra = true;
                        break;
                    }
                }
                if (!jauYra) listOfDistincts.Add(fieldToCompose.GetValue(suv).ToString());
            }
            listOfDistincts.Sort();
            StringBuilder sb = new StringBuilder();
            foreach (string sąvybė in listOfDistincts)
            {
                // Čia reikalinga, kad vardų rašytų tik pirmąsias raides
                sb.Append(rgxFind.Replace(sąvybė, new MatchEvaluator(FirstLetter)));
                sb.Append(delim);
            }
            return string.Join(delim, listOfDistincts.ToArray());
        }

        private string composeSimpleProperty(FieldInfo fieldToCompose, List<Suvirinimas> suvirinimai)
        {
            ArrayList tmpArray = new ArrayList();
            foreach (Suvirinimas suv in suvirinimai)
            {
                tmpArray.Add(fieldToCompose.GetValue(suv).ToString());
            }
            return rastiSekas(tmpArray);
        }

        private string rastiSekas(ArrayList stringai)
        {
            // stringų masyve randa skaičius, einančius iš eilės
            // ir tas sekas paverčia forma xx-yy

            string delim = ", ";
            List<KeyValuePair<int, string>> skaiciai = new List<KeyValuePair<int, string>>();
            List<string> neskaiciai = new List<string>();
            StringBuilder sorted = new StringBuilder();

            // atskiriami skaičiai nuo neskaičių
            foreach (string str in stringai)
            {
                try
                {
                    skaiciai.Add(new KeyValuePair<int, string>(Convert.ToInt32(str), str));
                }
                catch
                {
                    neskaiciai.Add(str);
                }
            }

            if (skaiciai.Count > 0)
            {
                // skaičiai išrikiuojami iš eilės
                skaiciai.Sort(
                    delegate(KeyValuePair<int, string> kvp1, KeyValuePair<int, string> kvp2)
                    {
                        return kvp1.Key.CompareTo(kvp2.Key);
                    }
                );
                List<string> sekos = new List<string>();

                // ieškoma sekų
                KeyValuePair<int, string> kvpstart = skaiciai[0];
                KeyValuePair<int, string> kvpend = skaiciai[0];
                for (int i = 1; i < skaiciai.Count; i++)
                {
                    if (skaiciai[i].Key == kvpend.Key + 1)
                    {
                        kvpend = skaiciai[i];
                    }
                    else
                    {
                        if (kvpstart.Key == kvpend.Key)
                            sekos.Add(kvpstart.Value);
                        else if (kvpend.Key == kvpstart.Key + 1)
                            sekos.Add(kvpstart.Value + delim + kvpend.Value);
                        else
                            sekos.Add(kvpstart.Value + "-" + kvpend.Value);

                        kvpstart = kvpend = skaiciai[i];
                    }
                }
                if (kvpstart.Key == kvpend.Key)
                    sekos.Add(kvpstart.Value);
                else if (kvpend.Key == kvpstart.Key + 1)
                    sekos.Add(kvpstart.Value + delim + kvpend.Value);
                else
                    sekos.Add(kvpstart.Value + "-" + kvpend.Value);

                foreach (string seka in sekos)
                {
                    sorted.Append(seka);
                    sorted.Append(delim);
                }
            }

            // gale pridedami išrikiuoti neskaičiai
            if (neskaiciai.Count > 0)
            {
                neskaiciai.Sort();
                foreach (string nesk in neskaiciai)
                {
                    sorted.Append(nesk);
                    sorted.Append(delim);
                }
            }
            sorted.Remove(sorted.Length - delim.Length, delim.Length);
            return sorted.ToString();
        }
    }
}
