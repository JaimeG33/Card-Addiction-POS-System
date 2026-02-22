using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Functions.Models
{
    public sealed class AddNewSet
    {
        public int CardGameId { get; init; }       // categoryId
        public int SetId { get; init; }            // groupId
        public string SetName { get; init; } = "";
        public string? Abbreviation { get; init; }
        public DateTime? SetDate { get; init; }    // publishedOn (date portion)
        public bool IsSupplemental { get; init; }  // optional: can filter later
    }

    public sealed class TcgCsvGroupsResponse
    {
        public int TotalItems { get; init; }
        public bool Success { get; init; }
        public List<GroupResult> Results { get; init; } = new();
    }

    public sealed class GroupResult
    {
        public int GroupId { get; init; }
        public string Name { get; init; } = "";
        public string? Abbreviation { get; init; }
        public bool IsSupplemental { get; init; }
        public DateTime? PublishedOn { get; init; }
        public int CategoryId { get; init; }
    }
}
