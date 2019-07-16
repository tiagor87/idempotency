using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Idempotency.Core;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Idempotency.Redis.UnitTests
{
    public class IdempotencyRepositoryTests
    {
        public IdempotencyRepositoryTests()
        {
            _redisDatabaseMock = new Mock<IDatabase>();
            _serializerMock = new Mock<IIdempotencySerializer>();
            _repository = new IdempotencyRepository(_redisDatabaseMock.Object, _serializerMock.Object);
        }

        private readonly Mock<IDatabase> _redisDatabaseMock;
        private readonly Mock<IIdempotencySerializer> _serializerMock;
        private readonly IIdempotencyRepository _repository;

        [Trait("Category", "Redis")]
        [Fact(DisplayName = "GIVEN Idempotency-Key, WHEN add fails, SHOULD return false")]
        public async Task GivenIdempotencyKeyWhenAddFailsShouldReturnFalse()
        {
            var key = Guid.NewGuid().ToString();
            var connectionMock = new Mock<IConnectionMultiplexer>();
            var lockDatabaseMock = new Mock<IDatabase>();

            connectionMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(lockDatabaseMock.Object)
                .Verifiable();
            lockDatabaseMock.Setup(x => x.StringSet(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(),
                    It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .Returns(false)
                .Verifiable();
            _redisDatabaseMock.SetupGet(x => x.Multiplexer)
                .Returns(connectionMock.Object)
                .Verifiable();

            var added = await _repository.TryAddAsync(key);

            added.Should().BeFalse();

            connectionMock.VerifyAll();
            lockDatabaseMock.VerifyAll();
            _serializerMock.VerifyAll();
            _redisDatabaseMock.VerifyAll();
        }

        [Trait("Category", "Redis")]
        [Fact(DisplayName = "GIVEN Idempotency-Key, WHEN get, SHOULD returns register")]
        public async Task GivenIdempotencyKeyWhenGetShouldReturnRegister()
        {
            var register = HttpIdempotencyRegister.Of(
                Guid.NewGuid().ToString(),
                HttpStatusCode.OK,
                new MemoryStream(Encoding.UTF8.GetBytes("Message body")));

            _serializerMock.Setup(x => x.Deserialize<HttpIdempotencyRegister>(It.IsAny<string>()))
                .Returns(register)
                .Verifiable();
            _redisDatabaseMock.Setup(x => x.StringGetAsync(register.Key, CommandFlags.None))
                .ReturnsAsync(It.IsAny<string>())
                .Verifiable();

            await _repository.GetAsync<HttpIdempotencyRegister>(register.Key);

            _serializerMock.VerifyAll();
            _redisDatabaseMock.VerifyAll();
        }

        [Trait("Category", "Redis")]
        [Fact(DisplayName = "GIVEN Idempotency-Key, WHEN key exists, SHOULD return false")]
        public async Task GivenIdempotencyKeyWhenKeyExistsShouldReturnFalse()
        {
            var key = Guid.NewGuid().ToString();

            _redisDatabaseMock.Setup(x => x.KeyExistsAsync(key, CommandFlags.None))
                .ReturnsAsync(true)
                .Verifiable();

            var added = await _repository.TryAddAsync(key);

            added.Should().BeFalse();

            _redisDatabaseMock.VerifyAll();
        }


        [Trait("Category", "Redis")]
        [Fact(DisplayName = "GIVEN Idempotency-Key, WHEN not exists, SHOULD add AND set expiration time")]
        public async Task GivenIdempotencyKeyWhenNotExistsShouldAddAndSetExpirationTime()
        {
            var key = Guid.NewGuid().ToString();
            var connectionMock = new Mock<IConnectionMultiplexer>();
            var lockDatabaseMock = new Mock<IDatabase>();

            connectionMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(lockDatabaseMock.Object)
                .Verifiable();
            lockDatabaseMock.Setup(x => x.StringSet(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(),
                    It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .Returns(true)
                .Verifiable();
            _serializerMock.Setup(x => x.Serialize(It.IsAny<IIdempotencyRegister>()))
                .Returns(string.Empty)
                .Verifiable();
            _redisDatabaseMock.SetupGet(x => x.Multiplexer)
                .Returns(connectionMock.Object)
                .Verifiable();
            _redisDatabaseMock.Setup(x => x.KeyExistsAsync(key, CommandFlags.None))
                .ReturnsAsync(false)
                .Verifiable();
            _redisDatabaseMock.Setup(x =>
                    x.StringSetAsync(key, string.Empty, TimeSpan.FromMinutes(1), When.NotExists, CommandFlags.None))
                .ReturnsAsync(true)
                .Verifiable();

            var added = await _repository.TryAddAsync(key);

            added.Should().BeTrue();

            connectionMock.VerifyAll();
            lockDatabaseMock.VerifyAll();
            _serializerMock.VerifyAll();
            _redisDatabaseMock.VerifyAll();
        }

        [Trait("Category", "Redis")]
        [Fact(DisplayName = "GIVEN Idempotency-Key, WHEN remove, SHOULD remove from redis")]
        public void GivenIdempotencyKeyWhenRemoveShouldRemoveFromRedis()
        {
            var key = Guid.NewGuid().ToString();
            _redisDatabaseMock.Setup(x => x.KeyDeleteAsync(key, CommandFlags.None))
                .Verifiable();

            _repository.Awaiting(x => x.RemoveAsync(key))
                .Should().NotThrow();

            _redisDatabaseMock.VerifyAll();
        }

        [Trait("Category", "Redis")]
        [Fact(DisplayName = "GIVEN Idempotency-Key, WHEN update, SHOULD change value and set expiration time")]
        public void GivenIdempotencyKeyWhenUpdateShouldChangeValueAndSetExpirationTime()
        {
            var idempotencyKey = HttpIdempotencyRegister.Of(
                Guid.NewGuid().ToString(),
                HttpStatusCode.OK,
                new MemoryStream());

            _serializerMock.Setup(x => x.Serialize(It.IsAny<IIdempotencyRegister>()))
                .Returns(string.Empty)
                .Verifiable();
            _redisDatabaseMock.Setup(x => x.StringSetAsync(idempotencyKey.Key, string.Empty, TimeSpan.FromDays(1),
                    When.Exists, CommandFlags.None))
                .Verifiable();

            _repository.Awaiting(async x => await x.UpdateAsync(idempotencyKey.Key, idempotencyKey))
                .Should().NotThrow();

            _serializerMock.VerifyAll();
            _redisDatabaseMock.VerifyAll();
        }
    }
}