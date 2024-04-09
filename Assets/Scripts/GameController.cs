using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System;
using System.Net.NetworkInformation;

public class GameController : MonoBehaviour
{

    float[] agentOneValues = { 0f, 3f, 5f, 2f };
    float[] agentTwoValues = { 1f, 2f, 3f, 6f };

    public GameObject canvas;
    public GameObject goodsCanvas;
    public GameObject GoodsPrefab;
    public GameObject letterPrefab;
    public GameObject scorePrefab;
    public GameObject highScorePrefab;
    public GameObject winPrefab;

    public GameObject Adisparity;
    public GameObject Bdisparity;
    public GameObject AdistSymef1;
    public GameObject BdistSymef1;

    float A1Total = 0;
    float A2Total = 0;
    float B1Total = 0;
    float B2Total = 0;

    float HighScore = Mathf.Infinity;

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
        
        scorePrefab.GetComponent<Text>().text = "Score: 0";
        Adisparity.GetComponent<Text>().text = "Disparity: 0";
        Bdisparity.GetComponent<Text>().text = "Disparity: 0";
        AdistSymef1.GetComponent<Text>().text = "Distance: 0";
        BdistSymef1.GetComponent<Text>().text = "Distance: 0";

        winPrefab.GetComponent<Text>().text = "";
        highScorePrefab.GetComponent<Text>().text = "";

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
            AddLetterToGood(GoodA, " A:" + (agentOneValues[i]).ToString());
            AddLetterToGood(GoodB, " B:" + (agentTwoValues[i]).ToString());

            // Generate random color for each pair
            Color randomColor = new Color((Random.Range(0f, 1.0f) * 1.25f), (Random.Range(0f, 1.0f) * 1.25f), (Random.Range(0f, 1.0f) * 1.25f));
            colors.Add(randomColor);
            GoodA.GetComponent<Image>().color = randomColor;
            GoodB.GetComponent<Image>().color = randomColor;

            // Add OnClick event listener to each instantiated object
            int index = i; // Store the index in a local variable
            GoodA.GetComponent<Button>().onClick.AddListener(() => unusedGoodClicked(index));
            GoodB.GetComponent<Button>().onClick.AddListener(() => unusedGoodClicked(index));
        }
    }

    void unusedGoodClicked(int index)
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
            DeleteAll();

            //update the total by adding the value of the good for bundle 1
            updateTotal(true, 1, addedGoodA1, addedGoodB1);

            // update the pos for the next good to be added:
            posA1 += addedGoodA1.GetComponent<RectTransform>().sizeDelta.y;
            posB1 += addedGoodB1.GetComponent<RectTransform>().sizeDelta.y;

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

            //update the total by adding the value of the good for bundle 2
            updateTotal(true, 2, addedGoodA2, addedGoodB2);

            // update the pos for the next good to be added:
            posA2 += addedGoodA2.GetComponent<RectTransform>().sizeDelta.y;
            posB2 += addedGoodB2.GetComponent<RectTransform>().sizeDelta.y;

            //now remove the newly added good from the side panel:
            AGoods[selectedObject].SetActive(false);
            BGoods[selectedObject].SetActive(false);
        }

        tempGoods.Clear();
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
        if (bundle == 0 || bundle == 1)
        {
            for (int i = 0; i < A1Goods.Count; i++)
            {
                if (A1Goods[i] == clickedGood || B1Goods[i] == clickedGood)
                {
                    //update position for future goods
                    posA1 -= A1Goods[i].GetComponent<RectTransform>().sizeDelta.y;
                    posB1 -= B1Goods[i].GetComponent<RectTransform>().sizeDelta.y;

                    //if there is a temp good above it, move that good down
                    if (tempGoods.Count > 0 && tempGoods[0] != null && tempGoods[2] != null)
                    {
                        tempGoods[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(tempGoods[0].GetComponent<RectTransform>().anchoredPosition.x, tempGoods[0].GetComponent<RectTransform>().anchoredPosition.y - A1Goods[i].GetComponent<RectTransform>().sizeDelta.y);
                        tempGoods[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(tempGoods[2].GetComponent<RectTransform>().anchoredPosition.x, tempGoods[2].GetComponent<RectTransform>().anchoredPosition.y - B1Goods[i].GetComponent<RectTransform>().sizeDelta.y);
                    }

                    //update positions above current good that's being removed:
                    for (int j = i + 1; j < A1Goods.Count; j++)
                    {
                        A1Goods[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(A1Goods[j].GetComponent<RectTransform>().anchoredPosition.x, A1Goods[j].GetComponent<RectTransform>().anchoredPosition.y - A1Goods[i].GetComponent<RectTransform>().sizeDelta.y);
                        B1Goods[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(B1Goods[j].GetComponent<RectTransform>().anchoredPosition.x, B1Goods[j].GetComponent<RectTransform>().anchoredPosition.y - B1Goods[i].GetComponent<RectTransform>().sizeDelta.y);
                    }

                    //temp objects to keep count accurate
                    GameObject tempA1 = A1Goods[i];
                    GameObject tempB1 = B1Goods[i]; 

                    //delete the good
                    Destroy(A1Goods[i]);
                    Destroy(B1Goods[i]);
                    A1Goods.RemoveAt(i);
                    B1Goods.RemoveAt(i);

                    //update the total by subtracting the value of the good for bundle 1
                    updateTotal(false, 1, tempA1, tempB1);

                    break;
                }
            }
        }
        else if (bundle == 2 || bundle == 3)
        {
            for (int i = 0; i < A2Goods.Count; i++)
            {
                if (A2Goods[i] == clickedGood || B2Goods[i] == clickedGood)
                {
                    //update position for future goods
                    posA2 -= A2Goods[i].GetComponent<RectTransform>().sizeDelta.y;
                    posB2 -= B2Goods[i].GetComponent<RectTransform>().sizeDelta.y;

                    //if there is a temp good above it, move that good down
                    if(tempGoods.Count > 0 && tempGoods[1] != null && tempGoods[3] != null)
                    {
                        tempGoods[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(tempGoods[1].GetComponent<RectTransform>().anchoredPosition.x, tempGoods[1].GetComponent<RectTransform>().anchoredPosition.y - A2Goods[i].GetComponent<RectTransform>().sizeDelta.y);
                        tempGoods[3].GetComponent<RectTransform>().anchoredPosition = new Vector2(tempGoods[3].GetComponent<RectTransform>().anchoredPosition.x, tempGoods[3].GetComponent<RectTransform>().anchoredPosition.y - B2Goods[i].GetComponent<RectTransform>().sizeDelta.y);
                    }    

                    //update positions above current good that's being removed:
                    for (int j = i + 1; j < A2Goods.Count; j++)
                    {
                        A2Goods[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(A2Goods[j].GetComponent<RectTransform>().anchoredPosition.x, A2Goods[j].GetComponent<RectTransform>().anchoredPosition.y - A2Goods[i].GetComponent<RectTransform>().sizeDelta.y);
                        B2Goods[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(B2Goods[j].GetComponent<RectTransform>().anchoredPosition.x, B2Goods[j].GetComponent<RectTransform>().anchoredPosition.y - B2Goods[i].GetComponent<RectTransform>().sizeDelta.y);
                    }

                    //temp objects to keep count accurate
                    GameObject tempA2 = A2Goods[i];
                    GameObject tempB2 = B2Goods[i];

                    //delete the good
                    Destroy(A2Goods[i]);
                    Destroy(B2Goods[i]);
                    A2Goods.RemoveAt(i);
                    B2Goods.RemoveAt(i);

                    //update the total by subtracting the value of the good for bundle 1
                    updateTotal(false, 2, tempA2, tempB2);

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

    void updateTotal(bool adding, int agent, GameObject goodA, GameObject goodB)
    {

        Transform childTransformA = goodA.transform.GetChild(0);
        TextMeshProUGUI textComponentA = childTransformA.GetComponent<TextMeshProUGUI>();
        string numberStringA = textComponentA.text.Substring(textComponentA.text.IndexOf(":") + 1);
        int numberA;

        Transform childTransformB = goodB.transform.GetChild(0);
        TextMeshProUGUI textComponentB = childTransformB.GetComponent<TextMeshProUGUI>();
        string numberStringB = textComponentB.text.Substring(textComponentB.text.IndexOf(":") + 1);
        int numberB;

        // updating the total of each bundle of goods:
        if (int.TryParse(numberStringA, out numberA) && int.TryParse(numberStringB, out numberB))
        {
            if(agent == 1)
            {
                if(adding)
                {
                    A1Total += numberA;
                    B1Total += numberB;
                }
                else
                {
                    A1Total -= numberA;
                    B1Total -= numberB;
                }
            }
            else if(agent == 2)
            {
                if(adding)
                {
                    A2Total += numberA;
                    B2Total += numberB;
                }
                else
                {
                    A2Total -= numberA;
                    B2Total -= numberB;
                }
            }   
        }

        // ======================================== SYMEF1 CALCULATIONS ======================================== //


        //find different between bundles for each agent (disparity):
        float disparityA = Mathf.Max(A1Total, A2Total) - Mathf.Min(A1Total, A2Total);
        float disparityB = Mathf.Max(B1Total, B2Total) - Mathf.Min(B1Total, B2Total);


        string DisparityOutputA = "Disparity: " + Math.Abs(disparityA);
        Adisparity.GetComponent<Text>().text = DisparityOutputA;

        string DisparityOutputB = "Disparity: " + Math.Abs(disparityB);
        Bdisparity.GetComponent<Text>().text = DisparityOutputB;


        //find distance to symef1: (remove largest good from largest bundle, then find the difference between the two bundles)
        float symef1DistanceA = 0;
        float largestGoodA = 0;
        if (A1Total > A2Total) 
        {
            largestGoodA = findLargestGood(A1Goods);
            symef1DistanceA = (A1Total - largestGoodA) - A2Total;
        } 
        else if (A2Total > A1Total) 
        {
            largestGoodA = findLargestGood(A2Goods);
            symef1DistanceA = (A2Total - largestGoodA) - A1Total;
        }

        if (symef1DistanceA < 0)
        {
            symef1DistanceA = 0;
        }

        float symef1DistanceB = 0;
        float largestGoodB = 0;
        if (B1Total > B2Total)
        {
            largestGoodB = findLargestGood(B1Goods);
            symef1DistanceB = (B1Total - largestGoodB) - B2Total;
        }
        else if (B2Total > B1Total)
        {
            largestGoodB = findLargestGood(B2Goods);
            symef1DistanceB = (B2Total - largestGoodB) - B1Total;
        }

        if (symef1DistanceB < 0)
        {
            symef1DistanceB = 0;
        }


        //check for symef1, if not update distance to symef1:
        bool symef1WinA = false;
        string distanceOutputA = "";
        if (symef1DistanceA == 0)
        {
            distanceOutputA = "SymEF1!";
            symef1DistanceA = 0;
            symef1WinA = true;
        } 
        else
        {
            distanceOutputA = "Distance: " + symef1DistanceA;
        }
        AdistSymef1.GetComponent<Text>().text = distanceOutputA;


        string distanceOutputB = "";
        bool symef1WinB = false;
        if (symef1DistanceB == 0)
        {
            distanceOutputB = "SymEF1!";
            symef1DistanceB = 0;
            symef1WinB = true;
        }
        else
        {
            distanceOutputB = "Distance: " + symef1DistanceB;
        }
        BdistSymef1.GetComponent<Text>().text = distanceOutputB;


        //update the score:
        float score = Math.Abs(symef1DistanceA) + Math.Abs(disparityA) + Math.Abs(symef1DistanceB) + Math.Abs(disparityB);
        scorePrefab.GetComponent<Text>().text = ("Score: " + score.ToString());


        //check if its a win:
        checkWin(symef1WinA, symef1WinB, score);
    }

    void checkWin(bool Asymef1, bool bsymef1, float score)
    {
        int totalGoods = AGoods.Count + BGoods.Count;
        int totalGoodsUsed = A1Goods.Count + A2Goods.Count + B1Goods.Count + B2Goods.Count;

        if(totalGoods == totalGoodsUsed && Asymef1 && bsymef1)
        {
            string winnerOutput = "WINNER! \nScore: " + score;
            winPrefab.GetComponent<Text>().text = winnerOutput;

            if(score < HighScore)
            {
                HighScore = score;
                highScorePrefab.GetComponent<Text>().text = "High Score: " + HighScore;
            }
        } 
        else
        {
            winPrefab.GetComponent<Text>().text = "";
        }
    }

    int findLargestGood(List<GameObject> Goods)
    {
        int largestGood = -1;
        for (int i = 0; i < Goods.Count; i++)
        {
            Transform childTransform = Goods[i].transform.GetChild(0);
            TextMeshProUGUI textComponent = childTransform.GetComponent<TextMeshProUGUI>();
            string numberString = textComponent.text.Substring(textComponent.text.IndexOf(":") + 1);
            int number;

            if (int.TryParse(numberString, out number))
            {
                if (number > largestGood)
                {
                    largestGood = number;
                }
            }
        }
        return largestGood;
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
