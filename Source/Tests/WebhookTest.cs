﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PayPal.Api;
using System.Collections.Generic;

namespace PayPal.Testing
{
    [TestClass]
    public class WebhookTest
    {
        public static readonly string WebhookJson =
            "{\"url\":\"https://www.paypal.com/paypal_webhook\"," +
            "\"event_types\":[" +
            WebhookEventTypeTest.WebhookEventTypeJsonCreated + "," +
            WebhookEventTypeTest.WebhookEventTypeJsonVoided + "]}";

        public static Webhook GetWebhook()
        {
            return JsonFormatter.ConvertFromJson<Webhook>(WebhookJson);
        }

        [TestMethod, TestCategory("Unit")]
        public void WebhookObjectTest()
        {
            var testObject = GetWebhook();
        }

        [TestMethod, TestCategory("Unit")]
        public void WebhookConvertToJsonTest()
        {
            Assert.IsFalse(GetWebhook().ConvertToJson().Length == 0);
        }

        [TestMethod, TestCategory("Unit")]
        public void WebhookToStringTest()
        {
            Assert.IsFalse(GetWebhook().ToString().Length == 0);
        }

        [TestMethod, TestCategory("Functional")]
        public void WebhookCreateAndGetTest()
        {
            try
            {
                var apiContext = TestingUtil.GetApiContext();
                var webhook = WebhookTest.GetWebhook();
                var url = "https://" + Guid.NewGuid().ToString() + ".com/paypal_webhooks";
                webhook.url = url;
                var createdWebhook = webhook.Create(apiContext);
                Assert.IsNotNull(createdWebhook);
                Assert.IsTrue(!string.IsNullOrEmpty(createdWebhook.id));

                var webhookId = createdWebhook.id;
                var retrievedWebhook = Webhook.Get(apiContext, webhookId);
                Assert.IsNotNull(retrievedWebhook);
                Assert.AreEqual(webhookId, retrievedWebhook.id);
                Assert.AreEqual(url, retrievedWebhook.url);

                // Cleanup
                retrievedWebhook.Delete(apiContext);
            }
            catch (ConnectionException ex)
            {
                TestingUtil.WriteConnectionExceptionDetails(ex);
                throw;
            }
        }

        [TestMethod, TestCategory("Functional")]
        public void WebhookGetListTest()
        {
            try
            {
                var webhookList = Webhook.GetAll(TestingUtil.GetApiContext());
                Assert.IsNotNull(webhookList);
                Assert.IsNotNull(webhookList.webhooks);
            }
            catch (ConnectionException ex)
            {
                TestingUtil.WriteConnectionExceptionDetails(ex);
                throw;
            }
        }

        [TestMethod, TestCategory("Functional")]
        public void WebhookUpdateTest()
        {
            try
            {
                var webhook = WebhookTest.GetWebhook();
                webhook.url = "https://" + Guid.NewGuid().ToString() + ".com/paypal_webhooks";
                var createdWebhook = webhook.Create(TestingUtil.GetApiContext());

                var newUrl = "https://update.com/paypal_webhooks/" + Guid.NewGuid().ToString();
                var newEventTypeName = "PAYMENT.SALE.REFUNDED";

                var patchRequest = new PatchRequest
                {
                    new Patch
                    {
                        op = "replace",
                        path = "/url",
                        value = newUrl
                    },
                    new Patch
                    {
                        op = "replace",
                        path = "/event_types",
                        value = new List<WebhookEventType>
                        {
                            new WebhookEventType
                            {
                                name = newEventTypeName
                            }
                        }
                    }
                };

                var updatedWebhook = createdWebhook.Update(TestingUtil.GetApiContext(), patchRequest);
                Assert.IsNotNull(updatedWebhook);
                Assert.AreEqual(createdWebhook.id, updatedWebhook.id);
                Assert.AreEqual(newUrl, updatedWebhook.url);
                Assert.IsNotNull(updatedWebhook.event_types);
                Assert.AreEqual(1, updatedWebhook.event_types.Count);
                Assert.AreEqual(newEventTypeName, updatedWebhook.event_types[0].name);

                // Cleanup
                updatedWebhook.Delete(TestingUtil.GetApiContext());
            }
            catch (ConnectionException ex)
            {
                TestingUtil.WriteConnectionExceptionDetails(ex);
                throw;
            }
        }

        [TestMethod, TestCategory("Functional")]
        public void WebhookDeleteTest()
        {
            try
            {
                var webhook = WebhookTest.GetWebhook();
                webhook.url = "https://" + Guid.NewGuid().ToString() + ".com/paypal_webhooks";
                var createdWebhook = webhook.Create(TestingUtil.GetApiContext());
                createdWebhook.Delete(TestingUtil.GetApiContext());
                TestingUtil.AssertThrownException<HttpException>(() => Webhook.Get(TestingUtil.GetApiContext(), createdWebhook.id));
            }
            catch (ConnectionException ex)
            {
                TestingUtil.WriteConnectionExceptionDetails(ex);
                throw;
            }
        }
    }
}
