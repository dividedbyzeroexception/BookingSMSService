//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Client
{
    using System;
    using System.Collections.Generic;
    
    public partial class Appointment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Appointment()
        {
            this.SMSLog = new HashSet<SMSLog>();
        }
    
        public string Id { get; set; }
        public Nullable<System.DateTime> Start { get; set; }
        public Nullable<System.DateTime> End { get; set; }
        public string json { get; set; }
        public string md5 { get; set; }
        public string customerName { get; set; }
        public string customerEmailAddress { get; set; }
        public string customerPhone { get; set; }
        public string customerNotes { get; set; }
        public string serviceId { get; set; }
        public string serviceName { get; set; }
        public string staffMemberIds { get; set; }
        public string customerId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SMSLog> SMSLog { get; set; }
    }
}