
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtChooseTargetNode : CtBehaviorTreeNodeBase
    {
        // private ushort[] _identifiers = { };
        private float[] _healthWeights = { };
        private DataList _identifiers = new DataList();
        private DataList _weights = new DataList();

        public override ENodeStatus Process(CtNpcContext context)
        {
#if DEBUG_LOGS
            LogDebug("CtChooseTargetNode");
#endif

            // CtNpcBehaviorUtils.AssertIfTargetIsValid(target);

            var targetType = ETargetType.SingleEnemy;
            context.TryGetInt("Result/SkillIndex", out var skillIndex);
            if (skillIndex != -1)
            {
                context.TryGetEnum($"Skills.Values[{skillIndex}]/TargetType", out targetType);
            }

            context.TryGetUShort("Self/Identifier", out var self);

            _identifiers.Clear();
            _weights.Clear();

            if (((int)targetType & (int)ETargetType.AllAllies) != 0)
            {
                context.TryGetInt("Allies.Count", out var count);
                for (int i = 0; i < count; ++i)
                {
                    context.TryGetUShort($"Allies.Values[{i}]/Identifier", out var identifier);
                    context.TryGetFloat($"Allies.Values[{i}]/Health", out var health);
                    _identifiers.Add(identifier);
                    _weights.Add(health);
                }
            }
            else
            {
                if (((int)targetType & (int)ETargetType.Self) != 0)
                {
                    _identifiers.Add(self);
                    context.TryGetFloat("Self/Health", out var selfHealth);
                    _weights.Add(selfHealth);
                }

                if (((int)targetType & (int)ETargetType.SingleAlly) != 0)
                {
                    context.TryGetInt("Allies.Count", out var count);
                    for (int i = 0; i < count; ++i)
                    {
                        context.TryGetUShort($"Allies.Values[{i}]/Identifier", out var identifier);
                        if (identifier == self) continue;
                        context.TryGetFloat($"Allies.Values[{i}]/Health", out var health);
                        _identifiers.Add(identifier);
                        _weights.Add(health);
                    }
                }
            }

            if (((int)targetType & (int)ETargetType.SingleEnemy) != 0 || ((int)targetType & (int)ETargetType.AllEnemies) != 0)
            {
                context.TryGetInt("Enemies.Count", out var count);
                for (int i = 0; i < count; ++i)
                {
                    context.TryGetUShort($"Enemies.Values[{i}]/Identifier", out var identifier);
                    context.TryGetFloat($"Enemies.Values[{i}]/Health", out var health);
                    _identifiers.Add(identifier);
                    _weights.Add(health);
                }
            }

            _healthWeights = new float[_weights.Count];
            for (int i = 0; i < _healthWeights.Length; ++i)
            {
                var health = _weights[i].Float;
                if (health <= 0)
                    _healthWeights[i] = 0;
                else
                    _healthWeights[i] = Mathf.Max(1.0f - health, 0.1f);
            }

            var index = CtRandomizer.GetRandomFromArray(_healthWeights);
            if (index != -1)
            {
                context.SetUShort("Result/TargetId", _identifiers[index].UShort);
                return ENodeStatus.Success;
            }
            else
            {
                // TODO: We know that this flow is kinda garbage. We need to make smarter decisions based on the
                //       state of the field, choose the target, then the skill based on that information.
#if DEBUG_LOGS
                LogDebug("Failed to find a target for skill... running fallback!");
#endif
                // Fallback if everything else fails...
                context.TryGetInt("Enemies.Count", out var count);
                _identifiers.Clear();
                _healthWeights = new float[count];
                for (int i = 0; i < count; ++i)
                {
                    context.TryGetUShort($"Enemies.Values[{i}]/Identifier", out var identifier);
                    _identifiers.Add(identifier);
                    context.TryGetFloat($"Enemies.Values[{i}]/Health", out var health);
                    if (health <= 0)
                        _healthWeights[i] = 0;
                    else
                        _healthWeights[i] = Mathf.Max(1.0f - health, 0.1f);
                }

                index = CtRandomizer.GetRandomFromArray(_healthWeights);
                context.SetUShort("Result/SkillId", CtConstants.InvalidId);
                context.SetUShort("Result/TargetId", _identifiers[index].UShort);

                return ENodeStatus.Success;
            }

            return ENodeStatus.Failure;
        }
    }
}
