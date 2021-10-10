
namespace AssignmentSErver.E2EECS.Client
{
    partial class FormTester
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.serverAddressBox = new System.Windows.Forms.TextBox();
            this.serverPortBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.connectButton = new System.Windows.Forms.Button();
            this.senderPayloadBox = new System.Windows.Forms.TextBox();
            this.receiverBox = new System.Windows.Forms.TextBox();
            this.sendPayloadButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // serverAddressBox
            // 
            this.serverAddressBox.Location = new System.Drawing.Point(59, 12);
            this.serverAddressBox.Name = "serverAddressBox";
            this.serverAddressBox.Size = new System.Drawing.Size(290, 21);
            this.serverAddressBox.TabIndex = 0;
            // 
            // serverPortBox
            // 
            this.serverPortBox.Location = new System.Drawing.Point(59, 39);
            this.serverPortBox.Name = "serverPortBox";
            this.serverPortBox.Size = new System.Drawing.Size(290, 21);
            this.serverPortBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Server";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "Port";
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(355, 12);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(101, 48);
            this.connectButton.TabIndex = 2;
            this.connectButton.Text = "CONNECT";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // senderPayloadBox
            // 
            this.senderPayloadBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.senderPayloadBox.Location = new System.Drawing.Point(13, 71);
            this.senderPayloadBox.Multiline = true;
            this.senderPayloadBox.Name = "senderPayloadBox";
            this.senderPayloadBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.senderPayloadBox.Size = new System.Drawing.Size(443, 171);
            this.senderPayloadBox.TabIndex = 3;
            // 
            // receiverBox
            // 
            this.receiverBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.receiverBox.Location = new System.Drawing.Point(13, 296);
            this.receiverBox.Multiline = true;
            this.receiverBox.Name = "receiverBox";
            this.receiverBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.receiverBox.Size = new System.Drawing.Size(443, 171);
            this.receiverBox.TabIndex = 3;
            // 
            // sendPayloadButton
            // 
            this.sendPayloadButton.Location = new System.Drawing.Point(12, 248);
            this.sendPayloadButton.Name = "sendPayloadButton";
            this.sendPayloadButton.Size = new System.Drawing.Size(444, 42);
            this.sendPayloadButton.TabIndex = 2;
            this.sendPayloadButton.Text = "▼";
            this.sendPayloadButton.UseVisualStyleBackColor = true;
            this.sendPayloadButton.Click += new System.EventHandler(this.sendPayloadButton_Click);
            // 
            // FormTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 479);
            this.Controls.Add(this.receiverBox);
            this.Controls.Add(this.senderPayloadBox);
            this.Controls.Add(this.sendPayloadButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.serverPortBox);
            this.Controls.Add(this.serverAddressBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormTester";
            this.Text = "TCP Socket Tester";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox serverAddressBox;
        private System.Windows.Forms.TextBox serverPortBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox senderPayloadBox;
        private System.Windows.Forms.TextBox receiverBox;
        private System.Windows.Forms.Button sendPayloadButton;
    }
}

