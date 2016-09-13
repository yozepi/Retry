﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Retry.Delays;
using System.Threading;

namespace Retry.Runners
{
    internal abstract class BaseRunner
    {

        public BaseRunner()
        {
            ExceptionList = new List<Exception>();
        }

        /// <summary>
        /// Contains the Action or Func that will be executed.
        /// </summary>
        /// <remarks>Ineritors cast this property to the apropriate value in the <see cref="ExecuteActorAsync"/> method</remarks>
        public Delegate Actor { get; set; }

        public int RetryCount { get; set; }

        public int Attempts { get; set; }

        public RetryStatus Status { get; private set; }
        public List<Exception> ExceptionList { get; private set; }

        public IDelay Delay { get; set; }

        public ErrorPolicyDelegate ErrorPolicy { get; set; }

        public Delegate SuccessPolicy { get; set; }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            Attempts = 0;
            ExceptionList.Clear();
            Status = RetryStatus.Running;

            try
            {
                for (int count = 0; count < RetryCount; count++)
                {

                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }

                    try
                    {
                        Attempts++;
                        await ExecuteActorAsync();
                        HandleSuccessPolicy(Attempts);
                        if (count == 0)
                        {
                            Status = RetryStatus.Success;
                        }
                        else
                        {
                            Status = RetryStatus.SuccessAfterRetries;
                        }
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }

                    catch (Exception ex)
                    {
                        if (HandleErrorPolicy(ex, count))
                        {
                            ExceptionList.Add(ex);

                            //Only wait if count hasn't ended.
                            if (count + 1 < RetryCount)
                            {
                                IDelay delay;
                                if (Delay != null)
                                    delay = Delay;
                                else
                                    delay = Delays.Delay.DefaultDelay;

                                await delay.WaitAsync(count, cancellationToken);
                            }
                        }
                        else
                        {
                            ExceptionList.Add(new ErrorPolicyException(ex));
                            Status = RetryStatus.Fail;
                            break;
                        }
                    }
                }

                if (Status == RetryStatus.Running)
                {
                    //still running after all attempts - FAIL!
                    Status = RetryStatus.Fail;
                }

            }
            catch (OperationCanceledException)
            {
                Status = RetryStatus.Canceled;
                throw;
            }

            return;
        }

        /// <summary>
        /// Implementors extend this method to execute the Func/Action.
        /// </summary>
        /// <returns></returns>
        protected abstract Task ExecuteActorAsync();

        /// <summary>
        /// Implementors execute this action to handle success policy calls.
        /// </summary>
        /// <param name="count"></param>
        protected abstract void HandleSuccessPolicy(int count);

        private bool HandleErrorPolicy(Exception ex, int retryCount)
        {
            if (ErrorPolicy == null)
                return true;
            return ErrorPolicy(ex, retryCount);
        }

        internal virtual void CopySettings(BaseRunner targetRunner)
        {
            targetRunner.Delay = Delay;
            targetRunner.ErrorPolicy = ErrorPolicy;
            targetRunner.SuccessPolicy = SuccessPolicy;
            targetRunner.RetryCount = RetryCount;
            targetRunner.Actor = Actor;
        }
    }
}
