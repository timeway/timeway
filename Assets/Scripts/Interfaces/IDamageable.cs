using Unity.VisualScripting;
using UnityEngine;

namespace Timeway.Interfaces
{
    public interface IDamageable
    {
        float Damage { get; set; }
        void TakeDamage(float m_Amount, GameObject @this);
    }
}
