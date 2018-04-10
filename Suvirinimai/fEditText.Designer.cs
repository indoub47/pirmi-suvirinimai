namespace SuvirinimaiApp
{
    partial class fEditText
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txb = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAppend = new System.Windows.Forms.Button();
            this.btnInsert = new System.Windows.Forms.Button();
            this.btnReplace = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txb
            // 
            this.txb.Location = new System.Drawing.Point(27, 21);
            this.txb.Multiline = true;
            this.txb.Name = "txb";
            this.txb.Size = new System.Drawing.Size(535, 43);
            this.txb.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(487, 79);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Atšaukti";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAppend
            // 
            this.btnAppend.Location = new System.Drawing.Point(351, 79);
            this.btnAppend.Name = "btnAppend";
            this.btnAppend.Size = new System.Drawing.Size(110, 23);
            this.btnAppend.TabIndex = 4;
            this.btnAppend.Text = "Įterpti gale";
            this.btnAppend.UseVisualStyleBackColor = true;
            this.btnAppend.Click += new System.EventHandler(this.btnAppend_Click);
            // 
            // btnInsert
            // 
            this.btnInsert.Location = new System.Drawing.Point(233, 79);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(110, 23);
            this.btnInsert.TabIndex = 3;
            this.btnInsert.Text = "Įterpti priekyje";
            this.btnInsert.UseVisualStyleBackColor = true;
            this.btnInsert.Click += new System.EventHandler(this.btnInsert_Click);
            // 
            // btnReplace
            // 
            this.btnReplace.Location = new System.Drawing.Point(115, 79);
            this.btnReplace.Name = "btnReplace";
            this.btnReplace.Size = new System.Drawing.Size(110, 23);
            this.btnReplace.TabIndex = 2;
            this.btnReplace.Text = "Pakeisti";
            this.btnReplace.UseVisualStyleBackColor = true;
            this.btnReplace.Click += new System.EventHandler(this.btnReplace_Click);
            // 
            // fEditText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 115);
            this.Controls.Add(this.btnReplace);
            this.Controls.Add(this.btnInsert);
            this.Controls.Add(this.btnAppend);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.txb);
            this.Name = "fEditText";
            this.Text = "Keisti tekstą";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txb;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAppend;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.Button btnReplace;
    }
}