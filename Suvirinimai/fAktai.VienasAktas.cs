using System;
using System.Windows.Forms;

namespace SuvirinimaiApp
{
    partial class fAktai
    {
        private void tsbNew_Click(object sender, EventArgs e)
        {
            using (fAktas fNaujasAktas = new fAktas())
            {
                fNaujasAktas.ShowDialog();
            }
            query();
        }

        private void tsbSimilar_Click(object sender, EventArgs e)
        {
            DataGridViewCell cl = dgvAktai.CurrentCell;

            // nefiltruoti, jeigu nieko nerodo
            if (cl.RowIndex < 0) return;

            long currentRecordId = Convert.ToInt64(dgvAktai.CurrentRow.Cells["id"].Value);

            using (fAktas fPanasusAktas = new fAktas(currentRecordId, true))
            {
                fPanasusAktas.ShowDialog();
            }
            query();
        }

        private void tsbEdit_Click(object sender, EventArgs e)
        {
            long currentRecordId = Convert.ToInt64(dgvAktai.CurrentRow.Cells["id"].Value);
            using (fAktas fTasAktas = new fAktas(currentRecordId, false))
            {
                fTasAktas.ShowDialog();
            }
            query();
        }
    }
}