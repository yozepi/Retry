﻿using Moq;
using NSpec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace Retry.Tests.Unit.specs
{
    class TryIt_Func_Methods : nspec
    {
        void Static_Func_TResult_TryItMethods()
        {
            ITryAndReturnValue<string> subject = null;
            Func<string> subjectFunc = null;
            string subjectResult = null;
            string expectedResult = null;
            int retries = default(int);
            Exception thrown = null;

            beforeAll = () =>
            {
                subjectResult = null;
                expectedResult = "Hello!";
                retries = 3;
                thrown = null;
                subjectFunc = () =>
                {
                    return expectedResult;
                };

            };

            act = () => subject = TryIt.Try(subjectFunc, retries);

   
            describe["TryIt.Try(func, retries).OnError()"] = () =>
            {
                OnErrorDelegate errorDelegate = (ex, i) => { return true; };
                object onErrorResult = null;
                before = () => onErrorResult = null;
                act = () => onErrorResult = subject.OnError(errorDelegate);

                it["should set the internal onError property"] = () =>
                    onErrorResult.As<IInternalAccessor>().OnError.Should().BeSameAs(errorDelegate);

                it["should return the subject"] = () =>
                    subject.Should().BeSameAs(onErrorResult);

                describe["TryIt.Try(func, retries).OnError(OnErrorDelegate).Go()"] = () =>
                {
                    int errorDelegateCallCount = default(int);
                    before = () => errorDelegateCallCount = default(int);
                    act = () =>
                    {
                        try
                        {
                            subject.Go();
                        }
                        catch (Exception ex)
                        {
                            thrown = ex;
                        }
                    };

                    context["when there are no exceptions to test"] = () =>
                    {
                        before = () => errorDelegate = (ex, i) => { errorDelegateCallCount++; return true; };
                        it["OnError delegate should never be called"] = () =>
                            errorDelegateCallCount.Should().Be(0);
                    };

                    context["when all exceptions pass the policy set by the delegate"] = () =>
                    {
                        before = () =>
                        {
                            errorDelegate = (ex, i) => { errorDelegateCallCount++; return true; };
                            subjectFunc = () => { throw new Exception(); };
                        };

                        it["should throw RetryFailedException"] = () =>
                            thrown.Should().BeOfType<RetryFailedException>();

                        it["should check the OnError police once for everey failed attempt"] = () =>
                        {
                            errorDelegateCallCount.Should().NotBe(0);
                            errorDelegateCallCount.Should().Be(subject.Attempts);
                        };
                    };

                    context["when the exception fails the policy set by the delegate"] = () =>
                    {
                        Exception expectedException = new Exception("I Failed!");
                        before = () =>
                        {
                            subjectFunc = () => { throw expectedException; };
                            errorDelegate = (ex, i) => { return false; };
                        };

                        it["should throw the exception"] = () =>
                            thrown.Should().Be(expectedException);
                    };

                    context["when the delegate throws an exception"] = () =>
                    {
                        Exception expectedException = new InvalidOperationException("Can't do that!");
                        before = () =>
                        {
                            subjectFunc = () => { throw new Exception(); };
                            errorDelegate = (ex, i) => { throw expectedException; };
                        };

                        it["should throw the exception"] = () =>
                            thrown.Should().Be(expectedException);
                    };

                };
            };

            describe["TryIt.Try(func, retries).OnSuccess()"] = () =>
            {
                OnSuccessDelegate<string> successDelegate = null;
                object onSuccessResult = null;
                beforeAll = () => successDelegate = (i, r) => { };
                act = () => onSuccessResult = subject.OnSuccess(successDelegate);

                it["should set the internal oSuccess property"] = () =>
                    onSuccessResult.As<IInternalAccessor>().OnSuccess.Should().BeSameAs(successDelegate);

                it["should return the subject"] = () =>
                    subject.Should().BeSameAs(onSuccessResult);

                describe["TryIt.Try(func, retries).OnSuccess(onSuccessDelegate<T>).Go()"] = () =>
                {
                    object testResult = null;
                    object result = null;
                    bool onSuccessCalled = false;
                    beforeAll = () =>
                    {
                        subjectFunc = () => expectedResult;
                        testResult = null;
                        result = null;
                        onSuccessCalled = false;
                        successDelegate = (i, r) => { testResult = r; onSuccessCalled = true; };
                    };
                    act = () =>
                    {
                        subject = TryIt.Try(subjectFunc, retries);
                        try
                        {
                            result = subject.OnSuccess(successDelegate).Go();
                        }
                        catch (Exception ex)
                        {

                            thrown = ex;
                        }
                    };

                    it["should call the onSuccessDelegate"] = () =>
                        onSuccessCalled.Should().BeTrue();

                    it["should pass the result to the OnSuccess delegate"] = () =>
                        testResult.Should().Be(expectedResult);

                    it["should return the expected result"] = () =>
                        result.Should().Be(expectedResult);


                    it["should set the status to Success"] = () =>
                        subject.Status.Should().Be(RetryStatus.Success);

                    context["when the OnSuccess delegate throws an exception (policy fail)"] = () =>
                    {

                        Exception expectedException = new NotImplementedException();

                        before = () =>
                        {
                            successDelegate = (i, r) =>
                            {
                                if (subject.ExceptionList.Count == 0)
                                    throw expectedException;
                            };
                        };

                        it["should put the exception into the exceptions list"] = () =>
                            subject.ExceptionList.Should().Contain(expectedException);

                        it["should set status to SuccessAfterRetries"] = () =>
                            subject.Status.Should().Be(RetryStatus.SuccessAfterRetries);    
                    };

                    context["when every successful attempt throws an exception (policy fail)"] = () =>
                    {

                        Exception expectedException = new NotImplementedException();

                        before = () =>
                        {
                            successDelegate = (i, r) =>
                            {
                                throw expectedException;
                            };
                        };

                        it["should set status to Fail"] = () =>
                            subject.Status.Should().Be(RetryStatus.Fail);

                        it["every policy exception should be in the ExceptionList"] = () =>
                            subject.ExceptionList.Where(x => x == expectedException)
                                .Count().Should().Be(subject.ExceptionList.Count);

                        it["should throw RetryFailedException"] = () =>
                            thrown.Should().BeOfType<RetryFailedException>();

                    };
                };

            };

            describe["TryIt.Try(func, retries).UsingDelay(delay)"] = () =>
            {
                Mock<IDelay> mockDelay = null;
                IDelay newPause = null;
                ITryAndReturnValue<string> result = null;
                before = () =>
                {
                    thrown = null;
                    result = null;
                    mockDelay = new Mock<IDelay>();
                    newPause = mockDelay.Object;
                };

                act = () =>
                {
                    try
                    {
                        result = subject.UsingDelay(newPause);
                    }
                    catch (Exception ex)
                    {
                        thrown = ex;
                    }
                };

                it["Should set the Delay property"] = () =>
                    subject.Delay.Should().Be(newPause);

                it["should return the subject"] = () =>
                    result.Should().Be(subject);

                it["should not throw any exceptions"] = () =>
                    thrown.Should().BeNull();

                context["when the Delay parameter is null"] = () =>
                {
                    before = () => newPause = null;

                    it["should throw an ArgumentNullException"] = () =>
                        thrown.Should().BeOfType<ArgumentNullException>();

                };
            };

            describe["TryIt.Try(func, retries).Go()"] = () =>
            {

                act = () =>
                {
                    subjectResult = subject.Go();
                };

                it["should return the result of calling the Func"] = () =>
                    subjectResult.Should().Be(expectedResult);

                it["should set status to RetryStatus.Success"] = () =>
                    subject.Status.Should().Be(RetryStatus.Success);

                it["should have an empty ExceptionList"] = () =>
                    subject.ExceptionList.Should().BeEmpty();

                context["when retries is an invalid value"] = () =>
                {
                    Action action = () => TryIt.Try(subjectFunc, 0);

                    it["should throw an ArgumentOutOfRangeException"] = () =>
                        action.ShouldThrow<ArgumentOutOfRangeException>();
                };

                context["when every attempt fails"] = () =>
                {
                    act = () =>
                    {
                        subjectFunc = () =>
                        {
                            throw new Exception("I tried. I failed.");
                        };
                        subject = TryIt.Try(subjectFunc, retries);
                        try
                        {
                            subject.Go();
                        }
                        catch (Exception ex)
                        {
                            thrown = ex;
                        }
                    };
                    it["should throw RetryFailedException exception"] = () =>
                        thrown.Should().BeOfType<RetryFailedException>();
                };
            };

            describe["TryIt.Try(func, retries).ThenTry(retries)"] = () =>
            {
                ITryAndReturnValue<string> child = null;
                before = () => child = subject.ThenTry(retries);

                it["should create a child TryIt instance"] = () =>
                    child.Should().NotBeNull();

                it["the child should be distinct from it's parent"] = () =>
                    child.Should().NotBe(subject);

                describe["TryIt.Try(func, retries).ThenTry(retries).Go()"] = () =>
                {
                    context["When initial try fails, should excecute ThenTry()"] = () =>
                    {
                        string altSubjectResult = null;
                        string altExpectedResult = "World!";
                        int funcExecutedCount = default(int);
                        beforeAll = () =>
                        {
                            altSubjectResult = null;
                            funcExecutedCount = default(int);
                            altSubjectResult = null;
                            subjectFunc = () =>
                            {
                                funcExecutedCount++;
                                if (funcExecutedCount <= retries)
                                    throw new Exception("Bad input!");

                                return altExpectedResult;
                            };
                        };

                        act = () =>
                        {
                            funcExecutedCount = default(int);
                            subject = TryIt.Try(subjectFunc, retries);
                            child = subject.ThenTry(retries);
                            child.Go();
                        };


                        it["should return the result of Func"] = () =>
                            altSubjectResult.Should().Be(altSubjectResult);

                        it["should set the Status to SuccessAfterRetries"] = () =>
                            child.Status.Should().Be(RetryStatus.SuccessAfterRetries);

                        it["ThenTry() should not have any exceptions in it's ExceptionsList"] = () =>
                            child.ExceptionList.Count.Should().Be(0);

                        it["ThenTry().GetAllExceptions() should have the exceptions from Try()"] = () =>
                            child.GetAllExceptions().Should().BeEquivalentTo(subject.ExceptionList);

                    };
                };
            };

        }

        void Static_Func_T_TResult_TryItMethods()
        {
            ITryAndReturnValue<string> subject = null;
            Func<int, string> subjectFunc = null;
            int arg = 23;
            string expectedResult = "Hi there!";
            int retries = 4;
            string actualResult = null;

            before = () =>
            {
                actualResult = null;
                subjectFunc = (i) => { return expectedResult; };

            };

            act = () => subject = TryIt.Try(subjectFunc, arg, retries);
            describe["TryIt.Try(func, arg, retries)"] = () =>
            {
                it["should return an ITryAndReturnValue<TResult> instance"] = () =>
                    subject.Should().NotBeNull();


                it["should set the arg internal property"] = () =>
                {
                    var asTryIt = subject.As<FuncTryIt<int, string>>();
                    asTryIt._arg.Should().Be(arg);
                };

            };

            describe["TryIt.Try(func, arg, retries).Go()"] = () =>
            {
                act = () => actualResult = subject.Go();

                it["should return the expected result"] = () =>
                    actualResult.Should().Be(expectedResult);

                it["should attempt the Try only once"] = () =>
                    subject.Attempts.Should().Be(1);
            };

            describe["TryIt.Try(func, arg, retries).ThenTry(arg, retries)"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () => child = subject.ThenTry(arg, retries);

                it["should return a child ITryAndReturnValue instance"] = () =>
                    child.Should().NotBeNull();

                it["the child should be distinct from the parent"] = () =>
                    child.Should().NotBe(subject);
            };

            describe["TryIt.Try(func, arg, retries).ThenTry(arg, retries).Go()"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () =>
                {
                    child = subject.ThenTry(arg, retries);
                    actualResult = child.Go();
                };

                context["when the parent Try succeeds"] = () =>
                {
                    it["should return the expected result"] = () =>
                         actualResult.Should().Be(expectedResult);

                    it["The subject should have tried once"] = () =>
                        subject.Attempts.Should().Be(1);

                    it["The child should not have been tried"] = () =>
                        child.Attempts.Should().Be(0);
                };

                context["when the parent Try fails"] = () =>
                {
                    int funcAttempts = default(int);

                    before = () =>
                    {
                        funcAttempts = 0;
                        subjectFunc = (i) =>
                        {
                            funcAttempts++;
                            if (funcAttempts <= retries)
                                throw new Exception("I didn't mean it!");

                            return expectedResult;
                        };
                    };
                    it["should return the expected result"] = () =>
                            actualResult.Should().Be(expectedResult);

                    it["the parent should have tried once for each retry"] = () =>
                        subject.Attempts.Should().Be(retries);

                    it["the result should have come from the child and not from the parent"] = () =>
                    {
                        subject.As<FuncTryIt<int, string>>().HasResult.Should().BeFalse();
                        child.As<FuncTryIt<int, string>>().HasResult.Should().BeTrue();
                    };
                };
            };
        }

        void Static_Func_T1_T2_TResult_TryItMethods()
        {
            ITryAndReturnValue<string> subject = null;
            Func<int, double, string> subjectFunc = null;
            int arg1 = 23;
            double arg2 = Math.E;
            string expectedResult = "Hi there!";
            int retries = 4;
            string actualResult = null;

            before = () =>
            {
                actualResult = null;
                subjectFunc = (i, d) => { return expectedResult; };
            };

            act = () => subject = TryIt.Try(subjectFunc, arg1, arg2, retries);

            describe["TryIt.Try(func, arg, retries)"] = () =>
            {
                it["should return an ITryAndReturnValue<TResult> instance"] = () =>
                    subject.Should().NotBeNull();


                it["should set arg1 and arg2 internal properties"] = () =>
                {
                    var asTryIt = subject.As<FuncTryIt<int, double, string>>();
                    asTryIt._arg1.Should().Be(arg1);
                    asTryIt._arg2.Should().Be(arg2);
                };

            };

            describe["TryIt.Try(func, arg1, arg2, retries).Go()"] = () =>
            {
                act = () => actualResult = subject.Go();

                it["should return the expected result"] = () =>
                    actualResult.Should().Be(expectedResult);

                it["should attempt the Try only once"] = () =>
                    subject.Attempts.Should().Be(1);
            };

            describe["TryIt.Try(func, arg1, arg2, retries).ThenTry(arg1, arg2, retries)"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () => child = subject.ThenTry(arg1, arg2, retries);

                it["should return a child ITryAndReturnValue instance"] = () =>
                    child.Should().NotBeNull();

                it["the child should be distinct from the parent"] = () =>
                    child.Should().NotBe(subject);
            };

            describe["TryIt.Try(func, arg1, arg2, retries).ThenTry(arg1, arg2, retries).Go()"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () =>
                {
                    child = subject.ThenTry(arg1, arg2, retries);
                    actualResult = child.Go();
                };

                context["when the parent Try succeeds"] = () =>
                {
                    it["should return the expected result"] = () =>
                         actualResult.Should().Be(expectedResult);

                    it["The subject should have tried once"] = () =>
                        subject.Attempts.Should().Be(1);

                    it["The child should not have been tried"] = () =>
                        child.Attempts.Should().Be(0);
                };

                context["when the parent Try fails"] = () =>
                {
                    int funcAttempts = default(int);

                    before = () =>
                    {
                        funcAttempts = 0;
                        subjectFunc = (a1, a2) =>
                        {
                            funcAttempts++;
                            if (funcAttempts <= retries)
                                throw new Exception("I didn't mean it!");

                            return expectedResult;
                        };
                    };
                    it["should return the expected result"] = () =>
                            actualResult.Should().Be(expectedResult);

                    it["the parent should have tried once for each retry"] = () =>
                        subject.Attempts.Should().Be(retries);

                    it["the result should have come from the child and not from the parent"] = () =>
                    {
                        subject.As<FuncTryIt<int, double, string>>().HasResult.Should().BeFalse();
                        child.As<FuncTryIt<int, double, string>>().HasResult.Should().BeTrue();
                    };
                };
            };
        }

        void Static_Func_T1_T2_T3_TResult_TryItMethods()
        {
            ITryAndReturnValue<string> subject = null;
            Func<int, double, long, string> subjectFunc = null;

            int arg1 = 23;
            double arg2 = Math.E;
            long arg3 = long.MinValue;

            string expectedResult = "Hi there!";
            int retries = 4;
            string actualResult = null;

            before = () =>
            {
                actualResult = null;
                subjectFunc = (i, d, l) => { return expectedResult; };
            };

            act = () => subject = TryIt.Try(subjectFunc, arg1, arg2, arg3, retries);

            describe["TryIt.Try(func, arg, retries)"] = () =>
            {
                it["should return an ITryAndReturnValue<TResult> instance"] = () =>
                    subject.Should().NotBeNull();


                it["should set arg1 - arg3 internal properties"] = () =>
                {
                    var asTryIt = subject.As<FuncTryIt<int, double, long, string>>();
                    asTryIt._arg1.Should().Be(arg1);
                    asTryIt._arg2.Should().Be(arg2);
                    asTryIt._arg3.Should().Be(arg3);
                };

            };

            describe["TryIt.Try(func, arg1, arg2, arg3, retries).Go()"] = () =>
            {
                act = () => actualResult = subject.Go();

                it["should return the expected result"] = () =>
                    actualResult.Should().Be(expectedResult);

                it["should attempt the Try only once"] = () =>
                    subject.Attempts.Should().Be(1);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, retries).ThenTry(arg1, arg2, arg3, retries)"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () => child = subject.ThenTry(arg1, arg2, arg3, retries);

                it["should return a child ITryAndReturnValue instance"] = () =>
                    child.Should().NotBeNull();

                it["the child should be distinct from the parent"] = () =>
                    child.Should().NotBe(subject);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, retries).ThenTry(arg1, arg2, arg3, retries).Go()"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () =>
                {
                    child = subject.ThenTry(arg1, arg2, arg3, retries);
                    actualResult = child.Go();
                };

                context["when the parent Try succeeds"] = () =>
                {
                    it["should return the expected result"] = () =>
                         actualResult.Should().Be(expectedResult);

                    it["The subject should have tried once"] = () =>
                        subject.Attempts.Should().Be(1);

                    it["The child should not have been tried"] = () =>
                        child.Attempts.Should().Be(0);
                };

                context["when the parent Try fails"] = () =>
                {
                    int funcAttempts = default(int);

                    before = () =>
                    {
                        funcAttempts = 0;
                        subjectFunc = (a1, a2, a3) =>
                        {
                            funcAttempts++;
                            if (funcAttempts <= retries)
                                throw new Exception("I didn't mean it!");

                            return expectedResult;
                        };
                    };
                    it["should return the expected result"] = () =>
                            actualResult.Should().Be(expectedResult);

                    it["the parent should have tried once for each retry"] = () =>
                        subject.Attempts.Should().Be(retries);

                    it["the result should have come from the child and not from the parent"] = () =>
                    {
                        subject.As<FuncTryIt<int, double, long, string>>().HasResult.Should().BeFalse();
                        child.As<FuncTryIt<int, double, long, string>>().HasResult.Should().BeTrue();
                    };
                };
            };
        }

        void Static_Func_T1_T2_T3_T4_TResult_TryItMethods()
        {
            ITryAndReturnValue<string> subject = null;
            Func<int, double, long, string, string> subjectFunc = null;

            int arg1 = 23;
            double arg2 = Math.E;
            long arg3 = long.MinValue;
            string arg4 = "Happy to be here!";

            string expectedResult = "Hi there!";
            int retries = 4;
            string actualResult = null;

            before = () =>
            {
                actualResult = null;
                subjectFunc = (i, d, l, s) => { return expectedResult; };
            };

            act = () => subject = TryIt.Try(subjectFunc, arg1, arg2, arg3, arg4, retries);
            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, retries)"] = () =>
            {
                it["should return an ITryAndReturnValue<TResult> instance"] = () =>
                    subject.Should().NotBeNull();


                it["should set arg1 - arg4 internal properties"] = () =>
                {
                    var asTryIt = subject.As<FuncTryIt<int, double, long, string, string>>();
                    asTryIt._arg1.Should().Be(arg1);
                    asTryIt._arg2.Should().Be(arg2);
                    asTryIt._arg3.Should().Be(arg3);
                    asTryIt._arg4.Should().Be(arg4);
                };

            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, retries).Go()"] = () =>
            {
                act = () => actualResult = subject.Go();

                it["should return the expected result"] = () =>
                    actualResult.Should().Be(expectedResult);

                it["should attempt the Try only once"] = () =>
                    subject.Attempts.Should().Be(1);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, retries).ThenTry(arg1, arg2, arg3, arg4, retries)"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () => child = subject.ThenTry(arg1, arg2, arg3, arg4, retries);

                it["should return a child ITryAndReturnValue instance"] = () =>
                    child.Should().NotBeNull();

                it["the child should be distinct from the parent"] = () =>
                    child.Should().NotBe(subject);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, retries).ThenTry(arg1, arg2, arg3, arg4, retries).Go()"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () =>
                {
                    child = subject.ThenTry(arg1, arg2, arg3, arg4, retries);
                    actualResult = child.Go();
                };

                context["when the parent Try succeeds"] = () =>
                {
                    it["should return the expected result"] = () =>
                         actualResult.Should().Be(expectedResult);

                    it["The subject should have tried once"] = () =>
                        subject.Attempts.Should().Be(1);

                    it["The child should not have been tried"] = () =>
                        child.Attempts.Should().Be(0);
                };

                context["when the parent Try fails"] = () =>
                {
                    int funcAttempts = default(int);

                    before = () =>
                    {
                        funcAttempts = 0;
                        subjectFunc = (a1, a2, a3, a4) =>
                        {
                            funcAttempts++;
                            if (funcAttempts <= retries)
                                throw new Exception("I didn't mean it!");

                            return expectedResult;
                        };
                    };
                    it["should return the expected result"] = () =>
                            actualResult.Should().Be(expectedResult);

                    it["the parent should have tried once for each retry"] = () =>
                        subject.Attempts.Should().Be(retries);

                    it["the result should have come from the child and not from the parent"] = () =>
                    {
                        subject.As<FuncTryIt<int, double, long, string, string>>().HasResult.Should().BeFalse();
                        child.As<FuncTryIt<int, double, long, string, string>>().HasResult.Should().BeTrue();
                    };
                };
            };
        }

        void Static_Func_T1_T2_T3_T4_T5_TResult_TryItMethods()
        {
            ITryAndReturnValue<string> subject = null;
            Func<int, double, long, string, float, string> subjectFunc = null;

            int arg1 = 23;
            double arg2 = Math.E;
            long arg3 = long.MinValue;
            string arg4 = "Happy to be here!";
            float arg5 = 373782.2378862F;

            string expectedResult = "Hi there!";
            int retries = 4;
            string actualResult = null;

            before = () =>
            {
                actualResult = null;
                subjectFunc = (i, d, l, s, f) => { return expectedResult; };
            };

            act = () => subject = TryIt.Try(subjectFunc, arg1, arg2, arg3, arg4, arg5, retries);
            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, retries)"] = () =>
            {
                it["should return an ITryAndReturnValue<TResult> instance"] = () =>
                    subject.Should().NotBeNull();


                it["should set arg1 - arg5 internal properties"] = () =>
                {
                    var asTryIt = subject.As<FuncTryIt<int, double, long, string, float, string>>();
                    asTryIt._arg1.Should().Be(arg1);
                    asTryIt._arg2.Should().Be(arg2);
                    asTryIt._arg3.Should().Be(arg3);
                    asTryIt._arg4.Should().Be(arg4);
                    asTryIt._arg5.Should().Be(arg5);
                };

            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, retries).Go()"] = () =>
            {
                act = () => actualResult = subject.Go();

                it["should return the expected result"] = () =>
                    actualResult.Should().Be(expectedResult);

                it["should attempt the Try only once"] = () =>
                    subject.Attempts.Should().Be(1);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, retries).ThenTry(arg1, arg2, arg3, arg4, arg5, retries)"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () => child = subject.ThenTry(arg1, arg2, arg3, arg4, arg5, retries);

                it["should return a child ITryAndReturnValue instance"] = () =>
                    child.Should().NotBeNull();

                it["the child should be distinct from the parent"] = () =>
                    child.Should().NotBe(subject);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, retries).ThenTry(arg1, arg2, arg3, arg4, arg5, retries).Go()"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () =>
                {
                    child = subject.ThenTry(arg1, arg2, arg3, arg4, arg5, retries);
                    actualResult = child.Go();
                };

                context["when the parent Try succeeds"] = () =>
                {
                    it["should return the expected result"] = () =>
                         actualResult.Should().Be(expectedResult);

                    it["The subject should have tried once"] = () =>
                        subject.Attempts.Should().Be(1);

                    it["The child should not have been tried"] = () =>
                        child.Attempts.Should().Be(0);
                };

                context["when the parent Try fails"] = () =>
                {
                    int funcAttempts = default(int);

                    before = () =>
                    {
                        funcAttempts = 0;
                        subjectFunc = (a1, a2, a3, a4, a5) =>
                        {
                            funcAttempts++;
                            if (funcAttempts <= retries)
                                throw new Exception("I didn't mean it!");

                            return expectedResult;
                        };
                    };
                    it["should return the expected result"] = () =>
                            actualResult.Should().Be(expectedResult);

                    it["the parent should have tried once for each retry"] = () =>
                        subject.Attempts.Should().Be(retries);

                    it["the result should have come from the child and not from the parent"] = () =>
                    {
                        subject.As<FuncTryIt<int, double, long, string, float, string>>().HasResult.Should().BeFalse();
                        child.As<FuncTryIt<int, double, long, string, float, string>>().HasResult.Should().BeTrue();
                    };
                };
            };
        }

        void Static_Func_T1_T2_T3_T4_T5_T6_TResult_TryItMethods()
        {
            ITryAndReturnValue<string> subject = null;
            Func<int, double, long, string, float, long, string> subjectFunc = null;

            int arg1 = 23;
            double arg2 = Math.E;
            long arg3 = long.MinValue;
            string arg4 = "Happy to be here!";
            float arg5 = 373782.2378862F;
            long arg6 = long.MaxValue;

            string expectedResult = "Hi there!";
            int retries = 4;
            string actualResult = null;

            before = () =>
            {
                actualResult = null;
                subjectFunc = (i, d, l, s, f, l2) => { return expectedResult; };
            };

            act = () => subject = TryIt.Try(subjectFunc, arg1, arg2, arg3, arg4, arg5, arg6, retries);
            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, retries)"] = () =>
            {
                it["should return an ITryAndReturnValue<TResult> instance"] = () =>
                    subject.Should().NotBeNull();


                it["should set arg1 - arg6 internal properties"] = () =>
                {
                    var asTryIt = subject.As<FuncTryIt<int, double, long, string, float, long, string>>();
                    asTryIt._arg1.Should().Be(arg1);
                    asTryIt._arg2.Should().Be(arg2);
                    asTryIt._arg3.Should().Be(arg3);
                    asTryIt._arg4.Should().Be(arg4);
                    asTryIt._arg5.Should().Be(arg5);
                    asTryIt._arg6.Should().Be(arg6);
                };

            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, retries).Go()"] = () =>
            {
                act = () => actualResult = subject.Go();

                it["should return the expected result"] = () =>
                    actualResult.Should().Be(expectedResult);

                it["should attempt the Try only once"] = () =>
                    subject.Attempts.Should().Be(1);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, retries).ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, retries)"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () => child = subject.ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, retries);

                it["should return a child ITryAndReturnValue instance"] = () =>
                    child.Should().NotBeNull();

                it["the child should be distinct from the parent"] = () =>
                    child.Should().NotBe(subject);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, retries).ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, retries).Go()"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () =>
                {
                    child = subject.ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, retries);
                    actualResult = child.Go();
                };

                context["when the parent Try succeeds"] = () =>
                {
                    it["should return the expected result"] = () =>
                         actualResult.Should().Be(expectedResult);

                    it["The subject should have tried once"] = () =>
                        subject.Attempts.Should().Be(1);

                    it["The child should not have been tried"] = () =>
                        child.Attempts.Should().Be(0);
                };

                context["when the parent Try fails"] = () =>
                {
                    int funcAttempts = default(int);

                    before = () =>
                    {
                        funcAttempts = 0;
                        subjectFunc = (a1, a2, a3, a4, a5, a6) =>
                        {
                            funcAttempts++;
                            if (funcAttempts <= retries)
                                throw new Exception("I didn't mean it!");

                            return expectedResult;
                        };
                    };
                    it["should return the expected result"] = () =>
                            actualResult.Should().Be(expectedResult);

                    it["the parent should have tried once for each retry"] = () =>
                        subject.Attempts.Should().Be(retries);

                    it["the result should have come from the child and not from the parent"] = () =>
                    {
                        subject.As<FuncTryIt<int, double, long, string, float, long, string>>().HasResult.Should().BeFalse();
                        child.As<FuncTryIt<int, double, long, string, float, long, string>>().HasResult.Should().BeTrue();
                    };
                };
            };
        }

        void Static_Func_T1_T2_T3_T4_T5_T6_T7_TResult_TryItMethods()
        {
            ITryAndReturnValue<string> subject = null;
            Func<int, double, long, string, float, long, bool, string> subjectFunc = null;

            int arg1 = 23;
            double arg2 = Math.E;
            long arg3 = long.MinValue;
            string arg4 = "Happy to be here!";
            float arg5 = 373782.2378862F;
            long arg6 = long.MaxValue;
            bool arg7 = true;

            string expectedResult = "Hi there!";
            int retries = 4;
            string actualResult = null;

            before = () =>
            {
                actualResult = null;
                subjectFunc = (a1, a2, a3, a4, a5, a6, a7) => { return expectedResult; };
            };

            act = () => subject = TryIt.Try(subjectFunc, arg1, arg2, arg3, arg4, arg5, arg6, arg7, retries);

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, retries)"] = () =>
            {
                it["should return an ITryAndReturnValue<TResult> instance"] = () =>
                    subject.Should().NotBeNull();


                it["should set arg1 - arg7 internal properties"] = () =>
                {
                    var asTryIt = subject.As<FuncTryIt<int, double, long, string, float, long, bool, string>>();
                    asTryIt._arg1.Should().Be(arg1);
                    asTryIt._arg2.Should().Be(arg2);
                    asTryIt._arg3.Should().Be(arg3);
                    asTryIt._arg4.Should().Be(arg4);
                    asTryIt._arg5.Should().Be(arg5);
                    asTryIt._arg6.Should().Be(arg6);
                    asTryIt._arg7.Should().Be(arg7);
                };

            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, retries).Go()"] = () =>
            {
                act = () => actualResult = subject.Go();

                it["should return the expected result"] = () =>
                    actualResult.Should().Be(expectedResult);

                it["should attempt the Try only once"] = () =>
                    subject.Attempts.Should().Be(1);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, 7, retries).ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, arg7, retries)"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () => child = subject.ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, arg7, retries);

                it["should return a child ITryAndReturnValue instance"] = () =>
                    child.Should().NotBeNull();

                it["the child should be distinct from the parent"] = () =>
                    child.Should().NotBe(subject);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, retries).ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, arg7, retries).Go()"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () =>
                {
                    child = subject.ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, arg7, retries);
                    actualResult = child.Go();
                };

                context["when the parent Try succeeds"] = () =>
                {
                    it["should return the expected result"] = () =>
                         actualResult.Should().Be(expectedResult);

                    it["The subject should have tried once"] = () =>
                        subject.Attempts.Should().Be(1);

                    it["The child should not have been tried"] = () =>
                        child.Attempts.Should().Be(0);
                };

                context["when the parent Try fails"] = () =>
                {
                    int funcAttempts = default(int);

                    before = () =>
                    {
                        funcAttempts = 0;
                        subjectFunc = (a1, a2, a3, a4, a5, a6, a7) =>
                        {
                            funcAttempts++;
                            if (funcAttempts <= retries)
                                throw new Exception("I didn't mean it!");

                            return expectedResult;
                        };
                    };
                    it["should return the expected result"] = () =>
                            actualResult.Should().Be(expectedResult);

                    it["the parent should have tried once for each retry"] = () =>
                        subject.Attempts.Should().Be(retries);

                    it["the result should have come from the child and not from the parent"] = () =>
                    {
                        subject.As<FuncTryIt<int, double, long, string, float, long, bool, string>>().HasResult.Should().BeFalse();
                        child.As<FuncTryIt<int, double, long, string, float, long, bool, string>>().HasResult.Should().BeTrue();
                    };
                };
            };
        }

        void Static_Func_T1_T2_T3_T4_T5_T6_T7_T8_TResult_TryItMethods()
        {
            ITryAndReturnValue<string> subject = null;
            Func<int, double, long, string, float, long, bool, string, string> subjectFunc = null;

            int arg1 = 23;
            double arg2 = Math.E;
            long arg3 = long.MinValue;
            string arg4 = "Happy to be here!";
            float arg5 = 373782.2378862F;
            long arg6 = long.MaxValue;
            bool arg7 = true;
            string arg8 = "Me too!";

            string expectedResult = "Bashful";
            int retries = 4;
            string actualResult = null;

            before = () =>
            {
                actualResult = null;
                subjectFunc = (a1, a2, a3, a4, a5, a6, a7, a8) => { return expectedResult; };
            };

            act = () => subject = TryIt.Try(subjectFunc, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, retries);

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, retries)"] = () =>
            {
                it["should return an ITryAndReturnValue<TResult> instance"] = () =>
                    subject.Should().NotBeNull();


                it["should set arg1 - arg7 internal properties"] = () =>
                {
                    var asTryIt = subject.As<FuncTryIt<int, double, long, string, float, long, bool, string, string>>();
                    asTryIt._arg1.Should().Be(arg1);
                    asTryIt._arg2.Should().Be(arg2);
                    asTryIt._arg3.Should().Be(arg3);
                    asTryIt._arg4.Should().Be(arg4);
                    asTryIt._arg5.Should().Be(arg5);
                    asTryIt._arg6.Should().Be(arg6);
                    asTryIt._arg7.Should().Be(arg7);
                    asTryIt._arg8.Should().Be(arg8);
                };

            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, retries).Go()"] = () =>
            {
                act = () => actualResult = subject.Go();

                it["should return the expected result"] = () =>
                    actualResult.Should().Be(expectedResult);

                it["should attempt the Try only once"] = () =>
                    subject.Attempts.Should().Be(1);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, retries).ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, retries)"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () => child = subject.ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, retries);

                it["should return a child ITryAndReturnValue instance"] = () =>
                    child.Should().NotBeNull();

                it["the child should be distinct from the parent"] = () =>
                    child.Should().NotBe(subject);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, retries).ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, retries).Go()"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () =>
                {
                    child = subject.ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, retries);
                    actualResult = child.Go();
                };

                context["when the parent Try succeeds"] = () =>
                {
                    it["should return the expected result"] = () =>
                         actualResult.Should().Be(expectedResult);

                    it["The subject should have tried once"] = () =>
                        subject.Attempts.Should().Be(1);

                    it["The child should not have been tried"] = () =>
                        child.Attempts.Should().Be(0);
                };

                context["when the parent Try fails"] = () =>
                {
                    int funcAttempts = default(int);

                    before = () =>
                    {
                        funcAttempts = 0;
                        subjectFunc = (a1, a2, a3, a4, a5, a6, a7, a8) =>
                        {
                            funcAttempts++;
                            if (funcAttempts <= retries)
                                throw new Exception("I didn't mean it!");

                            return expectedResult;
                        };
                    };
                    it["should return the expected result"] = () =>
                            actualResult.Should().Be(expectedResult);

                    it["the parent should have tried once for each retry"] = () =>
                        subject.Attempts.Should().Be(retries);

                    it["the result should have come from the child and not from the parent"] = () =>
                    {
                        subject.As<FuncTryIt<int, double, long, string, float, long, bool, string, string>>().HasResult.Should().BeFalse();
                        child.As<FuncTryIt<int, double, long, string, float, long, bool, string, string>>().HasResult.Should().BeTrue();
                    };
                };
            };
        }

        void Static_Func_T1_T2_T3_T4_T5_T6_T7_T8_T9_TResult_TryItMethods()
        {
            ITryAndReturnValue<string> subject = null;
            Func<int, double, long, string, float, long, bool, string, int, string> subjectFunc = null;

            int arg1 = 23;
            double arg2 = Math.E;
            long arg3 = long.MinValue;
            string arg4 = "Happy to be here!";
            float arg5 = 373782.2378862F;
            long arg6 = long.MaxValue;
            bool arg7 = true;
            string arg8 = "Me too!";
            int arg9 = 42;

            string expectedResult = "Bashful";
            int retries = 4;
            string actualResult = null;

            before = () =>
            {
                actualResult = null;
                subjectFunc = (a1, a2, a3, a4, a5, a6, a7, a8, a9) => { return expectedResult; };
            };

            act = () => subject = TryIt.Try(subjectFunc, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, retries);

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, retries)"] = () =>
            {
                it["should return an ITryAndReturnValue<TResult> instance"] = () =>
                    subject.Should().NotBeNull();


                it["should set arg1 - arg7 internal properties"] = () =>
                {
                    var asTryIt = subject.As<FuncTryIt<int, double, long, string, float, long, bool, string, int, string>>();
                    asTryIt._arg1.Should().Be(arg1);
                    asTryIt._arg2.Should().Be(arg2);
                    asTryIt._arg3.Should().Be(arg3);
                    asTryIt._arg4.Should().Be(arg4);
                    asTryIt._arg5.Should().Be(arg5);
                    asTryIt._arg6.Should().Be(arg6);
                    asTryIt._arg7.Should().Be(arg7);
                    asTryIt._arg8.Should().Be(arg8);
                    asTryIt._arg9.Should().Be(arg9);
                };

            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, retries).Go()"] = () =>
            {
                act = () => actualResult = subject.Go();

                it["should return the expected result"] = () =>
                    actualResult.Should().Be(expectedResult);

                it["should attempt the Try only once"] = () =>
                    subject.Attempts.Should().Be(1);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, retries).ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, retries)"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () => child = subject.ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, retries);

                it["should return a child ITryAndReturnValue instance"] = () =>
                    child.Should().NotBeNull();

                it["the child should be distinct from the parent"] = () =>
                    child.Should().NotBe(subject);
            };

            describe["TryIt.Try(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, retries).ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, retries).Go()"] = () =>
            {
                ITryAndReturnValue<string> child = null;

                before = () => child = null;
                act = () =>
                {
                    child = subject.ThenTry(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, retries);
                    actualResult = child.Go();
                };

                context["when the parent Try succeeds"] = () =>
                {
                    it["should return the expected result"] = () =>
                         actualResult.Should().Be(expectedResult);

                    it["The subject should have tried once"] = () =>
                        subject.Attempts.Should().Be(1);

                    it["The child should not have been tried"] = () =>
                        child.Attempts.Should().Be(0);
                };

                context["when the parent Try fails"] = () =>
                {
                    int funcAttempts = default(int);

                    before = () =>
                    {
                        funcAttempts = 0;
                        subjectFunc = (a1, a2, a3, a4, a5, a6, a7, a8, a9) =>
                        {
                            funcAttempts++;
                            if (funcAttempts <= retries)
                                throw new Exception("I didn't mean it!");

                            return expectedResult;
                        };
                    };
                    it["should return the expected result"] = () =>
                            actualResult.Should().Be(expectedResult);

                    it["the parent should have tried once for each retry"] = () =>
                        subject.Attempts.Should().Be(retries);

                    it["the result should have come from the child and not from the parent"] = () =>
                    {
                        subject.As<FuncTryIt<int, double, long, string, float, long, bool, string, int, string>>().HasResult.Should().BeFalse();
                        child.As<FuncTryIt<int, double, long, string, float, long, bool, string, int, string>>().HasResult.Should().BeTrue();
                    };
                };
            };
        }
    }
}
