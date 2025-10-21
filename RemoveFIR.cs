using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace RemoveFIR
{
    public record ModMetadata : AbstractModMetadata
    {
        public override string ModGuid { get; init; } = "com.zas.removefir";
        public override string Name { get; init; } = "Remove FIR from Hideout";
        public override string Author { get; init; } = "zas";
        public override List<string>? Contributors { get; init; }
        public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
        public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.0");
        public override List<string>? Incompatibilities { get; init; }
        public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
        public override string? Url { get; init; }
        public override bool? IsBundleMod { get; init; }
        public override string? License { get; init; } = "MIT";
    }

    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
    public class NoFIRHideoutMod(
        DatabaseServer databaseServer,
        ISptLogger<NoFIRHideoutMod> logger) : IOnLoad
    {
        public Task OnLoad()
        {
            try
            {
                // Get all the in-memory data from database
                var tables = databaseServer.GetTables();

                // Iterate through all hideout areas
                foreach (var hideoutAreaData in tables.Hideout.Areas)
                {
                    // Iterate through all stages in each area
                    foreach (var stage in hideoutAreaData.Stages.Values)
                    {
                        var requirements = stage.Requirements;
                        if (requirements != null && requirements.Count > 0)
                        {
                            foreach (var requirement in requirements)
                            {
                                // Use reflection to check if property exists and set it
                                var requirementType = requirement.GetType();
                                var isSpawnedProperty = requirementType.GetProperty("IsSpawnedInSession");

                                if (isSpawnedProperty != null && isSpawnedProperty.CanWrite)
                                {
                                    isSpawnedProperty.SetValue(requirement, false);
                                }
                            }
                        }
                    }
                }

                logger.Success("[RemoveFIR] Removed hideout FIR requirements.");
            }
            catch (Exception ex)
            {
                logger.Error($"[RemoveFIR] Error loading Remove FIR: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}