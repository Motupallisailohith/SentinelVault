using System.ComponentModel.DataAnnotations;

namespace DbBackup.WebApi.Models
{
    public class RestoreDto
    {
        [Required]
        public string ProfileId { get; set; } = string.Empty;

        [Required]
        public string SourceFileName { get; set; } = string.Empty;
    }
}
