//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SmsService
{
    using System;
    using System.Collections.Generic;
    
    public partial class SmsTasksByPassPoint
    {
        public int SmsTaskId { get; set; }
        public int PassPointId { get; set; }
    
        public virtual SmsTask SmsTask { get; set; }
    }
}