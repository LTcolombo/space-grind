using System.Threading.Tasks;
using Service;
using xNode_1._8._0.Scripts;
using Bootstrap = Utils.Bootstrap;

namespace Nodes
{
    [NodeTint("#994422")]
    public class LevelUpNode : Node, IDialogueNode
    {
        [Input(ShowBackingValue.Never)] public string @in;

        public async void Trigger()
        {
            await SettlementService.Instance.LevelUpBuildings();
            await Bootstrap.DelayAsync(2);
            _ = SettlementService.Instance.ReloadData();
        }
    }
}