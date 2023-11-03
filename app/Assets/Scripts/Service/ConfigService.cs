using Utils.Injection;

namespace Service
{
    public class CharacterData
    {
        public string prefab;
    }

    public enum BuildingState
    {
        Poor = 0,
        Fair,
        Good,
        Perfect
    }

    public enum BuildingType
    {
        Shop,
        Factory,
        Hotel
    }

    public class BuildConfig
    {
        public int width;
        public int height;
        public BuildingConfig[] buildings;
    }

    public class BuildingConfig
    {
        public int width;
        public int height;
        
        public int cost;
        public int rate;
        
        public string prefab;
    }

    public class AgentConfig
    {
        public string prefab;
    }

    [Singleton]
    public class ConfigService : InjectableObject<ConfigService>
    {
        public AgentConfig[] GetAgents()
        {
            return new[]
            {
                new AgentConfig()
                {
                    prefab = "Character_Medical_Male_01"
                },
                new AgentConfig()
                {
                    prefab = "Character_Junky_Female_01"
                },
                new AgentConfig()
                {
                    prefab = "Character_Monk_Male_01"
                }
            };
        }
        
        public BuildConfig Get()
        {
            return new BuildConfig()
            {
                width = 26,
                height = 23,
                buildings = new[]
                {
                    new BuildingConfig()
                    {
                        width = 4,
                        height = 4,
                        prefab = "Buildings/Shop",
                        cost = 30,
                        rate = 10,
                    },

                    new BuildingConfig()

                    {
                        width = 4,
                        height = 4,
                        prefab = "Buildings/Factory",
                        cost = 60,
                        rate = 20,
                    },

                    new BuildingConfig()

                    {
                        width = 4,
                        height = 4,
                        prefab = "Buildings/Hotel",
                        cost = 120,
                        rate = 40
                    }
                }
            };
        }
    }
}