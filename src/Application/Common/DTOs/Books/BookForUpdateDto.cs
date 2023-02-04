using System.ComponentModel.DataAnnotations;
using Application.Common.DTOs.Tags;

namespace Application.Common.DTOs.Books;

public class BookForUpdateDto
{
    [Required]
    public Guid Guid { get; set; }

    [MinLength(4, ErrorMessage = "The title is too short")]
    [MaxLength(200, ErrorMessage = "The title is too long")]
    public string Title { get; set; }

    [Range(0, int.MaxValue)]
    public int CurrentPage { get; set; }

    [MinLength(2, ErrorMessage = "The language is too short")]
    [MaxLength(60, ErrorMessage = "The language is too long")]
    public string Language { get; set; }

    [MinLength(2, ErrorMessage = "The creator is too short")]
    [MaxLength(200, ErrorMessage = "The creator is too long")]
    public string Creator { get; set; }

    [MinLength(2, ErrorMessage = "The authors are too short")]
    [MaxLength(400, ErrorMessage = "The authors are too long")]
    public string Authors { get; set; }

    [MinLength(4, ErrorMessage = "The creation date is too short")]
    [MaxLength(40, ErrorMessage = "The creation date is too long")]
    public string CreationDate { get; set; }

    [MinLength(4, ErrorMessage = "The last opened is too short")]
    [MaxLength(40, ErrorMessage = "The last opened is too long")]
    public string LastOpened { get; set; }

    [MinLength(4, ErrorMessage = "The last modified is too short")]
    [MaxLength(40, ErrorMessage = "The last modified is too long")]
    public string LastModified { get; set; }

    [MinLength(4, ErrorMessage = "The last modified is too short")]
    [MaxLength(40, ErrorMessage = "The last modified is too long")]
    public string CoverLink { get; set; }

    public ICollection<TagInDto> Tags { get; set; } = new List<TagInDto>();
}