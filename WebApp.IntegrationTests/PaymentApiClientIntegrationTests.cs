using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.ApiClients;
using WebApp.DTOs;

namespace WebApp.IntegrationTests
{
    public class PaymentApiClientIntegrationTests : IntegrationTestBase
    {
        private readonly PaymentApiClient _paymentApiClient;
        private readonly OrderApiClient _orderApiClient;
        public PaymentApiClientIntegrationTests()
        {
            _paymentApiClient = new PaymentApiClient(HttpClient, Logger);
            _orderApiClient = new OrderApiClient(HttpClient, Logger);
        }

        [Fact]
        public async Task GetAllPayments_ReturnsPayments()
        {
            var payments = await _paymentApiClient.GetAllPayments();
            Assert.NotNull(payments);
            Assert.True(payments.Count > 0);
        }


        [Fact]
        public async Task CreatePaymentAsync()
        {
            //prvo moramo napraviti order i vezati ga za payment, zbog orderId koji moze ici na samo jedan payment
            var createOrder = new OrderDto
            {
                OrderDate = DateTime.Now,
                Status = (int)OrderStatus.Paid,
                TotalAmount = 9,
                TableId = 2
            };

            var createdOrder = await _orderApiClient.CreateOrder(createOrder);

            Assert.NotNull(createdOrder);
            Assert.True(createdOrder.Id > 0); 

            var payment = new PaymentDto
            {
                Amount = createdOrder.TotalAmount,
                Method = "CreditCard",
                PaymentDate = createdOrder.OrderDate,
                OrderId = createdOrder.Id 
            };

            var createdPayment = await _paymentApiClient.CreatePaymentAsync(payment);

            Assert.NotNull(createdPayment); 
            Assert.Equal(payment.Amount, createdPayment?.Amount); 
            Assert.Equal(payment.Method, createdPayment?.Method); 
            Assert.Equal(payment.OrderId, createdPayment?.OrderId); 
            Assert.True(createdPayment?.Id > 0); 
        }

        [Fact]
        public async Task CreatePaymentFailWithWrongData()
        {

            var invalidPayment = new PaymentDto
            {
                Amount = -100.0M,
                Method = "InvalidMethod",
                PaymentDate = DateTime.Now,
                OrderId = 9999 
            };

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await _paymentApiClient.CreatePaymentAsync(invalidPayment);
            });
        }
    }
}
