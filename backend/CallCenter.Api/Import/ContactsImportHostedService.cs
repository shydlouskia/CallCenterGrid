using CallCenter.Api.Data;

namespace CallCenter.Api.Import;

public class ContactsImportHostedService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly IConfiguration _cfg;
    private readonly ILogger<ContactsImportHostedService> _log;

    public ContactsImportHostedService(IServiceProvider sp, IConfiguration cfg, ILogger<ContactsImportHostedService> log)
    {
        _sp = sp;
        _cfg = cfg;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(300, stoppingToken);

        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var importerLog = scope.ServiceProvider.GetRequiredService<ILogger<ContactsCsvImporter>>();

        var configured = _cfg["Import:CsvPath"] ?? "data/contacts_500k.csv";

        var baseDir = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));

        var csvPath = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(repoRoot, configured.Replace('/', Path.DirectorySeparatorChar));

        try
        {
            var importer = new ContactsCsvImporter(db, importerLog);
            await importer.ImportIfEmptyAsync(csvPath, stoppingToken);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "CSV import failed.");
        }
    }
}
