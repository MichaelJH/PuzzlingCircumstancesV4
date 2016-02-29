using UnityEngine;
using System.Collections;

public class boxScript : MonoBehaviour {
    private GameObject player;
    private Rigidbody2D rb2d;
    private Vector2 spawn;
    private bool fading;
    public float maxGravity = 80f;
    
    void Start() {
        player = GameObject.Find("Player");
        rb2d = GetComponent<Rigidbody2D>();
        spawn = transform.position;
        fading = false;
    }	

    void Update() {
        Vector2 veloc = rb2d.velocity;
        if (Mathf.Abs(veloc.x) > maxGravity) {
            veloc.x = maxGravity * Mathf.Sign(veloc.x);
        } else if (Mathf.Abs(veloc.y) > maxGravity) {
            veloc.y = maxGravity * Mathf.Sign(veloc.y);
        }
        rb2d.velocity = veloc;
    }

	void OnCollisionEnter2D(Collision2D coll) {
        if (coll.gameObject.tag == "Portal") {
            var portalScript = player.GetComponent<PortalScript>();
            float offset = 1.5f;
            bool teleport = false;
            PortalScript.WallOrientation orientation = PortalScript.WallOrientation.Left;
            Vector2 newPos = Vector2.zero;
            Vector2 newVeloc = Vector2.zero;

            if (coll.gameObject == portalScript.Portal1) {
                if (portalScript.Portal2.activeSelf) {
                    teleport = true;
                    newPos = portalScript.PPos.p2;
                    orientation = portalScript.PPos.p2Or;
                }
            }
            else {
                if (portalScript.Portal2.activeSelf) {
                    teleport = true;
                    newPos = portalScript.PPos.p1;
                    orientation = portalScript.PPos.p1Or;
                }
            }

            if (teleport) {
                float speed = rb2d.velocity.magnitude;
                if (speed < .3f)
                    speed = .3f;

                if (orientation == PortalScript.WallOrientation.Left) {
                    newPos.x += offset;
                    newVeloc = new Vector2(speed, 0);
                } else if (orientation == PortalScript.WallOrientation.Right) {
                    newPos.x -= offset;
                    newVeloc = new Vector2(-speed, 0);
                } else if (orientation == PortalScript.WallOrientation.Ceiling) {
                    newPos.y -= offset;
                    newVeloc = new Vector2(0, -speed);
                } else {
                    newPos.y += offset;
                    newVeloc = new Vector2(0, speed);
                }

                rb2d.velocity = newVeloc;
                transform.position = newPos;
            }
        } else if (coll.gameObject.tag == "Barrier") {
            if (!fading) {
                fading = true;
                StartCoroutine(FadeAndSpawn());
            }
        }
    }

    IEnumerator FadeAndSpawn() {
        var renderer = GetComponent<SpriteRenderer>();
        Color newColor = renderer.color;
        Color initColor = newColor;
        while (newColor.a > 0) {
            newColor.a -= 0.02f;
            renderer.color = newColor;
            yield return new WaitForSeconds(0.01f);
        }

        rb2d.velocity = Vector2.zero;
        transform.position = spawn;
        transform.rotation = Quaternion.identity;
        player.GetComponent<playerController>().carryingBox = false;
        renderer.color = initColor;
        rb2d.isKinematic = false;
        fading = false;
    }
}
