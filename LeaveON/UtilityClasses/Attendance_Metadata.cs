using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveON.UtilityClasses
{
    public interface IAttendance_MetadataType
    {
        //[DisplayFormat(DataFormatString = "{0: MM/dd/yyyy h:mm:ss tt}")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0: MM/dd/yyyy h:mm:ss tt}")]
        
        
        //[DisplayName("Start Date")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        //System.DateTime StartDate { get; set; }
        //[DisplayName("End Date")]
      

    [DisplayName("Time In")]
    [DisplayFormat(DataFormatString = "{0:hh:mm tt}")]
    DateTime TimeIn { get; set; }

    [DisplayName("Time Out")]
    [DisplayFormat(DataFormatString = "{0:hh:mm tt}")]
    DateTime TimeOut { get; set; }
  }

        [MetadataType(typeof(IAttendance_MetadataType))]
    public partial class Attendance : IAttendance_MetadataType
    {
        /* Id property has already existed in the mapped class */
    }


}
