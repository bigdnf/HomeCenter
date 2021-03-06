﻿using HomeCenter.Broker;
using HomeCenter.Broker.Behaviors;
using HomeCenter.Broker.Exceptions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Tests.Dummies;
using HomeCenter.Tests.Helpers;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Tests.MessageBroker
{
    [TestClass]
    public class EventAggregatorTests : ReactiveTest
    {
        private static IEventAggregator InitAggregator()
        {
            return new EventAggregator();
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeForType_ShouldReturnProperSubscriptions()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(handler => { });
            aggregator.Subscribe<OtherMessage>(handler => { });

            var result = aggregator.GetSubscriptors(new TestMessage(), null);

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeForType_ShouldReturnAlsoDerivedTypesSubscriptions()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(handler => { });
            aggregator.Subscribe<OtherMessage>(handler => { });

            var result = aggregator.GetSubscriptors(new DerivedTestMessage());

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeWithSimpleFilter_ShouldReturnOnlySubscriptionsWithThatType()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(handler => { });
            aggregator.Subscribe<TestMessage>(handler => { }, "x");

            var result = aggregator.GetSubscriptors(new TestMessage(), "x");

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeWithSimpleFilterAndSubscriblesHaveNoFilter_ShouldReturnNone()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(handler => { });
            aggregator.Subscribe<TestMessage>(handler => { });

            var result = aggregator.GetSubscriptors(new TestMessage(), "x");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeWithNoFilter_ShouldReturnOnlyNotFilteredSubscribles()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(handler => { });
            aggregator.Subscribe<TestMessage>(handler => { }, "x");

            var result = aggregator.GetSubscriptors(new TestMessage());

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeWithStarFilter_ShouldResultAllNotfilteredAndFilteredSubscriblesByType()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(handler => { });
            aggregator.Subscribe<TestMessage>(handler => { }, "x");

            var result = aggregator.GetSubscriptors(new TestMessage(), "*");

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenMissingRoutingFilter_ShouldReturnNoElements()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<Event>(handler => { }, new RoutingFilter(new Dictionary<string, string>
            {
                [MessageProperties.MessageSource] = "Adapter"
            }));

            var result = aggregator.GetSubscriptors(new Event());

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenRoutingFilterMechSubscriptionFromOneAttribute_ShouldReturnAllMechingSubscriptions()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<Event>(handler => { }, new RoutingFilter(new Dictionary<string, string>
            {
                [MessageProperties.MessageSource] = "Adapter"
            }));

            var ev = new Event();
            ev[MessageProperties.MessageSource] = "Adapter";

            var result = aggregator.GetSubscriptors(ev);

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenRoutingFilterMechSubscriptionFromManyAttributes_ShouldReturnAllMechingSubscriptions()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<Event>(handler => { }, new RoutingFilter(new Dictionary<string, string>
            {
                [MessageProperties.MessageSource] = "Adapter",
                [MessageProperties.PinNumber] = "5"
            }));

            var ev = new Event();
            ev[MessageProperties.MessageSource] = "Adapter";
            ev[MessageProperties.PinNumber] = "5";
            ev[MessageProperties.Value] = "Value";

            var result = aggregator.GetSubscriptors(ev);

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenPublishWithRoutingKeyAndPropertiesAndSubscriberWantAll_ShouldSkipRoutingKeyCheck()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<Event>(handler => { }, new RoutingFilter("*", new Dictionary<string, string>
            {
                [MessageProperties.MessageSource] = "Adapter",
                [MessageProperties.PinNumber] = "5"
            }));

            var ev = new Event();
            ev[MessageProperties.MessageSource] = "Adapter";
            ev[MessageProperties.PinNumber] = "5";
            ev[MessageProperties.Value] = "Value";

            var result = aggregator.GetSubscriptors(ev, "x");

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task QueryAsync_WhenSubscribed_ShouldReturnProperResult()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10);
                return "Test";
            });

            var result = await aggregator.QueryAsync<TestMessage, string>(new TestMessage());

            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        public async Task QueryAsync_WhenSubscribedWithProperSimpleFilter_ShouldReturnProperResult()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10);
                return "Test";
            }, "DNF");

            var result = await aggregator.QueryAsync<TestMessage, string>(new TestMessage(), "DNF");

            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        [ExpectedException(typeof(QueryException))]
        public async Task QueryAsync_WhenTwoSubscribed_ShouldThrow()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(50);
                return "Slower";
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10);
                return "Faster";
            });

            var result = await aggregator.QueryAsync<TestMessage, string>(new TestMessage());
        }

        [TestMethod]
        public void QueryAsync_WhenSubscribedForWrongReturnType_ShouldThrowInvalidCastException()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10);
                return "Test";
            });

            AggregateExceptionHelper.AssertInnerException<InvalidCastException>(aggregator.QueryAsync<TestMessage, List<string>>(new TestMessage()));
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task QueryAsync_WhenLongRun_ShouldThrowTimeoutException()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(100);
                return "Test";
            });

            await aggregator.QueryAsync<TestMessage, string>(new TestMessage(), timeout: TimeSpan.FromMilliseconds(50));
        }

        [TestMethod]
        public void QueryAsync_WhenExceptionInHandler_ShouldCatchIt()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10);
                throw new TestException();
            });

            AggregateExceptionHelper.AssertInnerException<TestException>(aggregator.QueryAsync<TestMessage, string>(new TestMessage()));
        }

        [TestMethod]
        public async Task QueryAsync_WhenRetry_ShouldRunAgainAndSucceed()
        {
            var aggregator = InitAggregator();
            int i = 1;

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10);
                if (i-- > 0) throw new Exception("Test");
                return "OK";
            });

            var result = await aggregator.QueryAsync<TestMessage, string>(new TestMessage(), retryCount: 1);
            Assert.AreEqual("OK", result);
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task QueryAsync_WhenCanceled_ShouldThrowOperationCancel()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(50);
                return "OK";
            });

            var ts = new CancellationTokenSource();
            var result = aggregator.QueryAsync<TestMessage, string>(new TestMessage(), cancellationToken: ts.Token);
            ts.Cancel();
            await result;
        }


        [TestMethod]
        public async Task QueryAsync_Test()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<DerivedTestMessage>(async handler =>
            {
                await Task.Delay(50);
                return "OK";
            });

       
            var result = await aggregator.QueryAsync<TestMessage, string>(new TestMessage());
         
        }

        [TestMethod]
        public void IsSubscribed_WhenCheckForActiveSubscription_ShouldReturnTrue()
        {
            var aggregator = InitAggregator();

            var subscription = aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(50);
                return "OK";
            });

            var result = aggregator.IsSubscribed(subscription.Token);
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void UnSubscribe_WhenInvokedForActiveSubscription_ShouldRemoveIt()
        {
            var aggregator = InitAggregator();

            var subscription = aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(50);
                return "OK";
            });

            aggregator.UnSubscribe(subscription.Token);

            var result = aggregator.IsSubscribed(subscription.Token);
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void ClearSubscriptions_WhenInvoked_ShouldClearAllSubscriptions()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(50);
                return "OK";
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(50);
                return "OK";
            });

            aggregator.ClearSubscriptions();

            var result = aggregator.GetSubscriptors(new TestMessage(), null);
            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]
        public void QueryWithResults_WhenSubscribed_ShouldReturnProperResult()
        {
            var aggregator = InitAggregator();
            var expected = new List<string> { "Test", "Test2" };

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10);
                return expected[0];
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(30);
                return expected[1];
            });

            var subscription = aggregator.QueryWithResults<TestMessage, string>(new TestMessage());

            subscription.AssertEqual(expected.ToObservable());
        }

        [TestMethod]
        public void QueryWithResults_WhenLongRun_ShouldTimeOut()
        {
            var aggregator = InitAggregator();
            var expected = new List<string> { "Test", "Test2" };

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(100);
                return expected[1];
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(100);
                return expected[0];
            });

            var subscription = aggregator.QueryWithResults<TestMessage, string>(new TestMessage(), behaviors: new BehaviorChain().WithTimeout(TimeSpan.FromMilliseconds(10)));

            AggregateExceptionHelper.AssertInnerException<TimeoutException>(subscription);
        }

        [TestMethod]
        public void QueryWithResults_WhenCanceled_ShouldThrowOperationCanceledException()
        {
            var aggregator = InitAggregator();
            var expected = new List<string> { "Test", "Test2" };

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                message.CancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(100);
                return expected[1];
            });

            var ts = new CancellationTokenSource();
            ts.Cancel();

            var subscription = aggregator.QueryWithResults<TestMessage, string>(new TestMessage(), cancellationToken: ts.Token);

            AggregateExceptionHelper.AssertInnerException<TaskCanceledException>(subscription);
        }

        [TestMethod]
        public async Task Publish_WhenSubscribed_ShouldInvokeSubscriber()
        {
            var aggregator = InitAggregator();
            bool isWorking = false;

            aggregator.Subscribe<TestMessage>(handler =>
            {
                isWorking = true;
            });

            await aggregator.Publish(new TestMessage());

            Assert.AreEqual(true, isWorking);
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task Publish_WhenCanceled_ShouldThrowOperationCanceledException()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(message =>
            {
                message.CancellationToken.ThrowIfCancellationRequested();
            });

            var ts = new CancellationTokenSource();
            ts.Cancel();

            await aggregator.Publish(new TestMessage(), cancellationToken: ts.Token);
        }

        [TestMethod]
        public async Task QueryWithRepublishResult_WhenPublishWithResend_ShouldGetResultInSeparateSubscription()
        {
            var aggregator = InitAggregator();
            bool isWorking = false;

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10);
                return new OtherMessage();
            });

            aggregator.Subscribe<OtherMessage>((x) =>
            {
                isWorking = true;
            });

            await aggregator.QueryWithRepublishResult<TestMessage, OtherMessage>(new TestMessage());

            Assert.AreEqual(true, isWorking);
        }

        [TestMethod]
        public async Task Observe_ShouldWorkUntilDispose()
        {
            var aggregator = InitAggregator();
            int counter = 0;

            var messages = aggregator.Observe<TestMessage>();

            var subscription = messages.Subscribe(x =>
            {
                counter++;
            });

            await aggregator.Publish(new TestMessage());

            subscription.Dispose();

            await aggregator.Publish(new TestMessage());

            Assert.AreEqual(1, counter);
        }
    }
}