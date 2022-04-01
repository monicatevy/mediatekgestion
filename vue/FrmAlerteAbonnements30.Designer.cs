
namespace Mediatek86.vue
{
    partial class FrmAlerteAbonnements30
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
            this.label1 = new System.Windows.Forms.Label();
            this.dgvAbonnements30Liste = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAbonnements30Liste)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(137, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(746, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ces abonnements expirent dans moins de 30 jours, pensez à les renouveler.";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // dgvAbonnements30Liste
            // 
            this.dgvAbonnements30Liste.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAbonnements30Liste.Location = new System.Drawing.Point(41, 89);
            this.dgvAbonnements30Liste.Name = "dgvAbonnements30Liste";
            this.dgvAbonnements30Liste.RowHeadersWidth = 82;
            this.dgvAbonnements30Liste.RowTemplate.Height = 33;
            this.dgvAbonnements30Liste.Size = new System.Drawing.Size(990, 462);
            this.dgvAbonnements30Liste.TabIndex = 1;
            // 
            // FrmAlerteAbonnements30
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1085, 589);
            this.Controls.Add(this.dgvAbonnements30Liste);
            this.Controls.Add(this.label1);
            this.Name = "FrmAlerteAbonnements30";
            this.Text = "Alerte";
            ((System.ComponentModel.ISupportInitialize)(this.dgvAbonnements30Liste)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgvAbonnements30Liste;
    }
}