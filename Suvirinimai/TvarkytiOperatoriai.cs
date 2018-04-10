using System;
using System.Windows.Forms;

namespace SuvirinimaiApp
{
    class TvarkytiOperatoriai : fTvarkytiItems
    {
        public TvarkytiOperatoriai() :
            base("Operatoriai",
            "SELECT id AS ID, vardas AS Vardas, isActive AS Aktyvus FROM Operatoriai ORDER BY isActive, vardas;")
        {}


        protected override void doColumns()
        {
            dgvItems.Columns["Vardas"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns["Vardas"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        protected override void buildInsertUpdateCommands()
        {
            // blank
        }

        protected override void tsbDelete_Click(object sender, EventArgs e)
        {
            string sqlCheckInUse = "SELECT id FROM Aktai WHERE tikrin_operatoriusId={0};";
            string sqlDelete = "DELETE FROM Operatoriai WHERE id={0};";

            deleteRecord(sqlCheckInUse, sqlDelete);
        }

        protected override void tsbNew_Click(object sender, EventArgs e)
        {
            string[] tekstai = {"Operatorius",
                                   "Įveskite operatoriaus kodą:"};

            if (requestData(RequestScenario.integer_textbox, tekstai) == DialogResult.Cancel) return;
            if (Program.pubInt < 0) return;

            string sqlCheckDupl = string.Format("SELECT id FROM Operatoriai WHERE id ={0};", Program.pubInt);
            string sqlInsert = string.Format("INSERT INTO Operatoriai (id) values ({0});", Program.pubInt);
            doThings(sqlCheckDupl, sqlInsert);
        }
    }
}
