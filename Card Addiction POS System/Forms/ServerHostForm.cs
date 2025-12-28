using System.Net;
using System.Windows.Forms;
using Card_Addiction_POS_System.Data.Settings;

namespace Card_Addiction_POS_System
{
    /// <summary>
    /// Small dialog to set/replace the SQL Server host (IPv4/hostname).
    /// </summary>
    public sealed class ServerHostForm : Form
    {
        private readonly TextBox _tbHost = new() { Width = 260 };
        private readonly NumericUpDown _nudPort = new() { Minimum = 1, Maximum = 65535, Width = 100, Value = 1433 };
        private readonly TextBox _tbDb = new() { Width = 260 };

        private readonly Button _btnOk = new() { Text = "OK", Width = 90 };
        private readonly Button _btnCancel = new() { Text = "Cancel", Width = 90 };

        public AppSettings Result { get; private set; }

        public ServerHostForm(AppSettings current)
        {
            Result = current;

            Text = "Database Server Settings";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 520;
            Height = 240;

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;

            _tbHost.Text = current.ServerHost;
            _nudPort.Value = current.Port;
            _tbDb.Text = current.Database;

            BuildLayout();
            WireEvents();
        }

        private void BuildLayout()
        {
            var lblHost = new Label { Text = "SQL Server host / IPv4:", AutoSize = true };
            var lblPort = new Label { Text = "Port:", AutoSize = true };
            var lblDb = new Label { Text = "Database:", AutoSize = true };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(12),
            };

            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            grid.Controls.Add(lblHost, 0, 0);
            grid.Controls.Add(_tbHost, 1, 0);

            var portPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            portPanel.Controls.Add(lblPort);
            portPanel.Controls.Add(_nudPort);
            grid.Controls.Add(portPanel, 1, 1);

            grid.Controls.Add(lblDb, 0, 2);
            grid.Controls.Add(_tbDb, 1, 2);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };
            buttons.Controls.Add(_btnOk);
            buttons.Controls.Add(_btnCancel);

            grid.Controls.Add(buttons, 1, 3);

            Controls.Add(grid);
        }

        private void WireEvents()
        {
            _btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

            _btnOk.Click += (_, _) =>
            {
                var host = _tbHost.Text.Trim();
                var db = _tbDb.Text.Trim();
                var port = (int)_nudPort.Value;

                if (!IsValidHost(host))
                {
                    MessageBox.Show(this, "Enter a valid IPv4/IPv6 address or hostname.", "Invalid Host",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(db))
                {
                    MessageBox.Show(this, "Database name is required.", "Invalid Database",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Result = Result with { ServerHost = host, Port = port, Database = db };
                DialogResult = DialogResult.OK;
            };
        }

        private static bool IsValidHost(string host)
        {
            if (host.Length == 0) return false;
            if (IPAddress.TryParse(host, out _)) return true;

            if (host.Length > 253) return false;
            if (host.Contains(' ')) return false;
            return true;
        }
    }
}