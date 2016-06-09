using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RedAlert.API.Models
{
    public class SerialNumber
    {
        [Key]
        [MaxLength(40)]
        public string Code { get; set; }

        public bool Activated { get; set; }
    }
}