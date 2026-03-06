using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Functions.Models;
using Microsoft.Data.SqlClient;
            using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Card_Addiction_POS_System.Functions.Sales
{
    /// <summary>
    /// Builds and prints a game-specific "pick list" from the in-memory cart.
    ///
    /// High-level flow:
    /// 1) Prompt for customer name.
    /// 2) Merge cart rows with DB inventory/set metadata.
    /// 3) Sort into buckets (case first, then normal sorted, then non-inventory).
    /// 4) Build printable text lines based on game-specific formatting rules.
    /// 5) Show Windows print dialog and print.
    /// </summary>
    internal sealed class PrintCart
    {
        // Factory used to open SQL connections for the currently signed-in user.
        private readonly SqlConnectionFactory _connectionFactory;

        // Delegate that supplies the current user's DB password when needed.
        private readonly Func<Task<string>> _getPasswordAsync;

        /// <summary>
        /// Constructor dependency injection.
        /// </summary>
        public PrintCart(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
        }

        /// <summary>
        /// Public entry point called by the form.
        /// Validates inputs, prompts for customer name, builds print content, then prints via dialog.
        /// </summary>
        public async Task PrintCartAsync(
            IWin32Window? owner,
            SelectedCardGameLogic selectedGame,
            IReadOnlyList<TransactionLineItem> cartLines)
        {
            if (selectedGame == null)
                throw new ArgumentNullException(nameof(selectedGame));

            if (cartLines == null || cartLines.Count == 0)
                throw new InvalidOperationException("Cart is empty.");

            var customerName = PromptForCustomerName(owner);
            if (string.IsNullOrWhiteSpace(customerName))
            {
                return;
            }

            // Keep continuation on UI thread (no ConfigureAwait(false) here).
            var printItems = await BuildPrintItemsAsync(selectedGame, cartLines);
            var title = $"{selectedGame.DisplayName} list for {customerName}";
            var lines = BuildPrintLines(selectedGame, title, printItems);

            PrintWithDialog(owner, title, lines);
        }

        /// <summary>
        /// Converts raw cart rows into enriched PrintItem rows.
        /// Uses cardId/setId to map each cart row to inventory/set metadata loaded from DB.
        /// </summary>
        private async Task<List<PrintItem>> BuildPrintItemsAsync(
            SelectedCardGameLogic selectedGame,
            IReadOnlyList<TransactionLineItem> cartLines)
        {
            // Pull metadata once, then do in-memory mapping per cart row.
            var inventoryLookup = await LoadInventorySetInfoAsync(selectedGame, cartLines).ConfigureAwait(false);

            var result = new List<PrintItem>(cartLines.Count);

            foreach (var line in cartLines)
            {
                // Lookup may fail if row no longer exists in DB; defaults are applied below.
                inventoryLookup.TryGetValue((line.CardId, line.SetId), out var dbInfo);

                result.Add(new PrintItem
                {
                    CardId = line.CardId,
                    SetId = line.SetId,
                    CardName = line.CardName ?? string.Empty,
                    Rarity = line.Rarity ?? string.Empty,
                    Quantity = line.AmtTraded,
                    AmtInCase = line.AmtInCase,

                    // Fields merged from DB inventory/set tables.
                    SetName = dbInfo?.SetName ?? string.Empty,
                    Abbreviation = dbInfo?.Abbreviation ?? string.Empty,
                    CardType = dbInfo?.CardType ?? string.Empty,
                    CardColor = dbInfo?.CardColor ?? string.Empty,
                    AmtInStock = dbInfo?.AmtInStock
                });
            }

            // Apply required output ordering rules before rendering text.
            return SortForPrint(selectedGame, result);
        }

        /// <summary>
        /// Minimal internal DTO for metadata fetched from DB join.
        /// Keeps DB-facing data separate from print-facing model.
        /// </summary>
        private sealed class InventorySetInfo
        {
            public string SetName { get; init; } = string.Empty;
            public string Abbreviation { get; init; } = string.Empty;
            public string CardType { get; init; } = string.Empty;
            public string CardColor { get; init; } = string.Empty;
            public int? AmtInStock { get; init; }
        }

        /// <summary>
        /// Loads inventory + set metadata for all cardIds in the cart.
        /// Join key between inventory and set tables is setId (plus cardGameId filter on set table).
        /// </summary>
        private async Task<Dictionary<(int CardId, int SetId), InventorySetInfo>> LoadInventorySetInfoAsync(
            SelectedCardGameLogic selectedGame,
            IReadOnlyList<TransactionLineItem> cartLines)
        {
            var output = new Dictionary<(int CardId, int SetId), InventorySetInfo>();

            if (cartLines.Count == 0)
            {
                return output;
            }

            // Resolve game-specific table names (e.g., MagicInventory + MagicSet).
            var inventoryTable = $"{selectedGame.DatabaseName}Inventory";
            var setTable = $"{selectedGame.DatabaseName}Set";

            // Open DB connection with current-user credentials.
            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync().ConfigureAwait(false);

            // Detect optional columns because schemas may differ by game table.
            var typeColumn = await FindExistingColumnAsync(conn, inventoryTable, "cardType", "type").ConfigureAwait(false);
            var colorColumn = await FindExistingColumnAsync(conn, inventoryTable, "color", "cardColor").ConfigureAwait(false);
            var abbreviationColumn = await FindExistingColumnAsync(conn, inventoryTable, "abbreviation").ConfigureAwait(false);

            // Query only the needed cardIds from the current cart.
            var cardIds = cartLines.Select(x => x.CardId).Distinct().ToList();
            var idParams = new List<string>(cardIds.Count);

            using var cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = selectedGame.CardGameId;

            // Build parameterized IN clause safely.
            for (int i = 0; i < cardIds.Count; i++)
            {
                var p = $"@cardId{i}";
                idParams.Add(p);
                cmd.Parameters.Add(p, SqlDbType.Int).Value = cardIds[i];
            }

            // If optional columns are missing, project NULL aliases so reader ordinals stay stable.
            var typeSelect = string.IsNullOrWhiteSpace(typeColumn)
                ? "CAST(NULL AS nvarchar(200)) AS cardType"
                : $"inv.[{typeColumn}] AS cardType";

            var colorSelect = string.IsNullOrWhiteSpace(colorColumn)
                ? "CAST(NULL AS nvarchar(200)) AS cardColor"
                : $"inv.[{colorColumn}] AS cardColor";

            var abbreviationSelect = string.IsNullOrWhiteSpace(abbreviationColumn)
                ? "CAST(NULL AS nvarchar(100)) AS abbreviation"
                : $"inv.[{abbreviationColumn}] AS abbreviation";

            // LEFT JOIN keeps inventory row even when set row is missing.
            cmd.CommandText = $@"
SELECT
    inv.cardId,
    inv.setId,
    {abbreviationSelect},
    s.setName,
    inv.amtInStock,
    {typeSelect},
    {colorSelect}
FROM dbo.[{inventoryTable}] AS inv
LEFT JOIN dbo.[{setTable}] AS s
    ON s.setId = inv.setId
   AND s.cardGameId = @cardGameId
WHERE inv.cardId IN ({string.Join(", ", idParams)});";

            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var cardId = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                var setId = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));

                var key = (cardId, setId);
                if (output.ContainsKey(key))
                {
                    continue;
                }

                output[key] = new InventorySetInfo
                {
                    Abbreviation = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    SetName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    AmtInStock = reader.IsDBNull(4) ? (int?)null : Convert.ToInt32(reader.GetValue(4)),
                    CardType = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    CardColor = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                };
            }

            return output;
        }

        /// <summary>
        /// Returns the first matching existing column name from candidates.
        /// Used to support schema variations between game inventory tables.
        /// </summary>
        private static async Task<string?> FindExistingColumnAsync(SqlConnection conn, string tableName, params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                const string sql = @"
SELECT TOP (1) c.name
FROM sys.columns c
INNER JOIN sys.tables t ON t.object_id = c.object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = 'dbo'
  AND t.name = @tableName
  AND c.name = @columnName;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@tableName", SqlDbType.NVarChar, 128).Value = tableName;
                cmd.Parameters.Add("@columnName", SqlDbType.NVarChar, 128).Value = candidate;

                var value = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (value != null && value != DBNull.Value)
                {
                    return Convert.ToString(value);
                }
            }

            return null;
        }

        /// <summary>
        /// Applies output sorting rules:
        /// - Bucket order: case first, normal next, non-inventory last.
        /// - Magic: sort by SetName then CardName.
        /// - Others: sort by CardName then SetName.
        /// </summary>
        private static List<PrintItem> SortForPrint(SelectedCardGameLogic game, List<PrintItem> items)
        {
            IOrderedEnumerable<PrintItem> ordered;

            if (string.Equals(game.DatabaseName, "Magic", StringComparison.OrdinalIgnoreCase))
            {
                ordered = items
                    .OrderBy(GetPrintBucket)
                    .ThenBy(x => x.SetName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.CardName ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                ordered = items
                    .OrderBy(GetPrintBucket)
                    .ThenBy(x => x.CardName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.SetName ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            }

            return ordered.ToList();
        }

        // 0 = case first, 1 = sorted normal, 2 = non inventory last
        private static int GetPrintBucket(PrintItem item)
        {
            if (item.IsNotInventory) return 2;
            if (item.IsCaseCard) return 0;
            return 1;
        }

        /// <summary>
        /// Converts sorted items into plain text lines for the printer.
        /// Splits output into three labeled sections (buckets).
        /// </summary>
        private static List<string> BuildPrintLines(SelectedCardGameLogic game, string title, List<PrintItem> items)
        {
            var lines = new List<string>
            {
                title,
                $"Printed: {DateTime.Now:g}",
                string.Empty
            };

            AddBucket(lines, "CASE CARDS", game, items.Where(x => GetPrintBucket(x) == 0).ToList());
            AddBucket(lines, "BACK INVENTORY", game, items.Where(x => GetPrintBucket(x) == 1).ToList());
            AddBucket(lines, "NON INVENTORY", game, items.Where(x => GetPrintBucket(x) == 2).ToList());

            return lines;
        }

        /// <summary>
        /// Appends one section header and all item lines for that section.
        /// Skips empty sections.
        /// </summary>
        private static void AddBucket(List<string> lines, string header, SelectedCardGameLogic game, List<PrintItem> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            lines.Add(header);
            lines.Add(new string('-', 80));

            foreach (var item in items)
            {
                lines.Add(FormatItemLine(game, item));
            }

            lines.Add(string.Empty);
        }

        /// <summary>
        /// Produces one printable line in game-specific format.
        /// Rules mirror requested output fields by game.
        /// </summary>
        private static string FormatItemLine(SelectedCardGameLogic game, PrintItem item)
        {
            var gameKey = game.DatabaseName ?? string.Empty;

            if (string.Equals(gameKey, "Magic", StringComparison.OrdinalIgnoreCase))
            {
                return $"{item.Quantity,3}x  {item.CardName}  | Set: {item.SetName}  | Case: {item.AmtInCase}";
            }

            if (string.Equals(gameKey, "Yugioh", StringComparison.OrdinalIgnoreCase))
            {
                var setCode = string.IsNullOrWhiteSpace(item.Abbreviation) ? item.SetName : item.Abbreviation;
                var typeText = string.IsNullOrWhiteSpace(item.CardType) ? string.Empty : $" | Type: {item.CardType}";
                return $"{item.Quantity,3}x  {item.CardName} | {item.Rarity} | {setCode} | Case: {item.AmtInCase}{typeText}";
            }

            if (string.Equals(gameKey, "OnePiece", StringComparison.OrdinalIgnoreCase))
            {
                var abbrev = string.IsNullOrWhiteSpace(item.Abbreviation) ? item.SetName : item.Abbreviation;
                return $"{item.Quantity,3}x  {item.CardName} | {abbrev} | Case: {item.AmtInCase}";
            }

            if (string.Equals(gameKey, "Digimon", StringComparison.OrdinalIgnoreCase))
            {
                var abbrev = string.IsNullOrWhiteSpace(item.Abbreviation) ? item.SetName : item.Abbreviation;
                var caseText = item.AmtInCase <= 0 ? string.Empty : $" | Case: {item.AmtInCase}";
                var colorText = string.IsNullOrWhiteSpace(item.CardColor) ? string.Empty : $" | Color: {item.CardColor}";
                return $"{item.Quantity,3}x  {item.CardName} | {abbrev}{caseText}{colorText}";
            }

            if (string.Equals(gameKey, "Lorcana", StringComparison.OrdinalIgnoreCase))
            {
                var colorText = string.IsNullOrWhiteSpace(item.CardColor) ? string.Empty : $" | Color: {item.CardColor}";
                return $"{item.Quantity,3}x  {item.CardName} | Set: {item.SetName} | Case: {item.AmtInCase}{colorText}";
            }

            // Default format for any other game not explicitly customized above.
            return $"{item.Quantity,3}x  {item.CardName} | Set: {item.SetName} | Case: {item.AmtInCase}";
        }

        /// <summary>
        /// Uses System.Drawing.Printing with the standard Windows print dialog.
        /// Handles pagination by tracking a shared line index across PrintPage events.
        /// </summary>
        private static void PrintWithDialog(IWin32Window? owner, string documentName, IReadOnlyList<string> lines)
        {
            using var printDoc = new PrintDocument();
            printDoc.DocumentName = documentName;

            // Tracks where we are across multiple printed pages.
            var lineIndex = 0;
            var printFont = new Font("Consolas", 10f);

            printDoc.PrintPage += (_, e) =>
            {
                float y = e.MarginBounds.Top;
                var left = e.MarginBounds.Left;
                var lineHeight = printFont.GetHeight(e.Graphics) + 2f;

                while (lineIndex < lines.Count && y + lineHeight <= e.MarginBounds.Bottom)
                {
                    e.Graphics.DrawString(lines[lineIndex], printFont, Brushes.Black, left, y);
                    lineIndex++;
                    y += lineHeight;
                }

                // Continue printing if there are remaining lines.
                e.HasMorePages = lineIndex < lines.Count;
            };

            using var printDialog = new PrintDialog
            {
                UseEXDialog = true,
                Document = printDoc
            };

            var result = owner == null ? printDialog.ShowDialog() : printDialog.ShowDialog(owner);
            if (result == DialogResult.OK)
            {
                printDoc.Print();
            }

            printFont.Dispose();
        }

        /// <summary>
        /// Lightweight modal dialog for entering customer name.
        /// Returns null on cancel or blank input.
        /// </summary>
        private static string? PromptForCustomerName(IWin32Window? owner)
        {
            using var dlg = new Form
            {
                Text = "Customer Name",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(420, 130)
            };

            var lbl = new Label
            {
                Text = "Enter customer name for this print list:",
                AutoSize = true,
                Left = 12,
                Top = 14
            };

            var txt = new TextBox
            {
                Left = 12,
                Top = 40,
                Width = 392
            };

            var btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Left = 248,
                Top = 78,
                Width = 75
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Left = 329,
                Top = 78,
                Width = 75
            };

            dlg.Controls.Add(lbl);
            dlg.Controls.Add(txt);
            dlg.Controls.Add(btnOk);
            dlg.Controls.Add(btnCancel);

            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            var result = owner == null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
            if (result != DialogResult.OK)
            {
                return null;
            }

            var value = txt.Text?.Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
