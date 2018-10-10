using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon.Pipelining
{
    public abstract class PipelineMiddleTask<I, O> : PipelineTask
    {
        public abstract O ProcessItem(I inputItem);

        public virtual void Process(BlockingCollection<I> input, BlockingCollection<O> output)
        {
            try
            {
                ProcessedCount = 0;
                foreach (var i in input.GetConsumingEnumerable())
                {
                    O outputItem = ProcessItem(i);
                    if(outputItem != null)
                        output.Add(outputItem);
                    ProcessedCount++;
                }
            }
            catch(Exception ex)
            {
                RiseExceptionEvent(ex);
            }
            finally
            {
                output.CompleteAdding();
                RiseFinishedEvent();
            }
        }
    }
}
