using LNF.Cache;
using LNF.CommonTools;
using LNF.Control;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using OnlineServices.Api.Billing;
using OnlineServices.Api.Scheduler;
using Scheduler.Models;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Scheduler
{
    public class ReservationManager
    {
        public HttpActionContext Context { get; }

        public ReservationManager(HttpActionContext context)
        {
            Context = context;
        }

        public async Task<Models.ReservationModel> Start(int reservationId, int clientId, string ip)
        {
            var rsv = DA.Scheduler.Reservation.Single(reservationId);

            var isInLab = KioskUtility.ClientInLab(rsv.Resource.ProcessTech.Lab.LabID, clientId, ip);

            ReservationState state = ReservationUtility.GetReservationState(rsv.ReservationID, clientId, isInLab);
            var startable = ReservationUtility.IsStartable(state);

            if (!startable)
                throw new InvalidOperationException(string.Format("ReservationID {0} cannot be started: {1}", reservationId, GetNotStartableMessage(state)));

            if (rsv != null)
            {
                await ReservationUtility.StartReservation(rsv, clientId, isInLab);
                return CreateReservationModel(rsv, clientId, ip);
            }
            else
            {
                throw new ArgumentException(string.Format("Cannot find record for ReservationID = {0}", reservationId), "reservationId");
            }
        }

        public async Task<bool> SaveHistory(int reservationId, ReservationHistoryCommand cmd)
        {
            double chargeMultiplier = 1.00 - (cmd.ForgivenPercentage / 100.0);

            using (var sc = new SchedulerClient())
            {
                var model = new ReservationHistoryUpdate()
                {
                    ClientID = cmd.ClientID,
                    ReservationID = reservationId,
                    AccountID = cmd.AccountID,
                    ChargeMultiplier = chargeMultiplier,
                    Notes = cmd.Notes,
                    EmailClient = cmd.EmailClient
                };

                bool result = await sc.UpdateHistory(model);

                return result;
            }
        }

        public async Task<bool> UpdateBilling(UpdateBillingCommand cmd)
        {
            bool isTemp = cmd.StartDate == DateTime.Now.FirstOfMonth();

            using (var bc = new BillingClient())
            {
                LNF.Models.Billing.Process.BillingProcessResult toolDataCleanResult = null;
                LNF.Models.Billing.Process.BillingProcessResult toolDataResult = null;
                LNF.Models.Billing.Process.BillingProcessResult toolStep1Result = null;
                LNF.Models.Billing.Process.BillingProcessResult roomDataCleanResult = null;
                LNF.Models.Billing.Process.BillingProcessResult roomDataResult = null;
                LNF.Models.Billing.Process.BillingProcessResult roomStep1Result = null;
                LNF.Models.Billing.Process.BillingProcessResult subsidyResult = null;

                // Tool
                toolDataCleanResult = await bc.BillingProcessDataClean(LNF.Models.Billing.BillingCategory.Tool, cmd.StartDate, cmd.EndDate, cmd.ClientID, 0);
                toolDataResult = await bc.BillingProcessData(LNF.Models.Billing.BillingCategory.Tool, cmd.StartDate, cmd.EndDate, cmd.ClientID, 0);
                toolStep1Result = await bc.BillingProcessStep1(LNF.Models.Billing.BillingCategory.Tool, cmd.StartDate, cmd.EndDate, cmd.ClientID, 0, isTemp, true);

                // Room
                roomDataCleanResult = await bc.BillingProcessDataClean(LNF.Models.Billing.BillingCategory.Room, cmd.StartDate, cmd.EndDate, cmd.ClientID, 0);
                roomDataResult = await bc.BillingProcessData(LNF.Models.Billing.BillingCategory.Room, cmd.StartDate, cmd.EndDate, cmd.ClientID, 0);
                roomStep1Result = await bc.BillingProcessStep1(LNF.Models.Billing.BillingCategory.Room, cmd.StartDate, cmd.EndDate, cmd.ClientID, 0, isTemp, true);

                // Subsidy
                if (!isTemp)
                    subsidyResult = await bc.BillingProcessStep4("subsidy", cmd.StartDate, cmd.ClientID);

                UpdateBillingResult updateResult = new UpdateBillingResult(toolDataCleanResult, toolDataResult, toolStep1Result, roomDataCleanResult, roomDataResult, roomStep1Result, subsidyResult);

                if (updateResult.HasError())
                    throw new Exception(updateResult.GetErrorMessage());

                return true;
            }
        }

        public Models.ReservationModel CreateReservationModel(Reservation rsv, int clientId, string ip)
        {
            var item = new Models.ReservationModel();
            item.ReservationID = rsv.ReservationID;
            item.ResourceID = rsv.Resource.ResourceID;
            item.ResourceName = rsv.Resource.ResourceName;
            item.AccountID = rsv.Account.AccountID;
            item.AccountName = rsv.Account.Name;
            item.ShortCode = rsv.Account.ShortCode;
            item.ReservedByClientID = rsv.Client.ClientID;
            item.ReservedByClientName = string.Format("{0} {1}", rsv.Client.FName, rsv.Client.LName);

            Client c;

            if (rsv.ClientIDBegin.HasValue)
            {
                if (rsv.ClientIDBegin.Value > 0)
                {
                    c = DA.Current.Single<Client>(rsv.ClientIDBegin.Value);
                    item.StartedByClientID = c.ClientID;
                    item.StartedByClientName = string.Format("{0} {1}", c.FName, c.LName);
                }
                else
                {
                    item.StartedByClientID = 0;
                    item.StartedByClientName = string.Empty;
                }
            }
            else
            {
                c = DA.Current.Single<Client>(clientId);
                item.StartedByClientID = clientId;
                item.StartedByClientName = string.Format("{0} {1}", c.FName, c.LName);
            }

            var isInLab = KioskUtility.ClientInLab(rsv.Resource.ProcessTech.Lab.LabID, clientId, ip);
            ReservationState state = ReservationUtility.GetReservationState(rsv.ReservationID, clientId, isInLab);
            item.Startable = ReservationUtility.IsStartable(state);
            item.NotStartableMessage = GetNotStartableMessage(state);

            var inst = ActionInstanceUtility.Find(ActionType.Interlock, rsv.Resource.ResourceID);
            item.HasInterlock = inst != null;

            item.ReturnUrl = GetResourceUrl(rsv.Resource.ResourceID);

            return item;
        }

        public string GetNotStartableMessage(ReservationState state)
        {
            switch (state)
            {
                case ReservationState.Undefined:
                    return "Reservation is not startable at this time.";
                case ReservationState.Editable:
                case ReservationState.PastSelf:
                case ReservationState.Other:
                case ReservationState.Invited:
                case ReservationState.PastOther:
                    return "Reservation has already ended.";
                case ReservationState.Endable:
                    return "Reservation is already in progress.";
                case ReservationState.Repair:
                    return "Resource offline for repair";
                case ReservationState.NotInLab:
                    return "You must be in the lab to start the reservation";
                case ReservationState.UnAuthToStart:
                    return "You are not authorized to start this reservation";
                case ReservationState.Meeting:
                    return "Reservation takes place during regular meeting time.";
                default:
                    return string.Empty;
            }
        }

        public string GetResourceUrl(int resourceId)
        {
            ResourceModel res = CacheManager.Current.GetResource(resourceId);

            string schedulerUrl = ConfigurationManager.AppSettings["SchedulerUrl"];

            if (string.IsNullOrEmpty(schedulerUrl))
                throw new InvalidOperationException("Missing appSetting: SchedulerUrl");

            return string.Format("{0}ResourceDayWeek.aspx?path={1}:{2}:{3}:{4}", schedulerUrl, res.BuildingID, res.LabID, res.ProcessTechID, res.ResourceID);
        }
    }
}