// from https://stackoverflow.com/questions/10105518/calling-shgetfileinfo-in-thread-to-avoid-ui-freeze

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NeeView.Threading
{
    /// <summary>
    /// for STA
    /// </summary>
    public static class SingleThreadedApartment
    {
        private static StaTaskSchedulerSource _source = new StaTaskSchedulerSource();

        public static TaskScheduler TaskScheduler => _source.TaskScheduler;
    }

    /// <summary>
    /// Exposes a <see cref="TaskScheduler"/> that schedules its work on a STA background thread.
    /// </summary>
    public class StaTaskSchedulerSource
    {
        /// <summary>
        /// A window that is used for message pumping.
        /// </summary>
        private Window window;

        /// <summary>
        /// Thread on which work is scheduled.
        /// </summary>
        private Thread thread;

        /// <summary>
        /// The <see cref="TaskScheduler"/> exposed by this class.
        /// </summary>
        private TaskScheduler taskScheduler;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaTaskSchedulerSource"/> class.
        /// </summary>
        public StaTaskSchedulerSource()
        {
            using (ManualResetEvent re = new ManualResetEvent(false))
            {
                this.thread = new Thread(
                    () =>
                    {
                        this.window = new Window();

                        re.Set();

                        Dispatcher.Run();
                    });

                this.thread.IsBackground = true;
                this.thread.SetApartmentState(ApartmentState.STA);

                this.thread.Start();

                re.WaitOne();
            }

            this.window.Dispatcher.Invoke(
                new Action(
                    () =>
                    {
                        this.taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                    }));
        }

        /// <summary>
        /// Gets a <see cref="TaskScheduler"/> that schedules work on a background STA
        /// thread.
        /// </summary>
        public TaskScheduler TaskScheduler
        {
            get
            {
                return this.taskScheduler;
            }
        }
    }
}
