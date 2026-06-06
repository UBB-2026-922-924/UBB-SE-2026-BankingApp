namespace BankingApp.Application.Tests.Features.Chat.Services;

using Application.Features.Chat.Services;
using Domain.Aggregates.ChatAggregate;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Shared.Clock;
using Shared.Persistence;

public sealed class ChatServiceTests
{
    private const int TestUserId = 1;
    private const int SessionId = 10;

    private static readonly DateTime _testNow = new(2026, 6, 5, 8, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IChatRepository> _chatRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);

    [Fact]
    public async Task CloseSessionAsync_WhenSessionExists_ShouldCloseAndPersistChanges()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        ChatSession session = CreateSession(TestUserId);

        _chatRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SessionId, cancellationToken))
            .ReturnsAsync(session);

        _chatRepositoryMock
            .Setup(repository => repository.UpdateAsync(session, cancellationToken))
            .Returns(Task.FromResult(Task.CompletedTask));

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        ChatService service = CreateService();

        ErrorOr<Success> result = await service.CloseSessionAsync(TestUserId, SessionId, cancellationToken);

        result.IsError.Should().BeFalse();
        session.Status.Should().Be(ChatSessionStatus.Closed);
        _chatRepositoryMock.Verify(repository => repository.UpdateAsync(session, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CloseSessionAsync_WhenSessionDoesNotExist_ShouldReturnSessionNotFound()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _chatRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SessionId, cancellationToken))
            .ReturnsAsync((ChatSession?)null);

        ChatService service = CreateService();

        ErrorOr<Success> result = await service.CloseSessionAsync(TestUserId, SessionId, cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ChatErrors.SessionNotFound);
        _chatRepositoryMock.Verify(repository => repository.GetByIdAsync(SessionId, cancellationToken), Times.Once);
        _chatRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SaveFeedbackAsync_WhenSessionExists_ShouldPersistFeedback()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        ChatSession session = CreateSession(TestUserId);

        _chatRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SessionId, cancellationToken))
            .ReturnsAsync(session);

        _chatRepositoryMock
            .Setup(repository => repository.UpdateAsync(session, cancellationToken))
            .Returns(Task.FromResult(Task.CompletedTask));

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        ChatService service = CreateService();

        ErrorOr<Success> result = await service.SaveFeedbackAsync(TestUserId, SessionId, 5, "Great support", cancellationToken);

        result.IsError.Should().BeFalse();
        session.Rating.Should().Be(5);
        session.Feedback.Should().Be("Great support");
    }

    [Fact]
    public async Task SaveFeedbackAsync_WhenSessionBelongsToAnotherUser_ShouldReturnSessionNotFound()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        ChatSession session = CreateSession(TestUserId + 1);

        _chatRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SessionId, cancellationToken))
            .ReturnsAsync(session);

        ChatService service = CreateService();

        ErrorOr<Success> result = await service.SaveFeedbackAsync(TestUserId, SessionId, 5, "Great support", cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ChatErrors.SessionNotFound);
        _chatRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<ChatSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private ChatService CreateService() =>
        new(_chatRepositoryMock.Object, _unitOfWorkMock.Object, _clockMock.Object);

    private static ChatSession CreateSession(int userId) =>
        ChatSession.Start(userId, "Support", _testNow);
}
