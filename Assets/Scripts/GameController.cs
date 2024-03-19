using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq.Expressions;

public class GameController : MonoBehaviour
{
    public GameObject canvas;
    public GameObject goodsCanvas;
    public GameObject GoodsPrefab;
    public GameObject letterPrefab;

    private int selectedObject = -1;
    private Color originalColor;

    float[] agentOneValues = { 5f, 3f, 5f, 4f};
    float[] agentTwoValues = { 1f, 4f, 3f, 2f};

    List<GameObject> AGoods = new List<GameObject>();
    List<GameObject> BGoods = new List<GameObject>();

    List<GameObject> A1Goods = new List<GameObject>();
    List<GameObject> A2Goods = new List<GameObject>();
    List<GameObject> B1Goods = new List<GameObject>();
    List<GameObject> B2Goods = new List<GameObject>();

    List<GameObject> tempGoods = new List<GameObject>();

    float spacing = 20f; // Adjust spacing between goods as needed
    float goodsHeight = 75f; // Original height of the goods    

    void Start()
    {
        Debug.Log("STARTING");

        RectTransform canvasRectTransform = goodsCanvas.GetComponent<RectTransform>();
        float canvasHeight = canvasRectTransform.rect.height;

        // Calculate the max height of the goods to then scale them to always fit the canvas
        float maxHeight = 0;
        float totalSpacing = 0;
        for (int i = 0; i < agentOneValues.Length; i++)
        {
            maxHeight += (Mathf.Max(agentOneValues[i], agentTwoValues[i]) * goodsHeight);
            totalSpacing += spacing;
        }

        float size = (canvasHeight - 60 + spacing - totalSpacing) / maxHeight; // Minus to account for the top and bottom margins
        float totalHeight = 30f; // Start 30 units above the bottom of the canvas

        for (int i = 0; i < agentOneValues.Length; i++)
        {
            // Instantiate the goods
            GameObject GoodA = Instantiate(GoodsPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            GoodA.transform.SetParent(goodsCanvas.transform, false);
            AGoods.Add(GoodA);
            GameObject GoodB = Instantiate(GoodsPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            GoodB.transform.SetParent(goodsCanvas.transform, false);
            BGoods.Add(GoodB);

            // Resize the goods to match the values
            RectTransform rectTransformA = GoodA.GetComponent<RectTransform>();
            rectTransformA.sizeDelta = new Vector2(rectTransformA.sizeDelta.x, goodsHeight * size * agentOneValues[i]);
            RectTransform rectTransformB = GoodB.GetComponent<RectTransform>();
            rectTransformB.sizeDelta = new Vector2(rectTransformB.sizeDelta.x, goodsHeight * size * agentTwoValues[i]);

            // Position the goods
            float yPos = (-canvasHeight / 2) + totalHeight + ((goodsHeight * size * Mathf.Max(agentOneValues[i], agentTwoValues[i])) / 2); // Starts at the bottom of the canvas, adds the prev goods, calculates size of the current good and adds half of it (for the middle)
            GoodA.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50, yPos);
            GoodB.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, yPos);

            // Update the total height
            totalHeight += (goodsHeight * size * Mathf.Max(agentOneValues[i], agentTwoValues[i])) + spacing; // Update total height including spacing

            // Change the box collider to match the size of the good
            BoxCollider2D boxColliderA = GoodA.GetComponent<BoxCollider2D>();
            boxColliderA.size = rectTransformA.sizeDelta;
            BoxCollider2D boxColliderB = GoodB.GetComponent<BoxCollider2D>();
            boxColliderB.size = rectTransformB.sizeDelta;
         
            // Add the letter to the goods
            AddLetterToGood(GoodA, "A" + (i + 1).ToString());
            AddLetterToGood(GoodB, "B" + (i + 1).ToString());

            // Generate random color for each pair
            Color randomColor = new Color((Random.Range(0f, 1.0f) * 1.25f), (Random.Range(0f, 1.0f) * 1.25f), (Random.Range(0f, 1.0f) * 1.25f));
            GoodA.GetComponent<Image>().color = randomColor;
            GoodB.GetComponent<Image>().color = randomColor;

            // Add OnClick event listener to each instantiated object
            int index = i; // Store the index in a local variable
            GoodA.GetComponent<Button>().onClick.AddListener(() => OnObjectClicked(index));
            GoodB.GetComponent<Button>().onClick.AddListener(() => OnObjectClicked(index));
        }
    }

    void OnObjectClicked(int index)
    {
        GameObject selectedGoodA = AGoods[index];
        GameObject selectedGoodB = BGoods[index];


        // ----------------- COLOR CHANGING CODE ----------------- //

        Image selectedImageA = selectedGoodA.GetComponent<Image>();
        Image selectedImageB = selectedGoodB.GetComponent<Image>();

        // If same good is selected again, restore their original colors
        if (index == selectedObject)
        {
            selectedImageA.color = originalColor;
            selectedImageB.color = originalColor;

            for(int i = 0; i < tempGoods.Count; i++) {
                Destroy(tempGoods[i]);
            }

            selectedObject = -1;
        }
        else 
        {
            // restore the original color of the previously selected good, only if there is a previous selection
            if (selectedObject != -1)
            {
                GameObject PREVselectedGoodA = AGoods[selectedObject];
                GameObject PREVselectedGoodB = BGoods[selectedObject];

                Image PREVselectedImageA = PREVselectedGoodA.GetComponent<Image>();
                Image PREVselectedImageB = PREVselectedGoodB.GetComponent<Image>();

                PREVselectedImageA.color = originalColor;
                PREVselectedImageB.color = originalColor;

                for (int i = 0; i < tempGoods.Count; i++){
                    Destroy(tempGoods[i]);
                }

            }

            // Dim the color of the newly selected good:
            originalColor = selectedImageA.color;
            
            Color dimColor = originalColor;
            dimColor.a = 0.25f;
            
            selectedImageA.color = dimColor;
            selectedImageB.color = dimColor;

            selectedObject = index;


            // ----------------- GENERATING DUPLICATES TO PREVIEW MOVE ----------------- //

            // Create a duplicate of the selected pair in the center of the canvas
            GameObject duplicateGoodA1 = Instantiate(selectedGoodA, Vector3.zero, Quaternion.identity, canvas.transform);
            GameObject duplicateGoodA2 = Instantiate(selectedGoodA, Vector3.zero, Quaternion.identity, canvas.transform);
            GameObject duplicateGoodB1 = Instantiate(selectedGoodB, Vector3.zero, Quaternion.identity, canvas.transform);
            GameObject duplicateGoodB2 = Instantiate(selectedGoodB, Vector3.zero, Quaternion.identity, canvas.transform);

            // Get the RectTransform of the duplicate goods
            RectTransform rectTransformA1 = duplicateGoodA1.GetComponent<RectTransform>();
            RectTransform rectTransformA2 = duplicateGoodA2.GetComponent<RectTransform>();
            RectTransform rectTransformB1 = duplicateGoodB1.GetComponent<RectTransform>();
            RectTransform rectTransformB2 = duplicateGoodB2.GetComponent<RectTransform>();

            // Set the pivot to the bottom center
            rectTransformA1.pivot = new Vector2(0.5f, 0f);
            rectTransformA2.pivot = new Vector2(0.5f, 0f);
            rectTransformB1.pivot = new Vector2(0.5f, 0f);
            rectTransformB2.pivot = new Vector2(0.5f, 0f);

            // Position the duplicate goods in the center of the canvas
            duplicateGoodA1.GetComponent<RectTransform>().anchoredPosition = new Vector2(-215, -275);
            duplicateGoodA2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-15, -275);
            duplicateGoodB1.GetComponent<RectTransform>().anchoredPosition = new Vector2(275, -275);
            duplicateGoodB2.GetComponent<RectTransform>().anchoredPosition = new Vector2(475, -275);

            //add the duplicate goods to the tempGoods list so they can be destroyed later
            tempGoods.Add(duplicateGoodA1);
            tempGoods.Add(duplicateGoodA2);
            tempGoods.Add(duplicateGoodB1);
            tempGoods.Add(duplicateGoodB2);
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
