using System;
using System.Collections.Generic;
using System.Linq;

namespace sjdawson.TruckSimulatorPlugin.Sections
{
    public class JobStatus
    {
        private readonly TruckSimulatorPlugin Base;

        private bool HasSeenSpeedLimit, HasBeenCloseToDestination, HasNavigationDistanceZeroSet, HasNavigationDistanceJumped;
        private DateTime LatchStatusUntil = DateTime.Now, NavigationDistanceLastZeroedAt;
        private string CurrentJobIdentifier;

        private float NavigationDistancePreviousFrame;
        private List<float> NavigationDistancePreviousFrames = new List<float>() { 0 };

        private enum Status { None, Taken, Loading, Ongoing, Completed, Abandoned };
        private Status CurrentStatus;
        private readonly Status[] InProgressStatuses = { Status.Taken, Status.Loading, Status.Ongoing };

        public JobStatus(TruckSimulatorPlugin truckSimulatorPlugin)
        {
            Base = truckSimulatorPlugin;

            Base.AddProp("Job.Status", "");
            Base.AddProp("Job.InProgress", false);

            Base.AddAction("JobStatusReset", (a, b) => { JobStatusReset(); });

            Base.AddEvent("JobTaken");
            Base.AddEvent("JobLoading");
            Base.AddEvent("JobOngoing");
            Base.AddEvent("JobCompleted");
            Base.AddEvent("JobAbandoned");
            Base.AddEvent("JobReset");
        }

        public void DataUpdate()
        {
            JobStatusUpdate();
            CalculateNavigationDistanceFrames();

            Base.SetProp("Job.Status", CurrentStatus);
            Base.SetProp("Job.InProgress", InProgress());
        }

        /**
         * @deprecated: Use `SpecialEventsValues.OnJob` natively instead.
         */
        private bool InProgress()
        {
            if (Base.HasProp("SpecialEventsValues.OnJob"))
            {
                return Base.GetProp("SpecialEventsValues.OnJob");
            }

            return InProgressStatuses.Contains(CurrentStatus);
        }

        private void JobStatusUpdate()
        {
            var CurrentJob = String.Format("{0}__{1}__{2}__{3}__{4}",
                (string)Base.GetProp("JobValues.CargoValues.Id"),
                (string)Base.GetProp("JobValues.CompanySourceId"),
                (string)Base.GetProp("JobValues.CitySourceId"),
                (string)Base.GetProp("JobValues.CompanyDestinationId"),
                (string)Base.GetProp("JobValues.CityDestinationId")
            ).Replace(" ", "-").Replace("________", "").ToLower();

            if (CurrentJob == "")
            {
                CurrentStatus = Status.None;
                return;
            }

            var NavDistanceLeft = (float)Base.GetProp("NavigationValues.NavigationDistance");

            if (NavDistanceLeft == 0 && HasNavigationDistanceZeroSet == false)
            {
                NavigationDistanceLastZeroedAt = DateTime.Now.AddSeconds(2);
                HasNavigationDistanceZeroSet = true;
            }

            if (NavDistanceLeft > 0 && HasNavigationDistanceZeroSet == true)
            {
                NavigationDistanceLastZeroedAt = DateTime.Now.AddYears(1);
                HasNavigationDistanceZeroSet = false;
            }

            var NavEnded = HasNavigationDistanceZeroSet && DateTime.Now > NavigationDistanceLastZeroedAt;
            var SpeedLimit = (float)Base.GetProp("NavigationValues.SpeedLimit.Value");

            if (CurrentStatus == Status.None && CurrentJob != CurrentJobIdentifier && DateTime.Now > LatchStatusUntil)
            {
                CurrentStatus = Status.Taken;
                Base.TriggerEvent("JobTaken");

                LatchStatusUntil = DateTime.Now.AddYears(1);
                CurrentJobIdentifier = CurrentJob;
                HasSeenSpeedLimit = false;
                HasBeenCloseToDestination = false;

                return;
            }

            if (CurrentStatus == Status.Taken && NavEnded)
            {
                CurrentStatus = Status.Loading;
                Base.TriggerEvent("JobLoading");

                return;
            }

            if ((CurrentStatus == Status.Taken || CurrentStatus == Status.Loading) && NavDistanceLeft > 0)
            {
                CurrentStatus = Status.Ongoing;
                Base.TriggerEvent("JobOngoing");

                return;
            }

            if (CurrentStatus == Status.Ongoing)
            {
                if (SpeedLimit > 0 && HasSeenSpeedLimit == false)
                    HasSeenSpeedLimit = true;

                if (NavDistanceLeft < 30 && NavDistanceLeft > 0 && HasSeenSpeedLimit && HasBeenCloseToDestination == false)
                    HasBeenCloseToDestination = true;

                if (HasSeenSpeedLimit && HasBeenCloseToDestination)
                {
                    if ((CurrentJob != CurrentJobIdentifier)
                        || (CurrentJob == CurrentJobIdentifier && (NavEnded || HasNavigationDistanceJumped)))
                    {
                        CurrentStatus = Status.Completed;
                        LatchStatusUntil = DateTime.Now.AddSeconds(2);
                        Base.TriggerEvent("JobCompleted");

                        return;
                    }
                }
                else
                {
                    if (CurrentJob != CurrentJobIdentifier)
                    {
                        CurrentStatus = Status.Abandoned;
                        LatchStatusUntil = DateTime.Now.AddSeconds(2);
                        Base.TriggerEvent("JobAbandoned");

                        return;
                    }
                }
            }

            if ((CurrentStatus == Status.Completed || CurrentStatus == Status.Abandoned) && DateTime.Now > LatchStatusUntil)
            {
                CurrentStatus = Status.None;
                LatchStatusUntil = DateTime.Now.AddSeconds(2);

                HasSeenSpeedLimit = false;
                HasBeenCloseToDestination = false;

                return;
            }
        }

        private void CalculateNavigationDistanceFrames()
        {
            if (NavigationDistancePreviousFrames.Count >= 20)
            {
                NavigationDistancePreviousFrames.RemoveAt(0);
            }

            NavigationDistancePreviousFrames.Add(NavigationDistancePreviousFrame);
            NavigationDistancePreviousFrame = (float)Base.GetProp("Job.NavigationDistanceLeft");

            if (NavigationDistancePreviousFrames.FindAll(x => x == 0).Count < 5)
            {
                NavigationDistancePreviousFrames.RemoveAll(x => x == 0);
            }

            if (NavigationDistancePreviousFrames.Count > 0)
            {
                HasNavigationDistanceJumped = NavigationDistancePreviousFrames.Max() - NavigationDistancePreviousFrames.Min() > 300;
            }
        }

        /// <summary>
        /// Resets the values of the various JobStatus variables, so the plugin
        /// can recalculate the current job situation.
        /// </summary>
        private void JobStatusReset()
        {
            CurrentStatus = Status.None;
            CurrentJobIdentifier = "";
            HasSeenSpeedLimit = false;
            HasBeenCloseToDestination = false;
            HasNavigationDistanceZeroSet = false;
            LatchStatusUntil = DateTime.Now;
            NavigationDistanceLastZeroedAt = DateTime.Now;

            Base.TriggerEvent("JobReset");
        }
    }
}
