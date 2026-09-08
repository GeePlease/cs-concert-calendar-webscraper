namespace ConcertsGraz.Data;


// ==================================================================================
// CLASS: ConcertsGrazdatabaseSettings - Eigenschaftswerte appsettings.json
// der Datei ConcertsGrazDatabase gespeichert
// ==================================================================================

public class ConcertsGrazDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string ConcertsCollectionName { get; set; } = null!;
}