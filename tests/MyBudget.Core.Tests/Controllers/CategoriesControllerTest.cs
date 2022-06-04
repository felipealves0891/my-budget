using Bogus;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyBudget.Core.Controllers;
using MyBudget.Core.Dtos.Categories;
using MyBudget.Core.Models;
using MyBudget.Core.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MyBudget.Core.Tests.Controllers
{
    public class CategoriesControllerTest : ControllerTest
    {
        private Mock<ICategoriesRepository> _mockRepository;

        private Faker _faker;

        public CategoriesControllerTest()
        {
            _mockRepository = new Mock<ICategoriesRepository>();
            _faker = new Faker();
        }

        [Fact]
        public void Post_AddCategory_Assertive()
        {
            //Arrange
            var dto = new CreatingCategoryDto
            {
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Flow = "Out",
                GroupId = _faker.Random.Int(1)
            };

            var category = new Category
            {
                Name = dto.Name,
                Abbr = dto.Abbr,
                Flow = Flow.Out,
                GroupId = dto.GroupId,
                Group = new Group { Id = dto.GroupId, Flow = Flow.Out },
                OwnerId = UserId
            };

            _mockRepository.Setup(
                x => x.GetGroupById(
                    It.Is<int>(x => x.Equals(dto.GroupId)),
                    It.Is<string>(x => x.Equals(UserId))
                    ))
                .Returns(category.Group)
                .Verifiable();

            _mockRepository.Setup(x => x.Add(It.IsAny<Category>()))
                           .Verifiable();

            //Act
            var controller = new CategoriesController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Post(dto);

            //Assert
            Assert.IsType<CreatedResult>(result);
            base.AssertByProperties(category, GetValueInResult(result));
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Post_GroupNotFound_Exception()
        {
            //Arrange
            var dto = new CreatingCategoryDto
            {
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Flow = "Out",
                GroupId = _faker.Random.Int(1)
            };

            //Act
            var controller = new CategoriesController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Post(dto);

            //Assert
            Assert.IsType<NotFoundResult>(result);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Post_InvalidName_Exception()
        {
            //Arrange
            var dto = new CreatingCategoryDto
            {
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Flow = "Out",
                GroupId = _faker.Random.Int(1)
            };

            _mockRepository.Setup(
                x => x.Get(
                    It.Is<string>(s => s.Equals(dto.Name)),
                    It.Is<string>(s => s.Equals(UserId))
                    ))
                .Returns(new Category { Id = 0 })
                .Verifiable();

            //Act
            var controller = new CategoriesController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Post(dto);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Você já possui um grupo com esse nome!", GetValueInResult(result));
            _mockRepository.VerifyAll();

        }

        [Fact]
        public void Post_InvalidFlow_Exception()
        {
            //Arrange
            var dto = new CreatingCategoryDto
            {
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Flow = "Saida",
                GroupId = _faker.Random.Int(1)
            };

            //Act
            var controller = new CategoriesController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Post(dto);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("O Fluxo é invalido!", GetValueInResult(result));
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Post_FlowDifferentFromGroup_Exception()
        {
            //Arrange
            var dto = new CreatingCategoryDto
            {
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Flow = "Out",
                GroupId = _faker.Random.Int(1)
            };

            _mockRepository.Setup(
                x => x.GetGroupById(
                    It.Is<int>(x => x.Equals(dto.GroupId)),
                    It.Is<string>(x => x.Equals(UserId))
                    ))
                .Returns(new Group { Flow = Flow.In })
                .Verifiable();

            //Act
            var controller = new CategoriesController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Post(dto);

            //Act
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("O Fluxo esta no sentido invalido!", GetValueInResult(result));
            _mockRepository.VerifyAll();

        }

        [Fact]
        public void Put_EditCategory_Assertive()
        {
            //Arrange
            var dto = new ChangeCategoryDto
            {
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Id = _faker.Random.Int(1)
            };

            var category = new Category
            {
                Id = dto.Id,
                Name = dto.Name,
                Abbr = dto.Abbr,
                Flow = Flow.Out,
                GroupId = _faker.Random.Int(1),
                OwnerId = UserId
            };

            _mockRepository.Setup(
                x => x.Get(
                    It.Is<int>(s => s.Equals(dto.Id)),
                    It.Is<string>(s => s.Equals(UserId))
                    ))
                .Returns(category)
                .Verifiable();

            //Act
            var controller = new CategoriesController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Put(dto);

            //Act
            Assert.IsType<NoContentResult>(result);
            _mockRepository.VerifyAll();

        }

        [Fact]
        public void Put_NotFound_Exception()
        {
            //Arrange
            var dto = new ChangeCategoryDto
            {
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Id = _faker.Random.Int(1)
            };

            //Act
            var controller = new CategoriesController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Put(dto);

            //Act
            Assert.IsType<NotFoundResult>(result);
            _mockRepository.VerifyAll();

        }

        [Fact]
        public void Put_InvalidName_Exception()
        {
            //Arrange
            var dto = new ChangeCategoryDto
            {
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Id = _faker.Random.Int(1)
            };

            var category = new Category
            {
                Id = dto.Id,
                Name = dto.Name,
                Abbr = dto.Abbr,
                Flow = Flow.Out,
                GroupId = _faker.Random.Int(1),
                OwnerId = UserId
            };

            _mockRepository.Setup(
                x => x.Get(
                    It.Is<int>(s => s.Equals(dto.Id)),
                    It.Is<string>(s => s.Equals(UserId))
                    ))
                .Returns(category)
                .Verifiable();

            _mockRepository.Setup(
                x => x.Get(
                    It.Is<string>(s => s.Equals(dto.Name)),
                    It.Is<string>(s => s.Equals(UserId))
                    ))
                .Returns(new Category { Id = 0 })
                .Verifiable();

            //Act
            var controller = new CategoriesController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Put(dto);

            //Act
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Você já possui um grupo com esse nome!", GetValueInResult(result));
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Delete_RemoveCategory_Assertive()
        {
            //Arrange
            var category = new Category
            {
                Id = _faker.Random.Int(1),
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Flow = Flow.Out,
                GroupId = _faker.Random.Int(1),
                OwnerId = UserId
            };

            _mockRepository.Setup(
                x => x.Get(
                    It.Is<int>(s => s.Equals(category.Id)),
                    It.Is<string>(s => s.Equals(UserId))
                    ))
                .Returns(category)
                .Verifiable();

            //Act
            var controller = new CategoriesController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Delete(category.Id);

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(category, GetValueInResult(result));
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Delete_NotFound_Exception()
        {
            //Arrange
            int id = _faker.Random.Int(1);

            //Act
            var controller = new CategoriesController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Delete(id);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_WithDependencies_Exception()
        {
            //Arrange
            var category = new Category
            {
                Id = _faker.Random.Int(1),
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Flow = Flow.Out,
                GroupId = _faker.Random.Int(1),
                Cashes = new List<Cash> { new Cash { Id = 0 } },
                OwnerId = UserId
            };

            _mockRepository.Setup(
                x => x.Get(
                    It.Is<int>(s => s.Equals(category.Id)),
                    It.Is<string>(s => s.Equals(UserId))
                    ))
                .Returns(category)
                .Verifiable();

            //Act
            var controller = new CategoriesController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Delete(category.Id);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Esta categoria não pode ser removida, pois possui dependentes", GetValueInResult(result));
            _mockRepository.VerifyAll();
        }
    }

}
