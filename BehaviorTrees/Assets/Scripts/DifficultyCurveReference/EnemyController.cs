using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float spinSpeed = 100f;
    public float spinDuration = 0.5f;

    public float abilityDuration = 5f;

    public float powerToDamageFactor = 5f;
    public float skillToFrequencyFactor = 1f;
    public float skillToIntelligenceFactor = 1f;

    public Card originCard;
    public int health;
    public int xpDropped;
    public int damage;
    public float attackFrequency;
    public float intelligence;
    public AttackType attackType;

    private GameManager gameManager;
    private GameObject protagonist;

    private float attackCooldown;
    private float attackCounter = 0;
    private float abilityCooldown;
    private float abilityCounter = 0;
    private float abilityTimer = 0;

    private float initialHealth;

    private float spinCounter = 0;

    private float radius;

    public void initializeEnemy(Card card, GameManager gameManager, GameObject protagonist)
    {
        this.gameManager = gameManager;
        this.protagonist = protagonist;
        originCard = card;
        health = card.health;
        initialHealth = card.health;
        xpDropped = card.xpDropped;
        damage = (int)(card.powerLevel * powerToDamageFactor);
        attackFrequency = card.skillLevel * skillToFrequencyFactor / 100;
        intelligence = card.skillLevel * skillToIntelligenceFactor / 100;
        attackType = card.attackType;

        attackCooldown = 1f / attackFrequency;
        abilityCooldown = 1f / intelligence;

        switch (attackType)
        {
            case AttackType.Ranged:
                this.transform.GetChild(3).GetComponent<Image>().sprite = gameManager.bowSprite;
                break;
            default:
                this.transform.GetChild(3).GetComponent<Image>().sprite = gameManager.swordSprite;
                break;
        }

        this.radius = this.gameObject.GetComponent<CircleCollider2D>().radius;

        this.transform.GetChild(0).GetComponent<Image>().color = card.outerColor;
        this.transform.GetChild(1).GetComponent<Image>().color = card.innerColor;
    }

    void FixedUpdate()
    {
        movement();
        avoidCollision();
        rotation();
        attack();
        this.transform.GetChild(2).GetComponent<Image>().fillAmount = health / initialHealth;
        if (health <= 0) die();
    }

    public void registerHit(int damage)
    {
        health -= damage;
    }
    public void die()
    {
        protagonist.GetComponent<ProtagonistController>().addXP(xpDropped);
        gameManager.GetComponent<GameManager>().removeEnemy(this.gameObject);
    }

    private void movement()
    {
        Vector2 direction = protagonist.transform.position - this.transform.position;
        if (abilityCounter >= abilityCooldown)
        {
            abilityTimer = abilityDuration;
            abilityCounter = 0;
        }
        if (abilityTimer > 0)
        {
            switch (attackType)
            {
                case AttackType.Ranged:
                    direction = -direction;
                    break;
                default:
                    float minDist = 10000f;
                    Transform closestEnemy = null;
                    for (int idx = 0; idx < gameManager.enemySlots.Count; idx++)
                    {
                        if (gameManager.enemySlots[idx] == null) continue;
                        Vector2 diff = gameManager.enemySlots[idx].transform.position - this.transform.position;
                        float distance = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
                        if (distance < 0.00001) continue; //This is the current controller, so skip it
                        if (distance < minDist)
                        {
                            minDist = distance;
                            closestEnemy = gameManager.enemySlots[idx].transform;
                        }
                    }
                    if (closestEnemy != null)
                    {
                        if (minDist > gameManager.meleeDistance + 80) direction = closestEnemy.position - this.transform.position;
                        else abilityTimer = 0;
                    }
                    break;
            }
            abilityTimer -= Time.fixedDeltaTime;
        }
        if (!gameManager.safeArea.OverlapPoint(this.transform.position))
        {
            Vector2 position = this.transform.position;
            direction = gameManager.safeArea.ClosestPoint(this.transform.position) - position;
        }
        direction.Normalize();
        Vector3 moveStep = direction * moveSpeed * Time.fixedDeltaTime;
        this.transform.position += moveStep;
        abilityCounter += Time.fixedDeltaTime;
    }
    private void avoidCollision()
    {
        Vector2 requiredShift = Vector2.zero;
        Vector2 diff;
        float distance;
        for (int idx = 0; idx < gameManager.enemySlots.Count; idx++)
        {
            if (gameManager.enemySlots[idx] == null) continue;
            diff = gameManager.enemySlots[idx].transform.position - this.transform.position;
            distance = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
            if (distance < 0.00001) continue; //This is the current controller, so skip it
            if (distance < radius * 2)
            {
                requiredShift += (2 * radius - distance) * -diff.normalized;
            }
        }
        diff = protagonist.transform.position - this.transform.position;
        distance = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
        float minDist = radius + protagonist.GetComponent<CircleCollider2D>().radius;
        if (distance < minDist) requiredShift += (minDist - distance) * -diff.normalized;
        this.transform.position += (Vector3)requiredShift;
    }
    private void rotation()
    {
        if (spinCounter > 0)
        {
            Vector3 currentRotation = this.transform.rotation.eulerAngles;
            this.transform.rotation = Quaternion.Euler(currentRotation + Vector3.forward * spinSpeed * Time.fixedDeltaTime);
            spinCounter -= Time.fixedDeltaTime;
        }
        else
        {
            Vector2 direction = protagonist.transform.position - this.transform.position;
            direction.Normalize();
            float angle = Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x) - 90;
            this.transform.rotation = Quaternion.Euler(Vector3.forward * angle);
        }
    }
    private void attack()
    {
        if (attackCounter >= attackCooldown)
        {
            switch (attackType)
            {
                case AttackType.Ranged:
                    gameManager.fireArrow(this.gameObject, protagonist, true);
                    attackCounter = 0;
                    break;
                default:
                    if (this.gameObject.GetComponent<CircleCollider2D>().Distance(protagonist.GetComponent<CircleCollider2D>()).distance < gameManager.meleeDistance)
                    {
                        protagonist.GetComponent<ProtagonistController>().registerHit(damage);
                        spinCounter = spinDuration;
                        attackCounter = 0;
                    }
                    break;
            }
        }
        attackCounter += Time.fixedDeltaTime;
    }
}
