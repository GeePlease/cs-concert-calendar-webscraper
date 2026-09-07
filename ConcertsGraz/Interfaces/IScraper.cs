using ConcertsGraz.Models;
namespace ConcertsGraz.Interfaces;


public interface IScraper
{
    Task<List<Concert>> RunAsync();
}