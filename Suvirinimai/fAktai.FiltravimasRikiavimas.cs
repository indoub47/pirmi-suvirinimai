using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ewal.Data;
using ewal.Msg;
using SuvirinimaiApp.Properties;

namespace SuvirinimaiApp
{
    partial class fAktai
    {
        struct Rikiavimas
        {
            internal string columnName;
            internal bool ascending;
            internal Rikiavimas(string coln, bool scend)
            {
                columnName = coln;
                ascending = scend;
            }
        }        
        
        #region Filtravimas

        private void prefilter(string format)
        {
            // šitas metodas iš parinktų dgv langelių atrenka tuos, kurie
            // dalyvaus sudarant filtrą

            DataGridViewSelectedCellCollection selCells = dgvAktai.SelectedCells;
            if (selCells.Count == 0) return;

            // langelius į sąrašą, kad būtų galima manipuliuoti
            List<DataGridViewCell> list = new List<DataGridViewCell>();
            foreach (DataGridViewCell cl in selCells)
            {
                list.Add(cl);
            }

            // sąraše langeliai išrikiuojami 
            list.Sort(new DGVCellsComparer());

            int nonFiltered = dgvAktai.Columns["id"].Index;

            int currentColumnIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ColumnIndex == nonFiltered)
                    continue; //praleisti id stulpelį

                // imti kiekvieno stulpelio aukščiausiąjį langelį:
                if (list[i].ColumnIndex != currentColumnIndex) // jeigu naujas stulpelis...
                {
                    currentColumnIndex = list[i].ColumnIndex; // ... jį padaryti senu...
                    // ... su juo daryti filtro elementą
                    filterElements.Add(string.Format(format, makeFilterByCell(list[i])));
                }
            }
        }

        private void doFilter()
        {
            if (filterElements.Count > Settings.Default.MaxFilterParts)
            {
                Msg.ExclamationMsg(string.Format(Messages.Per_ilgas_filtras, Settings.Default.MaxFilterParts));
                while (filterElements.Count > Settings.Default.MaxFilterParts)
                    filterElements.RemoveAt(filterElements.Count - 1);
            }
            dView.RowFilter = String.Join(cc, filterElements);
            updateStatusBar();
        }

        private string makeFilterByCell(DataGridViewCell cl)
        {
            string columnName = dgvAktai.Columns[cl.ColumnIndex].Name;
            switch (columnName)
            {
                case "k11":
                case "k12":
                case "k21":
                case "k22":
                case "k23":
                case "k24":
                case "k31":
                case "k32":
                case "k41":
                case "k42":
                case "k51":
                    return makeFilterString(columnName, cl.Value, ColumnType.intg);

                case "aktas_Nr":
                case "tikrin_defKodas":
                case "aktas_trukumai":
                case "vieta":
                    return makeFilterString(columnName, cl.Value, ColumnType.text);

                case "tikrin_arDefektas":
                case "aktas_arUzbaigtas":
                    return makeFilterString(columnName, cl.Value, ColumnType.booln);

                case "aktas_data":
                    return makeFilterString(columnName, cl.Value, ColumnType.date);

                case "pavadinimas":
                    return makeFilterString("aktas_padalinysId", dgvAktai.Rows[cl.RowIndex].Cells["aktas_padalinysId"].Value, ColumnType.intg);

                case "vardas":
                    return makeFilterString("tikrin_operatoriusId", dgvAktai.Rows[cl.RowIndex].Cells["tikrin_operatoriusId"].Value, ColumnType.intg);

                default:
                    return string.Empty;
            }
        }

        private string makeFilterString(string parameterName, object cellValue, ColumnType type)
        {
            if (cellValue == null) // || string.IsNullOrEmpty(cellValue.ToString())
                return string.Format("{0} IS NULL", parameterName);

            switch (type)
            {
                case ColumnType.intg:
                    return string.Format("{0}={1}", parameterName, cellValue.ToString());

                case ColumnType.date:
                    return string.Format("{0}={1}", parameterName, DbHelper.FormatDateValue(cellValue));

                case ColumnType.text:
                    return string.Format("{0}={1}", parameterName, string.Format("'{0}'", sanit(cellValue)));

                case ColumnType.booln:
                    return string.Format("{0}={1}", parameterName, cellValue.ToString());

                default:
                    return string.Empty;
            }
        }

        private void tsbFilterAddPos_Click(object sender, EventArgs e)
        {
            // filtruoti įrašus pagal pasirinktų langelių reikšmes
            prefilter("{0}");
            doFilter();
        }

        private void tsbFilterByDate_Click(object sender, EventArgs e)
        {
            using (fDates formaDatos = new fDates())
            {
                if (formaDatos.ShowDialog() == DialogResult.OK)
                {
                    if (Program.firstDate != DateTime.MinValue)
                    {
                        try
                        {
                            filterElements.Add(string.Format("aktas_data>={0}", DbHelper.FormatDateValue(Program.firstDate)));
                        }
                        catch
                        {
                            Msg.ErrorMsg(Messages.DbErrorMsg);
                            return;
                        }
                    }
                    if (Program.lastDate != DateTime.MinValue)
                    {
                        try
                        {
                            filterElements.Add(string.Format("aktas_data<={0}", DbHelper.FormatDateValue(Program.lastDate)));
                        }
                        catch
                        {
                            Msg.ErrorMsg(Messages.DbErrorMsg);
                            return;
                        }
                    }

                    doFilter();
                }
            }
        }


        private void tsbFilterByMonth_Click(object sender, EventArgs e)
        {
            using (fMonth formaMenuo = new fMonth())
            {
                if (formaMenuo.ShowDialog() == DialogResult.OK)
                {
                    if (Program.firstDate != DateTime.MinValue)
                    {
                        try
                        {
                            filterElements.Add(string.Format("aktas_data>={0}", DbHelper.FormatDateValue(Program.firstDate)));
                        }
                        catch
                        {
                            Msg.ErrorMsg(Messages.DbErrorMsg);
                            return;
                        }
                    }
                    if (Program.lastDate != DateTime.MinValue)
                    {
                        try
                        {
                            filterElements.Add(string.Format("aktas_data<={0}", DbHelper.FormatDateValue(Program.lastDate)));
                        }
                        catch
                        {
                            Msg.ErrorMsg(Messages.DbErrorMsg);
                            return;
                        }
                    }

                    doFilter();
                }
            }
        }

        private void tsbFilterThisYear_Click(object sender, EventArgs e)
        {
            DateTime firstDayOfThisYear = new DateTime(DateTime.Now.Year, 1, 1);
            string filterThisYear = string.Format("aktas_data>={0}", DbHelper.FormatDateValue(firstDayOfThisYear));

            bool wasFiltered = false;
            int i = 0;
            int filterElementsCount = filterElements.Count;
            while (i < filterElementsCount)
            {
                if (filterElements[i] == filterThisYear)
                {
                    filterElements.RemoveRange(i, 1);
                    filterElementsCount--;
                    wasFiltered = true;
                }
                i++;
            }

            if (!wasFiltered) filterElements.Add(filterThisYear);
            doFilter();
        }

        private void tsbFilterAddNeg_Click(object sender, EventArgs e)
        {
            // neigiamas filtras pagal pasirinktų langelių reikšmes
            prefilter("NOT({0})");
            doFilter();
        }

        private void tsbFilterDrop_Click(object sender, EventArgs e)
        {
            // numeta filtrą ir rūšiavimą
            unfilter();
        }

        private void tsbFilterRemoveTail_Click(object sender, EventArgs e)
        {
            if (filterElements.Count > 0)
                filterElements.RemoveAt(filterElements.Count - 1);
            doFilter();
        }

        private void unfilter()
        {
            // numeta filtrą ir rūšiavimą
            dView.RowFilter = string.Empty;
            filterElements.Clear();
            updateStatusBar();
        }

        #endregion


        #region Rikiavimas

        private void preOrder(bool ascending)
        {
            // šitas metodas iš parinktų dgv langelių atrenka tuos stulpelius, kurie
            // dalyvaus sudarant rikiavimą 

            DataGridViewSelectedCellCollection selCells = dgvAktai.SelectedCells;
            if (selCells.Count == 0) return;

            // langelius į sąrašą, kad būtų galima manipuliuoti
            List<DataGridViewCell> list = new List<DataGridViewCell>();
            foreach (DataGridViewCell cl in selCells)
            {
                list.Add(cl);
            }

            // sąraše langeliai išrikiuojami 
            list.Sort(new DGVCellsComparer());

            int currentColumnIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                // imti kiekvieno stulpelio pirmąjį langelį:
                if (list[i].ColumnIndex != currentColumnIndex) // jeigu tai naujas stulpelis,...
                {
                    currentColumnIndex = list[i].ColumnIndex; // ... jį padaryti senu...
                    // ... ir su juo daryti filtro elementą
                    rikiavimai.Add(new Rikiavimas(dgvAktai.Columns[list[i].ColumnIndex].Name, ascending));
                }
            }
        }

        private void doOrder()
        {
            string sortOrder = string.Empty;
            foreach (Rikiavimas rik in rikiavimai)
            {
                sortOrder += rik.columnName + (rik.ascending ? string.Empty : " DESC") + ", ";
            }

            dView.Sort = sortOrder.Remove(sortOrder.Length - 2);
            updateStatusBar();
        }

        private void tsbAddOrderAsc_Click(object sender, EventArgs e)
        {
            preOrder(true);
            doOrder();
        }

        private void tsbAddOrderDesc_Click(object sender, EventArgs e)
        {
            preOrder(false);
            doOrder();
        }

        private void tsbDropOrder_Click(object sender, EventArgs e)
        {
            rikiavimai.Clear();
            dView.Sort = "";
            updateStatusBar();
        }

        private void customOrder(string[] paras)
        {
            foreach (Rikiavimas rik in rikiavimai)
            {
                foreach (string colname in paras)
                {
                    if (rik.columnName == colname)
                    {
                        Msg.ExclamationMsg(string.Format(Messages.Pagal_toki_jau_rikiuota, colname));
                        return;
                    }
                }
            }

            for (int i = 0; i < paras.Length; i++)
            {
                rikiavimai.Add(new Rikiavimas(paras[i], true));
            }
            doOrder();
        }

        #endregion

    }

    class DGVCellsComparer : IComparer<DataGridViewCell>
    {
        #region IComparer<DataGridViewCell> Members

        public int Compare(DataGridViewCell cl1, DataGridViewCell cl2)
        {
            // ASC pagal stulpelius ir tada ASC pagal eilutes
            int returnValue = 1;
            if (cl1 != null && cl2 == null)
            {
                returnValue = 0;
            }
            else if (cl1 == null && cl2 != null)
            {
                returnValue = 0;
            }
            else if (cl1 != null && cl2 != null)
            {
                if (cl1.ColumnIndex.Equals(cl2.ColumnIndex))
                {
                    returnValue = cl1.RowIndex.CompareTo(cl2.RowIndex);
                }
                else
                {
                    returnValue = cl1.ColumnIndex.CompareTo(cl2.ColumnIndex);
                }
            }
            return returnValue;
        }

        #endregion
    }

}