namespace BoundMaker.Models.Triggers
{
    public interface ITriggerGenerator
    {
        string CreateUnit(string player, string unitName, string locationName);
        string KillUnit(string locationName);
        string Wait(int milliseconds);
        string PreserveTrigger();
    }
}
