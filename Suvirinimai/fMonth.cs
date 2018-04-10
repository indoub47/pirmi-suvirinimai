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
    public partial class fMonth : Form
    {
        const int MIN_YEAR = 2002;
        const int MAX_YEAR = 2020;
        public fMonth()
        {
            InitializeComponent();
            string[] months = {"sausis", "vasaris", "kovas", "balandis",
                                "gegužė", "birželis", "liepa", "rugpjūtis", 
                                "rugsėjis", "spalis", "lapkritis", "gruodis"};

            cmbMonth.Items.AddRange(months);
            cmbMonth.SelectedIndex = DateTime.Today.Month-1;
            nudYear.Minimum = MIN_YEAR;
            nudYear.Maximum = MAX_YEAR;
            nudYear.Value = DateTime.Today.Year;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            int selectedYear = (int)nudYear.Value;
            int selectedMonth = cmbMonth.SelectedIndex + 1;


            Program.firstDate = new DateTime(selectedYear, selectedMonth, 1);
            Program.lastDate = Program.firstDate.AddMonths(1).AddDays(-1);
            
            DialogResult = DialogResult.OK;                
        }
    }
}
