using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SuvirinimaiApp
{
    public partial class fDates : Form
    {
        public fDates()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Program.firstDate = DateTime.MinValue;
            Program.lastDate = DateTime.MinValue;
            if (dtpFrom.Checked)
            {
                Program.firstDate = dtpFrom.Value;
            }
            if (dtpTill.Checked)
            {
                Program.lastDate = dtpTill.Value;
            }
            DialogResult = DialogResult.OK;
                
        }
    }
}
