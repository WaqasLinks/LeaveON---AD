using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{



    public interface IAspNetCountry_Metadata
    {
        //[DisplayFormat(DataFormatString = "{0: MM/dd/yyyy h:mm:ss tt}")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0: MM/dd/yyyy h:mm:ss tt}")]
        
        [DisplayName("Date Created")]
        Nullable<System.DateTime> DateCreated { get; set; }
        [DisplayName("Date Modified")]
        Nullable<System.DateTime> DateModified { get; set; }
    }
    [MetadataType(typeof(IAspNetCountry_Metadata))]
    public partial class Country : IAspNetCountry_Metadata
    {
        /* Id property has already existed in the mapped class */
    }

 
}
