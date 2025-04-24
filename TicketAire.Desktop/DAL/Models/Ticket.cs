using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketAire.Desktop.DAL.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RequesterName { get; set; } 
        public string TechnicianName { get; set; } 
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
