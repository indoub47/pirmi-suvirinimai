using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ewal.Data;
using ewal.Msg;
using Nbb;
using SuvirinimaiApp.Properties;

namespace SuvirinimaiApp
{
    partial class fAktai
    {
        private string selectTemplateFile()
        {
            OpenFileDialog dSelectTemplate = new OpenFileDialog();
            dSelectTemplate.Title = "Pasirinkite xsl šablono failą";
            dSelectTemplate.InitialDirectory = Path.GetDirectoryName(xslTemplateFileName);
            dSelectTemplate.Filter = ".xsl|*.xsl";
            if (dSelectTemplate.ShowDialog() == DialogResult.OK)
            {
                return dSelectTemplate.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        private void tsmiMAtaskaita_Click(object sender, EventArgs e)
        {
            // pagamina ataskaitą pagal užduotas datas
            using (fDates formaDatos = new fDates())
            {
                if (formaDatos.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                else
                    makeOutput();
            }

        }

        private void makeOutput()
        {
            // parinkti failo vardą                    
            saveFileDialog.Title = "Išsaugoti failą";
            saveFileDialog.FileName = Settings.Default.DefaultOutputFileName;
            saveFileDialog.InitialDirectory = Settings.Default.OutputPath;
            saveFileDialog.Filter = string.Format("{0}|{1}", ".csv", "*.csv");
            saveFileDialog.FilterIndex = Settings.Default.FilterIndex;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.OutputPath = Path.GetDirectoryName(saveFileDialog.FileName);
                Settings.Default.FilterIndex = saveFileDialog.FilterIndex;
                Settings.Default.DefaultOutputFileName = saveFileDialog.FileName;
                Settings.Default.Save();
            }
            else
            {
                return;
            }
            exportCsv(saveFileDialog.FileName);
        }

        private void exportCsv(string outputFileName)
        {
            StringBuilder sb = new StringBuilder();

            string dlm = "\t";
            string endl = "\r\n";
            List<AtaskaitosEilute> ataskaita;
            try
            {
                ataskaita = darytiAtaskaita(Program.firstDate, Program.lastDate);
            }
            catch (DbException)
            {
                Msg.ErrorMsg(Messages.DbErrorMsg);
                return;
            }
            catch (Nbb.FileIOFailureException fiofx)
            {
                Msg.ErrorMsg(fiofx.Message);
                return;
            }
            catch (Nbb.InvalidDuomenysException idx)
            {
                Msg.ErrorMsg(idx.Message);
                return;
            }
            catch (Nbb.InvalidKodasException ikx)
            {
                Msg.ErrorMsg(ikx.Message);
                return;
            }
            catch (Exception e)
            {
                Msg.ErrorMsg("Kažkokia klaida: " + e.Message);
                return;
            }

            foreach (AtaskaitosEilute eil in ataskaita)
            {
                sb.Append(eil.aktuNumeriai); sb.Append(dlm);
                sb.Append(eil.data); sb.Append(dlm);
                sb.Append(eil.suvirinimuSkaicius); sb.Append(dlm);
                sb.Append(eil.vieta); sb.Append(dlm);
                sb.Append(eil.suvirintojai); sb.Append(dlm);
                sb.Append(eil.operatoriai); sb.Append(dlm);
                sb.Append(eil.kelioMeistrai); sb.Append(dlm);
                sb.Append(eil.begiuTipai); sb.Append(dlm);
                sb.Append(eil.begiuTemperaturos); sb.Append(dlm);
                sb.Append(eil.protarpiai); sb.Append(dlm);
                sb.Append(endl);
            }

            try
            {
                StreamWriter file = new System.IO.StreamWriter(outputFileName);
                file.WriteLine(sb.ToString());
                file.Close();
                Msg.InformationMsg("Išsaugota: " + outputFileName);
            }
            catch (Exception ex)
            {
                Msg.ErrorMsg(ex.Message);
            }
        }

        private XmlDocument createXml(DataTable dt)
        {
            XmlDocument xmlDoc = new XmlDocument();
            // create declaration
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", null, null));
            // Create the root element
            XmlElement root = xmlDoc.CreateElement("aktai");
            xmlDoc.AppendChild(root);
            XmlElement aktas, laukas;
            foreach (DataRow dr in dt.Rows)
            {
                aktas = xmlDoc.CreateElement("aktas");
                foreach (DataColumn dc in dt.Columns)
                {
                    laukas = xmlDoc.CreateElement(dc.ColumnName);
                    laukas.InnerText = dr[dc.ColumnName].ToString();
                    aktas.AppendChild(laukas);
                }
                root.AppendChild(aktas);
            }
            return xmlDoc;
        }

        private List<AtaskaitosEilute> darytiAtaskaita(DateTime bottomDate, DateTime topDate)
        {
            DataTable suvirinimaiTable;
            string where = string.Empty;

            if (bottomDate == null && topDate != null)
            {
                where = string.Format("WHERE Aktai.aktas_data <= {0} ", DbHelper.FormatDateValue(topDate));
            }
            else if (bottomDate != null && topDate == null)
            {
                where = string.Format("WHERE Aktai.aktas_data >= {0} ", DbHelper.FormatDateValue(bottomDate));
            }
            else if (bottomDate != null && topDate != null)
            {
                if (bottomDate <= topDate)
                    where = string.Format("WHERE Aktai.aktas_data >= {0} AND Aktai.aktas_data <= {1} ", DbHelper.FormatDateValue(bottomDate), DbHelper.FormatDateValue(topDate));
                else
                    where = string.Format("WHERE Aktai.aktas_data >= {0} OR Aktai.aktas_data <= {1} ", DbHelper.FormatDateValue(bottomDate), DbHelper.FormatDateValue(topDate));
            }

            List<List<Suvirinimas>> sarasai = new List<List<Suvirinimas>>();
            StringBuilder sb = new StringBuilder("SELECT ");
            // sb.Append("Aktai.*, Suvirintojai.*, Operatoriai.*, PadaliniaiSuvirintoju.*, PadaliniaiKelininku.*, Vadovai.*, BegiuTipai.* ");
            sb.Append("Aktai.aktas_data, Aktai.aktas_padalinysId, ");
            sb.Append("Aktai.k11, Aktai.k12, Aktai.k21, Aktai.k22, Aktai.k23, Aktai.k24, Aktai.k31, Aktai.k32, Aktai.k41, Aktai.k42, Aktai.k51, ");
            sb.Append("Aktai.id, Aktai.aktas_Nr, Aktai.salyg_begioTemp, Aktai.begis_protarpisMm, ");
            sb.Append("PadaliniaiSuvirintoju.pavadinimas AS suvirintojo_įmonė, Suvirintojai.vardas AS suvirintojo_vardas, Suvirintojai.pazymNr AS suvirintojo_kodas, ");
            sb.Append("Operatoriai.vardas AS operatoriaus_vardas, Operatoriai.id AS operatoriaus_kodas, ");
            sb.Append("KMFilialo.vardas AS kelio_meistro_vardas, KMFilialo.meistrija AS kelio_meistro_meistrija, ");
            sb.Append("BegiuTipai.pavadinimas AS bėgio_tipas ");
            sb.Append("FROM ");
            sb.Append("((((Aktai LEFT JOIN PadaliniaiSuvirintoju ON Aktai.suvirint_padalinysId=PadaliniaiSuvirintoju.id) ");
            sb.Append("LEFT JOIN Suvirintojai ON Aktai.suvirint_suvirintojasId=Suvirintojai.id) ");
            sb.Append("LEFT JOIN Operatoriai ON Aktai.tikrin_operatoriusId=Operatoriai.id) ");
            sb.Append("LEFT JOIN KMFilialo ON Aktai.aktas_pasiraseKMId=KMFilialo.id) ");
            sb.Append("LEFT JOIN BegiuTipai ON Aktai.begis_tipasId=BegiuTipai.id ");
            sb.Append("{0}");
            sb.Append("ORDER BY Aktai.aktas_data, Aktai.aktas_padalinysId, Aktai.k11, Aktai.k12, Aktai.k21, Aktai.k22, Aktai.k23, Aktai.k24, Aktai.k31, Aktai.k32, Aktai.k41, Aktai.k42, Aktai.k51;");
            string suvirinimaiSql = sb.ToString();
            suvirinimaiTable = new DataTable();
            try
            {
                suvirinimaiTable = DbHelper.FillDataTable(string.Format(suvirinimaiSql, where));
            }
            catch (DbException e)
            {
                Msg.ErrorMsg(Messages.DbErrorMsg);
                throw e;
            }
            List<AtaskaitosEilute> ataskaita = new List<AtaskaitosEilute>();
            if (suvirinimaiTable.Rows.Count == 0) return ataskaita;


            // Kartojasi veiksmas - kaskart, kai gaminama nauja ataskaita.
            // Bet taip pat tai leidžia pakeisti lgif.xml failą neuždarius programos.
            LGIF.loadData(obtainLgifxmlFileName());

            // Imama pirmoji iš DB partempta eilutė, ji tampa cmpRow
            // ir lyginama su toliau einančiomis.
            // Kiek laukų lyginti, nustatoma pagal k21 reikšmę:
            // jeigu k21 yra 9, lyginama 10 laukų, t.y. vietos kodas imamas iki k32 imtinai,
            // jeigu k21 nėra 9, lyginami 8 laukai, t.y. vietos kodas imamas iki k42 imtinai.
            // Panašios eilutės grupuojamos į grupeSuvirinimu.
            // Kada randama nebetinkama eilutė, grupeSuvirinimu įrašoma į sarasai, 
            // pradedama nauja grupeSuvirinimu, paskutinioji (netikusi) eilutė tampa ta, su kuria 
            // lyginamos visos toliau einančios, t.y. cmpRow,
            // ir ji lyginama su toliau einančiomis.
            // Resultatas - List sarasai, kuris sudarytas iš List<suvirinimaiTable.Row>
            DataRow cmpRow = suvirinimaiTable.Rows[0];
            int kiekLyginti = kiek(cmpRow);
            List<Suvirinimas> grupeSuvirinimu = new List<Suvirinimas>();
            grupeSuvirinimu.Add(new Suvirinimas(suvirinimaiTable.Rows[0]));

            // suvirinimai suskirstomi į grupes pagal [data + kas virino + vieta]
            for (int r = 1; r < suvirinimaiTable.Rows.Count; r++)
            {
                if (lygiosEilutės(cmpRow, suvirinimaiTable.Rows[r], kiekLyginti))
                {
                    grupeSuvirinimu.Add(new Suvirinimas(suvirinimaiTable.Rows[r]));
                }
                else
                {
                    sarasai.Add(grupeSuvirinimu);
                    grupeSuvirinimu = new List<Suvirinimas>();
                    grupeSuvirinimu.Add(new Suvirinimas(suvirinimaiTable.Rows[r]));
                    cmpRow = suvirinimaiTable.Rows[r];
                    kiekLyginti = kiek(cmpRow);
                }
            }
            if (grupeSuvirinimu.Count != 0) sarasai.Add(grupeSuvirinimu);

            // iš kiekvienos suvirinimų grupės padaroma viena ataskaitos eilute ir pridedama prie ataskaitos            
            foreach (List<Suvirinimas> suvirinimuGrupe in sarasai)
            {
                try
                {
                    ataskaita.Add(new AtaskaitosEilute(suvirinimuGrupe));
                }
                catch (Exception e)
                {
                    List<Suvirinimas> weldingGroup = suvirinimuGrupe;
                    Msg.ErrorMsg(e.Message);
                }
            }
            return ataskaita;
        }

        private int kiek(DataRow rw)
        {
            if (Convert.ToInt32(rw["k21"]) == 9) return 10;
            else return 8;
        }

        private bool lygiosEilutės(DataRow row1, DataRow row2, int langelių)
        {
            for (int i = 0; i < langelių; i++)
            {
                if (row1[i].ToString() != row2[i].ToString()) return false;
            }
            return true;
        }
    }
}