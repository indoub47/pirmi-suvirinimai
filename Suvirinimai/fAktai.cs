using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ewal.Data;
using ewal.Msg;
using SuvirinimaiApp.Properties;

namespace SuvirinimaiApp
{
    enum ColumnType { intg, date, text, booln };


    public partial class fAktai : Form
    {
        private BindingSource bindingSource;
        private DbDataAdapter dAdapter;
        private DbConnection conn;
        private DataTable dTable;
        private DataView dView;
        //private string visiAktaiSql;
        private string savedFilter = string.Empty;
        private string savedOrder = string.Empty;
        protected bool hasChanges;
        List<string> filterElements = new List<string>();
        ArrayList rikiavimai = new ArrayList();
        string cc = " AND "; // current connector
        //string outputFileName;
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        string xslTemplateFileName = string.Empty;
        private bool dontRun = true;
        
        struct QueryParam
        {
            private string parameterName;
            private string sourceColumnName;
            private DbType valueType;
            internal QueryParam(string paramName, string columnName, DbType typeOfValue)
            {
                parameterName = paramName;
                sourceColumnName = columnName;
                valueType = typeOfValue;
            }

            public string Vardas
            {
                get { return parameterName; }
                set { parameterName = value; }
            }

            public string Stulpelis
            {
                get { return sourceColumnName; }
                set { sourceColumnName = value; }
            }

            public DbType Tipas
            {
                get { return valueType; }
                set { valueType = value; }
            }
        }

        public fAktai()
        {
            InitializeComponent();
            dAdapter = DbHelper.DPFactory.CreateDataAdapter();
            conn = DbHelper.DPFactory.CreateConnection();
            conn.ConnectionString = DbHelper.ConnectionString;
            dAdapter.SelectCommand = DbHelper.DPFactory.CreateCommand();
            dAdapter.SelectCommand.Connection = conn;
            dTable = new DataTable();
            bindingSource = new BindingSource();
            buildUpdateCommand();
        }

        private void fVisiSuvirinimai_Load(object sender, EventArgs e)
        {
            //StringBuilder sb = new StringBuilder("SELECT ");
            //sb.Append("Aktai.id, Aktai.aktas_Nr, Aktai.aktas_data, Aktai.aktas_arUzbaigtas, Aktai.aktas_trukumai, ");
            //sb.Append("Padaliniai.pavadinimas, Aktai.aktas_padalinysId, ");
            //sb.Append("BegiuTipai.pavadinimas AS begio_tipas, Suvirinotojai.vardas AS suvirintojo_vardas, ");
            //sb.Append("Aktai.k11, Aktai.k12, Aktai.k21, Aktai.k22, Aktai.k23, Aktai.k24, Aktai.k31, Aktai.k32, Aktai.k41, Aktai.k42, Aktai.k51, ");
            //sb.Append("Operatoriai.vardas, ");
            //sb.Append("Aktai.tikrin_arDefektas, Aktai.tikrin_defKodas, Aktai.tikrin_operatoriusId ");
            //sb.Append("FROM ");
            //sb.Append("(Aktai LEFT JOIN Padaliniai ON Aktai.aktas_padalinysId=Padaliniai.id) ");
            //sb.Append("LEFT JOIN Operatoriai ON Aktai.tikrin_operatoriusId=Operatoriai.id ");
            //sb.Append("ORDER BY Aktai.id DESC;");

            string sql = @"SELECT
                                Aktai.id, Aktai.aktas_Nr, Aktai.aktas_data, Aktai.aktas_arUzbaigtas, Aktai.aktas_trukumai, 
                                Padaliniai.pavadinimas, Aktai.aktas_padalinysId, 
                                BegiuTipai.pavadinimas AS begio_tipas, Suvirintojai.vardas AS suvirintojo_vardas, 
                                Aktai.k11, Aktai.k12, Aktai.k21, Aktai.k22, Aktai.k23, Aktai.k24, Aktai.k31, Aktai.k32, Aktai.k41, Aktai.k42, Aktai.k51, 
                                Operatoriai.vardas, 
                                Aktai.tikrin_arDefektas, Aktai.tikrin_defKodas, Aktai.tikrin_operatoriusId 
                            FROM 
                                (((Aktai LEFT JOIN Padaliniai ON Aktai.aktas_padalinysId = Padaliniai.id) 
                                LEFT JOIN Operatoriai ON Aktai.tikrin_operatoriusId = Operatoriai.id)
                                LEFT JOIN BegiuTipai ON Aktai.begis_tipasId = BegiuTipai.id) 
                                LEFT JOIN Suvirintojai ON Aktai.suvirint_suvirintojasId = Suvirintojai.id
                            ORDER BY Aktai.id DESC;";


            // visiAktaiSql = sb.ToString();
            dAdapter.SelectCommand.CommandText = sql;
            doColumns();
            query();
            dontRun = false;
        }

        private void query()
        {
            try
            { savedFilter = dView.RowFilter; }
            catch
            { savedFilter = string.Empty; }

            try
            { savedOrder = dView.Sort; }
            catch
            { savedOrder = "id DESC"; }

            int topRow = 0;
            int selectedRow = -1;
            try
            {
                topRow = dgvAktai.FirstDisplayedScrollingRowIndex;
                selectedRow = dgvAktai.SelectedRows[0].Index;
            }
            catch // (Exception e1)
            {
                // Msg.ErrorMsg(e1.Message);
            }

            dTable.Clear();
            dAdapter.Fill(dTable);
            dView = new DataView(dTable, savedFilter, savedOrder, DataViewRowState.CurrentRows);
            bindingSource.DataSource = dView;
            dgvAktai.DataSource = bindingSource;

            hasChanges = false;
            tsbSave.Enabled = false;
            updateStatusBar();

            try
            {
                dgvAktai.FirstDisplayedScrollingRowIndex = topRow;
                dgvAktai.Rows[selectedRow].Selected = true;
            }
            catch // (Exception e2)
            {
                // Msg.ErrorMsg(e2.Message);
            }
        }

        private void doColumns()
        {
            dgvAktai.AutoGenerateColumns = false;
            foreach (DataGridViewColumn column in dgvAktai.Columns)
            {
                column.DataPropertyName = column.Name;
            }

            //dgvAktai.AutoResizeColumns();
            dgvAktai.Columns["aktas_trukumai"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void tsbEditText_Click(object sender, EventArgs e)
        {
            // nauja fTextEdit su paduodamu parinktų eilučių indeksų sąrašu
            if (dgvAktai.SelectedRows.Count == 0) return;
            List<string> selectedIndices = new List<string>();
            foreach (DataGridViewRow rw in dgvAktai.SelectedRows)
            {
                selectedIndices.Add(rw.Cells["id"].Value.ToString());
            }
            using (fEditText textEditForm = new fEditText(selectedIndices))
            {
                textEditForm.ShowDialog();
            }
            query();
        }

        private void tsbSave_Click(object sender, EventArgs e)
        {
            endEditAndSave();
        }

        private void tsbExit_Click(object sender, EventArgs e)
        {
            if (hasChanges)
            {
                DialogResult result = Msg.YesNoCancelQuestion(Messages.Yra_pakeitimu_Ar_issaugoti);

                if (result == DialogResult.No)  // pakeitimų yra, bet neišsaugoti
                {
                    Close();
                }
                else if (result == DialogResult.Cancel) // pakeitimų yra, bet nutraukti veiksmą
                {
                    return;
                }
                else   // pakeitimų yra ir išsaugoti
                {
                    endEditAndSave();
                    Close();
                }
            }
            else // pakeitimų nėra - išeiti ir viskas
            {
                Close();
            }
        }

        private void tsmiFileClose_Click(object sender, EventArgs e)
        {
            if (hasChanges)
            {
                DialogResult result = Msg.YesNoCancelQuestion(Messages.Yra_pakeitimu_Ar_issaugoti);

                if (result == DialogResult.No)  // pakeitimų yra, bet neišsaugoti
                {
                    Close();
                }
                else if (result == DialogResult.Cancel) // pakeitimų yra, bet nutraukti veiksmą
                {
                    return;
                }
                else   // pakeitimų yra ir išsaugoti
                {
                    endEditAndSave();
                    Close();
                }
            }
            else // pakeitimų nėra - išeiti ir viskas
            {
                Close();
            }
        }

        private void tsbDeleteRecord_Click(object sender, EventArgs e)
        {
            if (dgvAktai.SelectedRows.Count != 1) return;
            string sqlDelete = "DELETE FROM Aktai WHERE id={0};";
            int id = Convert.ToInt32(dgvAktai.SelectedRows[0].Cells["id"].Value);

            // Paskutinį kartą patikslinama, ar tikrai ištrinti
            if (Msg.YesNoQuestion(Messages.Ar_tikrai_istrinti) == DialogResult.No)
            {
                return;
            }

            try
            {
                DbHelper.ExecuteNonQuery(string.Format(sqlDelete, id));
            }
            catch (DbException)
            {
                Msg.ErrorMsg(Messages.DbErrorMsg);
            }

            query();
        } 

        private void updateStatusBar()
        {
            int countChecked = 0;
            foreach (DataGridViewRow row in dgvAktai.Rows)
                if (Convert.ToBoolean(row.Cells["pagalbinis"].Value) == true) countChecked++;
            //int countChecked = dView.FindRows("Pagalbinis = TRUE").GetLength(0);
            lblStatusFilter.Text = "Filtras: " + dView.RowFilter + "  Rikiavimas: " + dView.Sort;
            lblStatus.Text = string.Format("Įrašų: {0}, pažymėta eilučių: {1}, pažymėta pagalbiniame: {2}.", dView.Count, dgvAktai.SelectedRows.Count, countChecked);
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

        private void endEditAndSave()
        {
            //MessageBox.Show("Įrašomi atlikti pakeitimai...");
            dgvAktai.EndEdit(DataGridViewDataErrorContexts.Commit);
            bindingSource.EndEdit();
            dAdapter.Update(dTable);

            hasChanges = false;
            tsbSave.Enabled = false;
        }
        
        private void buildUpdateCommand()
        {
            StringBuilder updSql = new StringBuilder("UPDATE Aktai SET ");
            updSql.Append("tikrin_arDefektas=@tikrin_arDefektas, ");
            updSql.Append("tikrin_defKodas=@tikrin_defKodas, ");
            updSql.Append("aktas_trukumai=@aktas_trukumai, ");
            updSql.Append("aktas_arUzbaigtas=@aktas_arUzbaigtas ");
            updSql.Append("WHERE id=@id;");

            DbCommand updateCommand = DbHelper.DPFactory.CreateCommand();
            updateCommand.CommandText = updSql.ToString();

            QueryParam[] parametrai = {new QueryParam("@tikrin_arDefektas", "tikrin_arDefektas", DbType.Boolean),
                                      new QueryParam("@tikrin_defKodas", "tikrin_defKodas", DbType.String),
                                      new QueryParam("@aktas_trukumai", "aktas_trukumai", DbType.String),
                                      new QueryParam("@aktas_arUzbaigtas", "aktas_arUzbaigtas", DbType.Boolean),
                                      new QueryParam("@id", "id", DbType.Int32)};
            DbParameter p;
            foreach (QueryParam parametras in parametrai)
            {
                p = DbHelper.DPFactory.CreateParameter();
                p.ParameterName = parametras.Vardas;
                p.SourceColumn = parametras.Stulpelis;
                p.DbType = parametras.Tipas;
                updateCommand.Parameters.Add(p);
            }
            dAdapter.UpdateCommand = updateCommand;
            dAdapter.UpdateCommand.Connection = conn;
        }

        private void dgvAktai_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dontRun) return;
            if (e.ColumnIndex != dgvAktai.Columns["pagalbinis"].Index)
            {
                hasChanges = true;
                tsbSave.Enabled = true;
            }
            else
                updateStatusBar();
        }

        private void dgvAktai_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvAktai.CurrentRow != null && dgvAktai.Rows[dgvAktai.CurrentRow.Index].Selected == true)
            {
                tsbDeleteRecord.Enabled = true;
                updateStatusBar();

            }
            else
            {
                tsbDeleteRecord.Enabled = false;
            }
        }

        private void tsmiImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Pasirinkite failą su aktų duomenimis";
            ofd.Filter = "xml|*.xml";
            ofd.FileName = Settings.Default.DefaultImportFileName;
            if (ofd.ShowDialog() != DialogResult.OK) return;
            DataSet dsNew = new DataSet();
            DataSet dsOld = new DataSet();
            DbDataAdapter currAdapter = DbHelper.DPFactory.CreateDataAdapter();
            DbConnection currConn = DbHelper.DPFactory.CreateConnection();
            DbCommandBuilder cb = DbHelper.DPFactory.CreateCommandBuilder();

            currConn.ConnectionString = DbHelper.ConnectionString;
            currAdapter.SelectCommand = DbHelper.DPFactory.CreateCommand();
            currAdapter.SelectCommand.Connection = conn;
            currAdapter.SelectCommand.CommandText = "SELECT * FROM Aktai;";
            //currAdapter.SelectCommand.CommandText = "SELECT id, aktas_padalinysId, aktas_Nr, aktas_data, aktas_trukumai, begis_tipasId, begis_protarpisMm, k11, k12, k21, k22, k23, k24, k31, k32, k41, k42, k51, medz_formaGamMetai, medz_misinGamMetai, medz_misinKodasId, medz_misinPartNr, medz_misinPorcNr, salyg_arSausa, salyg_oroTemp, salyg_begioTemp, suvirint_padalinysId, suvirint_suvirintojasId, tikrin_operatoriusId, tikrin_defektoskopasId, tikrin_nelygumaiVirsausMm, tikrin_nelygumaiSonoMm, sutvark_vadovasId, sutvark_padalinysId, tikrin_arDefektas, tikrin_defKodas, tikrin_sanduruCharakter FROM Aktai;";
            currAdapter.TableMappings.Add("Table", "Aktai");
            currAdapter.UpdateCommand = DbHelper.DPFactory.CreateCommand();
            cb.DataAdapter = currAdapter;
            currAdapter.InsertCommand = cb.GetInsertCommand();
    
            try
            {
                dsNew.ReadXmlSchema("suvirinimai-schema.xsd");
                dsNew.ReadXml(ofd.FileName);       
                currAdapter.Fill(dsOld);

                dsOld.Merge(dsNew);
                currAdapter.Update(dsOld);
                currConn.Close();
            }
            catch (Exception importFailException)
            {
                Msg.ErrorMsg(string.Format(Messages.XmlImport_fail, Environment.NewLine, importFailException.Message));
                return;
            }
            finally
            {
                currConn.Close();
            }

            query();
            Msg.InformationMsg(string.Format("Importuota {0} įraš..", dsNew.Tables["Aktai"].Rows.Count));
            Settings.Default.DefaultImportFileName = ofd.FileName;
            Settings.Default.Save();
        }

        /// <summary>
        /// Grąžina infrastruktūros duomenų failo pilną pavadinimą (path, name, extension).
        /// </summary>
        /// <remarks>
        /// Mėgina rasti <c>Settings.Default.InfraDataFileName</c> failą toje-pačioje-papkėje-kaip-programos-exe\etc\.
        /// Jeigu tokio nėra, tada prašo nurodyti.
        /// </remarks>
        /// <returns>Infrastruktūros duomenų failo pilnas <see cref="System.String"/> pavadinimas (path, name, extension)</returns>
        /// <exception cref="Nbb.FileIOFailureException">Išmeta <see cref="Nbb.FileIOFailureException"/>, kai neranda failo toje pačioje
        /// papkėje, kaip ir programos exe, ir kai nenurodomas joks kitas failas.</exception>
        private string obtainLgifxmlFileName()
        {
            string fullName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), Settings.Default.EtcDirName, Settings.Default.InfraDataFileName);
            if (!File.Exists(fullName))
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                ofd.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                ofd.Title = Settings.Default.SelectInfraDataFileDialogTitle;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    fullName = ofd.FileName;
                }
                else
                {
                    throw new Nbb.FileIOFailureException(Settings.Default.InfraDataFileNotChosenError);
                }
            }
            return fullName;
        }

        private void dgvAktai_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                toolTip1.RemoveAll();
                return;
            }

            if (e.ColumnIndex == dgvAktai.Columns["aktas_trukumai"].Index && 
                !String.IsNullOrWhiteSpace(dgvAktai.Rows[e.RowIndex].Cells["aktas_trukumai"].Value.ToString()))                
            {
                toolTip1.SetToolTip(dgvAktai, dgvAktai.Rows[e.RowIndex].Cells["aktas_trukumai"].Value.ToString());
            }
            else if (e.ColumnIndex == dgvAktai.Columns["pavadinimas"].Index)
            {
                toolTip1.SetToolTip(dgvAktai,
                    dgvAktai.Rows[e.RowIndex].Cells["begio_tipas"].Value.ToString() + Environment.NewLine +
                    dgvAktai.Rows[e.RowIndex].Cells["suvirintojo_vardas"].Value.ToString());
            }
            else
            {
                toolTip1.RemoveAll();
            }
        }

        private void dgvAktai_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            toolTip1.RemoveAll();
            return;
        }

    }
    

    

    

}
