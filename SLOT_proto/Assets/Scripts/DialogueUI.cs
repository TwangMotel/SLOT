/*

The MIT License (MIT)

Copyright (c) 2015-2017 Secret Lab Pty. Ltd. and Yarn Spinner contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using Yarn.Unity;


/// Displays dialogue lines to the player, and sends
/// user choices back to the dialogue system.

/** Note that this is just one way of presenting the
 * dialogue to the user. The only hard requirement
 * is that you provide the RunLine, RunOptions, RunCommand
 * and DialogueComplete coroutines; what they do is up to you.
 */
public class DialogueUI : Yarn.Unity.DialogueUIBehaviour
{

    /// The object that contains the dialogue and the options.
    /** This object will be enabled when conversation starts, and 
     * disabled when it ends.
     */
    public GameObject dialogueContainer;

    /// The UI element that displays lines
    public Text lineText;

    /// A UI element that appears after lines have finished appearing
    public GameObject continuePrompt;

    /// A delegate (ie a function-stored-in-a-variable) that
    /// we call to tell the dialogue system about what option
    /// the user selected
    private Yarn.OptionChooser SetSelectedOption;

    /// How quickly to show the text, in seconds per character
    [Tooltip("How quickly to show the text, in seconds per character")]
    public float textSpeed = 0.025f;

    /// The buttons that let the user choose an option
    public List<Button> optionButtons;

    /// Make it possible to temporarily disable the controls when
    /// dialogue is active and to restore them when dialogue ends
    public RectTransform gameControlsContainer;

    //Store all portraits used in this scene
    public List<Sprite> portraits;

    //permanent, unaltered list of portraits. Used when the portraits list being altered needs repopulated
    private List<Sprite> fullPortraits = new List<Sprite>();

    //randomly generated number that will be used to access portraits in the portraits list
    private int index;

    private GameObject NPC_Portrait;

    //Store click to skip buttons here
    public Button clickToSkip;

    //bool that tracks of clickToSkip button is pushed
    private bool skipContinue;

    //Name of character currently speaking
    private string speakerName;

    //Declare Dialogue Runner class
    DialogueRunner dialogueRunner;




    void Awake()
    {
        //Assign Dialogue Runner class
        dialogueRunner = GetComponent<DialogueRunner>();

        // Start by hiding the container, line and option buttons
        if (dialogueContainer != null)
            dialogueContainer.SetActive(false);

        lineText.gameObject.SetActive(false);

        foreach (var button in optionButtons)
        {
            button.gameObject.SetActive(false);
        }

        //Hide the NPC portrait
        NPC_Portrait = GameObject.Find("NPC_portrait");
        NPC_Portrait.SetActive(false);


        //Hide click to skip button
        if (clickToSkip != null)
            clickToSkip.gameObject.SetActive(false);


        // Hide the continue prompt if it exists
        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        //Populate fullPortraits list with the Portraits list
        fullPortraits.AddRange(portraits);


    }


    /// Show a line of dialogue, gradually
    public override IEnumerator RunLine(Yarn.Line line)
    {
        // Holds GameObject of currently speaking character
        GameObject speaker;

        //Set clickToSkip button to active
        clickToSkip.gameObject.SetActive(true);

        var speakerStringBuilder = new StringBuilder();

        //Check to see who is speaking
        foreach (char c in line.text)
        {
            if (c != ':')
            {
                speakerStringBuilder.Append(c);
                speakerName = speakerStringBuilder.ToString();
            }
            else
            {
                break;
            }
        }

        //Find the name found from the Yarn Line
        if(speakerName == "CW")
        {
            speaker = GameObject.Find("CW");
        }
        else if(speakerName == "Narrator")
        {
            speaker = GameObject.Find("Narrator");
            lineText.fontStyle = FontStyle.Italic;
        }
        else
        {
            speaker = GameObject.Find("NPC");
        }


        //If found, put dialogue container at the speakers position
        if (speaker != null)
        {
            dialogueContainer.transform.position = speaker.transform.position;
        }
        else
        {
            Debug.Log("speaker does not have a dialogue container!");
        }


        //Show the NPC portrait 
        NPC_Portrait.SetActive(true);


        //Removes Character name, colon, and space from the displayed dialogue
        line.text = line.text.Remove(0, speakerName.Length + 2);


        // Show the text
        if (textSpeed > 0.0f)
        {
            lineText.gameObject.SetActive(true);

            // Display the line one character at a time
            var stringBuilder = new StringBuilder();
            foreach (char c in line.text)
            {
                stringBuilder.Append(c);
                lineText.text = stringBuilder.ToString();
                yield return new WaitForSeconds(textSpeed);

                //if skipContinue button is pushed, display line immediately
                if (skipContinue == true)
                {
                    skipContinue = false;
                    lineText.text = line.text;
                    break;
                }

            }
        }
        else
        {
            // Display the line immediately if textSpeed == 0
            lineText.text = line.text;
        }

        //Turn off clickToSkip button
        clickToSkip.gameObject.SetActive(false);

        // Show the 'press any key' prompt when done, if we have one
        if (continuePrompt != null)
            continuePrompt.SetActive(true);


        while (Input.anyKeyDown == false)
        {
            yield return null;
        }

        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        // Clear the speaker name
        speakerName = string.Empty;
        //"Unpush" skipToClick button 
        skipContinue = false;
        //If text is italic, back to normal
        if (lineText.fontStyle == FontStyle.Italic)
            lineText.fontStyle = FontStyle.Normal;       

    }


    /// Show a list of options, and wait for the player to make a selection.
    public override IEnumerator RunOptions(Yarn.Options optionsCollection,
                                            Yarn.OptionChooser optionChooser)
    {
        // Do a little bit of safety checking
        if (optionsCollection.options.Count > optionButtons.Count)
        {
            Debug.LogWarning("There are more options to present than there are" +
                             "buttons to present them in. This will cause problems.");
        }

        // Display each option in a button, and make it visible
        int i = 0;
        foreach (var optionString in optionsCollection.options)
        {
            optionButtons[i].gameObject.SetActive(true);
            optionButtons[i].GetComponentInChildren<Text>().text = optionString;
            i++;
        }

        // Record that we're using it
        SetSelectedOption = optionChooser;

        // Wait until the chooser has been used and then removed (see SetOption below)
        while (SetSelectedOption != null)
        {
            yield return null;
        }

        // Hide all the buttons
        foreach (var button in optionButtons)
        {
            button.gameObject.SetActive(false);
        }
    }

    /// Called by buttons to make a selection.
    public void SetOption(int selectedOption)
    {

        // Call the delegate to tell the dialogue system that we've
        // selected an option.
        SetSelectedOption(selectedOption);

        // Now remove the delegate so that the loop in RunOptions will exit
        SetSelectedOption = null;
    }

    /// Run an internal command.
    public override IEnumerator RunCommand(Yarn.Command command)
    {
        // "Perform" the command
        Debug.Log("Command: " + command.text);

        yield break;
    }

    /// Called when the dialogue system has started running.
    public override IEnumerator DialogueStarted()
    {
        Debug.Log("Dialogue starting!");

        // Enable the dialogue controls.
        if (dialogueContainer != null)
            dialogueContainer.SetActive(true);

        // Hide the game controls.
        if (gameControlsContainer != null)
        {
            gameControlsContainer.gameObject.SetActive(false);
        }
        //Get a random number to select a random portrait
        index = Random.Range(0, portraits.Count);

        //Set the NPC Portrait sprite to a random sprite in our list
        Image NPC_image = NPC_Portrait.GetComponent<Image>();
        NPC_image.sprite = portraits[index];

        yield break;
    }

    /// Called when the dialogue system has finished running.
    public override IEnumerator DialogueComplete()
    {
        Debug.Log("Complete!");

        // Hide the dialogue interface.
        if (dialogueContainer != null)
            dialogueContainer.SetActive(false);

        // Show the game controls.
        if (gameControlsContainer != null)
        {
            gameControlsContainer.gameObject.SetActive(true);
        }

        //Hide the NPC portrait
        NPC_Portrait.SetActive(false);

        //Remove selected sprite 
        Image NPC_image = NPC_Portrait.GetComponent<Image>();
        NPC_image = null;

        //Remove portrait from usedPortrait lsit
        portraits.RemoveAt(index);

        //If all portraits have been used, repopulate the list
        if (portraits.Count == 0)
            portraits.AddRange(fullPortraits);
            

        yield break;

    }

    public void SkipContinueClick()
    {
        skipContinue = true;
    }



}

