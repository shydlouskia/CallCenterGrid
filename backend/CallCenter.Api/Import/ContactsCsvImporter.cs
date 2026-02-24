using CallCenter.Api.Data;
using CallCenter.Api.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CallCenter.Api.Import;

public class ContactsCsvImporter
{
    private readonly AppDbContext _db;
    private readonly ILogger<ContactsCsvImporter> _log;

    public ContactsCsvImporter(AppDbContext db, ILogger<ContactsCsvImporter> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<long> ImportIfEmptyAsync(string csvPath, CancellationToken ct)
    {
        await _db.Database.EnsureCreatedAsync(ct);

        if (await _db.Contacts.AsNoTracking().AnyAsync(ct))
        {
            _log.LogInformation("Contacts table not empty. Import skipped.");
            return 0;
        }

        if (!File.Exists(csvPath))
            throw new FileNotFoundException($"CSV not found: {csvPath}");

        // Detect delimiter (tab vs comma)
        string firstLine;
        using (var r = new StreamReader(csvPath))
            firstLine = await r.ReadLineAsync() ?? "";

        var delimiter = firstLine.Contains('\t') ? "\t" : ",";

        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = delimiter,
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        _db.ChangeTracker.AutoDetectChangesEnabled = false;

        long total = 0;
        const int batchSize = 5000;
        var batch = new List<Contact>(batchSize);

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, cfg);

        await foreach (var r in csv.GetRecordsAsync<CsvContactRecord>().WithCancellation(ct))
        {
            batch.Add(new Contact
            {
                Id = r.id,
                FirstName = r.first_name ?? "",
                LastName = r.last_name ?? "",
                Phone = r.phone ?? "",
                Email = r.email ?? "",
                Address = r.address ?? "",
                City = r.city ?? "",
                State = r.state ?? "",
                Zip = r.zip ?? "",
                Age = r.age,
                Status = r.status ?? ""
            });

            if (batch.Count >= batchSize)
            {
                _db.Contacts.AddRange(batch);
                await _db.SaveChangesAsync(ct);
                total += batch.Count;
                batch.Clear();
                _log.LogInformation("Imported {Total} rows...", total);
            }
        }

        if (batch.Count > 0)
        {
            _db.Contacts.AddRange(batch);
            await _db.SaveChangesAsync(ct);
            total += batch.Count;
        }

        _db.ChangeTracker.AutoDetectChangesEnabled = true;
        _log.LogInformation("Import complete. Total inserted: {Total}", total);
        return total;
    }
}
