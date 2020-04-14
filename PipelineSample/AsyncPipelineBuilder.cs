using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PipelineSample
{
    public interface IAsyncPipelineBuilder<TContext> where TContext : IPipelineContext
    {
        IAsyncPipelineBuilder<TContext> Use(Func<Func<TContext, Task>, Func<TContext, Task>> middleware);

        Func<TContext, Task> Build();
    }

    public class AsyncPipelineBuilder<TContext> : IAsyncPipelineBuilder<TContext> where TContext : IPipelineContext
    {
        private readonly Func<TContext, Task> _completeFunc;
        private readonly IList<Func<Func<TContext, Task>, Func<TContext, Task>>> pipelines = new List<Func<Func<TContext, Task>, Func<TContext, Task>>>();

        public AsyncPipelineBuilder(Func<TContext, Task> completeFunc)
        {
            _completeFunc = completeFunc;
        }

        public IAsyncPipelineBuilder<TContext> Use(Func<Func<TContext, Task>, Func<TContext, Task>> middleware)
        {
            pipelines.Add(middleware);
            return this;
        }

        public static AsyncPipelineBuilder<TContext> New(Func<TContext, Task> completeFunc)
        {
            return new AsyncPipelineBuilder<TContext>(completeFunc);
        }

        public Func<TContext, Task> Build()
        {
            var request = _completeFunc;
            foreach (var pipeline in pipelines.Reverse())
            {
                request = pipeline(request);
            }
            return request;
        }
    }
}
