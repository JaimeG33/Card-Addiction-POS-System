using System;
using System.Drawing;
using System.Windows.Forms;

namespace Card_Addiction_POS_System.Forms.Controls
{
    public partial class CustomConfirm : Form
    {
        private Label _messageLabel = null!;
        private Button _leftButton = null!;
        private Button _rightButton = null!;

        public string MessageText
        {
            get => _messageLabel.Text;
            set => _messageLabel.Text = value ?? string.Empty;
        }

        public string LeftButtonText
        {
            get => _leftButton.Text;
            set => _leftButton.Text = value ?? string.Empty;
        }

        public string RightButtonText
        {
            get => _rightButton.Text;
            set => _rightButton.Text = value ?? string.Empty;
        }

        public CustomConfirm()
        {
            InitializeComponent();

            // Create controls here (designer file provides basic form wiring).
            _messageLabel = new Label
            {
                AutoSize = false,
                Location = new Point(12, 12),
                Size = new Size(this.ClientSize.Width - 24, 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _leftButton = new Button
            {
                DialogResult = DialogResult.Yes,
                Size = new Size(110, 30)
            };

            _rightButton = new Button
            {
                DialogResult = DialogResult.No,
                Size = new Size(110, 30)
            };

            // Add controls
            this.Controls.Add(_messageLabel);
            this.Controls.Add(_leftButton);
            this.Controls.Add(_rightButton);

            // Layout updated on load/resize
            this.Load += CustomConfirm_Load;
            this.Resize += CustomConfirm_Resize;

            // Wire click handlers to close with the assigned DialogResult
            _leftButton.Click += (s, e) => this.DialogResult = DialogResult.Yes;
            _rightButton.Click += (s, e) => this.DialogResult = DialogResult.No;

            // sensible defaults
            this.AcceptButton = _rightButton;
            this.CancelButton = _leftButton;
        }

        private void CustomConfirm_Load(object? sender, EventArgs e)
        {
            // Initial layout
            LayoutControls();
        }

        private void CustomConfirm_Resize(object? sender, EventArgs e)
        {
            LayoutControls();
        }

        private void LayoutControls()
        {
            // Keep a margin and position buttons bottom-right
            const int padding = 12;
            _messageLabel.Size = new Size(Math.Max(100, this.ClientSize.Width - 24), Math.Min(200, this.ClientSize.Height - 100));

            int btnTop = this.ClientSize.Height - padding - _rightButton.Height;
            _rightButton.Location = new Point(this.ClientSize.Width - padding - _rightButton.Width, btnTop);

            _leftButton.Location = new Point(_rightButton.Left - 8 - _leftButton.Width, btnTop);
        }

        /// <summary>
        /// Shows a two-button modal dialog with custom labels.
        /// Returns true when the left button is pressed, false otherwise.
        /// </summary>
        public static bool ShowTwoButton(IWin32Window? owner, string title, string message, string leftButtonText, string rightButtonText)
        {
            using var dlg = new CustomConfirm
            {
                Text = title ?? string.Empty,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(520, 160)
            };

            // Apply texts
            dlg.MessageText = message ?? string.Empty;
            dlg.LeftButtonText = leftButtonText ?? "Left";
            dlg.RightButtonText = rightButtonText ?? "Right";

            // Default button focus: right (continue)
            dlg.AcceptButton = dlg._rightButton;
            dlg.CancelButton = dlg._leftButton;

            var dr = (owner == null) ? dlg.ShowDialog() : dlg.ShowDialog(owner);
            return dr == DialogResult.Yes;
        }
    }
}
