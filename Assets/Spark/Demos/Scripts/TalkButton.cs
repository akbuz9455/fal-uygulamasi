using System.Collections;
using System.Collections.Generic;
using LeastSquares;
using TMPro;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class TalkButton : MonoBehaviour
    {

        public TMP_InputField Input;

        public InteractiveAIDialogue Dialogue;

        public void Talk()
        {
            Dialogue.Talk(Input.text);
        }
    }
}