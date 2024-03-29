using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProtagonistController : MonoBehaviour
{
    public GameManager gameManager;
    public UIManager uiManager;
    public CapsuleCollider2D safeArea;
    public CapsuleCollider2D coreArea;
    public Transform readout;

    public float moveSpeed = 20f;

    public float health = 10;
    public int healthIncrement = 5;
    public float healthRegenPercent = 0.1f;
    public int healthRegenTime = 3;
    public int powerLevel = 1;
    public float skillLevel = 0.1f;

    public float healthToSkillFactor = 3f;
    public float timeToSkillFactor = 0.01f;
    public float powerToDamageFactor = 5f;
    public float skillToFrequencyFactor = 1f;
    public float skillToIntelligenceFactor = 1f;
    public float healthThreshold = 0.5f;

    public int damage;
    public float attackFrequency;
    public float intelligence;

    public int currentXP = 0;
    public int xpThreshold = 10;
    public float xpThresholdIncrease = 2;

    public int levelsToNewPreset = 5;

    public float spinSpeed = 100f;
    public float spinDuration = 0.5f;

    private bool outOfBounds = false;

    private float attackCooldown;
    private float attackCounter = 0;
    private float abilityCooldown;
    private float abilityCounter = 0;

    private float spinCounter = 0;

    private int initialHealth;

    private float lastDamageCounter = 0;

    void Start()
    {
        initialHealth = (int)health;
        calculateStats();
    }

    void FixedUpdate()
    {
        if (health <= 0) gameManager.endGame();
        abilityCounter += Time.fixedDeltaTime;
        attackCounter += Time.fixedDeltaTime;
        lastDamageCounter += Time.fixedDeltaTime;
        if (lastDamageCounter > healthRegenTime)
        {
            health += healthRegenPercent * initialHealth;
            if (health > initialHealth) health = initialHealth;
        }
        skillLevel += timeToSkillFactor * Time.fixedDeltaTime * healthToSkillFactor * (1 - health / initialHealth);
        updateReadout();
        GameObject target = selectTarget();
        if (target == null) return;
        movement(target);
        rotation(target);
        attack(target);
    }

    public void registerHit(int damage)
    {
        health -= damage;
        lastDamageCounter = 0;
    }
    public void addXP(int xp)
    {
        currentXP += xp;
        if (currentXP >= xpThreshold)
        {
            powerLevel++;
            currentXP -= xpThreshold;
            xpThreshold = (int)(xpThreshold * xpThresholdIncrease);
            initialHealth += healthIncrement;

            calculateStats();
        }
    }

    private void calculateStats()
    {
        damage = (int)(powerLevel * powerToDamageFactor);
        attackFrequency = skillLevel * skillToFrequencyFactor;
        intelligence = skillLevel * skillToIntelligenceFactor;

        attackCooldown = 1f / attackFrequency;
        abilityCooldown = 1f / intelligence;
    }
    private void updateReadout()
    {
        readout.GetChild(1).GetComponent<Image>().fillAmount = (float)health / initialHealth;
        readout.GetChild(3).GetComponent<Image>().fillAmount = (float)currentXP / xpThreshold;
        readout.GetChild(5).GetComponent<Image>().fillAmount = skillLevel;
        readout.GetChild(7).GetComponent<TextMeshProUGUI>().text = (int)health + "/" + initialHealth;
        readout.GetChild(8).GetComponent<TextMeshProUGUI>().text = currentXP + "/" + xpThreshold;
        readout.GetChild(9).GetComponent<TextMeshProUGUI>().text = "Power: " + powerLevel;
    }
    private GameObject selectTarget()
    {
        GameObject target = null;
        float minDist = 1000f;
        for (int idx = 0; idx < gameManager.enemySlots.Count; idx++)
        {
            if (gameManager.enemySlots[idx] == null) continue;
            Vector2 diff = gameManager.enemySlots[idx].transform.position - this.transform.position;
            float distance = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
            if (distance < minDist)
            {
                minDist = distance;
                target = gameManager.enemySlots[idx];
            }
        }
        return target;
    }
    private void movement(GameObject target)
    {
        Vector2 direction = target.transform.position - this.transform.position;
        if (!safeArea.OverlapPoint(this.transform.position))
        {
            outOfBounds = true;
        }
        else if (coreArea.OverlapPoint(this.transform.position))
        {
            outOfBounds = false;
        }
        if (target.GetComponent<EnemyController>().damage > health * healthThreshold)
        {
            direction = -direction;
        }
        if (outOfBounds)
        {
            direction = coreArea.ClosestPoint(this.transform.position) - (Vector2)this.transform.position;
        }
        direction.Normalize();
        Vector3 moveStep = direction * moveSpeed * Time.fixedDeltaTime;
        this.transform.position += moveStep;
    }
    private void rotation(GameObject target)
    {
        if (spinCounter > 0)
        {
            Vector3 currentRotation = this.transform.rotation.eulerAngles;
            this.transform.rotation = Quaternion.Euler(currentRotation + Vector3.forward * spinSpeed * Time.fixedDeltaTime);
            spinCounter -= Time.fixedDeltaTime;
        }
        else
        {
            Vector2 direction = target.transform.position - this.transform.position;
            direction.Normalize();
            float angle = Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x) - 90;
            this.transform.rotation = Quaternion.Euler(Vector3.forward * angle);
        }
    }
    private void dodge()
    {

    }
    private void attack(GameObject target)
    {
        if (attackCounter >= attackCooldown)
        {
            Vector2 diff = target.transform.position - this.transform.position;
            float distance = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
            if (this.gameObject.GetComponent<CircleCollider2D>().Distance(target.GetComponent<CircleCollider2D>()).distance < gameManager.meleeDistance)
            {
                target.GetComponent<EnemyController>().registerHit(damage);
                spinCounter = spinDuration;
                this.transform.GetChild(0).GetComponent<Image>().sprite = gameManager.swordSprite;
            }
            else
            {
                gameManager.fireArrow(this.gameObject, target, false);
                this.transform.GetChild(0).GetComponent<Image>().sprite = gameManager.bowSprite;
            }
            attackCounter = 0;
        }
    }
}
