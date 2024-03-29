using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    public int cardID;
    public GameManager gameManager;
    public UIManager uiManager;
    public CardManager cardManager;
    public BoxCollider2D favoritesBox;

    public float liftHeight = 200;
    public float liftSpeed = 5;

    public float dropHeight = -100;
    public float activationHeight = 100;
    public float gravity = 9.81f;
    public float spinAccel = 100f;

    public float selectionBarHeight = 0;

    public bool selected = false;

    private float initialHeight;
    private float finalHeight;
    private float prevInterp = 0;

    private float fallSpeed = 0;
    private float spinSpeed = 0;

    private bool isCopy = false;
    private bool falling = false;
    private bool activated = false;
    private Vector3 mouseOffset = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        prevInterp = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCopy)
        {
            checkForSelect();
            cosInterp();
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (Input.mousePosition.y > selectionBarHeight && gameManager.canSpawnNewEnemy() && cardManager.getCard(this.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text) != null)
                {
                    activated = true;
                }
                else
                {
                    if (favoritesBox.OverlapPoint(Input.mousePosition))
                    {
                        cardManager.toggleFavorite(this.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text);
                        uiManager.updateDisplayOrder();
                    }
                    initialHeight = this.transform.position.y;
                    finalHeight = dropHeight;
                    falling = true;
                }
                prevInterp = 0;
            }
            if (!falling && !activated)
            {
                followMouse();
            }
            else if (falling)
            {
                fall();
            }
            else
            {
                activate();
            }
        }
    }

    public void initializeCopy(Vector3 initialCardPosition, Vector3 initialMousePosition)
    {
        isCopy = true;
        mouseOffset = initialCardPosition - initialMousePosition;
    }

    private void checkForSelect()
    {
        if (this.GetComponent<BoxCollider2D>().OverlapPoint(Input.mousePosition) && !uiManager.selectedCards.Contains(this.gameObject))
        {
            uiManager.addToSelectedCards(this.gameObject);
        }
        else if (!this.GetComponent<BoxCollider2D>().OverlapPoint(Input.mousePosition) && uiManager.selectedCards.Contains(this.gameObject))
        {
            uiManager.removeFromSelectedCards(this.gameObject);
        }
    }

    private void cosInterp()
    {
        float usedLiftSpeed = liftSpeed;
        if (prevInterp == 0)
        {
            initialHeight = this.transform.position.y;
            finalHeight = initialHeight + liftHeight;
        }
        if (!selected)
        {
            usedLiftSpeed = -usedLiftSpeed;
        }
        prevInterp = usedLiftSpeed * Time.deltaTime + prevInterp;
        if (prevInterp > 1) prevInterp = 1;
        else if (prevInterp < 0) prevInterp = 0;
        float scaledInterp = (1 - Mathf.Cos(prevInterp * Mathf.PI)) / 2;
        float newY = (initialHeight * (1 - scaledInterp) + finalHeight * scaledInterp);
        this.transform.position = new Vector3(this.transform.position.x, newY, this.transform.position.z);
    }

    private void followMouse()
    {
        this.transform.position = Input.mousePosition + mouseOffset;
    }
    private void fall()
    {
        fallSpeed += gravity * Time.deltaTime * 0.5f;
        spinSpeed += spinAccel * Time.deltaTime * 0.5f;
        this.transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        Vector3 currentRotation = this.transform.rotation.eulerAngles;
        this.transform.rotation = Quaternion.Euler(currentRotation + Vector3.forward * spinSpeed * Time.deltaTime);
        spinSpeed += spinAccel * Time.deltaTime * 0.5f;
        fallSpeed += gravity * Time.deltaTime * 0.5f;
        if (this.transform.position.y < dropHeight)
        {
            Destroy(this.gameObject);
        }
    }
    private void activate()
    {
        fallSpeed += gravity * Time.deltaTime * 0.5f;
        spinSpeed += spinAccel * Time.deltaTime * 0.5f;
        this.transform.position += Vector3.up * fallSpeed * Time.deltaTime;
        Vector3 currentRotation = this.transform.rotation.eulerAngles;
        this.transform.rotation = Quaternion.Euler(currentRotation + Vector3.back * spinSpeed * Time.deltaTime);
        spinSpeed += spinAccel * Time.deltaTime * 0.5f;
        fallSpeed += gravity * Time.deltaTime * 0.5f;
        if (this.transform.position.y > activationHeight)
        {
            gameManager.spawnNewEnemy(cardManager.getCard(this.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text));
            Destroy(this.gameObject);
        }
    }
}
