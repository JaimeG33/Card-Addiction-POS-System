using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Card_Addiction_POS_System.Forms.Theming
{
    internal class ThemeManager
    {
        public enum ThemeRole
        {
            Default,
            Background,
            Surface,
            Primary,
            Secondary,
            Accent,
            Text,
            MutedText,
            Input
        }

        public sealed record ThemePalette(
            Color Background,
            Color Surface,
            Color Primary,
            Color Secondary,
            Color Accent,
            Color Text,
            Color MutedText,
            Color InputBackground,
            Color InputText,
            Color Border
        );

        public static class Themes
        {
            public static readonly ThemePalette Light = new(
                Background: Color.FromArgb(245, 245, 245),
                Surface: Color.White,
                Primary: Color.FromArgb(33, 150, 243),
                Secondary: Color.FromArgb(96, 125, 139),
                Accent: Color.FromArgb(255, 193, 7),
                Text: Color.FromArgb(25, 25, 25),
                MutedText: Color.FromArgb(90, 90, 90),
                InputBackground: Color.White,
                InputText: Color.FromArgb(25, 25, 25),
                Border: Color.FromArgb(210, 210, 210)
            );

            public static readonly ThemePalette Dark = new(
                Background: Color.FromArgb(20, 20, 20),
                Surface: Color.FromArgb(30, 30, 30),
                Primary: Color.FromArgb(33, 150, 243),
                Secondary: Color.FromArgb(96, 125, 139),
                Accent: Color.FromArgb(255, 193, 7),
                Text: Color.FromArgb(235, 235, 235),
                MutedText: Color.FromArgb(170, 170, 170),
                InputBackground: Color.FromArgb(45, 45, 45),
                InputText: Color.FromArgb(235, 235, 235),
                Border: Color.FromArgb(70, 70, 70)
            );
        }

        public static class ThemeManagerInstance
        {
            public static event EventHandler? ThemeChanged;

            public static ThemePalette Current { get; private set; } = Themes.Light;

            public static void SetTheme(ThemePalette palette)
            {
                Current = palette;
                ThemeChanged?.Invoke(null, EventArgs.Empty);
            }

            public static void ApplyTheme(Control root)
            {
                ApplyThemeToControl(root);

                // Auto-theme new controls added later (helps with dynamic UI/UserControls)
                root.ControlAdded -= Root_ControlAdded;
                root.ControlAdded += Root_ControlAdded;

                foreach (Control child in root.Controls)
                    ApplyTheme(child);
            }

            private static void Root_ControlAdded(object? sender, ControlEventArgs e)
            {
                ApplyTheme(e.Control);
            }

            private static ThemeRole GetRole(Control c)
            {
                if (c.Tag is ThemeRole role)
                    return role;
                return ThemeRole.Default;
            }

            private static void ApplyThemeToControl(Control c)
            {
                var p = Current;
                var role = GetRole(c);

                // Defaults by control type (you can override per-control using Tag/ThemeRole)
                if (c is Form form)
                {
                    form.BackColor = p.Background;
                    form.ForeColor = p.Text;
                    return;
                }

                if (c is Panel or GroupBox or TableLayoutPanel or FlowLayoutPanel or TabPage)
                {
                    c.BackColor = role switch
                    {
                        ThemeRole.Background => p.Background,
                        ThemeRole.Surface => p.Surface,
                        ThemeRole.Primary => p.Primary,
                        ThemeRole.Secondary => p.Secondary,
                        ThemeRole.Accent => p.Accent,
                        _ => p.Background
                    };
                    c.ForeColor = p.Text;
                    return;
                }

                if (c is Label label)
                {
                    label.ForeColor = role switch
                    {
                        ThemeRole.MutedText => p.MutedText,
                        ThemeRole.Accent => p.Accent,
                        _ => p.Text
                    };
                    // Keep label backgrounds transparent-ish unless explicitly tagged
                    if (role is ThemeRole.Surface or ThemeRole.Primary or ThemeRole.Secondary or ThemeRole.Background)
                        label.BackColor = Color.Transparent;
                    return;
                }

                if (c is TextBoxBase tb)
                {
                    tb.BackColor = role == ThemeRole.Input ? p.InputBackground : p.InputBackground;
                    tb.ForeColor = p.InputText;
                    return;
                }

                if (c is ComboBox cb)
                {
                    cb.BackColor = p.InputBackground;
                    cb.ForeColor = p.InputText;
                    return;
                }

                if (c is Button btn)
                {
                    // Flat buttons theme consistently; if you prefer standard OS look, remove this section.
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = p.Border;
                    btn.FlatAppearance.BorderSize = 1;

                    (btn.BackColor, btn.ForeColor) = role switch
                    {
                        ThemeRole.Primary => (p.Primary, Color.White),
                        ThemeRole.Secondary => (p.Secondary, Color.White),
                        ThemeRole.Accent => (p.Accent, Color.Black),
                        _ => (p.Surface, p.Text)
                    };
                    return;
                }

                if (c is DataGridView dgv)
                {
                    dgv.BackgroundColor = p.Surface;
                    dgv.GridColor = p.Border;
                    dgv.DefaultCellStyle.BackColor = p.Surface;
                    dgv.DefaultCellStyle.ForeColor = p.Text;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = p.Background;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = p.Text;
                    dgv.EnableHeadersVisualStyles = false;
                    return;
                }

                // Fallback
                c.BackColor = role switch
                {
                    ThemeRole.Background => p.Background,
                    ThemeRole.Surface => p.Surface,
                    ThemeRole.Primary => p.Primary,
                    ThemeRole.Secondary => p.Secondary,
                    ThemeRole.Accent => p.Accent,
                    _ => c.BackColor // don't stomp unknown control types aggressively
                };

                if (role is ThemeRole.Text)
                    c.ForeColor = p.Text;
                else if (role is ThemeRole.MutedText)
                    c.ForeColor = p.MutedText;
            }
        }
    }
}
