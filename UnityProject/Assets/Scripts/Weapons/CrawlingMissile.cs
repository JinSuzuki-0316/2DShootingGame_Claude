using UnityEngine;

/// <summary>
/// 自機の下方向へ発射され、地面に着地すると地形に沿って
/// 這うように前進する対地ミサイル。
/// 落下中は下向き、着地後は水平向きに姿勢を変え、
/// 地面に少しめり込ませることで「地表を這っている」見た目にする。
/// </summary>
public class CrawlingMissile : MonoBehaviour
{
    public float fallSpeed = 6f;
    public float crawlSpeed = 5f;
    public int damage = 2;
    public float groundCheckDistance = 0.8f;
    public float groundEmbedOffset = 0.12f; // 着地時に地面へ少しめり込ませる量

    private bool isCrawling = false;
    private float direction = 1f; // 1: 右向き, -1: 左向き
    private TrailRenderer trail;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }

    private void Update()
    {
        if (!isCrawling)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, 0f, -90f); // 落下中は下向き

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance);
            if (hit.collider != null && hit.collider.GetComponent<GroundTag>() != null)
            {
                Land(hit.point);
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

    private void Land(Vector2 groundPoint)
    {
        isCrawling = true;
        transform.position = groundPoint + Vector2.up * groundEmbedOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, direction > 0 ? 0f : 180f); // 地面に着地したら水平向きに

        // 這行中は飛行中の尾（トレイル）を消し、地面に張り付いている見た目にする
        if (trail != null)
        {
            trail.Clear();
            trail.emitting = false;
        }
    }

    private void CrawlAlongGround()
    {
        Vector2 origin = transform.position + Vector3.right * direction * 0.1f + Vector3.up * 0.5f;
        RaycastHit2D groundAhead = Physics2D.Raycast(origin, Vector2.down, 1.5f);

        if (groundAhead.collider != null && groundAhead.collider.GetComponent<GroundTag>() != null)
        {
            Vector3 target = new Vector3(origin.x, groundAhead.point.y + groundEmbedOffset, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, target, crawlSpeed * Time.deltaTime);
        }
        else
        {
            // 地面が途切れたら消滅（崖など）
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

