using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using log4net;

namespace ThreeByte.TaskModel
{

    public abstract class AsyncTaskBase : IAsyncTask
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AsyncTaskBase));

        public event PropertyChangedEventHandler PropertyChanged;

        //Accesible by derived classes
        protected void NotifyPropertyChanged(string property) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }


        private BackgroundWorker _bgWorker;

        #region Properties
        private string _name;
        public string Name {
            get {
                return _name;
            }
            protected set {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private string _status;
        public string Status {
            get {
                return _status;
            }
            protected set {
                _status = value;
                NotifyPropertyChanged("Status");
            }
        }


        private int _percentComplete = 0;
        public int PercentComplete {
            get {
                return _percentComplete;
            }
            protected set {
                _percentComplete = value;
                NotifyPropertyChanged("PercentComplete");
            }
        }

        private bool _complete = false;
        public bool Complete {
            get {
                return _complete;
            }
            protected set {
                _complete = value;
                NotifyPropertyChanged("Complete");
            }
        }

        private bool _hasError = false;
        public bool HasError {
            get { return _hasError; }
            protected set {
                _hasError = value;
                NotifyPropertyChanged("HasError");
            }
        }

        private string _error = string.Empty;
        public string Error {
            get { return _error; }
            protected set {
                _error = value;
                NotifyPropertyChanged("Error");
            }
        }
        #endregion  //Properties

        public AsyncTaskBase() {
            /* NO-OP */
        }


        /// <summary>
        /// Allow derived classes to implement setup logic before the work is delegated
        /// </summary>
        /// <returns></returns>
        public virtual bool Setup() {
            //Does this need to do anything?

            return true;
        }

        private bool _started = false;
        private bool _canceled = false;
        public void Go() {
            if(_started) {
                throw new InvalidOperationException("The async task has already been started: " + Name);
            }
            _started = true;

            if(_canceled) {
                HasError = true;
                Error = "The operation was cancelled";
                Status = "The operation was cancelled";
                DoComplete();
                return;
            }
            _bgWorker = new BackgroundWorker();
            _bgWorker.DoWork += new DoWorkEventHandler(_bgWorker_DoWork);
            _bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bgWorker_RunWorkerCompleted);
            _bgWorker.WorkerReportsProgress = true;
            _bgWorker.WorkerSupportsCancellation = true;
            _bgWorker.ProgressChanged += new ProgressChangedEventHandler(_bgWorker_ProgressChanged);

            _bgWorker.RunWorkerAsync();
        }

        private void _bgWorker_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker bw = (BackgroundWorker)sender;
            //Call to do the bulk of the work
            Status = "Starting...";
            try {
                Run(bw, e);
            } catch(Exception ex) {
                log.Error("Error running the task process: " + Name, ex);
                HasError = true;
                Error += "Fatal Error running task: " + ex.Message + "\n";
            }
        }

        private void _bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            PercentComplete = e.ProgressPercentage;
        }

        public event EventHandler Completed;

        private void _bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if(e.Cancelled) {
                HasError = true;
                Error = "The operation was canceled";
                Status = "The operation was canceled";
            } else if(e.Error != null) {
                HasError = true;
                Error = e.Error.Message;
                //Status = "[Complete] " + Status;
            } else {
                //Status = "[Complete] " + Status;
            }
            _bgWorker.Dispose();

            DoComplete();
        }

        void DoComplete() {
            if(Completed != null) {
                Completed(this, EventArgs.Empty);
            }
            Complete = true;
            //log.Info("Async task complete: " + Name);
        }

        /// <summary>
        /// Derived classes must implement this method to provide functionality for the task
        /// </summary>
        /// <param name="e"></param>
        protected abstract void Run(BackgroundWorker bw, DoWorkEventArgs e);

        public void Cancel() {
            _canceled = true;
            if(_bgWorker != null) {
                _bgWorker.CancelAsync();
            }
        }


    }

   
}
