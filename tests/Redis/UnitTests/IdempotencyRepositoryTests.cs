namespace Idempotency.Redis.UnitTests
{
    public class IdempotencyRepositoryTests
    {
        /*
        public IdempotencyRepositoryTests()
        {
            _rediClientMock = new Mock<IRedisClient>();
            _repository = new IdempotencyRepository(_rediClientMock.Object);
        }

        private readonly Mock<IRedisClient> _rediClientMock;
        private readonly IIdempotencyRepository _repository;

        [Trait("Category", "Redis")]
        [Fact(DisplayName = "GIVEN Idempotency-Key, WHEN get, SHOULD returns register")]
        public async Task GivenIdempotencyKeyWhenGetShouldReturnRegister()
        {
            var expectedRegister = IdempotencyRegister.Of(
                Guid.NewGuid().ToString(),
                HttpStatusCode.OK,
                new MemoryStream(Encoding.UTF8.GetBytes("Message body")));
            var serializedValue =
                Convert.ToBase64String(ZeroFormatter.ZeroFormatterSerializer.Serialize(expectedRegister));
            _rediClientMock.Setup(x => x.GetValue(expectedRegister.Key))
                .Returns(serializedValue)
                .Verifiable();

            var register = await _repository.GetAsync(expectedRegister.Key);

            register.Should().NotBeNull();
            register.Key.Should().Be(expectedRegister.Key);
            register.StatusCode.Should().Be(expectedRegister.StatusCode);
            register.Body.Should().Be("Message body");
            _rediClientMock.VerifyAll();
        }

        [Trait("Category", "Redis")]
        [Fact(DisplayName = "GIVEN Idempotency-Key, WHEN not exists, SHOULD add AND set expiration time")]
        public async Task GivenIdempotencyKeyWhenNotExistsShouldAddAndSetExpirationTime()
        {
            var key = Guid.NewGuid().ToString();
            _rediClientMock.Setup(x => x.SetValueIfNotExists(key, It.IsAny<string>()))
                .Returns(true)
                .Verifiable();
            _rediClientMock.Setup(x => x.ExpireEntryIn(key, It.IsAny<TimeSpan>()))
                .Returns(true)
                .Verifiable();

            var added = await _repository.TryAddAsync(key);

            added.Should().BeTrue();
        }

        [Trait("Category", "Redis")]
        [Fact(DisplayName = "GIVEN Idempotency-Key, WHEN remove, SHOULD remove from redis")]
        public void GivenIdempotencyKeyWhenRemoveShouldRemoveFromRedis()
        {
            var key = Guid.NewGuid().ToString();
            _rediClientMock.Setup(x => x.Remove(key))
                .Verifiable();

            _repository.Awaiting(x => x.RemoveAsync(key))
                .Should().NotThrow();
            _rediClientMock.VerifyAll();
        }

        [Trait("Category", "Redis")]
        [Fact(DisplayName = "GIVEN Idempotency-Key, WHEN update, SHOULD change value and set expiration time")]
        public void GivenIdempotencyKeyWhenUpdateShouldChangeValueAndSetExpirationTime()
        {
            var idempotencyKey = IdempotencyRegister.Of(
                Guid.NewGuid().ToString(),
                HttpStatusCode.OK,
                new MemoryStream());
            _rediClientMock.Setup(x => x.SetValue(idempotencyKey.Key, It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Verifiable();

            _repository.Awaiting(x => x.UpdateAsync(idempotencyKey.Key, idempotencyKey))
                .Should().NotThrow();

            _rediClientMock.VerifyAll();
        }
        */
    }
}