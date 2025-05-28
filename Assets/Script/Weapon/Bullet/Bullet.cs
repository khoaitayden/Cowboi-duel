using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]private float speed;
    [SerializeField] private float lifeTime;
    private float timer;

    private Vector3 direction;

    public void Initialize(Vector3 dir)
    {
        direction = dir;
        timer = 0f;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        BulletPool.Instance.ReturnBullet(gameObject);
    }
}