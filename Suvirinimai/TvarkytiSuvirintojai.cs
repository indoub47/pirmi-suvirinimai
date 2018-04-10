using System;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;

namespace SuvirinimaiApp
{
    class TvarkytiSuvirintojai : fTvarkytiItems
    {
        public TvarkytiSuvirintojai() :
            base("Suvirintojai",
            "SELECT Suvirintojai.id AS ID, Suvirintojai.vardas AS Vardas, Suvirintojai.pazymNr as Pažymėjimas, Suvirintojai.isActive AS Aktyvus " +
                "FROM Suvirintojai " +
                "ORDER BY Suvirintojai.isActive, Suvirintojai.vardas;")
        {}

        protected override void doColumns()
        {
            dgvItems.Columns["Vardas"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns["Vardas"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        protected override void tsbDelete_Click(object sender, EventArgs e)
        {
            string sqlCheckInUse = "SELECT id FROM Aktai WHERE suvirint_suvirintojasId={0};";
            string sqlDelete = "DELETE FROM Suvirintojai WHERE id={0};";

            deleteRecord(sqlCheckInUse, sqlDelete);
        }

        protected override void tsbNew_Click(object sender, EventArgs e)
        {
            string[] tekstai = {"Suvirintojas",
                                   "Įveskite suvirintojo vardą:"};

            if (requestData(RequestScenario.text_textbox, tekstai) == DialogResult.Cancel) return;

            if (Program.pubString == string.Empty) return;

            string sqlCheckDupl = string.Format("SELECT id FROM Suvirintojai WHERE vardas='{0}';", Program.pubString);
            string sqlInsert = string.Format("INSERT INTO Suvirintojai (vardas) VALUES ('{0}');", Program.pubString);
            doThings(sqlCheckDupl, sqlInsert);
        }
    }
}
