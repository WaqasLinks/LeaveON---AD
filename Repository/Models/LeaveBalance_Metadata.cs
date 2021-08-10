using LMS.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public interface ILeaveBalance_MetadataType
    {

        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        //System.DateTime StartDate { get; set; }

        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        //System.DateTime EndDate { get; set; }
    }

    [MetadataType(typeof(ILeaveBalance_MetadataType))]
    public partial class LeaveBalance : ILeaveBalance_MetadataType
    {
        public LeaveBalance()
        {
        }
        public LeaveBalance(ref Leave leave)
        {
         
            if (leave.IsQuotaRequest == false)
            {
                int leaveTypeId = leave.LeaveTypeId;
                decimal UserLeavePolicyId = leave.AspNetUser.UserLeavePolicyId.Value;
                //LeaveBalance leaveBalance= leave.AspNetUser.LeaveBalances.FirstOrDefault(x => x.LeaveTypeId == LeaveTypeId && x.UserId == UserId);
                UserLeavePolicyDetail userLeavePolicyDetail = leave.AspNetUser.UserLeavePolicy.UserLeavePolicyDetails.FirstOrDefault(x => x.UserLeavePolicyId == UserLeavePolicyId && x.LeaveTypeId == leaveTypeId);
                if (userLeavePolicyDetail==null)
                {
                    Balance = 0;
                }
                else
                {
                    Balance = userLeavePolicyDetail.Allowed;
                }
                Taken = 0;
                Description = string.Empty;
            }
            else
            { //create leave balnace for CompensatoryLeave
                //decimal UserLeavePolicyId = leave.AspNetUser.UserLeavePolicyId.Value;
                //LeaveBalance leaveBalance= leave.AspNetUser.LeaveBalances.FirstOrDefault(x => x.LeaveTypeId == LeaveTypeId && x.UserId == UserId);
                //UserLeavePolicyDetail userLeavePolicyDetail = leave.LeaveType.UserLeavePolicyDetails.FirstOrDefault(x => x.LeaveTypeId == LeaveTypeId);
                //Balance = leave.TotalDays;//userLeavePolicyDetail.Allowed;
                
                Balance = leave.TotalDays;
                Taken = 0;
                Description = string.Empty;

            }
        }
        
    }


}
