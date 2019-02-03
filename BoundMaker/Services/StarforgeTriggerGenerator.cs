namespace BoundMaker.Models.Triggers
{
    public class StarforgeTriggerGenerator : ITriggerGenerator
    {
        public string CreateUnit(string player, string unitName, string locationName)
        {
            player = player?.Replace("Player ", "P");
            unitName = unitName?.Split(new[] { ' ' }, 2)[1];

            return $"CreateUnit(1, {unitName}, {locationName}, {player});";
        }

        public string KillUnit(string locationName)
        {
            return $"KillUnitAt(All, Men, {locationName}, All Players);";
        }

        public string PreserveTrigger()
        {
            return "Preserve Trigger();";
        }

        public string Wait(int milliseconds)
        {
            return $"Wait({milliseconds});";
        }
    }
}
