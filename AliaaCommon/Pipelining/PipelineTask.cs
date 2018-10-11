using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon.Pipelining
{
    public abstract class PipelineTask
    {
        public int ProcessedCount { get; protected set; }
        public event EventHandler<Exception> ExceptionOccured;
        public event EventHandler ProcessFinished;

        protected void RiseExceptionEvent(Exception ex)
        {
            ExceptionOccured?.Invoke(this, ex);
        }

        protected void RiseFinishedEvent()
        {
            ProcessFinished?.Invoke(this, new EventArgs());
        }
    }
}
