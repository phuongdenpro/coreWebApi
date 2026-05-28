using coreWebApi.Models;

namespace coreWebApi.Dtos
{
    public class UserQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? Keyword { get; set; }
        public Gender? Gender { get; set; }
        public string? SortBy { get; set; } = "id";
        public string? SortOrder { get; set; } = "desc";

    }
}
