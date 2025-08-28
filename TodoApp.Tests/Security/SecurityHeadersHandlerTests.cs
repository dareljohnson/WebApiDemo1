using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiDemo1.Security;

namespace WebApiDemo1.Tests.Security
{
    [TestClass]
    public class SecurityHeadersHandlerTests
    {
        [TestMethod]
        public async Task SecurityHeaders_Are_Added_To_Response()
        {
            // Arrange
            var handler = new SecurityHeadersHandler
            {
                InnerHandler = new DummyHandler()
            };
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

            // Act
            var response = await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.AreEqual("nosniff", response.Headers.GetValues("X-Content-Type-Options").ToString());
            Assert.AreEqual("DENY", response.Headers.GetValues("X-Frame-Options").ToString());
            Assert.AreEqual("1; mode=block", response.Headers.GetValues("X-XSS-Protection").ToString());
            Assert.AreEqual("no-referrer", response.Headers.GetValues("Referrer-Policy").ToString());
        }

        private class DummyHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }
    }
}
