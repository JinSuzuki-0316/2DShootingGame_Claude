using UnityEngine;

/// <summary>
/// 自機の下方向へ発射され、地面（レイヤー "Ground"）に着地すると
/// 地形に沿って這うように前進するミサイル。
/// </summary>
public class CrawlingMissile : MonoBehaviour
{
    public float fallSpeed = 6f;
    public float crawlSpeed = 5f;
    public int damage = 2;
    public float groundCheckDistance = 0.6f;

    private bool isCrawling = false;
    private float direction = 1f; // 1: 右向き, -1: 左向き

    private void Update()
    {
        if (!isCrawling)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance);
            if (hit.collider != null && hit.collider.GetComponent<GroundTag>() != null)
            {
                isCrawling = true;
                transform.position = hit.point;
            }
        }
        else
        {
            CrawlAlongGround();
        }

        if (transform.position.x < -12f || transform.position.x > 12f || transform.position.y < -8f)
        {
            Destroy(gameObject);
        }
    }

    private void CrawlAlongGround()
    {
        Vector2 origin = transform.position + Vector3.right * direction * 0.1f + Vector3.up * 0.5f;
        RaycastHit2D groundAhead = Physics2D.Raycast(origin, Vector2.down, 1.5f);

        if (groundAhead.collider != null && groundAhead.collider.GetComponent<GroundTag>() != null)
        {
            Vector3 target = new Vector3(origin.x, groundAhead.point.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, target, crawlSpeed * Time.deltaTime);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
