using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CapsuleCollider2D safeArea;
    public CapsuleCollider2D coreArea;
    public BoxCollider2D gameBounds;

    public GameObject protagonist;
    public GameObject endScreen;
    public GameObject pauseScreen;

    public Sprite swordSprite;
    public Sprite bowSprite;

    public int maxEnemies = 20;

    public Transform arrowParent;
    public GameObject arrowPrefab;
    public float arrowSpeed;
    public float arrowDamagePercent;

    public float meleeDistance = 20;

    public TextMeshProUGUI enemyCountDisplay;

    public GameObject enemyLayer;
    public GameObject enemyPrefab;
    public List<GameObject> enemySlots = new List<GameObject>();

    public bool paused = false;

    void Update()
    {
        
    }

    private void syncEnemySlots()
    {
        enemySlots.Clear();
        for (int idx = 0; idx < enemyLayer.transform.childCount; idx++)
        {
            enemySlots.Add(enemyLayer.transform.GetChild(idx).gameObject);
        }
        enemyCountDisplay.text = enemySlots.Count + "/" + maxEnemies;
    }

    public void setPause(bool desiredPause)
    {
        if (desiredPause)
        {
            paused = true;
            Time.timeScale = 0;
        }
        else
        {
            paused = false;
            Time.timeScale = 1;
        }
    }
    public void togglePause()
    {
        setPause(!paused);
        pauseScreen.SetActive(paused);
    }
    public bool canSpawnNewEnemy()
    {
        return (!paused && enemySlots.Count < maxEnemies);
    }
    public void spawnNewEnemy(Card card)
    {
        GameObject newEnemy = Instantiate(enemyPrefab, enemyLayer.transform);
        newEnemy.GetComponent<EnemyController>().initializeEnemy(card, this, protagonist);
        float yPosition = (1200 - 300) * Random.value + 300;
        if (protagonist.transform.position.x > 960) newEnemy.transform.position = new Vector2(2000,yPosition);
        else newEnemy.transform.position = new Vector2(-80,yPosition);
        syncEnemySlots();
    }
    public void removeEnemy(GameObject enemy)
    {
        Destroy(enemy);
        syncEnemySlots();
    }
    public void fireArrow(GameObject source, GameObject target, bool fromEnemy)
    {
        GameObject newArrow = Instantiate(arrowPrefab, arrowParent);
        Vector2 trajectory = target.transform.position - source.transform.position;
        int damage;
        if (fromEnemy) damage = source.GetComponent<EnemyController>().damage;
        else damage = source.GetComponent<ProtagonistController>().damage;
        newArrow.GetComponent<ArrowController>().initializeArrow(trajectory, arrowSpeed, (int)(damage * arrowDamagePercent), fromEnemy, source.transform.position, gameBounds);
    }
    public void endGame()
    {
        setPause(true);
        endScreen.SetActive(true);
        endScreen.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Power Level: " + protagonist.GetComponent<ProtagonistController>().powerLevel;
        endScreen.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Skill Level: " + protagonist.GetComponent<ProtagonistController>().skillLevel + "%";
        endScreen.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Total XP Gained: " + protagonist.GetComponent<ProtagonistController>().currentXP;
    }
}
