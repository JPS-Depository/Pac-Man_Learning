using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ghost : MonoBehaviour
{

    public float moveSpeed;
    public int pinkyReleaseTimer = 5;
    public int inkyReleaseTimer = 14;
    public int clydeReleaseTimer = 21;
    public float ghostReleaseTimer = 0;

    public bool canMove = true;

    public int frightenedModeDuration = 10;
    public int startBlinkingAt = 7;

    public bool isInGhostHouse = false;

    public Node startingPosition;
    public Node homeNode;
    public Node GhostNode;
    public Node ghostHouse;

    public int scatterModeTimer1 = 7;
    public int chaseModeTimer1 = 20;
    public int scatterModeTimer2 = 7;
    public int chaseModeTimer2 = 20;
    public int scatterModeTimer3 = 5;
    public int chaseModeTimer3 = 20;
    public int scatterModeTimer4 = 5;

    public Sprite eyesUp, eyesDown, eyesLeft, eyesRight;

    public RuntimeAnimatorController ghostUp;
    public RuntimeAnimatorController ghostDown;
    public RuntimeAnimatorController ghostLeft;
    public RuntimeAnimatorController ghostRight;
    public RuntimeAnimatorController ghostWhite;
    public RuntimeAnimatorController ghostBlue;

    private int modeChangeIteration = 1;
    private float modeChangeTimer = 0;

    private float FrightenedModeTimer = 0;
    private float BlinkTimer = 0;
    private bool FrightenedModeIsWhite = false;

    private float previousMoveSpeed;

    private AudioSource backgroundAudio;

    public enum Mode
    {
        Chase,
        Scatter,
        Frightened,
        Consumed
    }

    public enum GhostType
    {
        Red,
        Pink,
        Blue,
        Orange,
    }

    public GhostType ghostType = GhostType.Red;

    Mode currentMode = Mode.Scatter;
    Mode previousMode;

    private GameObject pacMan;

    private Node currentNode, targetNode, previousNode;
    private Vector2 direction, nextDirection;
    // Use this for initialization
    void Start()
    {
        backgroundAudio = GameObject.Find("Game").transform.GetComponent<AudioSource>();
        pacMan = GameObject.FindGameObjectWithTag("Pac-man");
        Node node = GetNodeAtPosition(transform.localPosition);
        if (node != null)
        {
            currentNode = node;
        }
        targetNode = ChooseNextNode();
        previousNode = currentNode;
        UpdateAnimatorController();
    }

    public void Restart()
    {
        canMove = true;
        transform.GetComponent<SpriteRenderer>().enabled = true;
        currentMode = Mode.Scatter;
        moveSpeed = 5.9f;
        previousMoveSpeed = 0;
        transform.position = startingPosition.transform.position;
        ghostReleaseTimer = 0;
        modeChangeIteration = 1;
        modeChangeTimer = 0;
        if (transform.name != "Blinky")
        {
            isInGhostHouse = true;
            GameObject.Find("pinkyStartPoint").GetComponent<Node>().validDirections[1] = Vector2.down;
            GameObject.Find("pinkyStartPoint").GetComponent<Node>().neighbors[1] =
                GameObject.Find("pinkyStartPoint (1)").GetComponent<Node>();
            GameObject.Find("inkyStartPoint").GetComponent<Node>().validDirections[1] = Vector2.down;
            GameObject.Find("inkyStartPoint").GetComponent<Node>().neighbors[1] =
                GameObject.Find("inkyStartPoint (1)").GetComponent<Node>();
            GameObject.Find("clydeStartPoint").GetComponent<Node>().validDirections[1] = Vector2.down;
            GameObject.Find("clydeStartPoint").GetComponent<Node>().neighbors[1] =
                GameObject.Find("clydeStartPoint (1)").GetComponent<Node>();
        }
        currentNode = startingPosition;
        if (isInGhostHouse)
        {
            targetNode = ChooseNextNode();
        }
        else
        { 
            targetNode = ChooseNextNode();
        }
        previousNode = currentNode;
        UpdateAnimatorController();
    }

    // Update is called once per frame
    void Update()
    {
        if(canMove)
        {
            ModeUpdate();
            Move();
            ReleaseGhost();
            CheckCollision();
            CheckIsInGhostHouse();
        }        
    }

    void CheckIsInGhostHouse()
    {
        if(currentMode == Mode.Consumed)
        {
            GameObject tile = GetTileAtPosition(transform.position);
            if (tile!= null)
            {
                if(tile.transform.GetComponent<Tile>()!= null)
                {
                    if (tile.transform.GetComponent<Tile>().IsGhostHouse)
                    {
                        isInGhostHouse = true;
                        moveSpeed = 1.9f;
                        Node node = GetNodeAtPosition(transform.position);
                        if (node != null)
                        {
                            currentNode = node;
                            direction = Vector2.up;
                            targetNode = currentNode.neighbors[0];

                            previousNode = currentNode;
                            currentMode = Mode.Chase;

                            UpdateAnimatorController();
                        }
                    }
                }
            }
        }
    }
    void CheckCollision()
    {
        Rect ghostRect = new Rect(transform.position, transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);
        Rect pacManRect = new Rect(pacMan.transform.position, pacMan.transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);

        if (ghostRect.Overlaps(pacManRect))
        {
            if(currentMode == Mode.Frightened)
            {
                Consumed();
            }
            else
            {
                if(currentMode != Mode.Consumed)
                {
                    GameObject.Find("Game").transform.GetComponent<GameBoard>().StartDeath();
                }                
            }
        }
    }

    void Consumed()
    {
        currentMode = Mode.Consumed;
        previousMoveSpeed = moveSpeed;
        moveSpeed = 12f;
        UpdateAnimatorController();
    }
    void UpdateAnimatorController()
    {
        if (currentMode != Mode.Frightened && currentMode != Mode.Consumed)
        {
            if (direction == Vector2.up)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostUp;
            }
            else if (direction == Vector2.down)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostDown;
            }
            else if (direction == Vector2.left)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostLeft;
            }
            else if (direction == Vector2.right)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostRight;
            }
            else
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostLeft;
            }
        }
        else if (currentMode == Mode.Frightened)
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = ghostBlue;
        }
        else if (currentMode == Mode.Consumed)
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = null;
            if (direction == Vector2.up)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesUp;
            }
            else if (direction == Vector2.down)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesDown;
            }
            else if (direction == Vector2.left)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesLeft;
            }
            else if (direction == Vector2.right)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesRight;
            }
        }
        
    }
    void Move()
    {
        if (targetNode != currentNode && targetNode != null)
        {
            if (overShotTarget())
            {
                currentNode = targetNode;
                transform.localPosition = currentNode.transform.position;
                GameObject otherPortal = GetPortal(currentNode.transform.position);

                if (otherPortal != null)
                {
                    transform.localPosition = otherPortal.transform.position;
                    currentNode = otherPortal.GetComponent<Node>();
                }
                targetNode = ChooseNextNode();
                previousNode = currentNode;
                currentNode = null;
                UpdateAnimatorController();
            }
            else
            {
                transform.localPosition += (Vector3)direction * moveSpeed * Time.deltaTime;
            }
        }
    }
    void ModeUpdate()
    {
        if (currentMode != Mode.Frightened && !isInGhostHouse)
        {
            modeChangeTimer += Time.deltaTime;
            if (modeChangeIteration == 1)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer1)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer1)
                {
                    modeChangeIteration = 2;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            }
            else if (modeChangeIteration == 2)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer2)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer2)
                {
                    modeChangeIteration = 3;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            }
            else if (modeChangeIteration == 3)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer3)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer3)
                {
                    modeChangeIteration = 4;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            }
            else if (modeChangeIteration == 4)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer4)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
            }
        }
        else if (currentMode == Mode.Frightened)
        {
            FrightenedModeTimer += Time.deltaTime;
            if (FrightenedModeTimer >= frightenedModeDuration)
            {
                backgroundAudio.clip = GameObject.Find("Game").transform.GetComponent<GameBoard>().BackgroundAudioNormal;
                backgroundAudio.Play();
                FrightenedModeTimer = 0;
                ChangeMode(previousMode);
            }
            if(FrightenedModeTimer >= startBlinkingAt)
            {
                BlinkTimer += Time.deltaTime;
                if(BlinkTimer >= 0.2f)
                {
                    BlinkTimer = 0f;
                    if(FrightenedModeIsWhite)
                    {
                        transform.GetComponent<Animator>().runtimeAnimatorController = ghostBlue;
                        FrightenedModeIsWhite = false;
                    }
                    else
                    {
                        transform.GetComponent<Animator>().runtimeAnimatorController = ghostWhite;
                        FrightenedModeIsWhite = true;
                    }
                }
            }
        }
    }
    void ChangeMode(Mode m)
    {
        if(currentMode == Mode.Frightened)
        {
            moveSpeed = previousMoveSpeed;
        }
        if(m == Mode.Frightened)
        {
            previousMoveSpeed = moveSpeed;
            moveSpeed = 2.9f;
        }
        if(currentMode != m)
        {
            previousMode = currentMode;
            currentMode = m;
        }       
        UpdateAnimatorController();
    }

    public void StartFrightenedMode()
    {
        if(currentMode != Mode.Consumed)
        {
            FrightenedModeTimer = 0;
            backgroundAudio.clip = GameObject.Find("Game").transform.GetComponent<GameBoard>().BackgroundAudioFrightened;
            backgroundAudio.Play();
            ChangeMode(Mode.Frightened);
        }       
    }

    Vector2 GetRedGhostTargetTile()
    {
        Vector2 pacManPosition = pacMan.transform.localPosition;
        Vector2 targetTile = new Vector2(Mathf.RoundToInt(pacManPosition.x), Mathf.RoundToInt(pacManPosition.y));
        return targetTile;
    }

    Vector2 GetPinkGhostTargetTile()
    {
        // 4 tile infront pacman according to pacman orientation
        Vector2 pacManPosition = pacMan.transform.localPosition;
        Vector2 pacManOrientation = pacMan.GetComponent<Pacman>().orientation;

        int pacManPositionX = Mathf.RoundToInt(pacManPosition.x);
        int pacManPositionY = Mathf.RoundToInt(pacManPosition.y);

        Vector2 pacManTile = new Vector2(pacManPositionX, pacManPositionY);
        Vector2 targetTile = pacManTile + (4 * pacManOrientation);

        return targetTile;
    }
    
    Vector2 GetBlueGhostTargetTile()
    {
        // two tiles infront pacman > get distance > double the lenght
        Vector2 pacManPosition = pacMan.transform.localPosition;
        Vector2 pacManOrientation = pacMan.GetComponent<Pacman>().orientation;

        int pacManPositionX = Mathf.RoundToInt(pacManPosition.x);
        int pacManPositionY = Mathf.RoundToInt(pacManPosition.y);

        Vector2 pacManTile = new Vector2(pacManPositionX, pacManPositionY);
        Vector2 targetTile = pacManTile + (2 * pacManOrientation);

        // temp blinky pos
        Vector2 tempBlinkyPosition = GameObject.Find("Blinky").transform.localPosition;

        int blinkyPositionX = Mathf.RoundToInt(tempBlinkyPosition.x);
        int blinkyPositionY = Mathf.RoundToInt(tempBlinkyPosition.y);

        tempBlinkyPosition = new Vector2(blinkyPositionX, blinkyPositionY);

        float distance = GetDistance(tempBlinkyPosition, targetTile);
        distance *= 2;
        targetTile = new Vector2(tempBlinkyPosition.x + distance, tempBlinkyPosition.y + distance);
        return targetTile;
    }

    Vector2 GetOrangeGhostTargetTile()
    {
        //calculate distance to pacman, if distance > 8, target = blinky target
        // if distance < 8, target = scatter
        Vector2 pacManPosition = pacMan.transform.localPosition;
        float distance = GetDistance(transform.localPosition, pacManPosition);

        Vector2 targetTile = Vector2.zero;

        if (distance >= 8)
        {
            targetTile = new Vector2(Mathf.RoundToInt(pacManPosition.x), Mathf.RoundToInt(pacManPosition.y));
        }
        else if (distance < 8)
        {
            targetTile = homeNode.transform.position;
        }
        return targetTile;
    }

    Vector2 GetTargetTile()
    {
        Vector2 targetTile = Vector2.zero;
        if (ghostType == GhostType.Red)
        {
            targetTile = GetRedGhostTargetTile();
        }
        else if (ghostType == GhostType.Pink)
        {
            targetTile = GetPinkGhostTargetTile();
        }
        else if (ghostType == GhostType.Blue)
        {
            targetTile = GetBlueGhostTargetTile();
        }
        else if (ghostType == GhostType.Orange)
        {
            targetTile = GetOrangeGhostTargetTile();
        }
        return targetTile;      
    }

    Vector2 GetRandomTile()
    {

        int x = UnityEngine.Random.Range(0, 28);
        int y = UnityEngine.Random.Range(0, 36);

        return new Vector2(x, y);
    }

    void ReleaseRedGhost()
    {
        if (ghostType == GhostType.Red && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }
    void ReleasePinkGhost()
    {
        if (ghostType == GhostType.Pink && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }

    void ReleaseBlueGhost()
    {
        if (ghostType == GhostType.Blue && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }
    void ReleaseOrangeGhost()
    {
        if (ghostType == GhostType.Orange && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }
    void ReleaseGhost()
    {
        ghostReleaseTimer += Time.deltaTime;
        if(ghostReleaseTimer > 0)
        {
            ReleaseRedGhost();
        }
        if(ghostReleaseTimer > pinkyReleaseTimer -1)
        {
            GameObject.Find("pinkyStartPoint").GetComponent<Node>().validDirections[1] = Vector2.zero;
            GameObject.Find("pinkyStartPoint").GetComponent<Node>().neighbors[1] = null;
        }
        if(ghostReleaseTimer > pinkyReleaseTimer)
        {
            ReleasePinkGhost();
        }

        if (ghostReleaseTimer > inkyReleaseTimer -1)
        {
            GameObject.Find("inkyStartPoint").GetComponent<Node>().validDirections[1] = Vector2.zero;
            GameObject.Find("inkyStartPoint").GetComponent<Node>().neighbors[1] = null;
        }
        if (ghostReleaseTimer > inkyReleaseTimer)
        {
            ReleaseBlueGhost();
        }

        if (ghostReleaseTimer > clydeReleaseTimer - 1)
        {
            GameObject.Find("clydeStartPoint").GetComponent<Node>().validDirections[1] = Vector2.zero;
            GameObject.Find("clydeStartPoint").GetComponent<Node>().neighbors[1] = null;
        }
        if (ghostReleaseTimer > clydeReleaseTimer)
        {
            ReleaseOrangeGhost();
        }
    }
    
    Node ChooseNextNode()
    {
        Vector2 targetTile = Vector2.zero;

        if (currentMode == Mode.Chase && !isInGhostHouse)
        {
            moveSpeed = 5.9f;
            targetTile = GetTargetTile();
        } else if(currentMode == Mode.Scatter && !isInGhostHouse)
        {
            moveSpeed = 5.9f;
            targetTile = homeNode.transform.position;
        } else if(isInGhostHouse)
        {
            moveSpeed = 2.9f;
            targetTile = GhostNode.transform.position;
        } else if(currentMode == Mode.Frightened && !isInGhostHouse)
        {
            targetTile = GetRandomTile();
        } else if(currentMode == Mode.Consumed && !isInGhostHouse)
        {
            targetTile = ghostHouse.transform.position;
        }
        Node moveToNode = null;

        Node[] foundNodes = new Node[4];
        Vector2[] foundNodesDirection = new Vector2[4];

        int nodeCounter = 0;

        for (int i = 0; i < currentNode.neighbors.Length; i++)
        {
            if (currentNode.validDirections[i] != direction * -1)
            {
                if(currentMode != Mode.Consumed)
                {
                    GameObject tile = GetTileAtPosition(currentNode.transform.position);
                    if (tile.transform.GetComponent <Tile>().isGhostHouseEntrance == true)
                    {
                        if (currentNode.validDirections[i] != Vector2.down)
                        {
                            foundNodes[nodeCounter] = currentNode.neighbors[i];
                            foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                            nodeCounter++;
                        }
                    }else
                    {
                        foundNodes[nodeCounter] = currentNode.neighbors[i];
                        foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                        nodeCounter++;
                    }
                }else
                {
                    foundNodes[nodeCounter] = currentNode.neighbors[i];
                    foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                    nodeCounter++;
                }
            }
            else if (isInGhostHouse == true)
            {
                foundNodes[nodeCounter] = currentNode.neighbors[i];
                foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                nodeCounter++;
            }
        }

        if (foundNodes.Length == 1)
        {
            moveToNode = foundNodes[0];
            direction = foundNodesDirection[0];
        }
        if (foundNodes.Length > 1)
        {
            float leastDistance = 10000f;

            for (int i = 0; i < foundNodes.Length; i++)
            {
                if (foundNodesDirection[i] != Vector2.zero)
                {
                    float distance = GetDistance(foundNodes[i].transform.position, targetTile);
                    if (distance < leastDistance)
                    {
                        leastDistance = distance;
                        moveToNode = foundNodes[i];
                        direction = foundNodesDirection[i];
                    }
                }
            }
        }
        return moveToNode;
    }
    Node GetNodeAtPosition(Vector2 pos)
    {
        GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)];
        if (tile != null)
        {
            if (tile.GetComponent<Node>() != null)
            {
                return tile.GetComponent<Node>();
            }
        }
        return null;
    }
    GameObject GetTileAtPosition(Vector2 pos)
    {
        int tileX = Mathf.RoundToInt(pos.x);
        int tileY = Mathf.RoundToInt(pos.y);

        GameObject tile = GameObject.Find("Game").transform.GetComponent<GameBoard>().board[tileX, tileY];

        if (tile != null)
        {
            return tile;
        }
        return null;
    }
    GameObject GetPortal(Vector2 pos)
    {
        GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[(int)pos.x, (int)pos.y];

        if (tile != null)
        {
            if (tile.GetComponent<Tile>().isPortal)
            {
                GameObject otherPortal = tile.GetComponent<Tile>().portalReceiver;
                return otherPortal;
            }
        }
        return null;
    }

    float LengthFromNode(Vector2 targetPosition)
    {
        Vector2 vec = targetPosition - (Vector2)previousNode.transform.position;
        return vec.sqrMagnitude;
    }

    bool overShotTarget()
    {
        float nodeToTarget = LengthFromNode(targetNode.transform.position);
        float nodeToSelf = LengthFromNode(transform.localPosition);
        return nodeToSelf > nodeToTarget;
    }

    float GetDistance(Vector2 posA, Vector2 posB)
    {
        float distance = Vector2.Distance(posA, posB);
        return distance;
    }
}
