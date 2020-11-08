using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;

public class Translation : MonoBehaviour
{

    public TextMeshPro recognizedText;
    public TextMeshPro translatedText;
    public PressableButton micButton;

    public string SpeechServiceSubscriptionKey = "";
    public string SpeechServiceRegion = "";

    private bool waitingforReco;
    private string recognizedString;
    private string translatedString;

    private bool micPermissionGranted = true;

    private object threadLocker = new object();   

    public async void ButtonClick()
    {
        Debug.Log("Onclick fired");
        var translationConfig = SpeechTranslationConfig.FromSubscription(SpeechServiceSubscriptionKey, SpeechServiceRegion);
        translationConfig.SpeechRecognitionLanguage = "en-US";
        translationConfig.AddTargetLanguage("fr");

        using (var recognizer = new TranslationRecognizer(translationConfig))
        {
            Debug.Log("Creating recognizer");
            lock (threadLocker)
            {
                waitingforReco = true;
            }

            var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

            if (result.Reason == ResultReason.TranslatedSpeech)
            {
               
                recognizedString = result.Text;
                Debug.Log("Text: " + recognizedString);
                foreach (var element in result.Translations)
                {
                    translatedString = element.Value;
                }
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                recognizedString = "NOMATCH: Speech could not be recognized.";
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                recognizedString = $"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}";
            }

            lock (threadLocker)
            {
                waitingforReco = false;
            }

        }
    }


    // Start is called before the first frame update
    void Start()
    {
        if (translatedText == null)
        {
            UnityEngine.Debug.LogError("translatedText property is null! Assign a UI TextMeshPro Text element to it.");
        }
        else if (micButton == null)
        {
            UnityEngine.Debug.LogError("micButton property is null! Assign a MRTK Pressable Button to it.");
        }
        else
        {
            micPermissionGranted = true;
            micButton.ButtonPressed.AddListener(ButtonClick);
        }
    }

    // Update is called once per frame
    void Update()
    {
        lock (threadLocker)
        {
            recognizedText.text = recognizedString;
            translatedText.text = translatedString;
        }
    }
}