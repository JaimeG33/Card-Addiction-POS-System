using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Card_Addiction_POS_System.Functions.Models;

namespace Card_Addiction_POS_System.Forms.Controls
{
    /*
     SelectCardGameControl (UserControl)
     -----------------------------------
     Purpose:
     - A reusable WinForms UserControl that exposes a ComboBox pre-populated
       with supported card games (bound to SelectedCardGameLogic.AllGames).
     - Shows a placeholder item ("Please Select Card Game") when no real game is selected.
     - Designed so you can drag-and-drop the control onto any form and read/write
       the selected game through properties/events rather than inspecting the raw ComboBox.

     How to implement in a form (quick steps):
     1. Add the `SelectCardGameControl` to the form (Designer or code).
     2. Read the selection:
          int id = mySelectCardGameControl.SelectedCardGameId; // -1 = placeholder / none selected
          var game = mySelectCardGameControl.SelectedGame;    // null = none selected
     3. Subscribe to changes:
          mySelectCardGameControl.SelectedGameChanged += (s,e) => { //handle change  };
     4. Programmatically select:
          mySelectCardGameControl.SelectedCardGameId = 2;
        or
          mySelectCardGameControl.SetSelectedByDatabaseName("Magic");

     Notes:
     - The control exposes a typed SelectedGame and an int SelectedCardGameId for compatibility.
     - The placeholder occupies index 0 and has CardGameId == -1.
     - Designer-time initialization is guarded so the control can be dropped onto forms safely.
    */

    public partial class SelectCardGameControl : UserControl
    {
        /// <summary>
        /// Internal ComboBox item wrapper so we can prepend a placeholder while keeping
        /// ValueMember/DisplayMember semantics and still surface the typed SelectedGame.
        /// </summary>
        private sealed class ComboItem
        {
            public int CardGameId { get; }
            public string DisplayName { get; }
            public SelectedCardGameLogic? Game { get; }

            public ComboItem(int cardGameId, string displayName, SelectedCardGameLogic? game)
            {
                CardGameId = cardGameId;
                DisplayName = displayName ?? string.Empty;
                Game = game;
            }

            // Some designer code or debugging might ToString; keep it friendly.
            public override string ToString() => DisplayName;
        }

        /// <summary>
        /// Raised when the selected card game changes.
        /// Consumers can subscribe to be notified when the user picks a different game.
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [Description("Raised when the selected card game changes.")]
        public event EventHandler? SelectedGameChanged;

        // Constructor: performs standard designer initialization and then populates the combo.
        public SelectCardGameControl()
        {
            InitializeComponent();

            // Guard runtime initialization from running inside the Visual Studio designer.
            // LicenseManager.UsageMode == LicenseUsageMode.Designtime while the designer is creating
            // the control; skip InitializeCombo in that scenario.
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                InitializeCombo();
            }
        }

        // InitializeCombo: configures the inner ComboBox for data-binding to the shared model.
        // Prepends a placeholder item so the UI shows "Please Select Card Game" whenever nothing is chosen.
        private void InitializeCombo()
        {
            // Build a local list with a placeholder first.
            var items = new List<ComboItem>(SelectedCardGameLogic.AllGames.Count + 1)
            {
                // Placeholder item; CardGameId = -1 indicates "none selected".
                new ComboItem(-1, "Please Select Card Game", null)
            };

            // Append real games from the shared model.
            foreach (var g in SelectedCardGameLogic.AllGames)
            {
                items.Add(new ComboItem(g.CardGameId, g.DisplayName, g));
            }

            // Configure binding members to match ComboItem properties.
            comboBox1.DisplayMember = nameof(ComboItem.DisplayName);
            comboBox1.ValueMember = nameof(ComboItem.CardGameId);
            comboBox1.DataSource = items;

            // Default to placeholder
            comboBox1.SelectedIndex = 0;

            // Hook the change event to forward notifications through SelectedGameChanged
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        // Event handler: forwards the event to external subscribers
        private void ComboBox1_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SelectedGameChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Current selected CardGameId. Setting will attempt to select the matching item.
        /// Returns -1 when the placeholder (none) is selected.
        /// </summary>
        [Browsable(true)]
        [Category("Data")]
        [Description("Selected card game's CardGameId.")]
        public int SelectedCardGameId
        {
            get
            {
                if (comboBox1.SelectedItem is ComboItem item)
                {
                    return item.CardGameId;
                }

                // fallback: when no selection or in designer, return -1
                return -1;
            }
            set
            {
                // Ensure DataSource is initialized before trying to set.
                if (comboBox1.DataSource is not IEnumerable<ComboItem> items)
                {
                    // If DataSource isn't initialized (designer or early runtime), try to initialize now.
                    InitializeCombo();
                    items = comboBox1.DataSource as IEnumerable<ComboItem>;
                }

                if (value < 0)
                {
                    // Select placeholder (index 0)
                    comboBox1.SelectedIndex = 0;
                    return;
                }

                // Try to find the item with matching CardGameId and select it.
                var list = items?.ToList() ?? new List<ComboItem>();
                var index = list.FindIndex(ci => ci.CardGameId == value);

                if (index >= 0)
                {
                    comboBox1.SelectedIndex = index;
                }
                else
                {
                    // If not found, keep placeholder selected.
                    comboBox1.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Selected game object (null if none / placeholder).
        /// Prefer this typed object when you need both display and canonical names.
        /// </summary>
        [Browsable(false)]
        public SelectedCardGameLogic? SelectedGame
        {
            get
            {
                if (comboBox1.SelectedItem is ComboItem item)
                {
                    return item.Game;
                }

                return null;
            }
        }

        /// <summary>
        /// Programmatically set selection by database name (case-insensitive). Returns true if selection changed.
        /// Convenience wrapper around SelectedCardGameLogic.TryGetByDatabaseName.
        /// </summary>
        public bool SetSelectedByDatabaseName(string databaseName)
        {
            if (SelectedCardGameLogic.TryGetByDatabaseName(databaseName, out var game))
            {
                SelectedCardGameId = game!.CardGameId;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Programmatically set selection by display name (case-insensitive). Returns true if selection changed.
        /// Convenience wrapper around SelectedCardGameLogic.TryGetByDisplayName.
        /// </summary>
        public bool SetSelectedByDisplayName(string displayName)
        {
            if (SelectedCardGameLogic.TryGetByDisplayName(displayName, out var game))
            {
                SelectedCardGameId = game!.CardGameId;
                return true;
            }

            return false;
        }
    }

    // Extension helper to support FindIndex on List<ComboItem> when using LINQ.
    internal static class ListExtensions
    {
        public static int FindIndex<T>(this List<T> list, Predicate<T> match)
        {
            if (list is null) return -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (match(list[i])) return i;
            }
            return -1;
        }
    }
}
