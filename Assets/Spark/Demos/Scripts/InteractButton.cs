using UnityEngine;

namespace LeastSquares.Spark
{
    public class InteractButton : MonoBehaviour
    {
        public BarkAIDialogue Dialogue;

        public void Talk()
        {
            Dialogue.Talk();
        }
    }
}