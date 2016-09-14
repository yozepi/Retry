﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using TryIt.Tests.Unit.specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryIt.Tests.Unit.Tests
{
    [TestClass]
    public class Tests : nSpecTestHarness
    {
        //[TestMethod]
        //public void BaseBuilder_Tests()
        //{
        //    this.LoadSpecs(() =>
        //    {
        //        Type[] types = { typeof(BaseBuilder_Methods) };
        //        return types;
        //    });
        //    this.RunSpecs();
        //}


        //[TestMethod]
        //public void Action_Tests()
        //{
        //    this.LoadSpecs(() =>
        //    {
        //        Type[] types = { typeof(Retry_Actions), typeof(Retry_actions_ASYNC) };
        //        return types;
        //    });
        //    this.RunSpecs();
        //}


        //[TestMethod]
        //public void Action_Extension_Tests()
        //{
        //    this.LoadSpecs(() =>
        //    {
        //        Type[] types = { typeof(Action_Extension_Methods) };
        //        return types;
        //    });
        //    this.RunSpecs();
        //}


        //[TestMethod]
        //public void Func_Tests()
        //{
        //    this.LoadSpecs(() =>
        //    {
        //        Type[] types = { typeof(Retry_Funcs), typeof(Retry_Funcs_ASYNC), typeof(Retry_WithAlternate_Funcs) };
        //        return types;
        //    });
        //    this.RunSpecs();
        //}


        //[TestMethod]
        //public void Func_Task_Tests()
        //{
        //    this.LoadSpecs(() =>
        //    {
        //        Type[] types = { typeof(Retry_Tasks), typeof(Retry_TaskTResults) };
        //        return types;
        //    });
        //    this.RunSpecs();
        //}


        //[TestMethod]
        //public void Func_Extension_Tests()
        //{
        //    this.LoadSpecs(() =>
        //    {
        //        Type[] types = { typeof(Func_Extension_Methods) };
        //        return types;
        //    });
        //    this.RunSpecs();
        //}


        //[TestMethod]
        //public void IDelay_Implementor_Tests()
        //{
        //    this.LoadSpecs(() =>
        //    {
        //        Type[] types = { typeof(Delay_Implementors) };
        //        return types;
        //    });
        //    this.RunSpecs();
        //}


        [TestMethod]
        public void Runner_Tests()
        {
            this.LoadSpecs(() =>
            {
                Type[] types = { typeof(BaseRunner_Methods) };
                return types;
            });
            this.RunSpecs();
        }

    }
}
