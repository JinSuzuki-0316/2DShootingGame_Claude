using UnityEngine;

/// <summary>
/// 敵撃破時にドロップし、自機が取得すると PowerUpManager のメーターを進める。
/// </summary>
public class PowerCapsule : MonoBehaviour
{
    public float driftSpeed = 2f;
    public float lifeTime = 10f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += Vector3.left * driftSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PowerUpManager pum = other.GetComponent<PowerUpManager>();
        if (pum != null)
        {
            pum.CollectCapsule();
        }

        Destroy(gameObject);
    }
}
