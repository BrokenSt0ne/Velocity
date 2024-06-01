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
            "Bee-hop",
            "no human allowed",
            "this is not the WON version!!!",
            "cbug isn't real, it can't hurt you!",
            "meanwhile cbug"
        };

        private void Awake() {
            gameObject.GetComponent<Text>().text = SPLASHES[Random.Range(0, SPLASHES.Length)];
        }
    }
}