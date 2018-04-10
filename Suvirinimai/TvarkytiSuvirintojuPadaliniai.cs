using System;
using System.Collections;
using System.Windows.Forms;
using SuvirinimaiApp.Properties;

namespace SuvirinimaiApp
{
    class TvarkytiSuvirintojuPadaliniai : fTvarkytiItems
    {
        public TvarkytiSuvirintojuPadaliniai(string formTitle) :
            base(formTitle,
            "SELECT id AS ID, pavadinimas AS Pavadinimas, oficPavadinimas AS Ofic_pavadinimas, isActive AS Aktyvus FROM PadaliniaiSuvirintoju ORDER BY isActive, pavadinimas;")
        { }


        protected override void doColumns()
        {
            dgvItems.Columns["Pavadinimas"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns["Pavadinimas"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvItems.Columns["Ofic_pavadinimas"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; 
        }

        protected override void tsbDelete_Click(object sender, EventArgs e)
        {
            ArrayList sqlsCheckIfUsed = new ArrayList();
            sqlsCheckIfUsed.Add("SELECT id FROM Aktai WHERE suvirint_padalinysId={0}");
            string sqlDelete = "DELETE FROM PadaliniaiSuvirintoju WHERE id={0};";

            deleteRecord(sqlsCheckIfUsed, sqlDelete);
        }

        protected override void tsbNew_Click(object sender, EventArgs e)
        {
            string[] tekstai = {"Padaliniai, rangovai",
                                   "Įveskite naujos suvirintojų įmonės trumpą pavadinimą:"};

            if (requestData(RequestScenario.text_textbox, tekstai) == DialogResult.Cancel) return;
            if (Program.pubString == string.Empty) return;
 
            string sqlCheckDupl  = string.Format("SELECT id FROM PadaliniaiSuvirintoju WHERE pavadinimas='{0}';",  Program.pubString);
            string sqlInsert = string.Format("INSERT INTO PadaliniaiSuvirintoju (pavadinimas, oficPavadinimas) values ('{0}', '{0}');", Program.pubString);
            doThings(sqlCheckDupl, sqlInsert);
        }
    }
}
