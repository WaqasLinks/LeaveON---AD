using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public interface ILeave_MetadataType
    {
        //[DisplayFormat(DataFormatString = "{0: MM/dd/yyyy h:mm:ss tt}")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0: MM/dd/yyyy h:mm:ss tt}")]
        
        
        [DisplayName("Start Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        System.DateTime StartDate { get; set; }
        [DisplayName("End Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        System.DateTime EndDate { get; set; }
        [DisplayName("Total Days")]
        Nullable<decimal> TotalDays { get; set; }

        [DisplayName("Line Manager 1")]
        string LineManager1Id { get; set; }
        [DisplayName("Line Manager 2")]
        string LineManager2Id { get; set; }

        [DisplayName("Leave Type")]
        LeaveType LeaveType { get; set; }

    }

        [MetadataType(typeof(ILeave_MetadataType))]
    public partial class Leave : ILeave_MetadataType
    {
        /* Id property has already existed in the mapped class */
    }


}
