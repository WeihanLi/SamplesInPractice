using Microsoft.Extensions.Hosting;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Services;

namespace Net8Sample;

public static class ExceptionThrowSample
{
    public static void MainTest()
    {
        ArgumentNullExceptionSample(null);
        ArgumentExceptionThrowIfNullOrEmptySample(null);
        ArgumentExceptionThrowIfNullOrEmptySample(string.Empty);
        
        ArgumentExceptionThrowIfNullOrWhiteSpaceSample(null);
        ArgumentExceptionThrowIfNullOrWhiteSpaceSample(string.Empty);
        ArgumentExceptionThrowIfNullOrWhiteSpaceSample(" ");

        ArgumentOutOfRangeExceptionSample();
        
        ObjectDisposedExceptionSample();
    }

    private static void ArgumentNullExceptionSample(string? value)
    {
        InvokeHelper.TryInvoke(() => ArgumentNullException.ThrowIfNull(value));
    }

    private static void ArgumentExceptionThrowIfNullOrEmptySample(string? value)
    {
        InvokeHelper.TryInvoke(() => ArgumentException.ThrowIfNullOrEmpty(value));
        
        // public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        // {
        //     if (string.IsNullOrEmpty(argument))
        //     {
        //         ThrowNullOrEmptyException(argument, paramName);
        //     }
        // }
        // private static void ThrowNullOrEmptyException(string? argument, string? paramName)
        // {
        //     ArgumentNullException.ThrowIfNull(argument, paramName);
        //     throw new ArgumentException(SR.Argument_EmptyString, paramName);
        // }
    }

    private static void ArgumentExceptionThrowIfNullOrWhiteSpaceSample(string? value)
    {
        InvokeHelper.TryInvoke(() => ArgumentException.ThrowIfNullOrWhiteSpace(value));
        //
        // public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        // {
        //     if (string.IsNullOrWhiteSpace(argument))
        //     {
        //         ThrowNullOrWhiteSpaceException(argument, paramName);
        //     }
        // }
        //
        // [DoesNotReturn]
        // private static void ThrowNullOrWhiteSpaceException(string? argument, string? paramName)
        // {
        //     ArgumentNullException.ThrowIfNull(argument, paramName);
        //     throw new ArgumentException(SR.Argument_EmptyOrWhiteSpaceString, paramName);
        // }
    }

    private static void ObjectDisposedExceptionSample()
    {
        InvokeHelper.TryInvoke(() => ObjectDisposedException.ThrowIf(true, typeof(BackgroundService)));
        InvokeHelper.TryInvoke(() => ObjectDisposedException.ThrowIf(true, new GuidIdGenerator()));
        
        // [StackTraceHidden]
        // public static void ThrowIf([DoesNotReturnIf(true)] bool condition, object instance)
        // {
        //     if (!condition)
        //         return;
        //     ThrowHelper.ThrowObjectDisposedException(instance);
        // }
        //
        // [StackTraceHidden]
        // public static void ThrowIf([DoesNotReturnIf(true)] bool condition, Type type)
        // {
        //     if (!condition)
        //         return;
        //     ThrowHelper.ThrowObjectDisposedException(type);
        // }
        //
        // [DoesNotReturn]
        // internal static void ThrowObjectDisposedException(object? instance)
        // {
        //     throw new ObjectDisposedException(instance?.GetType().FullName);
        // }
        //
        // [DoesNotReturn]
        // internal static void ThrowObjectDisposedException(Type? type)
        // {
        //     throw new ObjectDisposedException(type?.FullName);
        // }
    }

    private static void ArgumentOutOfRangeExceptionSample()
    {
        InvokeHelper.TryInvoke(() => ArgumentOutOfRangeException.ThrowIfZero(0));
        InvokeHelper.TryInvoke(() => ArgumentOutOfRangeException.ThrowIfNegative(-1));
        InvokeHelper.TryInvoke(() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero( 0));
        InvokeHelper.TryInvoke(() => ArgumentOutOfRangeException.ThrowIfEqual(-1, -1));
        InvokeHelper.TryInvoke(() => ArgumentOutOfRangeException.ThrowIfNotEqual(-1, 0));
        InvokeHelper.TryInvoke(() => ArgumentOutOfRangeException.ThrowIfGreaterThan(1, 0));
        InvokeHelper.TryInvoke(() => ArgumentOutOfRangeException.ThrowIfLessThan(-1, 0));
    }
}
