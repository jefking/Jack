using System;
using System.ComponentModel;

using Jack.Logger;

namespace Jack.Core.Threading
{
    /// <summary>
    /// Executor
    /// </summary>
    public class Executor : BackgroundWorker
    {
        #region Constants
        /// <summary>
        /// Error Message when no error is returned in Result
        /// </summary>
        private const string c_noDataReturnedInResultError = "No Data Returned in Result. (Avoid by using ErrorOnNullResult flag on DataLoader)";
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the background operation has completed, has been canceled, or
        /// has raised an exception.
        /// </summary>
        public new event RunWorkerCompletedEventHandler RunWorkerCompleted;
        /// <summary>
        /// Loading State Changed
        /// </summary>
        public event EventHandler LoadingStateChanged;
        /// <summary>
        /// Operation Errored Event
        /// </summary>
        public event EventHandler<Exception> OperationErrored;
        #endregion

        #region Members
        /// <summary>
        /// Argument
        /// </summary>
        private object m_argument = null;
        /// <summary>
        /// Loading State
        /// </summary>
        private LoadingState m_loadingState = LoadingState.NotLoaded;
        /// <summary>
        /// Error
        /// </summary>
        private Exception m_error = null;
        /// <summary>
        /// Error when there is no result returned from the Worker Call
        /// </summary>
        private bool m_errorOnNullResult = true;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Executor()
            : base()
        {
            using (var log = new TraceContext())
            {
                this.WorkerSupportsCancellation = true;

                base.RunWorkerCompleted += this.WorkerCompleted;
            }
        }
        /// <summary>
        /// Executor Constructor
        /// </summary>
        /// <param name="doWork">Do Work Event</param>
        /// <param name="workCompleted">Work Completed</param>
        public Executor(DoWorkEventHandler doWork
                , RunWorkerCompletedEventHandler workCompleted)
            : this()
        {
            using (var log = new TraceContext())
            {
                base.DoWork += doWork;
                base.RunWorkerCompleted += workCompleted;
            }
        }
        /// <summary>
        /// Deconstructor
        /// </summary>
        ~Executor()
        {
            using (var log = new TraceContext())
            {
                this.Dispose(false);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Fires Loading State Changed
        /// </summary>
        private void FireLoadingStateChanged()
        {
            using (var log = new TraceContext())
            {
                EventHandler handler = this.LoadingStateChanged;
                if (null != handler)
                {
                    handler(this
                        , EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Fires Operation Errored
        /// </summary>
        /// <param name="exception">Exception</param>
        private void FireOperationErrored(Exception exception)
        {
            using (var log = new TraceContext())
            {
                EventHandler<Exception> handler = this.OperationErrored;
                if (null != handler)
                {
                    handler(this
                        , new EventArguments<Exception>(exception));
                }
            }
        }
        /// <summary>
        /// Worker Completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkerCompleted(object sender
            , RunWorkerCompletedEventArgs e)
        {
            using (var log = new TraceContext())
            {
                if (!(this.CancelAttempted(e)))
                {
                    RunWorkerCompletedEventHandler handler = this.RunWorkerCompleted;
                    if (null != handler)
                    {
                        handler(sender
                            , e);
                    }
                }
            }
        }
        /// <summary>
        /// Do Work
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDoWork(DoWorkEventArgs e)
        {
            using (var log = new TraceContext())
            {
                this.m_argument = (null == e) ?
                    null :
                    e.Argument;
                this.LoadingState = LoadingState.Loading;
                this.LoadingError = null;
                try
                {
                    base.OnDoWork(e);
                }
                catch (Exception ex)
                {
                    log.Error(ex);

                    if (!(this.CancellationPending))
                    {
                        throw ex;
                    }
                }
                finally
                {
                    if (this.CancellationPending)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }
        /// <summary>
        /// On Progress Changed
        /// </summary>
        /// <param name="e">Progress Changed Event Args</param>
        protected override void OnProgressChanged(ProgressChangedEventArgs e)
        {
            using (var log = new TraceContext())
            {
                base.OnProgressChanged(e);
            }
        }
        /// <summary>
        /// On Run Worker Completed
        /// </summary>
        /// <param name="e">Run Worker Completed Event Args</param>
        protected override void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            using (var log = new TraceContext())
            {
                if (!(e.Cancelled))
                {
                    this.LoadingError = e.Error;

                    if (this.ErrorOnNullResult &&
                        null == this.LoadingError &&
                        null == e.Result)
                    {
                        this.LoadingError = new System.Data.DataException(c_noDataReturnedInResultError);
                    }
                }

                if (!(this.CancelAttempted(e)))
                {
                    base.OnRunWorkerCompleted(e);
                }
                if (e.Cancelled)
                {
                    this.LoadingState = LoadingState.NotLoaded;
                }
                else if (null != this.LoadingError)
                {
                    this.LoadingState = LoadingState.Failed;
                }
                else
                {
                    this.LoadingState = LoadingState.Loaded;
                }
            }
        }
        /// <summary>
        /// Cancel Attempted
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool CancelAttempted(RunWorkerCompletedEventArgs e)
        {
            using (var log = new TraceContext())
            {
                return e.Cancelled
                    || CancellationPending;
            }
        }
        /// <summary>
        /// Dispose Method
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected override void Dispose(bool disposing)
        {
            using (var log = new TraceContext())
            {
                if (disposing)
                {
                    //Members
                    this.m_argument = null;
                    this.m_error = null;

                    //Events
                    this.LoadingStateChanged = null;
                    this.OperationErrored = null;
                }
                base.Dispose(disposing);
            }
        }
        #endregion

        #region Static
        /// <summary>
        /// Runs Worker Async
        /// </summary>
        /// <param name="worker">Background Worker</param>
        public static void Run(BackgroundWorker worker)
        {
            using (var log = new TraceContext())
            {
                Executor.Run(worker
                    , null);
            }
        }
        /// <summary>
        /// Runs Worker Async
        /// </summary>
        /// <param name="worker">Background Worker</param>
        /// <param name="argument">Argument</param>
        public static void Run(BackgroundWorker worker
            , object argument)
        {
            using (var log = new TraceContext())
            {
                if (null == worker)
                {
                    throw new NullReferenceException("Background Worker is Null.");
                }

                if (worker.IsBusy)
                {
                    return;
                }

                if (null != argument)
                {
                    worker.RunWorkerAsync(argument);
                }
                else
                {
                    worker.RunWorkerAsync();
                }
            }
        }
        /// <summary>
        /// Cancels Background Worker
        /// </summary>
        /// <param name="worker">Background Worker</param>
        public static void Cancel(BackgroundWorker worker)
        {
            using (var log = new TraceContext())
            {
                Executor.Cancel(worker
                         , false);
            }
        }
        /// <summary>
        /// Cancels Background Worker
        /// </summary>
        /// <param name="worker">Background Worker</param>
        /// <param name="throwOnException"></param>
        public static void Cancel(BackgroundWorker worker
            , bool throwOnException)
        {
            using (var log = new TraceContext())
            {
                if (null != worker
                    && worker.WorkerSupportsCancellation
                    && worker.IsBusy
                    && !(worker.CancellationPending))
                {
                    try
                    {
                        worker.CancelAsync();
                    }
                    catch (System.InvalidOperationException ex)
                    {
                        log.Error(ex);
                        if (throwOnException)
                        {
                            throw ex;
                        }
                    }
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Argument that was passed to Do Work
        /// </summary>
        public object Argument
        {
            get
            {
                return this.m_argument;
            }
        }
        /// <summary>
        /// Is Loading
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return this.IsBusy ||
                    LoadingState.Loading == this.LoadingState;
            }
        }
        /// <summary>
        /// Error on null result set returned.
        /// </summary>
        public bool ErrorOnNullResult
        {
            get
            {
                return this.m_errorOnNullResult;
            }
            set
            {
                this.m_errorOnNullResult = value;
            }
        }
        /// <summary>
        /// Loading State
        /// </summary>
        public LoadingState LoadingState
        {
            get
            {
                return this.m_loadingState;
            }
            private set
            {
                if (this.m_loadingState != value)
                {
                    this.m_loadingState = value;
                    this.FireLoadingStateChanged();
                }
            }
        }
        /// <summary>
        /// Load Error
        /// </summary>
        public Exception LoadingError
        {
            get
            {
                return this.m_error;
            }
            private set
            {
                if (null == value)
                {
                    this.m_error = null;
                }
                else if (value != this.m_error)
                {
                    this.m_error = value;
                    this.FireOperationErrored(this.m_error);
                }
            }
        }
        #endregion
    }
}