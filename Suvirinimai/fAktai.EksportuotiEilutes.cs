using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;
using ewal.Data;
using ewal.Msg;
using SuvirinimaiApp.Properties;

namespace SuvirinimaiApp
{
    partial class fAktai
    {
        // eksportuoja duomenų bazės lentelės Aktai įrašus į xml failą, kurį pasku galima importuoti
        // su ta pačia programa (Suvirinimai) į duomenų bazę.

        private void tsmiExportSelectedRows_Click(object sender, EventArgs e)
        {
            List<string> selectedIndices = new List<string>();
            foreach (DataGridViewRow dr in dgvAktai.SelectedRows)
            {
                selectedIndices.Add("Aktai.id=" + dr.Cells["id"].Value.ToString());
            }
            doRowsExport(selectedIndices);
        }

        private void tsmiExportPresentedRows_Click(object sender, EventArgs e)
        {
            List<string> selectedIndices = new List<string>();
            foreach (DataRow dr in dView.ToTable().Rows)
            {
                selectedIndices.Add("Aktai.id=" + dr["id"].ToString());
            }

            doRowsExport(selectedIndices);
        }

        private void doRowsExport(List<string> indices)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Išsaugoti failą";
            sfd.Filter = "xml|*.xml|csv|*.csv";
            sfd.DefaultExt = "csv";
            sfd.FileName = Settings.Default.RowsExportFileName;
            sfd.FilterIndex = Settings.Default.RowsExportFileDialogFilerIndex;
            if (sfd.ShowDialog() != DialogResult.OK) return;            

            DataTable tblExport;
            if (indices.Count == 0) return;

            string where = string.Join(" OR ", indices);
            string big_query = @"SELECT 
Aktai.id AS Akto_ID, 
Aktai.aktas_Nr AS Akto_Nr, 
Aktai.aktas_data AS Akto_Data,
Aktai.k11 & Aktai.k12 & '.' & Aktai.k21 & Aktai.k22 & Aktai.k23 & Aktai.k24 & '.' & Aktai.k31 & Aktai.k32 & '.' & Aktai.k41 & Aktai.k42 & '.' & Aktai.k51 AS Vietos_kodas,
Padaliniai.pavadinimas AS Suvirinusio_padalinio_pavadinimas,
Padaliniai.oficPavadinimas AS Suvirinusio_padalinio_pilnas_pavadinimas,
Padaliniai.arRangovas AS Ar_suvirino_rangovas,
Aktai.aktas_padalinysId AS Suvirinusio_padalinio_ID,
Suvirintojai.vardas AS Suvirintojo_vardas,
Suvirintojai.pazymNr AS Suvirintojo_pažymėjimo_Nr,
Aktai.suvirint_suvirintojasId AS Suvirintojo_ID,
PadaliniaiSuvirintoju.pavadinimas AS Suvirintojo_padalinio_pavadinimas,
PadaliniaiSuvirintoju.oficPavadinimas AS Suvirintojo_padalinio_oficialus_pavadinimas, 
Aktai.suvirint_padalinysId AS Suvirintojo_įmonės_arba_padalinio_ID, 
Vadovai.vardas AS Suvirinimo_vietą_sutvarkiusios_brigados_vadovo_vardas,
Vadovai.pareigos AS Suvirinimo_vietą_sutvarkiusios_brigados_vadovo_pareigos,
Aktai.sutvark_vadovasId AS Suvirinimo_vietą_sutvarkiusios_brigados_vadovo_ID,
PadaliniaiKelininku.pavadinimas AS Suvirinimo_vietą_sutvarkiusios_brigados_pavadinimas,
PadaliniaiKelininku.oficPavadinimas AS Suvirinimo_vietą_sutvarkiusios_brigados_oficialus_pavadinimas,
Aktai.sutvark_padalinysId AS Suvirinimo_vietą_sutvarkiusios_brigados_ID, 
Operatoriai.vardas AS Operatoriaus_vardas,
Aktai.tikrin_operatoriusId AS Operatoriaus_kodas,
Aktai.tikrin_defektoskopasId AS Defektoskopo_kodas, 
Defektoskopai.gamyklNr AS Defektoskopo_gamyklinis_Nr,
Defektoskopai.tipas AS Defektoskopo_tipas,
BegiuTipai.pavadinimas AS Bėgio_tipas,
Aktai.begis_tipasID AS Bėgio_tipo_ID,
Aktai.begis_protarpisMm AS Protarpis_mm,
Aktai.medz_formaGamMetai AS Suvirinimo_formos_gam_metai, 
Aktai.medz_misinGamMetai AS Suvirinimo_mišinio_gam_metai, 
Aktai.medz_misinKodasId AS Suvirinimo_mišinio_ID,
Misiniai.pavadinimas AS Suvirinimo_mišinio_pavadinimas, 
Aktai.medz_misinPartNr AS Suvirinimo_mišinio_partijos_Nr, 
Aktai.medz_misinPorcNr AS Suvirinimo_mišinio_porcijos_Nr, 
Aktai.salyg_arSausa AS Ar_sausa, 
Aktai.salyg_oroTemp AS Oro_temperatūra, 
Aktai.salyg_begioTemp AS Bėgio_temperatūra, 
Aktai.tikrin_nelygumaiVirsausMm AS Suvirintos_vietos_viršaus_nelygumai_mm, 
Aktai.tikrin_nelygumaiSonoMm AS Suvirintos_vietos_šono_nelygumai_mm,
Aktai.tikrin_arDefektas AS Ar_nustatytas_defektas, 
Aktai.tikrin_defKodas AS Defekto_kodas, 
Aktai.tikrin_sanduruCharakter AS Suvirinimo_sandūros_charakteristika, 
Aktai.aktas_arUzbaigtas AS Ar_aktas_užbaigtas, 
Aktai.aktas_trukumai AS Akto_trūkumai,
Aktai.aktas_pasiraseKMId AS Pasirašiusio_filialo_KM_ID,
KMFilialo.vardas AS Filialo_KM_vardas,
KMFilialo.meistrija AS Filialo_meistrija
FROM ((((((((((Aktai 
LEFT JOIN Padaliniai ON Aktai.aktas_padalinysId = Padaliniai.id)
LEFT JOIN Suvirintojai ON Aktai.suvirint_suvirintojasId = Suvirintojai.id)
LEFT JOIN PadaliniaiSuvirintoju ON Aktai.suvirint_padalinysId = PadaliniaiSuvirintoju.id)
LEFT JOIN Vadovai ON Aktai.sutvark_vadovasId = Vadovai.id)
LEFT JOIN PadaliniaiKelininku ON Aktai.sutvark_padalinysId = PadaliniaiKelininku.id)
LEFT JOIN Operatoriai ON Aktai.tikrin_operatoriusID = Operatoriai.id)
LEFT JOIN Defektoskopai ON Aktai.tikrin_defektoskopasID = Defektoskopai.id)
LEFT JOIN BegiuTipai ON Aktai.begis_tipasID = BegiuTipai.id)
LEFT JOIN Misiniai ON Aktai.medz_misinKodasId = Misiniai.id)
LEFT JOIN KMFilialo ON Aktai.aktas_pasiraseKMId = KMFilialo.id)";
            try
            {
                // tblExport = DbHelper.FillDataTable("SELECT * FROM Aktai WHERE " + where);    
                tblExport = DbHelper.FillDataTable(big_query + " WHERE " + where);  
            }
            catch (DbException)
            {
                Msg.ErrorMsg(Messages.DbErrorMsg);
                return;
            }
            
            tblExport.TableName = "Aktai";



            try
            {

                switch (sfd.FilterIndex)
                {
                    case 1:
                        tblExport.WriteXml(sfd.FileName);
                        break;
                    default:
                        System.IO.StreamWriter file = new System.IO.StreamWriter(sfd.FileName);
                        file.WriteLine(DataTableToCsv(tblExport));
                        file.Close();
                        break;
                } 
            }
            catch (Exception sudas)
            {
                Msg.ErrorMsg("Ekportas nepavyko. Priežastis: " + sudas.Message);
                return;
            }
            Msg.InformationMsg(string.Format("Eksportuota sėkmingai {0} įraš., į failą {1}.", indices.Count, sfd.FileName));
        }

        private string DataTableToCsv(DataTable dataTable, string delimiter = ";", bool exportColumnHeaders = true)
        {
            
            StringBuilder sb = new StringBuilder();
            int columnsCount = dataTable.Columns.Count;
            string endline = "\n";

            if (exportColumnHeaders)
            {
                for (int i = 0; i < columnsCount; i++ )
                {
                    sb.Append(dataTable.Columns[i].ColumnName);
                    if (i < columnsCount - 1)
                        sb.Append(delimiter);
                    else
                        sb.Append(endline);
                }
            }

            foreach (DataRow row in dataTable.Rows)
            {
                for (int c = 0; c < columnsCount; c++)
                {
                    sb.Append(row[c].ToString());
                    if (c < columnsCount - 1)
                        sb.Append(delimiter);
                    else
                        sb.Append(endline);
                }
            }

            sb.Remove(sb.Length - endline.Length, endline.Length);
            return sb.ToString();
        }
    }
}