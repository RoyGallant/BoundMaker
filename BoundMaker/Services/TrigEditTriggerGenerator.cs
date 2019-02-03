namespace BoundMaker.Models.Triggers
{
    public class TrigEditTriggerGenerator : ITriggerGenerator
    {
        public string CreateUnit(string player, string unitName, string locationName)
        {
            player = player.Replace("Current Player", "CurrentPlayer");
            return $"\tCreateUnit(1, \"{unitName}\", \"{locationName}\", {player});";
        }

        public string KillUnit(string locationName)
        {
            return $"\tKillUnitAt(All, \"Men\", \"{locationName}\", AllPlayers);";
        }

        public string PreserveTrigger()
        {
            return $"\tPreserveTrigger();";
        }

        public string Wait(int milliseconds)
        {
            return $"\tWait({milliseconds});";
        }
    }
}
