using Model;
using UnityEngine;
using xNode_1._8._0.Scripts;
using Node = xNode_1._8._0.Scripts.Node;

namespace Nodes
{
    [NodeTint("#992222")]
    public class CreditCheckNode : Node, IDialogueNode
    {
        [Input(ShowBackingValue.Never)] public string @in;

        public int value;
        public bool perBuilding;

        [SerializeField] [Output] private string @pass;

        [SerializeField] [Output] private string @fail;


        public void Trigger()
        {
            var toCheck = value * (perBuilding ? BuildingsModel.Instance.Get()?.Length ?? 0 : 1);
            GoTo(BalanceModel.Instance.Get() < toCheck ? GetPort("fail") : GetPort("pass"));
        }

        void GoTo(NodePort port)
        {
            for (var i = 0; i < port.ConnectionCount; i++)
            {
                var connection = port.GetConnection(i);
                if (connection.node is IDialogueNode node)
                {
                    node.Trigger();
                }
            }
        }
    }
}