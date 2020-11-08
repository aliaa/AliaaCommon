using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon.Pipelining
{
    public abstract class PipelineEndTask<I> : PipelineTask
    {
        public abstract void ProcessItem(I inputItem);

        public virtual void Process(BlockingCollection<I> input)
        {
            try
            {
                ProcessedCount = 0;
                foreach (var i in input.GetConsumingEnumerable())
                {
                    ProcessItem(i);
                    ProcessedCount++;
                }
            }
            catch (Exception ex)
            {
                RiseExceptionEvent(ex);
            }
            finally
            {
                RiseFinishedEvent();
            }
        }
    }
}
