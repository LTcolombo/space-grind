using Service;
using Utils;
using xNode_1._8._0.Scripts;

namespace Nodes
{
    [NodeTint("#994422")]
    public class MintAgentNode : Node, IDialogueNode
    {
        [Input(ShowBackingValue.Never)] public string @in;

        public byte id;

        public async void Trigger()
        {
            await SettlementService.Instance.BuyAgent(id);
            await Bootstrap.DelayAsync(2);
            _ = SettlementService.Instance.ReloadData();
        }
    }
}