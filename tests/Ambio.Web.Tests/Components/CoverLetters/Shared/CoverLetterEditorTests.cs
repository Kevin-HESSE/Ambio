using Ambio.Application.Common;
using Ambio.Application.CoverLetters;
using Ambio.Application.CoverLetters.Dtos;
using Ambio.Web.Components.CoverLetters.Shared;
using Ambio.Web.Tests.Fixtures;

using Bogus;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.CoverLetters.Shared;

public class CoverLetterEditorTests : BunitContext
{
    private const string CloseSheet = "ambio.sheet.close";
    private const string Title = "Short message for the form";
    private const string Content = "Hello, I would enjoy joining your platform team.";
    private const string TitleError = "Enter a title for the letter.";
    private const string ContentError = "Enter the text of the letter.";

    private static readonly Guid ApplicationId = Guid.CreateVersion7();
    private static readonly DateTime EditedAt = new(2026, 9, 2, 9, 0, 0, DateTimeKind.Utc);

    private readonly ICoverLetterService _coverLetterService = Substitute.For<ICoverLetterService>();
    private readonly Faker<CoverLetterDto> _coverLetterFactory = CoverLetterFactory.GetGenerator();

    public CoverLetterEditorTests()
    {
        Services.AddSingleton(_coverLetterService);
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        JSInterop.SetupVoid(CloseSheet, _ => true).SetVoidResult();
    }

    [Fact]
    public void Render_BeforeTheCircuit_DisablesTheButtons()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        var cut = RenderEditor(_coverLetterFactory.Generate(2));

        Assert.All(
            cut.FindAll(".cover-letter-new, .cover-letter-item, .cover-letter-delete, .cover-letter-save"),
            button => Assert.True(button.HasAttribute("disabled")));
    }

    [Fact]
    public void Render_BeforeTheCircuit_LetsCopyTheTextWithItsToastMessages()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        var cut = RenderEditor(_coverLetterFactory.Generate(2));

        var copy = cut.Find(".cover-letter-copy");
        Assert.False(copy.HasAttribute("disabled"));
        Assert.Equal(
            (CoverLetterEditor.ContentId, CoverLetterEditor.CopiedMessage, CoverLetterEditor.CopyFailedMessage),
            (copy.GetAttribute("data-copy"), copy.GetAttribute("data-copied"), copy.GetAttribute("data-copy-failed")));
    }

    [Fact]
    public void Render_WithLetters_ListsThemAndShowsTheFirst()
    {
        var letters = _coverLetterFactory.Generate(2);
        letters[0] = letters[0] with { Content = Content, EditedAt = EditedAt };

        var cut = RenderEditor(letters);

        Assert.Equal(letters.Select(l => l.Title), cut.FindAll(".cover-letter-item-title").Select(title => title.TextContent));
        Assert.Equal("true", cut.FindAll(".cover-letter-item")[0].GetAttribute("aria-current"));
        AssertEditorShows(cut, letters[0].Title, Content);
        Assert.Equal($"{Content.Length} characters · Saved 2 Sep", cut.Find(".cover-letter-status").TextContent);
        Assert.NotNull(cut.Find(".cover-letter-delete"));
    }

    [Fact]
    public void Render_WithoutLetters_OpensANewLetter()
    {
        var cut = RenderEditor([]);

        Assert.NotNull(cut.Find(".cover-letter-list-empty"));
        AssertEditorShows(cut, "", "");
        Assert.Equal("0 characters · Not saved yet", cut.Find(".cover-letter-status").TextContent);
        Assert.Empty(cut.FindAll(".cover-letter-delete"));
    }

    [Fact]
    public void Click_AnotherLetter_ShowsIt()
    {
        var letters = _coverLetterFactory.Generate(2);
        var cut = RenderEditor(letters);

        cut.FindAll(".cover-letter-item")[1].Click();

        Assert.Equal("true", cut.FindAll(".cover-letter-item")[1].GetAttribute("aria-current"));
        AssertEditorShows(cut, letters[1].Title, letters[1].Content);
    }

    [Fact]
    public void ClickNew_EmptiesTheEditor()
    {
        var cut = RenderEditor(_coverLetterFactory.Generate(2));

        cut.Find(".cover-letter-new").Click();

        Assert.Empty(cut.FindAll(".cover-letter-item[aria-current]"));
        AssertEditorShows(cut, "", "");
    }

    [Fact]
    public void Input_Text_CountsItsCharactersAsTyped()
    {
        var cut = RenderEditor([]);

        cut.Find($"#{CoverLetterEditor.ContentId}").Input(new string('a', 1842));

        Assert.Equal("1,842 characters · Not saved yet", cut.Find(".cover-letter-status").TextContent);
    }

    [Fact]
    public void Submit_NewLetter_CreatesItAndRefreshesThePage()
    {
        _coverLetterService.CreateAsync(ApplicationId, Arg.Any<CoverLetterInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<Guid>.Success(Guid.CreateVersion7()));
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        var cut = RenderEditor([]);

        Fill(cut, Title, Content);
        cut.Find(".cover-letter-form").Submit();

        _coverLetterService.Received(1).CreateAsync(
            ApplicationId,
            Arg.Is<CoverLetterInput>(input => input.Title == Title && input.Content == Content),
            Arg.Any<CancellationToken>());
        Assert.Equal(navigation.Uri, Assert.Single(navigation.History).Uri);
    }

    [Fact]
    public void Submit_ShownLetter_UpdatesItAndRefreshesThePage()
    {
        var letter = _coverLetterFactory.Generate();
        _coverLetterService.UpdateAsync(letter.Id, Arg.Any<CoverLetterInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult.Success());
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        var cut = RenderEditor([letter]);

        Fill(cut, Title, Content);
        cut.Find(".cover-letter-form").Submit();

        _coverLetterService.Received(1).UpdateAsync(
            letter.Id,
            Arg.Is<CoverLetterInput>(input => input.Title == Title && input.Content == Content),
            Arg.Any<CancellationToken>());
        Assert.Single(navigation.History);
    }

    [Fact]
    public void Submit_Empty_ShowsErrorsWithoutSaving()
    {
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        var cut = RenderEditor([]);

        cut.Find(".cover-letter-form").Submit();

        Assert.Equal([TitleError, ContentError], cut.FindAll(".validation-message").Select(message => message.TextContent));
        _coverLetterService.DidNotReceive().CreateAsync(Arg.Any<Guid>(), Arg.Any<CoverLetterInput>(), Arg.Any<CancellationToken>());
        Assert.Empty(navigation.History);
    }

    [Fact]
    public void Submit_ApplicationGone_RequestsNotFound()
    {
        _coverLetterService.CreateAsync(ApplicationId, Arg.Any<CoverLetterInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<Guid>.Failure(CoverLetterErrors.ApplicationNotFound));
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;
        var cut = RenderEditor([]);

        Fill(cut, Title, Content);
        cut.Find(".cover-letter-form").Submit();

        Assert.True(notFound);
    }

    [Fact]
    public void ConfirmDelete_DeletesTheLetterClosesTheSheetAndRefreshesThePage()
    {
        var letters = _coverLetterFactory.Generate(2);
        _coverLetterService.DeleteAsync(letters[1].Id, Arg.Any<CancellationToken>()).Returns(ServiceResult.Success());
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        var cut = RenderEditor(letters);
        cut.FindAll(".cover-letter-item")[1].Click();

        Assert.Equal(CoverLetterEditor.DeleteSheetId, cut.Find(".cover-letter-delete").GetAttribute("data-sheet-open"));
        cut.Find(".cover-letter-delete-confirm").Click();

        _coverLetterService.Received(1).DeleteAsync(letters[1].Id, Arg.Any<CancellationToken>());
        Assert.Equal(CoverLetterEditor.DeleteSheetId, JSInterop.VerifyInvoke(CloseSheet).Arguments[0]);
        Assert.Single(navigation.History);
    }

    [Fact]
    public void Render_SameLettersAgain_KeepsTheTextBeingWritten()
    {
        var letters = _coverLetterFactory.Generate(2);
        var cut = RenderEditor(letters);
        Fill(cut, Title, Content);

        // As after a status change, which refreshes the page.
        cut.Render(parameters => parameters.Add(editor => editor.Letters, [.. letters]));

        AssertEditorShows(cut, Title, Content);
    }

    [Fact]
    public void Render_ShownLetterSaved_ShowsItsSavedText()
    {
        var letters = _coverLetterFactory.Generate(2);
        var cut = RenderEditor(letters);
        var saved = letters[0] with { Title = Title, Content = Content, EditedAt = EditedAt };

        cut.Render(parameters => parameters.Add(editor => editor.Letters, [saved, letters[1]]));

        AssertEditorShows(cut, Title, Content);
        Assert.EndsWith("Saved 2 Sep", cut.Find(".cover-letter-status").TextContent);
    }

    [Fact]
    public void Render_ShownLetterGone_ShowsTheFirst()
    {
        var letters = _coverLetterFactory.Generate(2);
        var cut = RenderEditor(letters);
        cut.FindAll(".cover-letter-item")[1].Click();

        cut.Render(parameters => parameters.Add(editor => editor.Letters, [letters[0]]));

        AssertEditorShows(cut, letters[0].Title, letters[0].Content);
    }

    [Fact]
    public void Render_CreatedLetterReceived_ShowsIt()
    {
        var created = _coverLetterFactory.Generate() with { Title = Title, Content = Content };
        _coverLetterService.CreateAsync(ApplicationId, Arg.Any<CoverLetterInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<Guid>.Success(created.Id));
        var letters = _coverLetterFactory.Generate(1);
        var cut = RenderEditor(letters);
        cut.Find(".cover-letter-new").Click();
        Fill(cut, Title, Content);
        cut.Find(".cover-letter-form").Submit();

        cut.Render(parameters => parameters.Add(editor => editor.Letters, [letters[0], created]));

        Assert.Equal("true", cut.FindAll(".cover-letter-item")[1].GetAttribute("aria-current"));
        Assert.NotNull(cut.Find(".cover-letter-delete"));
    }

    private static void Fill(IRenderedComponent<CoverLetterEditor> cut, string title, string content)
    {
        cut.Find("#cover-letter-title").Change(title);
        cut.Find($"#{CoverLetterEditor.ContentId}").Change(content);
    }

    private static void AssertEditorShows(IRenderedComponent<CoverLetterEditor> cut, string title, string content)
    {
        Assert.Equal(title, cut.Find("#cover-letter-title").GetAttribute("value"));
        Assert.Equal(content, cut.Find($"#{CoverLetterEditor.ContentId}").GetAttribute("value"));
    }

    private IRenderedComponent<CoverLetterEditor> RenderEditor(IReadOnlyList<CoverLetterDto> letters) =>
        Render<CoverLetterEditor>(parameters => parameters
            .Add(editor => editor.ApplicationId, ApplicationId)
            .Add(editor => editor.Letters, letters));
}
