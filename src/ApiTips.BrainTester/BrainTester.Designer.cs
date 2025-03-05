namespace ApiTips.BrainTester
{
    partial class BrainTester
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tbHistory = new RichTextBox();
            tbEnterMessage = new RichTextBox();
            label1 = new Label();
            label2 = new Label();
            btnSend = new Button();
            btnDisconnect = new Button();
            btnConnect = new Button();
            label3 = new Label();
            lbConnect = new Label();
            SuspendLayout();
            // 
            // tbHistory
            // 
            tbHistory.Location = new Point(398, 81);
            tbHistory.Name = "tbHistory";
            tbHistory.ReadOnly = true;
            tbHistory.Size = new Size(439, 436);
            tbHistory.TabIndex = 0;
            tbHistory.Text = "";
            // 
            // tbEnterMessage
            // 
            tbEnterMessage.Location = new Point(12, 81);
            tbEnterMessage.Name = "tbEnterMessage";
            tbEnterMessage.ReadOnly = true;
            tbEnterMessage.Size = new Size(380, 368);
            tbEnterMessage.TabIndex = 1;
            tbEnterMessage.Text = "";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 63);
            label1.Name = "label1";
            label1.Size = new Size(160, 15);
            label1.TabIndex = 2;
            label1.Text = "Поле для ввода сообщений";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(398, 63);
            label2.Name = "label2";
            label2.Size = new Size(108, 15);
            label2.TabIndex = 3;
            label2.Text = "История общения";
            // 
            // btnSend
            // 
            btnSend.Enabled = false;
            btnSend.Location = new Point(12, 455);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(380, 62);
            btnSend.TabIndex = 4;
            btnSend.Text = "Отправить";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += this.btnSend_Click;
            // 
            // btnDisconnect
            // 
            btnDisconnect.Location = new Point(398, 12);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Size = new Size(214, 29);
            btnDisconnect.TabIndex = 5;
            btnDisconnect.Text = "Отключиться";
            btnDisconnect.UseVisualStyleBackColor = true;
            btnDisconnect.Click += this.btnDisconnect_Click;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(623, 12);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(214, 29);
            btnConnect.TabIndex = 6;
            btnConnect.Text = "Подключиться";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += this.btnConnect_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 19);
            label3.Name = "label3";
            label3.Size = new Size(137, 15);
            label3.TabIndex = 7;
            label3.Text = "Состояние соединения:";
            // 
            // lbConnect
            // 
            lbConnect.AutoSize = true;
            lbConnect.Location = new Point(155, 19);
            lbConnect.Name = "lbConnect";
            lbConnect.Size = new Size(61, 15);
            lbConnect.TabIndex = 8;
            lbConnect.Text = "Diconnect";
            // 
            // BrainTester
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(849, 529);
            Controls.Add(lbConnect);
            Controls.Add(label3);
            Controls.Add(btnConnect);
            Controls.Add(btnDisconnect);
            Controls.Add(btnSend);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(tbEnterMessage);
            Controls.Add(tbHistory);
            Name = "BrainTester";
            Text = "Brain messager";
            Load += this.BrainTester_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox tbHistory;
        private RichTextBox tbEnterMessage;
        private Label label1;
        private Label label2;
        private Button btnSend;
        private Button btnDisconnect;
        private Button btnConnect;
        private Label label3;
        private Label lbConnect;
    }
}
