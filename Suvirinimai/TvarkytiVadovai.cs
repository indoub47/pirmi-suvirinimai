using System;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;

namespace SuvirinimaiApp
{
    class TvarkytiVadovai : fTvarkytiItems
    {
        public TvarkytiVadovai() :
            base("Darbų vadovai",
            "SELECT Vadovai.id AS ID, Vadovai.vardas AS Vardas, Vadovai.pareigos AS Pareigos, Vadovai.isActive AS Aktyvus " +
                "FROM Vadovai " +
                "ORDER BY Vadovai.isActive, Vadovai.vardas;")
        {}


        protected override void doColumns()
        {
            dgvItems.Columns["Vardas"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns["Pareigos"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }
        
        protected override void tsbDelete_Click(object sender, EventArgs e)
        {
            string sqlCheckInUse = "SELECT id FROM Aktai WHERE sutvark_vadovasId={0};";
            string sqlDelete = "DELETE FROM Vadovai WHERE id={0};";

            deleteRecord(sqlCheckInUse, sqlDelete);
        }

        protected override void tsbNew_Click(object sender, EventArgs e)
        {
            string[] tekstai = {"Kelio darbų vadovas",
                                   "Įveskite kelio darbų vadovo vardą:"};

            if (requestData(RequestScenario.text_textbox, tekstai) == DialogResult.Cancel) return;

            if (Program.pubString == string.Empty) return;

            string sqlCheckDupl = string.Format("SELECT id FROM Vadovai WHERE vardas='{0}';", Program.pubString);
            string sqlInsert = string.Format("INSERT INTO Vadovai (vardas) VALUES ('{0}');", Program.pubString);
            doThings(sqlCheckDupl, sqlInsert);
        }
    }
}
