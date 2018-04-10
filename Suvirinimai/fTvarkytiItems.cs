using System;
using ewal.Msg;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;
using SuvirinimaiApp.Properties;
using ewal.Data;

namespace SuvirinimaiApp
{
    public enum RequestScenario { text_textbox, integer_textbox, integer_combobox };
    public abstract partial class fTvarkytiItems : Form
    {

        protected DbDataAdapter dAdapter;
        DbConnection conn;
        DbCommandBuilder cBuilder = DbHelper.DPFactory.CreateCommandBuilder();
        protected string mainSelectSql;
        protected BindingSource bindingSource = new BindingSource();
        protected DataTable dTable = new DataTable();
        protected DataView dView = new DataView();
        protected bool hasChanges = false;

        public fTvarkytiItems(string formTitle, string sqlSelectStatement)
        {
            InitializeComponent();
            this.Text = formTitle;
            mainSelectSql = sqlSelectStatement;
            dAdapter = DbHelper.DPFactory.CreateDataAdapter();
            conn = DbHelper.DPFactory.CreateConnection();
            conn.ConnectionString = DbHelper.ConnectionString;
            dAdapter.SelectCommand = DbHelper.DPFactory.CreateCommand();
            dAdapter.SelectCommand.Connection = conn;
            dAdapter.SelectCommand.CommandText = sqlSelectStatement;
            cBuilder = DbHelper.DPFactory.CreateCommandBuilder();
            cBuilder.DataAdapter = dAdapter;

            buildInsertUpdateCommands();
        }

        protected void fTvarkytiItems_Load(object sender, EventArgs e)
        {
            query();
            foreach (DataGridViewColumn column in dgvItems.Columns)
                { column.DataPropertyName = column.Name; }
            dgvItems.Columns["ID"].ReadOnly = true;
            dgvItems.AutoResizeColumns();
            doColumns();
            tsbSave.Enabled = false;
            tsbDelete.Enabled = false;
            this.dgvItems.SelectionChanged += new System.EventHandler(this.dgvItems_SelectionChanged);
        }

        protected void query()
        {
            dTable.Clear();
            dAdapter.Fill(dTable);
            dView = dTable.DefaultView;
            bindingSource.DataSource = dView;
            dgvItems.DataSource = bindingSource;
        }

        protected void tsbSave_Click(object sender, EventArgs e)
        {
            endEditAndSave();
        }

        protected void dgvItems_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            Msg.WarningMsg(Messages.Netinka_reiksme);
            return;
        }

        protected void dgvItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            hasChanges = true;
            tsbSave.Enabled = true;
        }

        protected void endEditAndSave()
        {
            dgvItems.EndEdit(DataGridViewDataErrorContexts.Commit);
            bindingSource.EndEdit();
            dAdapter.Update(dTable);
            hasChanges = false;
            tsbSave.Enabled = false;
        }

        protected void deleteRecord(ArrayList sqlsCheckIfInUse, string sqlDelete)
        {
            int id = Convert.ToInt32(dgvItems.SelectedRows[0].Cells["id"].Value);

            // turi nebūti įrašų su šituo objektu - nei aktų, nei vadovų,  nei suvirintojų
           			
			foreach (string sqlSt in sqlsCheckIfInUse)
			{
                try
                {
                    if (DbHelper.FetchSingleValue(string.Format(sqlSt, id)) != null)
                    {
                        Msg.ExclamationMsg(Messages.Naudojamas_negalima_istrinti);
                        return;
                    }
                }
                catch
                {
                    Msg.ErrorMsg(Messages.DbErrorMsg + " Kadangi neįmanoma patikrinti, ar nėra kur nors naudojamas, jo trinti negalima.");
                    return;
                }
			}
			
			// Paskutinį kartą patikslinama, ar tikrai ištrinti
            if (Msg.YesNoQuestion(Messages.Ar_tikrai_istrinti) == DialogResult.No)
            {
                return;
            }
            // else
            try
            {
                DbHelper.ExecuteNonQuery(string.Format(sqlDelete, id));
            }
            catch
            {
                Msg.ErrorMsg(Messages.DbErrorMsg);
            }

            query();
		}

        protected void deleteRecord(string sqlCheckIfInUse, string sqlDelete)
        {
            int id = Convert.ToInt32(dgvItems.SelectedRows[0].Cells["id"].Value);

            // turi nebūti įrašų su šituo objektu - nei aktų, nei vadovų,  nei suvirintojų
            try
            {
                if (DbHelper.FetchSingleValue(string.Format(sqlCheckIfInUse, id)) != null)
                {
                    Msg.ExclamationMsg(Messages.Naudojamas_negalima_istrinti);
                    return;
                }
            }
            catch
            {
                Msg.ErrorMsg(Messages.DbErrorMsg + " Kadangi neįmanoma patikrinti, ar nėra kur nors naudojamas, jo trinti negalima.");
                return;
            }
                        

            // Paskutinį kartą patikslinama, ar tikrai ištrinti
            if (Msg.YesNoQuestion(Messages.Ar_tikrai_istrinti) == DialogResult.No)
            {
                return;
            }

            try
            {
                DbHelper.ExecuteNonQuery(string.Format(sqlDelete, id));
            }
            catch
            {
                Msg.ErrorMsg(Messages.DbErrorMsg);
            }           

            query();
        }

        protected DialogResult requestData(RequestScenario scenario,
            string[] texts)
        {
            // texts[0] - requestFormCaption
            // texts[1] - requestLabelText
            // texts[2] - comboboxValuesSql
            using (fRequest requestForm = new fRequest(scenario, texts))
            {
                requestForm.ShowDialog();
                return requestForm.DialogResult;
            }
        }

        protected void doThings(string sqlFindDupl, string sqlInsert)
			{
                try
                {
                    if (DbHelper.FetchSingleValue(sqlFindDupl) != null)
                    {
                        Msg.ExclamationMsg(Messages.Objektas_jau_yra);
                        return;
                    }
                }
                catch
                {
                    Msg.ErrorMsg(Messages.DbErrorMsg + " Neįmanoma patikrinti, ar toks objektas jau yra. Todėl gali būti, kad sukursite duplikatą.");
                }
				
				try
				{
					DbHelper.ExecuteNonQuery(sqlInsert);
				}
				catch
				{
                    Msg.ErrorMsg(Messages.DbErrorMsg);
					return;
				}
				query();
				Msg.InformationMsg(Messages.Irasas_sukurtas);			
			}

        protected void doThings(string sqlInsertWithoutParam)
        {
            string padalinioIdString;
            if (Program.pubInt == -1)
            {
                return;
            }
            else if (Program.pubInt == 0)
            {
                padalinioIdString = "Null";
            }
            else
            {
                padalinioIdString = Program.pubInt.ToString();
            }

            try
            {
                DbHelper.ExecuteNonQuery(string.Format(sqlInsertWithoutParam, padalinioIdString));
            }
            catch
            {
                Msg.ErrorMsg(Messages.DbErrorMsg);
                return;
            }
            query();
            Msg.InformationMsg(Messages.Irasas_sukurtas);
        }

        protected void tsbExit_Click(object sender, EventArgs e)
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

        protected void dgvItems_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvItems.CurrentRow != null && dgvItems.Rows[dgvItems.CurrentRow.Index].Selected == true)
            {
                tsbDelete.Enabled = true;
            }
            else
            {
                tsbDelete.Enabled = false;
            }
        }

        protected virtual void buildInsertUpdateCommands()
        {
            // intentionally left blank
            // may be overrident in descendants
        }

        protected virtual void doColumns()
        {
            //dgvItems.Columns["Inv_Nr"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        protected virtual void tsbDelete_Click(object sender, EventArgs e)
        {
            // left blank
        }

        protected virtual void tsbNew_Click(object sender, EventArgs e)
        {
            // left blank
        }        

    }
}
