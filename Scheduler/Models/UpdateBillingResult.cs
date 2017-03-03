using LNF.Models.Billing.Process;
using System;
using System.Collections.Generic;

namespace Scheduler.Models
{
    public class UpdateBillingResult
    {
        public BillingProcessResult ToolDataClean { get; }
        public BillingProcessResult ToolData { get; }
        public BillingProcessResult ToolStep1 { get; }
        public BillingProcessResult RoomDataClean { get; }
        public BillingProcessResult RoomData { get; }
        public BillingProcessResult RoomStep1 { get; }
        public BillingProcessResult Subsidy { get; }

        public UpdateBillingResult(BillingProcessResult toolDataClean, BillingProcessResult toolData, BillingProcessResult toolStep1, BillingProcessResult roomDataClean, BillingProcessResult roomData, BillingProcessResult roomStep1, BillingProcessResult subsidy)
        {
            ToolDataClean = toolDataClean;
            ToolData = toolData;
            ToolStep1 = toolStep1;
            RoomDataClean = roomDataClean;
            RoomData = roomData;
            RoomStep1 = roomStep1;
            Subsidy = subsidy;
        }

        public bool HasError()
        {
            bool result = !ToolDataClean.Success
                 || !ToolData.Success
                 || !ToolStep1.Success
                 || !RoomDataClean.Success
                 || !RoomData.Success
                 || !RoomStep1.Success;

            if (Subsidy != null)
                result = result || !Subsidy.Success;

            return result;
        }

        public TimeSpan TotalTimeTaken()
        {
            double totalSeconds = ToolDataClean.TimeTaken
                + ToolData.TimeTaken
                + ToolStep1.TimeTaken
                + RoomDataClean.TimeTaken
                + RoomData.TimeTaken
                + RoomStep1.TimeTaken;

            if (Subsidy != null)
                totalSeconds += Subsidy.TimeTaken;

            TimeSpan result = TimeSpan.FromSeconds(totalSeconds);

            return result;
        }

        public string GetErrorMessage()
        {
            string result = "OK";

            List<string> errors = new List<string>();

            if (!ToolDataClean.Success)
                errors.Add(ToolDataClean.ErrorMessage);

            if (!ToolData.Success)
                errors.Add(ToolData.ErrorMessage);

            if (!ToolStep1.Success)
                errors.Add(ToolStep1.ErrorMessage);

            if (!RoomDataClean.Success)
                errors.Add(RoomDataClean.ErrorMessage);

            if (!RoomData.Success)
                errors.Add(RoomData.ErrorMessage);

            if (!RoomStep1.Success)
                errors.Add(RoomStep1.ErrorMessage);

            if (Subsidy != null)
            {
                if (!Subsidy.Success)
                    errors.Add(Subsidy.ErrorMessage);
            }

            if (errors.Count > 0)
                result = string.Join(", ", errors);

            return result;
        }
    }
}