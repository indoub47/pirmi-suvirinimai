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
Aktai.k1 & '.' & Aktai.k2 & '.' & Aktai.k3 & '.' & Aktai.k4 & '.' & Aktai.k5 & '.' & Aktai.k6 AS Vietos_kodas,
Padaliniai.pavadinimas AS Suvirinusio_padalinio_pavadinimas,
Padaliniai.oficPavadinimas AS Suvirinusio_padalinio_pilnas_pavadinimas,
Aktai.aktas_padalinysId AS Suvirinusio_padalinio_ID,
Suvirintojai.vardas AS Suvirintojo_vardas,
Aktai.suvirint_suvirintojasId AS Suvirintojo_ID,
Operatoriai.vardas AS Operatoriaus_vardas,
Aktai.tikrin_operatoriusId AS Operatoriaus_kodas,
Aktai.tikrin_defektoskopasId AS Defektoskopo_kodas, 
Defektoskopai.gamyklNr AS Defektoskopo_gamyklinis_Nr,
Defektoskopai.tipas AS Defektoskopo_tipas,
Aktai.tikrin_defKodas AS Defekto_kodas, 
Aktai.tikrin_sanduruCharakter AS Suvirinimo_sandūros_charakteristika, 
Aktai.aktas_arUzbaigtas AS Ar_aktas_užbaigtas, 
Aktai.aktas_trukumai AS Akto_trūkumai
FROM ((((Aktai 
LEFT JOIN Padaliniai ON Aktai.aktas_padalinysId = Padaliniai.id)
LEFT JOIN Suvirintojai ON Aktai.suvirint_suvirintojasId = Suvirintojai.id)
LEFT JOIN Operatoriai ON Aktai.tikrin_operatoriusID = Operatoriai.id)
LEFT JOIN Defektoskopai ON Aktai.tikrin_defektoskopasID = Defektoskopai.id)";
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