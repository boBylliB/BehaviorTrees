using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    private Vector2 trajectory;
    private float speed;
    private int damage;
    private bool enemy;
    private BoxCollider2D gameBounds;
    private List<Collider2D> contacts = new List<Collider2D>();
    private ContactFilter2D filter = new ContactFilter2D();

    public void initializeArrow(Vector2 trajectory, float speed, int damage, bool fromEnemy, Vector3 startPosition, BoxCollider2D gameBounds)
    {
        this.gameBounds = gameBounds;
        this.trajectory = trajectory.normalized;
        this.speed = speed;
        this.damage = damage;
        enemy = fromEnemy;
        this.transform.position = startPosition;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(trajectory.y, trajectory.x) - 90;
        this.transform.rotation = Quaternion.Euler(Vector3.forward * angle);
        LayerMask layerMask = new LayerMask();
        if (enemy) layerMask.value = 6;
        else layerMask.value = 7;
        filter.SetLayerMask(layerMask);
        filter.NoFilter();
    }
    void FixedUpdate()
    {
        movement();
        checkForCollision();
    }
    private void movement()
    {
        Vector3 moveStep = trajectory * speed * Time.fixedDeltaTime;
        this.transform.position += moveStep;
    }
    private void checkForCollision()
    {
        if (!gameBounds.OverlapPoint(this.transform.position))
        {
            Destroy(this.gameObject);
        }
        contacts.Clear();
        this.gameObject.GetComponent<BoxCollider2D>().OverlapCollider(filter, contacts);
        for (int idx = 0; idx < contacts.Count; idx++)
        {
            if (enemy && contacts[idx].gameObject.CompareTag("Protagonist"))
            {
                contacts[idx].gameObject.GetComponent<ProtagonistController>().registerHit(damage);
                Destroy(this.gameObject);
            }
            else if (!enemy && contacts[idx].gameObject.CompareTag("Enemy"))
            {
                contacts[idx].gameObject.GetComponent<EnemyController>().registerHit(damage);
                Destroy(this.gameObject);
            }
        }
    }
}
