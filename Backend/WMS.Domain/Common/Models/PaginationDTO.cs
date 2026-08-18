using System.Collections.Generic;

namespace WMS.Domain.Common.Models
{
    public class PaginationDTO<T> where T : class
    {
        public int PageNumber { get; set; } = 0;
        public int ItemsPerPage { get; set; } = 0;
        public int TotalRecords { get; set; } = 0;
        public int TotalPages { get; set; } = 0;
        public int RecordFrom { get; set; } = 0;
        public int RecordTo { get; set; } = 0;
        public bool IsSort { get; set; } = false;
        public bool IsAsc { get; set; } = false;
        public string SortBy { get; set; } = string.Empty;
        public string? Search { get; set; } = string.Empty;
        public List<T> Records { get; set; } = [];
    }

    public class PaginationDTO<T, TObj> where T : class where TObj : class
    {
        public int PageNumber { get; set; } = 0;
        public int ItemsPerPage { get; set; } = 0;
        public int TotalRecords { get; set; } = 0;
        public int TotalPages { get; set; } = 0;
        public int RecordFrom { get; set; } = 0;
        public int RecordTo { get; set; } = 0;
        public bool IsSort { get; set; } = false;
        public bool IsAsc { get; set; } = false;
        public string SortBy { get; set; } = string.Empty;
        public string? Search { get; set; } = string.Empty;
        public TObj CustomFilters { get; set; } = null!;
        public List<T> Records { get; set; } = [];
    }
}
