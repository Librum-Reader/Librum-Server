using System.ComponentModel.DataAnnotations;

namespace Application.Common.DTOs.Books;

public class BookForUpdateDto
{
        [MinLength(4, ErrorMessage = "The title is too short")]
        [MaxLength(80, ErrorMessage = "The title is too long")]
        public string Title { get; set; }

        [Range(0, int.MaxValue)]
        public int CurrentPage { get; set; }

        public string Language { get; set; }
        
        public string Creator { get; set; }
        
        public string CreationDate { get; set; }
        
        public string LastOpened { get; set; }
        
        public string Cover { get; set; }
}