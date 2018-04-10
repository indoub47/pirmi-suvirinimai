using System;
using System.Windows.Forms;

namespace SuvirinimaiApp
{
    class TvarkytiDefektoskopai : fTvarkytiItems
    {
        public TvarkytiDefektoskopai() :
            base("Defektoskopai",
            "SELECT id AS ID, gamyklNr AS Gam_Nr, tipas AS Tipas, isActive AS Aktyvus FROM Defektoskopai ORDER BY isActive, id;")
        {}


        protected override void doColumns()
        {
            dgvItems.Columns["Tipas"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        protected override void tsbDelete_Click(object sender, EventArgs e)
        {
            string sqlCheckInUse = "SELECT id FROM Aktai WHERE tikrin_defektoskopasId={0};";
            string sqlDelete = "DELETE FROM Defektoskopai WHERE id={0};";

            deleteRecord(sqlCheckInUse, sqlDelete);            
        }

        protected override void tsbNew_Click(object sender, EventArgs e)
        {
            string[] tekstai = {"Defektoskopas",
                                   "Įveskite naują defektoskopo kodą:"};

            if (requestData(RequestScenario.integer_textbox, tekstai) == DialogResult.Cancel) return;
            if (Program.pubInt < 0) return;

            string sqlCheckDupl = string.Format("SELECT id FROM Defektoskopai WHERE id={0};", Program.pubInt);
            string sqlInsert = string.Format("INSERT INTO Defektoskopai (id) VALUES ({0});", Program.pubInt);
            doThings(sqlCheckDupl, sqlInsert);
        }
    }
}
