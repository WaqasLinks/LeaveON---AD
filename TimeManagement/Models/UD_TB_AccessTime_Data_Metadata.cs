using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeManagement.Models
{
    public interface IUD_TB_AccessTime_Data_Metadata
    {
        //[DisplayFormat(DataFormatString = "{0: MM/dd/yyyy h:mm:ss tt}")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0: MM/dd/yyyy h:mm:ss tt}")]


        //[DisplayName("Start Date")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        //System.DateTime StartDate { get; set; }


        [DisplayName("Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        Nullable<System.DateTime> Date_IN { get; set; }

        [DisplayName("Time_In")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm:ss tt}")]
        Nullable<System.DateTime> Time_IN { get; set; }

        [DisplayName("Time_Out")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm:ss tt}")]
        Nullable<System.DateTime> Time_OUT { get; set; }

    }

    [MetadataType(typeof(IUD_TB_AccessTime_Data_Metadata))]
    public partial class UD_TB_AccessTime_Data : IUD_TB_AccessTime_Data_Metadata
    {
        /* Id property has already existed in the mapped class */
        [NotMapped]
        public string Status { get; set; }
    }


}
