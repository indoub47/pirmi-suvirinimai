using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Windows.Forms;
using ewal.Data;
using ewal.Msg;
using Nbb;
using SuvirinimaiApp.Properties;
using Word = Microsoft.Office.Interop.Word;

namespace SuvirinimaiApp
{
    partial class fAktai
    {
        private void tsmiGamintiAktusDoc_Click(object sender, EventArgs e)
        {
            // patikrinti, ar parinkta eilučių
            // patikrinti, ar yra šablonas
            // parinkti outputfolder
            // iš parinktų eilučių surinkti id
            // pagal turimus id užpildyti datatable
            // pasiruošti gaminti aktus:
            // užkrauti infrastruktūros duomenis LGIF (VietosKodasClasses.cs)
            // sukurti Logger objektą ir log failą output aplanke (Logger.cs)
            // paleisti formą su progressbaru
            // iš kiekvienos datatable eilutės
            // sukonstruoti failo vardą (reikėtų nukelti į vėliau, kai bus žinoma vieta
            // iš DataRow gaminti Dictionary<string, string>
            // iš Dictionary<string, string> gaminti .doc failą
            // įrašyti į duombazę, kad failas sukurtas // nereikia
            // įraštyti į log failą apie sukūrimą                
            // jeigu nesėkmingai
            // log failą įrašyti nesėkmę ir jos priežastį
            // bet kokiu atveju
            // padidinti progressbaro užpildymą 
            // išmesti pranešimą, kiek užsakyta ir kiek pagaminta

            // patikrinti, ar parinkta eilučių
            int selected = dgvAktai.SelectedRows.Count;
            if (selected == 0)
            {
                Msg.InformationMsg("Aktai gaminami iš parinktų eilučių.");
                return;
            }

            // patikrinti, ar yra šablono failas
            object docTemplateFileName;
            try
            {
                docTemplateFileName = obtainAktasdotFileName();
            }
            catch
            {
                Msg.ErrorMsg(Settings.Default.DotTemplateFileNotChosenError);
                return;
            }

            // parinkti outputfolder
            string outputFolder = getOutputDirectoryName();
            if (outputFolder == "***") return;

            // iš parinktų eilučių surinkti id ir pagaminti WHERE sąlygą SQL užklausai
            List<string> selectedIds = new List<string>();
            foreach (DataGridViewRow rw in dgvAktai.SelectedRows) selectedIds.Add(rw.Cells["id"].Value.ToString());
            string whereCondition = string.Format("Aktai.id IN ({0})", string.Join(", ", selectedIds));

            // pagal turimus id užpildyti datatable
            string sqlSt = "SELECT ";
            sqlSt += "Aktai.id, Aktai.aktas_Nr, Aktai.aktas_data, ";
            sqlSt += "Aktai.k11, Aktai.k12, Aktai.k21, Aktai.k22, Aktai.k23, Aktai.k24, Aktai.k31, Aktai.k32, Aktai.k41, Aktai.k42, Aktai.k51, ";
            sqlSt += "Aktai.begis_protarpisMm, Aktai.medz_formaGamMetai, Aktai.medz_misinGamMetai, Aktai.medz_misinPartNr, Aktai.medz_misinPorcNr, ";
            sqlSt += "Aktai.salyg_arSausa, Aktai.salyg_oroTemp, Aktai.salyg_begioTemp, ";
            sqlSt += "Aktai.tikrin_nelygumaiVirsausMm, Aktai.tikrin_nelygumaiSonoMm, Aktai.tikrin_arDefektas, Aktai.tikrin_defKodas, Aktai.tikrin_sanduruCharakter, ";
            sqlSt += "Padaliniai.oficPavadinimas AS padalinys, Padaliniai.arRangovas, PadaliniaiSuvirintoju.pavadinimas AS virino_padalinys, ";
            sqlSt += "PadaliniaiKelininku.pavadinimas AS sutvarke_padalinys, Suvirintojai.vardas AS suvirintojas, Suvirintojai.pazymNr AS suvirintojas_kodas, ";
            sqlSt += "Operatoriai.id AS operatorius_kodas, Operatoriai.vardas AS operatorius, ";
            sqlSt += "Vadovai.vardas AS sutvarke_vadovas, Vadovai.pareigos AS sutvarke_vadovasPareigos, ";
            sqlSt += "Defektoskopai.id AS df_kodas, Defektoskopai.gamyklNr AS df_nr, Defektoskopai.tipas AS df_tipas, ";
            sqlSt += "BegiuTipai.pavadinimas AS begio_tipas, Misiniai.pavadinimas AS misinys, ";
            sqlSt += "KMFilialo.vardas AS KM_vardas ";
            sqlSt += "FROM ";
            sqlSt += "(((((((((Aktai LEFT JOIN Padaliniai ON Aktai.aktas_padalinysId=Padaliniai.id) ";
            sqlSt += "LEFT JOIN PadaliniaiSuvirintoju ON Aktai.suvirint_padalinysId=PadaliniaiSuvirintoju.id) ";
            sqlSt += "LEFT JOIN PadaliniaiKelininku ON Aktai.sutvark_padalinysId=PadaliniaiKelininku.id) ";
            sqlSt += "LEFT JOIN Suvirintojai ON Aktai.suvirint_suvirintojasId=Suvirintojai.id) ";
            sqlSt += "LEFT JOIN Operatoriai ON Aktai.tikrin_operatoriusId=Operatoriai.id) ";
            sqlSt += "LEFT JOIN Defektoskopai ON Aktai.tikrin_defektoskopasId=Defektoskopai.id) ";
            sqlSt += "LEFT JOIN Vadovai ON Aktai.sutvark_vadovasId=Vadovai.id) ";
            sqlSt += "LEFT JOIN BegiuTipai ON Aktai.begis_tipasId=BegiuTipai.id) ";
            sqlSt += "LEFT JOIN Misiniai ON Aktai.medz_misinKodasId=Misiniai.id) ";
            sqlSt += "LEFT JOIN KMFilialo ON Aktai.aktas_pasiraseKMId=KMFilialo.id ";
            sqlSt += "WHERE " + whereCondition;
            sqlSt += "ORDER BY Aktai.id;";
            DataTable tableAktai;
            try
            {
                tableAktai = DbHelper.FillDataTable(sqlSt);
            }
            catch (DbException)
            {
                Msg.ErrorMsg(Messages.DbErrorMsg);
                return;
            }

            // pasiruošti gaminti aktus:
            Dictionary<string, string> aktoDuomenys;

            //užkrauti infrastruktūros duomenis LGIF
            LGIF.loadData(obtainLgifxmlFileName());

            //sukurti Logger objektą ir log failą output aplanke
            Logger logger = new Logger(Path.Combine(outputFolder, "log.txt"));
            logger.Open();
            // parodyti progressbar
            lblStatus.Visible = lblStatusFilter.Visible = false;
            int created = 0;
            progressbar.Visible = true;
            progressbar.Value = created;
            progressbar.Maximum = selected;
            string fileExtension = "doc";

            // paleidžiama word programa
            Word._Application wordApp = new Word.Application();
            wordApp.Visible = false;
            string outputFileName = "phony";
            foreach (DataRow dr in tableAktai.Rows)
            {

                try
                {
                    // sukonstruoti failo vardą (reikėtų nukelti į vėliau, kai bus žinoma vieta
                    outputFileName = Path.Combine(outputFolder, string.Format("{0} {1} {2} {3}.{4}",
                                                                    Settings.Default.AktasOutputFileNamePrefix,
                                                                    dr["id"],
                                                                    dr["aktas_Nr"],
                                                                    Convert.ToDateTime(dr["aktas_data"]).ToString("yyyyMMdd"),
                                                                    fileExtension));

                    // iš DataRow gaminti XmlDocument
                    aktoDuomenys = generateDicByDataRow(dr);
                    // iš XmlDocument gaminti .doc failą
                    createAndSaveAktasDoc(wordApp, aktoDuomenys, outputFileName, docTemplateFileName);
                    // įrašyti į duombazę, kad failas sukurtas
                    //DbHelper.ExecuteNonQuery(string.Format("UPDATE Aktai SET aktas_arAtspausdintas=True WHERE id={0};", dr["id"])); 
                    // įraštyti į log failą apie sukūrimą
                    logger.Log(string.Format("sukurtas failas {0}", Path.GetFileName(outputFileName)));
                }
                catch (Exception exc)
                {
                    // log failą įrašyti nesėkmę ir jos priežastį
                    logger.Log(string.Format("nepavyko sukurti failo {0}, priežastis: {1}", Path.GetFileName(outputFileName), exc.Message));
                    continue;
                }
                finally
                {
                    // padidinti progressbaro užpildymą
                    created++;
                    progressbar.Value = created;
                }
            }
            object dontSaveChanges = Word.WdSaveOptions.wdDoNotSaveChanges;
            object missing = System.Reflection.Missing.Value;
            wordApp.Quit(ref dontSaveChanges, ref missing, ref missing);
            wordApp = null;
            logger.Close();
            progressbar.Visible = false;
            progressbar.Value = 0;
            lblStatus.Visible = lblStatusFilter.Visible = true;
            Msg.InformationMsg("Aktai sukurti, išsaugoti.");

        }

        private string getOutputDirectoryName()
        {
            // parenka aplanką išsaugoti pagamintiems aktų failams
            // jeigu nutraukiamas darbas, grąžinama "***" - signalas, kad aktų negaminti

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Pasirinkite, kur išsaugoti aktą (-us)";
            fbd.SelectedPath = CurrSet.Default.AktasOutputDirectoryName;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                return fbd.SelectedPath;
            }
            else
            {
                return "***";
            }
        }

        private Dictionary<string, string> generateDicByDataRow(DataRow drow)
        {
            Dictionary<string, string> bkdata = new Dictionary<string, string>();
            // paprasti
            bkdata.Add("akto_data", ((DateTime)drow["aktas_data"]).ToString("yyyy-MM-dd"));
            bkdata.Add("akto_nr", drow["aktas_nr"].ToString());
            bkdata.Add("begio_tipas", drow["begio_tipas"].ToString());
            bkdata.Add("formos_gm", drow["medz_formaGamMetai"].ToString());
            bkdata.Add("misinio_gm", drow["medz_misinGamMetai"].ToString());
            bkdata.Add("kas_virino", drow["padalinys"].ToString());
            bkdata.Add("misinio_kodas", drow["misinys"].ToString());
            bkdata.Add("partijos_nr", drow["medz_misinPartNr"].ToString());
            bkdata.Add("porcijos_nr", drow["medz_misinPorcNr"].ToString());
            bkdata.Add("filialo_KM", drow["KM_vardas"].ToString());
            bkdata.Add("temp_begio", drow["salyg_begioTemp"].ToString());
            bkdata.Add("temp_oro", drow["salyg_oroTemp"].ToString());
            bkdata.Add("protarpis", drow["begis_protarpisMm"].ToString());
            bkdata.Add("suvirintojo_kodas", drow["suvirintojas_kodas"].ToString());
            bkdata.Add("suvirintojo_padalinys", drow["virino_padalinys"].ToString());
            bkdata.Add("suvirintojo_vardas", drow["suvirintojas"].ToString());

            // sąlyginiai
            if ((bool)drow["arRangovas"])
                bkdata.Add("statusas", "rangovo įmonės pavadinimas");
            else
                bkdata.Add("statusas", "struktūrinio padalinio pavadinimas");

            if ((bool)drow["salyg_arSausa"])
                bkdata.Add("sausa_dregna", "sausa");
            else
                bkdata.Add("sausa_dregna", "drėgna");

            if (((bool)drow["tikrin_arDefektas"]))
            {
                bkdata.Add("ar_defektas", "defekto kodas");
                bkdata.Add("defekto_kodas", drow["tikrin_defKodas"].ToString());
                // sandūrą aprašantis tekstas kapojamas į tris dalis ir kiekviena priskiriama 
                // atskirai akto eilutei. Jeigu tekstas netelpa į tris dalis (apie 250 simbolių),
                // jis nukandamas.
                string[] eilutes = new string[3];
                splitString(drow["tikrin_sanduruCharakter"].ToString(), ref eilutes);
                bkdata.Add("sanduros_charakteristika_0", eilutes[0]);
                bkdata.Add("sanduros_charakteristika_1", eilutes[1]);
                bkdata.Add("sanduros_charakteristika_2", eilutes[2]);
            }
            else
            {
                bkdata.Add("ar_defektas", "defektų nenustatyta");
                bkdata.Add("defekto_kodas", "---");
                bkdata.Add("sanduros_charakteristika_0", "---");
            }

            // sudėtiniai
            bkdata.Add("nbb_defektoskopas", string.Format("{0}, {1} Nr. {2}",
                                                drow["df_kodas"].ToString(),
                                                drow["df_tipas"].ToString(),
                                                drow["df_nr"].ToString()));
            bkdata.Add("nbb_operatorius", string.Format("{0}, {1}",
                                                drow["operatorius_kodas"].ToString(),
                                                drow["operatorius"].ToString()));
            bkdata.Add("sutvarke", string.Format("{0}, {1} {2}",
                                                drow["sutvarke_padalinys"].ToString(),
                                                drow["sutvarke_vadovasPareigos"].ToString(),
                                                drow["sutvarke_vadovas"].ToString()));
            bkdata.Add("nelygumai", string.Format("{0} / {1}",
                                                getStringOfSignedDec(drow["tikrin_nelygumaiVirsausMm"]),
                                                getStringOfSignedDec(drow["tikrin_nelygumaiSonoMm"])));

            // prefix - nuo jo priklauso, į katrą lentelę rašys;
            // kelio kodo lentelėje visi bookmarkai prasideda iš "k",
            // o iešmo kodo lentelėje visi bookmarkai prasideda iš "i"
            string prfx;
            if ((int)drow["k21"] == 8 || (int)drow["k21"] == 9) prfx = "i";
            else prfx = "k";

            // vietos kodas
            bkdata.Add(prfx + "k11", drow["k11"].ToString());
            bkdata.Add(prfx + "k12", drow["k12"].ToString());
            bkdata.Add(prfx + "k21", drow["k21"].ToString());
            bkdata.Add(prfx + "k22", drow["k22"].ToString());
            bkdata.Add(prfx + "k23", drow["k23"].ToString());
            bkdata.Add(prfx + "k24", drow["k24"].ToString());
            bkdata.Add(prfx + "k31", drow["k31"].ToString());
            bkdata.Add(prfx + "k32", drow["k32"].ToString());
            bkdata.Add(prfx + "k41", drow["k41"].ToString());
            bkdata.Add(prfx + "k42", drow["k42"].ToString());
            bkdata.Add(prfx + "k51", drow["k51"].ToString());

            // parsinamas vietos kodas, kad suprasti, ką koks skaitmuo reiškia
            Vieta vt = VietosKodasParser.parseSuvirinimas(new VietosKodas(drow));

            // vietos kodo aprasymai lentelėse

            // XX.xxxx.xx.xx.x
            // jeigu stotis ir jeigu ta stotis didelė - rašomas stoties pavadinimas
            if (vt.galimosStotys.Count > 0 && vt.galimosStotys[0].kodas != "0")
                bkdata.Add(prfx + "k_aprasymas_1_12", "Stotis " + vt.galimosStotys[0].pavadinimas);
            // jeigu maža stotis rašomas linijos pavadinimas
            else if (vt.galimosStotys.Count > 0)
                bkdata.Add(prfx + "k_aprasymas_1_12", "Linija " + vt.galimosStotys[0].linija.pavadinimas);
            // jeigu tarpstotis
            else
                bkdata.Add(prfx + "k_aprasymas_1_12", "Linija " + vt.tarpstotis.stotis1.linija.pavadinimas);

            int b = Convert.ToInt32(drow["k21"]);
            int f = Convert.ToInt32(drow["k51"]);

            // xx.Xxxx.xx.xx.x
            // jeigu iešmas didelėje stotyje
            if (b == 8)
                bkdata.Add(prfx + "k_aprasymas_2_1", "Iešmo kodas didelėje stotyje");
            // jeigu iešmas mažoje stotyje
            else if (b == 9)
                bkdata.Add(prfx + "k_aprasymas_2_1", "Iešmo kodas mažoje stotyje");
            // jeigu didelė stotis arba jeigu trumpas kelias nesvarbu kokioje stotyje
            else if ((vt.galimosStotys.Count > 0 && vt.galimosStotys[0].kodas != "0") || b == 6 || b == 7)
                bkdata.Add(prfx + "k_aprasymas_2_1", "Stoties kelio kodas");
            // jeigu tarpstotis arba jeigu maža stotis ne iešmas ir ne trumpas kelias
            else
                bkdata.Add(prfx + "k_aprasymas_2_1", "Kelio numeris");

            // xx.xXXX.xx.xx.x
            // visais atvejais, kai nurodyta kelio koordinatė
            if (vt.koordinate != null)
                bkdata.Add(prfx + "k_aprasymas_2_234", "Kelio koordinatės kilometras");
            else
                // iešmas didelėje stotyje
                if (b == 8)
                    bkdata.Add(prfx + "k_aprasymas_2_234", "Iešmo Nr.");
                else
                    bkdata.Add(prfx + "k_aprasymas_2_234", "Stoties kelio Nr.");
            // xx.xxxx.XX.xx.x
            // didelės stoties iešme
            if (b == 8)
                bkdata.Add(prfx + "k_aprasymas_3_12", "00 didelėje stotyje");
            // mažos stoties iešme
            else if (b == 9)
                bkdata.Add(prfx + "k_aprasymas_3_12", "Iešmo Nr. mažoje stotyje");
            // ne iešmas ir nurodyta pilna koordinatė
            else if (vt.koordinate != null && vt.koordinate.yraPilna)
                bkdata.Add(prfx + "k_aprasymas_3_12", "Kelio koordinatės piketas");
            // kai nurodytas atstumas nuo iešmo
            else if (vt.atstumasNuoIesmo != null)
                bkdata.Add(prfx + "k_aprasymas_3_12", "Atstumas nuo iešmo, m");
            // čia būtų toks keistas atvejis, kai nurodyta kelio koordinatė, bet ji nepilna ir tai nėra mažos stoties iešmas
            else
                bkdata.Add(prfx + "k_aprasymas_3_12", ""); // neturėtų būti

            // xx.xxxx.xx.XX.x
            // kai iešmas
            if (b == 8 || b == 9)
                bkdata.Add(prfx + "k_aprasymas_4_12", "Iešmo sandūros Nr.");
            // kai nurodyta kelio koordinatė ir ji pilna
            else if (vt.koordinate != null && vt.koordinate.yraPilna)
                bkdata.Add(prfx + "k_aprasymas_4_12", "Atstumas nuo piketinio stulpelio, m");
            // kai nurodytas atstumas nuo iešmo
            else if (vt.atstumasNuoIesmo != null)
                bkdata.Add(prfx + "k_aprasymas_4_12", "Atstumas nuo iešmo, m");
            else
                bkdata.Add(prfx + "k_aprasymas_4_12", ""); // neturėtų būti

            // xx.xxxx.xx.xx.X
            // iešme
            if (b == 8 || b == 9)
                bkdata.Add(prfx + "k_aprasymas_5_1", "------------");
            // ne iešme
            else if (f == 0)
                bkdata.Add(prfx + "k_aprasymas_5_1", "Kairioji siūlė");
            // ne iešme
            else if (f == 9)
                bkdata.Add(prfx + "k_aprasymas_5_1", "Dešinioji siūlė");
            else
                bkdata.Add(prfx + "k_aprasymas_5_1", ""); // neturėtų būti

            return bkdata;
        }

        private string getStringOfSignedDec(object oSkaicius)
        {
            decimal skaicius;
            try
            {
                skaicius = Convert.ToDecimal(oSkaicius);
            }
            catch
            {
                return string.Empty;
            }

            if (skaicius > 0) return "+" + skaicius.ToString("0.0");
            if (skaicius < 0) return skaicius.ToString("0.0");
            return "0,0";
        }

        private void splitString(string text, ref string[] chunks)
        {
            int[] capacities = { 50, 100, 100 };
            text = text.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ').Replace("  ", " ");
            char[] delimiters = { ' ' };
            ArrayList alWords = new ArrayList(text.Split(delimiters));
            for (int i = 0; i < chunks.Length; i++)
            {
                while (alWords.Count != 0 && (chunks[i] + (string)alWords[0]).Length + 1 <= capacities[i])
                {
                    chunks[i] = chunks[i] + " " + (string)alWords[0];
                    alWords.RemoveAt(0);
                }
            }
        }

        private void createAndSaveAktasDoc(Word._Application wApp, Dictionary<string, string> dataDic, string outputFileName, object templateName)
        {
            // iš dic ima duomenis, atidaro doc šabloną, atlieka pakeitimus, išsaugo kaip outputFileName
            object missing = System.Reflection.Missing.Value;
            //Word._Application wordApp = new Word.Application();
            Word._Document aDoc = null;

            try
            {
                aDoc = wApp.Documents.Add(ref templateName, ref missing, ref missing, ref missing);
                performDocReplacements(aDoc, dataDic);
                object saveAs = outputFileName;
                aDoc.SaveAs(ref saveAs, ref missing, ref missing,
                    ref missing, ref missing, ref missing, ref missing, ref missing,
                    ref missing, ref missing, ref missing, ref missing,
                    ref missing, ref missing, ref missing, ref missing);
                aDoc.Close(ref missing, ref missing, ref missing);
            }
            catch (Exception suds)
            {
                MessageBox.Show("nesigavo atidaryti ir uždaryti: " + suds.Message);
            }
        }

        private void performDocReplacements(Word._Document documentas, Dictionary<string, string> data)
        {
            foreach (KeyValuePair<string, string> pair in data)
            {
                try
                {
                    documentas.Bookmarks[pair.Key].Range.Text = pair.Value;
                }
                catch (Exception suds)
                {
                    MessageBox.Show("nesigavo pakeisti \"" + pair.Key + "\", nes: " + suds.Message);
                }
            }
        }

        /// <summary>
        /// Grąžina akto .dot šablono failo pilną pavadinimą (path, name, extension).
        /// </summary>
        /// <remarks>
        /// Mėgina rasti <c>Settings.Default.InfraDataFileName</c> failą toje-pačioje-papkėje-kaip-programos-exe\etc\.
        /// Jeigu tokio nėra, tada prašo nurodyti.
        /// </remarks>
        /// <returns>MS Word šablono failo pilnas <see cref="System.String"/> pavadinimas (path, name, extension)</returns>
        /// <exception cref="Nbb.FileIOFailureException">Išmeta <see cref="Nbb.FileIOFailureException"/>, kai neranda failo toje pačioje
        /// papkėje, kaip ir programos exe, ir kai nenurodomas joks kitas failas.</exception>
        private string obtainAktasdotFileName()
        {
            string fullName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), Settings.Default.EtcDirName, Settings.Default.AktasDotTemplateFileName);
            if (!File.Exists(fullName))
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), Settings.Default.EtcDirName);
                ofd.Filter = "MS Word templates (*.dot)|*.dot|All files (*.*)|*.*";
                ofd.Title = Settings.Default.SelectDotTemplateFileDialogTitle;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    fullName = ofd.FileName;
                }
                else
                {
                    throw new Nbb.FileIOFailureException(Settings.Default.DotTemplateFileNotChosenError);
                }
            }
            return fullName;
        }
    }
}