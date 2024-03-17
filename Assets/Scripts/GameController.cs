using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
public class GameController : MonoBehaviour
{
    public GameObject goodsCanvas;
    public GameObject Goods;
    public GameObject letterPrefab;

    float[] agentOneValues = { 5f, 3f, 5f, 4f};
    float[] agentTwoValues = { 1f, 4f, 3f, 2f};

    List<GameObject> AGoods = new List<GameObject>();
    List<GameObject> BGoods = new List<GameObject>();

    float spacing = 20f; // Adjust spacing between goods as needed
    float goodsHeight = 75f; // Original height of the goods    

    void Start()
    {
        Debug.Log("STARTING");

        RectTransform canvasRectTransform = goodsCanvas.GetComponent<RectTransform>();
        float canvasHeight = canvasRectTransform.rect.height;
        
        float maxHeight = 0;
        float totalSpacing = 0;
        for (int i = 0; i < agentOneValues.Length; i++)
        {
            maxHeight += (Mathf.Max(agentOneValues[i], agentTwoValues[i]) * goodsHeight); 
            totalSpacing += spacing;
        }

        float size = (canvasHeight-60+spacing-totalSpacing) / maxHeight; //minus to account for the top and bottom margins
        float totalHeight = 30f; // Start 30 units above the bottom of the canvas

        for (int i = 0; i < agentOneValues.Length; i++)
        {
            GameObject GoodA = Instantiate(Goods, Vector3.zero, Quaternion.identity) as GameObject;
            GoodA.transform.SetParent(goodsCanvas.transform, false);
            AGoods.Add(GoodA);

            GameObject GoodB = Instantiate(Goods, Vector3.zero, Quaternion.identity) as GameObject;
            GoodB.transform.SetParent(goodsCanvas.transform, false);
            BGoods.Add(GoodB);

            RectTransform rectTransformA = GoodA.GetComponent<RectTransform>();
            rectTransformA.sizeDelta = new Vector2(rectTransformA.sizeDelta.x, goodsHeight * size * agentOneValues[i]);

            RectTransform rectTransformB = GoodB.GetComponent<RectTransform>();
            rectTransformB.sizeDelta = new Vector2(rectTransformB.sizeDelta.x, goodsHeight * size * agentTwoValues[i]);

            AddLetterToGood(GoodA, "A" + (i + 1).ToString());
            AddLetterToGood(GoodB, "B" + (i + 1).ToString());

            float yPos = (-canvasHeight / 2) + totalHeight + ((goodsHeight * size * Mathf.Max(agentOneValues[i], agentTwoValues[i])) / 2); //starts at the bottom of the canvas, adds the prev goods, calculates size of the current good and adds half of it (for the middle)
            GoodA.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50, yPos); 
            GoodB.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, yPos);

            totalHeight += (goodsHeight * size * Mathf.Max(agentOneValues[i], agentTwoValues[i])) + spacing; // Update total height including spacing

            // Generate random color for each pair
            Color randomColor = new Color(Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f));
            GoodA.GetComponent<Image>().color = randomColor;
            GoodB.GetComponent<Image>().color = randomColor;
        }

    }

    void AddLetterToGood(GameObject good, string letter)
    {
        GameObject letterObject = Instantiate(letterPrefab, good.transform.position, Quaternion.identity) as GameObject;
        letterObject.transform.SetParent(good.transform, false);

        TextMeshProUGUI letterText = letterObject.GetComponent<TextMeshProUGUI>();
        if (letterText != null)
        {
            letterText.text = letter;
            letterText.alignment = TextAlignmentOptions.Center;
            letterText.verticalAlignment = VerticalAlignmentOptions.Middle;

            RectTransform rectTransform = good.GetComponent<RectTransform>();
            float maxWidth = rectTransform.sizeDelta.x;
            float maxHeight = rectTransform.sizeDelta.y;
            float fontSize = Mathf.Min(maxWidth / letterText.preferredWidth, maxHeight / letterText.preferredHeight) * letterText.fontSize;

            letterText.fontSize = Mathf.Min(fontSize, letterText.fontSize);
        }
    }
}
