using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SuvirinimaiApp.Properties;
using ewal.Msg;
using ewal.Data;

namespace SuvirinimaiApp
{
    public partial class fAktas : Form
    {
        BindingSource bindingSource = new BindingSource();
        long id = 0;
        bool makeSimilar = false;

        // konstruktorius redaguoti esama aktui
        public fAktas(long aktasId, bool createNew)
        {
            InitializeComponent();
            id = aktasId;
            makeSimilar = createNew;
        }
                
        // konstruktorius įvesti naują aktą
        public fAktas()
        {
            InitializeComponent();                       
        }

        private void fAktas_Load(object sender, EventArgs e)
        {
            setupCombos();
            if (id == 0)
            {
                this.bindingSource.DataSource = null;
            }
            else
            {
                string sqlStr = string.Format("SELECT * FROM Aktai WHERE id={0}", id);
                try
                {
                    this.bindingSource.DataSource = DbHelper.FillDataTable(sqlStr);
                }
                catch (DbException)
                {
                    Msg.ErrorMsg(Messages.DbErrorMsg + " Darbas su programa neįmanomas.");
                    return;
                }
                populateData();                

                if (makeSimilar)
                {
                    id = 0;
                    txbId.Text = string.Empty;
                    txbAktasNr.Text = string.Empty;   
                    txbAktasNr.Focus();
                }                
            }         
        }        

        private void setupCombos()
        {
            setup_cmbVirinoPadalinys();
            setup_cmbVadovavoSuvirintojas();
            setup_cmbTikrinoOperatorius();
            setup_cmbTikrinoDefektoskopu();
        }

        private void populateData()
        {
            DataRowView row = (DataRowView)bindingSource.Current;

            setTxbValue(txbId, row["id"]);
            setTxbValue(txbAktasNr, row["aktas_Nr"]);
            setCmbValue(cmbVirinoPadalinys, row["aktas_padalinysId"]);
            setDtpValue(dtpAktasData, row["aktas_data"]);

            string vtKodas = row["k1"].ToString() + "." + row["k2"].ToString() + "." +
                                row["k3"].ToString() + "." + row["k4"].ToString() + "." +
                                row["k5"].ToString() + "." + row["k6"].ToString();
            txbVietaKodas.Text = vtKodas;
            setCmbValue(cmbVadovavoSuvirintojas, row["suvirint_suvirintojasId"]);

            setCmbValue(cmbTikrinoOperatorius, row["tikrin_operatoriusId"]);
            setCmbValue(cmbTikrinoDefektoskopu, row["tikrin_defektoskopasId"]);

            setTxbValue(txbDefektoKodas, row["tikrin_defKodas"]);
            setTxbValue(txbSanduruVietos, row["tikrin_sanduruCharakter"]);

            setChbValue(chbAktasUzbaigtas, row["aktas_arUzbaigtas"]);
            setTxbValue(txbAktoTrukumai, row["aktas_trukumai"]);
        }

        private void setCmbValue(ComboBox cmb, object valueProvider)
        {
            try
            {
                cmb.SelectedValue = Convert.ToInt32(valueProvider);
            }
            catch
            {
                cmb.SelectedIndex = -1;
            }
        }

        private void setDtpValue(DateTimePicker dtp, object valueProvider)
        {
            try
            {
                dtp.Value = (DateTime)valueProvider;
                dtp.Checked = true;
            }
            catch
            {
                dtp.Value = dtp.MinDate;
                dtp.Checked = false;
                dtp.Text = string.Empty;
            }
        }

        private void setChbValue(CheckBox chb, object valueProvider)
        {
            try
            {
                chb.Checked = Convert.ToBoolean(valueProvider);
            }
            catch
            {
                chb.CheckState = CheckState.Indeterminate;
            }
        }
        
        private void setTxbValue(TextBox txb, object valueProvider)
        {
            try
            {
                txb.Text = valueProvider.ToString();
            }
            catch
            {
                txb.Text = string.Empty;
            }
        }

        private void setIntNudValue(NumericUpDown nud, object valueProvider)
        {
            try
            {
                nud.Value = Convert.ToInt32(valueProvider);
            }
            catch
            {
                nud.ResetText();
            }
        }

        private void setDecNudValue(NumericUpDown nud, object valueProvider)
        {
            try
            {
                nud.Value = Convert.ToDecimal(valueProvider);
            }
            catch
            {
                nud.ResetText();
            }
        }

        private void setRdbValue(RadioButton rdb, RadioButton alterRdb, object valueProvider)
        {
            try
            {
                rdb.Checked = Convert.ToBoolean(valueProvider);
                alterRdb.Checked = !rdb.Checked;
            }
            catch
            {
                rdb.Checked = false;
                alterRdb.Checked = false;
            }
        }

        private void setupComboBox(
                    ComboBox combo,
                    string sqlSelectStatement,
                    string displayMemberName,
                    ColumnType displayMemberType,
                    string valueMemberName,
                    bool emptyrow)
        {
            DataTable dTable;
            try
            {
                dTable = DbHelper.FillDataTable(sqlSelectStatement);
            }
            catch(DbException)
            {
                Msg.ErrorMsg(Messages.DbErrorMsg);
                return;
            }

            if (emptyrow)
            {
                DataRow emptyRow = dTable.NewRow();
                if (displayMemberType == ColumnType.text)
                {
                    emptyRow[displayMemberName] = string.Empty;
                }
                else
                {
                    emptyRow[displayMemberName] = DBNull.Value;
                }
                emptyRow[valueMemberName] = DBNull.Value;
                dTable.Rows.InsertAt(emptyRow, 0);
            }
            combo.DataSource = dTable;
            combo.DisplayMember = displayMemberName;
            combo.ValueMember = valueMemberName;
            //combo.SelectedIndex = 1;
        }

        private void setup_cmbVirinoPadalinys()
        {
            setupComboBox(cmbVirinoPadalinys,
                "SELECT id, pavadinimas FROM Padaliniai WHERE isActive=True ORDER BY pavadinimas;",
                "pavadinimas",
                ColumnType.text,
                "id",
                true);
        }

        private void setup_cmbVadovavoSuvirintojas()
        {
            setupComboBox(cmbVadovavoSuvirintojas,
                "SELECT id, vardas AS suvirintojas FROM Suvirintojai WHERE isActive=True ORDER BY vardas;",
                "suvirintojas",
                ColumnType.text,
                "id",
                true);
        }

        private void setup_cmbTikrinoOperatorius()
        {
            setupComboBox(cmbTikrinoOperatorius,
                "SELECT id, (vardas+', '+str(id)) as operatorius FROM Operatoriai WHERE isActive=True ORDER BY vardas;",
                "operatorius",
                ColumnType.text,
                "id",
                true);
        }

        private void setup_cmbTikrinoDefektoskopu()
        {
            setupComboBox(cmbTikrinoDefektoskopu,
                "SELECT id FROM Defektoskopai WHERE isActive=True ORDER BY id;",
                "id",
                ColumnType.intg,
                "id",
                true);
        }

        private void clearForm()
        {
            this.bindingSource.DataSource = null;

            txbId.Text = string.Empty;
            txbAktasNr.Text = string.Empty;
            cmbVirinoPadalinys.SelectedIndex = -1;

            dtpAktasData.Text = string.Empty;
            dtpAktasData.Checked = false;
            cmbVadovavoSuvirintojas.SelectedIndex = -1;

            cmbTikrinoOperatorius.SelectedIndex = -1;
            cmbTikrinoDefektoskopu.SelectedIndex = -1;
            txbDefektoKodas.Text = string.Empty;
            txbSanduruVietos.Text = string.Empty;

            chbAktasUzbaigtas.Checked = false;
            txbAktoTrukumai.Text = string.Empty;
        }

        private void tsbIsvalytiForma_Click(object sender, EventArgs e)
        {
            clearForm();
        }

        private void tsbIrasyti_Click(object sender, EventArgs e)
        {
            try
            {
                saveRecord();
                clearForm();
                // Msg.InformationMsg(Messages.Irasas_issaugotas);
            }
            catch (Exception ex)
            {
                Msg.ErrorMsg(Messages.Irasas_neissaugotas + "\r\n" + ex.Message);
            }
        }

        private void saveRecord()
        {
            // Start collecting data
            Dictionary<string, string> kvps = new Dictionary<string, string>();
            kvps.Add("aktas_Nr", txbValue(txbAktasNr));
            kvps.Add("aktas_padalinysId", cmbNumValue(cmbVirinoPadalinys));
            kvps.Add("aktas_data", dtpValue(dtpAktasData));

            string vt = txbVietaKodas.Text.Trim();
            string point = ".";
            string[] cols = { "k1", "k2", "k3", "k4", "k5", "k6" };
            int currentPosition, length;
            int pointPosition = -1;
            foreach(string col in cols)
            {
                currentPosition = pointPosition + 1;
                pointPosition = vt.IndexOf(point, currentPosition);
                length = pointPosition - currentPosition;
                kvps.Add(col, vt.Substring(currentPosition, length));
            }
            kvps.Add("suvirint_suvirintojasId", cmbNumValue(cmbVadovavoSuvirintojas));
            kvps.Add("tikrin_operatoriusId", cmbNumValue(cmbTikrinoOperatorius));
            kvps.Add("tikrin_defektoskopasId", cmbNumValue(cmbTikrinoDefektoskopu));
            kvps.Add("tikrin_defKodas", txbValue(txbDefektoKodas));
            kvps.Add("tikrin_sanduruCharakter", txbValue(txbSanduruVietos));
            kvps.Add("aktas_arUzbaigtas", chbValue(chbAktasUzbaigtas));
            kvps.Add("aktas_trukumai", txbValue(txbAktoTrukumai));
            // End collecting data

            // format sql statement
            string sqlSt;
            if (id == 0)
            {
                sqlSt = sqlInsert("Aktai", kvps);
            }
            else
            {
                sqlSt = sqlUpdate("Aktai", kvps, "id=" + id.ToString());
                //Close();
            }
           
            // execute sql
            // MessageBox.Show(sqlSt);
            try
            {
                DbHelper.ExecuteNonQuery(sqlSt);
            }
            catch(DbException)
            {
                Msg.ErrorMsg(Messages.DbErrorMsg);
            }
            //frmAllPlaces.dgvAllPlaces.Refresh();
            Close();
        }

        private string txbValue(TextBox txb)
        {
            string str;
            if (string.IsNullOrWhiteSpace(txb.Text))
            {
                str = string.Empty;
            }
            else
            {
                str = sanit(txb.Text);
            }
            return string.Format("'{0}'", str);
        }

        private string txbIntValue(TextBox txb)
        {
            try
            {
                return (Convert.ToInt32(txb.Text)).ToString();
            }
            catch
            {
                return "NULL";
            }
        }

        private string cmbNumValue(ComboBox cmb)
        {
            if (cmb.SelectedIndex > -1 && cmb.SelectedValue != null && !DBNull.Value.Equals(cmb.SelectedValue))
            {
                return sanit(cmb.SelectedValue.ToString());
            }
            else
            {
                return "NULL";
            }
        }

        private string nudIntValue(NumericUpDown nud)
        {
            try
            {
                return (Convert.ToInt32(nud.Text)).ToString();
            }
            catch
            {
                return "NULL";
            }
        }

        private string nudDecValue(NumericUpDown nud)
        {
            try
            {
                return (Convert.ToDecimal(nud.Text)).ToString().Replace(',', '.');
            }
            catch
            {
                return "NULL";
            }
        }

        //private string cmbStringValue(ComboBox cmb)
        //{
        //    // jeigu būtų toks cmb, kurio valuemember tipas būtų string
        
        //    if (cmb.SelectedIndex > -1 && cmb.SelectedValue != null && !DBNull.Value.Equals(cmb.SelectedValue))
        //    {
        //        return string.Format("'{0}'", sanit(cmb.SelectedValue.ToString()));
        //    }
        //    else
        //    {
        //        return "NULL";
        //    }
        //}

        private string dtpValue(DateTimePicker dtp)
        {
            if (dtp.Checked)
            {
                return DbHelper.FormatDateValue(dtp.Value);
            }
            else
            {
                return "NULL";
            }
        }

        private string chbValue(CheckBox chb)
        {
            if (chb.CheckState == CheckState.Indeterminate)
            {
                return "NULL";
            }
            else if (chb.Checked)
            {
                return "True";
            }
            else
            {
                return "False";
            }
        }

        private string rdbValue(RadioButton rdb, RadioButton alterRdb)
        {
            if ((rdb.Checked && alterRdb.Checked) || (!rdb.Checked && !alterRdb.Checked))
            {
                return "NULL";
            }
            else if (rdb.Checked)
            {
                return "True";
            }
            else
            {
                return "False";
            }
        }

        private string sanit(object controlValue)
        {
            // išvalo stringus nuo sql neleistinų simbolių - ' ir "
            // (nors gal būtų galima escapinti?)
            string strToSanitize = string.Empty;
            try
            {
                strToSanitize = controlValue.ToString();
            }
            catch
            {
                return string.Empty;
            }

            Regex forb = new Regex("['\"]*");
            return forb.Replace(strToSanitize, string.Empty).Trim();
        }

        private string sqlUpdate(string tableName, Dictionary<string, string> colvalpairs, string where)
        {
            // UPDATE TableName SET column=value, column=value, colum=value, ... WHERE id = id;

            StringBuilder sbColVal = new StringBuilder();
            foreach (KeyValuePair<string, string> colval in colvalpairs)
            {
                sbColVal.Append(string.Format("{0}={1}, ", colval.Key, colval.Value));
            }
            sbColVal.Remove(sbColVal.Length - 2, 2);

            return string.Format("UPDATE {0} SET {1} WHERE {2};", tableName, sbColVal.ToString(), where);
        }

        private string sqlInsert(string tableName, Dictionary<string, string> colvalpairs)
        {
            // INSERT INTO TableName (column, column, column, ...) VALUES (value, value, value, ...);

            StringBuilder sbColumns = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();
            foreach (KeyValuePair<string, string> colval in colvalpairs)
            {
                sbColumns.Append(colval.Key);
                sbColumns.Append(", ");

                sbValues.Append(colval.Value);
                sbValues.Append(", ");
            }

            sbColumns.Remove(sbColumns.Length - 2, 2);
            sbValues.Remove(sbValues.Length - 2, 2);

            return string.Format("INSERT INTO {0} ({1}) VALUES ({2});", tableName, sbColumns.ToString(), sbValues.ToString());
        }

        #region selectAllText_onEnter procedures

        private void txbAktasNr_Enter(object sender, EventArgs e)
        {
            txbAktasNr.SelectAll();
        } 

        private void txbVietaKodas_Enter(object sender, EventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate()
            {
                txbVietaKodas.SelectAll();
            });           
        }       

        private void txbDefektoKodas_Enter(object sender, EventArgs e)
        {
            txbDefektoKodas.SelectAll();
        }
        
        private void txbSanduruVietos_Enter(object sender, EventArgs e)
        {
            txbSanduruVietos.SelectAll();
        }

        private void txbAktoTrukumai_Enter(object sender, EventArgs e)
        {
            txbAktoTrukumai.SelectAll();
        }

        #endregion selectAllText_onEnter procedures
		
		#region dalykų tvarkymas

        private void tsmiPadaliniai_Click(object sender, EventArgs e)
        {
            int virino;
            try
            { virino = Convert.ToInt32(cmbVirinoPadalinys.SelectedValue); }
            catch
            { virino = -1; }
            
            using (TvarkytiPadaliniai frmPadaliniai = new TvarkytiPadaliniai("Padaliniai, rangovai"))
            {
                frmPadaliniai.ShowDialog();
            }

            setup_cmbVirinoPadalinys();
            if (virino != -1)
            { cmbVirinoPadalinys.SelectedValue = virino; }
            else
            { cmbVirinoPadalinys.SelectedIndex = -1; }
        }

        private void tsmiOperatoriai_Click(object sender, EventArgs e)
        {
            int operatoriusId;
            try
            { operatoriusId = Convert.ToInt32(cmbTikrinoOperatorius.SelectedValue); }
            catch
            { operatoriusId = -1; }

            using (TvarkytiOperatoriai frmOperatoriai = new TvarkytiOperatoriai())
            {
                frmOperatoriai.ShowDialog();
            }

            setup_cmbTikrinoOperatorius();
            if (operatoriusId != -1)
            { cmbTikrinoOperatorius.SelectedValue = operatoriusId; }
            else
            { cmbTikrinoOperatorius.SelectedIndex = -1; }
        }

        private void tsmiDefektoskopai_Click(object sender, EventArgs e)
        {
            int defektoskopasId;
            try
            { defektoskopasId = Convert.ToInt32(cmbTikrinoDefektoskopu.SelectedValue); }
            catch
            { defektoskopasId = -1; }

            using (TvarkytiDefektoskopai frmDefektoskopai = new TvarkytiDefektoskopai())
            {
                frmDefektoskopai.ShowDialog();
            }

            setup_cmbTikrinoDefektoskopu();
            if (defektoskopasId != -1)
            { cmbTikrinoDefektoskopu.SelectedValue = defektoskopasId; }
            else
            { cmbTikrinoDefektoskopu.SelectedIndex = -1; }
        }

        private void tsmiSuvirintojai_Click(object sender, EventArgs e)
        {
            int suvirintojasId;
            try
            { suvirintojasId = Convert.ToInt32(cmbVadovavoSuvirintojas.SelectedValue); }
            catch
            { suvirintojasId = -1; }

            using (TvarkytiSuvirintojai frmSuvirintojai = new TvarkytiSuvirintojai())
            {
                frmSuvirintojai.ShowDialog();
            }

            setup_cmbVadovavoSuvirintojas();
            if (suvirintojasId != -1)
            { cmbVadovavoSuvirintojas.SelectedValue = suvirintojasId; }
            else
            { cmbVadovavoSuvirintojas.SelectedIndex = -1; }
        }
        

        #endregion

        private void tsbExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        
        
    }
}
