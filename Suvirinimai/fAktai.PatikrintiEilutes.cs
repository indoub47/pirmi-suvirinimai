using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;
using ewal.Data;
using ewal.Msg;
using Nbb;
using SuvirinimaiApp.Properties;
using System.IO;

namespace SuvirinimaiApp
{
    partial class fAktai
    {
        // patikrina parinktas eilutes, ar jos tinkamos ataskaitai
        private void tsmiCheckSelectedRows_Click(object sender, EventArgs e)
        {
            // surašo, kokie id yra parinkti
            List<int> selectedIndices = new List<int>();
            foreach (DataGridViewRow dr in dgvAktai.SelectedRows)
            {
                selectedIndices.Add(Convert.ToInt32(dr.Cells["id"].Value));
            }
            doRowsCheck(selectedIndices);
        }

        // patikrina rodomas eilutes, ar jos tinkmos ataskaitai
        private void tsmiCheckExposedRows_Click(object sender, EventArgs e)
        {
            // surašo, kokie id yra rodomi
            List<int> exposedIndices = new List<int>();
            foreach (DataRow dr in dView.ToTable().Rows)
            {
                exposedIndices.Add(Convert.ToInt32(dr["id"]));
            }
            doRowsCheck(exposedIndices);
        }

        private void doRowsCheck(List<int> idList)
        {
            // atlieka eilutėse esančių kodų patikrinimą ir rezultatų atvaizdavimą
            string sqlstring = constructSqlStatement(idList);
            DataTable rowsToCheckTable = createDataTable(sqlstring);
            if (rowsToCheckTable.Rows.Count == 0) return;
            LGIF.loadData(obtainLgifxmlFileName());
            DataTable rowsCheckResult = checkRows(rowsToCheckTable);
            outputCheckResult(rowsCheckResult);
        }

        private void outputCheckResult(DataTable resultTable)
        {
            // rezultato išmetimas
            StringBuilder sbToFile = new StringBuilder();
            StringBuilder sbToMsgBox = new StringBuilder();
            string errorMsg;
            string idStr;
            foreach (DataRow drow in resultTable.Rows)
            {
                idStr = drow["id"].ToString();
                errorMsg = drow["errorMsg"].ToString();

                sbToFile.Append(idStr);
                sbToFile.Append("\t");

                if (errorMsg != string.Empty)
                {
                    sbToFile.Append(errorMsg);
                    sbToMsgBox.Append(idStr);
                    sbToMsgBox.Append(" - ");
                    sbToMsgBox.Append(errorMsg);
                    sbToMsgBox.Append("\r\n");
                }
                else
                {
                    sbToFile.Append("tvarkoje;");
                }
                sbToFile.Append("\r\n");
            }
            if (sbToMsgBox.Length == 0) sbToMsgBox.Append("Visi įrašai geri!");

            sbToMsgBox.Append("\r\n\r\nAr išsaugoti rezultatą tekstiniame faile?");

            if (Msg.YesNoQuestion(sbToMsgBox.ToString()) == DialogResult.No) return;
            else saveResultAsFile(sbToFile.ToString());
        }

        private void saveResultAsFile(string resultString)
        {
            // parinkti failo vardą                    
            saveFileDialog.Title = "Išsaugoti failą";
            saveFileDialog.FileName = Settings.Default.RowsCheckResultFileName;
            saveFileDialog.InitialDirectory = Settings.Default.RowsCheckOutputPath;
            saveFileDialog.Filter = string.Format("{0}|{1}", ".txt", "*.txt");
            saveFileDialog.FilterIndex = Settings.Default.FilterIndex;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.RowsCheckOutputPath = Path.GetDirectoryName(saveFileDialog.FileName);
                Settings.Default.FilterIndex = saveFileDialog.FilterIndex;
                Settings.Default.RowsCheckResultFileName = saveFileDialog.FileName;
                Settings.Default.Save();
            }
            else
            {
                return;
            }

            try
            {
                StreamWriter file = new System.IO.StreamWriter(saveFileDialog.FileName);
                file.WriteLine(resultString);
                file.Close();
                Msg.InformationMsg("Išsaugota: " + saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                Msg.ErrorMsg(ex.Message);
            }
        }

        private static string constructSqlStatement(List<int> idList)
        {
            // sukonstruojamas DB užklausos sakinys
            return "SELECT id, k11, k12, k21, k22, k23, k24, k31, k32, k41, k42, k51 " 
                + "FROM Aktai "
                + "WHERE id in (" + String.Join(",", idList.ToArray()) + ")";
        }

        private DataTable createDataTable(string sqlstring)
        {
            // parsineša duomenis iš DB - id ir vietos kodo skaitmenis
            try
            {
                return DbHelper.FillDataTable(sqlstring);
            }
            catch (DbException e)
            {
                Msg.ErrorMsg(Messages.DbErrorMsg);
                throw e;
            }
        }

        

        private DataTable checkRows(DataTable rowsTable)
        {
            // atlieka eilučių tikrinimo darbą
            DataTable resultTable = new DataTable();
            resultTable.Columns.Add("id", typeof(int));
            resultTable.Columns.Add("errorMsg", typeof(string));

            foreach (DataRow drow in rowsTable.Rows)
            {
                int id = Convert.ToInt32(drow["id"]);
                VietosKodas vk;

                // preliminarus patikrinimas
                try
                {
                    vk = new VietosKodas(drow);
                }
                catch (InvalidDuomenysException ide)
                {
                    resultTable.Rows.Add(id, ide.Message);
                    continue;
                }
                catch (InvalidKodasException ike)
                {
                    resultTable.Rows.Add(id, ike.Message);
                    continue;
                }
                catch (Exception ex)
                {
                    resultTable.Rows.Add(id, ex.Message);
                    continue;
                }

                // patikrinimas, nagrinėjant kodą
                try
                {
                    VietosKodasParser.parseSuvirinimas(vk);
                }
                catch (InvalidKodasException ike)
                {
                    resultTable.Rows.Add(id, ike.Message);
                    continue;
                }
                catch (Exception ex)
                {
                    resultTable.Rows.Add(id, ex.Message);
                    continue;
                }

                resultTable.Rows.Add(id, "");
            }
            return resultTable;
        }
    }
}