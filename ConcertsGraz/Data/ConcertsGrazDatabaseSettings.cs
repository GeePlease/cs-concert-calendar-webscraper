namespace ConcertsGraz.Data;


// ==================================================================================
// CLASS: ConcertsGrazdatabaseSettings - config from  appsettings.json
// ==================================================================================

public class ConcertsGrazDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string ConcertsCollectionName { get; set; } = null!;

    public string UsersCollectionName { get; set; } = null!;
}