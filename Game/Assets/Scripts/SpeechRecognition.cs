using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;


public class SpeechRecognition : MonoBehaviour
{

    public static SpeechRecognition instance;

    public string[] keywords;
    public ConfidenceLevel confidence = ConfidenceLevel.Low;

    public GameObject [] weapons;
    public Text results;
    protected PhraseRecognizer recognizer;
    public string word;
    public bool equipped = false;

    private GameObject spawnedObject;
    public Transform gunContainer;


    // Start is called before the first frame update
    void Start()
    {
        recognizer = new KeywordRecognizer(keywords, confidence);
        recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
        recognizer.Start();

    }

    private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        word = args.text;
        SpawnWeapon(word);
        equipped = true;
        spawnedObject.transform.SetParent(gunContainer);
        spawnedObject.transform.localPosition = new Vector3(0,0,0);
        spawnedObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        Debug.Log("Rekli ste: <b>" + word + "</b>");
    }

    private void OnApplicationQuit()
    {
        if(recognizer != null && recognizer.IsRunning)
        {
            recognizer.OnPhraseRecognized -= Recognizer_OnPhraseRecognized;
            recognizer.Stop();
        }
    }

    private void SpawnWeapon(string word)
    {
        int weaponNum = 0;
        if (equipped)
        {
            Destroy(spawnedObject);
        }
        Vector3 defaultPos = new Vector3(0, 0, 0);
        switch (word)
        {
            case "ak": weaponNum = 0;break;
            case "m4": weaponNum = 1;break;
            case "gun":weaponNum = 2;break;

        }
        spawnedObject = Instantiate(weapons[weaponNum],defaultPos, Quaternion.Euler(0,0,0)) as GameObject;

    }
    public void Delete()
    {
        equipped = false;
    }
}
