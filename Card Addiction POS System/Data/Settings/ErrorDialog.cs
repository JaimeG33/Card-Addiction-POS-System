using System;
using System.Drawing;
using System.Windows.Forms;

public sealed class ErrorDialog : Form
{
    private readonly TextBox _text;

    public ErrorDialog(string title, string details)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        Width = 900;
        Height = 600;
        MinimumSize = new Size(700, 450);

        _text = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Dock = DockStyle.Fill,
            Font = new Font(FontFamily.GenericMonospace, 9f),
            Text = details
        };

        var btnCopy = new Button
        {
            Text = "Copy",
            AutoSize = true,
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom
        };
        btnCopy.Click += (_, __) =>
        {
            try { Clipboard.SetText(details); } catch { /* ignore */ }
        };

        var btnClose = new Button
        {
            Text = "Close",
            AutoSize = true,
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            DialogResult = DialogResult.OK
        };

        var bottomPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(10),
            Height = 50
        };
        bottomPanel.Controls.Add(btnClose);
        bottomPanel.Controls.Add(btnCopy);

        Controls.Add(_text);
        Controls.Add(bottomPanel);

        AcceptButton = btnClose;
    }

    public static void Show(IWin32Window? owner, string title, string details)
    {
        using var dlg = new ErrorDialog(title, details);
        if (owner is null) dlg.ShowDialog();
        else dlg.ShowDialog(owner);
    }
}