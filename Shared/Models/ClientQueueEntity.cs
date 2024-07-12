using System;
using System.ComponentModel.DataAnnotations;


namespace Shared.Models
{
    public class ClientQueueEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CorrelationId { get; set; }
        public DateTime Created { get; set; }
        public DateTime StatusDate { get; set; }
        public QueueStatus QueueStatus { get; set; }
        public string TypeName { get; set; }
        public string Content { get; set; }
    }
}
