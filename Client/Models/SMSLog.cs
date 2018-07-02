namespace ConsoleApp4
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SMSLog")]
    public partial class SMSLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? sentDate { get; set; }

        public int? messageId { get; set; }

        public string message { get; set; }

        [StringLength(255)]
        public string appointmentId { get; set; }

        [StringLength(20)]
        public string recipientPhone { get; set; }

        public int? repeatCount { get; set; }

        [StringLength(255)]
        public string serviceId { get; set; }

        public virtual Appointment Appointment { get; set; }
    }
}
