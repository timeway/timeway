using Unity.VisualScripting;
using UnityEngine;

namespace Timeway.Interfaces
{
    public interface ICurable
    {
        float Health { get; set; }
        float HealthCapacity { get; set; }
        void TakeHealth(float m_Amount, GameObject @this);
    }
}
