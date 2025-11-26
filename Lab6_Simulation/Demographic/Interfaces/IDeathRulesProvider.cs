namespace Demographic.Interfaces;

public interface IDeathRulesProvider
{
    void Load(string path);
    double GetProbability(bool isFemale, int age);
}