using System;

namespace SuvirinimaiApp
{
    partial class fAktai
    {
        private void tsmiPadaliniai_Click(object sender, EventArgs e)
        {
            using (TvarkytiPadaliniai frmPadaliniai = new TvarkytiPadaliniai("Padaliniai, rangovai"))
            {
                frmPadaliniai.ShowDialog();
            }
        }

        private void tsmiOperatoriai_Click(object sender, EventArgs e)
        {
            using (TvarkytiOperatoriai frmOperatoriai = new TvarkytiOperatoriai())
            {
                frmOperatoriai.ShowDialog();
            }
        }

        private void tsmiDefektoskopai_Click(object sender, EventArgs e)
        {
            using (TvarkytiDefektoskopai frmDefektoskopai = new TvarkytiDefektoskopai())
            {
                frmDefektoskopai.ShowDialog();
            }
        }

        private void tsmiSuvirintojai_Click(object sender, EventArgs e)
        {
            using (TvarkytiSuvirintojai frmSuvirintojai = new TvarkytiSuvirintojai())
            {
                frmSuvirintojai.ShowDialog();
            }
        }

    }
}