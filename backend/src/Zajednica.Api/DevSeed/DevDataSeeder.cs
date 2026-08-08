using Microsoft.EntityFrameworkCore;
using Zajednica.Community.Api.Dto.Certification;
using Zajednica.Community.Api.Dto.Communities;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Infrastructure.Database;
using Zajednica.Feed.Api.Dto.Comments;
using Zajednica.Feed.Api.Dto.Posts;
using Zajednica.Feed.Api.Public;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Api.DevSeed;

public sealed class DevDataSeeder(
    IServiceScopeFactory scopes,
    ILogger<DevDataSeeder> logger) : IHostedService
{
    public const string DefaultPassword = "asdfqwer";

    private static readonly SeedAccount[] People =
    [
        new("milanp", "milanp@zajednica.local", "Milan", "Petrović"),
        new("jovana88", "jovana88@zajednica.local", "Jovana", "Ilić"),
        new("stefan_92", "stefan_92@zajednica.local", "Stefan", "Jovanović"),
        new("anak", "anak@zajednica.local", "Ana", "Kovačević"),
        new("nikola021", "nikola021@zajednica.local", "Nikola", "Nikolić"),
        new("milicaa", "milicaa@zajednica.local", "Milica", "Đorđević"),
        new("markoo", "markoo@zajednica.local", "Marko", "Marković"),
        new("teodora7", "teodora7@zajednica.local", "Teodora", "Simić"),
        new("lukam", "lukam@zajednica.local", "Luka", "Stanković"),
        new("ivana.r", "ivanar@zajednica.local", "Ivana", "Radovanović"),
        new("djordje95", "djordje95@zajednica.local", "Đorđe", "Pavlović"),
        new("sara_v", "sarav@zajednica.local", "Sara", "Vasić"),
        new("filip013", "filip013@zajednica.local", "Filip", "Lukić"),
        new("jelenaa", "jelenaa@zajednica.local", "Jelena", "Đukić"),
        new("uros88", "uros88@zajednica.local", "Uroš", "Ristić"),
        new("katarinam", "katarinam@zajednica.local", "Katarina", "Milošević"),
        new("vladimir7", "vladimir7@zajednica.local", "Vladimir", "Todorović"),
        new("nina.p", "ninap@zajednica.local", "Nina", "Popović"),
        new("dusanb", "dusanb@zajednica.local", "Dušan", "Božić"),
        new("tamara04", "tamara04@zajednica.local", "Tamara", "Ćirić"),
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var sp = scope.ServiceProvider;
        var identityDb = sp.GetRequiredService<IdentityDbContext>();

        if (await identityDb.Accounts.AnyAsync(cancellationToken))
        {
            logger.LogInformation("[DevSeed] Baza vec ima naloge; preskacem seed.");
            return;
        }

        var auth = sp.GetRequiredService<IAuthenticationService>();
        var communities = sp.GetRequiredService<ICommunityService>();
        var certification = sp.GetRequiredService<ICertificationService>();
        var posts = sp.GetRequiredService<IPostService>();
        var comments = sp.GetRequiredService<ICommentService>();
        var communityDb = sp.GetRequiredService<CommunityDbContext>();

        var accounts = People.ToDictionary(p => p.Username, p => RegisterActivated(auth, identityDb, p));

        var vracar = BuildCommunity(
            communities, certification, communityDb, accounts,
            creatorId: accounts["milanp"],
            new CreateCommunityRequest(
                "SZ Njegoševa 24",
                new AddressDto("Njegoševa", "24", 44.8010m, 20.4720m),
                "17845210", "108234567", "160-0000012345678-90"),
            memberIds: Members(accounts, "anak", "nikola021", "milicaa", "markoo", "teodora7", "lukam", "ivana.r", "djordje95", "stefan_92"),
            starsByUsername: new Dictionary<string, int>
            {
                ["djordje95"] = 300, ["milicaa"] = 210, ["markoo"] = 175,
                ["anak"] = 140, ["lukam"] = 95, ["teodora7"] = 60,
                ["stefan_92"] = 40, ["nikola021"] = 20,
            });

        MakeManager(communityDb, vracar, accounts["djordje95"]);
        SeedGeneralPosts(posts, vracar,
            (accounts["milanp"], "Zakazan je sastanak stambene zajednice u petak u 19h u hodniku prizemlja.", "Plain"),
            (accounts["djordje95"], "Lift u levom ulazu ne radi, prijavljeno je servisu.", "Problem"),
            (accounts["milicaa"], "Poplava u podrumu, hitno zatvorite glavni ventil za vodu!", "Emergency"),
            (accounts["markoo"], "Ko je zainteresovan za zajedničku nabavku soli za posipanje stepeništa?", "Plain"));

        var vracarMembers = Members(accounts,
            "milanp", "anak", "nikola021", "milicaa", "markoo", "teodora7", "lukam", "ivana.r", "djordje95", "stefan_92");
        await SeedManyPosts(posts, vracar, vracarMembers);
        await SeedBusyDiscussion(posts, comments, vracar, vracarMembers);

        var noviBeograd = BuildCommunity(
            communities, certification, communityDb, accounts,
            creatorId: accounts["jovana88"],
            new CreateCommunityRequest(
                "SZ Bulevar Zorana Đinđića 105",
                new AddressDto("Bulevar Zorana Đinđića", "105", 44.8180m, 20.4210m),
                "20911345", "109876543", "265-0000098765432-11"),
            memberIds: Members(accounts, "sara_v", "filip013", "jelenaa", "uros88", "katarinam", "vladimir7", "nina.p", "dusanb", "tamara04", "stefan_92"),
            starsByUsername: new Dictionary<string, int>
            {
                ["katarinam"] = 275, ["uros88"] = 190, ["sara_v"] = 150,
                ["filip013"] = 120, ["nina.p"] = 80, ["dusanb"] = 55,
                ["tamara04"] = 30, ["jelenaa"] = 15,
            });

        SeedGeneralPosts(posts, noviBeograd,
            (accounts["jovana88"], "Dobrodošli svima u zajednicu! Kontejneri se prazne ponedeljkom i četvrtkom.", "Plain"),
            (accounts["uros88"], "Interfon na ulazu broj 2 ne zvoni, ne čuje se poziv.", "Problem"),
            (accounts["katarinam"], "Kvar na gasnoj instalaciji, oseti se miris gasa u prizemlju!", "Emergency"),
            (accounts["nina.p"], "Da li neko ima kontakt dobrog vodoinstalatera iz kraja?", "Plain"));

        logger.LogInformation(
            "[DevSeed] Napunjeno {Accounts} naloga i 2 zajednice (lozinka: {Password}).",
            People.Length, DefaultPassword);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private Guid BuildCommunity(
        ICommunityService communities,
        ICertificationService certification,
        CommunityDbContext communityDb,
        IReadOnlyDictionary<string, Guid> accounts,
        Guid creatorId,
        CreateCommunityRequest request,
        IReadOnlyList<Guid> memberIds,
        IReadOnlyDictionary<string, int> starsByUsername)
    {
        var community = communities.Create(creatorId, request);
        var qr = communities.GetQr(creatorId, community.Id);

        foreach (var memberId in memberIds)
        {
            communities.Join(memberId, new JoinCommunityRequest(qr.QrToken));

            var challenge = certification.CreateChallenge(creatorId, community.Id);
            certification.Confirm(memberId, new ConfirmCertificationRequest(challenge.Token));
        }

        AwardStars(communityDb, community.Id, accounts, starsByUsername);

        return community.Id;
    }

    private static void AwardStars(
        CommunityDbContext communityDb,
        Guid communityId,
        IReadOnlyDictionary<string, Guid> accounts,
        IReadOnlyDictionary<string, int> starsByUsername)
    {
        if (starsByUsername.Count == 0)
            return;

        var members = communityDb.Memberships
            .Where(m => m.CommunityId == communityId)
            .ToList();

        foreach (var (username, stars) in starsByUsername)
        {
            var accountId = accounts[username];
            var membership = members.SingleOrDefault(m => m.AccountId == accountId);
            membership?.AddStars(stars);
        }

        communityDb.SaveChanges();
    }

    private void MakeManager(CommunityDbContext communityDb, Guid communityId, Guid accountId)
    {
        var membership = communityDb.Memberships
            .Single(m => m.CommunityId == communityId && m.AccountId == accountId);

        membership.Grant(CommunityRole.Manager, null, DateTime.UtcNow);
        communityDb.SaveChanges();
    }

    private static void SeedGeneralPosts(
        IPostService posts,
        Guid communityId,
        params (Guid AuthorId, string Text, string Kind)[] items)
    {
        foreach (var item in items)
            posts.CreateGeneral(item.AuthorId, communityId, new CreateGeneralPostRequest(item.Text, item.Kind, null));
    }

    private static readonly string[] FillerPosts =
    [
        "Molba da se bicikli ne ostavljaju u hodniku prizemlja, smetaju prolazu.",
        "Da li je neko primetio da svetlo na drugom spratu stalno gori i danju?",
        "Skupljamo predloge za bojenje fasade sa dvorišne strane.",
        "Podsećanje: kante za reciklažu su od sada iza zgrade, kod parkinga.",
        "Traži se električar, kvar na osvetljenju u zajedničkoj garaži.",
        "Ko parkira ispred kontejnera, blokira odvoz smeća četvrtkom.",
        "Predlog da postavimo policu za pakete u ulazu, javite mišljenje.",
        "Krečenje stepeništa počinje u ponedeljak, koristite drugi ulaz.",
        "Deca se igraju u dvorištu, molim vozače da uspore pri ulasku.",
        "Da li neko ima ključ od tavana? Treba proveriti krov posle kiše.",
        "Organizujemo prolećno čišćenje dvorišta, prijavite se u komentarima.",
        "Interfon na trećem spratu prekida vezu, prijavljeno majstoru.",
        "Predlog za postavljanje kamere na ulaz zbog čestih provala.",
        "Voda je slabija na višim spratovima ovih dana, javljam upravniku.",
        "Nova pravila za korišćenje zajedničke perionice su okačena na oglasnoj tabli.",
        "Molim da se vrata podruma drže zaključana, mačke ulaze unutra.",
        "Kosačica za travu je pokvarena, tražimo majstora ili zamenu.",
        "Sakupljamo za novu rasvetu u ulazu, detalji uskoro.",
        "Ko je ostavio kolica za bebe kod lifta? Smetaju prolazu.",
        "Podsećanje na plaćanje mesečnog održavanja do 15. u mesecu.",
        "Grejanje u prizemlju ne radi kako treba, prijavljeno je.",
        "Predlog da zajednički kupimo aparat za gašenje požara po ulazu.",
        "Radovi na vodovodu u ulici, moguć prekid vode sutra pre podne.",
        "Hvala svima koji su učestvovali u sređivanju dvorišta prošlog vikenda!",
    ];

    private static async Task SeedManyPosts(IPostService posts, Guid communityId, IReadOnlyList<Guid> authors)
    {
        for (var i = 0; i < FillerPosts.Length; i++)
        {
            posts.CreateGeneral(authors[i % authors.Count], communityId,
                new CreateGeneralPostRequest(FillerPosts[i], "Plain", null));
            await Task.Delay(1);
        }
    }

    private static async Task SeedBusyDiscussion(
        IPostService posts, ICommentService comments, Guid communityId, IReadOnlyList<Guid> authors)
    {
        var post = posts.CreateGeneral(authors[0], communityId,
            new CreateGeneralPostRequest(
                "Predlog za uređenje zajedničkog dvorišta — ostavite komentare i odgovore ispod.", "Plain", null));
        await Task.Delay(1);

        var firstCommentId = Guid.Empty;
        for (var i = 0; i < 24; i++)
        {
            var comment = comments.Add(authors[i % authors.Count], communityId, post.Id,
                new AddCommentRequest($"Komentar broj {i + 1} na predlog o dvorištu."));
            if (i == 0)
                firstCommentId = comment.Id;
            await Task.Delay(1);
        }

        for (var i = 0; i < 24; i++)
        {
            comments.Reply(authors[i % authors.Count], communityId, post.Id, firstCommentId,
                new AddCommentRequest($"Odgovor broj {i + 1} na prvi komentar."));
            await Task.Delay(1);
        }
    }

    private static IReadOnlyList<Guid> Members(IReadOnlyDictionary<string, Guid> accounts, params string[] usernames)
        => usernames.Select(u => accounts[u]).ToList();

    private Guid RegisterActivated(IAuthenticationService auth, IdentityDbContext identityDb, SeedAccount person)
    {
        auth.Register(new RegisterAccountRequest(
            person.Username, person.Email, DefaultPassword, person.FirstName, person.LastName, Phone: null, ContactEmail: null));

        var accountId = identityDb.Accounts.Single(a => a.Username == person.Username).Id;
        var token = identityDb.Verifications.Where(v => v.AccountId == accountId).Select(v => v.Token).Single();

        auth.VerifyEmail(new VerifyEmailRequest(token));

        return accountId;
    }

    private sealed record SeedAccount(string Username, string Email, string FirstName, string LastName);
}
