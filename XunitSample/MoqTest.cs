using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using WeihanLi.Common;
using WeihanLi.Common.Services;
using Xunit;

namespace XunitSample
{
    public class MoqTest
    {
        [Fact]
        public void BasicTest()
        {
            var userIdProviderMock = new Mock<IUserIdProvider>();
            userIdProviderMock.Setup(x => x.GetUserId())
                .Returns("mock");

            Assert.Equal("mock", userIdProviderMock.Object.GetUserId());
        }

        [Fact]
        public void MethodParameterMatch()
        {
            var repositoryMock = new Mock<IRepository>();
            repositoryMock.Setup(x => x.Delete(It.IsAny<int>()))
                .Returns(true);
            repositoryMock.Setup(x => x.GetById(It.Is<int>(_ => _ > 0)))
                .Returns((int id) => new TestModel()
                {
                    Id = id
                });

            var service = new TestService(repositoryMock.Object);
            var deleted = service.Delete(new TestModel());
            Assert.True(deleted);

            var result = service.GetById(1);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);

            result = service.GetById(-1);
            Assert.Null(result);

            repositoryMock.Setup(x => x.GetById(It.Is<int>(_ => _ <= 0)))
                .Returns(new TestModel()
                {
                    Id = -1
                });
            result = service.GetById(0);
            Assert.NotNull(result);
            Assert.Equal(-1, result.Id);
        }

        [Fact]
        public async Task AsyncMethod()
        {
            var repositoryMock = new Mock<IRepository>();

            // Async method mock
            repositoryMock.Setup(x => x.GetCountAsync())
                .Returns(Task.FromResult(10));
            repositoryMock.Setup(x => x.GetCountAsync())
                .ReturnsAsync(10);
            // start from 4.16
            repositoryMock.Setup(x => x.GetCountAsync().Result)
                .Returns(10);

            var service = new TestService(repositoryMock.Object);
            var result = await service.GetCountAsync();
            Assert.True(result > 0);
        }

        [Fact]
        public void Property()
        {
            var repositoryMock = new Mock<IRepository>();
            var service = new TestService(repositoryMock.Object);
            repositoryMock.Setup(x => x.Version).Returns(1);
            Assert.Equal(1, service.Version);

            service.Version = 2;
            Assert.Equal(1, service.Version);
        }

        [Fact]
        public void PropertyTracking()
        {
            var repositoryMock = new Mock<IRepository>();
            var service = new TestService(repositoryMock.Object);
            repositoryMock.SetupProperty(x => x.Version, 1);
            Assert.Equal(1, service.Version);

            service.Version = 2;
            Assert.Equal(2, service.Version);
        }

        [Fact]
        public void Callback()
        {
            var deletedIds = new List<int>();
            var repositoryMock = new Mock<IRepository>();
            var service = new TestService(repositoryMock.Object);
            repositoryMock.Setup(x => x.Delete(It.IsAny<int>()))
                .Callback((int id) =>
                {
                    deletedIds.Add(id);
                })
                .Returns(true);

            for (var i = 0; i < 10; i++)
            {
                service.Delete(new TestModel() { Id = i });
            }
            Assert.Equal(10, deletedIds.Count);
            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(i, deletedIds[i]);
            }
        }

        [Fact]
        public void Verification()
        {
            var repositoryMock = new Mock<IRepository>();
            var service = new TestService(repositoryMock.Object);

            service.Delete(new TestModel()
            {
                Id = 1
            });

            repositoryMock.Verify(x => x.Delete(1));
        }

        [Fact]
        public void Sequence()
        {
            var repositoryMock = new Mock<IRepository>();
            var service = new TestService(repositoryMock.Object);

            repositoryMock.SetupSequence(x => x.GetCount())
                .Returns(1)
                .Returns(2)
                .Returns(3)
                .Throws(new InvalidOperationException());

            Assert.Equal(1, service.GetCount());
            Assert.Equal(2, service.GetCount());
            Assert.Equal(3, service.GetCount());
            Assert.Throws<InvalidOperationException>(() => service.GetCount());
        }

        [Fact]
        public void GenericType()
        {
            var repositoryMock = new Mock<IRepository>();
            var service = new TestService(repositoryMock.Object);

            repositoryMock.Setup(x => x.GetResult<int>(It.IsAny<string>()))
                .Returns(1);
            Assert.Equal(1, service.GetResult(""));

            repositoryMock.Setup(x => x.GetResult<string>(It.IsAny<string>()))
                .Returns("test");
            Assert.Equal("test", service.GetResult<string>(""));
        }

        [Fact]
        public void GenericTypeMatch()
        {
            var repositoryMock = new Mock<IRepository>();
            var service = new TestService(repositoryMock.Object);

            repositoryMock.Setup(m => m.GetNum<It.IsAnyType>())
                .Returns(-1);
            repositoryMock.Setup(m => m.GetNum<It.IsSubtype<TestModel>>())
                .Returns(0);
            repositoryMock.Setup(m => m.GetNum<string>())
                .Returns(1);
            repositoryMock.Setup(m => m.GetNum<int>())
                .Returns(2);

            Assert.Equal(0, service.GetNum<TestModel>());
            Assert.Equal(1, service.GetNum<string>());
            Assert.Equal(2, service.GetNum<int>());
            Assert.Equal(-1, service.GetNum<byte>());
        }

        [Fact]
        public void MockLinq()
        {
            var services = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(IRepository)) == Mock.Of<IRepository>(r => r.Version == 1) &&
                sp.GetService(typeof(IUserIdProvider)) == Mock.Of<IUserIdProvider>(a => a.GetUserId() == "test"));

            Assert.Equal(1, services.ResolveService<IRepository>().Version);
            Assert.Equal("test", services.ResolveService<IUserIdProvider>().GetUserId());
        }

        [Fact]
        public void MockBehaviorTest()
        {
            // Make mock behave like a "true Mock", raising exceptions for anything that doesn't have a corresponding expectation: in Moq slang a "Strict" mock;
            // default behavior is "Loose" mock, which never throws and returns default values or empty arrays, enumerable, etc

            var repositoryMock = new Mock<IRepository>();
            var service = new TestService(repositoryMock.Object);
            Assert.Equal(0, service.GetCount());
            Assert.Null(service.GetList());

            repositoryMock = new Mock<IRepository>(MockBehavior.Strict);
            Assert.Throws<MockException>(() => new TestService(repositoryMock.Object).GetCount());
        }
    }

    public class TestModel
    {
        public int Id { get; set; }
    }

    public interface IRepository
    {
        int Version { get; set; }

        int GetCount();

        Task<int> GetCountAsync();

        TestModel GetById(int id);

        List<TestModel> GetList();

        TResult GetResult<TResult>(string sql);

        int GetNum<T>();

        bool Delete(int id);
    }

    public class TestService
    {
        private readonly IRepository _repository;

        public TestService(IRepository repository)
        {
            _repository = repository;
        }

        public int Version
        {
            get => _repository.Version;
            set => _repository.Version = value;
        }

        public List<TestModel> GetList() => _repository.GetList();

        public TResult GetResult<TResult>(string sql) => _repository.GetResult<TResult>(sql);

        public int GetResult(string sql) => _repository.GetResult<int>(sql);

        public int GetNum<T>() => _repository.GetNum<T>();

        public int GetCount() => _repository.GetCount();

        public Task<int> GetCountAsync() => _repository.GetCountAsync();

        public TestModel GetById(int id) => _repository.GetById(id);

        public bool Delete(TestModel model) => _repository.Delete(model.Id);
    }
}
