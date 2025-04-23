
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtChooseTargetNode : CtBehaviorTreeNodeBase
    {
        private ushort[] _identifiers = { };
        private float[] _healthWeights = { };

        public override ENodeStatus Process(CtNpcContext context)
        {
            // CtNpcBehaviorUtils.AssertIfTargetIsValid(target);

            context.TryGetInt("Result/SkillIndex", out var skillIndex);

            bool isTargetEnemy = true;
            if (skillIndex != -1)
            {
                // Early out with self-targeting if skill used is self targeting only.
                context.TryGetInt($"Skills.Values[{skillIndex}]/IsSelfTargetOnly", out var isSelfTarget);
                if (isSelfTarget > 0)
                {
                    context.TryGetUShort("Self/Identifier", out var identifier);
                    context.SetInt("Result/TargetId", identifier);
                    return ENodeStatus.Success;
                }

                context.TryGetBool($"Skills.Values[{skillIndex}]/IsTargetEnemy", out isTargetEnemy);
            }

            context.TryGetFloat("Self/Party", out var isAllyTeam);

            if (isTargetEnemy)
            {
                if (isAllyTeam < 0)
                {
                    context.TryGetInt("Allies.Count", out var count);
                    if (_identifiers.Length != count || _healthWeights.Length != count)
                    {
                        _identifiers = new ushort[count];
                        _healthWeights = new float[count];
                    }

                    for (int i = 0; i < count; ++i)
                    {
                        context.TryGetUShort($"Allies.Values[{i}]/Identifier", out _identifiers[i]);
                        context.TryGetFloat($"Allies.Values[{i}]/Health", out _healthWeights[i]);
                    }
                }
                else
                {
                    context.TryGetInt("Enemies.Count", out var count);
                    if (_identifiers.Length != count || _healthWeights.Length != count)
                    {
                        _identifiers = new ushort[count];
                        _healthWeights = new float[count];
                    }

                    for (int i = 0; i < count; ++i)
                    {
                        context.TryGetUShort($"Enemies.Values[{i}]/Identifier", out _identifiers[i]);
                        context.TryGetFloat($"Enemies.Values[{i}]/Health", out _healthWeights[i]);
                    }
                }

                for (int i = 0; i < _healthWeights.Length; ++i)
                {
                    if (_healthWeights[i] <= 0)
                    {
                        _healthWeights[i] = 0;
                    }
                    else
                    {
                        _healthWeights[i] = Mathf.Max(1.0f - _healthWeights[i], 0.2f);
                    }
                }
            }
            else
            {
                if (isAllyTeam < 0)
                {
                    context.TryGetInt("Enemies.Count", out var count);
                    if (_identifiers.Length != count || _healthWeights.Length != count)
                    {
                        _identifiers = new ushort[count];
                        _healthWeights = new float[count];
                    }

                    for (int i = 0; i < count; ++i)
                    {
                        context.TryGetUShort($"Enemies.Values[{i}]/Identifier", out _identifiers[i]);
                        context.TryGetFloat($"Enemies.Values[{i}]/Health", out _healthWeights[i]);
                    }
                }
                else
                {
                    context.TryGetInt("Allies.Count", out var count);
                    if (_identifiers.Length != count || _healthWeights.Length != count)
                    {
                        _identifiers = new ushort[count];
                        _healthWeights = new float[count];
                    }

                    for (int i = 0; i < count; ++i)
                    {
                        context.TryGetUShort($"Allies.Values[{i}]/Identifier", out _identifiers[i]);
                        context.TryGetFloat($"Allies.Values[{i}]/Health", out _healthWeights[i]);
                    }
                }

                for (int i = 0; i < _healthWeights.Length; ++i)
                {
                    if (_healthWeights[i] <= 0)
                    {
                        _healthWeights[i] = 0;
                    }
                    else
                    {
                        _healthWeights[i] = Mathf.Max(1.0f - _healthWeights[i], 0.1f);
                    }
                }
            }

            var index = CtRandomizer.GetRandomFromArray(_healthWeights);
            if (index != -1)
            {
                context.SetUShort("Result/TargetId", _identifiers[index]);
                return ENodeStatus.Success;
            }

            return ENodeStatus.Failure;
        }
    }
}
