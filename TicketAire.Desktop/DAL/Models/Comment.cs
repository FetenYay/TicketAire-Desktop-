namespace TicketAire.Desktop.DAL.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }   
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSolution { get; set; }
    }
}
