//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VinEcom.Invoices.Web.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class VinID_Trading
    {
        public VinID_Trading()
        {
            this.Reconciles = new HashSet<Reconcile>();
        }
    
        public int VinID { get; set; }
        public string Description { get; set; }
        public string MerchantID { get; set; }
        public int CategoryID { get; set; }
        public string Unit { get; set; }
        public System.DateTime DateTime { get; set; }
        public int F_identity { get; set; }
        public byte StatusData { get; set; }
        public string Encryptkey { get; set; }
    
        public virtual ICollection<Reconcile> Reconciles { get; set; }
    }
}
