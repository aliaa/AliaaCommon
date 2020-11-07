using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon.Pipelining
{
    public abstract class PipelineStartingTask<O> : PipelineTask
    {
        public abstract void Process(BlockingCollection<O> output);
    }
}
