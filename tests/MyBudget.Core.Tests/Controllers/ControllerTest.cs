using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MyBudget.Core.Tests.Controllers
{
    public abstract class ControllerTest
    {
        protected string UserId { get; set; }

        public ControllerTest()
        {
            UserId = Guid.NewGuid().ToString();
        }

        protected void AddControllerContext(ControllerBase controller)
        {
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(x => x.User).Returns(GetIdentity());

            controller.ControllerContext = new ControllerContext() {
                HttpContext = mockHttpContext.Object
            };
        }

        private ClaimsPrincipal GetIdentity()
        {
            List<Claim> claims = new();
            claims.Add(new Claim("Id", UserId));

            ClaimsIdentity identity = new ClaimsIdentity(claims);
            return new ClaimsPrincipal(identity);
        }

        protected object GetValueInResult(ActionResult result)
        {
            object value = new object { };

            PropertyInfo[] properties = result.GetType().GetProperties();
            foreach (var property in properties)
            {
                if(property.Name == "Value")
                {
                    value = property.GetValue(result) ?? new object { };
                }
            }

            return value;
        }

        protected void AssertByProperties(object expected, object actual)
        {
            Assert.Equal(expected.GetType(), actual.GetType());
            var properties = expected.GetType().GetProperties();

            foreach (var property in properties)
            {
                var a = property.GetValue(expected);
                var b = property.GetValue(actual);
                Assert.Equal(a, b);
            }
        }
    }
}
