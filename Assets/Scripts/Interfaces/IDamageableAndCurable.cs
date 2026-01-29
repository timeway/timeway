namespace Timeway.Interfaces
{
    public interface IDamageableAndCurable
    {
        public float amount { get; set; }
        public void TakeDamageOrHealth(float m_Amount);
    }
}
