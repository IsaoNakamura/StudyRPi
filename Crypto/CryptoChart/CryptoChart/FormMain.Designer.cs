namespace CryptoChart
{
    partial class FormMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.CurrentInfoGrid = new System.Windows.Forms.DataGridView();
            this.AttributeType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AttributeValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StopBoxerButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.PositionHistoryGrid = new System.Windows.Forms.DataGridView();
            this.PositionLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OrderLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProfitLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EntryLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ExitLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IndicatorChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CurrentInfoGrid)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PositionHistoryGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IndicatorChart)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Size = new System.Drawing.Size(847, 764);
            this.splitContainer1.SplitterDistance = 206;
            this.splitContainer1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.CurrentInfoGrid, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.StopBoxerButton, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 74.89712F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.10288F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(206, 764);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // CurrentInfoGrid
            // 
            this.CurrentInfoGrid.AllowUserToAddRows = false;
            this.CurrentInfoGrid.AllowUserToDeleteRows = false;
            this.CurrentInfoGrid.AllowUserToResizeColumns = false;
            this.CurrentInfoGrid.AllowUserToResizeRows = false;
            this.CurrentInfoGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.CurrentInfoGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.CurrentInfoGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.AttributeType,
            this.AttributeValue});
            this.CurrentInfoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentInfoGrid.Location = new System.Drawing.Point(3, 3);
            this.CurrentInfoGrid.MultiSelect = false;
            this.CurrentInfoGrid.Name = "CurrentInfoGrid";
            this.CurrentInfoGrid.ReadOnly = true;
            this.CurrentInfoGrid.RowTemplate.Height = 21;
            this.CurrentInfoGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.CurrentInfoGrid.Size = new System.Drawing.Size(200, 566);
            this.CurrentInfoGrid.TabIndex = 0;
            this.CurrentInfoGrid.TabStop = false;
            // 
            // AttributeType
            // 
            this.AttributeType.HeaderText = "属性";
            this.AttributeType.Name = "AttributeType";
            this.AttributeType.ReadOnly = true;
            this.AttributeType.Width = 54;
            // 
            // AttributeValue
            // 
            this.AttributeValue.HeaderText = "値";
            this.AttributeValue.Name = "AttributeValue";
            this.AttributeValue.ReadOnly = true;
            this.AttributeValue.Width = 42;
            // 
            // StopBoxerButton
            // 
            this.StopBoxerButton.Location = new System.Drawing.Point(3, 575);
            this.StopBoxerButton.Name = "StopBoxerButton";
            this.StopBoxerButton.Size = new System.Drawing.Size(75, 23);
            this.StopBoxerButton.TabIndex = 1;
            this.StopBoxerButton.Text = "stop";
            this.StopBoxerButton.UseVisualStyleBackColor = true;
            this.StopBoxerButton.Click += new System.EventHandler(this.StopBoxerButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.chart1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.PositionHistoryGrid, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.IndicatorChart, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(637, 764);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // chart1
            // 
            this.chart1.BackColor = System.Drawing.Color.LightGray;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(3, 3);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(631, 301);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            // 
            // PositionHistoryGrid
            // 
            this.PositionHistoryGrid.AllowUserToAddRows = false;
            this.PositionHistoryGrid.AllowUserToDeleteRows = false;
            this.PositionHistoryGrid.AllowUserToResizeColumns = false;
            this.PositionHistoryGrid.AllowUserToResizeRows = false;
            this.PositionHistoryGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.PositionHistoryGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PositionHistoryGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PositionLabel,
            this.OrderLabel,
            this.ProfitLabel,
            this.EntryLabel,
            this.ExitLabel});
            this.PositionHistoryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PositionHistoryGrid.Location = new System.Drawing.Point(3, 617);
            this.PositionHistoryGrid.MultiSelect = false;
            this.PositionHistoryGrid.Name = "PositionHistoryGrid";
            this.PositionHistoryGrid.ReadOnly = true;
            this.PositionHistoryGrid.RowTemplate.Height = 21;
            this.PositionHistoryGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.PositionHistoryGrid.Size = new System.Drawing.Size(631, 144);
            this.PositionHistoryGrid.TabIndex = 1;
            this.PositionHistoryGrid.TabStop = false;
            // 
            // PositionLabel
            // 
            this.PositionLabel.HeaderText = "POS";
            this.PositionLabel.Name = "PositionLabel";
            this.PositionLabel.ReadOnly = true;
            this.PositionLabel.Width = 52;
            // 
            // OrderLabel
            // 
            this.OrderLabel.HeaderText = "ORDER";
            this.OrderLabel.Name = "OrderLabel";
            this.OrderLabel.ReadOnly = true;
            this.OrderLabel.Width = 69;
            // 
            // ProfitLabel
            // 
            this.ProfitLabel.HeaderText = "PROFIT";
            this.ProfitLabel.Name = "ProfitLabel";
            this.ProfitLabel.ReadOnly = true;
            this.ProfitLabel.Width = 70;
            // 
            // EntryLabel
            // 
            this.EntryLabel.HeaderText = "ENTRY";
            this.EntryLabel.Name = "EntryLabel";
            this.EntryLabel.ReadOnly = true;
            this.EntryLabel.Width = 67;
            // 
            // ExitLabel
            // 
            this.ExitLabel.HeaderText = "EXIT";
            this.ExitLabel.Name = "ExitLabel";
            this.ExitLabel.ReadOnly = true;
            this.ExitLabel.Width = 54;
            // 
            // IndicatorChart
            // 
            chartArea2.Name = "ChartArea1";
            this.IndicatorChart.ChartAreas.Add(chartArea2);
            this.IndicatorChart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.Name = "Legend1";
            this.IndicatorChart.Legends.Add(legend2);
            this.IndicatorChart.Location = new System.Drawing.Point(3, 310);
            this.IndicatorChart.Name = "IndicatorChart";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.IndicatorChart.Series.Add(series2);
            this.IndicatorChart.Size = new System.Drawing.Size(631, 301);
            this.IndicatorChart.TabIndex = 2;
            this.IndicatorChart.Text = "chart2";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 764);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormMain";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CurrentInfoGrid)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PositionHistoryGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IndicatorChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.DataGridView CurrentInfoGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn AttributeType;
        private System.Windows.Forms.DataGridViewTextBoxColumn AttributeValue;
        private System.Windows.Forms.DataGridView PositionHistoryGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn PositionLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn OrderLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn EntryLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn ExitLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn ProfitLabel;
        private System.Windows.Forms.Button StopBoxerButton;
        private System.Windows.Forms.DataVisualization.Charting.Chart IndicatorChart;
    }
}

