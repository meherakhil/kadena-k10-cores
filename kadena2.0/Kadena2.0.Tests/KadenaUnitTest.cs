﻿using Moq;
using Moq.AutoMock;
using System;
using System.Linq.Expressions;

namespace Kadena.Tests
{
    public abstract class KadenaUnitTest<TSut> where TSut : class
    {
        private readonly AutoMocker autoMocker = new AutoMocker();

        protected TSut Sut => autoMocker.CreateInstance<TSut>();

        protected void Setup<TService, TResult>(Expression<Func<TService, TResult>> setupAction, TResult result) where TService : class
        {
            autoMocker.Setup(setupAction).Returns(result);
        }

        protected void Setup<TService, T, TResult>(Expression<Func<TService, TResult>> setupAction, Func<T, TResult> result) where TService : class
        {
            autoMocker.Setup(setupAction).Returns(result);
        }

        protected void Setup<TService, T1, T2, TResult>(Expression<Func<TService, TResult>> setupAction, Func<T1, T2, TResult> result) where TService : class
        {
            autoMocker.Setup(setupAction).Returns(result);
        }

        protected void Verify<TService>(Expression<Action<TService>> verifyAction, Func<Times> times) where TService : class
        {
            autoMocker.Verify(verifyAction, times);
        }

        protected void Verify<TService>(Expression<Action<TService>> verifyAction, Times times) where TService : class
        {
            autoMocker.Verify(verifyAction, times);
        }

        protected void VerifyNoOtherCalls<TService>() where TService : class
        {
            autoMocker.GetMock<TService>().VerifyNoOtherCalls();
        }

        protected void Use<TMock, TService>() 
            where TMock : class 
            where TService : class, TMock
        {
            Use<TMock>(autoMocker.CreateInstance<TService>());
        }

        protected void Use<TService>(TService service) where TService : class
        {
            autoMocker.Use(service);
        }
    }
}