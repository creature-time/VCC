
using UdonSharp;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CtSequenceNode : CtGroupNode
    {
        public override void OnEnter(CtNpcContext context)
        {
            _index = 0;
            if (children.Length > 0)
                children[_index].OnEnter(context);
        }

        public override ENodeStatus Process(CtNpcContext context)
        {
            if (_index < children.Length)
            {
                switch (children[_index].Process(context))
                {
                    case ENodeStatus.Running:
                        return ENodeStatus.Running;
                    case ENodeStatus.Failure:
                        children[_index].OnExit(context);
                        return ENodeStatus.Failure;
                    default:
                        children[_index].OnExit(context);

                        _index++;
                        if (_index == children.Length)
                        {
                            return ENodeStatus.Success;
                        }

                        children[_index].OnEnter(context);
                        return ENodeStatus.Running;
                }
            }

            return ENodeStatus.Success;
        }
    }
}