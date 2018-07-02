namespace ConsoleApp4
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Appointment")]
    public partial class Appointment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Appointment()
        {
            SMSLog = new HashSet<SMSLog>();
        }

        [StringLength(255)]
        public string Id { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? Start { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? End { get; set; }

        public string json { get; set; }

        [StringLength(50)]
        public string md5 { get; set; }

        [StringLength(50)]
        public string customerName { get; set; }

        [StringLength(50)]
        public string customerEmailAddress { get; set; }

        [StringLength(20)]
        public string customerPhone { get; set; }

        public string customerNotes { get; set; }

        [StringLength(36)]
        public string serviceId { get; set; }

        [StringLength(255)]
        public string serviceName { get; set; }

        [StringLength(36)]
        public string staffMemberIds { get; set; }

        [StringLength(36)]
        public string customerId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SMSLog> SMSLog { get; set; }
    }
}
