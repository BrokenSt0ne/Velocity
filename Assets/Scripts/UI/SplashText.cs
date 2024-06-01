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
            "this is the WON version",
            "cbug isn't real, it can't hurt you!",
            "meanwhile cbug",
            "John Velocity",
            ":3",
            "Free money update is coming soon",
            "Guaranteed!",
            "This year is the year of the Linux desktop, I promise",
            "Made by gay cats",
            "It's called Twitter, not X",
            "Object reference not set to an instance of an object",
            "If it works, don't fix it",
            "Game design is my passion",
            "Try Half-Life!",
            "Go fast!",
            "It's free!",
            "Trans rights",
            "Open-source!"
        };

        private void Awake() {
            gameObject.GetComponent<Text>().text = SPLASHES[Random.Range(0, SPLASHES.Length)];
        }
    }
}