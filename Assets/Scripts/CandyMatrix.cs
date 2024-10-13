using static System.EventHandler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using System.Runtime.ConstrainedExecution;
using UnityEngine.XR;

public class CandyMatrix : MonoBehaviour
{
    public int rows = 8; // Number of rows in the grid
    public int columns = 8; // Number of columns in the grid
    [SerializeField] public CandyScriptableObject[] candyScriptObjs; // Array to store different types of candy prefabs
    [SerializeField] public CandyEntity candyPrefab; // Candy Prefab
    private CandyEntity[,] candyGrid; // 2D array to hold the candy objects
    public float swapDuration = 0.15f; // Time for swap animation
    public float fallDuration = 0.4f; // Time for swap animation

    private CandyEntity selectedCandy; // To store the first selected candy
    private Vector2Int selectedCandyPos;
    private bool isCandySelected = false; // To track if a candy has been selected

    public bool isActive = false;

    [SerializeField] public int level = 1;

    public bool canSelect = true;

    [SerializeField] bool makeCandiesFall = true;

    public event System.EventHandler ControlRevoked;
    public event System.EventHandler ControlGiven;

    //For Sound
    public event System.EventHandler SwappingOccured;
    public event System.EventHandler MatchOccured;

    //For scoring
    public event System.EventHandler<MatchArgs> MatchFound;

    AudioSource audioSource; // Drag your audio source here in the inspector.
    public AudioClip candyBreakSound;
    public AudioClip candyAddSound;
    public AudioClip candySwapSound;

    public Material defaultSkybox;
    public Material extraSkybox;

    public class MatchArgs : System.EventArgs
    {
        public float score;
        public ScriptableObject scriptableObject;

        public MatchArgs(float score, ScriptableObject scriptableObject)
        {
            this.score = score;
            this.scriptableObject = scriptableObject;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    // Method to play candy crush sound
    public void PlayCandyBreakSound()
    {
        audioSource.clip = candyBreakSound;
        audioSource.time = 0.4f;
        //audioSource.Play();
    }

    // Method to play candy add sound
    public void PlayCandyAddSound()
    {
        audioSource.clip = candyAddSound;
        //audioSource.time = 0.2f;
        audioSource.Play();
    }
    public void PlayCandySwapSound()
    {
        audioSource.clip = candySwapSound;
        audioSource.time = 0.1f;
        audioSource.Play();
    }

    public void InitiateMatrix(int level)
    {
        this.level = level;
        GenerateGrid(); // Populate the grid with candies
        canSelect = true;
        ControlGiven?.Invoke(this, System.EventArgs.Empty);

        if(level == 3)
        {
            foreach (var item in FindObjectsOfType<CandyCounter>())
            {
                item.remaining = 5;
            }

            FindObjectOfType<MovesLeft>().movesl = 17;
        }

        if (level == 4)
        {
            foreach (var item in FindObjectsOfType<CandyCounter>())
            {
                item.remaining = 8;
            }

            FindObjectOfType<MovesLeft>().movesl = 25;
            FindObjectOfType<Timer>().timeRemaining = 180f;
        }

        if (level == 5)
        {
            foreach (var item in FindObjectsOfType<CandyCounter>())
            {
                item.remaining = 20;
            }

            FindObjectOfType<MovesLeft>().movesl = 60;
            FindObjectOfType<Timer>().timeRemaining = 360f;

            RenderSettings.skybox = extraSkybox;
            // Update lighting if necessary
            DynamicGI.UpdateEnvironment();
        }
        else
        {
            RenderSettings.skybox = defaultSkybox;
            // Update lighting if necessary
            DynamicGI.UpdateEnvironment();
        }
    }

    // Generate the grid of candies
    void GenerateGrid()
    {
        candyGrid = new CandyEntity[rows, columns]; // Initialize the grid
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 position = new Vector2(col + 0.5f, row + 0.5f) - new Vector2(columns, rows) / 2; // Position in the grid
                CandyScriptableObject cso = GetRandomCandy(row, col); // Get a valid random candy

                // Instantiate the candy at the grid position
                CandyEntity randomCandy = Instantiate(candyPrefab, position, Quaternion.identity, gameObject.transform);
                randomCandy.init(cso);

                // Add the candy to the grid array
                candyGrid[row, col] = randomCandy;
            }
        }
    }

    // Handle user input for selecting and swapping candies
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canSelect) // Left mouse button clicked
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            HandleCandyClick(mousePos);
        }

        if (Input.GetKey(KeyCode.A))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CandyEntity clickedCandy = GetCandyAtPosition(mousePos);
            Debug.Log("Clicked: " + (clickedCandy ? GetCandyPosition(clickedCandy) : "Not in grid"));
        }
    }

    // Handle what happens when a candy is clicked
    void HandleCandyClick(Vector2 mousePos)
    {
        // Find which candy was clicked
        CandyEntity clickedCandy = GetCandyAtPosition(mousePos);

        if (clickedCandy == null) return; // If no candy was clicked, exit

        if (!isCandySelected) // No candy is currently selected
        {
            SelectCandy(clickedCandy); // Select the clicked candy
        }
        else
        {
            // Check if the second candy is adjacent to the first
            Vector2Int selectedPos = GetCandyPosition(selectedCandy);
            Vector2Int clickedPos = GetCandyPosition(clickedCandy);

            // Deselect candy
            DeselectCandy();

            if (IsAdjacent(selectedPos, clickedPos))
            {
                // Swap the candies
                SwapCandies(selectedPos, clickedPos);
            }
        }
    }

    // Select a candy (highlight or mark it visually if needed)
    void SelectCandy(CandyEntity candy)
    {
        selectedCandy = candy;
        isCandySelected = true;
        // Optional: Add some visual indication like changing color or adding an outline
        candy.Highlight(0);

        // Get the position of the selected candy
        selectedCandyPos = GetCandyPosition(candy);

        // Highlight adjacent candies (up, down, left, right)
        HighlightAdjacentCandies(selectedCandyPos, true);
    }

    // Deselect the candy and remove visual highlight
    void DeselectCandy()
    {
        if (selectedCandy != null)
        {
            // Remove highlight from the selected candy
            selectedCandy.Highlight(1);

            // Remove highlight from adjacent candies
            HighlightAdjacentCandies(selectedCandyPos, false);
        }

        selectedCandy = null;
        isCandySelected = false;
    }

    // Highlight or remove highlight from adjacent candies
    void HighlightAdjacentCandies(Vector2Int pos, bool highlight)
    {
        // Get the adjacent positions (left, right, up, down)
        Vector2Int[] adjacentPositions = {
        new Vector2Int(pos.x - 1, pos.y), // Left
        new Vector2Int(pos.x + 1, pos.y), // Right
        new Vector2Int(pos.x, pos.y - 1), // Down
        new Vector2Int(pos.x, pos.y + 1)  // Up
    };

        // Loop through each adjacent position
        foreach (Vector2Int adjacentPos in adjacentPositions)
        {
            // Check if the position is within bounds
            if (adjacentPos.x >= 0 && adjacentPos.x < columns && adjacentPos.y >= 0 && adjacentPos.y < rows)
            {
                CandyEntity adjacentCandy = candyGrid[adjacentPos.y, adjacentPos.x];
                if (adjacentCandy != null)
                {
                    // Highlight or remove highlight based on the 'highlight' flag
                    if (highlight)
                    {
                        adjacentCandy.Highlight(2); // Add highlight
                    }
                    else
                    {
                        //Debug.Log("adjacentCandy.Highlight(1)");
                        adjacentCandy.Highlight(1); // Remove highlight
                    }
                }
            }
        }
    }

    // Get the candy at the mouse click position
    CandyEntity GetCandyAtPosition(Vector2 position)
    {
        foreach (CandyEntity candy in candyGrid)
        {
            if (candy != null && candy.GetComponent<Collider2D>().bounds.Contains(position))
            {
                return candy;
            }
        }
        return null;
    }

    CandyEntity GetCandyAtPosition(int row, int col)
    {
        if(row >= rows || row < 0) return null;
        if(col >= columns || col < 0) return null;
        return candyGrid[row, col];
    }

    // Get a random candy, ensuring no match of 3 is created
    CandyScriptableObject GetRandomCandy(int row, int col)
    {
        List<CandyScriptableObject> potentialCandies = new List<CandyScriptableObject>(candyScriptObjs);

        // Check horizontally (left)
        if (col >= 2 &&
            candyGrid[row, col - 1] != null &&
            candyGrid[row, col - 2] != null &&
            candyGrid[row, col - 1].GetCandyType() == candyGrid[row, col - 2].GetCandyType())
        {
            CandyScriptableObject forbiddenCandy = candyGrid[row, col - 1].GetCandyType();
            potentialCandies.Remove(forbiddenCandy); // Prevent 3-in-a-row horizontally
        }

        // Check vertically (above)
        if (row >= 2 &&
            candyGrid[row - 1, col] != null &&
            candyGrid[row - 2, col] != null &&
            candyGrid[row - 1, col].GetCandyType() == candyGrid[row - 2, col].GetCandyType())
        {
            CandyScriptableObject forbiddenCandy = candyGrid[row - 1, col].GetCandyType();
            potentialCandies.Remove(forbiddenCandy); // Prevent 3-in-a-row vertically
        }

        // Return a valid random candy that does not cause a match of 3
        return potentialCandies[Random.Range(0, potentialCandies.Count)];
    }

    CandyScriptableObject GetRandomCandyDuring(CandyScriptableObject below = null, bool increaseChance = false)
    {
        List<CandyScriptableObject> potentialCandies = new List<CandyScriptableObject>(candyScriptObjs);

        if (below)
        {
            if(Random.Range(0, 1.0f) < (increaseChance ? 0.6f : 0.4f))
            {
                return below;
            }

            else
            {
                potentialCandies.Remove(below);
            }
        }

        // Return a valid random candy that does not cause a match of 3
        return potentialCandies[Random.Range(0, potentialCandies.Count)];
    }

    CandyScriptableObject GetRandomCandyDuring2(int row, int col)
    {
        List<CandyScriptableObject> potentialCandies = new List<CandyScriptableObject>(candyScriptObjs);

        // Check the surrounding 8 candies (neighborhood)
        List<CandyEntity> surroundingCandies = GetSurroundingCandies(row, col); // Implement this method

        // Multiply probabilities based on surrounding candies
        int neighborhoodCount = surroundingCandies.Count;

        // For now, assume we are just going to return a random candy from the neighborhood
        // You might want to customize how the probability is calculated here
        if (neighborhoodCount > 0)
        {
            // Optionally, you could weigh the chances of selecting each candy based on the neighborhood count
            return potentialCandies[Random.Range(0, potentialCandies.Count)];
        }
        // If there are no surrounding candies, return a random candy from the potential list
        return potentialCandies[Random.Range(0, potentialCandies.Count)];
    }

    // Implement a method to get surrounding candies
    private List<CandyEntity> GetSurroundingCandies(int row, int col)
    {
        List<CandyEntity> surroundingCandies = new List<CandyEntity>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // Skip the current candy itself

                int checkRow = row + y;
                int checkCol = col + x;

                // Ensure we're within grid bounds
                if (checkRow >= 0 && checkRow < rows && checkCol >= 0 && checkCol < columns)
                {
                    CandyEntity candy = candyGrid[checkRow, checkCol];
                    if (candy != null)
                    {
                        surroundingCandies.Add(candy);
                    }
                }
            }
        }

        //foreach (CandyEntity c in surroundingCandies) { Debug.Log(c.candyScriptObj.candyType); };
        //Debug.Log(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;");

        return surroundingCandies;
    }

    // Swap two candies visually and in the grid
    public void SwapCandies(Vector2Int pos1, Vector2Int pos2)
    {
        // Swap them in the candyGrid array
        CandyEntity temp = candyGrid[pos1.y, pos1.x];
        candyGrid[pos1.y, pos1.x] = candyGrid[pos2.y, pos2.x];
        candyGrid[pos2.y, pos2.x] = temp;

        // Move the GameObjects visually
        StartCoroutine(SwapCandyPositions(candyGrid[pos1.y, pos1.x], candyGrid[pos2.y, pos2.x]));
    }

    // Function to pop matched candies
    IEnumerator PopMatches(List<CandyEntity> matches)
    {
        if(matches.Count > 0)
        {
            MatchOccured?.Invoke(this, System.EventArgs.Empty);
        }

        int corouts = 0;
        foreach (CandyEntity candy in matches)
        {
            // Set the candyGrid entry to null
            Vector2Int pos = GetCandyPosition(candy);
            //Debug.Log("After Pos: " + pos);
            candyGrid[pos.y, pos.x] = null;

            corouts++;
            StartCoroutine(PopTileAnimation(candy.gameObject, 0.1f, () => corouts--)); // Animates over 0.5 seconds

            // Destroy the candy GameObject
            //Destroy(candy.gameObject);
        }
        //Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
        yield return new WaitUntil(() => corouts == 0);
    }

    IEnumerator PopTileAnimation(GameObject tile, float duration, System.Action callback)
    {
        Vector3 originalScale = tile.transform.localScale;
        Vector3 targetScale = originalScale * 1.5f; // Increase scale to 1.5 times the original size
        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>(); // Assuming tile has a SpriteRenderer
        Color originalColor = spriteRenderer.color;
        Color originalColorIcon = spriteRenderer.GetComponent<CandyEntity>().spriter.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Fade to transparent

        float elapsedTime = 0f;

        PlayCandyBreakSound();
        while (elapsedTime < duration)
        {
            // Scale the tile
            tile.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);

            // Fade the tile
            //spriteRenderer.color = Color.Lerp(originalColor, targetColor, elapsedTime / duration);
            spriteRenderer.GetComponent<CandyEntity>().spriter.color = Color.Lerp(originalColorIcon, targetColor, elapsedTime / duration);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        

        // Ensure the final state is fully applied
        tile.transform.localScale = targetScale;
        spriteRenderer.color = targetColor;

        // Destroy the tile after animation
        Destroy(tile);
        callback?.Invoke();
    }

    IEnumerator AddTileAnimation(GameObject tile, float duration)
    {
        Vector3 originalScale = tile.transform.localScale * 1.5f; // Start at 1.5 times the original size
        Vector3 targetScale = tile.transform.localScale; // Scale down to original size
        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>(); // Assuming tile has a SpriteRenderer
        Color originalColor = spriteRenderer.color;
        Color originalColorIcon = spriteRenderer.GetComponent<CandyEntity>().spriter.color;
        Color startColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Start fully transparent

        float elapsedTime = 0f;

        // Set the initial state (large and transparent)
        tile.transform.localScale = originalScale;
        spriteRenderer.GetComponent<CandyEntity>().spriter.color = startColor;

        PlayCandyAddSound();
        while (elapsedTime < duration)
        {
            // Scale the tile down
            tile.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);

            // Fade the tile in
            spriteRenderer.color = Color.Lerp(startColor, originalColor, elapsedTime / duration);
            spriteRenderer.GetComponent<CandyEntity>().spriter.color = Color.Lerp(startColor, originalColorIcon, elapsedTime / duration);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        

        // Ensure the final state is fully applied
        tile.transform.localScale = targetScale;
        spriteRenderer.GetComponent<CandyEntity>().spriter.color = originalColorIcon;
    }


    // Function to make candies fall into empty spaces and generate new candies
    IEnumerator MakeCandiesFall()
    {
        int cor = 0;

        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                if (candyGrid[row, col] == null) // If there's an empty spot
                {
                    // Find the first candy above the empty spot
                    for (int aboveRow = row; aboveRow < rows; aboveRow++)
                    {
                        if (candyGrid[aboveRow, col] != null)
                        {
                            // Move the candy down
                            candyGrid[row, col] = candyGrid[aboveRow, col];
                            candyGrid[aboveRow, col] = null;
                            cor++;
                            StartCoroutine(MoveCandy(candyGrid[row, col], row, col, () => cor--));
                            break;
                        }
                    }
                }
            }
        }

        yield return new WaitUntil(() => cor == 0);

        yield return FillEmpties();

        yield return null;
    }

    IEnumerator MakeCandiesWithoutFalling()
    {
        yield return FillEmpties();

        yield return null; // Ensure the coroutine completes
    }

    IEnumerator FillEmpties()
    {
        List<Vector2> matchesHorizon = CheckConsecutiveNullsHorizontally();

        //foreach (var vec in matchesHorizon) { Debug.Log(vec); }
        //Debug.Log("----------------------------------------");

        // Iterate through the grid to fill empty spots without making panels fall
        for (int col = 0; col < columns; col++)
        {
            bool first = true;

            // Fill the topmost empty spots with new candies, but no candy will fall
            for (int row = 0; row < rows; row++)
            {
                if (candyGrid[row, col] != null)
                    continue; // Skip if there's already a candy in this spot

                // Calculate the position for the new candy
                Vector2 position = new Vector2(col + 0.5f, row + 0.5f) - new Vector2(columns, rows) / 2;

                CandyEntity candybelow = level == 1 ? GetCandyAtPosition(row - 1, col) : null;

                // Check if this is part of a horizontal match
                bool horizon = false;
                foreach (var vec in matchesHorizon)
                {
                    if (vec.x == row && vec.y == col)
                    {
                        horizon = true;
                        break;
                    }
                }

                // Get a random new candy, adjusting logic based on whether it's part of a horizontal match
                CandyScriptableObject newCandySO = level == 2 ? GetRandomCandyDuring2(row, col) : GetRandomCandyDuring(candybelow?.candyScriptObj, !first || horizon);
                //Debug.Log($"{candybelow?.candyScriptObj?.candyType} : {horizon}");

                // Create and initialize the new candy at the calculated position
                CandyEntity newCandy = Instantiate(candyPrefab, position, Quaternion.identity, transform);
                newCandy.init(newCandySO);
                yield return AddTileAnimation(newCandy.gameObject, 0.2f);

                // Place the new candy in the grid
                candyGrid[row, col] = newCandy;
                first = false; // Mark that we've placed a candy in this column
            }
        }

        //Debug.Log("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
    }

    IEnumerator MoveCandy(CandyEntity candy, int targetRow, int targetCol, System.Action onComplete)
    {
        Vector3 startPos = candy.transform.position;
        Vector3 targetPos = new Vector3(targetCol + 0.5f, targetRow + 0.5f) - new Vector3(columns, rows) / 2;

        float elapsedTime = 0f;
        while (elapsedTime < fallDuration)
        {
            candy.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / fallDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        candy.transform.position = targetPos; // Ensure the final position is accurate

        onComplete?.Invoke();
    }

    // Coroutine to swap candy positions smoothly
    IEnumerator SwapCandyPositions(CandyEntity candy1, CandyEntity candy2)
    {
        canSelect = false;
        isActive = true;
        ControlRevoked?.Invoke(this, System.EventArgs.Empty);

        Vector3 startPos1 = candy1.spriter.transform.position;
        Vector3 startPos2 = candy2.spriter.transform.position;

        float elapsedTime = 0f;

        SwappingOccured?.Invoke(this, System.EventArgs.Empty);
        PlayCandySwapSound();

        // Animate the movement over swapDuration
        while (elapsedTime < swapDuration)
        {
            candy1.spriter.transform.position = Vector3.Lerp(startPos1, startPos2, elapsedTime / swapDuration);
            candy2.spriter.transform.position = Vector3.Lerp(startPos2, startPos1, elapsedTime / swapDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Final positions
        candy1.spriter.transform.position = startPos1;
        candy2.spriter.transform.position = startPos2;

        // Ensure the final position is correct
        Vector3 candyPos = candy1.transform.position;
        candy1.transform.position = candy2.transform.position;
        candy2.transform.position = candyPos;

        // After swap, check for matches
        List<CandyEntity> matches = CheckMatches(GetCandyPosition(candy1).y, GetCandyPosition(candy1).x);
        matches.AddRange(CheckMatches(GetCandyPosition(candy2).y, GetCandyPosition(candy2).x));

        matches = matches.Distinct().ToList();

        // Pop the matched candies if there are any
        if (matches.Count >= 3)
        {
            yield return PopMatches(matches);

            // Make candies fall and cascade matches
            yield return makeCandiesFall ? MakeCandiesFall() : MakeCandiesWithoutFalling();
            yield return CheckForNewMatches();
        }

        canSelect = true;
        isActive = false;
        ControlGiven?.Invoke(this, System.EventArgs.Empty);
    }

    // Function to check for new matches after candies fall
    IEnumerator CheckForNewMatches()
    {
        bool newMatchesFound = true;

        while (newMatchesFound)
        {
            newMatchesFound = false;

            Beg:

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    List<CandyEntity> matches = CheckMatches(row, col, true);
                    matches = matches.Distinct().ToList();
                    if (matches.Count >= 3)
                    {
                        newMatchesFound = true;
                        yield return PopMatches(matches);
                        yield return makeCandiesFall? MakeCandiesFall() : MakeCandiesWithoutFalling();
                        goto Beg;
                    }
                }
            }
        }
    }

    // Check for matches in the grid after a swap
    public List<CandyEntity> CheckMatches(int row, int col, bool extra = false)
    {
        List<CandyEntity> matchingCandies = new List<CandyEntity>();

        // Check horizontally (row)
        List<CandyEntity> horizontalMatches = new List<CandyEntity>();
        horizontalMatches.Add(candyGrid[row, col]);
        // Check left side
        for (int i = col - 1; i >= 0 && candyGrid[row, i].GetCandyType() == candyGrid[row, col].GetCandyType(); i--)
        {
            horizontalMatches.Add(candyGrid[row, i]);
        }
        // Check right side
        for (int i = col + 1; i < columns && candyGrid[row, i].GetCandyType() == candyGrid[row, col].GetCandyType(); i++)
        {
            horizontalMatches.Add(candyGrid[row, i]);
        }
        if (horizontalMatches.Count >= 3)
        {
            MatchFound?.Invoke(this, new MatchArgs(horizontalMatches.Count * (extra ? 1.5f : 1), candyGrid[row, col].GetCandyType()));
            matchingCandies.AddRange(horizontalMatches);
        }

        // Check vertically (column)
        List<CandyEntity> verticalMatches = new List<CandyEntity>();
        verticalMatches.Add(candyGrid[row, col]);
        // Check downwards
        for (int i = row - 1; i >= 0 && candyGrid[i, col].GetCandyType() == candyGrid[row, col].GetCandyType(); i--)
        {
            verticalMatches.Add(candyGrid[i, col]);
        }
        // Check upwards
        for (int i = row + 1; i < rows && candyGrid[i, col].GetCandyType() == candyGrid[row, col].GetCandyType(); i++)
        {
            verticalMatches.Add(candyGrid[i, col]);
        }
        if (verticalMatches.Count >= 3)
        {
            MatchFound?.Invoke(this, new MatchArgs(verticalMatches.Count * (extra ? 1.5f : 1), candyGrid[row, col].GetCandyType()));
            matchingCandies.AddRange(verticalMatches);
        }

        //foreach (CandyEntity candy in matchingCandies) { Debug.Log("Pos of candy: " + GetCandyPosition(candy)); }
        //Debug.Log("---------------------------------------------");
        return matchingCandies;
    }

    public List<Vector2> CheckConsecutiveNullsHorizontally()
    {
        List<Vector2> nullPositions = new List<Vector2>();

        // Loop through the entire grid row by row
        for (int row = 0; row < rows; row++)
        {
            List<Vector2> consecutiveNulls = new List<Vector2>();

            for (int col = 0; col < columns; col++)
            {
                if (candyGrid[row, col] == null)
                {
                    // If the current cell is null, add its position to the consecutive null list
                    consecutiveNulls.Add(new Vector2(row, col));
                }
                else
                {
                    // If it's not null and we had a sequence of at least 3 consecutive nulls, save the result
                    if (consecutiveNulls.Count >= 3)
                    {
                        nullPositions.AddRange(consecutiveNulls);
                    }

                    // Reset the consecutive null list as the sequence has been broken
                    consecutiveNulls.Clear();
                }

                // Edge case: Check for consecutive nulls at the last column
                if (col == columns - 1 && consecutiveNulls.Count >= 3)
                {
                    nullPositions.AddRange(consecutiveNulls);
                }
            }
        }

        return nullPositions;
    }

    // Get the grid position of a candy
    Vector2Int GetCandyPosition(CandyEntity candy)
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (candyGrid[row, col] == candy)
                {
                    return new Vector2Int(col, row);
                }
            }
        }
        return new Vector2Int(-1, -1); // Return invalid position if not found
    }

    // Check if two positions are adjacent in the grid
    bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
    {
        return (Mathf.Abs(pos1.x - pos2.x) == 1 && pos1.y == pos2.y) ||
               (Mathf.Abs(pos1.y - pos2.y) == 1 && pos1.x == pos2.x);
    }
}
