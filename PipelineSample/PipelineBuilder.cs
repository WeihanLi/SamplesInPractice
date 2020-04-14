using System;
using System.Collections.Generic;
using System.Linq;

namespace PipelineSample
{
    public interface IPipelineBuilder<TContext> where TContext : IPipelineContext
    {
        IPipelineBuilder<TContext> Use(Func<Action<TContext>, Action<TContext>> middleware);

        Action<TContext> Build();
    }

    public class PipelineBuilder<TContext> : IPipelineBuilder<TContext> where TContext : IPipelineContext
    {
        private readonly Action<TContext> _completeFunc;
        private readonly IList<Func<Action<TContext>, Action<TContext>>> pipelines = new List<Func<Action<TContext>, Action<TContext>>>();

        public PipelineBuilder(Action<TContext> completeFunc)
        {
            _completeFunc = completeFunc;
        }

        public IPipelineBuilder<TContext> Use(Func<Action<TContext>, Action<TContext>> middleware)
        {
            pipelines.Add(middleware);
            return this;
        }

        public static PipelineBuilder<TContext> New(Action<TContext> completeFunc)
        {
            return new PipelineBuilder<TContext>(completeFunc);
        }

        public Action<TContext> Build()
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
