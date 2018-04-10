using System;
using System.Windows.Forms;

namespace SuvirinimaiApp
{
    class TvarkytiBegiuTipai : fTvarkytiItems
    {
        public TvarkytiBegiuTipai() :
            base("Bėgių tipai",
            "SELECT id AS ID, pavadinimas AS Tipas, isActive AS Aktyvus FROM BegiuTipai ORDER BY isActive, pavadinimas;")
        {}


        protected override void doColumns()
        {
            dgvItems.Columns["Tipas"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns["Tipas"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        protected override void tsbDelete_Click(object sender, EventArgs e)
        {
            string sqlCheckInUse = "SELECT id FROM Aktai WHERE begis_tipasId={0};";
            string sqlDelete = "DELETE FROM BegiuTipai WHERE id={0};";

            deleteRecord(sqlCheckInUse, sqlDelete);
        }

        protected override void tsbNew_Click(object sender, EventArgs e)
        {
            string[] tekstai = {"Bėgių tipas",
                                   "Įveskite naują bėgių tipo pavadinimą:"};

            if (requestData(RequestScenario.text_textbox, tekstai) == DialogResult.Cancel) return;
   
            if (Program.pubString == string.Empty) return;

            string sqlCheckDupl = string.Format("SELECT id FROM BegiuTipai WHERE pavadinimas='{0}';", Program.pubString);
            string sqlInsert = string.Format("INSERT INTO BegiuTipai (pavadinimas) VALUES ('{0}');", Program.pubString);
            doThings(sqlCheckDupl, sqlInsert);
        }
    }
}
