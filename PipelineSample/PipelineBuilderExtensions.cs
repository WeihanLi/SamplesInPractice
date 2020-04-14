using System;
using System.Threading.Tasks;

namespace PipelineSample
{
    public static class PipelineBuilderExtensions
    {
        public static IPipelineBuilder<TContext> Use<TContext>(this IPipelineBuilder<TContext> builder, Action<TContext, Action> action)
            where TContext : IPipelineContext
        {
            return builder.Use(next =>
                context =>
                {
                    action(context, () => next(context));
                });
        }

        public static IAsyncPipelineBuilder<TContext> Use<TContext>(this IAsyncPipelineBuilder<TContext> builder,
            Func<TContext, Func<Task>, Task> func)
            where TContext : IPipelineContext
        {
            return builder.Use(next =>
                context =>
                {
                    return func(context, () => next(context));
                });
        }
    }
}
