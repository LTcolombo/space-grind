using Model;
using Service;
using xNode_1._8._0.Scripts;

namespace Nodes
{
    [NodeTint("#994422")]
    public class BuildNode : Node, IDialogueNode
    {
        
        [Input(ShowBackingValue.Never)] public string @in;
        public BuildingType building;

        public async void Trigger()
        {
            BuildingsModel.Instance.StartPlacement((byte)building);
        }
    }
}