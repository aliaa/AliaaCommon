using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon.Pipelining
{
    public class Pipeliner
    {
        public abstract class BaseStage
        {
            public PipelineTask Impl { get; set; }
            public Task Task { get; set; }
        }
        public class Stage<O> : BaseStage
        {
            public BlockingCollection<O> OutputBuffer { get; set; }
        }

        public readonly List<BaseStage> Stages = new List<BaseStage>();

        public Pipeliner AddStartingTask<O>(PipelineStartingTask<O> startingTask)
        {
            if (Stages.Count > 0)
                throw new Exception("Starting task must be added at first!");

            BlockingCollection<O> buffer = new BlockingCollection<O>();
            Task task = new Task(() => startingTask.Process(buffer));
            Stage<O> stage = new Stage<O>
            {
                Impl = startingTask,
                Task = task,
                OutputBuffer = buffer
            };
            Stages.Add(stage);
            return this;
        }

        public Pipeliner AddMiddleTask<I, O>(PipelineMiddleTask<I, O> middleTask)
        {
            if (Stages.Count == 0)
                throw new Exception("middle task must not be added at first!");
            Stage<I> previousStage = (Stage<I>)Stages.Last();
            BlockingCollection<O> buffer = new BlockingCollection<O>();
            Task task = new Task(() => middleTask.Process(previousStage.OutputBuffer, buffer));
            Stage<O> stage = new Stage<O>
            {
                Impl = middleTask, 
                Task = task,
                OutputBuffer = buffer
            };
            Stages.Add(stage);

            return this;
        }

        public void AddEndTask<I>(PipelineEndTask<I> endTask)
        {
            if (Stages.Count == 0)
                throw new Exception("end task must not be added at first!");
            Stage<I> previousStage = (Stage<I>)Stages.Last();
            Task task = new Task(() => endTask.Process(previousStage.OutputBuffer));
            Stage<I> stage = new Stage<I>
            {
                Impl = endTask,
                Task = task
            };
            Stages.Add(stage);
        }

        public void StartParallel()
        {
            foreach (var s in Stages)
                s.Task.Start();
        }

        public void WaitToFinish()
        {
            Task.WaitAll(Stages.Select(s => s.Task).ToArray());
        }

        public void RunInSequence()
        {
            foreach (var s in Stages)
                s.Task.RunSynchronously();
        }
    }
}
