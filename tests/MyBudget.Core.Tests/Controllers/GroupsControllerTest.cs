using Bogus;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyBudget.Core.Controllers;
using MyBudget.Core.Dtos.Groups;
using MyBudget.Core.Models;
using MyBudget.Core.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace MyBudget.Core.Tests.Controllers
{
    public class GroupsControllerTest : ControllerTest
    {
        private readonly Faker _faker;

        private readonly Mock<IGroupsRepository> _mockRepository;

        public GroupsControllerTest()
            : base()
        {
            _faker = new Faker("pt_BR");
            _mockRepository = new Mock<IGroupsRepository>();
        }
        
        [Fact]
        public void Post_AddGroup_Assertive()
        {
            //Arrange
            CreatingGroupDto dto = new CreatingGroupDto
            {
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Sense = "Entry"
            };

            _mockRepository.Setup(r => r.Add(It.IsAny<Group>()))
                           .Verifiable();

            //Act
            var controller = new GroupsController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Post(dto);

            //Assert
            Assert.IsType<CreatedResult>(result);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Post_InvalidName_Exception()
        {
            //Arrange
            CreatingGroupDto dto = new CreatingGroupDto
            {
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Sense = "Out"
            };

            _mockRepository.Setup(x => x.Get(It.Is<string>(s => s.Equals(dto.Name)), It.Is<string>(s => s.Equals(UserId))))
                           .Returns(new Group())
                           .Verifiable();

            //Act
            var controller = new GroupsController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Post(dto);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Você já possui um grupo com esse nome!", GetValueInResult(result));
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Post_InvalidSense_Exception()
        {
            //Arrange
            CreatingGroupDto dto = new CreatingGroupDto
            {
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Sense = "Teste"
            };

            //Act
            var controller = new GroupsController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Post(dto);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("O fluxo escolhido é invalido!", GetValueInResult(result));
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Put_EditGroup_Assertive()
        {
            //Arrange
            ChangeGroupDto dto = new ChangeGroupDto
            {
                Id = _faker.Random.Int(1),
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word()
            };

            Group group = new Group
            {
                Id = dto.Id,
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word(),
                Sense = Sense.Entry,
                OwnerId = UserId
            };

            _mockRepository.Setup(x => x.Get(It.Is<int>(s => s.Equals(dto.Id)), It.Is<string>(s => s.Equals(UserId))))
                           .Returns(group)
                           .Verifiable();

            _mockRepository.Setup(r => r.Update(It.IsAny<Group>()))
                           .Verifiable();

            //Act
            var controller = new GroupsController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Put(dto);

            //Assert
            Assert.IsType<NoContentResult>(result);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Put_NotFound_Exception()
        {
            //Arrange
            ChangeGroupDto dto = new ChangeGroupDto
            {
                Id = _faker.Random.Int(1),
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word()
            };

            //Act
            var controller = new GroupsController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Put(dto);

            //Assert
            Assert.IsType<NotFoundResult>(result);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Put_InvalidName_Exception()
        {
            //Arrange
            ChangeGroupDto dto = new ChangeGroupDto
            {
                Id = _faker.Random.Int(1),
                Name = _faker.Random.Word(),
                Abbr = _faker.Random.Word()
            };

            Group group = new Group() { Id = 0 };

            _mockRepository.Setup(
                x => x.Get(
                    It.Is<string>(s => s.Equals(dto.Name)),
                    It.Is<string>(x => x.Equals(UserId))))
                .Returns(group)
                .Verifiable();

            //Act
            var controller = new GroupsController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Put(dto);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Você já possui um grupo com esse nome!", GetValueInResult(result));
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Delete_RemoveGroup_Assertive()
        {
            //Arrange 
            int id = _faker.Random.Int(1);
            var group = new Group() { Id = 0 };
            _mockRepository.Setup(x => x.Delete(It.Is<int>(x => x.Equals(id))))
                .Returns(group)
                .Verifiable();

            //Act
            var controller = new GroupsController(_mockRepository.Object);
            ActionResult result = controller.Delete(id);

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(group, GetValueInResult(result));
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Delete_WithDependencies_Exception()
        {
            //Arrange 
            int id = _faker.Random.Int(1);
            var group = new Group() 
            { 
                Id = 0, 
                Categories = new List<Category> { new Category { Id = 0 } } 
            };

            _mockRepository.Setup(
                x => x.Get(
                    It.Is<int>(x => x.Equals(id)),
                    It.Is<string>(x => x.Equals(UserId))
                    ))
                .Returns(group)
                .Verifiable();

            //Act
            var controller = new GroupsController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Delete(id);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Este grupo não pode ser removido, pois possui dependentes", GetValueInResult(result));
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void Delete_NotFound_Exception()
        {
            //Arrange 
            int id = _faker.Random.Int(1);

            //Act
            var controller = new GroupsController(_mockRepository.Object);
            base.AddControllerContext(controller);

            ActionResult result = controller.Delete(id);

            //Assert
            Assert.IsType<NotFoundResult>(result);
            _mockRepository.VerifyAll();

        }

    }
}
