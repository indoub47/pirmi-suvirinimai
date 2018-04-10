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

        private void tsmiThingsSuvirPadaliniai_Click(object sender, EventArgs e)
        {
            using (TvarkytiSuvirintojuPadaliniai frmPadaliniai = new TvarkytiSuvirintojuPadaliniai("Suvirintojų įmonės/padaliniai"))
            {
                frmPadaliniai.ShowDialog();
            }
        }

        private void tsmiThingsKelininkPadaliniai_Click(object sender, EventArgs e)
        {
            using (TvarkytiKelininkuPadaliniai frmPadaliniai = new TvarkytiKelininkuPadaliniai("Kelininkų įmonės/padaliniai"))
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

        private void tsmiBegiuTipai_Click(object sender, EventArgs e)
        {
            using (TvarkytiBegiuTipai frmBegiuTipai = new TvarkytiBegiuTipai())
            {
                frmBegiuTipai.ShowDialog();
            }
        }

        private void tsmiMisiniai_Click(object sender, EventArgs e)
        {
            using (TvarkytiMisiniai frmMisiniai = new TvarkytiMisiniai())
            {
                frmMisiniai.ShowDialog();
            }
        }

        private void tsmiSuvirintojai_Click(object sender, EventArgs e)
        {
            using (TvarkytiSuvirintojai frmSuvirintojai = new TvarkytiSuvirintojai())
            {
                frmSuvirintojai.ShowDialog();
            }
        }

        private void tsmiDarbuVadovai_Click(object sender, EventArgs e)
        {
            using (TvarkytiVadovai frmDVadovai = new TvarkytiVadovai())
            {
                frmDVadovai.ShowDialog();
            }
        }

        private void tsmiThingsKMFilialo_Click(object sender, EventArgs e)
        {
            using (TvarkytiKMFilialo frmKMFilialo = new TvarkytiKMFilialo())
            {
                frmKMFilialo.ShowDialog();
            }
        }

    }
}