using Bogus;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyBudget.Core.Models;
using MyBudget.Core.Models.Repositories;
using System;
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
            Assert.Equal("O Nome escolhido não esta disponivel!", GetValueInResult(result));
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
        }

        [Fact]
        public void Put_InvalidGroup_Assertive()
        {

        }


    }

    public class CreatingGroupDto
    {
        public string Name { get; set; }

        public string Abbr { get; set; }

        public string Sense { get; set; }
    }

    public class ChangeGroupDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Abbr { get; set; }
    }

    public class GroupsController : ControllerBase
    {
        private readonly IGroupsRepository _repository;

        public GroupsController(IGroupsRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ActionResult Post([FromBody] CreatingGroupDto dto)
        {
            var ownerId = ((ClaimsIdentity)User.Identity).FindFirst("Id").Value;

            var exists = _repository.Get(dto.Name, ownerId);
            if (exists != null)
                return BadRequest("O Nome escolhido não esta disponivel!");

            if(!Enum.TryParse<Sense>(dto.Sense, out var sense))
                return BadRequest("O fluxo escolhido é invalido!");

            var group = new Group
            {
                Name = dto.Name,
                Abbr = dto.Abbr,
                Sense = sense,
                OwnerId = ownerId
            };

            _repository.Add(group);
            return Created("", group);
        }

        [HttpPut]
        public ActionResult Put([FromBody] ChangeGroupDto dto)
        {
            var ownerId = ((ClaimsIdentity)User.Identity).FindFirst("Id").Value;

            var group = _repository.Get(dto.Id, ownerId);
            if (group == null)
                return BadRequest("Grupo não localizado!");

            group.Name = dto.Name;
            group.Abbr = dto.Abbr;

            _repository.Update(group);
            return NoContent();
        }
        
    }

}
