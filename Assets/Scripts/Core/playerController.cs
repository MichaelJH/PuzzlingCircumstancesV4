using UnityEngine;
using System.Collections;

public class playerController : MonoBehaviour {
    private GameObject playerGO;
    private Vector2 playerVelocity;
    private Vector2 gravityAccel;

    private bool _jump;
    private int _framesJumped;

    public float jumpForce = 0.09f;
    public float playerSpeed = 0.02f;
    public float airSpeed = 0.001f;
    public float gravityLimit = 2f;
    private Vector2 jumpDirection;

    public GameObject portalLinePrefab;
    private GameObject portalLine;
    private GameObject exitDoor;
    private Transform doorTrans;

    private GameObject carriedBox;
    public bool carryingBox = false;

    public bool move;
    public bool hasPortals = true;

	void Awake () {
        playerGO = gameObject;
        // initialize collision script
        GetComponent<playerCollisions>().Init(playerGO);
        // initialize gravity (will move this to a level controller)
        playerVelocity = Vector2.zero;

        _jump = false;
        _framesJumped = 0;

        exitDoor = GameObject.Find("Door");
        doorTrans = exitDoor.transform;
        
        GameObject entrance = GameObject.Find("Entrance");
        if (entrance != null) {
            gameObject.layer = LayerMask.NameToLayer("Default");
            Vector3 enterPos = new Vector3(entrance.transform.position.x, entrance.transform.position.y, 0);
            enterPos -= 1.25f * entrance.transform.up;
            transform.position = enterPos;
        }
	}

    void Update() {
        var collisionScript = GetComponent<playerCollisions>();
        var gravityScript = GetComponent<gravityHandler>();
        Vector2 gravity = gravityScript.GetGravityVector();

        if (move) {
            detectDoor();
            if (hasPortals)
                DrawAimLine();
        }
        
        // check for picking up a box
        if (Input.GetKeyDown(KeyCode.E)) {
            if (carryingBox) {
                carryingBox = false;
                Rigidbody2D boxBody = carriedBox.GetComponent<Rigidbody2D>();
                boxBody.isKinematic = false;

                Vector2 dropVelocity = (playerVelocity * 50) + (gravity * -800);
                boxBody.velocity = dropVelocity;
            } else if (!carryingBox && collisionScript.touchingBox) {// && !collisionScript.inDoor) { need to make sure the player isn't about to enter the door. inDoor isn't enough, it might be shut
                carriedBox = collisionScript.touchedBox;
                carryingBox = true;
                carriedBox.GetComponent<Rigidbody2D>().isKinematic = true;
                carriedBox.transform.rotation = Quaternion.identity;
            }
        }

        if (carryingBox) {
            Vector2 boxPos = transform.position;
            boxPos += gravity * -75;
            carriedBox.transform.position = boxPos;
        }

        // change gravity (will move to an input controller)
        if (Input.GetKeyDown(KeyCode.I)) {
            gravityScript.SetGravityOrientation(gravityHandler.GravityOrientation.Up);
        } else if (Input.GetKeyDown(KeyCode.K)) {
            gravityScript.SetGravityOrientation(gravityHandler.GravityOrientation.Down);
        } else if (Input.GetKeyDown(KeyCode.L)) {
            gravityScript.SetGravityOrientation(gravityHandler.GravityOrientation.Right);
        } else if (Input.GetKeyDown(KeyCode.J)) {
            gravityScript.SetGravityOrientation(gravityHandler.GravityOrientation.Left);
        }

        if (collisionScript.onGround && Input.GetButtonDown("Jump") && move) {
            _jump = true;
        }
    }

	// Update is called once per frame
	void FixedUpdate () {
        var collisionScript = GetComponent<playerCollisions>();
        var gravityScript = GetComponent<gravityHandler>();
        Vector2 gravity = gravityScript.GetGravityVector();

        if (!move) {
            if (hasPortals && portalLine != null)
                portalLine.SetActive(false);
        }
        // check if a portal has been touched
        else if (collisionScript.portalTouched != 0) {
            PortalCollision(collisionScript.portalTouched);
            collisionScript.portalTouched = 0;
        } // otherwise, react to physics
        else {
            // check for movement input
            CheckMovementInput(gravity);

            // add gravity to the current velocity
            playerVelocity += gravity;

            // ensure player doesn't exceed maximum allowable x or y speed
            if (Mathf.Abs(playerVelocity.y) > gravityLimit)
                playerVelocity.y = gravityLimit * Mathf.Sign(playerVelocity.y);
            if (Mathf.Abs(playerVelocity.x) > gravityLimit)
                playerVelocity.x = gravityLimit * Mathf.Sign(playerVelocity.x);

            // check for collisions
            playerVelocity = collisionScript.Move(playerVelocity, playerGO);

            // make the player jump
            if (_jump) {
                CalculateJumpForce(gravity);
            }

            // calculate the new player position and move the transform
            Vector3 newPosition = transform.position;
            newPosition.x += playerVelocity.x;
            newPosition.y += playerVelocity.y;
            transform.position = newPosition;
        }
	}

    private void CheckMovementInput(Vector2 gravity) {
        var collisionScript = GetComponent<playerCollisions>();
        bool vertical = gravity.y != 0;

        if (vertical) {
            if (collisionScript.onGround) {
                if (Input.GetKey(KeyCode.A))
                    playerVelocity.x = -playerSpeed;
                else if (Input.GetKey(KeyCode.D))
                    playerVelocity.x = playerSpeed;
                else
                    playerVelocity.x = 0;
            } else {
                if (Input.GetKey(KeyCode.A) && playerVelocity.x > -playerSpeed)
                    playerVelocity.x -= airSpeed;
                else if (Input.GetKey(KeyCode.D) && playerVelocity.x < playerSpeed)
                    playerVelocity.x += airSpeed;
                else if (!collisionScript.touchedPortal && Mathf.Abs(playerVelocity.x) > airSpeed)
                    playerVelocity.x += Mathf.Sign(playerVelocity.x) * -airSpeed;
            }
        } else {
            if (collisionScript.onGround) {
                if (Input.GetKey(KeyCode.S))
                    playerVelocity.y = -playerSpeed;
                else if (Input.GetKey(KeyCode.W))
                    playerVelocity.y = playerSpeed;
                else
                    playerVelocity.y = 0;
            } else {
                if (Input.GetKey(KeyCode.S) && playerVelocity.y > -playerSpeed)
                    playerVelocity.y -= airSpeed;
                else if (Input.GetKey(KeyCode.W) && playerVelocity.y < playerSpeed)
                    playerVelocity.y += airSpeed;   
                else if (!collisionScript.touchedPortal && Mathf.Abs(playerVelocity.y) > airSpeed)
                    playerVelocity.y += Mathf.Sign(playerVelocity.y) * -airSpeed;
            }
        }
    }

    private void CalculateJumpForce(Vector2 gravity) {
        var collisionScript = GetComponent<playerCollisions>();
        if (collisionScript.onGround && _framesJumped > 0) {
            _jump = false;
            _framesJumped = 0;
            return;
        }
        if (_framesJumped == 0) {
            jumpDirection = gravity;
            float gravityMag = jumpDirection.magnitude;
            jumpDirection /= -gravityMag;
        }
        float jumpFactor = 1f;

        if (Input.GetButton("Jump")) {
            _framesJumped++;
        } else {
            _jump = false;
            if (_framesJumped < 5)
                _framesJumped = 5;
            jumpFactor = 0.5f + (0.05f * _framesJumped);
        }

        if (_framesJumped > 10) {
            _jump = false;
        }

        if (!_jump) {
            _framesJumped = 0;
        }

        if (gravity.y != 0)
            playerVelocity.y = jumpForce * jumpDirection.y * jumpFactor;
        else
            playerVelocity.x = jumpForce * jumpDirection.x * jumpFactor;
    }

    private void DrawAimLine() {
        if (portalLine == null) {
            portalLine = GameObject.Instantiate(portalLinePrefab);
        }
        int platform = LayerMask.GetMask("PortalPlatform", "Platform", "Barrier");

        Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos = transform.position;
        Vector2 direction = target - pos;

        RaycastHit2D hit = Physics2D.Raycast(pos, direction, Mathf.Infinity, platform);
        Vector3 hit3D = new Vector3(hit.point.x, hit.point.y, 0);
        Vector3[] points = { transform.position, hit3D };

        var renderer = portalLine.GetComponent<LineRenderer>();

        renderer.SetPositions(points);
    }

    private void detectDoor() {
        Vector2 direction = new Vector2(doorTrans.position.x - transform.position.x, doorTrans.position.y - transform.position.y);
        float proximity = 10f;
        int _collisionMask = LayerMask.GetMask("Door", "Platform", "PortalPlatform");
        var doorScript = exitDoor.GetComponent<doorScript>();

        RaycastHit2D doorHit = Physics2D.Raycast(transform.position, direction, proximity, _collisionMask);

        if (doorHit && doorHit.collider.gameObject.tag == "Door") {
            if (!doorScript.open)
                doorScript.RaiseGate();
        } else {
            if (doorScript.open)
                doorScript.LowerGate();
        }

    }

    void PortalCollision(int whichPortal) {
        var portalScript = GetComponent<PortalScript>();
        float offset = 1.5f;

        if (whichPortal == 1) {
            if (portalScript.Portal2.activeSelf) {
                Vector2 newPos = portalScript.PPos.p2;
                PortalScript.WallOrientation orientation = portalScript.PPos.p2Or;
                if (orientation == PortalScript.WallOrientation.Left)
                    newPos.x += offset;
                else if (orientation == PortalScript.WallOrientation.Right)
                    newPos.x -= offset;
                else if (orientation == PortalScript.WallOrientation.Ceiling)
                    newPos.y -= offset;
                else
                    newPos.y += offset;
                transform.position = newPos;
                playerVelocity = NewVelocity(2);
            }
        }
        else {
            if (portalScript.Portal1.activeSelf) {
                Vector2 newPos = portalScript.PPos.p1;
                PortalScript.WallOrientation orientation = portalScript.PPos.p1Or;
                if (orientation == PortalScript.WallOrientation.Left)
                    newPos.x += offset;
                else if (orientation == PortalScript.WallOrientation.Right)
                    newPos.x -= offset;
                else if (orientation == PortalScript.WallOrientation.Ceiling)
                    newPos.y -= offset;
                else
                    newPos.y += offset;
                transform.position = newPos;
                playerVelocity = NewVelocity(1);
            }
        }

        // this is important, not useless! It makes the raycasts activate again after portaling
        _jump = false;
        GetComponent<playerCollisions>().onGround = false;
    }

    Vector2 NewVelocity(int exitPortal) {
        Vector2 result;
        var PortalScript = GetComponent<PortalScript>();
        PortalScript.WallOrientation orient;
        if (exitPortal == 1)
            orient = PortalScript.PPos.p1Or;
        else
            orient = PortalScript.PPos.p2Or;
        float velocity = playerVelocity.magnitude - 0.03f;
        if (velocity < .3f)
            velocity = .3f;
        if (orient == PortalScript.WallOrientation.Left) {
            result = new Vector2(velocity, 0);
        }
        else if (orient == PortalScript.WallOrientation.Right) {
            result = new Vector2(-velocity, 0);
        }
        else if (orient == PortalScript.WallOrientation.Ceiling) {
            result = new Vector2(0, -velocity);
        }
        else {
            result = new Vector2(0, velocity);
        }

        return result;
    }

    void OnCollisionEnter2D(Collision2D coll) {
        if (coll.gameObject.tag == "Barrier") {
            var portalScript = GetComponent<PortalScript>();
            portalScript.Portal1.SetActive(false);
            portalScript.Portal2.SetActive(false);
        }
        else if (coll.gameObject.tag == "Deadly") {
            var manager = GameObject.Find("roomManager").GetComponent<roomManagement>();
            manager.RestartLevel();
        }
    }
}
