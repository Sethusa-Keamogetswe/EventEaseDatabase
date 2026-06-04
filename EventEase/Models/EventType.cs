using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeID {  get; set; }

        [Required]
        public string? Name { get; set; }

        public ICollection<Event>? Events { get; set; }
    }
}
