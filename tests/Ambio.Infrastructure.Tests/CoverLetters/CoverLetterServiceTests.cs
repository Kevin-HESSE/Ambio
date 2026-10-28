using Ambio.Application.CoverLetters;
using Ambio.Application.CoverLetters.Dtos;
using Ambio.Domain.CoverLetters;
using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Tests.Stories;

using Bogus;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

namespace Ambio.Infrastructure.Tests.CoverLetters;

public class CoverLetterServiceTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 9, 28, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = Now.AddDays(2);

    private readonly Faker<CoverLetterInput> _inputFactory = CoverLetterInputFactory.GetGenerator();
    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync()
    {
        _services = await SqliteServiceProvider.CreateAsync();
        _services.TimeProvider.GetUtcNow().Returns(Now);
    }

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task ListAsync_WithoutLetters_ReturnsEmpty()
    {
        var story = await SaveApplicationsAsync(1);

        var letters = await _services.CallAsync((ICoverLetterService service) =>
            service.ListAsync(story.Applications[0].Id, TestContext.Current.CancellationToken));

        Assert.Empty(letters);
    }

    [Fact]
    public async Task ListAsync_WithLetters_ReturnsTheLettersOfTheApplicationInTheOrderWritten()
    {
        var story = await SaveApplicationsAsync(2);
        var applicationId = story.Applications[0].Id;
        var newest = CoverLetterFactory.GetGenerator(applicationId).Generate();
        var oldest = CoverLetterFactory.GetGenerator(applicationId).Generate();
        await AddAtAsync(Now, newest);
        await AddAtAsync(Now.AddDays(-1), oldest);
        await AddAtAsync(Now, CoverLetterFactory.GetGenerator(story.Applications[1].Id).Generate());

        var letters = await _services.CallAsync((ICoverLetterService service) =>
            service.ListAsync(applicationId, TestContext.Current.CancellationToken));

        Assert.Equal(
            [
                new CoverLetterDto(oldest.Id, oldest.Title, oldest.Content, Now.AddDays(-1).UtcDateTime),
                new CoverLetterDto(newest.Id, newest.Title, newest.Content, Now.UtcDateTime),
            ],
            letters);
    }

    [Fact]
    public async Task CreateAsync_WithTitleAndText_SavesTheTrimmedLetterAndReturnsItsId()
    {
        var story = await SaveApplicationsAsync(1);
        var input = _inputFactory.Generate();
        var typed = input with { Title = $" {input.Title} ", Content = $"\n{input.Content}\n\n" };

        var result = await _services.CallAsync((ICoverLetterService service) =>
            service.CreateAsync(story.Applications[0].Id, typed, TestContext.Current.CancellationToken));

        Assert.True(result.TryGetValue(out var id));
        var saved = await FindLetterAsync(id);
        Assert.Equal(
            (story.Applications[0].Id, input.Title, input.Content, Now.UtcDateTime),
            (saved.JobApplicationId, saved.Title, saved.Content, saved.CreatedAt));
    }

    [Fact]
    public async Task CreateAsync_UnknownApplication_FailsWithApplicationNotFound()
    {
        var result = await _services.CallAsync((ICoverLetterService service) =>
            service.CreateAsync(Guid.CreateVersion7(), _inputFactory.Generate(), TestContext.Current.CancellationToken));

        Assert.False(result.IsSuccess);
        Assert.Equal(CoverLetterErrors.ApplicationNotFound, result.GetMessage);
    }

    [Fact]
    public async Task UpdateAsync_ExistingLetter_ReplacesItsTitleAndTextAndMovesItsEditedDay()
    {
        var story = await SaveApplicationsAsync(1);
        var letter = CoverLetterFactory.GetGenerator(story.Applications[0].Id).Generate();
        await _services.AddAsync(letter);
        var input = _inputFactory.Generate();
        _services.TimeProvider.GetUtcNow().Returns(Later);

        var result = await _services.CallAsync((ICoverLetterService service) =>
            service.UpdateAsync(letter.Id, input, TestContext.Current.CancellationToken));

        Assert.True(result.IsSuccess);
        var saved = await FindLetterAsync(letter.Id);
        Assert.Equal(
            (input.Title, input.Content, Now.UtcDateTime, Later.UtcDateTime),
            (saved.Title, saved.Content, saved.CreatedAt, saved.UpdatedAt));
    }

    [Fact]
    public async Task UpdateAsync_UnknownLetter_FailsWithNotFound()
    {
        var result = await _services.CallAsync((ICoverLetterService service) =>
            service.UpdateAsync(Guid.CreateVersion7(), _inputFactory.Generate(), TestContext.Current.CancellationToken));

        Assert.False(result.IsSuccess);
        Assert.Equal(CoverLetterErrors.NotFound, result.GetMessage);
    }

    [Fact]
    public async Task DeleteAsync_ExistingLetter_DeletesOnlyThisLetter()
    {
        var story = await SaveApplicationsAsync(1);
        var letters = CoverLetterFactory.GetGenerator(story.Applications[0].Id).Generate(2);
        await _services.AddAsync([.. letters]);

        var result = await _services.CallAsync((ICoverLetterService service) =>
            service.DeleteAsync(letters[0].Id, TestContext.Current.CancellationToken));

        Assert.True(result.IsSuccess);
        var left = await _services.QueryAsync(context => context.CoverLetters.ToListAsync(TestContext.Current.CancellationToken));
        Assert.Equal(letters[1].Id, Assert.Single(left).Id);
    }

    [Fact]
    public async Task DeleteAsync_UnknownLetter_FailsWithNotFound()
    {
        var result = await _services.CallAsync((ICoverLetterService service) =>
            service.DeleteAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken));

        Assert.False(result.IsSuccess);
        Assert.Equal(CoverLetterErrors.NotFound, result.GetMessage);
    }

    private async Task<OfferApplicationStory> SaveApplicationsAsync(int count)
    {
        var story = new OfferApplicationStory().WithApplications(count);
        await story.SaveAsync(_services);
        return story;
    }

    private async Task AddAtAsync(DateTimeOffset createdAt, CoverLetter letter)
    {
        _services.TimeProvider.GetUtcNow().Returns(createdAt);
        await _services.AddAsync(letter);
    }

    private Task<CoverLetter> FindLetterAsync(Guid id) =>
        _services.QueryAsync(context => context.CoverLetters.SingleAsync(l => l.Id == id, TestContext.Current.CancellationToken));
}
