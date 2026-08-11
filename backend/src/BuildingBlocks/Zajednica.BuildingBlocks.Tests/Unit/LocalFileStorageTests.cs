using Microsoft.Extensions.Configuration;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.BuildingBlocks.Infrastructure.Storage;

namespace Zajednica.BuildingBlocks.Tests.Unit;

public class LocalFileStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"zajednica-storage-{Guid.NewGuid()}");

    [Fact]
    public void Saves_under_the_folder_of_its_kind_and_returns_the_storage_key()
    {
        var storage = new LocalFileStorage(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:LocalPath"] = _root
            })
            .Build());
        var stored = FileKind.Image.AcceptUpload("balcony.PNG", 12);

        var key = storage.Save(new MemoryStream([1, 2, 3]), stored);

        key.ShouldBe(stored.ObjectName);
        stored.ObjectName.ShouldStartWith("images/");
        stored.ObjectName.ShouldEndWith(".png");
        stored.ContentType.ShouldBe("image/png");
        File.ReadAllBytes(Path.Combine(_root, "images", Path.GetFileName(stored.ObjectName))).Length.ShouldBe(3);
    }

    [Fact]
    public void Save_writes_under_an_absolute_local_path_without_affecting_the_key()
    {
        var absolute = Path.Combine(Path.GetTempPath(), $"zajednica-abs-{Guid.NewGuid()}");
        try
        {
            var storage = new LocalFileStorage(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Storage:LocalPath"] = absolute
                })
                .Build());
            var stored = FileKind.Image.AcceptUpload("balcony.png", 12);

            var key = storage.Save(new MemoryStream([1, 2, 3]), stored);

            key.ShouldBe(stored.ObjectName);
            File.Exists(Path.Combine(absolute, "images", Path.GetFileName(stored.ObjectName))).ShouldBeTrue();
        }
        finally
        {
            if (Directory.Exists(absolute))
                Directory.Delete(absolute, true);
        }
    }

    [Fact]
    public void Two_uploads_of_the_same_name_do_not_collide()
    {
        FileKind.Audio.AcceptUpload("note.m4a", 12).ObjectName
            .ShouldNotBe(FileKind.Audio.AcceptUpload("note.m4a", 12).ObjectName);
    }

    [Fact]
    public void Rejects_an_extension_outside_the_whitelist_of_its_kind()
    {
        Should.Throw<EntityValidationException>(() => FileKind.Image.AcceptUpload("payload.exe", 12));
        Should.Throw<EntityValidationException>(() => FileKind.Image.AcceptUpload("note.m4a", 12));
        Should.Throw<EntityValidationException>(() => FileKind.Document.AcceptUpload("kucni-red.docx", 12));
    }

    [Fact]
    public void Stores_a_pdf_document_under_its_own_folder()
    {
        var stored = FileKind.Document.AcceptUpload("Kucni red.PDF", 12);

        stored.ObjectName.ShouldStartWith("documents/");
        stored.ContentType.ShouldBe("application/pdf");
    }

    [Fact]
    public void Rejects_an_empty_or_oversized_file()
    {
        Should.Throw<EntityValidationException>(() => FileKind.Audio.AcceptUpload("note.m4a", 0));
        Should.Throw<EntityValidationException>(() =>
            FileKind.Audio.AcceptUpload("note.m4a", FileKind.AudioMaxBytes + 1));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, true);
    }
}
