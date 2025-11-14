using UnityEngine;

public class CoinsManager : MonoBehaviour
{
    public int coins;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            gameObject.SetActive(false);
            coins++;
        }
    }
}
