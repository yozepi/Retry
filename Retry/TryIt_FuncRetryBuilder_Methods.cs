﻿using Retry.Runners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Retry.Builders;

namespace Retry
{
    partial class TryIt
    {


        #region Try methods:

        public static FuncRetryBuilder<TResult> Try<TResult>(Func<TResult> func, int retries)
        {
            return new FuncRetryBuilder<TResult>()
                .AddRunner(new FuncRunner<TResult>())
                .SetActor(func)
                .SetRetryCount(retries) as FuncRetryBuilder<TResult>;
        }

        public static FuncRetryBuilder<TResult> Try<T, TResult>(Func<T, TResult> func, T arg, int retries)
        {
            return new FuncRetryBuilder<TResult>()
                .AddRunner(new FuncRunner<T, TResult>(arg))
                .SetActor(func)
                .SetRetryCount(retries) as FuncRetryBuilder<TResult>;
        }

        public static FuncRetryBuilder<TResult> Try<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 arg1, T2 arg2, int retries)
        {
            return new FuncRetryBuilder<TResult>()
                   .AddRunner(new FuncRunner<T1, T2, TResult>(arg1, arg2))
                   .SetActor(func)
                   .SetRetryCount(retries) as FuncRetryBuilder<TResult>;
        }

        public static FuncRetryBuilder<TResult> Try<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 arg1, T2 arg2, T3 arg3, int retries)
        {
            return new FuncRetryBuilder<TResult>()
                       .AddRunner(new FuncRunner<T1, T2, T3, TResult>(arg1, arg2, arg3))
                       .SetActor(func)
                       .SetRetryCount(retries) as FuncRetryBuilder<TResult>;
        }

        public static FuncRetryBuilder<TResult> Try<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, int retries)
        {
            return new FuncRetryBuilder<TResult>()
                         .AddRunner(new FuncRunner<T1, T2, T3, T4, TResult>(arg1, arg2, arg3, arg4))
                         .SetActor(func)
                         .SetRetryCount(retries) as FuncRetryBuilder<TResult>;
        }

        //        public static ITryAndReturnValue<TResult> Try<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, int retries)
        //        {
        //            return new FuncTryIt<T1, T2, T3, T4, T5, TResult>(retries, arg1, arg2, arg3, arg4, arg5, func);
        //        }

        //        public static ITryAndReturnValue<TResult> Try<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, int retries)
        //        {
        //            return new FuncTryIt<T1, T2, T3, T4, T5, T6, TResult>(retries, arg1, arg2, arg3, arg4, arg5, arg6, func);
        //        }

        //        public static ITryAndReturnValue<TResult> Try<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, int retries)
        //        {
        //            return new FuncTryIt<T1, T2, T3, T4, T5, T6, T7, TResult>(retries, arg1, arg2, arg3, arg4, arg5, arg6, arg7, func);
        //        }

        //        public static ITryAndReturnValue<TResult> Try<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, int retries)
        //        {
        //            return new FuncTryIt<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(retries, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, func);
        //        }

        //        public static ITryAndReturnValue<TResult> Try<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, int retries)
        //        {
        //            return new FuncTryIt<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(retries, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, func);
        //        }

        #endregion //Try methods:


        #region UsingDelay, OnError, OnSuccess

        public static FuncRetryBuilder<TResult> UsingDelay<TResult>(this FuncRetryBuilder<TResult> builder, IDelay delay)
        {
            return builder
                .SetDelay(delay) as FuncRetryBuilder<TResult>;

        }

        public static FuncRetryBuilder<TResult> OnError<TResult>(this FuncRetryBuilder<TResult> builder, OnErrorDelegate onError)
        {
            return builder
                .SetOnError(onError) as FuncRetryBuilder<TResult>;
        }

        public static FuncRetryBuilder<TResult> OnSuccess<TResult>(this FuncRetryBuilder<TResult> builder, OnSuccessDelegate<TResult> onSuccess)
        {
            return builder
                .SetOnSuccess(onSuccess) as FuncRetryBuilder<TResult>;
        }

        #endregion //UsingDelay, OnError, OnSuccess


        #region ThenTry extensions:

        public static FuncRetryBuilder<TResult> ThenTry<TResult>(this FuncRetryBuilder<TResult> builder, int retries)
        {
            BaseRunner runner =
                  builder.LastRunner.GetType() == typeof(TaskWithResultRunner<TResult>)
                  ? new TaskWithResultRunner<TResult>()
                  : new FuncRunner<TResult>() as BaseRunner;

            return builder
                .AddRunner(runner)
                .SetRetryCount(retries) as FuncRetryBuilder<TResult>;
         }

        public static FuncRetryBuilder<TResult> ThenTry<T, TResult>(this FuncRetryBuilder<TResult> builder, T arg, int retries)
        {
            BaseRunner runner =
                builder.LastRunner.GetType() == typeof(TaskWithResultRunner<T, TResult>)
                ? new TaskWithResultRunner<T, TResult>(arg)
                : new FuncRunner<T, TResult>(arg) as BaseRunner;

            return builder
                .AddRunner(runner)
                .SetRetryCount(retries) as FuncRetryBuilder<TResult>;
        }

        public static FuncRetryBuilder<TResult> ThenTry<T1, T2, TResult>(this FuncRetryBuilder<TResult> builder, T1 arg1, T2 arg2, int retries)
        {
            BaseRunner runner =
                builder.LastRunner.GetType() == typeof(TaskWithResultRunner<T1, T2, TResult>)
                ? new TaskWithResultRunner<T1, T2, TResult>(arg1, arg2)
                : new FuncRunner<T1, T2, TResult>(arg1, arg2) as BaseRunner;

            return builder
                .AddRunner(runner)
                .SetRetryCount(retries) as FuncRetryBuilder<TResult>;
        }

        public static FuncRetryBuilder<TResult> ThenTry<T1, T2, T3, TResult>(this FuncRetryBuilder<TResult> builder, T1 arg1, T2 arg2, T3 arg3, int retries)
        {
            BaseRunner runner =
                builder.LastRunner.GetType() == typeof(TaskWithResultRunner<T1, T2, T3, TResult>)
                ? new TaskWithResultRunner<T1, T2, T3, TResult>(arg1, arg2, arg3)
                : new FuncRunner<T1, T2, T3, TResult>(arg1, arg2, arg3) as BaseRunner;

            return builder
                 .AddRunner(runner)
                 .SetRetryCount(retries) as FuncRetryBuilder<TResult>;
        }

        public static FuncRetryBuilder<TResult> ThenTry<T1, T2, T3, T4, TResult>(this FuncRetryBuilder<TResult> builder, T1 arg1, T2 arg2, T3 arg3, T4 arg4, int retries)
        {
            BaseRunner runner =
                  builder.LastRunner.GetType() == typeof(TaskWithResultRunner<T1, T2, T3, T4, TResult>)
                  ? new TaskWithResultRunner<T1, T2, T3, T4, TResult>(arg1, arg2, arg3, arg4)
                  : new FuncRunner<T1, T2, T3, T4, TResult>(arg1, arg2, arg3, arg4) as BaseRunner;

            return builder
                 .AddRunner(runner)
                 .SetRetryCount(retries) as FuncRetryBuilder<TResult>;
        }

        //        public static ITryAndReturnValue<TResult> ThenTry<T1, T2, T3, T4, T5, TResult>(this ITryAndReturnValue<TResult> tryit, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, int retries)
        //        {
        //            IInternalAccessor parent = tryit as IInternalAccessor;
        //            var child = new FuncTryIt<T1, T2, T3, T4, T5, TResult>(retries, arg1, arg2, arg3, arg4, arg5, parent.Actor as Func<T1, T2, T3, T4, T5, TResult>);
        //            ((IInternalAccessor)child).Parent = parent;
        //            return child;
        //        }

        //        public static ITryAndReturnValue<TResult> ThenTry<T1, T2, T3, T4, T5, T6, TResult>(this ITryAndReturnValue<TResult> tryit, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, int retries)
        //        {
        //            IInternalAccessor parent = tryit as IInternalAccessor;
        //            var child = new FuncTryIt<T1, T2, T3, T4, T5, T6, TResult>(retries, arg1, arg2, arg3, arg4, arg5, arg6, parent.Actor as Func<T1, T2, T3, T4, T5, T6, TResult>);
        //            ((IInternalAccessor)child).Parent = parent;
        //            return child;
        //        }

        //        public static ITryAndReturnValue<TResult> ThenTry<T1, T2, T3, T4, T5, T6, T7, TResult>(this ITryAndReturnValue<TResult> tryit, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, int retries)
        //        {
        //            IInternalAccessor parent = tryit as IInternalAccessor;
        //            var child = new FuncTryIt<T1, T2, T3, T4, T5, T6, T7, TResult>(retries, arg1, arg2, arg3, arg4, arg5, arg6, arg7, parent.Actor as Func<T1, T2, T3, T4, T5, T6, T7, TResult>);
        //            ((IInternalAccessor)child).Parent = parent;
        //            return child;
        //        }

        //        public static ITryAndReturnValue<TResult> ThenTry<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this ITryAndReturnValue<TResult> tryit, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, int retries)
        //        {
        //            IInternalAccessor parent = tryit as IInternalAccessor;
        //            var child = new FuncTryIt<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(retries, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, parent.Actor as Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>);
        //            ((IInternalAccessor)child).Parent = parent;
        //            return child;
        //        }

        //        public static ITryAndReturnValue<TResult> ThenTry<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this ITryAndReturnValue<TResult> tryit, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, int retries)
        //        {
        //            IInternalAccessor parent = tryit as IInternalAccessor;
        //            var child = new FuncTryIt<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(retries, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, parent.Actor as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>);
        //            ((IInternalAccessor)child).Parent = parent;
        //            return child;
        //        }

        #endregion //ThenTry extensions:

    }
}
