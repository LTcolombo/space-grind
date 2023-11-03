using System;
using DefaultNamespace.Model;
using Service;
using Settlement.Types;
using Utils.Injection;
using Utils.Signal;

namespace Model
{
    [Singleton]
    public class BuildingsModel : InjectableObject<BuildingsModel>
    {
        [Inject] private InteractionModel _interaction;
        [Inject] private ConfigService _config;

        public readonly Signal Updated = new();

        public byte[,] OccupiedData { get; private set; }
        
        public byte CurrentId { get; private set; }
        public BuildingConfig Current => GetConfig(CurrentId);

        private Building[] _data = Array.Empty<Building>();

        public void Set(Building[] value)
        {
            _data = value;
            OccupiedData = GetCellsData();
            Updated.Dispatch();
        }

        public Building[] Get()
        {
            return _data;
        }

        private byte[,] GetCellsData()
        {
            var result = new byte[26, 23];

            //this is a proper dynamic data calculated based on placed buildings
            foreach (var building in _data)
            {
                var config = GetConfig(building.Id);

                for (var i = building.X; i < building.X + config.width; i++)
                for (var j = building.Y; j < building.Y + config.height; j++)
                    result[i, j] = 1;
            }
            
            //this is a temporary hardcoded code to block out roads etc.
            
            for (var i = 0; i < 26; i++)
            for (var j = 0; j < 23; j++)
            {
                if (i < 9)
                    result[i, j] = 1;
                if (i > 18)
                    result[i, j] = 1;
                if (j is > 4 and < 12)
                    result[i, j] = 1;
                
                if (j > 15)
                    result[i, j] = 1;
            }

            return result;
        }


        public void StartPlacement(byte type)
        {
            CurrentId = type;
            _interaction.Set(InteractionState.Building);
        }

        public BuildingConfig GetConfig(int type)
        {
            return _config.Get().buildings[type % _config.Get().buildings.Length];
        }
    }
}