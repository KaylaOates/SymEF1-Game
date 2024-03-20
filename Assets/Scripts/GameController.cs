using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;

public class GameController : MonoBehaviour
{
    public GameObject canvas;
    public GameObject goodsCanvas;
    public GameObject GoodsPrefab;
    public GameObject letterPrefab;

    float[] agentOneValues = { 5f, 3f, 5f, 4f };
    float[] agentTwoValues = { 1f, 4f, 3f, 2f };

    //the curr selected object
    private int selectedObject = -1;

    //list of goods in side panel:
    List<GameObject> AGoods = new List<GameObject>();
    List<GameObject> BGoods = new List<GameObject>();

    List<Color> colors = new List<Color>();

    //list of goods in each bundle:
    List<GameObject> A1Goods = new List<GameObject>();
    List<GameObject> A2Goods = new List<GameObject>();
    List<GameObject> B1Goods = new List<GameObject>();
    List<GameObject> B2Goods = new List<GameObject>();

    //positions of objects in the bundles:
    float posA1 = 0f;
    float posA2 = 0f;
    float posB1 = 0f;
    float posB2 = 0f;

    //list of goods selected for preview:
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
            colors.Add(randomColor);
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
            selectedImageA.color = colors[index];
            selectedImageB.color = colors[index];

            DeleteAll();

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

                PREVselectedImageA.color = colors[selectedObject];
                PREVselectedImageB.color = colors[selectedObject];

                DeleteAll();

            }

            // Dim the color of the newly selected good:
            Color dimColor = colors[index];
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
            duplicateGoodA1.GetComponent<RectTransform>().anchoredPosition = new Vector2(-215, -275 + posA1);
            duplicateGoodA2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-15, -275 + posA2);
            duplicateGoodB1.GetComponent<RectTransform>().anchoredPosition = new Vector2(275, -275 + posB1);
            duplicateGoodB2.GetComponent<RectTransform>().anchoredPosition = new Vector2(475, -275 + posB2);

            // adding listener for if they get clicked 
            duplicateGoodA1.GetComponent<Button>().onClick.AddListener(() => OnTempGoodClicked(0));
            duplicateGoodA2.GetComponent<Button>().onClick.AddListener(() => OnTempGoodClicked(1));
            duplicateGoodB1.GetComponent<Button>().onClick.AddListener(() => OnTempGoodClicked(2));
            duplicateGoodB2.GetComponent<Button>().onClick.AddListener(() => OnTempGoodClicked(3));

            //add the duplicate goods to the tempGoods list so they can be destroyed later
            tempGoods.Add(duplicateGoodA1);
            tempGoods.Add(duplicateGoodA2);
            tempGoods.Add(duplicateGoodB1);
            tempGoods.Add(duplicateGoodB2);
        }
    }

    void OnTempGoodClicked(int duplicateIndex)
    {
        //add the good to the corresponding bundle:

        GameObject selectedGoodA = AGoods[selectedObject];
        GameObject selectedGoodB = BGoods[selectedObject];

        if (duplicateIndex == 0 || duplicateIndex == 2) 
        {
            // Create a duplicate of the selected pair in the center of the canvas
            GameObject addedGoodA1 = Instantiate(selectedGoodA, Vector3.zero, Quaternion.identity, canvas.transform);
            GameObject addedGoodB1 = Instantiate(selectedGoodB, Vector3.zero, Quaternion.identity, canvas.transform);

            // Get the RectTransform of the duplicate goods
            RectTransform rectTransformA1 = addedGoodA1.GetComponent<RectTransform>();
            RectTransform rectTransformB1 = addedGoodB1.GetComponent<RectTransform>();

            // Set the pivot to the bottom center
            rectTransformA1.pivot = new Vector2(0.5f, 0f);
            rectTransformB1.pivot = new Vector2(0.5f, 0f);

            // Position the duplicate goods in the center of the canvas
            addedGoodA1.GetComponent<RectTransform>().anchoredPosition = new Vector2(-215, -275 + posA1);
            addedGoodB1.GetComponent<RectTransform>().anchoredPosition = new Vector2(275, -275 + posB1);

            //restore the original color of the goods in the bundle
            Image selectedImageA = addedGoodA1.GetComponent<Image>();
            Image selectedImageB = addedGoodB1.GetComponent<Image>();
            selectedImageA.color = colors[selectedObject];
            selectedImageB.color = colors[selectedObject];

            // adding listener for if they get clicked 
            int i = selectedObject;
            addedGoodA1.GetComponent<Button>().onClick.AddListener(() => OnGoodClicked("A", i, 0, addedGoodA1));
            addedGoodB1.GetComponent<Button>().onClick.AddListener(() => OnGoodClicked("B", i, 1, addedGoodB1));

            A1Goods.Add(addedGoodA1);
            B1Goods.Add(addedGoodB1);

            // update the pos for the next good to be added:
            posA1 += addedGoodA1.GetComponent<RectTransform>().sizeDelta.y;
            posB1 += addedGoodB1.GetComponent<RectTransform>().sizeDelta.y;

            DeleteAll();

            //now remove the newly added good from the side panel:
            AGoods[selectedObject].SetActive(false);
            BGoods[selectedObject].SetActive(false);

        } 
        else if (duplicateIndex == 1 || duplicateIndex == 3)
        {

            // Create a duplicate of the selected pair in the center of the canvas
           
            GameObject addedGoodA2 = Instantiate(selectedGoodA, Vector3.zero, Quaternion.identity, canvas.transform);
            GameObject addedGoodB2 = Instantiate(selectedGoodB, Vector3.zero, Quaternion.identity, canvas.transform);

            // Get the RectTransform of the duplicate goods
            RectTransform rectTransformA2 = addedGoodA2.GetComponent<RectTransform>();
            RectTransform rectTransformB2 = addedGoodB2.GetComponent<RectTransform>();

            // Set the pivot to the bottom center
            rectTransformA2.pivot = new Vector2(0.5f, 0f);
            rectTransformB2.pivot = new Vector2(0.5f, 0f);

            // Position the duplicate goods in the center of the canvas
            addedGoodA2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-15, -275 + posA2);
            addedGoodB2.GetComponent<RectTransform>().anchoredPosition = new Vector2(475, -275 + posB2);

            //restore the original color of the goods in the bundle
            Image selectedImageA = addedGoodA2.GetComponent<Image>();
            Image selectedImageB = addedGoodB2.GetComponent<Image>();
            selectedImageA.color = colors[selectedObject];
            selectedImageB.color = colors[selectedObject];

            // adding listener for if they get clicked 
            int i = selectedObject;
            addedGoodA2.GetComponent<Button>().onClick.AddListener(() => OnGoodClicked("A", i, 2, addedGoodA2));
            addedGoodB2.GetComponent<Button>().onClick.AddListener(() => OnGoodClicked("B", i, 3, addedGoodB2));

            A2Goods.Add(addedGoodA2);
            B2Goods.Add(addedGoodB2);
            DeleteAll();

            // update the pos for the next good to be added:
            posA2 += addedGoodA2.GetComponent<RectTransform>().sizeDelta.y;
            posB2 += addedGoodB2.GetComponent<RectTransform>().sizeDelta.y;

            //now remove the newly added good from the side panel:
            AGoods[selectedObject].SetActive(false);
            BGoods[selectedObject].SetActive(false);
        }
    }

    void OnGoodClicked(string AB, int ABlistIndex, int bundle, GameObject clickedGood)
    { 
        //restore the good in the side panel:

        AGoods[ABlistIndex].SetActive(true);
        BGoods[ABlistIndex].SetActive(true);

        Image selectedImageA = AGoods[ABlistIndex].GetComponent<Image>();
        Image selectedImageB = BGoods[ABlistIndex].GetComponent<Image>();
        selectedImageA.color = colors[ABlistIndex];
        selectedImageB.color = colors[ABlistIndex];


        //destroy from bundle list:
        if(bundle == 0 || bundle == 1)
        {
            for(int i = 0; i < A1Goods.Count; i++)
            {
                if (A1Goods[i] == clickedGood || B1Goods[i] == clickedGood)
                {
                    //update position for future goods
                    posA1 -= A1Goods[i].GetComponent<RectTransform>().sizeDelta.y;
                    posB1 -= B1Goods[i].GetComponent<RectTransform>().sizeDelta.y;

                    //update positions above current good that's being removed:
                    for (int j = i + 1; j < A1Goods.Count; j++)
                    {
                        A1Goods[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(A1Goods[j].GetComponent<RectTransform>().anchoredPosition.x, A1Goods[j].GetComponent<RectTransform>().anchoredPosition.y - A1Goods[i].GetComponent<RectTransform>().sizeDelta.y);
                        B1Goods[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(B1Goods[j].GetComponent<RectTransform>().anchoredPosition.x, B1Goods[j].GetComponent<RectTransform>().anchoredPosition.y - B1Goods[i].GetComponent<RectTransform>().sizeDelta.y);
                    }

                    //delete the good
                    Destroy(A1Goods[i]);
                    Destroy(B1Goods[i]);
                    A1Goods.RemoveAt(i);
                    B1Goods.RemoveAt(i);
                    break;
                }
            }
        } 
        else if(bundle == 2 || bundle == 3)
        {
            for (int i = 0; i < A2Goods.Count; i++)
            {
                if (A2Goods[i] == clickedGood || B2Goods[i] == clickedGood)
                {
                    //update position for future goods
                    posA2 -= A2Goods[i].GetComponent<RectTransform>().sizeDelta.y;
                    posB2 -= B2Goods[i].GetComponent<RectTransform>().sizeDelta.y;

                    //update positions above current good that's being removed:
                    for (int j = i + 1; j < A2Goods.Count; j++)
                    {
                        A2Goods[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(A2Goods[j].GetComponent<RectTransform>().anchoredPosition.x, A2Goods[j].GetComponent<RectTransform>().anchoredPosition.y - A2Goods[i].GetComponent<RectTransform>().sizeDelta.y);
                        B2Goods[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(B2Goods[j].GetComponent<RectTransform>().anchoredPosition.x, B2Goods[j].GetComponent<RectTransform>().anchoredPosition.y - B2Goods[i].GetComponent<RectTransform>().sizeDelta.y);
                    }

                    //destroy the good
                    Destroy(A2Goods[i]);
                    Destroy(B2Goods[i]);
                    A2Goods.RemoveAt(i);
                    B2Goods.RemoveAt(i);
                    break;
                }
            }
        }

    }

    void DeleteAll()
    {
        for (int i = 0; i < tempGoods.Count; i++)
        {
            Destroy(tempGoods[i]);
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
