using System.ComponentModel.DataAnnotations;
using Application.Common.DTOs.Tags;

namespace Application.Common.DTOs.Books;

public class BookForUpdateDto
{
        public string Guid { get; set; }
        
        [MinLength(4, ErrorMessage = "The title is too short")]
        [MaxLength(200, ErrorMessage = "The title is too long")]
        public string Title { get; set; }

        [Range(0, int.MaxValue)]
        public int CurrentPage { get; set; }

        public string Language { get; set; }
        
        public string Creator { get; set; }
        
        public string Authors { get; set; }
        
        public string CreationDate { get; set; }
        
        public string LastOpened { get; set; }
        
        public string LastModified { get; set; }
        
        public string CoverLink { get; set; }

        public ICollection<TagInDto> Tags { get; set; } = new List<TagInDto>();
}