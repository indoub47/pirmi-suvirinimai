using System;
using ewal.Data;
using ewal.Msg;
using System.Windows.Forms;
using SuvirinimaiApp.Properties;

namespace SuvirinimaiApp
{
    public partial class fRequest : Form
    {
        private System.Windows.Forms.ComboBox cmbInt;
        private System.Windows.Forms.TextBox txbText;
        private RequestScenario scenarijus;

        public fRequest(RequestScenario scenario, string[] texts)
        {
            InitializeComponent();
            this.scenarijus = scenario;
            switch (this.scenarijus)
            {
                case RequestScenario.text_textbox:
                    this.txbText = new System.Windows.Forms.TextBox();
                    this.txbText.Location = new System.Drawing.Point(Settings.Default.requestBoxLocationX,
                                                                     Settings.Default.requestBoxLocationY);
	                this.txbText.Name = "txbText";
	                this.txbText.Size = new System.Drawing.Size(Settings.Default.requestStringTextBoxSizeX,
                                                                Settings.Default.requestStringTextBoxSizeY);
                    this.txbText.TextAlign = HorizontalAlignment.Left;
	                this.txbText.TabIndex = 1;
	                this.txbText.TabStop = true;
                    this.Controls.Add(this.txbText);
                    break;

                case  RequestScenario.integer_combobox:
                    this.cmbInt = new System.Windows.Forms.ComboBox();
                    this.cmbInt.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                    this.cmbInt.FormattingEnabled = true;
                    this.cmbInt.Location = new System.Drawing.Point(Settings.Default.requestBoxLocationX,
                                                                     Settings.Default.requestBoxLocationY);
                    this.cmbInt.Name = "cmbInt";
                    this.cmbInt.Size = new System.Drawing.Size(Settings.Default.requestComboBoxSizeX,
                                                                Settings.Default.requestComboBoxSizeY);
                    this.cmbInt.TabIndex = 1;
                    this.cmbInt.TabStop = true;
                    this.Controls.Add(this.cmbInt);
                    try
                    {
                        this.cmbInt.DataSource = DbHelper.FillDataTable(texts[2]);
                    }
                    catch
                    {
                        Msg.ErrorMsg(Messages.DbErrorMsg);
                        return;
                    }
                    this.cmbInt.ValueMember = "valueMember";
                    this.cmbInt.DisplayMember = "displayMember";
                    break;

                case  RequestScenario.integer_textbox:
                    this.txbText = new System.Windows.Forms.TextBox();
                    this.txbText.Location = new System.Drawing.Point(Settings.Default.requestBoxLocationX,
                                                                     Settings.Default.requestBoxLocationY);
	                this.txbText.Name = "txbText";
                    this.txbText.Size = new System.Drawing.Size(Settings.Default.requestIntTextBoxSizeX,
                                                                Settings.Default.requestIntTextBoxSizeY);
                    this.txbText.TextAlign = HorizontalAlignment.Right;
	                this.txbText.TabIndex = 1;
	                this.txbText.TabStop = true;
                    this.Controls.Add(this.txbText);
                    break;
            }
            Program.pubString = string.Empty;
            Program.pubInt = -1;
            this.Text = texts[0];
            lblRequest.Text = texts[1];
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            switch (this.scenarijus)
            {
                case RequestScenario.text_textbox:
                    if (!string.IsNullOrEmpty(txbText.Text.Trim()))
                    {
                        Program.pubString = txbText.Text.Trim();
                        this.DialogResult = DialogResult.OK;
                        Close();
                    }
                    break;

                case RequestScenario.integer_combobox:
                    if (cmbInt.SelectedIndex != -1)
                    {
                        try
                        {
                            Program.pubInt = Convert.ToInt32(cmbInt.SelectedValue);
                            this.DialogResult = DialogResult.OK;
                            Close();
                        }
                        catch
                        {
                            Program.pubInt = 0;
                            this.DialogResult = DialogResult.OK;
                            Close();
                        }
                    }
                    break;

                case RequestScenario.integer_textbox:
                    if (!string.IsNullOrEmpty(txbText.Text.Trim()))
                    {
                        try
                        {
                            Program.pubInt = Convert.ToInt32(txbText.Text.Trim());
                            this.DialogResult = DialogResult.OK;
                            Close();
                        }
                        catch
                        {
                            Msg.ExclamationMsg(Messages.Kodas_turi_buti_skaicius);
                            txbText.SelectAll();
                        }
                    }
                    break;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

    }
}
