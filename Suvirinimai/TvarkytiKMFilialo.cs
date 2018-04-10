using System;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;

namespace SuvirinimaiApp
{
    class TvarkytiKMFilialo : fTvarkytiItems
    {
        public TvarkytiKMFilialo() :
            base("Filialo kelio meistrai",
            "SELECT KMFilialo.id AS ID, KMFilialo.vardas AS Vardas, KMFilialo.meistrija as Meistrija, KMFilialo.isActive AS Aktyvus " +
                "FROM KMFilialo " +
                "ORDER BY KMFilialo.isActive, KMFilialo.vardas;")
        { }

        protected override void doColumns()
        {
            dgvItems.Columns["Vardas"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns["Vardas"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        protected override void tsbDelete_Click(object sender, EventArgs e)
        {
            string sqlCheckInUse = "SELECT id FROM Aktai WHERE aktas_pasiraseKMId={0};";
            string sqlDelete = "DELETE FROM KMFilialo WHERE id={0};";

            deleteRecord(sqlCheckInUse, sqlDelete);
        }

        protected override void tsbNew_Click(object sender, EventArgs e)
        {
            string[] tekstai = {"Filialo kelio meistras",
                                   "Įveskite kelio meistro vardą:"};

            if (requestData(RequestScenario.text_textbox, tekstai) == DialogResult.Cancel) return;

            if (Program.pubString == string.Empty) return;

            string sqlCheckDupl = string.Format("SELECT id FROM KMFilialo WHERE vardas='{0}';", Program.pubString);
            string sqlInsert = string.Format("INSERT INTO KMFilialo (vardas) VALUES ('{0}');", Program.pubString);
            doThings(sqlCheckDupl, sqlInsert);
        }
    }
}
