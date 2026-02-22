using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Card_Addiction_POS_System.Functions.Models
{
    /*
     SelectedCardGameLogic
     ---------------------
     Centralized mapping and model for supported card games.

     How to use:
     - This class provides the authoritative list of supported card games via the
       static property `AllGames`. It is suitable for data-binding directly to a
       ComboBox (or any list control). Each entry exposes:
         - CardGameId    : integer identifier used throughout the app
         - DatabaseName  : canonical database string used in DB lookups
         - TCGCSVname    : name used when interacting with CSV/TCG sources
         - DisplayName   : friendly UI string (used by the control)
     - Example for manual binding:
         comboBox.DataSource = SelectedCardGameLogic.AllGames;
         comboBox.DisplayMember = nameof(SelectedCardGameLogic.DisplayName);
         comboBox.ValueMember = nameof(SelectedCardGameLogic.CardGameId);
     - Prefer using the provided helper methods (TryGetById, TryGetByDatabaseName)
       when converting between names and ids to avoid duplicating mapping logic.

     Notes:
     - Add or update entries in the s_allGames array to support new games.
     - This class is intentionally immutable (init-only) so entries are stable.
    */

    /// <summary>
    /// Centralized mapping and model for supported card games.
    /// Designed for data-binding to combo boxes and programmatic lookups.
    /// </summary>
    public sealed class SelectedCardGameLogic
    {
        // Primary identifier used in the application for this game
        public int CardGameId { get; init; }

        // Canonical database name (used when constructing DB queries or keys)
        public string DatabaseName { get; init; } = string.Empty;

        // Name used for TCG CSV integrations (if different from DatabaseName)
        public string TCGCSVname { get; init; } = string.Empty;

        // Friendly text intended for display in UI lists
        public string DisplayName { get; init; } = string.Empty;

        // Private ctor to enforce mapping defined in s_allGames
        private SelectedCardGameLogic() { }

        // The authoritative list of supported games. Add or modify entries here.
        // This collection is read-only and safe for binding.
        private static readonly IReadOnlyList<SelectedCardGameLogic> s_allGames = new ReadOnlyCollection<SelectedCardGameLogic>(new[]
        {
            new SelectedCardGameLogic
            {
                    CardGameId = 0,
                    DatabaseName = "Misc",
                    TCGCSVname = "None",
                    DisplayName = "Misc"
            },
            new SelectedCardGameLogic
            {
                CardGameId = 1,
                DatabaseName = "Magic",
                TCGCSVname = "Magic",
                DisplayName = "Magic: The Gathering"
            },
            new SelectedCardGameLogic
            {
                CardGameId = 2,
                DatabaseName = "Yugioh",
                TCGCSVname = "YuGiOh",
                DisplayName = "Yu-Gi-Oh!"
            },
            new SelectedCardGameLogic
            {
                CardGameId = 3,
                DatabaseName = "Pokemon",
                TCGCSVname = "Pokemon",
                DisplayName = "Pokemon"
            },
            new SelectedCardGameLogic
            {
                CardGameId = 63,
                DatabaseName = "Digimon",
                TCGCSVname = "Digimon Card Game",
                DisplayName = "Digimon"
            },
            new SelectedCardGameLogic
            {
                CardGameId = 68,
                DatabaseName = "OnePiece",
                TCGCSVname = "One Piece Card Game",
                DisplayName = "One Piece"
            },
            new SelectedCardGameLogic
            {
                CardGameId = 71,
                DatabaseName = "Lorcana",
                TCGCSVname = "Lorcana TCG",
                DisplayName = "Lorcana"
            }
        });

        /// <summary>
        /// Read-only collection of all known card games for binding.
        /// Use this as the DataSource for a ComboBox or other list UI.
        /// </summary>
        public static IReadOnlyList<SelectedCardGameLogic> AllGames => s_allGames;

        /// <summary>
        /// Try to find a game by CardGameId.
        /// Returns true and outputs the matching item when found.
        /// </summary>
        public static bool TryGetById(int id, out SelectedCardGameLogic? game)
        {
            game = s_allGames.FirstOrDefault(g => g.CardGameId == id);
            return game is not null;
        }

        /// <summary>
        /// Try to find a game by DatabaseName (case-insensitive).
        /// Useful when you have a stored database name and need the mapped id/display name.
        /// </summary>
        public static bool TryGetByDatabaseName(string databaseName, out SelectedCardGameLogic? game)
        {
            game = s_allGames.FirstOrDefault(g => string.Equals(g.DatabaseName, databaseName, StringComparison.OrdinalIgnoreCase));
            return game is not null;
        }

        /// <summary>
        /// Try to find a game by DisplayName (case-insensitive).
        /// Useful for UI-driven lookups.
        /// </summary>
        public static bool TryGetByDisplayName(string displayName, out SelectedCardGameLogic? game)
        {
            game = s_allGames.FirstOrDefault(g => string.Equals(g.DisplayName, displayName, StringComparison.OrdinalIgnoreCase));
            return game is not null;
        }

        // ToString returns DisplayName so controls that call ToString on items show the friendly name.
        public override string ToString() => DisplayName;
    }
}
