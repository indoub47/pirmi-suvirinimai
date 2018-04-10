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
                    nudNelygumaiSonin.Value = 0;
                    nudNelygumaiVirs.Value = 0;
                    txbAktasNr.Focus();
                }                
            }         
        }        

        private void setupCombos()
        {
            setup_cmbVirinoPadalinys();
            setup_cmbBegiuTipas();
            setup_cmbMisinioKodas();
            setup_cmbVadovavoPadalinys();
            setup_cmbVadovavoSuvirintojas();
            setup_cmbTikrinoOperatorius();
            setup_cmbTikrinoDefektoskopu();
            setup_cmbSutvarkePadalinys();
            setup_cmbSutvarkeVadovas();
            setup_cmbPasiraseKM();
        }

        private void populateData()
        {
            DataRowView row = (DataRowView)bindingSource.Current;

            setTxbValue(txbId, row["id"]);
            setTxbValue(txbAktasNr, row["aktas_Nr"]);
            setCmbValue(cmbVirinoPadalinys, row["aktas_padalinysId"]);
            setDtpValue(dtpAktasData, row["aktas_data"]);

            setCmbValue(cmbBegiuTipas, row["begis_tipasId"]);

            string vtKodas = row["k11"].ToString() + row["k12"].ToString() + 
                                row["k21"].ToString() + row["k22"].ToString() + row["k23"].ToString() + row["k24"].ToString() + 
                                row["k31"].ToString() + row["k32"].ToString() + 
                                row["k41"].ToString() + row["k42"].ToString() + 
                                row["k51"].ToString();
            txbVietaKodas.Text = vtKodas;
            setIntNudValue(nudProtarpis, row["begis_protarpisMm"]);
            setIntNudValue(nudGMetaiFormu, row["medz_formaGamMetai"]);
            setIntNudValue(nudGMetaiMisinio, row["medz_misinGamMetai"]);

            setCmbValue(cmbMisinioKodas, row["medz_misinKodasId"]);
            setTxbValue(txbPartijosNr, row["medz_misinPartNr"]);
            setTxbValue(txbPorcijosNr, row["medz_misinPorcNr"]);

            setRdbValue(rdbSausa, rdbDregna, row["salyg_arSausa"]);
            setIntNudValue(nudOroT, row["salyg_oroTemp"]);
            setIntNudValue(nudBegioT, row["salyg_begioTemp"]);

            setCmbValue(cmbVadovavoPadalinys, row["suvirint_padalinysId"]);
            setCmbValue(cmbVadovavoSuvirintojas, row["suvirint_suvirintojasId"]);

            setCmbValue(cmbTikrinoOperatorius, row["tikrin_operatoriusId"]);
            setCmbValue(cmbTikrinoDefektoskopu, row["tikrin_defektoskopasId"]);
            setDecNudValue(nudNelygumaiVirs, row["tikrin_nelygumaiVirsausMm"]);
            setDecNudValue(nudNelygumaiSonin, row["tikrin_nelygumaiSonoMm"]);

            setCmbValue(cmbSutvarkePadalinys, row["sutvark_padalinysId"]);
            setCmbValue(cmbSutvarkeVadovas, row["sutvark_vadovasId"]);

            setRdbValue(rdbDefektasAptiktas, rdbDefektasNeaptiktas, row["tikrin_arDefektas"]);
            setTxbValue(txbDefektoKodas, row["tikrin_defKodas"]);
            setTxbValue(txbSanduruVietos, row["tikrin_sanduruCharakter"]);

            setCmbValue(cmbPasiraseKM, row["aktas_pasiraseKMId"]);

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

        private void setup_cmbBegiuTipas()
        {
            setupComboBox(cmbBegiuTipas,
                "SELECT id, pavadinimas FROM BegiuTipai WHERE isActive=True ORDER BY pavadinimas;",
                "pavadinimas",
                ColumnType.text,
                "id",
                true);
        }

        private void setup_cmbMisinioKodas()
        {
            setupComboBox(cmbMisinioKodas,
                "SELECT id, pavadinimas FROM Misiniai WHERE isActive=True ORDER BY pavadinimas;",
                "pavadinimas",
                ColumnType.text,
                "id",
                true);
        }

        private void setup_cmbVadovavoPadalinys()
        {
            setupComboBox(cmbVadovavoPadalinys,
                "SELECT id, pavadinimas FROM PadaliniaiSuvirintoju WHERE isActive=True ORDER BY pavadinimas;",
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

        private void setup_cmbSutvarkePadalinys()
        {
            setupComboBox(cmbSutvarkePadalinys,
                "SELECT id, pavadinimas FROM PadaliniaiKelininku WHERE isActive=True ORDER BY pavadinimas;",
                "pavadinimas",
                ColumnType.text,
                "id",
                true);
        }

        private void setup_cmbSutvarkeVadovas()
        {
            setupComboBox(cmbSutvarkeVadovas,
                "SELECT id, vardas FROM Vadovai WHERE isActive=True ORDER BY vardas;",
                "vardas",
                ColumnType.text,
                "id",
                true);
        }

        private void setup_cmbPasiraseKM()
        {
            setupComboBox(cmbPasiraseKM,
                "SELECT id, vardas FROM KMFilialo WHERE isActive=True ORDER BY vardas;",
                "vardas",
                ColumnType.text,
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

            cmbBegiuTipas.SelectedIndex = -1;

            txbVietaKodas.ResetText();

            nudProtarpis.ResetText();
            nudGMetaiFormu.ResetText();
            nudGMetaiMisinio.ResetText();

            cmbMisinioKodas.SelectedIndex = -1;
            txbPartijosNr.Text = string.Empty;
            txbPorcijosNr.Text = string.Empty;

            rdbSausa.Checked = true;
            rdbDregna.Checked = !rdbSausa.Checked;
            nudOroT.ResetText();
            nudBegioT.ResetText();

            cmbVadovavoPadalinys.SelectedIndex = -1;
            cmbVadovavoSuvirintojas.SelectedIndex = -1;

            cmbTikrinoOperatorius.SelectedIndex = -1;
            cmbTikrinoDefektoskopu.SelectedIndex = -1;
            nudNelygumaiVirs.Text = string.Empty;
            nudNelygumaiSonin.Text = string.Empty;

            cmbSutvarkePadalinys.SelectedIndex = -1;
            cmbSutvarkeVadovas.SelectedIndex = -1;

            rdbDefektasAptiktas.Checked = false;
            rdbDefektasNeaptiktas.Checked = !rdbDefektasAptiktas.Checked;
            txbDefektoKodas.Text = string.Empty;
            txbSanduruVietos.Text = string.Empty;

            cmbPasiraseKM.SelectedIndex = -1;

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
            catch
            {
                Msg.ErrorMsg(Messages.Irasas_neissaugotas);
            }
        }

        private void saveRecord()
        {
            // Start collecting data
            Dictionary<string, string> kvps = new Dictionary<string, string>();
            kvps.Add("aktas_Nr", txbValue(txbAktasNr));
            kvps.Add("aktas_padalinysId", cmbNumValue(cmbVirinoPadalinys));
            kvps.Add("aktas_data", dtpValue(dtpAktasData));
            kvps.Add("begis_tipasId", cmbNumValue(cmbBegiuTipas));
            kvps.Add("k11", txbVietaKodas.Text.Substring(0, 1));
            kvps.Add("k12", txbVietaKodas.Text.Substring(1, 1));
            kvps.Add("k21", txbVietaKodas.Text.Substring(3, 1));
            kvps.Add("k22", txbVietaKodas.Text.Substring(4, 1));
            kvps.Add("k23", txbVietaKodas.Text.Substring(5, 1));
            kvps.Add("k24", txbVietaKodas.Text.Substring(6, 1));
            kvps.Add("k31", txbVietaKodas.Text.Substring(8, 1));
            kvps.Add("k32", txbVietaKodas.Text.Substring(9, 1));
            kvps.Add("k41", txbVietaKodas.Text.Substring(11, 1));
            kvps.Add("k42", txbVietaKodas.Text.Substring(12, 1));
            kvps.Add("k51", txbVietaKodas.Text.Substring(14, 1));
            kvps.Add("begis_protarpisMm", nudIntValue(nudProtarpis));
            kvps.Add("medz_formaGamMetai", nudIntValue(nudGMetaiFormu));
            kvps.Add("medz_misinGamMetai", nudIntValue(nudGMetaiMisinio));
            kvps.Add("medz_misinKodasId", cmbNumValue(cmbMisinioKodas));
            kvps.Add("medz_misinPartNr", txbValue(txbPartijosNr));
            kvps.Add("medz_misinPorcNr", txbValue(txbPorcijosNr));
            kvps.Add("salyg_arSausa", rdbValue(rdbSausa, rdbDregna));
            kvps.Add("salyg_oroTemp", nudIntValue(nudOroT));
            kvps.Add("salyg_begioTemp", nudIntValue(nudBegioT));
            kvps.Add("suvirint_padalinysId", cmbNumValue(cmbVadovavoPadalinys));
            kvps.Add("suvirint_suvirintojasId", cmbNumValue(cmbVadovavoSuvirintojas));
            kvps.Add("tikrin_operatoriusId", cmbNumValue(cmbTikrinoOperatorius));
            kvps.Add("tikrin_defektoskopasId", cmbNumValue(cmbTikrinoDefektoskopu));
            kvps.Add("tikrin_nelygumaiVirsausMm", nudDecValue(nudNelygumaiVirs));
            kvps.Add("tikrin_nelygumaiSonoMm", nudDecValue(nudNelygumaiSonin));
            kvps.Add("sutvark_padalinysId", cmbNumValue(cmbSutvarkePadalinys));
            kvps.Add("sutvark_vadovasId", cmbNumValue(cmbSutvarkeVadovas));
            kvps.Add("tikrin_arDefektas", rdbValue(rdbDefektasAptiktas, rdbDefektasNeaptiktas));
            kvps.Add("tikrin_defKodas", txbValue(txbDefektoKodas));
            kvps.Add("tikrin_sanduruCharakter", txbValue(txbSanduruVietos));
            kvps.Add("aktas_pasiraseKMId", cmbNumValue(cmbPasiraseKM));
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
        
        private void nudProtarpis_Enter(object sender, EventArgs e)
        {
            nudProtarpis.Select(0, nudProtarpis.Text.Length);
        }

        private void nudGMetaiFormu_Enter(object sender, EventArgs e)
        {
            nudGMetaiFormu.Select(0, nudGMetaiFormu.Text.Length);
        }

        private void nudGMetaiMisinio_Enter(object sender, EventArgs e)
        {
            nudGMetaiMisinio.Select(0, nudGMetaiMisinio.Text.Length);
        }

        private void txbPartijosNr_Enter(object sender, EventArgs e)
        {
            txbPartijosNr.SelectAll();
        }

        private void txbPorcijosNr_Enter(object sender, EventArgs e)
        {
            txbPorcijosNr.SelectAll();
        }

        private void txbVietaKodas_Enter(object sender, EventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate()
            {
                txbVietaKodas.SelectAll();
            });           
        }

        private void nudOroT_Enter(object sender, EventArgs e)
        {
            nudOroT.Select(0, nudOroT.Text.Length);
        }

        private void nudBegioT_Enter(object sender, EventArgs e)
        {
            nudBegioT.Select(0, nudBegioT.Text.Length);
        }

        private void nudNelygumaiVirs_Enter(object sender, EventArgs e)
        {
            nudNelygumaiVirs.Select(0, nudNelygumaiVirs.Text.Length);
        }

        private void nudNelygumaiSonin_Enter(object sender, EventArgs e)
        {
            nudNelygumaiSonin.Select(0, nudNelygumaiSonin.Text.Length);
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

        private void tsmiBegiuTipai_Click(object sender, EventArgs e)
        {
            int begiuTipasId;
            try
            { begiuTipasId = Convert.ToInt32(cmbBegiuTipas.SelectedValue); }
            catch
            { begiuTipasId = -1; }

            using (TvarkytiBegiuTipai frmBegiuTipai = new TvarkytiBegiuTipai())
            {
                frmBegiuTipai.ShowDialog();
            }

            setup_cmbBegiuTipas();
            if (begiuTipasId != -1)
            { cmbBegiuTipas.SelectedValue = begiuTipasId; }
            else
            { cmbBegiuTipas.SelectedIndex = -1; }
        }

        private void tsmiMisiniai_Click(object sender, EventArgs e)
        {
            int misinioKodasId; // išsaugo, kad žinoti į kokį paskui atstatyti
            try
            { misinioKodasId = Convert.ToInt32(cmbMisinioKodas.SelectedValue); }
            catch
            { misinioKodasId = -1; }

            using (TvarkytiMisiniai frmMisiniai = new TvarkytiMisiniai())
            {
                frmMisiniai.ShowDialog();
            }

            setup_cmbMisinioKodas();
            if (misinioKodasId != -1)
            { cmbMisinioKodas.SelectedValue = misinioKodasId; }
            else
            { cmbMisinioKodas.SelectedIndex = -1; }
        }

        private void tsmiDVadovai_Click(object sender, EventArgs e)
        {
            int dVadovasId;
            try
            { dVadovasId = Convert.ToInt32(cmbSutvarkeVadovas.SelectedValue); }
            catch
            { dVadovasId = -1; }

            using (TvarkytiVadovai frmDVadovai = new TvarkytiVadovai())
            {
                frmDVadovai.ShowDialog();
            }

            setup_cmbSutvarkeVadovas();
            if (dVadovasId != -1)
            { cmbSutvarkeVadovas.SelectedValue = dVadovasId; }
            else
            { cmbSutvarkeVadovas.SelectedIndex = -1; }
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

        private void tsmiPadaliniaiSuvirintoju_Click(object sender, EventArgs e)
        {
            int vadovavo;
            try
            { vadovavo = Convert.ToInt32(cmbVadovavoPadalinys.SelectedValue); }
            catch
            { vadovavo = -1; }

            using (TvarkytiSuvirintojuPadaliniai frmPadaliniai = new TvarkytiSuvirintojuPadaliniai("Suvirintojų įmonės/padaliniai"))
            {
                frmPadaliniai.ShowDialog();
            }

            setup_cmbVadovavoPadalinys();
            if (vadovavo != -1)
            { cmbVadovavoPadalinys.SelectedValue = vadovavo; }
            else
            { cmbVadovavoPadalinys.SelectedIndex = -1; }
        }

        private void tsmiPadaliniaiKelininku_Click(object sender, EventArgs e)
        {
            int tvarke;
            try
            { tvarke = Convert.ToInt32(cmbSutvarkePadalinys.SelectedValue); }
            catch
            { tvarke = -1; }

            using (TvarkytiKelininkuPadaliniai frmPadaliniai = new TvarkytiKelininkuPadaliniai("Kelininkų įmonės/padaliniai"))
            {
                frmPadaliniai.ShowDialog();
            }

            setup_cmbSutvarkePadalinys();
            if (tvarke != -1)
            { cmbSutvarkePadalinys.SelectedValue = tvarke; }
            else
            { cmbSutvarkePadalinys.SelectedIndex = -1; }
        }

        private void tsmiKMFilialo_Click(object sender, EventArgs e)
        {
            int pasirase;
            try
            { pasirase = Convert.ToInt32(cmbPasiraseKM.SelectedValue); }
            catch
            { pasirase = -1; }

            using (TvarkytiKMFilialo frmKMFilialo = new TvarkytiKMFilialo())
            {
                frmKMFilialo.ShowDialog();
            }

            setup_cmbPasiraseKM();
            if (pasirase != -1)
            { cmbPasiraseKM.SelectedValue = pasirase; }
            else
            { cmbPasiraseKM.SelectedIndex = -1; }
        }

        #endregion

        private void tsbExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        
        
    }
}
