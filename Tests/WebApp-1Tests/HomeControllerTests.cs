using Cake.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace WebApp_1Tests {
    public class HomeControllerTests {
        [Fact]
        public void Test1() {
            HomeController sut = new HomeController();
            IActionResult result = sut.Index();
            Assert.IsType<ViewResult>(result);
        }
    }
}