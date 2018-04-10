using System;
using System.Windows.Forms;

namespace SuvirinimaiApp
{
    class TvarkytiMisiniai : fTvarkytiItems
    {
        public TvarkytiMisiniai() :
            base("Suvirinimo mišiniai",
            "SELECT id AS ID, pavadinimas AS Mišinys, isActive AS Aktyvus FROM Misiniai ORDER BY isActive, pavadinimas;")
        { }


        protected override void doColumns()
        {
            dgvItems.Columns["Mišinys"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns["Mišinys"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        protected override void tsbDelete_Click(object sender, EventArgs e)
        {
            string sqlCheckInUse = "SELECT id FROM Aktai WHERE medz_misinKodasId={0};";
            string sqlDelete = "DELETE FROM Misiniai WHERE id={0};";

            deleteRecord(sqlCheckInUse, sqlDelete);
        }

        protected override void tsbNew_Click(object sender, EventArgs e)
        {
            string[] tekstai = {"Suvirinimo mišinys",
                                   "Įveskite naują mišinio pavadinimą:"};

            if (requestData(RequestScenario.text_textbox, tekstai) == DialogResult.Cancel) return;

            if (Program.pubString == string.Empty) return;

            string sqlCheckDupl = string.Format("SELECT id FROM Misiniai WHERE pavadinimas='{0}';", Program.pubString);
            string sqlInsert = string.Format("INSERT INTO Misiniai (pavadinimas) VALUES ('{0}');", Program.pubString);
            doThings(sqlCheckDupl, sqlInsert);
        }
    }
}
