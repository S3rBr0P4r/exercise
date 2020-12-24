﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Udemy.NetCore5.Angular.Api.Controllers;
using Udemy.NetCore5.Angular.Api.DTOs;
using Udemy.NetCore5.Angular.Data;
using Udemy.NetCore5.Angular.Data.Entities;
using Udemy.NetCore5.Angular.Logic.Interfaces;
using Xunit;

namespace Udemy.NetCore5.Angular.Api.Tests.Controllers
{
    public class AccountControllerTests
    {
        [Fact]
        public async Task GivenAccountToRegister_WhenAccountDoesNotPreviouslyExistInTheDatabase_ThenReturnsUserToken()
        {
            // Arrange
            var dbSetMock = new List<AppUser>().AsQueryable().BuildMockDbSet();
            var dataContextOptions = new DbContextOptionsBuilder<DataContext>().Options;
            var dataContextMock = new Mock<DataContext>(dataContextOptions);
            dataContextMock.Setup(x => x.Users).Returns(dbSetMock.Object);
            var request = new RegisterUserRequest {UserName = "UserName1", Password = "Password1"};
            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock.Setup(ts => ts.CreateToken(It.Is<AppUser>(user => user.UserName == "username1"))).Returns("Here is the token");
            var testee = new AccountController(dataContextMock.Object, tokenServiceMock.Object);

            // Act
            var result = await testee.Register(request).ConfigureAwait(false);

            // Assert
            result.Result.Should().BeNull("so far, in the course the result is not ok");
            result.Value.Should().NotBeNull("the user must be registered");
            result.Value.UserName.Should().Be("username1", "the username UserName1 must be registered on lowercase");
            result.Value.Token.Should().Be("Here is the token", "the token must be stored in the response");
        }

        [Fact]
        public async Task GivenAccountToRegister_WhenAccountDoesNotPreviouslyExistInTheDatabase_ThenUserIsStoredInDatabase()
        {
            // Arrange
            var dbSetMock = new List<AppUser>().AsQueryable().BuildMockDbSet();
            var dataContextOptions = new DbContextOptionsBuilder<DataContext>().Options;
            var dataContextMock = new Mock<DataContext>(dataContextOptions);
            dataContextMock.Setup(x => x.Users).Returns(dbSetMock.Object);
            var request = new RegisterUserRequest { UserName = "UserName1", Password = "Password1" };
            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock.Setup(ts => ts.CreateToken(It.Is<AppUser>(user => user.UserName == "username1"))).Returns("Here is the token");
            var testee = new AccountController(dataContextMock.Object, tokenServiceMock.Object);

            // Act
            await testee.Register(request).ConfigureAwait(false);

            // Assert
            dbSetMock.Verify(dbs => dbs.AddAsync(It.Is<AppUser>(user => user.UserName == "username1"), It.IsAny<CancellationToken>()), Times.Once);
            dataContextMock.Verify(dc => dc.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GivenAccountToRegister_WhenAccountPreviouslyExistsInTheDatabase_ThenReturnsBadRequest()
        {
            // Arrange
            var dbSetMock = new List<AppUser> {new AppUser {UserName = "username1"}}.AsQueryable().BuildMockDbSet();
            var dataContextOptions = new DbContextOptionsBuilder<DataContext>().Options;
            var dataContextMock = new Mock<DataContext>(dataContextOptions);
            dataContextMock.Setup(x => x.Users).Returns(dbSetMock.Object);
            var request = new RegisterUserRequest { UserName = "UserName1", Password = "Password1" };
            var tokenServiceMock = new Mock<ITokenService>();
            var testee = new AccountController(dataContextMock.Object, tokenServiceMock.Object);

            // Act
            var result = await testee.Register(request).ConfigureAwait(false);

            // Assert
            result.Value.Should().BeNull("the user 'UserName1' already exists in the database");
            result.Result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be("Username is taken");
        }
    }
}