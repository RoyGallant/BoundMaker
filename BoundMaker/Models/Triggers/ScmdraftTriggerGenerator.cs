namespace BoundMaker.Models.Triggers
{
    public class ScmdraftTriggerGenerator : ITriggerGenerator
    {
        public string CreateUnit(string player, string unitName, string locationName)
        {
            return $"\tCreate Unit(\"{player}\", \"{unitName}\", 1, \"{locationName}\");";
        }

        public string KillUnit(string locationName)
        {
            return $"\tKill Unit At Location(\"All Players\", \"Men\", All, \"{locationName}\");";
        }

        public string PreserveTrigger()
        {
            return $"\tPreserve Trigger();";
        }

        public string Wait(int milliseconds)
        {
            return $"\tWait({milliseconds});";
        }
    }
}
