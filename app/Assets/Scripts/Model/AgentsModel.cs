using Settlement.Types;
using Utils.Injection;
using Utils.Signal;

namespace Model
{
    [Singleton]
    public class AgentsModel
    {
        public readonly Signal Updated = new();

        private Agent[] _data;

        public void Set(Agent[] value)
        {
            _data = value;
            Updated.Dispatch();
        }

        public Agent[] Get()
        {
            return _data;
        }
    }
}