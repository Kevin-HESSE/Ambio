using System.Text;

using Ambio.Application.Common;
using Ambio.Application.CvDocuments;
using Ambio.Web.Components.CvDocuments.Shared;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.CvDocuments.Shared;

public class PdfUploadTests : BunitContext
{
    private const string FileName = "CV_Backend_2026-09.pdf";
    private const long MaxUploadSize = 5 * 1024 * 1024;

    private static readonly Guid ApplicationId = Guid.CreateVersion7();
    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.7 a CV");

    private readonly ICvDocumentService _cvDocumentService = Substitute.For<ICvDocumentService>();

    public PdfUploadTests()
    {
        _cvDocumentService.MaxUploadSize.Returns(MaxUploadSize);
        Services.AddSingleton(_cvDocumentService);
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
    }

    [Fact]
    public void Render_BeforeTheCircuit_DisablesTheInput()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        var cut = RenderUpload();

        Assert.True(cut.Find($"#{PdfUpload.InputId}").HasAttribute("disabled"));
    }

    [Fact]
    public void Render_LabelsTheInputAndSaysTheSizeLimit()
    {
        var cut = RenderUpload();

        Assert.Equal("Choose a file", cut.Find($"label[for='{PdfUpload.InputId}']").TextContent);
        Assert.Equal("PDF only, 5 MB max", cut.Find(".form-text").TextContent);
    }

    [Fact]
    public void Upload_Pdf_SendsItAndRefreshesThePage()
    {
        byte[]? sent = null;
        _cvDocumentService.UploadAsync(ApplicationId, FileName, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                using var copy = new MemoryStream();
                call.Arg<Stream>().CopyTo(copy);
                sent = copy.ToArray();
                return ServiceResult<Guid>.Success(Guid.CreateVersion7());
            });
        var cut = RenderUpload();

        UploadFile(cut, Pdf);

        cut.WaitForAssertion(() => Assert.Single(Services.GetRequiredService<BunitNavigationManager>().History));
        Assert.Equal(Pdf, sent);
        Assert.Empty(cut.FindAll("[role=alert]"));
    }

    [Fact]
    public void Upload_OverTheLimit_ShowsTheErrorWithoutSendingIt()
    {
        _cvDocumentService.MaxUploadSize.Returns(Pdf.Length - 1);
        var cut = RenderUpload();

        UploadFile(cut, Pdf);

        Assert.Equal(CvDocumentErrors.TooLarge, cut.Find("[role=alert]").TextContent.Trim());
        _cvDocumentService.DidNotReceiveWithAnyArgs().UploadAsync(default, default!, default!, Xunit.TestContext.Current.CancellationToken);
    }

    [Fact]
    public void Upload_Refused_ShowsTheMessageWithoutRefreshing()
    {
        _cvDocumentService.UploadAsync(ApplicationId, FileName, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<Guid>.Failure(CvDocumentErrors.NotPdf));
        var cut = RenderUpload();

        UploadFile(cut, Pdf);

        cut.WaitForAssertion(() => Assert.Equal(CvDocumentErrors.NotPdf, cut.Find("[role=alert]").TextContent.Trim()));
        Assert.Empty(Services.GetRequiredService<BunitNavigationManager>().History);
        Assert.False(cut.Find($"#{PdfUpload.InputId}").HasAttribute("disabled"));
    }

    [Fact]
    public void Upload_ApplicationGone_RequestsNotFound()
    {
        _cvDocumentService.UploadAsync(ApplicationId, FileName, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<Guid>.Failure(CvDocumentErrors.ApplicationNotFound));
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;
        var cut = RenderUpload();

        UploadFile(cut, Pdf);

        cut.WaitForAssertion(() => Assert.True(notFound));
    }

    private IRenderedComponent<PdfUpload> RenderUpload() =>
        Render<PdfUpload>(parameters => parameters.Add(upload => upload.ApplicationId, ApplicationId));

    private static void UploadFile(IRenderedComponent<PdfUpload> cut, byte[] content) =>
        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(content, FileName, contentType: "application/pdf"));
}
