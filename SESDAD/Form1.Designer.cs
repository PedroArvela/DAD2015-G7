namespace SESDAD
{
    partial class PuppetMasterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PuppetMasterForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.importConfigFileButton = new System.Windows.Forms.ToolStripButton();
            this.SubscriberActionButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.SubscribeButton = new System.Windows.Forms.ToolStripMenuItem();
            this.UnsubscribeButton = new System.Windows.Forms.ToolStripMenuItem();
            this.PublisherActionButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.PublishButton = new System.Windows.Forms.ToolStripMenuItem();
            this.SpecialActionButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.CrashButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FreezeButton = new System.Windows.Forms.ToolStripMenuItem();
            this.UnfreezeButton = new System.Windows.Forms.ToolStripMenuItem();
            this.LoadScriptButton = new System.Windows.Forms.ToolStripMenuItem();
            this.LogOptionsFile = new System.Windows.Forms.ToolStripMenuItem();
            this.FullLogButton = new System.Windows.Forms.ToolStripMenuItem();
            this.LightLogButton = new System.Windows.Forms.ToolStripMenuItem();
            this.OrderingPolicyButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FloodPolicyButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FifoPolicyButton = new System.Windows.Forms.ToolStripMenuItem();
            this.TotalOrderPolicyButton = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusBox = new System.Windows.Forms.TextBox();
            this.StatusUpdateButton = new System.Windows.Forms.Button();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importConfigFileButton,
            this.SubscriberActionButton,
            this.PublisherActionButton,
            this.SpecialActionButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1034, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // importConfigFileButton
            // 
            this.importConfigFileButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.importConfigFileButton.Image = ((System.Drawing.Image)(resources.GetObject("importConfigFileButton.Image")));
            this.importConfigFileButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.importConfigFileButton.Name = "importConfigFileButton";
            this.importConfigFileButton.Size = new System.Drawing.Size(145, 22);
            this.importConfigFileButton.Text = "Import Configuration File";
            this.importConfigFileButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.importConfigFileButton.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // SubscriberActionButton
            // 
            this.SubscriberActionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SubscriberActionButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SubscribeButton,
            this.UnsubscribeButton});
            this.SubscriberActionButton.Image = ((System.Drawing.Image)(resources.GetObject("SubscriberActionButton.Image")));
            this.SubscriberActionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SubscriberActionButton.Name = "SubscriberActionButton";
            this.SubscriberActionButton.Size = new System.Drawing.Size(84, 22);
            this.SubscriberActionButton.Text = "Subscriber...";
            // 
            // SubscribeButton
            // 
            this.SubscribeButton.Name = "SubscribeButton";
            this.SubscribeButton.Size = new System.Drawing.Size(139, 22);
            this.SubscribeButton.Text = "Subscribe";
            // 
            // UnsubscribeButton
            // 
            this.UnsubscribeButton.Name = "UnsubscribeButton";
            this.UnsubscribeButton.Size = new System.Drawing.Size(139, 22);
            this.UnsubscribeButton.Text = "Unsubscribe";
            // 
            // PublisherActionButton
            // 
            this.PublisherActionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.PublisherActionButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PublishButton});
            this.PublisherActionButton.Image = ((System.Drawing.Image)(resources.GetObject("PublisherActionButton.Image")));
            this.PublisherActionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PublisherActionButton.Name = "PublisherActionButton";
            this.PublisherActionButton.Size = new System.Drawing.Size(78, 22);
            this.PublisherActionButton.Text = "Publisher...";
            this.PublisherActionButton.Click += new System.EventHandler(this.toolStripDropDownButton1_Click);
            // 
            // PublishButton
            // 
            this.PublishButton.Name = "PublishButton";
            this.PublishButton.Size = new System.Drawing.Size(113, 22);
            this.PublishButton.Text = "Publish";
            // 
            // SpecialActionButton
            // 
            this.SpecialActionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SpecialActionButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CrashButton,
            this.FreezeButton,
            this.UnfreezeButton,
            this.LoadScriptButton,
            this.LogOptionsFile,
            this.OrderingPolicyButton});
            this.SpecialActionButton.Image = ((System.Drawing.Image)(resources.GetObject("SpecialActionButton.Image")));
            this.SpecialActionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SpecialActionButton.Name = "SpecialActionButton";
            this.SpecialActionButton.Size = new System.Drawing.Size(66, 22);
            this.SpecialActionButton.Text = "Sepcial...";
            this.SpecialActionButton.Click += new System.EventHandler(this.SpecialActionButton_Click);
            // 
            // CrashButton
            // 
            this.CrashButton.Name = "CrashButton";
            this.CrashButton.Size = new System.Drawing.Size(161, 22);
            this.CrashButton.Text = "Crash";
            // 
            // FreezeButton
            // 
            this.FreezeButton.Name = "FreezeButton";
            this.FreezeButton.Size = new System.Drawing.Size(161, 22);
            this.FreezeButton.Text = "Freeze";
            // 
            // UnfreezeButton
            // 
            this.UnfreezeButton.Name = "UnfreezeButton";
            this.UnfreezeButton.Size = new System.Drawing.Size(161, 22);
            this.UnfreezeButton.Text = "Unfreeze";
            // 
            // LoadScriptButton
            // 
            this.LoadScriptButton.Name = "LoadScriptButton";
            this.LoadScriptButton.Size = new System.Drawing.Size(161, 22);
            this.LoadScriptButton.Text = "Load Script file...";
            // 
            // LogOptionsFile
            // 
            this.LogOptionsFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FullLogButton,
            this.LightLogButton});
            this.LogOptionsFile.Name = "LogOptionsFile";
            this.LogOptionsFile.Size = new System.Drawing.Size(161, 22);
            this.LogOptionsFile.Text = "Log Options";
            // 
            // FullLogButton
            // 
            this.FullLogButton.Name = "FullLogButton";
            this.FullLogButton.Size = new System.Drawing.Size(101, 22);
            this.FullLogButton.Text = "Full";
            // 
            // LightLogButton
            // 
            this.LightLogButton.Name = "LightLogButton";
            this.LightLogButton.Size = new System.Drawing.Size(101, 22);
            this.LightLogButton.Text = "Light";
            // 
            // OrderingPolicyButton
            // 
            this.OrderingPolicyButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FloodPolicyButton,
            this.FifoPolicyButton,
            this.TotalOrderPolicyButton});
            this.OrderingPolicyButton.Name = "OrderingPolicyButton";
            this.OrderingPolicyButton.Size = new System.Drawing.Size(161, 22);
            this.OrderingPolicyButton.Text = "Ordering Policy";
            this.OrderingPolicyButton.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // FloodPolicyButton
            // 
            this.FloodPolicyButton.Name = "FloodPolicyButton";
            this.FloodPolicyButton.Size = new System.Drawing.Size(134, 22);
            this.FloodPolicyButton.Text = "Flood";
            // 
            // FifoPolicyButton
            // 
            this.FifoPolicyButton.Name = "FifoPolicyButton";
            this.FifoPolicyButton.Size = new System.Drawing.Size(134, 22);
            this.FifoPolicyButton.Text = "FIFO";
            // 
            // TotalOrderPolicyButton
            // 
            this.TotalOrderPolicyButton.Name = "TotalOrderPolicyButton";
            this.TotalOrderPolicyButton.Size = new System.Drawing.Size(134, 22);
            this.TotalOrderPolicyButton.Text = "Total Order";
            // 
            // StatusBox
            // 
            this.StatusBox.Location = new System.Drawing.Point(12, 28);
            this.StatusBox.Multiline = true;
            this.StatusBox.Name = "StatusBox";
            this.StatusBox.Size = new System.Drawing.Size(760, 446);
            this.StatusBox.TabIndex = 1;
            // 
            // StatusUpdateButton
            // 
            this.StatusUpdateButton.Location = new System.Drawing.Point(12, 480);
            this.StatusUpdateButton.Name = "StatusUpdateButton";
            this.StatusUpdateButton.Size = new System.Drawing.Size(760, 52);
            this.StatusUpdateButton.TabIndex = 2;
            this.StatusUpdateButton.Text = "Status";
            this.StatusUpdateButton.UseVisualStyleBackColor = true;
            // 
            // PuppetMasterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1034, 961);
            this.Controls.Add(this.StatusUpdateButton);
            this.Controls.Add(this.StatusBox);
            this.Controls.Add(this.toolStrip1);
            this.Name = "PuppetMasterForm";
            this.Text = "PuppetMaster";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton importConfigFileButton;
        private System.Windows.Forms.ToolStripDropDownButton SubscriberActionButton;
        private System.Windows.Forms.ToolStripMenuItem SubscribeButton;
        private System.Windows.Forms.ToolStripMenuItem UnsubscribeButton;
        private System.Windows.Forms.ToolStripDropDownButton PublisherActionButton;
        private System.Windows.Forms.ToolStripMenuItem PublishButton;
        private System.Windows.Forms.ToolStripDropDownButton SpecialActionButton;
        private System.Windows.Forms.ToolStripMenuItem CrashButton;
        private System.Windows.Forms.ToolStripMenuItem FreezeButton;
        private System.Windows.Forms.ToolStripMenuItem UnfreezeButton;
        private System.Windows.Forms.ToolStripMenuItem LoadScriptButton;
        private System.Windows.Forms.ToolStripMenuItem LogOptionsFile;
        private System.Windows.Forms.ToolStripMenuItem FullLogButton;
        private System.Windows.Forms.ToolStripMenuItem LightLogButton;
        private System.Windows.Forms.TextBox StatusBox;
        private System.Windows.Forms.Button StatusUpdateButton;
        private System.Windows.Forms.ToolStripMenuItem OrderingPolicyButton;
        private System.Windows.Forms.ToolStripMenuItem FloodPolicyButton;
        private System.Windows.Forms.ToolStripMenuItem FifoPolicyButton;
        private System.Windows.Forms.ToolStripMenuItem TotalOrderPolicyButton;
    }
}

