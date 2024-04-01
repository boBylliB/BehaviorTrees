using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public GameObject arrowhead;
    public GameObject line;
    public float arrowheadHeight;
    public float lineWidthScale;

    private Vector2 startPos;
    private Vector2 endPos;

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        setPoints(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), mousePos);
    }

    public void setPoints(Vector2 startPos, Vector2 endPos)
    {
        if (startPos != null)
            this.startPos = startPos;
        if (endPos != null)
            this.endPos = endPos;
        updateTransform();
    }

    private void updateTransform()
    {
        float angle = Mathf.Atan2(-(endPos.x - startPos.x), (endPos.y - startPos.y));
        Vector2 endOffset = new Vector2(-arrowheadHeight * Mathf.Sin(angle), arrowheadHeight * Mathf.Cos(angle)) / 2;
        arrowhead.transform.position = endPos - endOffset;
        arrowhead.transform.eulerAngles = new Vector3(0,0, angle * 180 / Mathf.PI);
        line.transform.position = (endPos - endOffset + startPos) / 2;
        line.transform.localScale = new Vector3(lineWidthScale, Vector3.Distance(endPos - endOffset, startPos) / arrowheadHeight);
        line.transform.eulerAngles = new Vector3(0, 0, angle * 180 / Mathf.PI);
    }
}
