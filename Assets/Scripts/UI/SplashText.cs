using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SplashText : MonoBehaviour
    {
        string[] SPLASHES = {
            "0.5 will have you be a real bunny",
            "weeeeeee",
            "Resonance cascade!",
            "Now with 100% less networking",
            "NOT a minecraft splash ripoff",
            "tempel is the best map",
            "Shoutouts to Simpleflips",
            "Velocity!",
            "Bee-hop"
        };

        private void Awake() {
            gameObject.GetComponent<Text>().text = SPLASHES[Random.Range(0, SPLASHES.Length)];
        }
    }
}