using System;
using ewal.Msg;
using ewal.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SuvirinimaiApp.Properties;

namespace SuvirinimaiApp
{
    public partial class fEditText : Form
    {
        private List<string> inds;
        public fEditText(List<string> indeksai)
        {
            InitializeComponent();
            inds = indeksai;
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {            
            string sqlStr = string.Format("UPDATE Aktai SET aktas_trukumai='{0}' WHERE id IN {1};", DbHelper.Escape(txb.Text), whereByIds());
            performEdit(sqlStr);            
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            string sqlStr = string.Format("UPDATE Aktai SET aktas_trukumai='{0}' + aktas_trukumai WHERE id IN {1};", DbHelper.Escape(txb.Text), whereByIds());
            performEdit(sqlStr);
        }

        private void btnAppend_Click(object sender, EventArgs e)
        {
            string sqlStr = string.Format("UPDATE Aktai SET aktas_trukumai=aktas_trukumai + '{0}' WHERE id IN {1};", DbHelper.Escape(txb.Text), whereByIds());
            //MessageBox.Show(sqlStr);
            performEdit(sqlStr);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private string whereByIds()
        {
            return "(" + string.Join(", ", inds) + ")";
        }

        private void performEdit(string actionSql)
        {
            try
            {
                DbHelper.ExecuteNonQuery(actionSql);
            }
            catch
            {
                Msg.ErrorMsg(Messages.DbErrorMsg);
            }

            Close();
        }
    }
}
